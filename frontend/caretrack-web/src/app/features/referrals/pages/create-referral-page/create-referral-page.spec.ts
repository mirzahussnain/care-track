import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of, Subject, throwError } from 'rxjs';

import { CARETRACK_ROLES, CareTrackRole } from '../../../../core/auth/auth.models';
import { AuthService } from '../../../../core/auth/auth.service';
import { PatientApiService } from '../../../patients/data-access/patient-api.service';
import { ReferralPatientSummary } from '../../../patients/models/patient.models';
import { ReferralApiService } from '../../data-access/referral-api.service';
import { REFERRAL_PRIORITIES, REFERRAL_STATUSES, Referral } from '../../models/referral.models';
import { CreateReferralPage } from './create-referral-page';

describe('CreateReferralPage', () => {
  let fixture: ComponentFixture<CreateReferralPage>;
  const createReferral = vi.fn();
  let roles: readonly CareTrackRole[];
  const searchReferralPatients = vi.fn();
  const patient: ReferralPatientSummary = {
    id: '11111111-1111-1111-1111-111111111111',
    patientReference: 'PAT-001',
    fullName: 'Amina Khan',
    dateOfBirth: '1988-04-12',
  };
  const referral: Referral = {
    id: '22222222-2222-2222-2222-222222222222',
    referralReference: 'REF-001',
    patientId: patient.id,
    status: REFERRAL_STATUSES.draft,
    priority: REFERRAL_PRIORITIES.urgent,
    reason: 'Specialist review',
    triageNote: null,
    createdAt: '2026-08-25T10:00:00Z',
    submittedAt: null,
    updatedAt: null,
    triagedAt: null,
    assignedTo: null,
    assignedAt: null,
  };

  beforeEach(async () => {
    createReferral.mockReset();
    roles = [CARETRACK_ROLES.referralCoordinator];
    searchReferralPatients.mockReset().mockReturnValue(
      of({
        items: [],
        page: 1,
        pageSize: 5,
        totalCount: 0,
        totalPages: 0,
      }),
    );
    await TestBed.configureTestingModule({
      imports: [CreateReferralPage],
      providers: [
        provideRouter([]),
        { provide: ReferralApiService, useValue: { createReferral } },
        { provide: PatientApiService, useValue: { searchReferralPatients } },
        {
          provide: AuthService,
          useValue: { hasRole: (role: CareTrackRole) => roles.includes(role) },
        },
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(CreateReferralPage);
  });

  function setValidForm(): void {
    fixture.componentInstance.selectPatient(patient);
    fixture.componentInstance.form.patchValue({
      referralReference: ' REF-001 ',
      priority: REFERRAL_PRIORITIES.urgent,
      reason: ' Specialist review ',
    });
  }

  it('requires a selected patient and referral fields before calling the API', () => {
    fixture.componentInstance.submit();
    fixture.detectChanges();

    expect(createReferral).not.toHaveBeenCalled();
    expect(fixture.nativeElement.textContent).toContain('Select a patient');
    expect(fixture.nativeElement.textContent).toContain('Referral reference is required');
  });

  it('sends the exact trimmed numeric-enum contract and navigates to detail', () => {
    createReferral.mockReturnValue(of(referral));
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    setValidForm();

    fixture.componentInstance.submit();

    expect(createReferral).toHaveBeenCalledWith({
      patientId: patient.id,
      referralReference: 'REF-001',
      priority: 1,
      reason: 'Specialist review',
    });
    expect(navigate).toHaveBeenCalledWith(['/referrals', referral.id]);
  });

  it('prevents duplicate submission while creation is pending', () => {
    const response$ = new Subject<Referral>();
    createReferral.mockReturnValue(response$);
    setValidForm();

    fixture.componentInstance.submit();
    fixture.componentInstance.submit();

    expect(createReferral).toHaveBeenCalledOnce();
  });

  it('maps a duplicate-reference 409 without clearing the form', () => {
    createReferral.mockReturnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 409,
            error: { title: 'Conflict', detail: 'Referral reference already exists.' },
          }),
      ),
    );
    setValidForm();

    fixture.componentInstance.submit();
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('already exists');
    expect(fixture.componentInstance.form.controls.reason.value).toBe(' Specialist review ');
  });

  it('does not render patient lookup or creation controls for Administrator', () => {
    roles = [CARETRACK_ROLES.administrator];
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain(
      'Referral management is not available for your role',
    );
    expect(fixture.nativeElement.querySelector('form')).toBeNull();
  });
});
