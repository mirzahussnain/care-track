import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, Subject, throwError } from 'rxjs';

import { CARETRACK_ROLES, CareTrackRole } from '../../../../core/auth/auth.models';
import { AuthService } from '../../../../core/auth/auth.service';
import { PagedResult } from '../../../../shared/models/paged-result.model';
import { ReferralApiService } from '../../data-access/referral-api.service';
import { REFERRAL_PRIORITIES, REFERRAL_STATUSES, Referral } from '../../models/referral.models';
import { ReferralsPage } from './referrals-page';

describe('ReferralsPage', () => {
  let fixture: ComponentFixture<ReferralsPage>;
  let response$: Subject<PagedResult<Referral>>;
  let roles: readonly CareTrackRole[];
  const searchReferrals = vi.fn();
  const getAssignmentTargets = vi.fn();
  const referral: Referral = {
    id: '22222222-2222-2222-2222-222222222222',
    referralReference: 'REF-001',
    patientId: '11111111-1111-1111-1111-111111111111',
    status: REFERRAL_STATUSES.assigned,
    priority: REFERRAL_PRIORITIES.urgent,
    reason: 'Specialist review',
    triageNote: 'Urgent review',
    createdAt: '2026-08-25T10:00:00Z',
    submittedAt: '2026-08-25T10:05:00Z',
    updatedAt: '2026-08-25T10:10:00Z',
    triagedAt: '2026-08-25T10:08:00Z',
    assignedTo: 'Cardiology Team A',
    assignedAt: '2026-08-25T10:10:00Z',
  };
  const result: PagedResult<Referral> = {
    items: [referral], page: 1, pageSize: 20, totalCount: 1, totalPages: 1,
  };

  beforeEach(async () => {
    roles = [CARETRACK_ROLES.clinician];
    response$ = new Subject<PagedResult<Referral>>();
    searchReferrals.mockReset().mockReturnValue(response$);
    getAssignmentTargets.mockReset().mockReturnValue(of({ items: ['Cardiology Team A'] }));
    await TestBed.configureTestingModule({
      imports: [ReferralsPage],
      providers: [
        provideRouter([]),
        { provide: ReferralApiService, useValue: { searchReferrals, getAssignmentTargets } },
        {
          provide: AuthService,
          useValue: { hasRole: (role: CareTrackRole) => roles.includes(role) },
        },
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(ReferralsPage);
  });

  it('loads the referral queue with fixed server paging and sorting', () => {
    fixture.detectChanges();

    expect(searchReferrals).toHaveBeenCalledWith({
      status: undefined,
      priority: undefined,
      assignedTo: undefined,
      createdFrom: undefined,
      createdTo: undefined,
      page: 1,
      pageSize: 20,
      sortBy: 'createdAt',
      sortDirection: 'desc',
    });
    expect(fixture.nativeElement.querySelector('[aria-label="Loading referrals"]')).not.toBeNull();
  });

  it('renders the operational fields in a semantic table', () => {
    response$.next(result);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('table')).not.toBeNull();
    expect(fixture.nativeElement.textContent).toContain('REF-001');
    expect(fixture.nativeElement.textContent).toContain('Urgent');
    expect(fixture.nativeElement.textContent).toContain('Assigned');
    expect(fixture.nativeElement.textContent).toContain('Cardiology Team A');
  });

  it('preserves numeric zero filters and sends configured team/date values', () => {
    searchReferrals.mockReturnValue(of(result));
    fixture.componentInstance.filters.patchValue({
      status: REFERRAL_STATUSES.draft,
      priority: REFERRAL_PRIORITIES.routine,
      assignedTo: 'Cardiology Team A',
      createdFrom: '2026-08-01',
      createdTo: '2026-08-31',
      sortBy: 'priority',
      sortDirection: 'asc',
    });

    fixture.componentInstance.applyFilters();

    expect(searchReferrals).toHaveBeenLastCalledWith({
      status: 0,
      priority: 0,
      assignedTo: 'Cardiology Team A',
      createdFrom: '2026-08-01',
      createdTo: '2026-08-31',
      page: 1,
      pageSize: 20,
      sortBy: 'priority',
      sortDirection: 'asc',
    });
  });

  it('rejects an inverted date range without issuing a request', () => {
    fixture.componentInstance.filters.patchValue({
      createdFrom: '2026-09-01',
      createdTo: '2026-08-01',
    });

    fixture.componentInstance.applyFilters();

    expect(searchReferrals).toHaveBeenCalledOnce();
    expect(fixture.componentInstance.error()).toBe('validation');
  });

  it('supersedes the previous list request when filters are applied', () => {
    searchReferrals.mockReturnValueOnce(of(result));
    fixture.componentInstance.applyFilters();

    expect(response$.observed).toBe(false);
  });

  it('treats 403 as a capability state and hides creation from Administrator', () => {
    roles = [CARETRACK_ROLES.administrator];
    response$.error(new HttpErrorResponse({ status: 403 }));
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain(
      'Referral management is not available for your role',
    );
    expect(fixture.nativeElement.textContent).not.toContain('Create referral');
  });

  it('shows a retryable generic error independently of assignment-target loading', () => {
    searchReferrals.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 500 })));
    fixture.componentInstance.retry();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Referrals could not be loaded');
    expect(fixture.nativeElement.textContent).toContain('Try again');
  });
});
