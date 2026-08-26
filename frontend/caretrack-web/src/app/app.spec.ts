import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { MsalBroadcastService, MsalService } from '@azure/msal-angular';
import { AccountInfo, AuthenticationResult, InteractionStatus } from '@azure/msal-browser';
import { Subject } from 'rxjs';

import { App } from './app';
import { AuthService } from './core/auth/auth.service';

@Component({
  standalone: true,
  template: '',
})
class TestRouteComponent {}

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
      imports: [App, TestRouteComponent],
      providers: [
        provideRouter([
          { path: '', pathMatch: 'full', component: TestRouteComponent },
          { path: 'auth/sign-in', component: TestRouteComponent },
          { path: 'dashboard', component: TestRouteComponent },
          { path: 'patients', component: TestRouteComponent },
        ]),
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

  function createApp(): ReturnType<typeof TestBed.createComponent<App>> {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    return fixture;
  }

  async function navigate(url: string): Promise<void> {
    await TestBed.inject(Router).navigateByUrl(url);
  }

  function redirectResult(account: AccountInfo): AuthenticationResult {
    return { account } as AuthenticationResult;
  }

  it('creates the app', () => {
    expect(createApp().componentInstance).toBeTruthy();
  });

  it('does not hydrate or make an application-user request for public root with a cached account', async () => {
    cachedAccounts = [firstAccount];
    createApp();

    interactionStatus$.next(InteractionStatus.None);
    await navigate('/');

    expect(setActiveAccount).not.toHaveBeenCalled();
    expect(loadCurrentUser).not.toHaveBeenCalled();
    expect(clearCurrentUser).not.toHaveBeenCalled();
  });

  it('does not hydrate the application user on the public sign-in route', async () => {
    activeAccount = firstAccount;
    createApp();

    await navigate('/auth/sign-in');
    interactionStatus$.next(InteractionStatus.None);

    expect(loadCurrentUser).not.toHaveBeenCalled();
  });

  it('hydrates once only after router navigation confirms guarded workspace access', async () => {
    activeAccount = firstAccount;
    createApp();

    await navigate('/dashboard');

    expect(loadCurrentUser).toHaveBeenCalledOnce();
  });

  it('does not duplicate hydration while navigating within the guarded workspace', async () => {
    activeAccount = firstAccount;
    createApp();

    await navigate('/dashboard');
    await navigate('/patients');
    interactionStatus$.next(InteractionStatus.None);

    expect(loadCurrentUser).toHaveBeenCalledOnce();
  });

  it('clears only application-user state when navigating from workspace to the public landing page', async () => {
    activeAccount = firstAccount;
    createApp();

    await navigate('/dashboard');
    await navigate('/');

    expect(clearCurrentUser).toHaveBeenCalledOnce();
    expect(activeAccount).toBe(firstAccount);
    expect(setActiveAccount).not.toHaveBeenCalledWith(null);
  });

  it('preserves redirect completion then hydrates once after dashboard navigation', async () => {
    const fixture = createApp();

    redirectResult$.next(redirectResult(firstAccount));
    await fixture.whenStable();

    expect(activeAccount).toBe(firstAccount);
    expect(TestBed.inject(Router).url).toBe('/dashboard');
    expect(loadCurrentUser).toHaveBeenCalledOnce();
  });

  it('does not select or hydrate when multiple cached accounts exist', async () => {
    cachedAccounts = [firstAccount, secondAccount];
    createApp();

    interactionStatus$.next(InteractionStatus.None);
    await navigate('/dashboard');

    expect(setActiveAccount).not.toHaveBeenCalled();
    expect(loadCurrentUser).not.toHaveBeenCalled();
  });
});
