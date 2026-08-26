import { Injectable, computed, inject, signal } from '@angular/core';

import { MsalService } from '@azure/msal-angular';

import { environment } from '../../../environments/environment';
import { AuthenticatedUser, AuthLoadStatus, CareTrackRole } from './auth.models';
import { CurrentUserApiService } from './current-user-api.service';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly msalService = inject(MsalService);

  private readonly currentUserApi = inject(CurrentUserApiService);

  private readonly userState = signal<AuthenticatedUser | null>(null);
  private readonly statusState = signal<AuthLoadStatus>('idle');

  private loadedAccountId: string | null = null;
  private pendingAccountId: string | null = null;
  private activeRequestId: number | null = null;
  private nextRequestId = 0;

  readonly currentUser = this.userState.asReadonly();
  readonly status = this.statusState.asReadonly();

  readonly isAuthenticated = computed(() => this.currentUser() !== null);

  readonly isLoading = computed(() => this.status() === 'loading');

  readonly roles = computed(() => this.currentUser()?.roles ?? []);

  loadCurrentUser(): void {
    const account = this.msalService.instance.getActiveAccount();

    if (!account) {
      this.clearCurrentUser();
      return;
    }

    const accountId = account.homeAccountId;

    if (
      this.pendingAccountId === accountId ||
      (this.loadedAccountId === accountId && this.status() === 'ready')
    ) {
      return;
    }

    const requestId = ++this.nextRequestId;

    this.userState.set(null);
    this.loadedAccountId = null;
    this.pendingAccountId = accountId;
    this.activeRequestId = requestId;
    this.statusState.set('loading');

    this.currentUserApi.getCurrentUser().subscribe({
      next: (user) => {
        if (!this.isCurrentRequest(accountId, requestId)) {
          return;
        }

        this.userState.set(user);
        this.loadedAccountId = accountId;
        this.pendingAccountId = null;
        this.activeRequestId = null;
        this.statusState.set('ready');
      },

      error: () => {
        if (!this.isCurrentRequest(accountId, requestId)) {
          return;
        }

        this.userState.set(null);
        this.loadedAccountId = null;
        this.pendingAccountId = null;
        this.activeRequestId = null;
        this.statusState.set('error');
      },
    });
  }

  clearCurrentUser(): void {
    this.userState.set(null);
    this.loadedAccountId = null;
    this.pendingAccountId = null;
    this.activeRequestId = null;
    this.statusState.set('idle');
  }

  hasRole(role: CareTrackRole): boolean {
    return this.roles().includes(role);
  }

  signOut(): void {
    this.clearCurrentUser();
    this.msalService.logoutRedirect({
      account: this.msalService.instance.getActiveAccount() ?? undefined,

      postLogoutRedirectUri: `${environment.auth.redirectUri}/`,
    });
  }

  private isCurrentRequest(accountId: string, requestId: number): boolean {
    return (
      this.activeRequestId === requestId &&
      this.pendingAccountId === accountId &&
      this.msalService.instance.getActiveAccount()?.homeAccountId === accountId
    );
  }
}
