import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Subject, of, throwError } from 'rxjs';

import { AuthLoadStatus, CARETRACK_ROLES, CareTrackRole } from '../../../core/auth/auth.models';
import { AuthService } from '../../../core/auth/auth.service';
import { PagedResult } from '../../../shared/models/paged-result.model';
import { AppointmentApiService } from '../../appointments/data-access/appointment-api.service';
import {
  APPOINTMENT_STATUSES,
  APPOINTMENT_TYPES,
  AppointmentSearchItem,
  AppointmentSearchQuery,
} from '../../appointments/models/appointment.models';
import { PatientApiService } from '../../patients/data-access/patient-api.service';
import { Patient, PatientSearchQuery } from '../../patients/models/patient.models';
import { ReferralApiService } from '../../referrals/data-access/referral-api.service';
import {
  REFERRAL_PRIORITIES,
  REFERRAL_STATUSES,
  Referral,
  ReferralSearchQuery,
} from '../../referrals/models/referral.models';
import { DashboardPage } from './dashboard-page';

const patient: Patient = {
  id: '11111111-1111-1111-1111-111111111111',
  patientReference: 'PAT-001',
  firstName: 'Amina',
  lastName: 'Khan',
  fullName: 'Amina Khan',
  dateOfBirth: '1988-04-12',
  createdAt: '2026-08-01T09:00:00Z',
  rowVersion: 'AAAAAAAAB9E=',
};

const referral: Referral = {
  id: '22222222-2222-2222-2222-222222222222',
  referralReference: 'REF-ATTENTION-001',
  patientId: patient.id,
  status: REFERRAL_STATUSES.awaitingTriage,
  priority: REFERRAL_PRIORITIES.urgent,
  reason: 'Specialist assessment required.',
  triageNote: null,
  createdAt: '2026-08-20T10:00:00Z',
  submittedAt: '2026-08-20T11:00:00Z',
  updatedAt: null,
  triagedAt: null,
  assignedTo: null,
  assignedAt: null,
};

const appointment: AppointmentSearchItem = {
  id: '33333333-3333-3333-3333-333333333333',
  appointmentReference: 'APT-UPCOMING-001',
  patientId: patient.id,
  referralId: referral.id,
  appointmentType: APPOINTMENT_TYPES.consultation,
  scheduledStart: '2026-08-27T09:00:00Z',
  scheduledEnd: '2026-08-27T09:30:00Z',
  location: 'Clinic A',
  status: APPOINTMENT_STATUSES.scheduled,
  createdAt: '2026-08-20T12:00:00Z',
};

function page<T>(items: readonly T[], totalCount = items.length, pageSize = 5): PagedResult<T> {
  return {
    items,
    page: 1,
    pageSize,
    totalCount,
    totalPages: totalCount === 0 ? 0 : Math.ceil(totalCount / pageSize),
  };
}

describe('DashboardPage', () => {
  let fixture: ComponentFixture<DashboardPage>;
  const authStatus = signal<AuthLoadStatus>('ready');
  const roles = signal<readonly string[]>([CARETRACK_ROLES.clinician]);
  const currentUser = signal({
    id: '55555555-5555-5555-5555-555555555555',
    name: 'Amina Khan',
    username: 'amina@example.test',
    roles: [CARETRACK_ROLES.clinician],
  });

  const searchPatients =
    vi.fn<(query: PatientSearchQuery) => ReturnType<PatientApiService['searchPatients']>>();
  const getReferralPatientSummary = vi.fn();
  const searchReferrals =
    vi.fn<(query: ReferralSearchQuery) => ReturnType<ReferralApiService['searchReferrals']>>();
  const getReferral = vi.fn();
  const searchAppointments =
    vi.fn<
      (query: AppointmentSearchQuery) => ReturnType<AppointmentApiService['searchAppointments']>
    >();

  const authServiceMock = {
    status: authStatus.asReadonly(),
    roles: roles.asReadonly(),
    currentUser: currentUser.asReadonly(),
    hasRole: (role: CareTrackRole) => roles().includes(role),
  };

  beforeEach(async () => {
    authStatus.set('ready');
    roles.set([CARETRACK_ROLES.clinician]);
    currentUser.set({
      id: '55555555-5555-5555-5555-555555555555',
      name: 'Amina Khan',
      username: 'amina@example.test',
      roles: [CARETRACK_ROLES.clinician],
    });
    searchPatients.mockReset().mockReturnValue(of(page([], 12, 1)));
    searchReferrals.mockReset().mockImplementation((query) => {
      const count = query.status === REFERRAL_STATUSES.awaitingTriage ? 4 : 2;
      return of(page([], count, query.pageSize));
    });
    searchAppointments.mockReset().mockImplementation((query) => {
      const count = query.status === APPOINTMENT_STATUSES.scheduled ? 3 : 1;
      return of(page([], count, query.pageSize));
    });
    getReferralPatientSummary.mockReset();
    getReferral.mockReset();

    await TestBed.configureTestingModule({
      imports: [DashboardPage],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: authServiceMock },
        {
          provide: PatientApiService,
          useValue: { searchPatients, getReferralPatientSummary },
        },
        { provide: ReferralApiService, useValue: { searchReferrals, getReferral } },
        { provide: AppointmentApiService, useValue: { searchAppointments } },
      ],
    }).compileComponents();
  });

  afterEach(() => vi.useRealTimers());

  function createDashboard(): DashboardPage {
    fixture = TestBed.createComponent(DashboardPage);
    fixture.detectChanges();
    return fixture.componentInstance;
  }

  function text(): string {
    return fixture.nativeElement.textContent.replace(/\s+/g, ' ').trim();
  }
  it('waits for ready authentication before loading role-appropriate data', () => {
    authStatus.set('loading');
    const component = createDashboard();

    expect(component.audience()).toBe('loading');
    expect(searchPatients).not.toHaveBeenCalled();
    expect(searchReferrals).not.toHaveBeenCalled();
    expect(searchAppointments).not.toHaveBeenCalled();

    authStatus.set('ready');
    fixture.detectChanges();

    expect(component.audience()).toBe('clinician');
    expect(searchPatients).toHaveBeenCalledTimes(1);
    expect(searchReferrals).toHaveBeenCalledTimes(2);
    expect(searchAppointments).toHaveBeenCalledTimes(2);
  });

  it('loads the exact clinician filters, minimal page sizes, and seven-day UTC window', () => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date('2026-08-26T10:15:30.000Z'));
    createDashboard();

    expect(searchPatients).toHaveBeenCalledWith({
      page: 1,
      pageSize: 1,
      sortBy: 'lastName',
      sortDirection: 'asc',
    });
    expect(searchReferrals.mock.calls.map(([query]) => query)).toEqual([
      {
        status: REFERRAL_STATUSES.awaitingTriage,
        page: 1,
        pageSize: 5,
        sortBy: 'priority',
        sortDirection: 'desc',
      },
      {
        status: REFERRAL_STATUSES.moreInformationRequired,
        page: 1,
        pageSize: 5,
        sortBy: 'priority',
        sortDirection: 'desc',
      },
    ]);
    expect(searchAppointments.mock.calls.map(([query]) => query)).toEqual([
      {
        status: APPOINTMENT_STATUSES.scheduled,
        scheduledFrom: '2026-08-26T10:15:30.000Z',
        scheduledTo: '2026-09-02T10:15:30.000Z',
        page: 1,
        pageSize: 5,
        sortBy: 'scheduledStart',
        sortDirection: 'asc',
      },
      {
        status: APPOINTMENT_STATUSES.inProgress,
        page: 1,
        pageSize: 5,
        sortBy: 'scheduledStart',
        sortDirection: 'asc',
      },
    ]);
    expect(getReferralPatientSummary).not.toHaveBeenCalled();
    expect(getReferral).not.toHaveBeenCalled();
  });

  it('uses the clinician audience for a user who also has coordinator access', () => {
    roles.set([CARETRACK_ROLES.referralCoordinator, CARETRACK_ROLES.clinician]);
    const component = createDashboard();

    expect(component.audience()).toBe('clinician');
    expect(searchPatients).toHaveBeenCalledTimes(1);
    expect(searchAppointments).toHaveBeenCalledTimes(2);
    expect(text()).toContain('Appointment Activity');
  });

  it('loads only four referral requests for a referral coordinator', () => {
    roles.set([CARETRACK_ROLES.referralCoordinator]);
    const component = createDashboard();

    expect(component.audience()).toBe('coordinator');
    expect(searchPatients).not.toHaveBeenCalled();
    expect(searchAppointments).not.toHaveBeenCalled();
    expect(searchReferrals).toHaveBeenCalledTimes(4);
    expect(searchReferrals.mock.calls.map(([query]) => [query.status, query.pageSize])).toEqual([
      [REFERRAL_STATUSES.awaitingTriage, 5],
      [REFERRAL_STATUSES.moreInformationRequired, 5],
      [REFERRAL_STATUSES.accepted, 1],
      [REFERRAL_STATUSES.assigned, 1],
    ]);
    expect(text()).toContain('Accepted');
    expect(text()).toContain('Assigned');
    expect(text()).toContain('Register patient');
    expect(text()).toContain('Create referral');
    expect(text()).toContain('View referrals');
    expect(text()).not.toContain('Appointment Activity');
    expect(getReferralPatientSummary).not.toHaveBeenCalled();
    expect(getReferral).not.toHaveBeenCalled();
  });

  it('renders an administrator workspace without clinical requests', () => {
    roles.set([CARETRACK_ROLES.administrator]);
    const component = createDashboard();

    expect(component.audience()).toBe('administrator');
    expect(searchPatients).not.toHaveBeenCalled();
    expect(searchReferrals).not.toHaveBeenCalled();
    expect(searchAppointments).not.toHaveBeenCalled();
    expect(text()).toContain('Administrator Workspace');
    expect(text()).not.toContain('Operational Summary');
  });
  it('renders total counts, operational rows, status labels, and detail links', () => {
    searchPatients.mockReturnValue(of(page([patient], 27, 1)));
    searchReferrals.mockImplementation((query) => {
      if (query.status === REFERRAL_STATUSES.awaitingTriage) {
        return of(page([referral], 8, 5));
      }
      return of(
        page(
          [
            {
              ...referral,
              id: '44444444-4444-4444-4444-444444444444',
              referralReference: 'REF-MORE-001',
              status: REFERRAL_STATUSES.moreInformationRequired,
            },
          ],
          3,
          5,
        ),
      );
    });
    searchAppointments.mockImplementation((query) => {
      const item = { ...appointment, status: query.status ?? APPOINTMENT_STATUSES.scheduled };
      return of(page([item], query.status === APPOINTMENT_STATUSES.scheduled ? 6 : 2, 5));
    });

    createDashboard();
    const content = text();

    expect(content).toContain('27');
    expect(content).toContain('REF-ATTENTION-001');
    expect(content).toContain('REF-MORE-001');
    expect(content).toContain('Urgent');
    expect(content).toContain('APT-UPCOMING-001');
    expect(content).toContain('Consultation');
    expect(content).toContain('UTC');
    expect(fixture.nativeElement.querySelector(`a[href='/referrals/${referral.id}']`)).toBeTruthy();
    expect(
      fixture.nativeElement.querySelector(`a[href='/appointments/${appointment.id}']`),
    ).toBeTruthy();
  });

  it('distinguishes a successful zero queue from an unavailable queue', () => {
    searchReferrals.mockImplementation((query) =>
      query.status === REFERRAL_STATUSES.awaitingTriage
        ? of(page([], 0, 5))
        : throwError(() => new Error('referral failure')),
    );
    createDashboard();

    expect(text()).toContain('No referrals awaiting triage');
    expect(text()).toContain('More-information queue unavailable');
    expect(text()).toContain('Some summary counts are unavailable');
  });
  it('keeps appointment data visible when one referral request fails', () => {
    searchReferrals.mockImplementation((query) =>
      query.status === REFERRAL_STATUSES.awaitingTriage
        ? throwError(() => new Error('referral failure'))
        : of(page([], 0, query.pageSize)),
    );
    searchAppointments.mockImplementation((query) =>
      of(page([{ ...appointment, status: query.status ?? appointment.status }], 1, 5)),
    );
    createDashboard();

    expect(text()).toContain('Awaiting-triage queue unavailable');
    expect(text()).toContain('Appointment Activity');
    expect(text()).toContain('APT-UPCOMING-001');
  });

  it('uses one section-level message when both referral queues fail', () => {
    searchReferrals.mockReturnValue(throwError(() => new Error('referral failure')));
    createDashboard();

    expect(text()).toContain('Referral workload is unavailable');
    expect(text()).not.toContain('Awaiting-triage queue unavailable');
    expect(text()).toContain('Appointment Activity');
  });

  it('keeps other sections usable when the patient count fails', () => {
    searchPatients.mockReturnValue(throwError(() => new Error('patient failure')));
    searchReferrals.mockImplementation((query) => of(page([referral], 1, query.pageSize)));
    searchAppointments.mockImplementation((query) =>
      of(page([{ ...appointment, status: query.status ?? appointment.status }], 1, 5)),
    );
    createDashboard();

    expect(
      fixture.nativeElement.querySelector(`[aria-label='Patients count unavailable']`),
    ).toBeTruthy();
    expect(text()).toContain('REF-ATTENTION-001');
    expect(text()).toContain('APT-UPCOMING-001');
  });
  it('prevents overlapping page-level refresh batches', () => {
    const pending = new Subject<PagedResult<never>>();
    searchPatients.mockReturnValue(pending);
    searchReferrals.mockReturnValue(pending);
    searchAppointments.mockReturnValue(pending);
    const component = createDashboard();

    expect(component.batchLoading()).toBe(true);
    component.refresh();
    component.refresh();
    expect(searchPatients).toHaveBeenCalledTimes(1);
    expect(searchReferrals).toHaveBeenCalledTimes(2);
    expect(searchAppointments).toHaveBeenCalledTimes(2);

    pending.complete();
    expect(component.batchLoading()).toBe(false);
    component.refresh();
    expect(searchPatients).toHaveBeenCalledTimes(2);
    expect(searchReferrals).toHaveBeenCalledTimes(4);
    expect(searchAppointments).toHaveBeenCalledTimes(4);
  });

  it('cancels stale requests and clears privileged data immediately after a role downgrade', () => {
    const patientsPending = new Subject<PagedResult<Patient>>();
    const referralsPending = new Subject<PagedResult<Referral>>();
    const appointmentsPending = new Subject<PagedResult<AppointmentSearchItem>>();
    searchPatients.mockReturnValue(patientsPending);
    searchReferrals.mockReturnValue(referralsPending);
    searchAppointments.mockReturnValue(appointmentsPending);
    const component = createDashboard();

    patientsPending.next(page([patient], 99, 1));
    referralsPending.next(page([referral], 1, 5));
    appointmentsPending.next(page([appointment], 1, 5));
    fixture.detectChanges();
    expect(text()).toContain('REF-ATTENTION-001');
    expect(text()).toContain('APT-UPCOMING-001');

    roles.set([CARETRACK_ROLES.administrator]);
    fixture.detectChanges();

    expect(component.audience()).toBe('administrator');
    expect(component.patientCount()).toEqual({ status: 'idle', data: null });
    expect(component.awaitingTriage()).toEqual({ status: 'idle', data: null });
    expect(component.upcomingAppointments()).toEqual({ status: 'idle', data: null });
    expect(component.batchLoading()).toBe(false);
    expect(patientsPending.observed).toBe(false);
    expect(referralsPending.observed).toBe(false);
    expect(appointmentsPending.observed).toBe(false);
    expect(text()).toContain('Administrator Workspace');
    expect(text()).not.toContain('REF-ATTENTION-001');
    expect(text()).not.toContain('APT-UPCOMING-001');

    appointmentsPending.next(page([appointment], 50, 5));
    expect(component.upcomingAppointments()).toEqual({ status: 'idle', data: null });
  });

  it('uses only the trimmed current-user name in the local-time greeting', () => {
    currentUser.update((user) => ({ ...user, name: '  Amina Khan  ' }));
    const component = createDashboard();

    expect(component.displayName()).toBe('Amina Khan');
    expect(component.welcomeTitle()).toMatch(/^Good (Morning|Afternoon|Evening), Amina Khan$/);
  });

  it('omits punctuation when the current-user name is blank', () => {
    currentUser.update((user) => ({
      ...user,
      name: '   ',
      username: 'must-not-be-used@example.test',
    }));
    const component = createDashboard();

    expect(component.displayName()).toBe('');
    expect(component.welcomeTitle()).toMatch(/^Good (Morning|Afternoon|Evening)$/);
    expect(component.welcomeTitle()).not.toContain(',');
    expect(component.welcomeTitle()).not.toContain('must-not-be-used');
  });

  it('issues exactly five, four, or zero dashboard requests by audience', () => {
    createDashboard();
    expect(
      searchPatients.mock.calls.length +
        searchReferrals.mock.calls.length +
        searchAppointments.mock.calls.length,
    ).toBe(5);

    fixture.destroy();
    searchPatients.mockClear();
    searchReferrals.mockClear();
    searchAppointments.mockClear();
    roles.set([CARETRACK_ROLES.referralCoordinator]);
    createDashboard();
    expect(
      searchPatients.mock.calls.length +
        searchReferrals.mock.calls.length +
        searchAppointments.mock.calls.length,
    ).toBe(4);

    fixture.destroy();
    searchPatients.mockClear();
    searchReferrals.mockClear();
    searchAppointments.mockClear();
    roles.set([CARETRACK_ROLES.administrator]);
    createDashboard();
    expect(
      searchPatients.mock.calls.length +
        searchReferrals.mock.calls.length +
        searchAppointments.mock.calls.length,
    ).toBe(0);
    expect(text()).not.toContain('Register patient');
    expect(text()).not.toContain('Create referral');
  });
});
