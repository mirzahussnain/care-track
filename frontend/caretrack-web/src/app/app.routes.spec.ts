import { MsalGuard } from '@azure/msal-angular';

import { routes } from './app.routes';

describe('app routes', () => {
  it('protects the application shell with MsalGuard', () => {
    const shellRoute = routes.find((route) => route.path === '' && route.loadComponent);

    expect(shellRoute).toBeDefined();

    expect(shellRoute?.canActivate).toContain(MsalGuard);
  });

  it('registers all Patients routes inside the protected shell', () => {
    const shellRoute = routes.find((route) => route.path === '' && route.loadComponent);
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
    const shellRoute = routes.find((route) => route.path === '' && route.loadComponent);
    const paths = shellRoute?.children?.map((route) => route.path) ?? [];

    expect(paths.indexOf('patients/new')).toBeLessThan(paths.indexOf('patients/:id'));
    expect(paths.indexOf('patients/:id/edit')).toBeLessThan(paths.indexOf('patients/:id'));
  });

  it('registers all Referrals routes inside the protected shell', () => {
    const shellRoute = routes.find((route) => route.path === '' && route.loadComponent);
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
    const shellRoute = routes.find((route) => route.path === '' && route.loadComponent);
    const paths = shellRoute?.children?.map((route) => route.path) ?? [];

    expect(paths.indexOf('referrals/new')).toBeLessThan(paths.indexOf('referrals/:id'));
  });

  it('registers Appointment routes inside the protected shell', () => {
    const shellRoute = routes.find((route) => route.path === '' && route.loadComponent);
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
    const shellRoute = routes.find((route) => route.path === '' && route.loadComponent);
    const paths = shellRoute?.children?.map((route) => route.path) ?? [];

    expect(paths.indexOf('appointments/new')).toBeLessThan(paths.indexOf('appointments/:id'));
  });
});
