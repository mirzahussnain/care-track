import { HttpErrorResponse } from '@angular/common/http';
import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, convertToParamMap, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';

import { CARETRACK_ROLES, CareTrackRole } from '../../../../core/auth/auth.models';
import { AuthService } from '../../../../core/auth/auth.service';
import { PatientApiService } from '../../../patients/data-access/patient-api.service';
import { ReferralApiService } from '../../../referrals/data-access/referral-api.service';
import { REFERRAL_PRIORITIES, REFERRAL_STATUSES, Referral } from '../../../referrals/models/referral.models';
import { AppointmentApiService } from '../../data-access/appointment-api.service';
import {
  APPOINTMENT_STATUSES,
  APPOINTMENT_TYPES,
  Appointment,
} from '../../models/appointment.models';
import { CreateAppointmentPage } from './create-appointment-page';

describe('CreateAppointmentPage', () => {
  let fixture: ComponentFixture<CreateAppointmentPage>;
  let router: Router;
  const roles = signal<readonly CareTrackRole[]>([CARETRACK_ROLES.referralCoordinator]);
  const referralId = '22222222-2222-2222-2222-222222222222';
  const patientId = '11111111-1111-1111-1111-111111111111';
  const getReferral = vi.fn();
  const getReferralPatientSummary = vi.fn();
  const createAppointment = vi.fn();
  const referral: Referral = {
    id: referralId,
    referralReference: 'REF-001',
    patientId,
    status: REFERRAL_STATUSES.assigned,
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
  const appointment: Appointment = {
    id: '33333333-3333-3333-3333-333333333333',
    appointmentReference: 'APT-001',
    patientId,
    referralId,
    appointmentType: APPOINTMENT_TYPES.consultation,
    scheduledStart: '2026-09-01T09:00:00Z',
    scheduledEnd: '2026-09-01T09:30:00Z',
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

  beforeEach(async () => {
    roles.set([CARETRACK_ROLES.referralCoordinator]);
    getReferral.mockReset().mockReturnValue(of(referral));
    getReferralPatientSummary.mockReset().mockReturnValue(
      of({ id: patientId, patientReference: 'PAT-001', fullName: 'Amina Khan', dateOfBirth: '1988-04-12' }),
    );
    createAppointment.mockReset().mockReturnValue(of(appointment));

    await TestBed.configureTestingModule({
      imports: [CreateAppointmentPage],
      providers: [
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { queryParamMap: convertToParamMap({ referralId }) } },
        },
        { provide: ReferralApiService, useValue: { getReferral } },
        { provide: PatientApiService, useValue: { getReferralPatientSummary } },
        { provide: AppointmentApiService, useValue: { createAppointment } },
        {
          provide: AuthService,
          useValue: { hasRole: (role: CareTrackRole) => roles().includes(role) },
        },
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(CreateAppointmentPage);
    router = TestBed.inject(Router);
  });

  it('loads referral and reduced patient context from the URL handoff', () => {
    fixture.detectChanges();

    expect(getReferral).toHaveBeenCalledWith(referralId);
    expect(getReferralPatientSummary).toHaveBeenCalledWith(patientId);
    expect(fixture.nativeElement.textContent).toContain('REF-001');
    expect(fixture.nativeElement.textContent).toContain('Amina Khan');
    expect(fixture.nativeElement.textContent).toContain('All appointment times use UTC');
  });

  it('submits the exact request with server-related IDs and explicit UTC values', () => {
    fixture.componentInstance.form.setValue({
      appointmentReference: ' APT-001 ',
      appointmentType: APPOINTMENT_TYPES.consultation,
      scheduledStart: '2026-09-01T09:00',
      scheduledEnd: '2026-09-01T09:30',
      location: ' Clinic A ',
    });

    fixture.componentInstance.submit();

    expect(createAppointment).toHaveBeenCalledWith({
      appointmentReference: 'APT-001',
      patientId,
      referralId,
      appointmentType: 0,
      scheduledStart: '2026-09-01T09:00:00.000Z',
      scheduledEnd: '2026-09-01T09:30:00.000Z',
      location: 'Clinic A',
    });
    expect(fixture.componentInstance.form.contains('patientId')).toBe(false);
    expect(fixture.componentInstance.form.contains('referralId')).toBe(false);
  });

  it('keeps a Referral Coordinator on an inline success state', () => {
    const navigate = vi.spyOn(router, 'navigate');
    fixture.componentInstance.form.setValue({
      appointmentReference: 'APT-001',
      appointmentType: APPOINTMENT_TYPES.consultation,
      scheduledStart: '2026-09-01T09:00',
      scheduledEnd: '2026-09-01T09:30',
      location: 'Clinic A',
    });

    fixture.componentInstance.submit();
    fixture.detectChanges();

    expect(navigate).not.toHaveBeenCalled();
    expect(fixture.nativeElement.textContent).toContain('Appointment scheduled');
    expect(fixture.nativeElement.textContent).toContain('require the Clinician role');
  });

  it('navigates a Clinician to readable appointment detail after creation', () => {
    roles.set([CARETRACK_ROLES.clinician]);
    const navigate = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    fixture.componentInstance.form.setValue({
      appointmentReference: 'APT-001',
      appointmentType: APPOINTMENT_TYPES.consultation,
      scheduledStart: '2026-09-01T09:00',
      scheduledEnd: '2026-09-01T09:30',
      location: 'Clinic A',
    });

    fixture.componentInstance.submit();

    expect(navigate).toHaveBeenCalledWith(['/appointments', appointment.id]);
  });

  it('preserves form values and identifies an overlap conflict without retrying', () => {
    createAppointment.mockReturnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 409,
            error: { title: 'Conflict', detail: 'The patient already has an overlapping appointment.' },
          }),
      ),
    );
    fixture.componentInstance.form.setValue({
      appointmentReference: 'APT-001',
      appointmentType: APPOINTMENT_TYPES.consultation,
      scheduledStart: '2026-09-01T09:00',
      scheduledEnd: '2026-09-01T09:30',
      location: 'Clinic A',
    });

    fixture.componentInstance.submit();

    expect(fixture.componentInstance.errorMessage()).toContain('overlapping appointment');
    expect(fixture.componentInstance.form.controls.scheduledStart.value).toBe('2026-09-01T09:00');
    expect(createAppointment).toHaveBeenCalledOnce();
  });

  it.each([
    REFERRAL_STATUSES.scheduled,
    REFERRAL_STATUSES.inProgress,
  ])('accepts verified schedulable referral status %s', (status) => {
    getReferral.mockReturnValue(of({ ...referral, status }));
    fixture.componentInstance.loadContext();

    expect(fixture.componentInstance.contextError()).toBeNull();
  });
});
