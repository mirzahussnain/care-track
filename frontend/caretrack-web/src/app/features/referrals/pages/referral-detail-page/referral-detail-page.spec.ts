import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of, Subject, throwError } from 'rxjs';

import { CARETRACK_ROLES, CareTrackRole } from '../../../../core/auth/auth.models';
import { AuthService } from '../../../../core/auth/auth.service';
import { PatientApiService } from '../../../patients/data-access/patient-api.service';
import { ReferralApiService } from '../../data-access/referral-api.service';
import {
  REFERRAL_HISTORY_EVENT_TYPES,
  REFERRAL_PRIORITIES,
  REFERRAL_STATUSES,
  Referral,
  ReferralHistoryEntry,
} from '../../models/referral.models';
import { ReferralDetailPage } from './referral-detail-page';

describe('ReferralDetailPage', () => {
  let fixture: ComponentFixture<ReferralDetailPage>;
  let roles: readonly CareTrackRole[];
  const referralId = '22222222-2222-2222-2222-222222222222';
  const patientId = '11111111-1111-1111-1111-111111111111';
  const getReferral = vi.fn();
  const getHistory = vi.fn();
  const getAssignmentTargets = vi.fn();
  const submitReferral = vi.fn();
  const startTriage = vi.fn();
  const requestMoreInformation = vi.fn();
  const resubmitReferral = vi.fn();
  const acceptReferral = vi.fn();
  const rejectReferral = vi.fn();
  const recordTriageAssessment = vi.fn();
  const assignReferral = vi.fn();
  const reassignReferral = vi.fn();
  const completeReferral = vi.fn();
  const getReferralPatientSummary = vi.fn();

  const referral: Referral = {
    id: referralId,
    referralReference: 'REF-001',
    patientId,
    status: REFERRAL_STATUSES.draft,
    priority: REFERRAL_PRIORITIES.routine,
    reason: 'Specialist review',
    triageNote: null,
    createdAt: '2026-08-25T10:00:00Z',
    submittedAt: null,
    updatedAt: null,
    triagedAt: null,
    assignedTo: null,
    assignedAt: null,
  };
  const history: readonly ReferralHistoryEntry[] = [
    {
      id: '33333333-3333-3333-3333-333333333333',
      eventType: REFERRAL_HISTORY_EVENT_TYPES.created,
      fromStatus: null,
      toStatus: REFERRAL_STATUSES.draft,
      priority: REFERRAL_PRIORITIES.routine,
      triageNote: null,
      assignedTo: null,
      occurredAt: '2026-08-25T10:00:00Z',
    },
  ];

  beforeEach(async () => {
    roles = [CARETRACK_ROLES.clinician];
    for (const mock of [
      getReferral,
      getHistory,
      getAssignmentTargets,
      submitReferral,
      startTriage,
      requestMoreInformation,
      resubmitReferral,
      acceptReferral,
      rejectReferral,
      recordTriageAssessment,
      assignReferral,
      reassignReferral,
      completeReferral,
      getReferralPatientSummary,
    ]) {
      mock.mockReset();
    }
    getReferral.mockReturnValue(of(referral));
    getHistory.mockReturnValue(of(history));
    getAssignmentTargets.mockReturnValue(of({ items: ['Cardiology Team A'] }));
    getReferralPatientSummary.mockReturnValue(
      of({
        id: patientId,
        patientReference: 'PAT-001',
        fullName: 'Amina Khan',
        dateOfBirth: '1988-04-12',
      }),
    );
    await TestBed.configureTestingModule({
      imports: [ReferralDetailPage],
      providers: [
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: convertToParamMap({ id: referralId }) } },
        },
        {
          provide: ReferralApiService,
          useValue: {
            getReferral,
            getHistory,
            getAssignmentTargets,
            submitReferral,
            startTriage,
            requestMoreInformation,
            resubmitReferral,
            acceptReferral,
            rejectReferral,
            recordTriageAssessment,
            assignReferral,
            reassignReferral,
            completeReferral,
          },
        },
        { provide: PatientApiService, useValue: { getReferralPatientSummary } },
        {
          provide: AuthService,
          useValue: { hasRole: (role: CareTrackRole) => roles.includes(role) },
        },
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(ReferralDetailPage);
  });

  it('loads the reduced patient identity, history, and configured assignment targets', () => {
    fixture.detectChanges();

    expect(getReferralPatientSummary).toHaveBeenCalledWith(patientId);
    expect(getHistory).toHaveBeenCalledWith(referralId);
    expect(getAssignmentTargets).toHaveBeenCalledOnce();
    expect(fixture.nativeElement.textContent).toContain('Amina Khan');
    expect(fixture.nativeElement.textContent).toContain('PAT-001');
    expect(fixture.nativeElement.textContent).toContain('Referral created');
    expect(fixture.nativeElement.textContent).toContain('Submit referral');
  });

  it('hides all workflow actions from Administrator without adding a bypass', () => {
    roles = [CARETRACK_ROLES.administrator];
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('#workflow-actions-title')).toBeNull();
    expect(fixture.nativeElement.textContent).not.toContain('Submit referral');
  });

  it('hands eligible referrals to the Appointment create route without GUID entry', () => {
    fixture.componentInstance.referral.set({ ...referral, status: REFERRAL_STATUSES.assigned });
    fixture.detectChanges();

    const link = [...fixture.nativeElement.querySelectorAll('a')].find(
      (candidate: HTMLAnchorElement) => candidate.textContent?.includes('Schedule appointment'),
    ) as HTMLAnchorElement | undefined;

    expect(link).toBeDefined();
    expect(link?.getAttribute('href')).toContain('/appointments/new');
    expect(link?.getAttribute('href')).toContain(`referralId=${referralId}`);
    expect(fixture.componentInstance.canScheduleAppointment(REFERRAL_STATUSES.scheduled)).toBe(true);
    expect(fixture.componentInstance.canScheduleAppointment(REFERRAL_STATUSES.inProgress)).toBe(true);
    expect(fixture.componentInstance.canScheduleAppointment(REFERRAL_STATUSES.completed)).toBe(false);
  });

  it('shows only awaiting-triage decisions and sends a numeric triage assessment', () => {
    const assessed = {
      ...referral,
      status: REFERRAL_STATUSES.awaitingTriage,
      priority: REFERRAL_PRIORITIES.urgent,
      triageNote: 'Review promptly',
    };
    recordTriageAssessment.mockReturnValue(of(assessed));
    fixture.componentInstance.referral.set({ ...referral, status: REFERRAL_STATUSES.awaitingTriage });
    fixture.componentInstance.triageForm.setValue({
      priority: REFERRAL_PRIORITIES.urgent,
      note: ' Review promptly ',
    });
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Accept referral');
    expect(fixture.nativeElement.textContent).toContain('Request information');
    expect(fixture.nativeElement.textContent).not.toContain('Assign clinical team');
    fixture.componentInstance.recordTriageAssessment();

    expect(recordTriageAssessment).toHaveBeenCalledWith(referralId, {
      priority: 1,
      note: 'Review promptly',
    });
  });

  it('assigns only a selected configured clinical-team name', () => {
    const assigned = {
      ...referral,
      status: REFERRAL_STATUSES.assigned,
      assignedTo: 'Cardiology Team A',
    };
    assignReferral.mockReturnValue(of(assigned));
    fixture.componentInstance.referral.set({ ...referral, status: REFERRAL_STATUSES.accepted });
    fixture.componentInstance.assignmentForm.setValue({ assignedTo: 'Cardiology Team A' });

    fixture.componentInstance.assignReferral();

    expect(assignReferral).toHaveBeenCalledWith(referralId, {
      assignedTo: 'Cardiology Team A',
    });
  });

  it('distinguishes stale-state and workflow 409 titles and preserves the loaded detail', () => {
    const stale$ = new Subject<Referral>();
    submitReferral.mockReturnValue(stale$);
    fixture.componentInstance.submitReferral();
    stale$.error(
      new HttpErrorResponse({
        status: 409,
        error: { title: 'Concurrency Conflict', detail: 'The referral changed.' },
      }),
    );
    fixture.detectChanges();

    expect(fixture.componentInstance.commandError()?.kind).toBe('concurrency');
    expect(fixture.componentInstance.referral()?.id).toBe(referralId);

    startTriage.mockReturnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 409,
            error: { title: 'Invalid State Transition', detail: 'Cannot start triage.' },
          }),
      ),
    );
    fixture.componentInstance.startTriage();

    expect(fixture.componentInstance.commandError()?.kind).toBe('workflow');
  });

  it('refetches the referral and history after successful completion', () => {
    completeReferral.mockReturnValue(of(void 0));
    fixture.componentInstance.referral.set({ ...referral, status: REFERRAL_STATUSES.inProgress });
    getReferral.mockClear().mockReturnValue(of({ ...referral, status: REFERRAL_STATUSES.completed }));
    getHistory.mockClear().mockReturnValue(of(history));

    fixture.componentInstance.completeReferral();

    expect(completeReferral).toHaveBeenCalledWith(referralId);
    expect(getReferral).toHaveBeenCalledWith(referralId);
    expect(getHistory).toHaveBeenCalledWith(referralId);
    expect(fixture.componentInstance.referral()?.status).toBe(REFERRAL_STATUSES.completed);
  });
});
