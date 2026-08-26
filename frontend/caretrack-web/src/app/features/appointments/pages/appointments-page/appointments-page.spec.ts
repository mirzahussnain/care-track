import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Subject, of, throwError } from 'rxjs';

import { PagedResult } from '../../../../shared/models/paged-result.model';
import { PatientApiService } from '../../../patients/data-access/patient-api.service';
import { ReferralApiService } from '../../../referrals/data-access/referral-api.service';
import { REFERRAL_PRIORITIES, REFERRAL_STATUSES, Referral } from '../../../referrals/models/referral.models';
import { AppointmentApiService } from '../../data-access/appointment-api.service';
import {
  APPOINTMENT_STATUSES,
  APPOINTMENT_TYPES,
  AppointmentSearchItem,
} from '../../models/appointment.models';
import { AppointmentsPage } from './appointments-page';

describe('AppointmentsPage', () => {
  let fixture: ComponentFixture<AppointmentsPage>;
  let response$: Subject<PagedResult<AppointmentSearchItem>>;
  const searchAppointments = vi.fn();
  const getReferralPatientSummary = vi.fn();
  const getReferral = vi.fn();
  const patientId = '11111111-1111-1111-1111-111111111111';
  const referralId = '22222222-2222-2222-2222-222222222222';
  const appointment: AppointmentSearchItem = {
    id: '33333333-3333-3333-3333-333333333333',
    appointmentReference: 'APT-001',
    patientId,
    referralId,
    appointmentType: APPOINTMENT_TYPES.consultation,
    scheduledStart: '2026-09-01T09:00:00',
    scheduledEnd: '2026-09-01T09:30:00',
    location: 'Clinic A',
    status: APPOINTMENT_STATUSES.scheduled,
    createdAt: '2026-08-25T10:00:00Z',
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

  beforeEach(async () => {
    response$ = new Subject<PagedResult<AppointmentSearchItem>>();
    searchAppointments.mockReset().mockReturnValue(response$);
    getReferralPatientSummary.mockReset().mockReturnValue(
      of({ id: patientId, patientReference: 'PAT-001', fullName: 'Amina Khan', dateOfBirth: '1988-04-12' }),
    );
    getReferral.mockReset().mockReturnValue(of(referral));

    await TestBed.configureTestingModule({
      imports: [AppointmentsPage],
      providers: [
        provideRouter([]),
        { provide: AppointmentApiService, useValue: { searchAppointments } },
        { provide: PatientApiService, useValue: { getReferralPatientSummary } },
        { provide: ReferralApiService, useValue: { getReferral } },
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(AppointmentsPage);
  });

  it('loads with backend defaults and a visible loading state', () => {
    fixture.detectChanges();

    expect(searchAppointments).toHaveBeenCalledWith({
      status: undefined,
      appointmentType: undefined,
      location: undefined,
      scheduledFrom: undefined,
      scheduledTo: undefined,
      page: 1,
      pageSize: 20,
      sortBy: 'scheduledStart',
      sortDirection: 'asc',
    });
    expect(fixture.nativeElement.querySelector('[aria-label="Loading appointments"]')).not.toBeNull();
  });

  it('deduplicates identity enrichment and renders human-readable context', () => {
    response$.next({
      items: [appointment, { ...appointment, id: '44444444-4444-4444-4444-444444444444' }],
      page: 1,
      pageSize: 20,
      totalCount: 2,
      totalPages: 1,
    });
    fixture.detectChanges();

    expect(getReferralPatientSummary).toHaveBeenCalledOnce();
    expect(getReferralPatientSummary).toHaveBeenCalledWith(patientId);
    expect(getReferral).toHaveBeenCalledOnce();
    expect(getReferral).toHaveBeenCalledWith(referralId);
    expect(fixture.nativeElement.textContent).toContain('Amina Khan');
    expect(fixture.nativeElement.textContent).toContain('PAT-001');
    expect(fixture.nativeElement.textContent).toContain('REF-001');
    expect(fixture.nativeElement.textContent).toContain('01 Sep 2026, 09:00 UTC');
  });

  it('degrades failed enrichment to Unavailable without failing results', () => {
    searchAppointments.mockReturnValue(of({
      items: [appointment], page: 1, pageSize: 20, totalCount: 1, totalPages: 1,
    }));
    getReferralPatientSummary.mockReturnValue(throwError(() => new Error('lookup failed')));
    getReferral.mockReturnValue(throwError(() => new Error('lookup failed')));

    fixture.componentInstance.retry();
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('table')).not.toBeNull();
    expect(fixture.nativeElement.textContent.match(/Unavailable/g)?.length).toBe(2);
  });

  it('converts filter values through the UTC convention and preserves numeric zero enums', () => {
    searchAppointments.mockReturnValue(of({ items: [], page: 1, pageSize: 20, totalCount: 0, totalPages: 0 }));
    fixture.componentInstance.filters.patchValue({
      status: APPOINTMENT_STATUSES.scheduled,
      appointmentType: APPOINTMENT_TYPES.consultation,
      location: ' Clinic A ',
      scheduledFrom: '2026-09-01T09:00',
      scheduledTo: '2026-09-01T10:00',
      sortBy: 'status',
      sortDirection: 'desc',
    });

    fixture.componentInstance.applyFilters();

    expect(searchAppointments).toHaveBeenLastCalledWith({
      status: 0,
      appointmentType: 0,
      location: 'Clinic A',
      scheduledFrom: '2026-09-01T09:00:00.000Z',
      scheduledTo: '2026-09-01T10:00:00.000Z',
      page: 1,
      pageSize: 20,
      sortBy: 'status',
      sortDirection: 'desc',
    });
  });

  it('rejects an inverted UTC range and supersedes a prior request on a valid change', () => {
    fixture.componentInstance.filters.patchValue({
      scheduledFrom: '2026-09-01T10:00',
      scheduledTo: '2026-09-01T09:00',
    });
    fixture.componentInstance.applyFilters();
    expect(searchAppointments).toHaveBeenCalledOnce();
    expect(fixture.componentInstance.error()).toBe('validation');

    searchAppointments.mockReturnValueOnce(of({ items: [], page: 1, pageSize: 20, totalCount: 0, totalPages: 0 }));
    fixture.componentInstance.filters.patchValue({
      scheduledFrom: '2026-09-01T09:00',
      scheduledTo: '2026-09-01T10:00',
    });
    fixture.componentInstance.applyFilters();
    expect(response$.observed).toBe(false);
  });

  it('shows a capability state for 403 and a retry for generic failures', () => {
    response$.error(new HttpErrorResponse({ status: 403 }));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Appointments are not available for your role');

    searchAppointments.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 500 })));
    fixture.componentInstance.retry();
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Appointments could not be loaded');
    expect(fixture.nativeElement.textContent).toContain('Try again');
  });
});
