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
  AuthService,
} from './auth.service';

import {
  CARETRACK_ROLES,
} from './auth.models';
import { environment } from '../../../environments/environment';

describe('AuthService', () => {
  const getActiveAccount = vi.fn();
  const logoutRedirect = vi.fn();

  const msalServiceMock = {
    instance: {
      getActiveAccount,
    },

    logoutRedirect,
  };

  const clinicianAccount: AccountInfo = {
    homeAccountId: 'home-account-1',
    environment: 'login.microsoftonline.com',
    tenantId: 'tenant-1',
    username: 'clinician@example.com',
    localAccountId: 'user-1',
    name: 'Test Clinician',

    idTokenClaims: {
      roles: [
        'Clinician',
      ],
    },
  };

  beforeEach(() => {
    getActiveAccount.mockReset();
    logoutRedirect.mockReset();

    getActiveAccount.mockReturnValue(null);

    TestBed.configureTestingModule({
      providers: [
        AuthService,
        {
          provide: MsalService,
          useValue: msalServiceMock,
        },
      ],
    });
  });

  it('creates', () => {
    const service =
      TestBed.inject(AuthService);

    expect(service).toBeTruthy();
  });

  it('is unauthenticated when there is no active account', () => {
    getActiveAccount.mockReturnValue(null);

    const service =
      TestBed.inject(AuthService);

    service.refreshUser();

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

  it('maps the active MSAL account into CareTrack user state', () => {
    getActiveAccount.mockReturnValue(
      clinicianAccount
    );

    const service =
      TestBed.inject(AuthService);

    service.refreshUser();

    expect(
      service.currentUser()
    ).toEqual({
      id: 'user-1',
      name: 'Test Clinician',
      username:
        'clinician@example.com',
      roles: [
        'Clinician',
      ],
    });
  });

  it('marks the user as authenticated when an active account exists', () => {
    getActiveAccount.mockReturnValue(
      clinicianAccount
    );

    const service =
      TestBed.inject(AuthService);

    service.refreshUser();

    expect(
      service.isAuthenticated()
    ).toBe(true);
  });

  it('extracts roles from the account claims', () => {
    getActiveAccount.mockReturnValue(
      clinicianAccount
    );

    const service =
      TestBed.inject(AuthService);

    service.refreshUser();

    expect(
      service.roles()
    ).toEqual([
      'Clinician',
    ]);
  });

  it('returns true when the user has the requested role', () => {
    getActiveAccount.mockReturnValue(
      clinicianAccount
    );

    const service =
      TestBed.inject(AuthService);

    service.refreshUser();

    expect(
      service.hasRole(
        CARETRACK_ROLES.clinician
      )
    ).toBe(true);
  });

  it('returns false when the user does not have the requested role', () => {
    getActiveAccount.mockReturnValue(
      clinicianAccount
    );

    const service =
      TestBed.inject(AuthService);

    service.refreshUser();

    expect(
      service.hasRole(
        CARETRACK_ROLES.administrator
      )
    ).toBe(false);
  });

  it('uses username as the display name when account name is unavailable', () => {
    const accountWithoutName:
      AccountInfo = {
        ...clinicianAccount,
        name: undefined,
      };

    getActiveAccount.mockReturnValue(
      accountWithoutName
    );

    const service =
      TestBed.inject(AuthService);

    service.refreshUser();

    expect(
      service.currentUser()?.name
    ).toBe(
      'clinician@example.com'
    );
  });

  it('ignores non-string role values', () => {
    const accountWithMixedRoles =
      {
        ...clinicianAccount,

        idTokenClaims: {
          roles: [
            'Clinician',
            123,
            null,
            'Administrator',
          ],
        },
      } as AccountInfo;

    getActiveAccount.mockReturnValue(
      accountWithMixedRoles
    );

    const service =
      TestBed.inject(AuthService);

    service.refreshUser();

    expect(
      service.roles()
    ).toEqual([
      'Clinician',
      'Administrator',
    ]);
  });

  it('uses an empty role list when the roles claim is absent', () => {
    const accountWithoutRoles:
      AccountInfo = {
        ...clinicianAccount,

        idTokenClaims: {},
      };

    getActiveAccount.mockReturnValue(
      accountWithoutRoles
    );

    const service =
      TestBed.inject(AuthService);

    service.refreshUser();

    expect(
      service.roles()
    ).toEqual([]);
  });

  it('updates user state when refreshUser is called after the active account changes', () => {
    getActiveAccount.mockReturnValue(
      null
    );

    const service =
      TestBed.inject(AuthService);

    service.refreshUser();

    expect(
      service.isAuthenticated()
    ).toBe(false);

    getActiveAccount.mockReturnValue(
      clinicianAccount
    );

    service.refreshUser();

    expect(
      service.isAuthenticated()
    ).toBe(true);

    expect(
      service.currentUser()?.id
    ).toBe('user-1');
  });

  it('clears user state when the active account is removed', () => {
    getActiveAccount.mockReturnValue(
      clinicianAccount
    );

    const service =
      TestBed.inject(AuthService);

    service.refreshUser();

    expect(
      service.isAuthenticated()
    ).toBe(true);

    getActiveAccount.mockReturnValue(
      null
    );

    service.refreshUser();

    expect(
      service.currentUser()
    ).toBeNull();

    expect(
      service.isAuthenticated()
    ).toBe(false);
  });

  it('calls logoutRedirect with the active account', () => {
    getActiveAccount.mockReturnValue(
      clinicianAccount
    );

    const service =
      TestBed.inject(AuthService);

    service.signOut();

    expect(
      logoutRedirect
    ).toHaveBeenCalledOnce();

    expect(
      logoutRedirect
    ).toHaveBeenCalledWith({
      account: clinicianAccount,
      postLogoutRedirectUri:
      `${environment.auth.redirectUri}/auth/sign-in`,
    });
  });

  it('allows logout when there is no active account', () => {
    getActiveAccount.mockReturnValue(
      null
    );

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