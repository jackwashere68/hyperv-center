import { Component, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDialog } from '@angular/material/dialog';
import { ClustersStore } from '../../store/clusters.store';
import { Cluster, ClusterStatus } from '@core/models/cluster.model';
import { PageHeaderComponent } from '@shared/components/page-header/page-header.component';
import { ClusterStatusPipe } from '@shared/pipes/cluster-status.pipe';
import {
  ConfirmDialogComponent,
  ConfirmDialogData,
} from '@shared/components/confirm-dialog/confirm-dialog.component';
import { ClusterCreateDialogComponent } from '../cluster-create-dialog/cluster-create-dialog.component';
import { ClusterEditDialogComponent } from '../cluster-edit-dialog/cluster-edit-dialog.component';

@Component({
  selector: 'app-cluster-list',
  templateUrl: './cluster-list.component.html',
  imports: [
    DatePipe,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    PageHeaderComponent,
    ClusterStatusPipe,
  ],
})
export class ClusterListComponent {
  readonly store = inject(ClustersStore);
  private readonly dialog = inject(MatDialog);
  private readonly router = inject(Router);

  readonly displayedColumns = [
    'name',
    'credentialName',
    'status',
    'nodeCount',
    'createdAt',
    'actions',
  ];

  readonly statusColors: Record<ClusterStatus, string> = {
    [ClusterStatus.Unknown]: 'bg-gray-200 text-gray-800',
    [ClusterStatus.Online]: 'bg-green-100 text-green-800',
    [ClusterStatus.Degraded]: 'bg-yellow-100 text-yellow-800',
    [ClusterStatus.Offline]: 'bg-red-100 text-red-800',
    [ClusterStatus.Error]: 'bg-red-200 text-red-900',
  };

  statusColor(status: ClusterStatus): string {
    return this.statusColors[status] ?? '';
  }

  openCreateDialog(): void {
    this.dialog.open(ClusterCreateDialogComponent, { width: '480px' });
  }

  openProperties(cluster: Cluster): void {
    this.router.navigate(['/clusters', cluster.id]);
  }

  editCluster(cluster: Cluster): void {
    this.dialog.open(ClusterEditDialogComponent, {
      width: '480px',
      data: cluster,
    });
  }

  deleteCluster(id: string, name: string): void {
    const ref = this.dialog.open<ConfirmDialogComponent, ConfirmDialogData, boolean>(
      ConfirmDialogComponent,
      {
        data: {
          title: 'Delete Cluster',
          message: `Are you sure you want to delete "${name}"? Nodes will be kept as standalone hosts.`,
        },
      },
    );
    ref.afterClosed().subscribe((confirmed) => {
      if (confirmed) this.store.remove(id);
    });
  }
}
