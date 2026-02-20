import { Component, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog } from '@angular/material/dialog';
import { HostsStore } from '../../store/hosts.store';
import { HyperVHost, HostStatus } from '@core/models/hyperv-host.model';
import { PageHeaderComponent } from '@shared/components/page-header/page-header.component';
import { HostStatusPipe } from '@shared/pipes/host-status.pipe';
import {
  ConfirmDialogComponent,
  ConfirmDialogData,
} from '@shared/components/confirm-dialog/confirm-dialog.component';
import { HostCreateDialogComponent } from '../host-create-dialog/host-create-dialog.component';
import { HostEditDialogComponent } from '../host-edit-dialog/host-edit-dialog.component';
import { HostPropertiesDialogComponent } from '../host-properties-dialog/host-properties-dialog.component';

@Component({
  selector: 'app-host-list',
  templateUrl: './host-list.component.html',
  imports: [
    DatePipe,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    PageHeaderComponent,
    HostStatusPipe,
  ],
})
export class HostListComponent {
  readonly store = inject(HostsStore);
  private readonly dialog = inject(MatDialog);

  readonly displayedColumns = [
    'name',
    'hostname',
    'credentialName',
    'clusterName',
    'status',
    'createdAt',
    'actions',
  ];

  readonly statusColors: Record<HostStatus, string> = {
    [HostStatus.Unknown]: 'bg-gray-200 text-gray-800',
    [HostStatus.Online]: 'bg-green-100 text-green-800',
    [HostStatus.Offline]: 'bg-red-100 text-red-800',
    [HostStatus.Error]: 'bg-yellow-100 text-yellow-800',
  };

  readonly syncingHosts = new Set<string>();

  statusColor(status: HostStatus): string {
    return this.statusColors[status] ?? '';
  }

  openCreateDialog(): void {
    this.dialog.open(HostCreateDialogComponent, { width: '480px' });
  }

  openProperties(host: HyperVHost): void {
    this.dialog.open(HostPropertiesDialogComponent, {
      width: '480px',
      data: host,
    });
  }

  editHost(host: HyperVHost): void {
    this.dialog.open(HostEditDialogComponent, {
      width: '480px',
      data: host,
    });
  }

  async syncHost(id: string): Promise<void> {
    this.syncingHosts.add(id);
    try {
      await this.store.sync(id);
      await this.store.loadAll();
    } finally {
      this.syncingHosts.delete(id);
    }
  }

  deleteHost(id: string, name: string): void {
    const ref = this.dialog.open<ConfirmDialogComponent, ConfirmDialogData, boolean>(
      ConfirmDialogComponent,
      {
        data: {
          title: 'Delete Host',
          message: `Are you sure you want to delete "${name}"? This action cannot be undone.`,
        },
      },
    );
    ref.afterClosed().subscribe((confirmed) => {
      if (confirmed) this.store.remove(id);
    });
  }
}
