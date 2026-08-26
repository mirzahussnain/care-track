import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../../../environments/environment';
import {
  REFERRAL_PRIORITIES,
  REFERRAL_STATUSES,
  Referral,
} from '../models/referral.models';
import { ReferralApiService } from './referral-api.service';

describe('ReferralApiService', () => {
  let service: ReferralApiService;
  let http: HttpTestingController;
  const baseUrl = `${environment.apiBaseUrl}/api/referrals`;
  const referral: Referral = {
    id: '11111111-1111-1111-1111-111111111111',
    referralReference: 'REF-001',
    patientId: '22222222-2222-2222-2222-222222222222',
    status: REFERRAL_STATUSES.draft,
    priority: REFERRAL_PRIORITIES.routine,
    reason: 'Specialist assessment required.',
    triageNote: null,
    createdAt: '2026-08-25T10:00:00Z',
    submittedAt: null,
    updatedAt: null,
    triagedAt: null,
    assignedTo: null,
    assignedAt: null,
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ReferralApiService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(ReferralApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('searches with exact numeric filters, dates, sorting, and paging', () => {
    service
      .searchReferrals({
        status: REFERRAL_STATUSES.draft,
        priority: REFERRAL_PRIORITIES.routine,
        assignedTo: ' Cardiology Team A ',
        createdFrom: '2026-08-01',
        createdTo: '2026-08-25',
        page: 2,
        pageSize: 20,
        sortBy: 'createdAt',
        sortDirection: 'desc',
      })
      .subscribe();

    const request = http.expectOne((candidate) => candidate.url === baseUrl);
    expect(request.request.method).toBe('GET');
    expect(request.request.params.get('status')).toBe('0');
    expect(request.request.params.get('priority')).toBe('0');
    expect(request.request.params.get('assignedTo')).toBe('Cardiology Team A');
    expect(request.request.params.get('createdFrom')).toBe('2026-08-01');
    expect(request.request.params.get('createdTo')).toBe('2026-08-25');
    expect(request.request.params.get('page')).toBe('2');
    expect(request.request.params.get('pageSize')).toBe('20');
    expect(request.request.params.get('sortBy')).toBe('createdAt');
    expect(request.request.params.get('sortDirection')).toBe('desc');
    request.flush({ items: [], page: 2, pageSize: 20, totalCount: 0, totalPages: 0 });
  });

  it('omits optional search filters when not applied', () => {
    service
      .searchReferrals({
        page: 1,
        pageSize: 20,
        sortBy: 'createdAt',
        sortDirection: 'desc',
      })
      .subscribe();

    const request = http.expectOne((candidate) => candidate.url === baseUrl);
    expect(request.request.params.has('status')).toBe(false);
    expect(request.request.params.has('priority')).toBe(false);
    expect(request.request.params.has('assignedTo')).toBe(false);
    request.flush({ items: [], page: 1, pageSize: 20, totalCount: 0, totalPages: 0 });
  });

  it('gets referral detail and history from their exact routes', () => {
    service.getReferral(referral.id).subscribe();
    const detail = http.expectOne(`${baseUrl}/${referral.id}`);
    expect(detail.request.method).toBe('GET');
    detail.flush(referral);

    service.getHistory(referral.id).subscribe();
    const history = http.expectOne(`${baseUrl}/${referral.id}/history`);
    expect(history.request.method).toBe('GET');
    history.flush([]);
  });

  it('creates with the exact request body', () => {
    const body = {
      referralReference: 'REF-001',
      patientId: referral.patientId,
      priority: REFERRAL_PRIORITIES.routine,
      reason: referral.reason,
    };
    service.createReferral(body).subscribe();
    const request = http.expectOne(baseUrl);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(body);
    request.flush(referral);
  });

  it.each([
    ['submitReferral', 'submit'],
    ['startTriage', 'start-triage'],
    ['requestMoreInformation', 'request-more-information'],
    ['acceptReferral', 'accept'],
    ['rejectReferral', 'reject'],
    ['resubmitReferral', 'resubmit'],
  ] as const)('posts a null body for %s', (method, route) => {
    service[method](referral.id).subscribe();
    const request = http.expectOne(`${baseUrl}/${referral.id}/${route}`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toBeNull();
    request.flush(referral);
  });

  it('records triage assessment with the exact body', () => {
    const body = { priority: REFERRAL_PRIORITIES.urgent, note: 'Urgent review.' };
    service.recordTriageAssessment(referral.id, body).subscribe();
    const request = http.expectOne(`${baseUrl}/${referral.id}/triage-assessment`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(body);
    request.flush({ ...referral, priority: REFERRAL_PRIORITIES.urgent });
  });

  it('discovers canonical assignment targets', () => {
    service.getAssignmentTargets().subscribe();
    const request = http.expectOne(`${baseUrl}/assignment-targets`);
    expect(request.request.method).toBe('GET');
    request.flush({ items: ['Cardiology Team A'] });
  });

  it.each([
    ['assignReferral', 'assign'],
    ['reassignReferral', 'reassign'],
  ] as const)('posts the exact canonical team payload for %s', (method, route) => {
    const body = { assignedTo: 'Cardiology Team A' };
    service[method](referral.id, body).subscribe();
    const request = http.expectOne(`${baseUrl}/${referral.id}/${route}`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(body);
    request.flush({ ...referral, assignedTo: body.assignedTo });
  });

  it('surfaces completion as a 204 void response', () => {
    let completed = false;
    service.completeReferral(referral.id).subscribe(() => (completed = true));
    const request = http.expectOne(`${baseUrl}/${referral.id}/complete`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toBeNull();
    request.flush(null, { status: 204, statusText: 'No Content' });
    expect(completed).toBe(true);
  });
});
