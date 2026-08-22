import {
  BrowserCacheLocation,
  Configuration,
  InteractionType,
  PublicClientApplication,
} from '@azure/msal-browser';

import {
  MSAL_GUARD_CONFIG,
  MSAL_INSTANCE,
  MSAL_INTERCEPTOR_CONFIG,
  MsalGuardConfiguration,
  MsalInterceptorConfiguration,
} from '@azure/msal-angular';

import { environment } from '../../../environments/environment';

const msalConfiguration: Configuration = {
  auth: {
    clientId: environment.auth.clientId,

    authority:
      `https://login.microsoftonline.com/${environment.auth.tenantId}`,

    redirectUri: environment.auth.redirectUri,

    postLogoutRedirectUri:
      environment.auth.redirectUri,
  },

  cache: {
    cacheLocation: BrowserCacheLocation.SessionStorage,
  },
};

export function msalInstanceFactory() {
  return new PublicClientApplication(
    msalConfiguration
  );
}

export function msalGuardConfigurationFactory():
  MsalGuardConfiguration {
  return {
    interactionType: InteractionType.Redirect,

    authRequest: {
      scopes: [
        environment.auth.apiScope,
      ],
    },
  };
}

export function msalInterceptorConfigurationFactory():
  MsalInterceptorConfiguration {

  const protectedResourceMap =
    new Map<string, string[]>();

  protectedResourceMap.set(
    environment.apiBaseUrl,
    [
      environment.auth.apiScope,
    ]
  );

  return {
    interactionType: InteractionType.Redirect,
    protectedResourceMap,
  };
}

export const msalProviders = [
  {
    provide: MSAL_INSTANCE,
    useFactory: msalInstanceFactory,
  },
  {
    provide: MSAL_GUARD_CONFIG,
    useFactory:
      msalGuardConfigurationFactory,
  },
  {
    provide: MSAL_INTERCEPTOR_CONFIG,
    useFactory:
      msalInterceptorConfigurationFactory,
  },
];