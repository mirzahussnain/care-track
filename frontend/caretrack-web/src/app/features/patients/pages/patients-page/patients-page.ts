import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';

import { CARETRACK_ROLES } from '../../../../core/auth/auth.models';
import { HasRoleDirective } from '../../../../core/auth/has-role.directive';
import {
  Button,
  EmptyState,
  FormField,
  Skeleton,
  Surface,
} from '../../../../design-system/components';
import { DataToolbar, PageHeader } from '../../../../design-system/patterns';
import { Pagination } from '../../../../design-system/patterns/pagination/pagination';
import { PagedResult } from '../../../../shared/models/paged-result.model';
import { PatientApiService } from '../../data-access/patient-api.service';
import { Patient, PatientSortField, SortDirection } from '../../models/patient.models';

type ListError = 'forbidden' | 'generic' | null;

const PATIENT_SORT_OPTIONS = [
  { value: 'lastName', label: 'Last name' },
  { value: 'firstName', label: 'First name' },
  { value: 'patientReference', label: 'Patient reference' },
  { value: 'createdAt', label: 'Created date' },
] as const satisfies readonly { value: PatientSortField; label: string }[];

const SORT_DIRECTION_OPTIONS = [
  { value: 'asc', label: 'Ascending' },
  { value: 'desc', label: 'Descending' },
] as const satisfies readonly { value: SortDirection; label: string }[];

@Component({
  selector: 'app-patients-page',
  standalone: true,
  imports: [
    Button,
    DataToolbar,
    DatePipe,
    EmptyState,
    FormField,
    HasRoleDirective,
    PageHeader,
    Pagination,
    ReactiveFormsModule,
    RouterLink,
    Skeleton,
    Surface,
  ],
  templateUrl: './patients-page.html',
  styleUrl: './patients-page.css',
})
export class PatientsPage {
  private readonly patientApi = inject(PatientApiService);
  private readonly destroyRef = inject(DestroyRef);
  private requestSubscription?: Subscription;

  readonly patientManagers = [
    CARETRACK_ROLES.clinician,
    CARETRACK_ROLES.referralCoordinator,
  ] as const;
  readonly searchControl = new FormControl('', { nonNullable: true });
  readonly searchForm = new FormGroup({ search: this.searchControl });
  readonly sortByControl = new FormControl<PatientSortField>('lastName', { nonNullable: true });
  readonly sortDirectionControl = new FormControl<SortDirection>('asc', { nonNullable: true });
  readonly sortForm = new FormGroup({
    sortBy: this.sortByControl,
    sortDirection: this.sortDirectionControl,
  });
  readonly sortOptions = PATIENT_SORT_OPTIONS;
  readonly sortDirectionOptions = SORT_DIRECTION_OPTIONS;
  readonly appliedSearch = signal('');
  readonly page = signal(1);
  readonly pageSize = 20;
  readonly result = signal<PagedResult<Patient> | null>(null);
  readonly loading = signal(true);
  readonly error = signal<ListError>(null);

  constructor() {
    const sortSubscription = this.sortForm.valueChanges.subscribe(() => {
      this.page.set(1);
      this.loadPatients();
    });
    this.destroyRef.onDestroy(() => {
      sortSubscription.unsubscribe();
      this.requestSubscription?.unsubscribe();
    });
    this.loadPatients();
  }

  submitSearch(): void {
    const search = this.searchControl.value.trim();
    if (search === this.appliedSearch() && this.page() === 1 && !this.error()) {
      return;
    }
    this.appliedSearch.set(search);
    this.page.set(1);
    this.loadPatients();
  }

  clearSearch(): void {
    this.searchControl.setValue('');
    if (!this.appliedSearch() && this.page() === 1 && !this.error()) {
      return;
    }
    this.appliedSearch.set('');
    this.page.set(1);
    this.loadPatients();
  }

  changePage(page: number): void {
    this.page.set(page);
    this.loadPatients();
  }

  retry(): void {
    this.loadPatients();
  }

  private loadPatients(): void {
    this.requestSubscription?.unsubscribe();
    this.loading.set(true);
    this.error.set(null);

    this.requestSubscription = this.patientApi
      .searchPatients({
        search: this.appliedSearch() || undefined,
        page: this.page(),
        pageSize: this.pageSize,
        sortBy: this.sortByControl.value,
        sortDirection: this.sortDirectionControl.value,
      })
      .subscribe({
        next: (result) => {
          this.result.set(result);
          this.loading.set(false);
        },
        error: (error: HttpErrorResponse) => {
          this.result.set(null);
          this.error.set(error.status === 403 ? 'forbidden' : 'generic');
          this.loading.set(false);
        },
      });
  }
}
