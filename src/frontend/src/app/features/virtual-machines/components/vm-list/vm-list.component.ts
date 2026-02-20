import { Component, inject } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog } from '@angular/material/dialog';
import { VirtualMachinesStore } from '../../store/virtual-machines.store';
import { VirtualMachine, VmState, VmAction } from '@core/models/virtual-machine.model';
import { PageHeaderComponent } from '@shared/components/page-header/page-header.component';
import { BytesPipe } from '@shared/pipes/bytes.pipe';
import { VmStatePipe } from '@shared/pipes/vm-state.pipe';
import {
  ConfirmDialogComponent,
  ConfirmDialogData,
} from '@shared/components/confirm-dialog/confirm-dialog.component';
import { VmCreateDialogComponent } from '../vm-create-dialog/vm-create-dialog.component';
import { VmPropertiesDialogComponent } from '../vm-properties-dialog/vm-properties-dialog.component';

@Component({
  selector: 'app-vm-list',
  templateUrl: './vm-list.component.html',
  imports: [
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    PageHeaderComponent,
    BytesPipe,
    VmStatePipe,
  ],
})
export class VmListComponent {
  readonly store = inject(VirtualMachinesStore);
  private readonly dialog = inject(MatDialog);
  readonly VmState = VmState;

  readonly displayedColumns = [
    'name',
    'hostName',
    'state',
    'cpuCount',
    'memoryBytes',
    'actions',
  ];

  readonly stateColors: Record<string, string> = {
    [VmState.Off]: 'bg-gray-200 text-gray-800',
    [VmState.Running]: 'bg-green-100 text-green-800',
    [VmState.Paused]: 'bg-yellow-100 text-yellow-800',
    [VmState.Saved]: 'bg-blue-100 text-blue-800',
    [VmState.Starting]: 'bg-blue-100 text-blue-800',
    [VmState.Stopping]: 'bg-orange-100 text-orange-800',
    [VmState.Saving]: 'bg-blue-100 text-blue-800',
    [VmState.Pausing]: 'bg-yellow-100 text-yellow-800',
    [VmState.Resuming]: 'bg-blue-100 text-blue-800',
    [VmState.Other]: 'bg-gray-200 text-gray-800',
  };

  readonly actioningVms = new Set<string>();

  stateColor(state: VmState): string {
    return this.stateColors[state] ?? '';
  }

  isTransitional(state: VmState): boolean {
    return [VmState.Starting, VmState.Stopping, VmState.Saving, VmState.Pausing, VmState.Resuming].includes(state);
  }

  openCreateDialog(): void {
    this.dialog.open(VmCreateDialogComponent, { width: '480px' });
  }

  openProperties(vm: VirtualMachine): void {
    this.dialog.open(VmPropertiesDialogComponent, {
      width: '480px',
      data: vm,
    });
  }

  async performAction(id: string, action: VmAction): Promise<void> {
    this.actioningVms.add(id);
    try {
      await this.store.performAction(id, action);
    } finally {
      this.actioningVms.delete(id);
    }
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
}
