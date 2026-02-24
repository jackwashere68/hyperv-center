import { Component, inject, signal, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { ClustersService } from '../../services/clusters.service';
import { ClusterDetail, ClusterStatus } from '@core/models/cluster.model';
import { HostStatus } from '@core/models/hyperv-host.model';
import { StatusBadgeComponent, StatusVariant } from '@shared/components/status-badge/status-badge.component';
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
    StatusBadgeComponent,
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

  clusterStatusVariant(status: ClusterStatus): StatusVariant {
    const map: Record<ClusterStatus, StatusVariant> = {
      [ClusterStatus.Unknown]: 'muted',
      [ClusterStatus.Online]: 'success',
      [ClusterStatus.Degraded]: 'warning',
      [ClusterStatus.Offline]: 'error',
      [ClusterStatus.Error]: 'error',
    };
    return map[status] ?? 'muted';
  }

  hostStatusVariant(status: HostStatus): StatusVariant {
    const map: Record<HostStatus, StatusVariant> = {
      [HostStatus.Unknown]: 'muted',
      [HostStatus.Online]: 'success',
      [HostStatus.Offline]: 'error',
      [HostStatus.Error]: 'warning',
    };
    return map[status] ?? 'muted';
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
