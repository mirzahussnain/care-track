import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Subscription, catchError, forkJoin, map, of, switchMap } from 'rxjs';

import {
  Button,
  EmptyState,
  FormField,
  Skeleton,
  StatusChip,
  Surface,
} from '../../../../design-system/components';
import { DataToolbar, PageHeader, Pagination } from '../../../../design-system/patterns';
import { PagedResult } from '../../../../shared/models/paged-result.model';
import { PatientApiService } from '../../../patients/data-access/patient-api.service';
import { ReferralPatientSummary } from '../../../patients/models/patient.models';
import { ReferralApiService } from '../../../referrals/data-access/referral-api.service';
import { Referral } from '../../../referrals/models/referral.models';
import { AppointmentApiService } from '../../data-access/appointment-api.service';
import { appointmentUtcRangeValidator } from '../../forms/appointment-form.validators';
import { appointmentInputToUtcIso, formatAppointmentUtc } from '../../models/appointment-datetime';
import {
  APPOINTMENT_STATUSES,
  APPOINTMENT_TYPES,
  AppointmentSearchItem,
  AppointmentSortField,
  AppointmentStatus,
  AppointmentType,
  SortDirection,
  appointmentStatusLabel,
  appointmentStatusTone,
  appointmentTypeLabel,
} from '../../models/appointment.models';

type ListError = 'forbidden' | 'validation' | 'generic' | null;

interface AppliedFilters {
  readonly status: AppointmentStatus | null;
  readonly appointmentType: AppointmentType | null;
  readonly location: string;
  readonly scheduledFrom: string;
  readonly scheduledTo: string;
  readonly sortBy: AppointmentSortField;
  readonly sortDirection: SortDirection;
}

type PatientLookup = Readonly<Record<string, ReferralPatientSummary | null>>;
type ReferralLookup = Readonly<Record<string, Referral | null>>;

@Component({
  selector: 'app-appointments-page',
  standalone: true,
  imports: [
    Button,
    DataToolbar,
    EmptyState,
    FormField,
    PageHeader,
    Pagination,
    ReactiveFormsModule,
    RouterLink,
    Skeleton,
    StatusChip,
    Surface,
  ],
  templateUrl: './appointments-page.html',
  styleUrl: './appointments-page.css',
})
export class AppointmentsPage {
  private readonly appointmentApi = inject(AppointmentApiService);
  private readonly patientApi = inject(PatientApiService);
  private readonly referralApi = inject(ReferralApiService);
  private readonly destroyRef = inject(DestroyRef);
  private requestSubscription?: Subscription;

  readonly statuses = Object.values(APPOINTMENT_STATUSES) as AppointmentStatus[];
  readonly appointmentTypes = Object.values(APPOINTMENT_TYPES) as AppointmentType[];
  readonly sortOptions = [
    { value: 'scheduledStart', label: 'Start time' },
    { value: 'scheduledEnd', label: 'End time' },
    { value: 'createdAt', label: 'Created date' },
    { value: 'appointmentReference', label: 'Appointment reference' },
    { value: 'status', label: 'Status' },
  ] as const satisfies readonly { value: AppointmentSortField; label: string }[];

  readonly filters = new FormGroup(
    {
      status: new FormControl<AppointmentStatus | null>(null),
      appointmentType: new FormControl<AppointmentType | null>(null),
      location: new FormControl('', { nonNullable: true }),
      scheduledFrom: new FormControl('', { nonNullable: true }),
      scheduledTo: new FormControl('', { nonNullable: true }),
      sortBy: new FormControl<AppointmentSortField>('scheduledStart', { nonNullable: true }),
      sortDirection: new FormControl<SortDirection>('asc', { nonNullable: true }),
    },
    { validators: appointmentUtcRangeValidator('scheduledFrom', 'scheduledTo') },
  );

  readonly appliedFilters = signal<AppliedFilters>(this.readFilters());
  readonly page = signal(1);
  readonly pageSize = 20;
  readonly result = signal<PagedResult<AppointmentSearchItem> | null>(null);
  readonly patientLookup = signal<PatientLookup>({});
  readonly referralLookup = signal<ReferralLookup>({});
  readonly loading = signal(true);
  readonly error = signal<ListError>(null);
  readonly filtersExpanded = signal(false);

  constructor() {
    this.destroyRef.onDestroy(() => this.requestSubscription?.unsubscribe());
    this.loadAppointments();
  }

  applyFilters(): void {
    if (this.filters.invalid) {
      this.filters.markAllAsTouched();
      this.error.set('validation');
      return;
    }
    this.appliedFilters.set(this.readFilters());
    this.page.set(1);
    this.loadAppointments();
  }

  toggleFilters(): void {
    this.filtersExpanded.update((expanded) => !expanded);
  }

  resetFilters(): void {
    this.filters.reset({
      status: null,
      appointmentType: null,
      location: '',
      scheduledFrom: '',
      scheduledTo: '',
      sortBy: 'scheduledStart',
      sortDirection: 'asc',
    });
    this.appliedFilters.set(this.readFilters());
    this.page.set(1);
    this.loadAppointments();
  }

  changePage(page: number): void {
    this.page.set(page);
    this.loadAppointments();
  }

  retry(): void {
    this.loadAppointments();
  }

  isFiltered(): boolean {
    const filters = this.appliedFilters();
    return (
      filters.status !== null ||
      filters.appointmentType !== null ||
      !!filters.location ||
      !!filters.scheduledFrom ||
      !!filters.scheduledTo
    );
  }

  patientFor(patientId: string): ReferralPatientSummary | null {
    return this.patientLookup()[patientId] ?? null;
  }

  referralFor(referralId: string): Referral | null {
    return this.referralLookup()[referralId] ?? null;
  }

  statusLabel(status: AppointmentStatus): string {
    return appointmentStatusLabel(status);
  }

  statusTone(status: AppointmentStatus) {
    return appointmentStatusTone(status);
  }

  typeLabel(type: AppointmentType): string {
    return appointmentTypeLabel(type);
  }

  utc(value: string): string {
    return formatAppointmentUtc(value);
  }

  private readFilters(): AppliedFilters {
    const value = this.filters.getRawValue();
    return {
      status: value.status,
      appointmentType: value.appointmentType,
      location: value.location.trim(),
      scheduledFrom: value.scheduledFrom,
      scheduledTo: value.scheduledTo,
      sortBy: value.sortBy,
      sortDirection: value.sortDirection,
    };
  }

  private loadAppointments(): void {
    this.requestSubscription?.unsubscribe();
    this.loading.set(true);
    this.error.set(null);
    const filters = this.appliedFilters();

    this.requestSubscription = this.appointmentApi
      .searchAppointments({
        status: filters.status ?? undefined,
        appointmentType: filters.appointmentType ?? undefined,
        location: filters.location || undefined,
        scheduledFrom: filters.scheduledFrom
          ? appointmentInputToUtcIso(filters.scheduledFrom)
          : undefined,
        scheduledTo: filters.scheduledTo
          ? appointmentInputToUtcIso(filters.scheduledTo)
          : undefined,
        page: this.page(),
        pageSize: this.pageSize,
        sortBy: filters.sortBy,
        sortDirection: filters.sortDirection,
      })
      .pipe(
        switchMap((result) => {
          const patientIds = [...new Set(result.items.map((item) => item.patientId))];
          const referralIds = [...new Set(result.items.map((item) => item.referralId))];

          const patients$ = patientIds.length
            ? forkJoin(
                patientIds.map((id) =>
                  this.patientApi.getReferralPatientSummary(id).pipe(
                    map((patient) => [id, patient] as const),
                    catchError(() => of([id, null] as const)),
                  ),
                ),
              )
            : of([] as readonly (readonly [string, ReferralPatientSummary | null])[]);

          const referrals$ = referralIds.length
            ? forkJoin(
                referralIds.map((id) =>
                  this.referralApi.getReferral(id).pipe(
                    map((referral) => [id, referral] as const),
                    catchError(() => of([id, null] as const)),
                  ),
                ),
              )
            : of([] as readonly (readonly [string, Referral | null])[]);

          return forkJoin({ result: of(result), patients: patients$, referrals: referrals$ });
        }),
      )
      .subscribe({
        next: ({ result, patients, referrals }) => {
          this.result.set(result);
          this.patientLookup.set(Object.fromEntries(patients));
          this.referralLookup.set(Object.fromEntries(referrals));
          this.loading.set(false);
        },
        error: (error: HttpErrorResponse) => {
          this.result.set(null);
          this.patientLookup.set({});
          this.referralLookup.set({});
          this.error.set(
            error.status === 403 ? 'forbidden' : error.status === 400 ? 'validation' : 'generic',
          );
          this.loading.set(false);
        },
      });
  }
}
