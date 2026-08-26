import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  inject,
  signal,
} from '@angular/core';

import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { MsalBroadcastService, MsalService } from '@azure/msal-angular';

import { InteractionStatus, RedirectRequest } from '@azure/msal-browser';

import { filter } from 'rxjs/operators';

import { Button } from '../../../../design-system/components';
import { environment } from '../../../../../environments/environment';
import { Router } from '@angular/router';

@Component({
  selector: 'app-sign-in-page',
  standalone: true,
  imports: [Button],
  templateUrl: './sign-in-page.html',
  styleUrl: './sign-in-page.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SignInPage {
  private readonly router = inject(Router);
  private readonly msalService = inject(MsalService);

  private readonly msalBroadcastService = inject(MsalBroadcastService);

  private readonly destroyRef = inject(DestroyRef);

  readonly interactionInProgress = signal(true);

  readonly canSignIn = computed(() => !this.interactionInProgress());

  constructor() {
    this.msalBroadcastService.inProgress$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((status) => {
        const busy = status !== InteractionStatus.None;
        this.interactionInProgress.set(busy);
        if (busy) {
          return;
        }

        const account = this.msalService.instance.getActiveAccount();
        if (account) {
          void this.router.navigate(['/dashboard']);
        }
      });
  }

  signIn(): void {
    if (!this.canSignIn()) {
      return;
    }

    const request: RedirectRequest = {
      scopes: [environment.auth.apiScope],
    };

    this.msalService.loginRedirect(request);
  }
}
