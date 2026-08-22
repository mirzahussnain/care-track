import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StatusChip } from './status-chip';

describe('StatusChip', () => {
  let fixture: ComponentFixture<StatusChip>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [StatusChip] }).compileComponents();
    fixture = TestBed.createComponent(StatusChip);
    fixture.detectChanges();
  });

  it('creates with the neutral tone by default', () => {
    const chip = fixture.nativeElement.querySelector('.ct-status-chip') as HTMLElement;
    expect(fixture.componentInstance).toBeTruthy();
    expect(chip.classList.contains('ct-status-chip--neutral')).toBe(true);
  });

  it('applies the requested semantic tone', () => {
    fixture.componentRef.setInput('tone', 'success');
    fixture.detectChanges();
    const chip = fixture.nativeElement.querySelector('.ct-status-chip') as HTMLElement;
    expect(chip.classList.contains('ct-status-chip--success')).toBe(true);
  });

  it('does not create an ARIA live region', () => {
    expect(fixture.nativeElement.querySelector('[role="status"]')).toBeNull();
  });
});

@Component({
  imports: [StatusChip],
  template: '<ct-status-chip>Completed</ct-status-chip>',
})
class StatusChipHost {}

describe('StatusChip projection', () => {
  it('renders projected content and hides its decorative dot', async () => {
    await TestBed.configureTestingModule({ imports: [StatusChipHost] }).compileComponents();
    const fixture = TestBed.createComponent(StatusChipHost);
    fixture.detectChanges();
    const element = fixture.nativeElement as HTMLElement;
    expect(element.textContent).toContain('Completed');
    expect(element.querySelector('.ct-status-chip__dot')?.getAttribute('aria-hidden')).toBe('true');
  });
});
