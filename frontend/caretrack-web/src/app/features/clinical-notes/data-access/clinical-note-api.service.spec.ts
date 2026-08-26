import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../../../environments/environment';
import { ClinicalNote } from '../models/clinical-note.models';
import { ClinicalNoteApiService } from './clinical-note-api.service';

describe('ClinicalNoteApiService', () => {
  let service: ClinicalNoteApiService;
  let http: HttpTestingController;
  const appointmentId = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';
  const note: ClinicalNote = {
    id: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
    appointmentId,
    content: 'Synthetic clinical note.',
    createdBy: 'cccccccc-cccc-cccc-cccc-cccccccccccc',
    createdAt: '2026-08-25T10:00:00Z',
    updatedAt: null,
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ClinicalNoteApiService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(ClinicalNoteApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('lists notes by appointment and gets a note by ID', () => {
    service.getNotesForAppointment(appointmentId).subscribe();
    const list = http.expectOne(
      `${environment.apiBaseUrl}/api/appointments/${appointmentId}/clinical-notes`,
    );
    expect(list.request.method).toBe('GET');
    list.flush([note]);

    service.getClinicalNote(note.id).subscribe();
    const detail = http.expectOne(`${environment.apiBaseUrl}/api/clinical-notes/${note.id}`);
    expect(detail.request.method).toBe('GET');
    detail.flush(note);
  });

  it('creates with content only and never submits CreatedBy', () => {
    service.createClinicalNote(appointmentId, { content: note.content }).subscribe();
    const request = http.expectOne(
      `${environment.apiBaseUrl}/api/appointments/${appointmentId}/clinical-notes`,
    );
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ content: note.content });
    expect(request.request.body).not.toHaveProperty('createdBy');
    request.flush(note, { status: 201, statusText: 'Created' });
  });

  it('updates with content only', () => {
    const body = { content: 'Updated synthetic note.' };
    service.updateClinicalNote(note.id, body).subscribe();
    const request = http.expectOne(`${environment.apiBaseUrl}/api/clinical-notes/${note.id}`);
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual(body);
    expect(request.request.body).not.toHaveProperty('createdBy');
    request.flush({ ...note, ...body, updatedAt: '2026-08-25T11:00:00Z' });
  });

  it('does not expose a delete method', () => {
    expect('deleteClinicalNote' in service).toBe(false);
  });
});
