import { Routes } from '@angular/router';
import { ClusterListComponent } from './components/cluster-list/cluster-list.component';
import { ClusterDetailComponent } from './components/cluster-detail/cluster-detail.component';

export const clustersRoutes: Routes = [
  { path: '', component: ClusterListComponent },
  { path: ':id', component: ClusterDetailComponent },
];
