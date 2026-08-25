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
