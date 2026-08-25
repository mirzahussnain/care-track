import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PatientIdentityBanner } from './patient-identity-banner';

describe('PatientIdentityBanner', () => {
  let fixture: ComponentFixture<PatientIdentityBanner>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [PatientIdentityBanner] }).compileComponents();
    fixture = TestBed.createComponent(PatientIdentityBanner);
    fixture.componentRef.setInput('fullName', 'Amina Khan');
    fixture.componentRef.setInput('patientReference', 'PAT-001');
    fixture.componentRef.setInput('dateOfBirth', '1988-04-12');
    fixture.detectChanges();
  });

  it('renders all supported patient identity fields', () => {
    expect(fixture.nativeElement.textContent).toContain('Amina Khan');
    expect(fixture.nativeElement.textContent).toContain('PAT-001');
    expect(fixture.nativeElement.textContent).toContain('12 Apr 1988');
  });

  it('uses a labelled semantic patient identity region', () => {
    expect(
      fixture.nativeElement.querySelector('section[aria-label="Patient identity"]'),
    ).not.toBeNull();
    expect(fixture.nativeElement.querySelector('dl')).not.toBeNull();
  });
});
