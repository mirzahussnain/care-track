import { TestBed } from '@angular/core/testing';

import { AccountInfo } from '@azure/msal-browser';

import { MsalService } from '@azure/msal-angular';

import { Subject, of, throwError } from 'rxjs';

import { environment } from '../../../environments/environment';

import { AuthenticatedUser, CARETRACK_ROLES } from './auth.models';

import { AuthService } from './auth.service';

import { CurrentUserApiService } from './current-user-api.service';

describe('AuthService', () => {
  let activeAccount: AccountInfo | null;

  const getActiveAccount = vi.fn(() => activeAccount);

  const logoutRedirect = vi.fn();
  const getCurrentUser = vi.fn();

  const msalServiceMock = {
    instance: {
      getActiveAccount,
    },
    logoutRedirect,
  };

  const currentUserApiMock = {
    getCurrentUser,
  };

  const clinicianAccount: AccountInfo = {
    homeAccountId: 'home-account-1',
    environment: 'login.microsoftonline.com',
    tenantId: 'tenant-1',
    username: 'clinician@example.com',
    localAccountId: 'user-1',
    name: 'Test Clinician',
  };

  const administratorAccount: AccountInfo = {
    ...clinicianAccount,
    homeAccountId: 'home-account-2',
    username: 'administrator@example.com',
    localAccountId: 'user-2',
    name: 'Test Administrator',
  };

  const clinicianUser: AuthenticatedUser = {
    id: 'caretrack-user-1',
    name: 'Test Clinician',
    username: 'clinician@example.com',
    roles: [CARETRACK_ROLES.clinician],
  };

  const administratorUser: AuthenticatedUser = {
    id: 'caretrack-user-2',
    name: 'Test Administrator',
    username: 'administrator@example.com',
    roles: [CARETRACK_ROLES.administrator],
  };

  beforeEach(() => {
    activeAccount = null;
    getActiveAccount.mockClear();
    logoutRedirect.mockReset();
    getCurrentUser.mockReset();

    TestBed.configureTestingModule({
      providers: [
        AuthService,
        {
          provide: MsalService,
          useValue: msalServiceMock,
        },
        {
          provide: CurrentUserApiService,
          useValue: currentUserApiMock,
        },
      ],
    });
  });

  function createService(): AuthService {
    return TestBed.inject(AuthService);
  }

  it('starts idle and unauthenticated', () => {
    const service = createService();

    expect(service.status()).toBe('idle');
    expect(service.isLoading()).toBe(false);
    expect(service.currentUser()).toBeNull();
    expect(service.isAuthenticated()).toBe(false);
    expect(service.roles()).toEqual([]);
  });

  it('does not call the API without an active account', () => {
    const service = createService();

    service.loadCurrentUser();

    expect(getCurrentUser).not.toHaveBeenCalled();
    expect(service.status()).toBe('idle');
    expect(service.currentUser()).toBeNull();
  });

  it('sets loading while the current-user request is pending', () => {
    const response$ = new Subject<AuthenticatedUser>();
    activeAccount = clinicianAccount;
    getCurrentUser.mockReturnValue(response$);
    const service = createService();

    service.loadCurrentUser();

    expect(service.status()).toBe('loading');
    expect(service.isLoading()).toBe(true);
    expect(service.isAuthenticated()).toBe(false);
  });

  it('sets the API user, roles, and ready status after success', () => {
    activeAccount = clinicianAccount;
    getCurrentUser.mockReturnValue(of(clinicianUser));
    const service = createService();

    service.loadCurrentUser();

    expect(getCurrentUser).toHaveBeenCalledOnce();
    expect(service.status()).toBe('ready');
    expect(service.isLoading()).toBe(false);
    expect(service.currentUser()).toEqual(clinicianUser);
    expect(service.isAuthenticated()).toBe(true);
    expect(service.roles()).toEqual([CARETRACK_ROLES.clinician]);
  });

  it('checks roles returned by the API', () => {
    activeAccount = clinicianAccount;
    getCurrentUser.mockReturnValue(of(clinicianUser));
    const service = createService();

    service.loadCurrentUser();

    expect(service.hasRole(CARETRACK_ROLES.clinician)).toBe(true);

    expect(service.hasRole(CARETRACK_ROLES.administrator)).toBe(false);
  });

  it('sets error and clears stale user and role state when loading fails', () => {
    activeAccount = clinicianAccount;
    getCurrentUser.mockReturnValueOnce(of(clinicianUser));
    const service = createService();
    service.loadCurrentUser();

    activeAccount = administratorAccount;
    getCurrentUser.mockReturnValueOnce(throwError(() => new Error('Current user request failed')));

    service.loadCurrentUser();

    expect(service.status()).toBe('error');
    expect(service.isLoading()).toBe(false);
    expect(service.currentUser()).toBeNull();
    expect(service.isAuthenticated()).toBe(false);
    expect(service.roles()).toEqual([]);
  });

  it('coalesces duplicate calls for an in-flight account', () => {
    const response$ = new Subject<AuthenticatedUser>();
    activeAccount = clinicianAccount;
    getCurrentUser.mockReturnValue(response$);
    const service = createService();

    service.loadCurrentUser();
    service.loadCurrentUser();

    expect(getCurrentUser).toHaveBeenCalledOnce();
    expect(service.status()).toBe('loading');
  });

  it('does not reload the same successfully loaded account', () => {
    activeAccount = clinicianAccount;
    getCurrentUser.mockReturnValue(of(clinicianUser));
    const service = createService();

    service.loadCurrentUser();
    service.loadCurrentUser();

    expect(getCurrentUser).toHaveBeenCalledOnce();
    expect(service.currentUser()).toEqual(clinicianUser);
  });

  it('allows the same account to load again after state is cleared', () => {
    activeAccount = clinicianAccount;
    getCurrentUser.mockReturnValue(of(clinicianUser));
    const service = createService();

    service.loadCurrentUser();
    service.clearCurrentUser();
    service.loadCurrentUser();

    expect(getCurrentUser).toHaveBeenCalledTimes(2);
    expect(service.status()).toBe('ready');
  });

  it('clears all application state when the active account is removed', () => {
    activeAccount = clinicianAccount;
    getCurrentUser.mockReturnValue(of(clinicianUser));
    const service = createService();
    service.loadCurrentUser();

    activeAccount = null;
    service.loadCurrentUser();

    expect(getCurrentUser).toHaveBeenCalledOnce();
    expect(service.status()).toBe('idle');
    expect(service.currentUser()).toBeNull();
    expect(service.roles()).toEqual([]);
  });

  it('ignores a stale response from the previously active account', () => {
    const clinicianResponse$ = new Subject<AuthenticatedUser>();
    const administratorResponse$ = new Subject<AuthenticatedUser>();

    activeAccount = clinicianAccount;
    getCurrentUser.mockReturnValueOnce(clinicianResponse$);
    const service = createService();
    service.loadCurrentUser();

    activeAccount = administratorAccount;
    getCurrentUser.mockReturnValueOnce(administratorResponse$);
    service.loadCurrentUser();

    clinicianResponse$.next(clinicianUser);

    expect(service.status()).toBe('loading');
    expect(service.currentUser()).toBeNull();

    administratorResponse$.next(administratorUser);

    expect(service.status()).toBe('ready');
    expect(service.currentUser()).toEqual(administratorUser);
  });

  it('ignores a stale response after the active account is removed', () => {
    const response$ = new Subject<AuthenticatedUser>();
    activeAccount = clinicianAccount;
    getCurrentUser.mockReturnValue(response$);
    const service = createService();
    service.loadCurrentUser();

    activeAccount = null;
    service.loadCurrentUser();
    response$.next(clinicianUser);

    expect(service.status()).toBe('idle');
    expect(service.currentUser()).toBeNull();
  });

  it('loads a newly active account after another account was ready', () => {
    activeAccount = clinicianAccount;
    getCurrentUser.mockReturnValueOnce(of(clinicianUser));
    const service = createService();
    service.loadCurrentUser();

    activeAccount = administratorAccount;
    getCurrentUser.mockReturnValueOnce(of(administratorUser));
    service.loadCurrentUser();

    expect(getCurrentUser).toHaveBeenCalledTimes(2);
    expect(service.currentUser()).toEqual(administratorUser);
    expect(service.roles()).toEqual([CARETRACK_ROLES.administrator]);
  });

  it('supports multiple roles returned by the API', () => {
    const multiRoleUser: AuthenticatedUser = {
      ...clinicianUser,
      roles: [CARETRACK_ROLES.clinician, CARETRACK_ROLES.referralCoordinator],
    };

    activeAccount = clinicianAccount;
    getCurrentUser.mockReturnValue(of(multiRoleUser));
    const service = createService();
    service.loadCurrentUser();

    expect(service.roles()).toEqual(multiRoleUser.roles);
    expect(service.hasRole(CARETRACK_ROLES.referralCoordinator)).toBe(true);
  });

  it('supports an authenticated user with zero roles', () => {
    const userWithoutRoles: AuthenticatedUser = {
      ...clinicianUser,
      roles: [],
    };

    activeAccount = clinicianAccount;
    getCurrentUser.mockReturnValue(of(userWithoutRoles));
    const service = createService();
    service.loadCurrentUser();

    expect(service.status()).toBe('ready');
    expect(service.isAuthenticated()).toBe(true);
    expect(service.roles()).toEqual([]);
  });

  it('clearCurrentUser resets user, request bookkeeping, and status', () => {
    const response$ = new Subject<AuthenticatedUser>();
    activeAccount = clinicianAccount;
    getCurrentUser.mockReturnValue(response$);
    const service = createService();
    service.loadCurrentUser();

    service.clearCurrentUser();
    response$.next(clinicianUser);

    expect(service.status()).toBe('idle');
    expect(service.isLoading()).toBe(false);
    expect(service.currentUser()).toBeNull();
  });

  it('clears state before signing out with the active account', () => {
    activeAccount = clinicianAccount;
    getCurrentUser.mockReturnValue(of(clinicianUser));
    const service = createService();
    service.loadCurrentUser();

    service.signOut();

    expect(service.status()).toBe('idle');
    expect(service.currentUser()).toBeNull();
    expect(logoutRedirect).toHaveBeenCalledOnce();
    expect(logoutRedirect).toHaveBeenCalledWith({
      account: clinicianAccount,
      postLogoutRedirectUri: `${environment.auth.redirectUri}/`,
    });
  });

  it('signs out without an active account', () => {
    const service = createService();

    service.signOut();

    expect(service.status()).toBe('idle');
    expect(logoutRedirect).toHaveBeenCalledWith({
      account: undefined,
      postLogoutRedirectUri: `${environment.auth.redirectUri}/`,
    });
  });
});
