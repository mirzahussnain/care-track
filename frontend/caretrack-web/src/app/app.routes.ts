import { Routes } from '@angular/router';
import { MsalGuard } from '@azure/msal-angular';

export const routes: Routes = [
  {
    path: 'auth/sign-in',
    loadComponent: () =>
      import('./core/auth/pages/sign-in-page/sign-in-page').then((module) => module.SignInPage),
  },
  {
    path: 'design-lab',
    loadComponent: () =>
      import('./features/design-lab/pages/design-lab-page/design-lab-page').then(
        (m) => m.DesignLabPage,
      ),
  },
  {
    path: '',
    canActivate: [MsalGuard],
    loadComponent: () => import('./core/layout/app-shell/app-shell').then((m) => m.AppShell),
    children: [
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/dashboard/pages/dashboard-page').then((m) => m.DashboardPage),
        data: {
          areaLabel: 'Dashboard',
        },
      },
      {
        path: 'patients',
        loadComponent: () =>
          import('./features/patients/pages/patients-page/patients-page').then(
            (m) => m.PatientsPage,
          ),
        data: { areaLabel: 'Patients' },
      },
      {
        path: 'patients/new',
        loadComponent: () =>
          import('./features/patients/pages/create-patient-page/create-patient-page').then(
            (m) => m.CreatePatientPage,
          ),
        data: { areaLabel: 'Patients' },
      },
      {
        path: 'patients/:id/edit',
        loadComponent: () =>
          import('./features/patients/pages/edit-patient-page/edit-patient-page').then(
            (m) => m.EditPatientPage,
          ),
        data: { areaLabel: 'Patients' },
      },
      {
        path: 'patients/:id',
        loadComponent: () =>
          import('./features/patients/pages/patient-detail-page/patient-detail-page').then(
            (m) => m.PatientDetailPage,
          ),
        data: { areaLabel: 'Patients' },
      },
      {
        path: 'referrals',
        loadComponent: () =>
          import('./features/referrals/pages/referrals-page/referrals-page').then(
            (m) => m.ReferralsPage,
          ),
        data: { areaLabel: 'Referrals' },
      },
      {
        path: 'referrals/new',
        loadComponent: () =>
          import('./features/referrals/pages/create-referral-page/create-referral-page').then(
            (m) => m.CreateReferralPage,
          ),
        data: { areaLabel: 'Referrals' },
      },
      {
        path: 'referrals/:id',
        loadComponent: () =>
          import('./features/referrals/pages/referral-detail-page/referral-detail-page').then(
            (m) => m.ReferralDetailPage,
          ),
        data: { areaLabel: 'Referrals' },
      },
      {
        path: 'appointments',
        loadComponent: () =>
          import('./features/appointments/pages/appointments-page/appointments-page').then(
            (m) => m.AppointmentsPage,
          ),
        data: { areaLabel: 'Appointments' },
      },
      {
        path: 'appointments/new',
        loadComponent: () =>
          import(
            './features/appointments/pages/create-appointment-page/create-appointment-page'
          ).then((m) => m.CreateAppointmentPage),
        data: { areaLabel: 'Appointments' },
      },
      {
        path: 'appointments/:id',
        loadComponent: () =>
          import(
            './features/appointments/pages/appointment-detail-page/appointment-detail-page'
          ).then((m) => m.AppointmentDetailPage),
        data: { areaLabel: 'Appointments' },
      },
      {
        path: '',
        pathMatch: 'full',
        redirectTo: 'dashboard',
      },
    ],
  },
  {
    path: '**',
    redirectTo: 'dashboard',
  },
];
