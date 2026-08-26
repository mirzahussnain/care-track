import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EmptyState } from './empty-state';

describe('EmptyState', () => {
  let fixture: ComponentFixture<EmptyState>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [EmptyState] }).compileComponents();
    fixture = TestBed.createComponent(EmptyState);
    fixture.componentRef.setInput('title', 'No results');
  });

  it('renders its required title without an alert role', () => {
    fixture.detectChanges();
    const element = fixture.nativeElement as HTMLElement;
    expect(element.querySelector('.ct-empty-state__title')?.textContent).toContain('No results');
    expect(element.querySelector('[role="alert"]')).toBeNull();
  });

  it('renders the description only when supplied', () => {
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.ct-empty-state__description')).toBeNull();
    fixture.componentRef.setInput('description', 'Adjust the current filters.');
    fixture.detectChanges();
    expect(
      fixture.nativeElement.querySelector('.ct-empty-state__description')?.textContent,
    ).toContain('Adjust the current filters.');
  });

  it('renders an optional decorative icon', () => {
    fixture.componentRef.setInput('icon', 'ph-folder-open');
    fixture.detectChanges();
    const icon = fixture.nativeElement.querySelector('i') as HTMLElement;
    expect(icon.classList.contains('ph-folder-open')).toBe(true);
    expect(icon.getAttribute('aria-hidden')).toBe('true');
  });
});

@Component({
  imports: [EmptyState],
  template: '<ct-empty-state title="No results"><button>Clear</button></ct-empty-state>',
})
class EmptyStateHost {}

describe('EmptyState projection', () => {
  it('renders projected action content', async () => {
    await TestBed.configureTestingModule({ imports: [EmptyStateHost] }).compileComponents();
    const fixture = TestBed.createComponent(EmptyStateHost);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('button')?.textContent).toContain('Clear');
  });
});
