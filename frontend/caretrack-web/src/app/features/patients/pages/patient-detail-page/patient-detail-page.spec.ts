import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { Subject } from 'rxjs';

import { CARETRACK_ROLES, CareTrackRole } from '../../../../core/auth/auth.models';
import { AuthService } from '../../../../core/auth/auth.service';
import { PatientApiService } from '../../data-access/patient-api.service';
import { Patient } from '../../models/patient.models';
import { PatientDetailPage } from './patient-detail-page';

describe('PatientDetailPage', () => {
  let fixture: ComponentFixture<PatientDetailPage>;
  let response$: Subject<Patient>;
  let roles: readonly CareTrackRole[];
  const getPatient = vi.fn();
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

  beforeEach(async () => {
    roles = [CARETRACK_ROLES.clinician];
    response$ = new Subject<Patient>();
    getPatient.mockReset().mockReturnValue(response$);
    await TestBed.configureTestingModule({
      imports: [PatientDetailPage],
      providers: [
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: convertToParamMap({ id: patient.id }) } },
        },
        { provide: PatientApiService, useValue: { getPatient } },
        {
          provide: AuthService,
          useValue: { hasRole: (role: CareTrackRole) => roles.includes(role) },
        },
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(PatientDetailPage);
  });

  it('loads the route patient and exposes a loading state', () => {
    fixture.detectChanges();
    expect(getPatient).toHaveBeenCalledWith(patient.id);
    expect(fixture.nativeElement.querySelector('[aria-label="Loading patient"]')).not.toBeNull();
  });

  it('renders PatientIdentityBanner data and record details', () => {
    response$.next(patient);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('ct-patient-identity-banner')).not.toBeNull();
    expect(fixture.nativeElement.textContent).toContain('Amina Khan');
    expect(fixture.nativeElement.textContent).toContain('PAT-001');
    expect(fixture.nativeElement.textContent).toContain('Edit patient');
    expect(fixture.nativeElement.textContent).not.toContain(patient.rowVersion);
  });

  it('does not offer edit to a ReferralCoordinator', () => {
    roles = [CARETRACK_ROLES.referralCoordinator];
    response$.next(patient);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).not.toContain('Edit patient');
  });

  it('renders a patient-not-found state for 404', () => {
    response$.error(new HttpErrorResponse({ status: 404 }));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Patient not found');
  });

  it('renders expected capability guidance for coordinator 403', () => {
    roles = [CARETRACK_ROLES.referralCoordinator];
    response$.error(new HttpErrorResponse({ status: 403 }));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain(
      'Patient details are not available for your role',
    );
    expect(fixture.nativeElement.textContent).toContain('Register patient');
  });

  it('renders a retry action for generic failure', () => {
    response$.error(new HttpErrorResponse({ status: 500 }));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Patient details could not be loaded');
    expect(fixture.nativeElement.textContent).toContain('Try again');
  });
});
