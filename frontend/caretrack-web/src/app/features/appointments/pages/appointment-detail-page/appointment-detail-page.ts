import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Observable, Subscription, forkJoin } from 'rxjs';

import { CARETRACK_ROLES } from '../../../../core/auth/auth.models';
import { AuthService } from '../../../../core/auth/auth.service';
import { HasRoleDirective } from '../../../../core/auth/has-role.directive';
import {
  Button,
  EmptyState,
  Skeleton,
  StatusChip,
  Surface,
} from '../../../../design-system/components';
import { PageHeader, PatientIdentityBanner } from '../../../../design-system/patterns';
import {
  ClinicalNotesSection,
  UpdateClinicalNoteEvent,
} from '../../../clinical-notes/components/clinical-notes-section/clinical-notes-section';
import { ClinicalNoteApiService } from '../../../clinical-notes/data-access/clinical-note-api.service';
import { ClinicalNote } from '../../../clinical-notes/models/clinical-note.models';
import { PatientApiService } from '../../../patients/data-access/patient-api.service';
import { ReferralPatientSummary } from '../../../patients/models/patient.models';
import { ReferralApiService } from '../../../referrals/data-access/referral-api.service';
import {
  buttonFromEvent,
  restoreFocusIfAvailable,
} from '../../../../shared/utils/focus-management';
import {
  Referral,
  referralStatusLabel,
  referralStatusTone,
} from '../../../referrals/models/referral.models';
import { AppointmentApiService } from '../../data-access/appointment-api.service';
import { AppointmentErrorMessage, appointmentErrorMessage } from '../../models/appointment-errors';
import { formatAppointmentUtc } from '../../models/appointment-datetime';
import {
  APPOINTMENT_STATUSES,
  Appointment,
  AppointmentStatus,
  AppointmentType,
  appointmentStatusLabel,
  appointmentStatusTone,
  appointmentTypeLabel,
} from '../../models/appointment.models';

type DetailError = 'forbidden' | 'not-found' | 'generic' | null;

@Component({
  selector: 'app-appointment-detail-page',
  standalone: true,
  imports: [
    Button,
    ClinicalNotesSection,
    EmptyState,
    HasRoleDirective,
    PageHeader,
    PatientIdentityBanner,
    RouterLink,
    Skeleton,
    StatusChip,
    Surface,
  ],
  templateUrl: './appointment-detail-page.html',
  styleUrl: './appointment-detail-page.css',
})
export class AppointmentDetailPage {
  private readonly appointmentApi = inject(AppointmentApiService);
  private readonly clinicalNoteApi = inject(ClinicalNoteApiService);
  private readonly patientApi = inject(PatientApiService);
  private readonly referralApi = inject(ReferralApiService);
  private readonly authService = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);
  private detailSubscription?: Subscription;

  readonly clinicianRole = CARETRACK_ROLES.clinician;
  readonly statuses = APPOINTMENT_STATUSES;
  readonly appointmentId = this.route.snapshot.paramMap.get('id') ?? '';
  readonly currentUser = this.authService.currentUser;
  readonly appointment = signal<Appointment | null>(null);
  readonly patient = signal<ReferralPatientSummary | null>(null);
  readonly referral = signal<Referral | null>(null);
  readonly relatedLoading = signal(false);
  readonly relatedUnavailable = signal(false);
  readonly loading = signal(true);
  readonly detailError = signal<DetailError>(null);
  readonly activeAction = signal<string | null>(null);
  readonly commandError = signal<AppointmentErrorMessage | null>(null);
  readonly successMessage = signal<string | null>(null);
  readonly confirmingCancel = signal(false);
  readonly confirmingDidNotAttend = signal(false);
  private cancelTrigger: HTMLButtonElement | null = null;
  private didNotAttendTrigger: HTMLButtonElement | null = null;

  readonly notes = signal<readonly ClinicalNote[]>([]);
  readonly notesLoading = signal(false);
  readonly notesError = signal<string | null>(null);
  readonly noteMutationError = signal<string | null>(null);
  readonly creatingNote = signal(false);
  readonly updatingNoteId = signal<string | null>(null);
  readonly noteSaveVersion = signal(0);

  constructor() {
    this.destroyRef.onDestroy(() => this.detailSubscription?.unsubscribe());
    this.loadAppointment();
  }

  loadAppointment(): void {
    if (!this.appointmentId) {
      this.loading.set(false);
      this.detailError.set('not-found');
      return;
    }

    this.detailSubscription?.unsubscribe();
    this.loading.set(true);
    this.detailError.set(null);
    this.commandError.set(null);
    this.detailSubscription = this.appointmentApi.getAppointment(this.appointmentId).subscribe({
      next: (appointment) => {
        this.appointment.set(appointment);
        this.loading.set(false);
        this.loadRelatedContext(appointment);
        this.loadNotes();
      },
      error: (error: HttpErrorResponse) => {
        this.appointment.set(null);
        this.loading.set(false);
        this.detailError.set(
          error.status === 403 ? 'forbidden' : error.status === 404 ? 'not-found' : 'generic',
        );
      },
    });
  }

  checkIn(): void {
    this.runAction(
      'check-in',
      this.appointmentApi.checkInAppointment(this.appointmentId),
      'Appointment checked in.',
    );
  }

  start(): void {
    this.runAction(
      'start',
      this.appointmentApi.startAppointment(this.appointmentId),
      'Appointment started. The related referral state is managed by the server.',
      true,
    );
  }

  complete(): void {
    this.runAction(
      'complete',
      this.appointmentApi.completeAppointment(this.appointmentId),
      'Appointment completed. The referral remains separate and is not automatically completed.',
    );
  }

  cancel(): void {
    this.confirmingCancel.set(false);
    this.runAction(
      'cancel',
      this.appointmentApi.cancelAppointment(this.appointmentId),
      'Appointment cancelled.',
    );
  }

  markDidNotAttend(): void {
    this.confirmingDidNotAttend.set(false);
    this.runAction(
      'did-not-attend',
      this.appointmentApi.markDidNotAttend(this.appointmentId),
      'Appointment marked as did not attend.',
    );
  }

  showCancelConfirmation(event: MouseEvent): void {
    this.cancelTrigger = buttonFromEvent(event);
    this.confirmingCancel.set(true);
  }

  dismissCancelConfirmation(): void {
    this.confirmingCancel.set(false);
    restoreFocusIfAvailable(this.cancelTrigger);
  }

  showDidNotAttendConfirmation(event: MouseEvent): void {
    this.didNotAttendTrigger = buttonFromEvent(event);
    this.confirmingDidNotAttend.set(true);
  }

  dismissDidNotAttendConfirmation(): void {
    this.confirmingDidNotAttend.set(false);
    restoreFocusIfAvailable(this.didNotAttendTrigger);
  }

  loadNotes(): void {
    this.notesLoading.set(true);
    this.notesError.set(null);
    this.clinicalNoteApi
      .getNotesForAppointment(this.appointmentId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (notes) => {
          this.notes.set(notes);
          this.notesLoading.set(false);
        },
        error: (error: HttpErrorResponse) => {
          this.notesLoading.set(false);
          this.notesError.set(this.noteErrorMessage(error, 'Clinical notes could not be loaded.'));
        },
      });
  }

  createNote(content: string): void {
    if (this.creatingNote() || this.updatingNoteId()) return;
    this.creatingNote.set(true);
    this.noteMutationError.set(null);
    this.clinicalNoteApi
      .createClinicalNote(this.appointmentId, { content })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (note) => {
          this.notes.update((notes) => [...notes, note]);
          this.creatingNote.set(false);
          this.noteSaveVersion.update((version) => version + 1);
        },
        error: (error: HttpErrorResponse) => {
          this.creatingNote.set(false);
          this.noteMutationError.set(
            this.noteErrorMessage(error, 'The clinical note could not be added.'),
          );
        },
      });
  }

  updateNote(event: UpdateClinicalNoteEvent): void {
    if (this.creatingNote() || this.updatingNoteId()) return;
    this.updatingNoteId.set(event.id);
    this.noteMutationError.set(null);
    this.clinicalNoteApi
      .updateClinicalNote(event.id, { content: event.content })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (updated) => {
          this.notes.update((notes) =>
            notes.map((note) => (note.id === updated.id ? updated : note)),
          );
          this.updatingNoteId.set(null);
          this.noteSaveVersion.update((version) => version + 1);
        },
        error: (error: HttpErrorResponse) => {
          this.updatingNoteId.set(null);
          this.noteMutationError.set(
            this.noteErrorMessage(error, 'The clinical note could not be updated.'),
          );
        },
      });
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

  referralStatusLabel(referral: Referral): string {
    return referralStatusLabel(referral.status);
  }

  referralStatusTone(referral: Referral) {
    return referralStatusTone(referral.status);
  }

  utc(value: string | null): string {
    return formatAppointmentUtc(value);
  }

  private runAction(
    action: string,
    request: Observable<Appointment>,
    successMessage: string,
    refreshReferral = false,
  ): void {
    if (this.activeAction()) return;
    this.activeAction.set(action);
    this.commandError.set(null);
    this.successMessage.set(null);
    request.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (appointment) => {
        this.appointment.set(appointment);
        this.activeAction.set(null);
        this.successMessage.set(successMessage);
        if (refreshReferral) this.refreshReferral(appointment.referralId);
      },
      error: (error: HttpErrorResponse) => {
        this.activeAction.set(null);
        this.commandError.set(
          appointmentErrorMessage(
            error,
            'The appointment action could not be completed. Try again.',
          ),
        );
      },
    });
  }

  private loadRelatedContext(appointment: Appointment): void {
    this.relatedLoading.set(true);
    this.relatedUnavailable.set(false);
    forkJoin({
      patient: this.patientApi.getReferralPatientSummary(appointment.patientId),
      referral: this.referralApi.getReferral(appointment.referralId),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ patient, referral }) => {
          this.patient.set(patient);
          this.referral.set(referral);
          this.relatedLoading.set(false);
        },
        error: () => {
          this.patient.set(null);
          this.referral.set(null);
          this.relatedUnavailable.set(true);
          this.relatedLoading.set(false);
        },
      });
  }

  private refreshReferral(referralId: string): void {
    this.referralApi
      .getReferral(referralId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({ next: (referral) => this.referral.set(referral) });
  }

  private noteErrorMessage(error: HttpErrorResponse, fallback: string): string {
    if (error.status === 400) return 'Clinical note content is not valid. Check it and try again.';
    if (error.status === 403) return 'Your role does not permit access to Clinical Notes.';
    if (error.status === 404) return 'The appointment or clinical note is no longer available.';
    return fallback;
  }
}
