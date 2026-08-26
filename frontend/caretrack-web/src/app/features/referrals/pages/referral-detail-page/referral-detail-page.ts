import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Observable, forkJoin, of, switchMap } from 'rxjs';

import { CARETRACK_ROLES } from '../../../../core/auth/auth.models';
import { AuthService } from '../../../../core/auth/auth.service';
import {
  Button,
  EmptyState,
  FormField,
  Skeleton,
  StatusChip,
  Surface,
} from '../../../../design-system/components';
import { PageHeader } from '../../../../design-system/patterns';
import { PatientIdentityBanner } from '../../../../design-system/patterns/patient-identity-banner/patient-identity-banner';
import { PatientApiService } from '../../../patients/data-access/patient-api.service';
import { ReferralPatientSummary } from '../../../patients/models/patient.models';
import { ReferralApiService } from '../../data-access/referral-api.service';
import { ReferralErrorMessage, referralErrorMessage } from '../../models/referral-errors';
import {
  REFERRAL_PRIORITIES,
  REFERRAL_STATUSES,
  Referral,
  ReferralHistoryEntry,
  ReferralPriority,
  ReferralStatus,
  referralHistoryLabel,
  referralPriorityLabel,
  referralPriorityTone,
  referralStatusLabel,
  referralStatusTone,
} from '../../models/referral.models';

type DetailError = 'forbidden' | 'not-found' | 'generic' | null;

@Component({
  selector: 'app-referral-detail-page',
  standalone: true,
  imports: [
    Button,
    DatePipe,
    EmptyState,
    FormField,
    PageHeader,
    PatientIdentityBanner,
    ReactiveFormsModule,
    RouterLink,
    Skeleton,
    StatusChip,
    Surface,
  ],
  templateUrl: './referral-detail-page.html',
  styleUrl: './referral-detail-page.css',
})
export class ReferralDetailPage {
  private readonly route = inject(ActivatedRoute);
  private readonly referralApi = inject(ReferralApiService);
  private readonly patientApi = inject(PatientApiService);
  private readonly authService = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);

  readonly statuses = REFERRAL_STATUSES;
  readonly priorities = Object.values(REFERRAL_PRIORITIES) as ReferralPriority[];
  readonly referralId = this.route.snapshot.paramMap.get('id') ?? '';
  readonly referral = signal<Referral | null>(null);
  readonly patient = signal<ReferralPatientSummary | null>(null);
  readonly history = signal<readonly ReferralHistoryEntry[]>([]);
  readonly assignmentTargets = signal<readonly string[]>([]);
  readonly loading = signal(true);
  readonly detailError = signal<DetailError>(null);
  readonly activeAction = signal<string | null>(null);
  readonly commandError = signal<ReferralErrorMessage | null>(null);
  readonly successMessage = signal<string | null>(null);
  readonly confirmingReject = signal(false);
  readonly canManageReferrals = computed(
    () =>
      this.authService.hasRole(CARETRACK_ROLES.clinician) ||
      this.authService.hasRole(CARETRACK_ROLES.referralCoordinator),
  );

  canScheduleAppointment(status: ReferralStatus): boolean {
    return (
      status === REFERRAL_STATUSES.assigned ||
      status === REFERRAL_STATUSES.scheduled ||
      status === REFERRAL_STATUSES.inProgress
    );
  }

  readonly triageForm = new FormGroup({
    priority: new FormControl<ReferralPriority>(REFERRAL_PRIORITIES.routine, {
      nonNullable: true,
      validators: [Validators.required],
    }),
    note: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(2000)],
    }),
  });
  readonly assignmentForm = new FormGroup({
    assignedTo: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
  });

  constructor() {
    this.loadDetail();
  }

  loadDetail(): void {
    if (!this.referralId) {
      this.loading.set(false);
      this.detailError.set('not-found');
      return;
    }

    this.loading.set(true);
    this.detailError.set(null);
    this.commandError.set(null);

    this.referralApi
      .getReferral(this.referralId)
      .pipe(
        switchMap((referral) =>
          forkJoin({
            referral: of(referral),
            patient: this.patientApi.getReferralPatientSummary(referral.patientId),
            history: this.referralApi.getHistory(referral.id),
            targets: this.referralApi.getAssignmentTargets(),
          }),
        ),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: ({ referral, patient, history, targets }) => {
          this.referral.set(referral);
          this.patient.set(patient);
          this.history.set(history);
          this.assignmentTargets.set(targets.items);
          this.triageForm.patchValue({
            priority: referral.priority,
            note: referral.triageNote ?? '',
          });
          this.assignmentForm.patchValue({
            assignedTo: referral.assignedTo ?? '',
          });
          this.loading.set(false);
        },
        error: (error: HttpErrorResponse) => {
          this.loading.set(false);
          this.detailError.set(
            error.status === 403 ? 'forbidden' : error.status === 404 ? 'not-found' : 'generic',
          );
        },
      });
  }

  submitReferral(): void {
    this.runReferralAction(
      'submit',
      this.referralApi.submitReferral(this.referralId),
      'Referral submitted.',
    );
  }

  startTriage(): void {
    this.runReferralAction(
      'start-triage',
      this.referralApi.startTriage(this.referralId),
      'Referral moved to triage.',
    );
  }

  requestMoreInformation(): void {
    this.runReferralAction(
      'request-information',
      this.referralApi.requestMoreInformation(this.referralId),
      'More information requested.',
    );
  }

  resubmitReferral(): void {
    this.runReferralAction(
      'resubmit',
      this.referralApi.resubmitReferral(this.referralId),
      'Referral resubmitted.',
    );
  }

  acceptReferral(): void {
    this.runReferralAction(
      'accept',
      this.referralApi.acceptReferral(this.referralId),
      'Referral accepted.',
    );
  }

  rejectReferral(): void {
    this.confirmingReject.set(false);
    this.runReferralAction(
      'reject',
      this.referralApi.rejectReferral(this.referralId),
      'Referral rejected.',
    );
  }

  recordTriageAssessment(): void {
    if (this.triageForm.invalid || this.activeAction()) {
      this.triageForm.markAllAsTouched();
      return;
    }
    const value = this.triageForm.getRawValue();
    this.runReferralAction(
      'triage-assessment',
      this.referralApi.recordTriageAssessment(this.referralId, {
        priority: value.priority,
        note: value.note.trim(),
      }),
      'Triage assessment recorded.',
    );
  }

  assignReferral(reassign = false): void {
    if (this.assignmentForm.invalid || this.activeAction()) {
      this.assignmentForm.markAllAsTouched();
      return;
    }
    const request = {
      assignedTo: this.assignmentForm.controls.assignedTo.value,
    };
    this.runReferralAction(
      reassign ? 'reassign' : 'assign',
      reassign
        ? this.referralApi.reassignReferral(this.referralId, request)
        : this.referralApi.assignReferral(this.referralId, request),
      reassign ? 'Referral reassigned.' : 'Referral assigned.',
    );
  }

  completeReferral(): void {
    if (this.activeAction()) {
      return;
    }
    this.prepareAction('complete');
    this.referralApi
      .completeReferral(this.referralId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.activeAction.set(null);
          this.successMessage.set('Referral completed.');
          this.refreshReferralAndHistory();
        },
        error: (error: HttpErrorResponse) => this.handleCommandError(error),
      });
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

  historyLabel(entry: ReferralHistoryEntry): string {
    return referralHistoryLabel(entry.eventType);
  }

  private runReferralAction(
    action: string,
    request: Observable<Referral>,
    successMessage: string,
  ): void {
    if (this.activeAction()) {
      return;
    }
    this.prepareAction(action);
    request.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (referral) => {
        this.referral.set(referral);
        this.activeAction.set(null);
        this.successMessage.set(successMessage);
        this.refreshHistory();
      },
      error: (error: HttpErrorResponse) => this.handleCommandError(error),
    });
  }

  private prepareAction(action: string): void {
    this.activeAction.set(action);
    this.commandError.set(null);
    this.successMessage.set(null);
  }

  private handleCommandError(error: HttpErrorResponse): void {
    this.activeAction.set(null);
    this.commandError.set(
      referralErrorMessage(error, 'The referral action could not be completed. Try again.'),
    );
  }

  private refreshHistory(): void {
    this.referralApi
      .getHistory(this.referralId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (history) => this.history.set(history),
      });
  }

  private refreshReferralAndHistory(): void {
    forkJoin({
      referral: this.referralApi.getReferral(this.referralId),
      history: this.referralApi.getHistory(this.referralId),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ referral, history }) => {
          this.referral.set(referral);
          this.history.set(history);
        },
        error: (error: HttpErrorResponse) => this.handleCommandError(error),
      });
  }
}
