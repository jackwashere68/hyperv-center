import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'virtual-machines',
    loadChildren: () =>
      import('./features/virtual-machines/virtual-machines.routes').then(
        (m) => m.virtualMachinesRoutes,
      ),
  },
  {
    path: '',
    redirectTo: 'virtual-machines',
    pathMatch: 'full',
  },
];
