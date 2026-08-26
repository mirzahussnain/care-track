import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, inject, input, output, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';

import { Button, EmptyState, FormField, Skeleton } from '../../../../design-system/components';
import { Pagination } from '../../../../design-system/patterns/pagination/pagination';
import { PagedResult } from '../../../../shared/models/paged-result.model';
import { PatientApiService } from '../../../patients/data-access/patient-api.service';
import { ReferralPatientSummary } from '../../../patients/models/patient.models';

type LookupError = 'forbidden' | 'generic' | null;

@Component({
  selector: 'app-referral-patient-selector',
  standalone: true,
  imports: [Button, EmptyState, FormField, Pagination, ReactiveFormsModule, Skeleton],
  templateUrl: './referral-patient-selector.html',
  styleUrl: './referral-patient-selector.css',
})
export class ReferralPatientSelector {
  private readonly patientApi = inject(PatientApiService);
  private readonly destroyRef = inject(DestroyRef);
  private requestSubscription?: Subscription;

  readonly selectedPatient = input<ReferralPatientSummary | null>(null);
  readonly patientSelected = output<ReferralPatientSummary>();
  readonly searchControl = new FormControl('', { nonNullable: true });
  readonly searchForm = new FormGroup({ search: this.searchControl });
  readonly appliedSearch = signal('');
  readonly page = signal(1);
  readonly pageSize = 5;
  readonly result = signal<PagedResult<ReferralPatientSummary> | null>(null);
  readonly loading = signal(false);
  readonly searched = signal(false);
  readonly error = signal<LookupError>(null);

  constructor() {
    this.destroyRef.onDestroy(() => this.requestSubscription?.unsubscribe());
  }

  submitSearch(): void {
    this.appliedSearch.set(this.searchControl.value.trim());
    this.page.set(1);
    this.loadPatients();
  }

  clearSearch(): void {
    this.searchControl.setValue('');
    this.appliedSearch.set('');
    this.page.set(1);
    this.loadPatients();
  }

  changePage(page: number): void {
    this.page.set(page);
    this.loadPatients();
  }

  selectPatient(patient: ReferralPatientSummary): void {
    this.patientSelected.emit(patient);
  }

  retry(): void {
    this.loadPatients();
  }

  private loadPatients(): void {
    this.requestSubscription?.unsubscribe();
    this.loading.set(true);
    this.searched.set(true);
    this.error.set(null);

    this.requestSubscription = this.patientApi
      .searchReferralPatients({
        search: this.appliedSearch() || undefined,
        page: this.page(),
        pageSize: this.pageSize,
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
