import {
  Injectable,
  computed,
  inject,
  signal,
} from '@angular/core';

import {
  MsalService,
} from '@azure/msal-angular';

import {
  environment,
} from '../../../environments/environment';
import { AuthenticatedUser, CareTrackRole } from './auth.models';
import { CurrentUserApiService } from './current-user-api.service';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly msalService =inject(MsalService);
   private readonly currentUserApi =inject(CurrentUserApiService);

  private readonly userState = signal<AuthenticatedUser | null>(null);
  readonly currentUser = this.userState.asReadonly();
    readonly isAuthenticated =computed(
      () => this.currentUser() !== null
    );


  readonly roles= computed(()=>this.currentUser()?.roles??[]);

  loadCurrentUser(): void {
    const account =this.msalService.instance.getActiveAccount();

    if (!account) {
      this.userState.set(null);
      return;
    }

    this.currentUserApi
      .getCurrentUser()
      .subscribe({
        next: user => {
          this.userState.set(user);
        },

        error: () => {
          this.userState.set(null);
        },
      });
  }

    clearCurrentUser(): void {
    this.userState.set(null);
  }
  
  
   hasRole(
    role: CareTrackRole
  ): boolean {
    return this.roles()
      .includes(role);
  }

  signOut(): void {
    this.clearCurrentUser();
    this.msalService.logoutRedirect({
      account:
        this.msalService.instance
          .getActiveAccount() ?? undefined,

      postLogoutRedirectUri:
        `${environment.auth.redirectUri}/auth/sign-in`,
    });
  }
}
