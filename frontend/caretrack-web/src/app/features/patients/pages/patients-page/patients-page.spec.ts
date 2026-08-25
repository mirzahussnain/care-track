import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, Subject } from 'rxjs';

import { CARETRACK_ROLES, CareTrackRole } from '../../../../core/auth/auth.models';
import { AuthService } from '../../../../core/auth/auth.service';
import { PagedResult } from '../../../../shared/models/paged-result.model';
import { PatientApiService } from '../../data-access/patient-api.service';
import { Patient } from '../../models/patient.models';
import { PatientsPage } from './patients-page';

describe('PatientsPage', () => {
  let fixture: ComponentFixture<PatientsPage>;
  let response$: Subject<PagedResult<Patient>>;
  let roles: readonly CareTrackRole[];
  const searchPatients = vi.fn();
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
  const result: PagedResult<Patient> = {
    items: [patient],
    page: 1,
    pageSize: 20,
    totalCount: 1,
    totalPages: 1,
  };

  beforeEach(async () => {
    roles = [CARETRACK_ROLES.clinician];
    response$ = new Subject<PagedResult<Patient>>();
    searchPatients.mockReset().mockReturnValue(response$);
    await TestBed.configureTestingModule({
      imports: [PatientsPage],
      providers: [
        provideRouter([]),
        { provide: PatientApiService, useValue: { searchPatients } },
        {
          provide: AuthService,
          useValue: { hasRole: (role: CareTrackRole) => roles.includes(role) },
        },
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(PatientsPage);
  });

  it('loads with fixed paging and sorting and shows structured loading state', () => {
    fixture.detectChanges();
    expect(searchPatients).toHaveBeenCalledWith({
      search: undefined,
      page: 1,
      pageSize: 20,
      sortBy: 'lastName',
      sortDirection: 'asc',
    });
    expect(fixture.nativeElement.querySelector('[aria-label="Loading patients"]')).not.toBeNull();
  });

  it('renders patient identifying fields in a semantic table', () => {
    response$.next(result);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('table')).not.toBeNull();
    expect(fixture.nativeElement.querySelector('th[scope="col"]')).not.toBeNull();
    expect(fixture.nativeElement.textContent).toContain('Amina Khan');
    expect(fixture.nativeElement.textContent).toContain('PAT-001');
    expect(fixture.nativeElement.textContent).toContain('12 Apr 1988');
  });

  it('trims search, resets page, and supersedes the previous request', () => {
    fixture.componentInstance.page.set(3);
    fixture.componentInstance.searchControl.setValue('  Khan  ');
    searchPatients.mockReturnValue(of(result));
    fixture.componentInstance.submitSearch();
    fixture.detectChanges();
    expect(response$.observed).toBe(false);
    expect(searchPatients).toHaveBeenLastCalledWith({
      search: 'Khan',
      page: 1,
      pageSize: 20,
      sortBy: 'lastName',
      sortDirection: 'asc',
    });
  });

  it('changes page using the applied search', () => {
    response$.next({ ...result, totalCount: 45, totalPages: 3 });
    fixture.componentInstance.appliedSearch.set('Khan');
    searchPatients.mockReturnValue(of({ ...result, page: 2, totalCount: 45, totalPages: 3 }));
    fixture.componentInstance.changePage(2);
    expect(searchPatients).toHaveBeenLastCalledWith({
      search: 'Khan',
      page: 2,
      pageSize: 20,
      sortBy: 'lastName',
      sortDirection: 'asc',
    });
  });

  it('changes the sort field, resets to page one, and preserves the applied search', () => {
    fixture.detectChanges();
    fixture.componentInstance.appliedSearch.set('Khan');
    fixture.componentInstance.page.set(3);
    searchPatients.mockReturnValue(of(result));

    fixture.componentInstance.sortByControl.setValue('createdAt');

    expect(fixture.componentInstance.page()).toBe(1);
    expect(response$.observed).toBe(false);
    expect(searchPatients).toHaveBeenLastCalledWith({
      search: 'Khan',
      page: 1,
      pageSize: 20,
      sortBy: 'createdAt',
      sortDirection: 'asc',
    });
  });

  it('changes the sort direction, resets to page one, and preserves the applied search', () => {
    fixture.detectChanges();
    fixture.componentInstance.appliedSearch.set('Khan');
    fixture.componentInstance.page.set(4);
    searchPatients.mockReturnValue(of(result));

    fixture.componentInstance.sortDirectionControl.setValue('desc');

    expect(fixture.componentInstance.page()).toBe(1);
    expect(searchPatients).toHaveBeenLastCalledWith({
      search: 'Khan',
      page: 1,
      pageSize: 20,
      sortBy: 'lastName',
      sortDirection: 'desc',
    });
  });

  it('preserves search and sorting when changing page', () => {
    fixture.detectChanges();
    fixture.componentInstance.appliedSearch.set('Khan');
    searchPatients.mockReturnValue(of({ ...result, page: 2, totalCount: 45, totalPages: 3 }));
    fixture.componentInstance.sortByControl.setValue('firstName');
    fixture.componentInstance.sortDirectionControl.setValue('desc');

    fixture.componentInstance.changePage(2);

    expect(searchPatients).toHaveBeenLastCalledWith({
      search: 'Khan',
      page: 2,
      pageSize: 20,
      sortBy: 'firstName',
      sortDirection: 'desc',
    });
  });

  it('prevents stale sort responses from overwriting the latest result', () => {
    const olderSort$ = new Subject<PagedResult<Patient>>();
    const latestSort$ = new Subject<PagedResult<Patient>>();
    const latestPatient = { ...patient, id: '22222222-2222-2222-2222-222222222222' };
    fixture.detectChanges();
    searchPatients.mockReturnValueOnce(olderSort$).mockReturnValueOnce(latestSort$);

    fixture.componentInstance.sortByControl.setValue('firstName');
    fixture.componentInstance.sortDirectionControl.setValue('desc');
    latestSort$.next({ ...result, items: [latestPatient] });
    olderSort$.next(result);

    expect(olderSort$.observed).toBe(false);
    expect(fixture.componentInstance.result()?.items).toEqual([latestPatient]);
  });

  it('renders labeled native selects with only supported typed values', () => {
    fixture.detectChanges();
    const sortBySelect = fixture.nativeElement.querySelector(
      '#patient-sort-by',
    ) as HTMLSelectElement;
    const directionSelect = fixture.nativeElement.querySelector(
      '#patient-sort-direction',
    ) as HTMLSelectElement;
    const sortByLabel = fixture.nativeElement.querySelector(
      'label[for="patient-sort-by"]',
    ) as HTMLLabelElement;
    const directionLabel = fixture.nativeElement.querySelector(
      'label[for="patient-sort-direction"]',
    ) as HTMLLabelElement;

    expect(sortBySelect.tagName).toBe('SELECT');
    expect(directionSelect.tagName).toBe('SELECT');
    expect(sortByLabel.textContent?.trim()).toBe('Sort by');
    expect(directionLabel.textContent?.trim()).toBe('Direction');
    expect([...sortBySelect.options].map((option) => option.value)).toEqual([
      'lastName',
      'firstName',
      'patientReference',
      'createdAt',
    ]);
    expect([...directionSelect.options].map((option) => option.value)).toEqual(['asc', 'desc']);
    expect(sortBySelect.selectedOptions[0]?.textContent).toBe('Last name');
    expect(directionSelect.selectedOptions[0]?.textContent).toBe('Ascending');
  });

  it('distinguishes no matches from an empty unfiltered list', () => {
    fixture.componentInstance.appliedSearch.set('Nobody');
    response$.next({ items: [], page: 1, pageSize: 20, totalCount: 0, totalPages: 0 });
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('No patients matched your search');
  });

  it('treats 403 as a capability state and keeps Register patient for a coordinator', () => {
    roles = [CARETRACK_ROLES.referralCoordinator];
    response$.error(new HttpErrorResponse({ status: 403 }));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain(
      'Patient browsing is not available for your role',
    );
    expect(fixture.nativeElement.textContent).toContain('Register patient');
    expect(fixture.nativeElement.textContent).not.toContain('Patients could not be loaded');
  });

  it('shows a retryable generic error', () => {
    response$.error(new HttpErrorResponse({ status: 500 }));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Patients could not be loaded');
    expect(fixture.nativeElement.textContent).toContain('Try again');
  });

  it('hides registration from an Administrator', () => {
    roles = [CARETRACK_ROLES.administrator];
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).not.toContain('Register patient');
  });

  it('handles a native form submit through Angular without browser navigation', () => {
    fixture.detectChanges();
    fixture.componentInstance.page.set(4);
    fixture.componentInstance.searchControl.setValue('  Khan  ');
    searchPatients.mockReturnValue(of(result));
    const form = fixture.nativeElement.querySelector('form') as HTMLFormElement;
    const submitEvent = new Event('submit', { bubbles: true, cancelable: true });

    const dispatchResult = form.dispatchEvent(submitEvent);
    fixture.detectChanges();

    expect(dispatchResult).toBe(false);
    expect(submitEvent.defaultPrevented).toBe(true);
    expect(fixture.componentInstance.appliedSearch()).toBe('Khan');
    expect(fixture.componentInstance.page()).toBe(1);
    expect(searchPatients).toHaveBeenLastCalledWith({
      search: 'Khan',
      page: 1,
      pageSize: 20,
      sortBy: 'lastName',
      sortDirection: 'asc',
    });
  });

  it('removes an applied search when whitespace is submitted', () => {
    fixture.detectChanges();
    fixture.componentInstance.appliedSearch.set('Khan');
    fixture.componentInstance.page.set(3);
    fixture.componentInstance.searchControl.setValue('   ');
    searchPatients.mockReturnValue(of({ ...result, items: [] }));

    fixture.nativeElement
      .querySelector('form')
      .dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));

    expect(fixture.componentInstance.appliedSearch()).toBe('');
    expect(fixture.componentInstance.page()).toBe(1);
    expect(searchPatients).toHaveBeenLastCalledWith({
      search: undefined,
      page: 1,
      pageSize: 20,
      sortBy: 'lastName',
      sortDirection: 'asc',
    });
  });

  it('supports repeated form searches with the latest submitted term', () => {
    fixture.detectChanges();
    searchPatients.mockReturnValue(of(result));
    const form = fixture.nativeElement.querySelector('form') as HTMLFormElement;

    fixture.componentInstance.searchControl.setValue('Amina');
    form.dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
    fixture.componentInstance.searchControl.setValue('PAT-001');
    form.dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));

    expect(searchPatients).toHaveBeenNthCalledWith(2, {
      search: 'Amina',
      page: 1,
      pageSize: 20,
      sortBy: 'lastName',
      sortDirection: 'asc',
    });
    expect(searchPatients).toHaveBeenNthCalledWith(3, {
      search: 'PAT-001',
      page: 1,
      pageSize: 20,
      sortBy: 'lastName',
      sortDirection: 'asc',
    });
  });

  it('renders safe button types and Clear does not cause an extra submit', () => {
    fixture.componentInstance.appliedSearch.set('Khan');
    fixture.componentInstance.searchControl.setValue('Khan');
    fixture.componentInstance.page.set(2);
    searchPatients.mockReturnValue(of({ ...result, items: [] }));
    fixture.detectChanges();
    const buttons = [
      ...fixture.nativeElement.querySelectorAll('form button'),
    ] as HTMLButtonElement[];
    const searchButton = buttons.find((button) => button.textContent?.includes('Search'));
    const clearButton = buttons.find((button) => button.textContent?.includes('Clear'));

    expect(searchButton?.type).toBe('submit');
    expect(clearButton?.type).toBe('button');
    clearButton?.click();

    expect(searchPatients).toHaveBeenCalledTimes(2);
    expect(fixture.componentInstance.appliedSearch()).toBe('');
    expect(fixture.componentInstance.page()).toBe(1);
    expect(searchPatients).toHaveBeenLastCalledWith({
      search: undefined,
      page: 1,
      pageSize: 20,
      sortBy: 'lastName',
      sortDirection: 'asc',
    });
  });

  it('clears search without resetting the selected sorting', () => {
    fixture.detectChanges();
    searchPatients.mockReturnValue(of(result));
    fixture.componentInstance.sortByControl.setValue('createdAt');
    fixture.componentInstance.sortDirectionControl.setValue('desc');
    fixture.componentInstance.appliedSearch.set('Khan');
    fixture.componentInstance.searchControl.setValue('Khan');
    fixture.componentInstance.page.set(2);

    fixture.componentInstance.clearSearch();

    expect(fixture.componentInstance.sortByControl.value).toBe('createdAt');
    expect(fixture.componentInstance.sortDirectionControl.value).toBe('desc');
    expect(searchPatients).toHaveBeenLastCalledWith({
      search: undefined,
      page: 1,
      pageSize: 20,
      sortBy: 'createdAt',
      sortDirection: 'desc',
    });
  });
});
