import {
  ComponentFixture,
  TestBed,
} from '@angular/core/testing';

import {
  provideRouter,
} from '@angular/router';

import {
  signal,
} from '@angular/core';

import {
  AuthService,
} from '../../auth/auth.service';

import {
  CARETRACK_ROLES,
  CareTrackRole,
} from '../../auth/auth.models';

import { AppShell } from './app-shell';

describe('AppShell', () => {
  let component: AppShell;
  let fixture: ComponentFixture<AppShell>;
  const rolesSignal =
  signal<readonly string[]>([
    CARETRACK_ROLES.clinician,
  ]);

const authServiceMock = {
  roles:
    rolesSignal.asReadonly(),

  hasRole: (
    role: CareTrackRole
  ) =>
    rolesSignal()
      .includes(role),
};

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppShell],
      providers: [
        provideRouter([]),
        {
  provide: AuthService,
  useValue: authServiceMock,
},
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

  it('opens and closes mobile navigation', () => {
  expect(
    component.mobileNavigationOpen()
  ).toBe(false);

  component.openMobileNavigation();

  expect(
    component.mobileNavigationOpen()
  ).toBe(true);

  component.closeMobileNavigation();

  expect(
    component.mobileNavigationOpen()
  ).toBe(false);
});

it('closes mobile navigation on Escape', () => {
  component.openMobileNavigation();

  component.onEscape();

  expect(
    component.mobileNavigationOpen()
  ).toBe(false);
});

it('renders the mobile navigation drawer when opened', () => {
  component.openMobileNavigation();

  fixture.detectChanges();

  const element =
    fixture.nativeElement as HTMLElement;

  expect(
    element.querySelector(
      '[role="dialog"]'
    )
  ).not.toBeNull();
});

it('removes the mobile navigation drawer when closed', () => {
  component.openMobileNavigation();
  fixture.detectChanges();

  component.closeMobileNavigation();
  fixture.detectChanges();

  const element =
    fixture.nativeElement as HTMLElement;

  expect(
    element.querySelector(
      '[role="dialog"]'
    )
  ).toBeNull();
});
});