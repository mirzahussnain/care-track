import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { AuthenticatedUser, CARETRACK_ROLES, CareTrackRole } from '../../auth/auth.models';
import { AuthService } from '../../auth/auth.service';
import { AppShell } from './app-shell';

describe('AppShell', () => {
  let component: AppShell;
  let fixture: ComponentFixture<AppShell>;
  const rolesSignal = signal<readonly string[]>([CARETRACK_ROLES.clinician]);
  const currentUserSignal = signal<AuthenticatedUser | null>({
    id: 'user-1',
    name: 'Amina Khan',
    username: 'amina.khan@example.test',
    roles: [CARETRACK_ROLES.clinician],
    isDemoAccount: false,
  });
  const signOut = vi.fn();

  const authServiceMock = {
    currentUser: currentUserSignal.asReadonly(),
    roles: rolesSignal.asReadonly(),
    hasRole: (role: CareTrackRole) => rolesSignal().includes(role),
    signOut,
  };

  beforeEach(async () => {
    rolesSignal.set([CARETRACK_ROLES.clinician]);
    currentUserSignal.set({
      id: 'user-1',
      name: 'Amina Khan',
      username: 'amina.khan@example.test',
      roles: [CARETRACK_ROLES.clinician],
      isDemoAccount: false,
    });
    signOut.mockReset();

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
    expect(component.areaLabel()).toBe('Dashboard');
  });

  it('toggles sidebar state', () => {
    expect(component.sidebarCollapsed()).toBe(false);
    component.toggleSidebar();
    expect(component.sidebarCollapsed()).toBe(true);
  });

  it('opens and closes mobile navigation', () => {
    expect(component.mobileNavigationOpen()).toBe(false);
    component.openMobileNavigation();
    expect(component.mobileNavigationOpen()).toBe(true);
    component.closeMobileNavigation();
    expect(component.mobileNavigationOpen()).toBe(false);
  });

  it('closes mobile navigation on Escape', () => {
    component.openMobileNavigation();
    component.onEscape();
    expect(component.mobileNavigationOpen()).toBe(false);
  });

  it('returns focus to the mobile menu trigger after the drawer closes', async () => {
    const element = fixture.nativeElement as HTMLElement;
    const trigger = element.querySelector<HTMLButtonElement>('[aria-label="Open navigation"]')!;
    trigger.focus();
    trigger.click();
    fixture.detectChanges();

    element.querySelector<HTMLButtonElement>('[aria-label="Close navigation"]')?.click();
    fixture.detectChanges();
    await Promise.resolve();

    expect(component.mobileNavigationOpen()).toBe(false);
    expect(document.activeElement).toBe(trigger);
  });

  it('renders the mobile navigation drawer when opened', () => {
    component.openMobileNavigation();
    fixture.detectChanges();

    const element = fixture.nativeElement as HTMLElement;
    expect(element.querySelector('[role="dialog"]')).not.toBeNull();
  });

  it('removes the mobile navigation drawer when closed', () => {
    component.openMobileNavigation();
    fixture.detectChanges();
    component.closeMobileNavigation();
    fixture.detectChanges();

    const element = fixture.nativeElement as HTMLElement;
    expect(element.querySelector('[role="dialog"]')).toBeNull();
  });

  it('shows account context and delegates sign out to the existing auth service', () => {
    const element = fixture.nativeElement as HTMLElement;

    expect(element.textContent).toContain('Amina Khan');
    expect(element.textContent).toContain('Clinician');

    const signOutButton = element.querySelector<HTMLButtonElement>(
      '[data-testid="topbar-sign-out"]',
    )!;
    expect(signOutButton.textContent).toContain('Sign out');
    expect(signOutButton.type).toBe('button');

    signOutButton.click();

    expect(signOut).toHaveBeenCalledOnce();
  });

  it('shows the synthetic-data banner only for server-identified demo accounts', () => {
    const element = fixture.nativeElement as HTMLElement;
    expect(element.querySelector('[data-testid="demo-account-banner"]')).toBeNull();

    currentUserSignal.update((user) =>
      user
        ? {
            ...user,
            isDemoAccount: true,
          }
        : null,
    );
    fixture.detectChanges();

    expect(element.querySelector('[data-testid="demo-account-banner"]')?.textContent).toContain(
      'DEMO ACCOUNT · SYNTHETIC DATA ONLY',
    );
  });

  it('keeps sign out reachable from mobile navigation and closes the drawer', () => {
    component.openMobileNavigation();
    fixture.detectChanges();

    const element = fixture.nativeElement as HTMLElement;
    element.querySelector<HTMLButtonElement>('[data-testid="mobile-sign-out"]')?.click();
    fixture.detectChanges();

    expect(signOut).toHaveBeenCalledOnce();
    expect(component.mobileNavigationOpen()).toBe(false);
  });
});
