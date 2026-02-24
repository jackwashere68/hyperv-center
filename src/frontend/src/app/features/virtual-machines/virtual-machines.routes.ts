import { Routes } from '@angular/router';
import { VmListComponent } from './components/vm-list/vm-list.component';
import { VmConsoleComponent } from './components/vm-console/vm-console.component';

export const virtualMachinesRoutes: Routes = [
  { path: '', component: VmListComponent },
  { path: ':id/console', component: VmConsoleComponent },
];
