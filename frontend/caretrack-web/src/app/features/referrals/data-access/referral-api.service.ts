import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { apiUrl } from '../../../core/http/api-url';
import { PagedResult } from '../../../shared/models/paged-result.model';
import {
  AssignReferralRequest,
  CreateReferralRequest,
  RecordTriageAssessmentRequest,
  Referral,
  ReferralAssignmentTargetsResponse,
  ReferralHistoryEntry,
  ReferralSearchQuery,
} from '../models/referral.models';

@Injectable({ providedIn: 'root' })
export class ReferralApiService {
  private readonly http = inject(HttpClient);
  private readonly referralsUrl = apiUrl('/api/referrals');

  searchReferrals(query: ReferralSearchQuery): Observable<PagedResult<Referral>> {
    let params = new HttpParams()
      .set('page', query.page)
      .set('pageSize', query.pageSize)
      .set('sortBy', query.sortBy)
      .set('sortDirection', query.sortDirection);

    if (query.status !== undefined) {
      params = params.set('status', query.status);
    }
    if (query.priority !== undefined) {
      params = params.set('priority', query.priority);
    }
    if (query.assignedTo?.trim()) {
      params = params.set('assignedTo', query.assignedTo.trim());
    }
    if (query.createdFrom) {
      params = params.set('createdFrom', query.createdFrom);
    }
    if (query.createdTo) {
      params = params.set('createdTo', query.createdTo);
    }

    return this.http.get<PagedResult<Referral>>(this.referralsUrl, { params });
  }

  getReferral(id: string): Observable<Referral> {
    return this.http.get<Referral>(`${this.referralsUrl}/${id}`);
  }

  createReferral(request: CreateReferralRequest): Observable<Referral> {
    return this.http.post<Referral>(this.referralsUrl, request);
  }

  getHistory(id: string): Observable<readonly ReferralHistoryEntry[]> {
    return this.http.get<readonly ReferralHistoryEntry[]>(`${this.referralsUrl}/${id}/history`);
  }

  getAssignmentTargets(): Observable<ReferralAssignmentTargetsResponse> {
    return this.http.get<ReferralAssignmentTargetsResponse>(
      `${this.referralsUrl}/assignment-targets`,
    );
  }

  submitReferral(id: string): Observable<Referral> {
    return this.postTransition(id, 'submit');
  }

  startTriage(id: string): Observable<Referral> {
    return this.postTransition(id, 'start-triage');
  }

  requestMoreInformation(id: string): Observable<Referral> {
    return this.postTransition(id, 'request-more-information');
  }

  acceptReferral(id: string): Observable<Referral> {
    return this.postTransition(id, 'accept');
  }

  rejectReferral(id: string): Observable<Referral> {
    return this.postTransition(id, 'reject');
  }

  resubmitReferral(id: string): Observable<Referral> {
    return this.postTransition(id, 'resubmit');
  }

  recordTriageAssessment(id: string, request: RecordTriageAssessmentRequest): Observable<Referral> {
    return this.http.post<Referral>(`${this.referralsUrl}/${id}/triage-assessment`, request);
  }

  assignReferral(id: string, request: AssignReferralRequest): Observable<Referral> {
    return this.http.post<Referral>(`${this.referralsUrl}/${id}/assign`, request);
  }

  reassignReferral(id: string, request: AssignReferralRequest): Observable<Referral> {
    return this.http.post<Referral>(`${this.referralsUrl}/${id}/reassign`, request);
  }

  completeReferral(id: string): Observable<void> {
    return this.http.post<void>(`${this.referralsUrl}/${id}/complete`, null);
  }

  private postTransition(id: string, action: string): Observable<Referral> {
    return this.http.post<Referral>(`${this.referralsUrl}/${id}/${action}`, null);
  }
}
