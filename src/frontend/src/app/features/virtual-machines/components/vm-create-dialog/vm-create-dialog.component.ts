import { Component, inject } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { VirtualMachinesStore } from '../../store/virtual-machines.store';

const GB = 1_073_741_824;

@Component({
  selector: 'app-vm-create-dialog',
  templateUrl: './vm-create-dialog.component.html',
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
  ],
})
export class VmCreateDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<VmCreateDialogComponent>);
  private readonly store = inject(VirtualMachinesStore);

  readonly memoryOptions = Array.from({ length: 32 }, (_, i) => ({
    label: `${i + 1} GB`,
    value: (i + 1) * GB,
  }));

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(256)]],
    host: ['', [Validators.required, Validators.maxLength(256)]],
    cpuCount: [1, [Validators.required, Validators.min(1)]],
    memoryBytes: [GB, [Validators.required, Validators.min(1)]],
    notes: [''],
  });

  async onSubmit(): Promise<void> {
    if (this.form.invalid) return;
    await this.store.create(this.form.getRawValue());
    this.dialogRef.close(true);
  }
}
