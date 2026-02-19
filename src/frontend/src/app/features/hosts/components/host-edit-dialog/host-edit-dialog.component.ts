import { Component, inject } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { HyperVHost } from '@core/models/hyperv-host.model';
import { HostsStore } from '../../store/hosts.store';
import { CredentialsStore } from '@features/credentials/store/credentials.store';

@Component({
  selector: 'app-host-edit-dialog',
  templateUrl: './host-edit-dialog.component.html',
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
  ],
})
export class HostEditDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(
    MatDialogRef<HostEditDialogComponent>,
  );
  private readonly store = inject(HostsStore);
  private readonly data = inject<HyperVHost>(MAT_DIALOG_DATA);
  readonly credentialsStore = inject(CredentialsStore);

  readonly form = this.fb.nonNullable.group({
    name: [this.data.name, [Validators.required, Validators.maxLength(256)]],
    hostname: [
      this.data.hostname,
      [Validators.required, Validators.maxLength(256)],
    ],
    credentialId: [this.data.credentialId, [Validators.required]],
    notes: [this.data.notes ?? ''],
  });

  async onSubmit(): Promise<void> {
    if (this.form.invalid) return;

    const values = this.form.getRawValue();
    await this.store.update(this.data.id, {
      id: this.data.id,
      ...values,
    });
    this.dialogRef.close(true);
  }
}
