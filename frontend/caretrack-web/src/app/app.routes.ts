import { Routes } from '@angular/router';

export const routes: Routes = [
    {
        path: '',
        loadComponent: () => import('./features/dashboard/pages/dashboard-page').then((m) => m.DashboardPage)
    },
     {
    path: 'design-lab',
    loadComponent: () =>
      import(
        './features/design-lab/pages/design-lab-page/design-lab-page'
      ).then((m) => m.DesignLabPage),
  },
  {
    path: '**',
    redirectTo: '',
  },
];
