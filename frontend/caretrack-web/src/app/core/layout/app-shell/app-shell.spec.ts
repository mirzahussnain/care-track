import {
  ComponentFixture,
  TestBed,
} from '@angular/core/testing';

import {
  provideRouter,
} from '@angular/router';

import { AppShell } from './app-shell';

describe('AppShell', () => {
  let component: AppShell;
  let fixture: ComponentFixture<AppShell>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppShell],
      providers: [
        provideRouter([]),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AppShell);
    component = fixture.componentInstance;

    fixture.detectChanges();
  });

  it('creates', () => {
    expect(component).toBeTruthy();
  });

  it('starts with the default area label', () => {
    expect(component.areaLabel()).toBe(
      'Dashboard'
    );
  });

  it('toggles sidebar state', () => {
    expect(
      component.sidebarCollapsed()
    ).toBe(false);

    component.toggleSidebar();

    expect(
      component.sidebarCollapsed()
    ).toBe(true);
  });
});