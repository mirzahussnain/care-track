import { Component, signal,ChangeDetectionStrategy,DestroyRef,inject } from '@angular/core';
import { RouterOutlet,Router } from '@angular/router';

import {
  MsalBroadcastService,
  MsalService,
} from '@azure/msal-angular';

import {
  InteractionStatus,
} from '@azure/msal-browser';

import {
  filter,
  tap,
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
  private readonly router=inject(Router);
  private readonly authService=inject(AuthService);
   private readonly msalService =
    inject(MsalService);

  private readonly msalBroadcastService =
    inject(MsalBroadcastService);

  private readonly destroyRef =
    inject(DestroyRef);

  constructor() {
    this.msalService
      .handleRedirectObservable({
        navigateToLoginRequestUrl:false,
      })
      .pipe(
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next:result=>{
          if(!result?.account){
            return;
          }

          this.msalService.instance.setActiveAccount(result.account);
          this.msalService.instance.setActiveAccount(result.account);
          this.authService.loadCurrentUser();

          void this.router.navigate(['/dashboard']);
        },
        error:error=>{
          console.error('Authenticate redirect failed',error);
        }
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
        if (
          this.msalService.instance
            .getActiveAccount()
        ) {
          return;
        }

        const accounts =
          this.msalService.instance
            .getAllAccounts();

        if (accounts.length === 1) {
          this.msalService.instance.setActiveAccount(accounts[0]);
          this.authService.loadCurrentUser();
        }
      });
  }
}
