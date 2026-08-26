import { HttpErrorResponse } from '@angular/common/http';
import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';

import { CARETRACK_ROLES, CareTrackRole } from '../../../../core/auth/auth.models';
import { AuthService } from '../../../../core/auth/auth.service';
import { ClinicalNoteApiService } from '../../../clinical-notes/data-access/clinical-note-api.service';
import { ClinicalNote } from '../../../clinical-notes/models/clinical-note.models';
import { PatientApiService } from '../../../patients/data-access/patient-api.service';
import { ReferralApiService } from '../../../referrals/data-access/referral-api.service';
import {
  REFERRAL_PRIORITIES,
  REFERRAL_STATUSES,
  Referral,
} from '../../../referrals/models/referral.models';
import { AppointmentApiService } from '../../data-access/appointment-api.service';
import {
  APPOINTMENT_STATUSES,
  APPOINTMENT_TYPES,
  Appointment,
} from '../../models/appointment.models';
import { AppointmentDetailPage } from './appointment-detail-page';

describe('AppointmentDetailPage', () => {
  let fixture: ComponentFixture<AppointmentDetailPage>;
  const roles = signal<readonly CareTrackRole[]>([CARETRACK_ROLES.clinician]);
  const currentUser = signal({
    id: 'user-001',
    name: 'Dr Amina Khan',
    username: 'amina@example.test',
    roles: ['Clinician'],
  });
  const appointmentId = '33333333-3333-3333-3333-333333333333';
  const patientId = '11111111-1111-1111-1111-111111111111';
  const referralId = '22222222-2222-2222-2222-222222222222';
  const getAppointment = vi.fn();
  const checkInAppointment = vi.fn();
  const startAppointment = vi.fn();
  const completeAppointment = vi.fn();
  const cancelAppointment = vi.fn();
  const markDidNotAttend = vi.fn();
  const getReferralPatientSummary = vi.fn();
  const getReferral = vi.fn();
  const getNotesForAppointment = vi.fn();
  const createClinicalNote = vi.fn();
  const updateClinicalNote = vi.fn();

  const appointment: Appointment = {
    id: appointmentId,
    appointmentReference: 'APT-001',
    patientId,
    referralId,
    appointmentType: APPOINTMENT_TYPES.consultation,
    scheduledStart: '2026-09-01T09:00:00',
    scheduledEnd: '2026-09-01T09:30:00',
    location: 'Clinic A',
    status: APPOINTMENT_STATUSES.scheduled,
    createdAt: '2026-08-25T10:00:00Z',
    updatedAt: null,
    checkedInAt: null,
    startedAt: null,
    completedAt: null,
    cancelledAt: null,
    didNotAttendAt: null,
  };
  const referral: Referral = {
    id: referralId,
    referralReference: 'REF-001',
    patientId,
    status: REFERRAL_STATUSES.scheduled,
    priority: REFERRAL_PRIORITIES.routine,
    reason: 'Synthetic referral',
    triageNote: null,
    createdAt: '2026-08-25T09:00:00Z',
    submittedAt: null,
    updatedAt: null,
    triagedAt: null,
    assignedTo: 'Team A',
    assignedAt: null,
  };
  const note: ClinicalNote = {
    id: '44444444-4444-4444-4444-444444444444',
    appointmentId,
    content: 'Synthetic note.',
    createdBy: 'user-001',
    createdAt: '2026-08-25T11:00:00Z',
    updatedAt: null,
  };

  beforeEach(async () => {
    roles.set([CARETRACK_ROLES.clinician]);
    for (const mock of [
      getAppointment,
      checkInAppointment,
      startAppointment,
      completeAppointment,
      cancelAppointment,
      markDidNotAttend,
      getReferralPatientSummary,
      getReferral,
      getNotesForAppointment,
      createClinicalNote,
      updateClinicalNote,
    ])
      mock.mockReset();

    getAppointment.mockReturnValue(of(appointment));
    getReferralPatientSummary.mockReturnValue(
      of({
        id: patientId,
        patientReference: 'PAT-001',
        fullName: 'Amina Khan',
        dateOfBirth: '1988-04-12',
      }),
    );
    getReferral.mockReturnValue(of(referral));
    getNotesForAppointment.mockReturnValue(of([note]));

    await TestBed.configureTestingModule({
      imports: [AppointmentDetailPage],
      providers: [
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: convertToParamMap({ id: appointmentId }) } },
        },
        {
          provide: AppointmentApiService,
          useValue: {
            getAppointment,
            checkInAppointment,
            startAppointment,
            completeAppointment,
            cancelAppointment,
            markDidNotAttend,
          },
        },
        { provide: PatientApiService, useValue: { getReferralPatientSummary } },
        { provide: ReferralApiService, useValue: { getReferral } },
        {
          provide: ClinicalNoteApiService,
          useValue: { getNotesForAppointment, createClinicalNote, updateClinicalNote },
        },
        {
          provide: AuthService,
          useValue: {
            currentUser: currentUser.asReadonly(),
            hasRole: (role: CareTrackRole) => roles().includes(role),
          },
        },
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(AppointmentDetailPage);
  });

  it('loads appointment, patient, referral, and Clinical Notes independently', () => {
    fixture.detectChanges();
    const text = fixture.nativeElement.textContent as string;

    expect(getAppointment).toHaveBeenCalledWith(appointmentId);
    expect(getReferralPatientSummary).toHaveBeenCalledWith(patientId);
    expect(getReferral).toHaveBeenCalledWith(referralId);
    expect(getNotesForAppointment).toHaveBeenCalledWith(appointmentId);
    expect(text).toContain('APT-001');
    expect(text).toContain('Amina Khan');
    expect(text).toContain('REF-001');
    expect(text).toContain('Synthetic note.');
    expect(text).toContain('01 Sep 2026, 09:00 UTC');
  });

  it.each([
    [APPOINTMENT_STATUSES.scheduled, 'Check in'],
    [APPOINTMENT_STATUSES.checkedIn, 'Start appointment'],
    [APPOINTMENT_STATUSES.inProgress, 'Complete appointment'],
  ] as const)('shows only the valid primary action for status %s', (status, label) => {
    fixture.componentInstance.appointment.set({ ...appointment, status });
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain(label);
  });

  it.each([
    APPOINTMENT_STATUSES.completed,
    APPOINTMENT_STATUSES.cancelled,
    APPOINTMENT_STATUSES.didNotAttend,
  ])('shows no mutation controls for terminal status %s', (status) => {
    fixture.componentInstance.appointment.set({ ...appointment, status });
    fixture.detectChanges();
    const text = fixture.nativeElement.textContent as string;
    expect(text).toContain('No workflow actions are available');
    expect(text).not.toContain('Check in');
    expect(text).not.toContain('Start appointment');
    expect(text).not.toContain('Complete appointment');
  });

  it('uses the exact workflow service and refreshes referral state after Start', () => {
    const started = {
      ...appointment,
      status: APPOINTMENT_STATUSES.inProgress,
      startedAt: '2026-09-01T09:05:00Z',
    };
    startAppointment.mockReturnValue(of(started));
    getReferral.mockReturnValue(of({ ...referral, status: REFERRAL_STATUSES.inProgress }));

    fixture.componentInstance.start();

    expect(startAppointment).toHaveBeenCalledWith(appointmentId);
    expect(fixture.componentInstance.appointment()?.status).toBe(APPOINTMENT_STATUSES.inProgress);
    expect(getReferral).toHaveBeenCalledTimes(2);
    expect(fixture.componentInstance.referral()?.status).toBe(REFERRAL_STATUSES.inProgress);
  });

  it('preserves detail and offers reload for an invalid-state conflict', () => {
    checkInAppointment.mockReturnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 409,
            error: { title: 'Invalid State Transition', detail: 'Cannot check in.' },
          }),
      ),
    );

    fixture.componentInstance.checkIn();
    fixture.detectChanges();

    expect(fixture.componentInstance.appointment()?.id).toBe(appointmentId);
    expect(fixture.componentInstance.commandError()?.kind).toBe('workflow');
    expect(fixture.nativeElement.textContent).toContain('Reload appointment');
  });

  it('updates note state from create and edit responses without submitting CreatedBy', () => {
    const created = { ...note, id: '55555555-5555-5555-5555-555555555555', content: 'New note.' };
    createClinicalNote.mockReturnValue(of(created));
    updateClinicalNote.mockReturnValue(
      of({ ...note, content: 'Updated note.', updatedAt: '2026-08-25T12:00:00Z' }),
    );

    fixture.componentInstance.createNote('New note.');
    expect(createClinicalNote).toHaveBeenCalledWith(appointmentId, { content: 'New note.' });
    expect(createClinicalNote.mock.calls[0][1]).not.toHaveProperty('createdBy');

    fixture.componentInstance.updateNote({ id: note.id, content: 'Updated note.' });
    expect(updateClinicalNote).toHaveBeenCalledWith(note.id, { content: 'Updated note.' });
    expect(fixture.componentInstance.notes().find((item) => item.id === note.id)?.content).toBe(
      'Updated note.',
    );
  });

  it('hides workflow actions and Clinical Notes without an Administrator bypass', () => {
    roles.set([CARETRACK_ROLES.administrator]);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('#appointment-actions-title')).toBeNull();
    expect(fixture.nativeElement.querySelector('#clinical-notes-title')).toBeNull();
  });

  it('keeps the Appointment visible when related identity or notes fail', () => {
    getReferralPatientSummary.mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 500 })),
    );
    getNotesForAppointment.mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 500 })),
    );
    fixture.componentInstance.loadAppointment();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('APT-001');
    expect(fixture.nativeElement.textContent).toContain(
      'Patient and referral context is unavailable',
    );
    expect(fixture.nativeElement.textContent).toContain('Clinical notes could not be loaded');
  });

  it('maps appointment workflow decisions to their required semantic variants', () => {
    fixture.componentInstance.appointment.set({
      ...appointment,
      status: APPOINTMENT_STATUSES.scheduled,
    });
    fixture.detectChanges();

    const buttonFor = (label: string): HTMLButtonElement =>
      [...fixture.nativeElement.querySelectorAll('button')].find((button: HTMLButtonElement) =>
        button.textContent?.includes(label),
      ) as HTMLButtonElement;

    expect(buttonFor('Check in').classList).toContain('ct-button--primary');
    expect(buttonFor('Mark as did not attend').classList).toContain('ct-button--warning');
    expect(buttonFor('Cancel appointment').classList).toContain('ct-button--danger');

    fixture.componentInstance.appointment.set({
      ...appointment,
      status: APPOINTMENT_STATUSES.inProgress,
    });
    fixture.detectChanges();

    expect(buttonFor('Complete appointment').classList).toContain('ct-button--success');
  });

  it('restores focus when inline appointment confirmations are dismissed', async () => {
    fixture.detectChanges();
    const buttonFor = (label: string): HTMLButtonElement =>
      [...fixture.nativeElement.querySelectorAll('button')].find((button: HTMLButtonElement) =>
        button.textContent?.includes(label),
      ) as HTMLButtonElement;

    const didNotAttendTrigger = buttonFor('Mark as did not attend');
    didNotAttendTrigger.focus();
    didNotAttendTrigger.click();
    fixture.detectChanges();
    buttonFor('Keep scheduled').click();
    fixture.detectChanges();
    await Promise.resolve();
    expect(document.activeElement).toBe(didNotAttendTrigger);

    const cancelTrigger = buttonFor('Cancel appointment');
    cancelTrigger.focus();
    cancelTrigger.click();
    fixture.detectChanges();
    buttonFor('Keep appointment').click();
    fixture.detectChanges();
    await Promise.resolve();
    expect(document.activeElement).toBe(cancelTrigger);
  });
});
