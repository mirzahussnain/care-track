import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of, Subject, throwError } from 'rxjs';

import { CARETRACK_ROLES, CareTrackRole } from '../../../../core/auth/auth.models';
import { AuthService } from '../../../../core/auth/auth.service';
import { PatientApiService } from '../../data-access/patient-api.service';
import { Patient } from '../../models/patient.models';
import { CreatePatientPage } from './create-patient-page';

describe('CreatePatientPage', () => {
  let fixture: ComponentFixture<CreatePatientPage>;
  let roles: readonly CareTrackRole[];
  const createPatient = vi.fn();
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
    roles = [CARETRACK_ROLES.referralCoordinator];
    createPatient.mockReset();
    await TestBed.configureTestingModule({
      imports: [CreatePatientPage],
      providers: [
        provideRouter([]),
        { provide: PatientApiService, useValue: { createPatient } },
        {
          provide: AuthService,
          useValue: { hasRole: (role: CareTrackRole) => roles.includes(role) },
        },
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(CreatePatientPage);
  });

  function setValidForm(): void {
    fixture.componentInstance.form.setValue({
      patientReference: ' PAT-001 ',
      firstName: ' Amina ',
      lastName: ' Khan ',
      dateOfBirth: '1988-04-12',
    });
  }

  it('shows required validation and does not call the API for an invalid form', () => {
    fixture.componentInstance.submit();
    fixture.detectChanges();
    expect(createPatient).not.toHaveBeenCalled();
    expect(fixture.nativeElement.textContent).toContain('Patient reference is required');
    expect(fixture.nativeElement.textContent).toContain('Date of birth is required');
  });

  it('rejects a future DOB as a frontend validation decision', () => {
    setValidForm();
    fixture.componentInstance.form.controls.dateOfBirth.setValue('2999-01-01');
    fixture.componentInstance.submit();
    fixture.detectChanges();
    expect(createPatient).not.toHaveBeenCalled();
    expect(fixture.nativeElement.textContent).toContain('Date of birth cannot be in the future');
  });

  it('submits the exact trimmed create request', () => {
    createPatient.mockReturnValue(of(patient));
    setValidForm();
    fixture.componentInstance.submit();
    expect(createPatient).toHaveBeenCalledWith({
      patientReference: 'PAT-001',
      firstName: 'Amina',
      lastName: 'Khan',
      dateOfBirth: '1988-04-12',
    });
  });

  it('shows coordinator success solely from the POST response and can register another', () => {
    createPatient.mockReturnValue(of(patient));
    setValidForm();
    fixture.componentInstance.submit();
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Patient Registered');
    expect(fixture.nativeElement.textContent).toContain('Amina Khan');
    fixture.componentInstance.registerAnother();
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Patient Identity');
    expect(fixture.componentInstance.form.controls.patientReference.value).toBe('');
  });

  it('navigates a clinician to created patient detail', () => {
    roles = [CARETRACK_ROLES.clinician];
    createPatient.mockReturnValue(of(patient));
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    setValidForm();
    fixture.componentInstance.submit();
    expect(navigate).toHaveBeenCalledWith(['/patients', patient.id]);
  });

  it('disables duplicate submission while the request is pending', () => {
    const response$ = new Subject<Patient>();
    createPatient.mockReturnValue(response$);
    setValidForm();
    fixture.componentInstance.submit();
    fixture.componentInstance.submit();
    fixture.detectChanges();
    expect(createPatient).toHaveBeenCalledOnce();
    expect(
      fixture.nativeElement.querySelector('button[type="submit"]')?.getAttribute('aria-busy'),
    ).toBe('true');
  });

  it('maps duplicate-reference conflict without clearing the form', () => {
    createPatient.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 409 })));
    setValidForm();
    fixture.componentInstance.submit();
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('already exists');
    expect(fixture.componentInstance.form.controls.firstName.value).toBe(' Amina ');
  });
});
