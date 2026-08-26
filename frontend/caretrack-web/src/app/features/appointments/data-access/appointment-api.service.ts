import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { apiUrl } from '../../../core/http/api-url';
import { PagedResult } from '../../../shared/models/paged-result.model';
import {
  Appointment,
  AppointmentSearchItem,
  AppointmentSearchQuery,
  CreateAppointmentRequest,
} from '../models/appointment.models';

@Injectable({ providedIn: 'root' })
export class AppointmentApiService {
  private readonly http = inject(HttpClient);
  private readonly appointmentsUrl = apiUrl('/api/appointments');

  searchAppointments(
    query: AppointmentSearchQuery,
  ): Observable<PagedResult<AppointmentSearchItem>> {
    let params = new HttpParams()
      .set('page', query.page)
      .set('pageSize', query.pageSize)
      .set('sortBy', query.sortBy)
      .set('sortDirection', query.sortDirection);

    if (query.patientId) params = params.set('patientId', query.patientId);
    if (query.referralId) params = params.set('referralId', query.referralId);
    if (query.status !== undefined) params = params.set('status', query.status);
    if (query.appointmentType !== undefined) {
      params = params.set('appointmentType', query.appointmentType);
    }
    if (query.location?.trim()) params = params.set('location', query.location.trim());
    if (query.scheduledFrom) params = params.set('scheduledFrom', query.scheduledFrom);
    if (query.scheduledTo) params = params.set('scheduledTo', query.scheduledTo);

    return this.http.get<PagedResult<AppointmentSearchItem>>(this.appointmentsUrl, { params });
  }

  getAppointment(id: string): Observable<Appointment> {
    return this.http.get<Appointment>(`${this.appointmentsUrl}/${id}`);
  }

  createAppointment(request: CreateAppointmentRequest): Observable<Appointment> {
    return this.http.post<Appointment>(this.appointmentsUrl, request);
  }

  checkInAppointment(id: string): Observable<Appointment> {
    return this.postAction(id, 'check-in');
  }

  startAppointment(id: string): Observable<Appointment> {
    return this.postAction(id, 'start');
  }

  completeAppointment(id: string): Observable<Appointment> {
    return this.postAction(id, 'complete');
  }

  cancelAppointment(id: string): Observable<Appointment> {
    return this.postAction(id, 'cancel');
  }

  markDidNotAttend(id: string): Observable<Appointment> {
    return this.postAction(id, 'did-not-attend');
  }

  private postAction(id: string, action: string): Observable<Appointment> {
    return this.http.post<Appointment>(`${this.appointmentsUrl}/${id}/${action}`, null);
  }
}
