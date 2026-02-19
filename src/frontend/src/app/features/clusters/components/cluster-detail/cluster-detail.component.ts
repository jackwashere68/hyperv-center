import { Component, inject, signal, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatCardModule } from '@angular/material/card';
import { ClustersService } from '../../services/clusters.service';
import { ClusterDetail, ClusterStatus } from '@core/models/cluster.model';
import { HostStatus } from '@core/models/hyperv-host.model';
import { ClusterStatusPipe } from '@shared/pipes/cluster-status.pipe';
import { HostStatusPipe } from '@shared/pipes/host-status.pipe';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-cluster-detail',
  templateUrl: './cluster-detail.component.html',
  imports: [
    DatePipe,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    MatCardModule,
    ClusterStatusPipe,
    HostStatusPipe,
  ],
})
export class ClusterDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly clustersService = inject(ClustersService);

  readonly cluster = signal<ClusterDetail | null>(null);
  readonly loading = signal(true);

  readonly nodeColumns = ['name', 'hostname', 'credentialName', 'status', 'createdAt'];

  readonly clusterStatusColors: Record<ClusterStatus, string> = {
    [ClusterStatus.Unknown]: 'bg-gray-200 text-gray-800',
    [ClusterStatus.Online]: 'bg-green-100 text-green-800',
    [ClusterStatus.Degraded]: 'bg-yellow-100 text-yellow-800',
    [ClusterStatus.Offline]: 'bg-red-100 text-red-800',
    [ClusterStatus.Error]: 'bg-red-200 text-red-900',
  };

  readonly hostStatusColors: Record<HostStatus, string> = {
    [HostStatus.Unknown]: 'bg-gray-200 text-gray-800',
    [HostStatus.Online]: 'bg-green-100 text-green-800',
    [HostStatus.Offline]: 'bg-red-100 text-red-800',
    [HostStatus.Error]: 'bg-yellow-100 text-yellow-800',
  };

  clusterStatusColor(status: ClusterStatus): string {
    return this.clusterStatusColors[status] ?? '';
  }

  hostStatusColor(status: HostStatus): string {
    return this.hostStatusColors[status] ?? '';
  }

  async ngOnInit(): Promise<void> {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.router.navigate(['/clusters']);
      return;
    }

    try {
      const detail = await firstValueFrom(this.clustersService.getById(id));
      this.cluster.set(detail);
    } catch {
      this.router.navigate(['/clusters']);
    } finally {
      this.loading.set(false);
    }
  }

  goBack(): void {
    this.router.navigate(['/clusters']);
  }
}
