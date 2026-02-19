import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'clusters',
    loadChildren: () =>
      import('./features/clusters/clusters.routes').then(
        (m) => m.clustersRoutes,
      ),
  },
  {
    path: 'credentials',
    loadChildren: () =>
      import('./features/credentials/credentials.routes').then(
        (m) => m.credentialsRoutes,
      ),
  },
  {
    path: 'hosts',
    loadChildren: () =>
      import('./features/hosts/hosts.routes').then(
        (m) => m.hostsRoutes,
      ),
  },
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
