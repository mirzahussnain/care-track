import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../../../environments/environment';
import { Patient } from '../models/patient.models';
import { PatientApiService } from './patient-api.service';

describe('PatientApiService', () => {
  let service: PatientApiService;
  let http: HttpTestingController;
  const patient: Patient = {
    id: '11111111-1111-1111-1111-111111111111',
    patientReference: 'PAT-001',
    firstName: 'Amina',
    lastName: 'Khan',
    fullName: 'Amina Khan',
    dateOfBirth: '1988-04-12',
    createdAt: '2026-08-25T10:00:00Z',
    rowVersion: 'AAAAAAAAB9E=',
  };
  const baseUrl = `${environment.apiBaseUrl}/api/patients`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [PatientApiService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(PatientApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('searches with exact paging and fixed sorting parameters', () => {
    service
      .searchPatients({
        search: ' Khan ',
        page: 2,
        pageSize: 20,
        sortBy: 'lastName',
        sortDirection: 'asc',
      })
      .subscribe();
    const request = http.expectOne((req) => req.url === baseUrl);
    expect(request.request.method).toBe('GET');
    expect(request.request.params.get('search')).toBe('Khan');
    expect(request.request.params.get('page')).toBe('2');
    expect(request.request.params.get('pageSize')).toBe('20');
    expect(request.request.params.get('sortBy')).toBe('lastName');
    expect(request.request.params.get('sortDirection')).toBe('asc');
    request.flush({ items: [], page: 2, pageSize: 20, totalCount: 0, totalPages: 0 });
  });

  it('omits a whitespace-only search value', () => {
    service
      .searchPatients({
        search: '   ',
        page: 1,
        pageSize: 20,
        sortBy: 'lastName',
        sortDirection: 'asc',
      })
      .subscribe();
    const request = http.expectOne((req) => req.url === baseUrl);
    expect(request.request.params.has('search')).toBe(false);
    request.flush({ items: [], page: 1, pageSize: 20, totalCount: 0, totalPages: 0 });
  });

  it('gets a patient by ID', () => {
    service.getPatient(patient.id).subscribe((value) => expect(value).toEqual(patient));
    const request = http.expectOne(`${baseUrl}/${patient.id}`);
    expect(request.request.method).toBe('GET');
    request.flush(patient);
  });

  it('creates a patient with the exact request body', () => {
    const body = {
      patientReference: 'PAT-001',
      firstName: 'Amina',
      lastName: 'Khan',
      dateOfBirth: '1988-04-12',
    };
    service
      .createPatient(body)
      .subscribe((value) => expect(value.rowVersion).toBe(patient.rowVersion));
    const request = http.expectOne(baseUrl);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(body);
    request.flush(patient);
  });

  it('updates the route ID and preserves the Base64 RowVersion in the body', () => {
    const body = {
      firstName: 'Amira',
      lastName: 'Khan',
      dateOfBirth: '1988-04-12',
      rowVersion: patient.rowVersion,
    };
    service.updatePatient(patient.id, body).subscribe();
    const request = http.expectOne(`${baseUrl}/${patient.id}`);
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual(body);
    expect(request.request.body).not.toHaveProperty('id');
    request.flush({
      ...patient,
      firstName: 'Amira',
      fullName: 'Amira Khan',
      rowVersion: 'AAAAAAAAB9I=',
    });
  });
});
