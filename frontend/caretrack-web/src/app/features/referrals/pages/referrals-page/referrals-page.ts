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
  StatusChip,
  Surface,
} from '../../../../design-system/components';
import { DataToolbar, PageHeader } from '../../../../design-system/patterns';
import { Pagination } from '../../../../design-system/patterns/pagination/pagination';
import { PagedResult } from '../../../../shared/models/paged-result.model';
import { ReferralApiService } from '../../data-access/referral-api.service';
import {
  REFERRAL_PRIORITIES,
  REFERRAL_STATUSES,
  Referral,
  ReferralPriority,
  ReferralSortField,
  ReferralStatus,
  SortDirection,
  referralPriorityLabel,
  referralPriorityTone,
  referralStatusLabel,
  referralStatusTone,
} from '../../models/referral.models';

type ListError = 'forbidden' | 'validation' | 'generic' | null;

interface AppliedFilters {
  readonly status: ReferralStatus | null;
  readonly priority: ReferralPriority | null;
  readonly assignedTo: string;
  readonly createdFrom: string;
  readonly createdTo: string;
  readonly sortBy: ReferralSortField;
  readonly sortDirection: SortDirection;
}

@Component({
  selector: 'app-referrals-page',
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
    StatusChip,
    Surface,
  ],
  templateUrl: './referrals-page.html',
  styleUrl: './referrals-page.css',
})
export class ReferralsPage {
  private readonly referralApi = inject(ReferralApiService);
  private readonly destroyRef = inject(DestroyRef);
  private requestSubscription?: Subscription;
  private targetsSubscription?: Subscription;

  readonly referralManagers = [
    CARETRACK_ROLES.clinician,
    CARETRACK_ROLES.referralCoordinator,
  ] as const;
  readonly statuses = Object.values(REFERRAL_STATUSES) as ReferralStatus[];
  readonly priorities = Object.values(REFERRAL_PRIORITIES) as ReferralPriority[];
  readonly sortOptions = [
    { value: 'createdAt', label: 'Created date' },
    { value: 'updatedAt', label: 'Updated date' },
    { value: 'priority', label: 'Priority' },
    { value: 'status', label: 'Status' },
    { value: 'referralReference', label: 'Referral reference' },
  ] as const satisfies readonly { value: ReferralSortField; label: string }[];
  readonly assignmentTargets = signal<readonly string[]>([]);

  readonly filters = new FormGroup({
    status: new FormControl<ReferralStatus | null>(null),
    priority: new FormControl<ReferralPriority | null>(null),
    assignedTo: new FormControl('', { nonNullable: true }),
    createdFrom: new FormControl('', { nonNullable: true }),
    createdTo: new FormControl('', { nonNullable: true }),
    sortBy: new FormControl<ReferralSortField>('createdAt', { nonNullable: true }),
    sortDirection: new FormControl<SortDirection>('desc', { nonNullable: true }),
  });

  readonly appliedFilters = signal<AppliedFilters>(this.readFilters());
  readonly page = signal(1);
  readonly pageSize = 20;
  readonly result = signal<PagedResult<Referral> | null>(null);
  readonly loading = signal(true);
  readonly error = signal<ListError>(null);

  constructor() {
    this.destroyRef.onDestroy(() => {
      this.requestSubscription?.unsubscribe();
      this.targetsSubscription?.unsubscribe();
    });
    this.loadAssignmentTargets();
    this.loadReferrals();
  }

  applyFilters(): void {
    const filters = this.readFilters();
    if (
      filters.createdFrom &&
      filters.createdTo &&
      filters.createdFrom > filters.createdTo
    ) {
      this.error.set('validation');
      return;
    }
    this.appliedFilters.set(filters);
    this.page.set(1);
    this.loadReferrals();
  }

  resetFilters(): void {
    this.filters.reset({
      status: null,
      priority: null,
      assignedTo: '',
      createdFrom: '',
      createdTo: '',
      sortBy: 'createdAt',
      sortDirection: 'desc',
    });
    this.appliedFilters.set(this.readFilters());
    this.page.set(1);
    this.loadReferrals();
  }

  changePage(page: number): void {
    this.page.set(page);
    this.loadReferrals();
  }

  retry(): void {
    this.loadReferrals();
  }

  statusLabel(status: ReferralStatus): string {
    return referralStatusLabel(status);
  }

  statusTone(status: ReferralStatus) {
    return referralStatusTone(status);
  }

  priorityLabel(priority: ReferralPriority): string {
    return referralPriorityLabel(priority);
  }

  priorityTone(priority: ReferralPriority) {
    return referralPriorityTone(priority);
  }

  private readFilters(): AppliedFilters {
    const value = this.filters.getRawValue();
    return {
      status: value.status,
      priority: value.priority,
      assignedTo: value.assignedTo,
      createdFrom: value.createdFrom,
      createdTo: value.createdTo,
      sortBy: value.sortBy,
      sortDirection: value.sortDirection,
    };
  }

  private loadAssignmentTargets(): void {
    this.targetsSubscription = this.referralApi.getAssignmentTargets().subscribe({
      next: (response) => this.assignmentTargets.set(response.items),
      error: () => this.assignmentTargets.set([]),
    });
  }

  private loadReferrals(): void {
    this.requestSubscription?.unsubscribe();
    this.loading.set(true);
    this.error.set(null);
    const filters = this.appliedFilters();

    this.requestSubscription = this.referralApi
      .searchReferrals({
        status: filters.status ?? undefined,
        priority: filters.priority ?? undefined,
        assignedTo: filters.assignedTo || undefined,
        createdFrom: filters.createdFrom || undefined,
        createdTo: filters.createdTo || undefined,
        page: this.page(),
        pageSize: this.pageSize,
        sortBy: filters.sortBy,
        sortDirection: filters.sortDirection,
      })
      .subscribe({
        next: (result) => {
          this.result.set(result);
          this.loading.set(false);
        },
        error: (error: HttpErrorResponse) => {
          this.result.set(null);
          this.error.set(
            error.status === 403
              ? 'forbidden'
              : error.status === 400
                ? 'validation'
                : 'generic',
          );
          this.loading.set(false);
        },
      });
  }
}
