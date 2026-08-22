import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';

import { DataToolbar } from './data-toolbar';

@Component({
  imports: [DataToolbar],
  template: `
    <ct-data-toolbar>
      <label ctToolbarPrimary>Search</label>
      <button ctToolbarActions>Clear filters</button>
    </ct-data-toolbar>
  `,
})
class DataToolbarHost {}

describe('DataToolbar', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [DataToolbarHost] }).compileComponents();
  });

  it('creates its structural wrapper', () => {
    const fixture = TestBed.createComponent(DataToolbarHost);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.ct-data-toolbar')).toBeTruthy();
  });

  it('renders primary and action projected content in their slots', () => {
    const fixture = TestBed.createComponent(DataToolbarHost);
    fixture.detectChanges();
    const primary = fixture.nativeElement.querySelector('.ct-data-toolbar__primary') as HTMLElement;
    const actions = fixture.nativeElement.querySelector('.ct-data-toolbar__actions') as HTMLElement;
    expect(primary.textContent).toContain('Search');
    expect(actions.textContent).toContain('Clear filters');
  });
});
