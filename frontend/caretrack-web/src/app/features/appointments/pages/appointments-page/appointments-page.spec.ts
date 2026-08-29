import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Subject, of, throwError } from 'rxjs';

import { PagedResult } from '../../../../shared/models/paged-result.model';
import { PatientApiService } from '../../../patients/data-access/patient-api.service';
import { ReferralApiService } from '../../../referrals/data-access/referral-api.service';
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
    patientReference: 'PAT-001',
    patientDisplayName: 'Amina Khan',
    referralId,
    referralReference: 'REF-001',
    appointmentType: APPOINTMENT_TYPES.consultation,
    scheduledStart: '2026-09-01T09:00:00',
    scheduledEnd: '2026-09-01T09:30:00',
    location: 'Clinic A',
    status: APPOINTMENT_STATUSES.scheduled,
    createdAt: '2026-08-25T10:00:00Z',
  };
  beforeEach(async () => {
    response$ = new Subject<PagedResult<AppointmentSearchItem>>();
    searchAppointments.mockReset().mockReturnValue(response$);
    getReferralPatientSummary.mockReset();
    getReferral.mockReset();

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
    expect(
      fixture.nativeElement.querySelector('[aria-label="Loading appointments"]'),
    ).not.toBeNull();
  });

  it('renders operational identity fields without patient or referral enrichment calls', () => {
    response$.next({
      items: [
        {
          ...appointment,
          reason: 'SENSITIVE-REASON',
          triageNote: 'SENSITIVE-TRIAGE',
          clinicalNote: 'SENSITIVE-CLINICAL',
        } as AppointmentSearchItem,
        { ...appointment, id: '44444444-4444-4444-4444-444444444444' },
      ],
      page: 1,
      pageSize: 20,
      totalCount: 2,
      totalPages: 1,
    });
    fixture.detectChanges();

    expect(searchAppointments).toHaveBeenCalledOnce();
    expect(getReferralPatientSummary).not.toHaveBeenCalled();
    expect(getReferral).not.toHaveBeenCalled();
    expect(fixture.nativeElement.textContent).toContain('Amina Khan');
    expect(fixture.nativeElement.textContent).toContain('PAT-001');
    expect(fixture.nativeElement.textContent).toContain('REF-001');
    expect(fixture.nativeElement.textContent).toContain('01 Sep 2026, 09:00 UTC');
    expect(fixture.nativeElement.textContent).not.toContain('SENSITIVE-REASON');
    expect(fixture.nativeElement.textContent).not.toContain('SENSITIVE-TRIAGE');
    expect(fixture.nativeElement.textContent).not.toContain('SENSITIVE-CLINICAL');
  });

  it('requests the selected page through the appointment API only', () => {
    searchAppointments.mockReturnValue(
      of({ items: [], page: 2, pageSize: 20, totalCount: 25, totalPages: 2 }),
    );

    fixture.componentInstance.changePage(2);

    expect(searchAppointments).toHaveBeenLastCalledWith(
      expect.objectContaining({ page: 2, pageSize: 20 }),
    );
    expect(getReferralPatientSummary).not.toHaveBeenCalled();
    expect(getReferral).not.toHaveBeenCalled();
  });

  it('renders an empty state from the appointment response without follow-up calls', () => {
    searchAppointments.mockReturnValue(
      of({
        items: [],
        page: 1,
        pageSize: 20,
        totalCount: 0,
        totalPages: 0,
      }),
    );

    fixture.componentInstance.retry();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('No appointments yet');
    expect(getReferralPatientSummary).not.toHaveBeenCalled();
    expect(getReferral).not.toHaveBeenCalled();
  });

  it('converts filter values through the UTC convention and preserves numeric zero enums', () => {
    searchAppointments.mockReturnValue(
      of({ items: [], page: 1, pageSize: 20, totalCount: 0, totalPages: 0 }),
    );
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

    searchAppointments.mockReturnValueOnce(
      of({ items: [], page: 1, pageSize: 20, totalCount: 0, totalPages: 0 }),
    );
    fixture.componentInstance.filters.patchValue({
      scheduledFrom: '2026-09-01T09:00',
      scheduledTo: '2026-09-01T10:00',
    });
    fixture.componentInstance.applyFilters();
    expect(response$.observed).toBe(false);
  });

  it('toggles the mobile appointment filters from the filter button', () => {
    fixture.detectChanges();
    const toggle = fixture.nativeElement.querySelector(
      '.appointments-page__filter-toggle',
    ) as HTMLButtonElement;
    const filters = fixture.nativeElement.querySelector(
      '.appointments-page__filters',
    ) as HTMLElement;

    expect(toggle.getAttribute('aria-expanded')).toBe('false');
    expect(filters.classList.contains('appointments-page__filters--expanded')).toBe(false);

    toggle.click();
    fixture.detectChanges();

    expect(toggle.getAttribute('aria-expanded')).toBe('true');
    expect(filters.classList.contains('appointments-page__filters--expanded')).toBe(true);
  });

  it('shows a capability state for 403 and a retry for generic failures', () => {
    response$.error(new HttpErrorResponse({ status: 403 }));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain(
      'Appointments are not available for your role',
    );

    searchAppointments.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 500 })));
    fixture.componentInstance.retry();
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Appointments could not be loaded');
    expect(fixture.nativeElement.textContent).toContain('Try again');
  });
});
