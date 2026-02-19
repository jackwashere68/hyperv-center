import { Component, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { VirtualMachine } from '@core/models/virtual-machine.model';
import { VmStatePipe } from '@shared/pipes/vm-state.pipe';
import { BytesPipe } from '@shared/pipes/bytes.pipe';

@Component({
  selector: 'app-vm-properties-dialog',
  templateUrl: './vm-properties-dialog.component.html',
  imports: [
    DatePipe,
    MatDialogModule,
    MatButtonModule,
    VmStatePipe,
    BytesPipe,
  ],
})
export class VmPropertiesDialogComponent {
  readonly data = inject<VirtualMachine>(MAT_DIALOG_DATA);
}
