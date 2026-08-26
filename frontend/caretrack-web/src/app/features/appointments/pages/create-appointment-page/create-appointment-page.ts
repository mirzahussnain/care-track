import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin, of, switchMap } from 'rxjs';

import { CARETRACK_ROLES } from '../../../../core/auth/auth.models';
import { AuthService } from '../../../../core/auth/auth.service';
import {
  Button,
  EmptyState,
  FormField,
  Skeleton,
  Surface,
} from '../../../../design-system/components';
import { PageHeader, PatientIdentityBanner } from '../../../../design-system/patterns';
import { PatientApiService } from '../../../patients/data-access/patient-api.service';
import { ReferralPatientSummary } from '../../../patients/models/patient.models';
import { ReferralApiService } from '../../../referrals/data-access/referral-api.service';
import { REFERRAL_STATUSES, Referral } from '../../../referrals/models/referral.models';
import { focusFirstInvalidControl } from '../../../../shared/utils/focus-management';
import { AppointmentApiService } from '../../data-access/appointment-api.service';
import { appointmentUtcRangeValidator } from '../../forms/appointment-form.validators';
import { appointmentErrorMessage } from '../../models/appointment-errors';
import { appointmentInputToUtcIso, formatAppointmentUtc } from '../../models/appointment-datetime';
import {
  APPOINTMENT_TYPES,
  Appointment,
  AppointmentType,
  appointmentTypeLabel,
} from '../../models/appointment.models';

type ContextError =
  'missing-referral' | 'forbidden' | 'not-found' | 'unschedulable' | 'generic' | null;

@Component({
  selector: 'app-create-appointment-page',
  standalone: true,
  imports: [
    Button,
    EmptyState,
    FormField,
    PageHeader,
    PatientIdentityBanner,
    ReactiveFormsModule,
    RouterLink,
    Skeleton,
    Surface,
  ],
  templateUrl: './create-appointment-page.html',
  styleUrl: './create-appointment-page.css',
})
export class CreateAppointmentPage {
  private readonly appointmentApi = inject(AppointmentApiService);
  private readonly referralApi = inject(ReferralApiService);
  private readonly patientApi = inject(PatientApiService);
  private readonly authService = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly referralId = this.route.snapshot.queryParamMap.get('referralId') ?? '';
  readonly appointmentTypes = Object.values(APPOINTMENT_TYPES) as AppointmentType[];
  readonly canSchedule = computed(
    () =>
      this.authService.hasRole(CARETRACK_ROLES.clinician) ||
      this.authService.hasRole(CARETRACK_ROLES.referralCoordinator),
  );
  readonly canReadAppointment = computed(() => this.authService.hasRole(CARETRACK_ROLES.clinician));
  readonly referral = signal<Referral | null>(null);
  readonly patient = signal<ReferralPatientSummary | null>(null);
  readonly createdAppointment = signal<Appointment | null>(null);
  readonly loading = signal(true);
  readonly contextError = signal<ContextError>(null);
  readonly submitting = signal(false);
  readonly submitted = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly form = new FormGroup(
    {
      appointmentReference: new FormControl('', {
        nonNullable: true,
        validators: [Validators.required, Validators.maxLength(30)],
      }),
      appointmentType: new FormControl<AppointmentType>(APPOINTMENT_TYPES.consultation, {
        nonNullable: true,
        validators: [Validators.required],
      }),
      scheduledStart: new FormControl('', {
        nonNullable: true,
        validators: [Validators.required],
      }),
      scheduledEnd: new FormControl('', {
        nonNullable: true,
        validators: [Validators.required],
      }),
      location: new FormControl('', {
        nonNullable: true,
        validators: [Validators.required, Validators.maxLength(200)],
      }),
    },
    { validators: appointmentUtcRangeValidator('scheduledStart', 'scheduledEnd') },
  );

  constructor() {
    this.loadContext();
  }

  loadContext(): void {
    if (!this.referralId) {
      this.loading.set(false);
      this.contextError.set('missing-referral');
      return;
    }

    this.loading.set(true);
    this.contextError.set(null);
    this.referralApi
      .getReferral(this.referralId)
      .pipe(
        switchMap((referral) =>
          forkJoin({
            referral: of(referral),
            patient: this.patientApi.getReferralPatientSummary(referral.patientId),
          }),
        ),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: ({ referral, patient }) => {
          this.referral.set(referral);
          this.patient.set(patient);
          this.contextError.set(this.isSchedulable(referral) ? null : 'unschedulable');
          this.loading.set(false);
        },
        error: (error: HttpErrorResponse) => {
          this.loading.set(false);
          this.contextError.set(
            error.status === 403 ? 'forbidden' : error.status === 404 ? 'not-found' : 'generic',
          );
        },
      });
  }

  submit(): void {
    this.submitted.set(true);
    this.errorMessage.set(null);
    const referral = this.referral();
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      focusFirstInvalidControl([
        'appointment-reference',
        'appointment-type',
        'appointment-start',
        'appointment-end',
        'appointment-location',
      ]);
      return;
    }
    if (!referral || !this.patient() || this.contextError() || this.submitting()) return;

    const value = this.form.getRawValue();
    this.submitting.set(true);
    this.appointmentApi
      .createAppointment({
        appointmentReference: value.appointmentReference.trim(),
        patientId: referral.patientId,
        referralId: referral.id,
        appointmentType: value.appointmentType,
        scheduledStart: appointmentInputToUtcIso(value.scheduledStart),
        scheduledEnd: appointmentInputToUtcIso(value.scheduledEnd),
        location: value.location.trim(),
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (appointment) => {
          this.submitting.set(false);
          this.createdAppointment.set(appointment);
          if (this.canReadAppointment()) {
            void this.router.navigate(['/appointments', appointment.id]);
          }
        },
        error: (error: HttpErrorResponse) => {
          this.submitting.set(false);
          const mapped = appointmentErrorMessage(
            error,
            'The appointment could not be scheduled. Try again.',
          );
          this.errorMessage.set(mapped.message);
          if (mapped.kind === 'referral-state') {
            this.contextError.set('unschedulable');
          }
        },
      });
  }

  typeLabel(type: AppointmentType): string {
    return appointmentTypeLabel(type);
  }

  utc(value: string): string {
    return formatAppointmentUtc(value);
  }

  textError(control: FormControl<string>, label: string, maximum?: number): string | undefined {
    if (!(control.touched || this.submitted())) return undefined;
    if (control.hasError('required') || !control.value.trim()) return `${label} is required.`;
    if (maximum && control.hasError('maxlength')) {
      return `${label} cannot exceed ${maximum} characters.`;
    }
    return undefined;
  }

  dateError(control: FormControl<string>, label: string): string | undefined {
    if (!(control.touched || this.submitted())) return undefined;
    return control.hasError('required') ? `${label} is required.` : undefined;
  }

  appointmentEndError(): string | undefined {
    if (
      this.form.hasError('appointmentRange') &&
      (this.form.controls.scheduledEnd.touched || this.submitted())
    ) {
      return 'Scheduled end must be after scheduled start.';
    }
    return this.dateError(this.form.controls.scheduledEnd, 'Scheduled end');
  }

  private isSchedulable(referral: Referral): boolean {
    return (
      referral.status === REFERRAL_STATUSES.assigned ||
      referral.status === REFERRAL_STATUSES.scheduled ||
      referral.status === REFERRAL_STATUSES.inProgress
    );
  }
}
