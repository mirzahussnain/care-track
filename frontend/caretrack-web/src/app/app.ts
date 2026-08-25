import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  inject,
  signal,
} from '@angular/core';

import {
  Router,
  RouterOutlet,
} from '@angular/router';

import {
  MsalBroadcastService,
  MsalService,
} from '@azure/msal-angular';

import {
  AccountInfo,
  InteractionStatus,
} from '@azure/msal-browser';

import {
  filter,
} from 'rxjs/operators';

import {
  takeUntilDestroyed,
} from '@angular/core/rxjs-interop';
import { AuthService } from './core/auth/auth.service';

@Component({
  imports: [RouterOutlet],
  selector: 'app-root',
   standalone: true,
  styleUrl: './app.css',
  templateUrl: './app.html',
   changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {
  protected readonly title = signal('caretrack-web');

  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);

  private readonly msalService =
    inject(MsalService);

  private readonly msalBroadcastService =
    inject(MsalBroadcastService);

  private readonly destroyRef =
    inject(DestroyRef);

  private lifecycleAccountId: string | null = null;
  private navigatedRedirectAccountId: string | null = null;

  constructor() {
    this.msalService
      .handleRedirectObservable({
        navigateToLoginRequestUrl:false,
      })
      .pipe(
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: result => {
          if (!result?.account) {
            return;
          }

          this.activateAccount(result.account);
          this.loadForAccount(result.account);

          if (
            this.navigatedRedirectAccountId !==
            result.account.homeAccountId
          ) {
            this.navigatedRedirectAccountId =
              result.account.homeAccountId;

            void this.router.navigate(['/dashboard']);
          }
        },
        error: () => undefined,
      });

    this.msalBroadcastService.inProgress$
      .pipe(
        filter(
          status =>
            status === InteractionStatus.None
        ),

        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => {
        let account =
          this.msalService.instance
            .getActiveAccount();

        if (!account) {
          const accounts =
            this.msalService.instance
              .getAllAccounts();

          if (accounts.length === 1) {
            account = accounts[0];
            this.activateAccount(account);
          }
        }

        if (account) {
          this.loadForAccount(account);
          return;
        }

        if (this.lifecycleAccountId !== null) {
          this.lifecycleAccountId = null;
          this.authService.clearCurrentUser();
        }
      });
  }

  private activateAccount(account: AccountInfo): void {
    if (
      this.msalService.instance
        .getActiveAccount()
        ?.homeAccountId === account.homeAccountId
    ) {
      return;
    }

    this.msalService.instance.setActiveAccount(account);
  }

  private loadForAccount(account: AccountInfo): void {
    if (this.lifecycleAccountId === account.homeAccountId) {
      return;
    }

    this.lifecycleAccountId = account.homeAccountId;
    this.authService.loadCurrentUser();
  }
}
