import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../../../environments/environment';
import { APPOINTMENT_STATUSES, APPOINTMENT_TYPES, Appointment } from '../models/appointment.models';
import { AppointmentApiService } from './appointment-api.service';

describe('AppointmentApiService', () => {
  let service: AppointmentApiService;
  let http: HttpTestingController;
  const baseUrl = `${environment.apiBaseUrl}/api/appointments`;
  const appointment: Appointment = {
    id: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
    appointmentReference: 'APT-001',
    patientId: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
    referralId: 'cccccccc-cccc-cccc-cccc-cccccccccccc',
    appointmentType: APPOINTMENT_TYPES.consultation,
    scheduledStart: '2026-09-01T09:00:00Z',
    scheduledEnd: '2026-09-01T09:30:00Z',
    location: 'Clinic A',
    status: APPOINTMENT_STATUSES.scheduled,
    createdAt: '2026-08-25T10:00:00Z',
    updatedAt: null,
    checkedInAt: null,
    startedAt: null,
    completedAt: null,
    cancelledAt: null,
    didNotAttendAt: null,
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [AppointmentApiService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(AppointmentApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('searches with exact filters, numeric enums, UTC values, sorting, and paging', () => {
    service
      .searchAppointments({
        patientId: appointment.patientId,
        referralId: appointment.referralId,
        status: APPOINTMENT_STATUSES.scheduled,
        appointmentType: APPOINTMENT_TYPES.consultation,
        location: ' Clinic A ',
        scheduledFrom: '2026-09-01T00:00:00.000Z',
        scheduledTo: '2026-09-02T00:00:00.000Z',
        page: 2,
        pageSize: 20,
        sortBy: 'scheduledStart',
        sortDirection: 'desc',
      })
      .subscribe();

    const request = http.expectOne((candidate) => candidate.url === baseUrl);
    expect(request.request.method).toBe('GET');
    expect(request.request.params.get('patientId')).toBe(appointment.patientId);
    expect(request.request.params.get('referralId')).toBe(appointment.referralId);
    expect(request.request.params.get('status')).toBe('0');
    expect(request.request.params.get('appointmentType')).toBe('0');
    expect(request.request.params.get('location')).toBe('Clinic A');
    expect(request.request.params.get('scheduledFrom')).toBe('2026-09-01T00:00:00.000Z');
    expect(request.request.params.get('scheduledTo')).toBe('2026-09-02T00:00:00.000Z');
    expect(request.request.params.get('page')).toBe('2');
    expect(request.request.params.get('pageSize')).toBe('20');
    expect(request.request.params.get('sortBy')).toBe('scheduledStart');
    expect(request.request.params.get('sortDirection')).toBe('desc');
    request.flush({ items: [], page: 2, pageSize: 20, totalCount: 0, totalPages: 0 });
  });

  it('omits optional search filters', () => {
    service
      .searchAppointments({
        page: 1,
        pageSize: 20,
        sortBy: 'scheduledStart',
        sortDirection: 'asc',
      })
      .subscribe();

    const request = http.expectOne((candidate) => candidate.url === baseUrl);
    expect(request.request.params.has('patientId')).toBe(false);
    expect(request.request.params.has('status')).toBe(false);
    expect(request.request.params.has('appointmentType')).toBe(false);
    expect(request.request.params.has('location')).toBe(false);
    request.flush({ items: [], page: 1, pageSize: 20, totalCount: 0, totalPages: 0 });
  });

  it('gets detail and creates with the exact body', () => {
    service.getAppointment(appointment.id).subscribe();
    const detail = http.expectOne(`${baseUrl}/${appointment.id}`);
    expect(detail.request.method).toBe('GET');
    detail.flush(appointment);

    const body = {
      appointmentReference: appointment.appointmentReference,
      patientId: appointment.patientId,
      referralId: appointment.referralId,
      appointmentType: appointment.appointmentType,
      scheduledStart: appointment.scheduledStart,
      scheduledEnd: appointment.scheduledEnd,
      location: appointment.location,
    };
    service.createAppointment(body).subscribe();
    const create = http.expectOne(baseUrl);
    expect(create.request.method).toBe('POST');
    expect(create.request.body).toEqual(body);
    create.flush(appointment, { status: 201, statusText: 'Created' });
  });

  it.each([
    ['checkInAppointment', 'check-in'],
    ['startAppointment', 'start'],
    ['completeAppointment', 'complete'],
    ['cancelAppointment', 'cancel'],
    ['markDidNotAttend', 'did-not-attend'],
  ] as const)('posts a null body to the exact %s route', (method, route) => {
    service[method](appointment.id).subscribe();
    const request = http.expectOne(`${baseUrl}/${appointment.id}/${route}`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toBeNull();
    request.flush(appointment);
  });
});
