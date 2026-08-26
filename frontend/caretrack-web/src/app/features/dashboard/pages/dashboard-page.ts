import { DatePipe } from '@angular/common';
import {
  Component,
  DestroyRef,
  WritableSignal,
  computed,
  effect,
  inject,
  signal,
  untracked,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { Observable, Subscription, finalize } from 'rxjs';

import { CARETRACK_ROLES } from '../../../core/auth/auth.models';
import { AuthService } from '../../../core/auth/auth.service';
import { HasRoleDirective } from '../../../core/auth/has-role.directive';
import {
  Button,
  EmptyState,
  Skeleton,
  StatusChip,
  Surface,
} from '../../../design-system/components';
import { PageHeader } from '../../../design-system/patterns';
import { PagedResult } from '../../../shared/models/paged-result.model';
import { AppointmentApiService } from '../../appointments/data-access/appointment-api.service';
import { formatAppointmentUtc } from '../../appointments/models/appointment-datetime';
import {
  APPOINTMENT_STATUSES,
  AppointmentSearchItem,
  AppointmentStatus,
  AppointmentType,
  appointmentStatusLabel,
  appointmentStatusTone,
  appointmentTypeLabel,
} from '../../appointments/models/appointment.models';
import { PatientApiService } from '../../patients/data-access/patient-api.service';
import { ReferralApiService } from '../../referrals/data-access/referral-api.service';
import {
  REFERRAL_STATUSES,
  Referral,
  ReferralPriority,
  ReferralStatus,
  referralPriorityLabel,
  referralPriorityTone,
  referralStatusLabel,
  referralStatusTone,
} from '../../referrals/models/referral.models';

type DashboardAudience =
  'loading' | 'auth-error' | 'clinician' | 'coordinator' | 'administrator' | 'unsupported';
type LoadStatus = 'idle' | 'loading' | 'ready' | 'error';

interface Loadable<T> {
  readonly status: LoadStatus;
  readonly data: T | null;
}

interface DashboardQueue<T> {
  readonly items: readonly T[];
  readonly totalCount: number;
}

const IDLE_STATE: Loadable<never> = { status: 'idle', data: null };
const LOADING_STATE: Loadable<never> = { status: 'loading', data: null };
const UPCOMING_WINDOW_MS = 7 * 24 * 60 * 60 * 1000;
const DASHBOARD_LIST_SIZE = 5;

@Component({
  selector: 'app-dashboard-page',
  imports: [
    Button,
    DatePipe,
    EmptyState,
    HasRoleDirective,
    PageHeader,
    RouterLink,
    Skeleton,
    StatusChip,
    Surface,
  ],
  templateUrl: './dashboard-page.html',
  styleUrl: './dashboard-page.css',
})
export class DashboardPage {
  private readonly authService = inject(AuthService);
  private readonly patientApi = inject(PatientApiService);
  private readonly referralApi = inject(ReferralApiService);
  private readonly appointmentApi = inject(AppointmentApiService);
  private readonly destroyRef = inject(DestroyRef);

  private activeBatchSubscription?: Subscription;
  private activeBatchId: number | null = null;
  private nextBatchId = 0;
  private pendingRequests = 0;
  private loadedAudience: DashboardAudience | null = null;

  readonly roles = CARETRACK_ROLES;
  readonly clinicianRole = CARETRACK_ROLES.clinician;
  readonly coordinatorRole = CARETRACK_ROLES.referralCoordinator;

  readonly audience = computed<DashboardAudience>(() => {
    const authStatus = this.authService.status();
    if (authStatus === 'idle' || authStatus === 'loading') return 'loading';
    if (authStatus === 'error') return 'auth-error';
    if (this.authService.hasRole(CARETRACK_ROLES.clinician)) return 'clinician';
    if (this.authService.hasRole(CARETRACK_ROLES.referralCoordinator)) return 'coordinator';
    if (this.authService.hasRole(CARETRACK_ROLES.administrator)) return 'administrator';
    return 'unsupported';
  });

  readonly dashboardDescription = computed(() => {
    switch (this.audience()) {
      case 'clinician':
        return 'Review referral attention queues and scheduled clinical work.';
      case 'coordinator':
        return 'Review referral workload and the queues that need coordination.';
      case 'administrator':
        return 'Your CareTrack workspace.';
      default:
        return 'Operational workload and activity.';
    }
  });

  readonly batchLoading = signal(false);
  readonly patientCount = signal<Loadable<number>>(IDLE_STATE);
  readonly awaitingTriage = signal<Loadable<DashboardQueue<Referral>>>(IDLE_STATE);
  readonly moreInformation = signal<Loadable<DashboardQueue<Referral>>>(IDLE_STATE);
  readonly acceptedCount = signal<Loadable<number>>(IDLE_STATE);
  readonly assignedCount = signal<Loadable<number>>(IDLE_STATE);
  readonly upcomingAppointments =
    signal<Loadable<DashboardQueue<AppointmentSearchItem>>>(IDLE_STATE);
  readonly inProgressAppointments =
    signal<Loadable<DashboardQueue<AppointmentSearchItem>>>(IDLE_STATE);

  readonly operationalAudience = computed(
    () => this.audience() === 'clinician' || this.audience() === 'coordinator',
  );

  readonly summaryHasError = computed(() => {
    const states: readonly Loadable<unknown>[] =
      this.audience() === 'clinician'
        ? [
            this.patientCount(),
            this.awaitingTriage(),
            this.moreInformation(),
            this.upcomingAppointments(),
            this.inProgressAppointments(),
          ]
        : [
            this.awaitingTriage(),
            this.moreInformation(),
            this.acceptedCount(),
            this.assignedCount(),
          ];
    return states.some((state) => state.status === 'error');
  });

  constructor() {
    effect(() => {
      const audience = this.audience();
      untracked(() => this.handleAudienceChange(audience));
    });
    this.destroyRef.onDestroy(() => this.cancelActiveBatch());
  }

  refresh(): void {
    const audience = this.audience();
    if (this.batchLoading() || (audience !== 'clinician' && audience !== 'coordinator')) {
      return;
    }
    this.startBatch(audience);
  }

  referralStatusLabel(status: ReferralStatus): string {
    return referralStatusLabel(status);
  }

  referralStatusTone(status: ReferralStatus) {
    return referralStatusTone(status);
  }

  priorityLabel(priority: ReferralPriority): string {
    return referralPriorityLabel(priority);
  }

  priorityTone(priority: ReferralPriority) {
    return referralPriorityTone(priority);
  }

  appointmentStatusLabel(status: AppointmentStatus): string {
    return appointmentStatusLabel(status);
  }

  appointmentStatusTone(status: AppointmentStatus) {
    return appointmentStatusTone(status);
  }

  appointmentTypeLabel(type: AppointmentType): string {
    return appointmentTypeLabel(type);
  }

  appointmentUtc(value: string): string {
    return formatAppointmentUtc(value);
  }

  referralRoute(id: string): readonly string[] {
    return ['/referrals', id];
  }

  appointmentRoute(id: string): readonly string[] {
    return ['/appointments', id];
  }

  private handleAudienceChange(audience: DashboardAudience): void {
    if (audience === this.loadedAudience) return;

    this.loadedAudience = audience;
    this.cancelActiveBatch();
    this.resetAllState();

    if (audience === 'clinician' || audience === 'coordinator') {
      this.startBatch(audience);
    }
  }

  private startBatch(audience: 'clinician' | 'coordinator'): void {
    if (this.activeBatchId !== null) return;

    this.resetAllState();
    const batchId = ++this.nextBatchId;
    const batch = new Subscription();

    this.activeBatchId = batchId;
    this.activeBatchSubscription = batch;
    this.pendingRequests = audience === 'clinician' ? 5 : 4;
    this.batchLoading.set(true);
    this.awaitingTriage.set(LOADING_STATE);
    this.moreInformation.set(LOADING_STATE);

    this.addRequest(
      batch,
      batchId,
      audience,
      this.referralApi.searchReferrals({
        status: REFERRAL_STATUSES.awaitingTriage,
        page: 1,
        pageSize: DASHBOARD_LIST_SIZE,
        sortBy: 'priority',
        sortDirection: 'desc',
      }),
      this.awaitingTriage,
      this.toQueue,
    );

    this.addRequest(
      batch,
      batchId,
      audience,
      this.referralApi.searchReferrals({
        status: REFERRAL_STATUSES.moreInformationRequired,
        page: 1,
        pageSize: DASHBOARD_LIST_SIZE,
        sortBy: 'priority',
        sortDirection: 'desc',
      }),
      this.moreInformation,
      this.toQueue,
    );

    if (audience === 'clinician') {
      this.startClinicianRequests(batch, batchId);
    } else {
      this.startCoordinatorRequests(batch, batchId);
    }
  }

  private startClinicianRequests(batch: Subscription, batchId: number): void {
    this.patientCount.set(LOADING_STATE);
    this.upcomingAppointments.set(LOADING_STATE);
    this.inProgressAppointments.set(LOADING_STATE);

    const scheduledFrom = new Date();
    const scheduledTo = new Date(scheduledFrom.getTime() + UPCOMING_WINDOW_MS);

    this.addRequest(
      batch,
      batchId,
      'clinician',
      this.patientApi.searchPatients({
        page: 1,
        pageSize: 1,
        sortBy: 'lastName',
        sortDirection: 'asc',
      }),
      this.patientCount,
      (result) => result.totalCount,
    );

    this.addRequest(
      batch,
      batchId,
      'clinician',
      this.appointmentApi.searchAppointments({
        status: APPOINTMENT_STATUSES.scheduled,
        scheduledFrom: scheduledFrom.toISOString(),
        scheduledTo: scheduledTo.toISOString(),
        page: 1,
        pageSize: DASHBOARD_LIST_SIZE,
        sortBy: 'scheduledStart',
        sortDirection: 'asc',
      }),
      this.upcomingAppointments,
      this.toQueue,
    );

    this.addRequest(
      batch,
      batchId,
      'clinician',
      this.appointmentApi.searchAppointments({
        status: APPOINTMENT_STATUSES.inProgress,
        page: 1,
        pageSize: DASHBOARD_LIST_SIZE,
        sortBy: 'scheduledStart',
        sortDirection: 'asc',
      }),
      this.inProgressAppointments,
      this.toQueue,
    );
  }

  private startCoordinatorRequests(batch: Subscription, batchId: number): void {
    this.acceptedCount.set(LOADING_STATE);
    this.assignedCount.set(LOADING_STATE);

    this.addRequest(
      batch,
      batchId,
      'coordinator',
      this.referralApi.searchReferrals({
        status: REFERRAL_STATUSES.accepted,
        page: 1,
        pageSize: 1,
        sortBy: 'createdAt',
        sortDirection: 'desc',
      }),
      this.acceptedCount,
      (result) => result.totalCount,
    );

    this.addRequest(
      batch,
      batchId,
      'coordinator',
      this.referralApi.searchReferrals({
        status: REFERRAL_STATUSES.assigned,
        page: 1,
        pageSize: 1,
        sortBy: 'createdAt',
        sortDirection: 'desc',
      }),
      this.assignedCount,
      (result) => result.totalCount,
    );
  }

  private addRequest<TResponse, TData>(
    batch: Subscription,
    batchId: number,
    audience: 'clinician' | 'coordinator',
    request: Observable<TResponse>,
    target: WritableSignal<Loadable<TData>>,
    select: (response: TResponse) => TData,
  ): void {
    batch.add(
      request.pipe(finalize(() => this.finishRequest(batchId))).subscribe({
        next: (response) => {
          if (this.isCurrentBatch(batchId, audience)) {
            target.set({ status: 'ready', data: select(response) });
          }
        },
        error: () => {
          if (this.isCurrentBatch(batchId, audience)) {
            target.set({ status: 'error', data: null });
          }
        },
      }),
    );
  }

  private readonly toQueue = <T>(result: PagedResult<T>): DashboardQueue<T> => ({
    items: result.items,
    totalCount: result.totalCount,
  });

  private isCurrentBatch(batchId: number, audience: 'clinician' | 'coordinator'): boolean {
    return this.activeBatchId === batchId && this.audience() === audience;
  }

  private finishRequest(batchId: number): void {
    if (this.activeBatchId !== batchId) return;

    this.pendingRequests -= 1;
    if (this.pendingRequests === 0) {
      this.activeBatchId = null;
      this.activeBatchSubscription = undefined;
      this.batchLoading.set(false);
    }
  }

  private cancelActiveBatch(): void {
    const subscription = this.activeBatchSubscription;

    this.activeBatchId = null;
    this.activeBatchSubscription = undefined;
    this.pendingRequests = 0;
    this.batchLoading.set(false);
    subscription?.unsubscribe();
  }

  private resetAllState(): void {
    this.patientCount.set(IDLE_STATE);
    this.awaitingTriage.set(IDLE_STATE);
    this.moreInformation.set(IDLE_STATE);
    this.acceptedCount.set(IDLE_STATE);
    this.assignedCount.set(IDLE_STATE);
    this.upcomingAppointments.set(IDLE_STATE);
    this.inProgressAppointments.set(IDLE_STATE);
  }
}
