import { Component, inject, signal, computed } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog } from '@angular/material/dialog';
import { FormsModule } from '@angular/forms';
import { VirtualMachinesStore } from '../../store/virtual-machines.store';
import { VirtualMachine, VmState, VmAction } from '@core/models/virtual-machine.model';
import { PageHeaderComponent } from '@shared/components/page-header/page-header.component';
import { StatusBadgeComponent, StatusVariant } from '@shared/components/status-badge/status-badge.component';
import { BytesPipe } from '@shared/pipes/bytes.pipe';
import { VmStatePipe } from '@shared/pipes/vm-state.pipe';
import {
  ConfirmDialogComponent,
  ConfirmDialogData,
} from '@shared/components/confirm-dialog/confirm-dialog.component';
import { VmCreateDialogComponent } from '../vm-create-dialog/vm-create-dialog.component';
import { VmPropertiesDialogComponent } from '../vm-properties-dialog/vm-properties-dialog.component';
import { VmCardComponent } from '../vm-card/vm-card.component';

@Component({
  selector: 'app-vm-list',
  templateUrl: './vm-list.component.html',
  imports: [
    RouterLink,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    FormsModule,
    PageHeaderComponent,
    StatusBadgeComponent,
    BytesPipe,
    VmStatePipe,
    VmCardComponent,
  ],
})
export class VmListComponent {
  readonly store = inject(VirtualMachinesStore);
  private readonly dialog = inject(MatDialog);
  readonly VmState = VmState;

  readonly searchQuery = signal('');
  readonly viewMode = signal<'cards' | 'table'>('cards');
  readonly stateFilter = signal<VmState | 'all'>('all');

  readonly displayedColumns = [
    'name',
    'hostName',
    'state',
    'cpuCount',
    'memoryBytes',
    'actions',
  ];

  readonly actioningVms = new Set<string>();

  readonly filteredVms = computed(() => {
    let vms = this.store.vmEntities();
    const query = this.searchQuery().toLowerCase();
    const filter = this.stateFilter();

    if (query) {
      vms = vms.filter(
        (vm) =>
          vm.name.toLowerCase().includes(query) ||
          vm.hostName.toLowerCase().includes(query),
      );
    }

    if (filter !== 'all') {
      vms = vms.filter((vm) => vm.state === filter);
    }

    return vms;
  });

  readonly stateCounts = computed(() => {
    const vms = this.store.vmEntities();
    return {
      all: vms.length,
      [VmState.Running]: vms.filter((v) => v.state === VmState.Running).length,
      [VmState.Off]: vms.filter((v) => v.state === VmState.Off).length,
      [VmState.Paused]: vms.filter((v) => v.state === VmState.Paused).length,
      [VmState.Saved]: vms.filter((v) => v.state === VmState.Saved).length,
    };
  });

  readonly subtitle = computed(() => {
    const total = this.store.vmCount();
    const running = this.stateCounts()[VmState.Running];
    return `${total} total, ${running} running`;
  });

  stateVariant(state: VmState): StatusVariant {
    const map: Partial<Record<VmState, StatusVariant>> = {
      [VmState.Running]: 'success',
      [VmState.Off]: 'muted',
      [VmState.Paused]: 'warning',
      [VmState.Saved]: 'info',
      [VmState.Starting]: 'info',
      [VmState.Stopping]: 'warning',
      [VmState.Saving]: 'info',
      [VmState.Pausing]: 'warning',
      [VmState.Resuming]: 'info',
    };
    return map[state] ?? 'muted';
  }

  isTransitional(state: VmState): boolean {
    return [VmState.Starting, VmState.Stopping, VmState.Saving, VmState.Pausing, VmState.Resuming].includes(state);
  }

  setStateFilter(filter: VmState | 'all'): void {
    this.stateFilter.set(filter);
  }

  clearFilters(): void {
    this.searchQuery.set('');
    this.stateFilter.set('all');
  }

  openCreateDialog(): void {
    this.dialog.open(VmCreateDialogComponent, { width: '480px' });
  }

  openProperties(vm: VirtualMachine): void {
    this.dialog.open(VmPropertiesDialogComponent, {
      width: '720px',
      data: vm,
    });
  }

  canConsole(vm: VirtualMachine): boolean {
    return [VmState.Running, VmState.Paused, VmState.Saved].includes(vm.state);
  }

  async performAction(id: string, action: VmAction): Promise<void> {
    this.actioningVms.add(id);
    try {
      await this.store.performAction(id, action);
    } finally {
      this.actioningVms.delete(id);
    }
  }

  onCardAction(event: { id: string; action: VmAction }): void {
    this.performAction(event.id, event.action);
  }

  deleteVm(id: string, name: string): void {
    const ref = this.dialog.open<ConfirmDialogComponent, ConfirmDialogData, boolean>(
      ConfirmDialogComponent,
      {
        data: {
          title: 'Delete Virtual Machine',
          message: `Are you sure you want to delete "${name}"? This only removes it from the database, not from the host.`,
        },
      },
    );
    ref.afterClosed().subscribe((confirmed) => {
      if (confirmed) this.store.remove(id);
    });
  }

  onCardDelete(event: { id: string; name: string }): void {
    this.deleteVm(event.id, event.name);
  }
}
