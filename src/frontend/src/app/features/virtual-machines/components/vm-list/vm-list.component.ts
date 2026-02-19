import { Component, inject } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDialog } from '@angular/material/dialog';
import { VirtualMachinesStore } from '../../store/virtual-machines.store';
import { VirtualMachine, VmState } from '@core/models/virtual-machine.model';
import { PageHeaderComponent } from '@shared/components/page-header/page-header.component';
import { BytesPipe } from '@shared/pipes/bytes.pipe';
import { VmStatePipe } from '@shared/pipes/vm-state.pipe';
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
    PageHeaderComponent,
    BytesPipe,
    VmStatePipe,
  ],
})
export class VmListComponent {
  readonly store = inject(VirtualMachinesStore);
  private readonly dialog = inject(MatDialog);

  readonly displayedColumns = [
    'name',
    'host',
    'state',
    'cpuCount',
    'memoryBytes',
    'actions',
  ];

  readonly stateColors: Record<VmState, string> = {
    [VmState.PoweredOff]: 'bg-gray-200 text-gray-800',
    [VmState.Running]: 'bg-green-100 text-green-800',
    [VmState.Paused]: 'bg-yellow-100 text-yellow-800',
    [VmState.Saved]: 'bg-blue-100 text-blue-800',
  };

  stateColor(state: VmState): string {
    return this.stateColors[state] ?? '';
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
}
