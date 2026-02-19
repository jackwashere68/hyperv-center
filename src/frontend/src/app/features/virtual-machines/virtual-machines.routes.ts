import { Routes } from '@angular/router';
import { VmListComponent } from './components/vm-list/vm-list.component';

export const virtualMachinesRoutes: Routes = [
  { path: '', component: VmListComponent },
];
