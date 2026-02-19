import { Component, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { HyperVHost } from '@core/models/hyperv-host.model';
import { HostStatusPipe } from '@shared/pipes/host-status.pipe';

@Component({
  selector: 'app-host-properties-dialog',
  templateUrl: './host-properties-dialog.component.html',
  imports: [
    DatePipe,
    MatDialogModule,
    MatButtonModule,
    HostStatusPipe,
  ],
})
export class HostPropertiesDialogComponent {
  readonly data = inject<HyperVHost>(MAT_DIALOG_DATA);
}
