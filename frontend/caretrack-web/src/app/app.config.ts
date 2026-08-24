import { ApplicationConfig, provideBrowserGlobalErrorListeners,provideAppInitializer } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { provideHttpClient } from '@angular/common/http';
import {MsalBroadcastService,MsalGuard,MsalService} from '@azure/msal-angular';
import {msalProviders} from './core/auth/auth.config';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(),
     ...msalProviders,
    MsalService,
    MsalGuard,
    MsalBroadcastService,
  ]
};
