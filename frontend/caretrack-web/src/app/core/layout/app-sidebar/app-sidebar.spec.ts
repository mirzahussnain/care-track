import {
  Component,
  signal,
} from '@angular/core';

import {
  ComponentFixture,
  TestBed,
} from '@angular/core/testing';

import {
  provideRouter,
} from '@angular/router';

import {
  AppSidebar,
} from './app-sidebar';

import {
  AuthService,
} from '../../auth/auth.service';

import {
  CARETRACK_ROLES,
  CareTrackRole,
} from '../../auth/auth.models';

import type {
  ShellNavigationItem,
} from '../shell-navigation';

@Component({
  template: '',
})
class TestDashboardPage {}

describe('AppSidebar', () => {
  let component: AppSidebar;
  let fixture:
    ComponentFixture<AppSidebar>;

  const rolesSignal =
    signal<readonly string[]>([]);

  const authServiceMock = {
    roles:
      rolesSignal.asReadonly(),

    hasRole: (
      role: CareTrackRole
    ) =>
      rolesSignal()
        .includes(role),
  };

  const navigation:
    readonly ShellNavigationItem[] = [
      {
        label: 'Dashboard',
        route: '/dashboard',
        icon: 'ph-squares-four',
        exact: true,
      },

      {
        label: 'Patients',
        route: '/patients',
        icon: 'ph-users',
        exact: false,
        roles: [
          CARETRACK_ROLES.clinician,
          CARETRACK_ROLES.referralCoordinator,
        ],
      },

      {
        label: 'Referrals',
        route: '/referrals',
        icon: 'ph-files',
        exact: false,
        roles: [
          CARETRACK_ROLES.clinician,
          CARETRACK_ROLES.referralCoordinator,
        ],
      },

      {
        label: 'Appointments',
        route: '/appointments',
        icon: 'ph-calendar-dots',
        exact: false,
        roles: [
          CARETRACK_ROLES.clinician,
        ],
      },
    ];

  beforeEach(async () => {
    rolesSignal.set([]);

    await TestBed
      .configureTestingModule({
        imports: [
          AppSidebar,
        ],

        providers: [
          provideRouter([
            {
              path: 'dashboard',
              component:
                TestDashboardPage,
            },
          ]),

          {
            provide: AuthService,
            useValue:
              authServiceMock,
          },
        ],
      })
      .compileComponents();

    fixture =
      TestBed.createComponent(
        AppSidebar
      );

    fixture.componentRef
      .setInput(
        'navigation',
        navigation
      );

    component =
      fixture.componentInstance;

    fixture.detectChanges();
  });

  it('creates', () => {
    expect(component)
      .toBeTruthy();
  });

  it('renders navigation items without role requirements', () => {
    const element =
      fixture.nativeElement as HTMLElement;

    expect(
      element.textContent
    ).toContain(
      'Dashboard'
    );
  });

  it('shows all clinical navigation items for a clinician', () => {
    rolesSignal.set([
      CARETRACK_ROLES
        .clinician,
    ]);

    fixture.detectChanges();

    const element =
      fixture.nativeElement as HTMLElement;

    expect(
      element.textContent
    ).toContain(
      'Dashboard'
    );

    expect(
      element.textContent
    ).toContain(
      'Patients'
    );

    expect(
      element.textContent
    ).toContain(
      'Referrals'
    );

    expect(
      element.textContent
    ).toContain(
      'Appointments'
    );

    expect(element.textContent).not.toContain('Clinical Notes');
  });

  it('hides Clinician-only appointments and contextual Clinical Notes for a referral coordinator', () => {
    rolesSignal.set([
      CARETRACK_ROLES
        .referralCoordinator,
    ]);

    fixture.detectChanges();

    const element =
      fixture.nativeElement as HTMLElement;

    expect(
      element.textContent
    ).toContain(
      'Dashboard'
    );

    expect(
      element.textContent
    ).toContain(
      'Patients'
    );

    expect(
      element.textContent
    ).toContain(
      'Referrals'
    );

    expect(
      element.textContent
    ).not.toContain(
      'Appointments'
    );

    expect(
      element.textContent
    ).not.toContain(
      'Clinical Notes'
    );
  });

  it('shows only unrestricted navigation for an administrator-only user', () => {
    rolesSignal.set([
      CARETRACK_ROLES
        .administrator,
    ]);

    fixture.detectChanges();

    const element =
      fixture.nativeElement as HTMLElement;

    expect(
      element.textContent
    ).toContain(
      'Dashboard'
    );

    expect(
      element.textContent
    ).not.toContain(
      'Patients'
    );

    expect(
      element.textContent
    ).not.toContain(
      'Referrals'
    );

    expect(
      element.textContent
    ).not.toContain(
      'Appointments'
    );

    expect(
      element.textContent
    ).not.toContain(
      'Clinical Notes'
    );
  });

  it('emits a collapse request when the toggle is clicked', () => {
    const emitSpy =
      vi.spyOn(
        component.collapseToggle,
        'emit'
      );

    const button =
      fixture.nativeElement
        .querySelector(
          'button'
        ) as HTMLButtonElement;

    button.click();

    expect(
      emitSpy
    ).toHaveBeenCalledOnce();
  });

  it('emits navigationSelected when a nav item is clicked', async () => {
    const emitSpy =
      vi.spyOn(
        component
          .navigationSelected,
        'emit'
      );

    const link =
      fixture.nativeElement
        .querySelector(
          'a'
        ) as HTMLAnchorElement;

    link.click();

    await fixture.whenStable();

    expect(
      emitSpy
    ).toHaveBeenCalledOnce();
  });

  it('shows branding by default', () => {
    const element =
      fixture.nativeElement as HTMLElement;

    expect(
      element.textContent
    ).toContain(
      'CareTrack'
    );

    expect(
      element.textContent
    ).toContain(
      'Clinical operations'
    );
  });

  it('hides branding when showBrand is false', () => {
    fixture.componentRef
      .setInput(
        'showBrand',
        false
      );

    fixture.detectChanges();

    const element =
      fixture.nativeElement as HTMLElement;

    expect(
      element.textContent
    ).not.toContain(
      'Clinical operations'
    );
  });

  it('hides the collapse control when requested', () => {
    fixture.componentRef
      .setInput(
        'showCollapseControl',
        false
      );

    fixture.detectChanges();

    const button =
      fixture.nativeElement
        .querySelector(
          'button[aria-controls="primary-navigation"]'
        );

    expect(button)
      .toBeNull();
  });
});
