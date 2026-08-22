import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FormField } from './form-field';

describe('FormField', () => {
  let fixture: ComponentFixture<FormField>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [FormField] }).compileComponents();
    fixture = TestBed.createComponent(FormField);
    fixture.componentRef.setInput('label', 'NHS number');
    fixture.componentRef.setInput('forId', 'nhs-number');
  });

  it('connects the label to the projected control id', () => {
    fixture.detectChanges();
    const label = fixture.nativeElement.querySelector('label') as HTMLLabelElement;
    expect(label.textContent).toContain('NHS number');
    expect(label.htmlFor).toBe('nhs-number');
  });

  it('provides visible and accessible required indicators', () => {
    fixture.componentRef.setInput('required', true);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[aria-hidden="true"]')?.textContent).toBe('*');
    expect(fixture.nativeElement.querySelector('.sr-only')?.textContent).toContain('required');
  });

  it('renders the hint only when supplied', () => {
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.ct-form-field__hint')).toBeNull();
    fixture.componentRef.setInput('hint', 'Enter the 10-digit number.');
    fixture.detectChanges();
    const hint = fixture.nativeElement.querySelector('.ct-form-field__hint') as HTMLElement;
    expect(hint.id).toBe('nhs-number-hint');
    expect(hint.textContent).toContain('Enter the 10-digit number.');
  });

  it('renders the error only when supplied', () => {
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.ct-form-field__error')).toBeNull();
    fixture.componentRef.setInput('error', 'Enter a valid NHS number.');
    fixture.detectChanges();
    const error = fixture.nativeElement.querySelector('.ct-form-field__error') as HTMLElement;
    expect(error.id).toBe('nhs-number-error');
    expect(error.textContent).toContain('Enter a valid NHS number.');
  });
});
