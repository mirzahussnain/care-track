import { TestBed } from '@angular/core/testing';

import { provideRouter, Router } from '@angular/router';

import { MsalBroadcastService, MsalService } from '@azure/msal-angular';

import { AccountInfo, AuthenticationResult, InteractionStatus } from '@azure/msal-browser';

import { Subject } from 'rxjs';

import { App } from './app';
import { AuthService } from './core/auth/auth.service';

describe('App', () => {
  let activeAccount: AccountInfo | null;
  let cachedAccounts: AccountInfo[];
  let redirectResult$: Subject<AuthenticationResult | null>;
  let interactionStatus$: Subject<InteractionStatus>;

  const getActiveAccount = vi.fn(() => activeAccount);

  const getAllAccounts = vi.fn(() => cachedAccounts);

  const setActiveAccount = vi.fn((account: AccountInfo | null) => {
    activeAccount = account;
  });

  const handleRedirectObservable = vi.fn();
  const loadCurrentUser = vi.fn();
  const clearCurrentUser = vi.fn();

  const msalServiceMock = {
    handleRedirectObservable,
    instance: {
      getActiveAccount,
      getAllAccounts,
      setActiveAccount,
    },
  };

  const authServiceMock = {
    loadCurrentUser,
    clearCurrentUser,
  };

  const firstAccount: AccountInfo = {
    homeAccountId: 'home-account-1',
    environment: 'login.microsoftonline.com',
    tenantId: 'tenant-1',
    username: 'first@example.com',
    localAccountId: 'user-1',
    name: 'First User',
  };

  const secondAccount: AccountInfo = {
    ...firstAccount,
    homeAccountId: 'home-account-2',
    username: 'second@example.com',
    localAccountId: 'user-2',
    name: 'Second User',
  };

  beforeEach(async () => {
    activeAccount = null;
    cachedAccounts = [];
    redirectResult$ = new Subject<AuthenticationResult | null>();
    interactionStatus$ = new Subject<InteractionStatus>();

    getActiveAccount.mockClear();
    getAllAccounts.mockClear();
    setActiveAccount.mockClear();
    loadCurrentUser.mockReset();
    clearCurrentUser.mockReset();
    handleRedirectObservable.mockReset();
    handleRedirectObservable.mockReturnValue(redirectResult$);

    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        provideRouter([]),
        {
          provide: MsalService,
          useValue: msalServiceMock,
        },
        {
          provide: MsalBroadcastService,
          useValue: {
            inProgress$: interactionStatus$,
          },
        },
        {
          provide: AuthService,
          useValue: authServiceMock,
        },
      ],
    }).compileComponents();
  });

  function createApp(): void {
    TestBed.createComponent(App);
  }

  function redirectResult(account: AccountInfo): AuthenticationResult {
    return {
      account,
    } as AuthenticationResult;
  }

  it('creates the app', () => {
    const fixture = TestBed.createComponent(App);

    expect(fixture.componentInstance).toBeTruthy();
  });

  it('establishes the redirect account then loads the CareTrack user', () => {
    const router = TestBed.inject(Router);
    const navigate = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    createApp();

    redirectResult$.next(redirectResult(firstAccount));

    expect(setActiveAccount).toHaveBeenCalledOnce();
    expect(setActiveAccount).toHaveBeenCalledWith(firstAccount);
    expect(activeAccount).toBe(firstAccount);
    expect(loadCurrentUser).toHaveBeenCalledOnce();
    expect(navigate).toHaveBeenCalledWith(['/dashboard']);
  });

  it('uses the single cached account fallback and loads its user', () => {
    cachedAccounts = [firstAccount];
    createApp();

    interactionStatus$.next(InteractionStatus.None);

    expect(setActiveAccount).toHaveBeenCalledWith(firstAccount);
    expect(activeAccount).toBe(firstAccount);
    expect(loadCurrentUser).toHaveBeenCalledOnce();
  });

  it('loads the user for an already active account after MSAL settles', () => {
    activeAccount = firstAccount;
    createApp();

    interactionStatus$.next(InteractionStatus.None);

    expect(setActiveAccount).not.toHaveBeenCalled();
    expect(loadCurrentUser).toHaveBeenCalledOnce();
  });

  it('does not select or load when multiple cached accounts exist', () => {
    cachedAccounts = [firstAccount, secondAccount];
    createApp();

    interactionStatus$.next(InteractionStatus.None);

    expect(setActiveAccount).not.toHaveBeenCalled();
    expect(loadCurrentUser).not.toHaveBeenCalled();
    expect(activeAccount).toBeNull();
  });

  it('does not load a user when no account can be resolved', () => {
    createApp();

    interactionStatus$.next(InteractionStatus.None);

    expect(setActiveAccount).not.toHaveBeenCalled();
    expect(loadCurrentUser).not.toHaveBeenCalled();
  });

  it('does not duplicate loading or navigation for repeated MSAL None events', () => {
    const router = TestBed.inject(Router);
    const navigate = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    createApp();

    redirectResult$.next(redirectResult(firstAccount));
    interactionStatus$.next(InteractionStatus.None);
    interactionStatus$.next(InteractionStatus.None);

    expect(setActiveAccount).toHaveBeenCalledOnce();
    expect(loadCurrentUser).toHaveBeenCalledOnce();
    expect(navigate).toHaveBeenCalledOnce();
  });

  it('loads again when MSAL resolves a different active account', () => {
    activeAccount = firstAccount;
    createApp();
    interactionStatus$.next(InteractionStatus.None);

    activeAccount = secondAccount;
    interactionStatus$.next(InteractionStatus.None);

    expect(loadCurrentUser).toHaveBeenCalledTimes(2);
  });

  it('clears the CareTrack user when the previously resolved account disappears', () => {
    activeAccount = firstAccount;
    createApp();
    interactionStatus$.next(InteractionStatus.None);

    activeAccount = null;
    cachedAccounts = [];
    interactionStatus$.next(InteractionStatus.None);

    expect(clearCurrentUser).toHaveBeenCalledOnce();
  });
});
