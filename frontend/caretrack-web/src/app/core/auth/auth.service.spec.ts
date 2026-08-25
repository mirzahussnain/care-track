import {
  TestBed,
} from '@angular/core/testing';

import {
  AccountInfo,
} from '@azure/msal-browser';

import {
  MsalService,
} from '@azure/msal-angular';

import {
  of,
  throwError,
} from 'rxjs';

import {
  environment,
} from '../../../environments/environment';

import {
  AuthService,
} from './auth.service';

import {
  CARETRACK_ROLES,
  AuthenticatedUser,
} from './auth.models';

import {
  CurrentUserApiService,
} from './current-user-api.service';

describe('AuthService', () => {
  const getActiveAccount =
    vi.fn();

  const logoutRedirect =
    vi.fn();

  const getCurrentUser =
    vi.fn();

  const msalServiceMock = {
    instance: {
      getActiveAccount,
    },

    logoutRedirect,
  };

  const currentUserApiMock = {
    getCurrentUser,
  };

  const clinicianAccount:
    AccountInfo = {
      homeAccountId:
        'home-account-1',

      environment:
        'login.microsoftonline.com',

      tenantId:
        'tenant-1',

      username:
        'clinician@example.com',

      localAccountId:
        'user-1',

      name:
        'Test Clinician',
    };

  const clinicianUser:
    AuthenticatedUser = {
      id: 'user-1',

      name:
        'Test Clinician',

      username:
        'clinician@example.com',

      roles: [
        'Clinician',
      ],
    };

  beforeEach(() => {
    getActiveAccount.mockReset();
    logoutRedirect.mockReset();
    getCurrentUser.mockReset();

    getActiveAccount
      .mockReturnValue(null);

    TestBed.configureTestingModule({
      providers: [
        AuthService,

        {
          provide: MsalService,
          useValue: msalServiceMock,
        },

        {
          provide:
            CurrentUserApiService,

          useValue:
            currentUserApiMock,
        },
      ],
    });
  });

  it('creates', () => {
    const service =
      TestBed.inject(AuthService);

    expect(service)
      .toBeTruthy();
  });

  it('is unauthenticated when there is no active MSAL account', () => {
    getActiveAccount
      .mockReturnValue(null);

    const service =
      TestBed.inject(AuthService);

    service.loadCurrentUser();

    expect(
      service.currentUser()
    ).toBeNull();

    expect(
      service.isAuthenticated()
    ).toBe(false);

    expect(
      service.roles()
    ).toEqual([]);

    expect(
      getCurrentUser
    ).not.toHaveBeenCalled();
  });

  it('loads the current CareTrack user from the API when an active account exists', () => {
    getActiveAccount
      .mockReturnValue(
        clinicianAccount
      );

    getCurrentUser
      .mockReturnValue(
        of(clinicianUser)
      );

    const service =
      TestBed.inject(AuthService);

    service.loadCurrentUser();

    expect(
      getCurrentUser
    ).toHaveBeenCalledOnce();

    expect(
      service.currentUser()
    ).toEqual(
      clinicianUser
    );
  });

  it('marks the user as authenticated after the current-user API succeeds', () => {
    getActiveAccount
      .mockReturnValue(
        clinicianAccount
      );

    getCurrentUser
      .mockReturnValue(
        of(clinicianUser)
      );

    const service =
      TestBed.inject(AuthService);

    service.loadCurrentUser();

    expect(
      service.isAuthenticated()
    ).toBe(true);
  });

  it('uses roles returned by the current-user API', () => {
    getActiveAccount
      .mockReturnValue(
        clinicianAccount
      );

    getCurrentUser
      .mockReturnValue(
        of(clinicianUser)
      );

    const service =
      TestBed.inject(AuthService);

    service.loadCurrentUser();

    expect(
      service.roles()
    ).toEqual([
      'Clinician',
    ]);
  });

  it('returns true when the current user has the requested role', () => {
    getActiveAccount
      .mockReturnValue(
        clinicianAccount
      );

    getCurrentUser
      .mockReturnValue(
        of(clinicianUser)
      );

    const service =
      TestBed.inject(AuthService);

    service.loadCurrentUser();

    expect(
      service.hasRole(
        CARETRACK_ROLES.clinician
      )
    ).toBe(true);
  });

  it('returns false when the current user does not have the requested role', () => {
    getActiveAccount
      .mockReturnValue(
        clinicianAccount
      );

    getCurrentUser
      .mockReturnValue(
        of(clinicianUser)
      );

    const service =
      TestBed.inject(AuthService);

    service.loadCurrentUser();

    expect(
      service.hasRole(
        CARETRACK_ROLES.administrator
      )
    ).toBe(false);
  });

  it('supports multiple roles returned by the API', () => {
    const multiRoleUser:
      AuthenticatedUser = {
        ...clinicianUser,

        roles: [
          'Clinician',
          'ReferralCoordinator',
        ],
      };

    getActiveAccount
      .mockReturnValue(
        clinicianAccount
      );

    getCurrentUser
      .mockReturnValue(
        of(multiRoleUser)
      );

    const service =
      TestBed.inject(AuthService);

    service.loadCurrentUser();

    expect(
      service.roles()
    ).toEqual([
      'Clinician',
      'ReferralCoordinator',
    ]);

    expect(
      service.hasRole(
        CARETRACK_ROLES
          .referralCoordinator
      )
    ).toBe(true);
  });

  it('supports a user with no application roles', () => {
    const userWithoutRoles:
      AuthenticatedUser = {
        ...clinicianUser,
        roles: [],
      };

    getActiveAccount
      .mockReturnValue(
        clinicianAccount
      );

    getCurrentUser
      .mockReturnValue(
        of(userWithoutRoles)
      );

    const service =
      TestBed.inject(AuthService);

    service.loadCurrentUser();

    expect(
      service.isAuthenticated()
    ).toBe(true);

    expect(
      service.roles()
    ).toEqual([]);
  });

  it('clears user state when loading the current user fails', () => {
    getActiveAccount
      .mockReturnValue(
        clinicianAccount
      );

    getCurrentUser
      .mockReturnValue(
        of(clinicianUser)
      );

    const service =
      TestBed.inject(AuthService);

    service.loadCurrentUser();

    expect(
      service.isAuthenticated()
    ).toBe(true);

    getCurrentUser
      .mockReturnValue(
        throwError(
          () =>
            new Error(
              'Current user request failed'
            )
        )
      );

    service.loadCurrentUser();

    expect(
      service.currentUser()
    ).toBeNull();

    expect(
      service.isAuthenticated()
    ).toBe(false);

    expect(
      service.roles()
    ).toEqual([]);
  });

  it('clears user state when the active MSAL account is removed', () => {
    getActiveAccount
      .mockReturnValue(
        clinicianAccount
      );

    getCurrentUser
      .mockReturnValue(
        of(clinicianUser)
      );

    const service =
      TestBed.inject(AuthService);

    service.loadCurrentUser();

    expect(
      service.isAuthenticated()
    ).toBe(true);

    getActiveAccount
      .mockReturnValue(null);

    service.loadCurrentUser();

    expect(
      service.currentUser()
    ).toBeNull();

    expect(
      service.isAuthenticated()
    ).toBe(false);

    expect(
      getCurrentUser
    ).toHaveBeenCalledTimes(1);
  });

  it('clearCurrentUser clears the current application user', () => {
    getActiveAccount
      .mockReturnValue(
        clinicianAccount
      );

    getCurrentUser
      .mockReturnValue(
        of(clinicianUser)
      );

    const service =
      TestBed.inject(AuthService);

    service.loadCurrentUser();

    expect(
      service.isAuthenticated()
    ).toBe(true);

    service.clearCurrentUser();

    expect(
      service.currentUser()
    ).toBeNull();

    expect(
      service.isAuthenticated()
    ).toBe(false);
  });

  it('clears user state and calls logoutRedirect with the active account', () => {
    getActiveAccount
      .mockReturnValue(
        clinicianAccount
      );

    getCurrentUser
      .mockReturnValue(
        of(clinicianUser)
      );

    const service =
      TestBed.inject(AuthService);

    service.loadCurrentUser();

    expect(
      service.isAuthenticated()
    ).toBe(true);

    service.signOut();

    expect(
      service.currentUser()
    ).toBeNull();

    expect(
      logoutRedirect
    ).toHaveBeenCalledOnce();

    expect(
      logoutRedirect
    ).toHaveBeenCalledWith({
      account:
        clinicianAccount,

      postLogoutRedirectUri:
        `${environment.auth.redirectUri}/auth/sign-in`,
    });
  });

  it('allows logout when there is no active account', () => {
    getActiveAccount
      .mockReturnValue(null);

    const service =
      TestBed.inject(AuthService);

    service.signOut();

    expect(
      logoutRedirect
    ).toHaveBeenCalledWith({
      account: undefined,

      postLogoutRedirectUri:
        `${environment.auth.redirectUri}/auth/sign-in`,
    });
  });
});