import {
  Injectable,
  computed,
  inject,
  signal,
} from '@angular/core';

import {
  AccountInfo,
} from '@azure/msal-browser';

import {
  MsalService,
} from '@azure/msal-angular';

import {
  environment,
} from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly msalService =
    inject(MsalService);

  private readonly accountState =
    signal<AccountInfo | null>(
      this.msalService.instance.getActiveAccount()
    );

  readonly account =
    this.accountState.asReadonly();

  readonly isAuthenticated =
    computed(
      () => this.account() !== null
    );

  refreshAccount(): void {
    this.accountState.set(
      this.msalService.instance.getActiveAccount()
    );
  }

  signOut(): void {
    this.msalService.logoutRedirect({
      account:
        this.msalService.instance
          .getActiveAccount() ?? undefined,

      postLogoutRedirectUri:
        `${environment.auth.redirectUri}/auth/sign-in`,
    });
  }
}