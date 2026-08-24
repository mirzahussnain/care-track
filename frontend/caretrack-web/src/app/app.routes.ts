import { Routes } from '@angular/router';

export const routes: Routes = [
    {
  path: 'auth/sign-in',
  loadComponent: () =>
    import(
      './core/auth/pages/sign-in-page/sign-in-page'
    ).then(
      module => module.SignInPage
    ),
},
   {
    path: 'design-lab',
    loadComponent: () =>
      import(
        './features/design-lab/pages/design-lab-page/design-lab-page'
      ).then((m) => m.DesignLabPage),
  },
  {
    path: '',
    loadComponent: () =>
      import('./core/layout/app-shell/app-shell')
        .then((m) => m.AppShell),
    children: [
      {
        path: 'dashboard',
        loadComponent: () =>
          import(
            './features/dashboard/pages/dashboard-page'
          ).then((m) => m.DashboardPage),
        data:{
            areaLabel:'Dashboard',
        },
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
