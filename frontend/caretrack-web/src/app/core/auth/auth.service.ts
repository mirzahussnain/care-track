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
import { AuthenticatedUser, CareTrackRole } from './auth.models';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly msalService =inject(MsalService);

  private readonly userState = signal<AuthenticatedUser | null>(null);
  readonly currentUser = this.userState.asReadonly();
    readonly isAuthenticated =computed(
      () => this.currentUser() !== null
    );

  readonly roles = computed(
      () => this.currentUser()?.roles ?? []
    );

  constructor() {
    this.refreshUser();
  }

   refreshUser(): void {
    const account =
      this.msalService.instance
        .getActiveAccount();

    this.userState.set(
      account
        ? this.mapAccount(account)
        : null
    );
  }

  hasRole(role: CareTrackRole): boolean {
    return this.roles().includes(role);
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

   private mapAccount(
    account: AccountInfo
  ): AuthenticatedUser {

    const claims =
      account.idTokenClaims as
        | Record<string, unknown>
        | undefined;

    const roles =
      Array.isArray(claims?.['roles'])
        ? claims['roles']
            .filter(
              (
                role
              ): role is string =>
                typeof role === 'string'
            )
        : [];

    return {
      id:
        account.localAccountId,

      name:
        account.name
        ?? account.username,

      username:
        account.username,

      roles,
    };
  }
}
