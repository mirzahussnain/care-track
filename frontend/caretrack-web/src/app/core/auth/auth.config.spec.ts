import {
  msalInterceptorConfigurationFactory,
} from './auth.config';

import {
  environment,
} from '../../../environments/environment';
import { InteractionType } from '@azure/msal-browser';

describe('msalInterceptorConfigurationFactory', () => {
  it('maps CareTrack API routes to the delegated API scope', () => {
    const config =
      msalInterceptorConfigurationFactory();

    expect(
      config.protectedResourceMap.get(
        `${environment.apiBaseUrl}/*`
      )
    ).toEqual([
      environment.auth.apiScope,
    ]);
  });
  
  it('uses redirect interaction for protected API authentication', () => {
  const config =
    msalInterceptorConfigurationFactory();

  expect(config.interactionType)
    .toBe(InteractionType.Redirect);
});
});