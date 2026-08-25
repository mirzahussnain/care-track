import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { apiUrl } from '../../../core/http/api-url';
import { PagedResult } from '../../../shared/models/paged-result.model';
import {
  CreatePatientRequest,
  Patient,
  PatientSearchQuery,
  UpdatePatientRequest,
} from '../models/patient.models';

@Injectable({ providedIn: 'root' })
export class PatientApiService {
  private readonly http = inject(HttpClient);
  private readonly patientsUrl = apiUrl('/api/patients');

  searchPatients(query: PatientSearchQuery): Observable<PagedResult<Patient>> {
    let params = new HttpParams()
      .set('page', query.page)
      .set('pageSize', query.pageSize)
      .set('sortBy', query.sortBy)
      .set('sortDirection', query.sortDirection);

    const search = query.search?.trim();
    if (search) {
      params = params.set('search', search);
    }

    return this.http.get<PagedResult<Patient>>(this.patientsUrl, { params });
  }

  getPatient(id: string): Observable<Patient> {
    return this.http.get<Patient>(`${this.patientsUrl}/${id}`);
  }

  createPatient(request: CreatePatientRequest): Observable<Patient> {
    return this.http.post<Patient>(this.patientsUrl, request);
  }

  updatePatient(id: string, request: UpdatePatientRequest): Observable<Patient> {
    return this.http.put<Patient>(`${this.patientsUrl}/${id}`, request);
  }
}
