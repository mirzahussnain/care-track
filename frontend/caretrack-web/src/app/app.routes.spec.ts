import { MsalGuard } from '@azure/msal-angular';

import { routes } from './app.routes';

describe('app routes', () => {
  function getShellRoute() {
    return routes.find((route) => route.path === '' && route.canActivate?.includes(MsalGuard));
  }

  it('registers a public full-match landing route', () => {
    const landingRoute = routes.find((route) => route.path === '' && route.pathMatch === 'full');

    expect(landingRoute?.canActivate).toBeUndefined();
    expect(landingRoute?.loadComponent).toBeDefined();
  });
  it('protects the application shell with MsalGuard', () => {
    const shellRoute = getShellRoute();

    expect(shellRoute).toBeDefined();

    expect(shellRoute?.canActivate).toContain(MsalGuard);
  });

  it('registers all Patients routes inside the protected shell', () => {
    const shellRoute = getShellRoute();
    const patientRoutes = shellRoute?.children?.filter((route) =>
      route.path?.startsWith('patients'),
    );

    expect(patientRoutes?.map((route) => route.path)).toEqual([
      'patients',
      'patients/new',
      'patients/:id/edit',
      'patients/:id',
    ]);
    expect(patientRoutes?.every((route) => route.loadComponent)).toBe(true);
    expect(patientRoutes?.every((route) => route.data?.['areaLabel'] === 'Patients')).toBe(true);
  });

  it('orders static and edit Patients routes before patient detail', () => {
    const shellRoute = getShellRoute();
    const paths = shellRoute?.children?.map((route) => route.path) ?? [];

    expect(paths.indexOf('patients/new')).toBeLessThan(paths.indexOf('patients/:id'));
    expect(paths.indexOf('patients/:id/edit')).toBeLessThan(paths.indexOf('patients/:id'));
  });

  it('registers all Referrals routes inside the protected shell', () => {
    const shellRoute = getShellRoute();
    const referralRoutes = shellRoute?.children?.filter((route) =>
      route.path?.startsWith('referrals'),
    );

    expect(referralRoutes?.map((route) => route.path)).toEqual([
      'referrals',
      'referrals/new',
      'referrals/:id',
    ]);
    expect(referralRoutes?.every((route) => route.loadComponent)).toBe(true);
    expect(referralRoutes?.every((route) => route.data?.['areaLabel'] === 'Referrals')).toBe(true);
  });

  it('orders the static Referrals create route before referral detail', () => {
    const shellRoute = getShellRoute();
    const paths = shellRoute?.children?.map((route) => route.path) ?? [];

    expect(paths.indexOf('referrals/new')).toBeLessThan(paths.indexOf('referrals/:id'));
  });

  it('registers Appointment routes inside the protected shell', () => {
    const shellRoute = getShellRoute();
    const appointmentRoutes = shellRoute?.children?.filter((route) =>
      route.path?.startsWith('appointments'),
    );

    expect(appointmentRoutes?.map((route) => route.path)).toEqual([
      'appointments',
      'appointments/new',
      'appointments/:id',
    ]);
    expect(appointmentRoutes?.every((route) => route.loadComponent)).toBe(true);
    expect(appointmentRoutes?.every((route) => route.data?.['areaLabel'] === 'Appointments')).toBe(
      true,
    );
  });

  it('orders the static Appointment create route before appointment detail', () => {
    const shellRoute = getShellRoute();
    const paths = shellRoute?.children?.map((route) => route.path) ?? [];

    expect(paths.indexOf('appointments/new')).toBeLessThan(paths.indexOf('appointments/:id'));
  });
});
