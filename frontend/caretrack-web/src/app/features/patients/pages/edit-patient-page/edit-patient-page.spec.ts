import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter, Router } from '@angular/router';
import { of, Subject, throwError } from 'rxjs';

import { CARETRACK_ROLES, CareTrackRole } from '../../../../core/auth/auth.models';
import { AuthService } from '../../../../core/auth/auth.service';
import { PatientApiService } from '../../data-access/patient-api.service';
import { Patient } from '../../models/patient.models';
import { EditPatientPage } from './edit-patient-page';

describe('EditPatientPage', () => {
  let fixture: ComponentFixture<EditPatientPage>;
  let load$: Subject<Patient>;
  const getPatient = vi.fn();
  const updatePatient = vi.fn();
  let navigate: ReturnType<typeof vi.spyOn>;
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
    load$ = new Subject<Patient>();
    getPatient.mockReset().mockReturnValue(load$);
    updatePatient.mockReset();
    await TestBed.configureTestingModule({
      imports: [EditPatientPage],
      providers: [
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: convertToParamMap({ id: patient.id }) } },
        },
        { provide: PatientApiService, useValue: { getPatient, updatePatient } },
        {
          provide: AuthService,
          useValue: { hasRole: (role: CareTrackRole) => role === CARETRACK_ROLES.clinician },
        },
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(EditPatientPage);
    navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
  });

  function finishLoad(value: Patient = patient): void {
    load$.next(value);
    fixture.detectChanges();
  }

  it('loads existing values and retains RowVersion outside editable controls', () => {
    finishLoad();
    expect(getPatient).toHaveBeenCalledWith(patient.id);
    expect(fixture.componentInstance.form.getRawValue()).toEqual({
      firstName: 'Amina',
      lastName: 'Khan',
      dateOfBirth: '1988-04-12',
    });
    expect(fixture.componentInstance.originalRowVersion()).toBe(patient.rowVersion);
    expect(fixture.componentInstance.form.get('rowVersion')).toBeNull();
    expect(fixture.nativeElement.textContent).not.toContain(patient.rowVersion);
  });

  it('sends the originally loaded Base64 RowVersion in the exact update body', () => {
    finishLoad();
    fixture.componentInstance.form.setValue({
      firstName: ' Amira ',
      lastName: ' Khan ',
      dateOfBirth: '1988-04-13',
    });
    updatePatient.mockReturnValue(
      of({ ...patient, firstName: 'Amira', fullName: 'Amira Khan', rowVersion: 'AAAAAAAAB9I=' }),
    );
    fixture.componentInstance.submit();
    expect(updatePatient).toHaveBeenCalledWith(patient.id, {
      firstName: 'Amira',
      lastName: 'Khan',
      dateOfBirth: '1988-04-13',
      rowVersion: patient.rowVersion,
    });
  });

  it('navigates to detail after a successful update', () => {
    finishLoad();
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    updatePatient.mockReturnValue(of({ ...patient, rowVersion: 'AAAAAAAAB9I=' }));
    fixture.componentInstance.submit();
    expect(navigate).toHaveBeenCalledWith(['/patients', patient.id]);
  });

  it('preserves unsaved values and never retries automatically on 409', () => {
    finishLoad();
    fixture.componentInstance.form.controls.lastName.setValue('Patel');
    updatePatient.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 409 })));
    fixture.componentInstance.submit();
    fixture.detectChanges();
    expect(updatePatient).toHaveBeenCalledOnce();
    expect(fixture.componentInstance.form.controls.lastName.value).toBe('Patel');
    expect(fixture.componentInstance.originalRowVersion()).toBe(patient.rowVersion);
    expect(fixture.nativeElement.textContent).toContain('Your unsaved values are preserved');
    expect(fixture.nativeElement.textContent).toContain(
      'Reloading the latest record will replace all unsaved edits',
    );
    expect(fixture.nativeElement.textContent).toContain('Return to patient');
  });

  it('reloads only after explicit recovery and replaces form and RowVersion', () => {
    finishLoad();
    fixture.componentInstance.form.controls.lastName.setValue('Patel');
    updatePatient.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 409 })));
    fixture.componentInstance.submit();
    const latest$ = new Subject<Patient>();
    getPatient.mockReturnValue(latest$);
    fixture.componentInstance.reloadLatest();
    expect(getPatient).toHaveBeenCalledTimes(2);
    expect(fixture.componentInstance.form.controls.lastName.value).toBe('Patel');
    const latest = {
      ...patient,
      lastName: 'Jones',
      fullName: 'Amina Jones',
      rowVersion: 'AAAAAAAAB9I=',
    };
    latest$.next(latest);
    expect(fixture.componentInstance.form.controls.lastName.value).toBe('Jones');
    expect(fixture.componentInstance.originalRowVersion()).toBe(latest.rowVersion);
    expect(fixture.componentInstance.conflict()).toBe(false);
  });

  it('keeps unsaved edits if explicit reload fails', () => {
    finishLoad();
    fixture.componentInstance.form.controls.lastName.setValue('Patel');
    updatePatient.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 409 })));
    fixture.componentInstance.submit();
    getPatient.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 500 })));
    fixture.componentInstance.reloadLatest();
    fixture.detectChanges();
    expect(fixture.componentInstance.form.controls.lastName.value).toBe('Patel');
    expect(fixture.nativeElement.textContent).toContain('unsaved edits are still preserved');
  });

  it('handles ReferralCoordinator read denial without exposing an edit form', () => {
    load$.error(new HttpErrorResponse({ status: 403 }));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain(
      'Patient editing is not available for your role',
    );
    expect(fixture.nativeElement.querySelector('form')).toBeNull();
  });
});
