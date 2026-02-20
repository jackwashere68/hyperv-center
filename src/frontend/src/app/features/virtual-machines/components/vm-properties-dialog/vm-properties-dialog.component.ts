import { Component, inject, signal, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { VirtualMachine } from '@core/models/virtual-machine.model';
import { VmHardwareInfo } from '@core/models/vm-hardware.model';
import { VmStatePipe } from '@shared/pipes/vm-state.pipe';
import { BytesPipe } from '@shared/pipes/bytes.pipe';
import { UptimePipe } from '@shared/pipes/uptime.pipe';
import { MacAddressPipe } from '@shared/pipes/mac-address.pipe';
import { VirtualMachinesService } from '../../services/virtual-machines.service';

@Component({
  selector: 'app-vm-properties-dialog',
  templateUrl: './vm-properties-dialog.component.html',
  imports: [
    DatePipe,
    MatDialogModule,
    MatButtonModule,
    MatTabsModule,
    MatTableModule,
    MatProgressSpinnerModule,
    VmStatePipe,
    BytesPipe,
    UptimePipe,
    MacAddressPipe,
  ],
})
export class VmPropertiesDialogComponent implements OnInit {
  readonly data = inject<VirtualMachine>(MAT_DIALOG_DATA);
  private readonly vmService = inject(VirtualMachinesService);

  readonly hardware = signal<VmHardwareInfo | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  readonly diskColumns = ['path', 'controller', 'format', 'currentSize', 'maxSize'];
  readonly nicColumns = ['name', 'switch', 'macAddress', 'ipAddresses'];
  readonly snapshotColumns = ['name', 'creationTime', 'parent'];

  ngOnInit(): void {
    this.vmService.getHardware(this.data.id).subscribe({
      next: (hw) => {
        this.hardware.set(hw);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err?.error?.detail ?? 'Failed to load hardware details');
        this.loading.set(false);
      },
    });
  }
}
