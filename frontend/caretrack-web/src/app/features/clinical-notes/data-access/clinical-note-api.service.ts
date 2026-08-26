import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { apiUrl } from '../../../core/http/api-url';
import {
  ClinicalNote,
  CreateClinicalNoteRequest,
  UpdateClinicalNoteRequest,
} from '../models/clinical-note.models';

@Injectable({ providedIn: 'root' })
export class ClinicalNoteApiService {
  private readonly http = inject(HttpClient);

  getNotesForAppointment(appointmentId: string): Observable<readonly ClinicalNote[]> {
    return this.http.get<readonly ClinicalNote[]>(
      apiUrl(`/api/appointments/${appointmentId}/clinical-notes`),
    );
  }

  getClinicalNote(id: string): Observable<ClinicalNote> {
    return this.http.get<ClinicalNote>(apiUrl(`/api/clinical-notes/${id}`));
  }

  createClinicalNote(
    appointmentId: string,
    request: CreateClinicalNoteRequest,
  ): Observable<ClinicalNote> {
    return this.http.post<ClinicalNote>(
      apiUrl(`/api/appointments/${appointmentId}/clinical-notes`),
      request,
    );
  }

  updateClinicalNote(id: string, request: UpdateClinicalNoteRequest): Observable<ClinicalNote> {
    return this.http.put<ClinicalNote>(apiUrl(`/api/clinical-notes/${id}`), request);
  }
}
