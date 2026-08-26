import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, Subject, throwError } from 'rxjs';

import { PagedResult } from '../../../../shared/models/paged-result.model';
import { PatientApiService } from '../../../patients/data-access/patient-api.service';
import { ReferralPatientSummary } from '../../../patients/models/patient.models';
import { ReferralPatientSelector } from './referral-patient-selector';

describe('ReferralPatientSelector', () => {
  let fixture: ComponentFixture<ReferralPatientSelector>;
  const searchReferralPatients = vi.fn();
  const patient: ReferralPatientSummary = {
    id: '11111111-1111-1111-1111-111111111111',
    patientReference: 'PAT-001',
    fullName: 'Amina Khan',
    dateOfBirth: '1988-04-12',
  };
  const result: PagedResult<ReferralPatientSummary> = {
    items: [patient],
    page: 1,
    pageSize: 5,
    totalCount: 1,
    totalPages: 1,
  };

  beforeEach(async () => {
    searchReferralPatients.mockReset();
    await TestBed.configureTestingModule({
      imports: [ReferralPatientSelector],
      providers: [{ provide: PatientApiService, useValue: { searchReferralPatients } }],
    }).compileComponents();
    fixture = TestBed.createComponent(ReferralPatientSelector);
  });

  it('trims an explicit search and renders only referral-safe identity fields', () => {
    searchReferralPatients.mockReturnValue(of(result));
    fixture.componentInstance.searchControl.setValue('  Khan  ');

    fixture.componentInstance.submitSearch();
    fixture.detectChanges();

    expect(searchReferralPatients).toHaveBeenCalledWith({ search: 'Khan', page: 1, pageSize: 5 });
    expect(fixture.nativeElement.textContent).toContain('Amina Khan');
    expect(fixture.nativeElement.textContent).toContain('PAT-001');
    expect(fixture.nativeElement.textContent).toContain('1988-04-12');
  });

  it('supersedes an older request when paging', () => {
    const first$ = new Subject<PagedResult<ReferralPatientSummary>>();
    searchReferralPatients
      .mockReturnValueOnce(first$)
      .mockReturnValueOnce(of({ ...result, page: 2 }));
    fixture.componentInstance.submitSearch();

    fixture.componentInstance.changePage(2);

    expect(first$.observed).toBe(false);
    expect(searchReferralPatients).toHaveBeenLastCalledWith({
      search: undefined,
      page: 2,
      pageSize: 5,
    });
  });

  it('emits the selected patient and exposes a pressed selection state', () => {
    searchReferralPatients.mockReturnValue(of(result));
    const selected = vi.fn();
    fixture.componentInstance.patientSelected.subscribe(selected);
    fixture.componentRef.setInput('selectedPatient', patient);
    fixture.componentInstance.submitSearch();
    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector(
      '.patient-selector__results button',
    ) as HTMLButtonElement;
    button.click();

    expect(button.getAttribute('aria-pressed')).toBe('true');
    expect(selected).toHaveBeenCalledWith(patient);
  });

  it('distinguishes forbidden lookup from a generic failure', () => {
    searchReferralPatients.mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 403 })),
    );
    fixture.componentInstance.submitSearch();
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Patient lookup is not available');
  });
});
