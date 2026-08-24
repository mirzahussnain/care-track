import {
  MsalGuard,
} from '@azure/msal-angular';

import {
  routes,
} from './app.routes';

describe('app routes', () => {
  it('protects the application shell with MsalGuard', () => {
    const shellRoute =
      routes.find(
        route =>
          route.path === '' &&
          route.loadComponent
      );

    expect(shellRoute).toBeDefined();

    expect(
      shellRoute?.canActivate
    ).toContain(MsalGuard);
  });
});