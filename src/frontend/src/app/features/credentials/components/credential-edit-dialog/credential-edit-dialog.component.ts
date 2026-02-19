import { Component, inject, signal } from '@angular/core';
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
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Credential } from '@core/models/credential.model';
import { CredentialsStore } from '../../store/credentials.store';

@Component({
  selector: 'app-credential-edit-dialog',
  templateUrl: './credential-edit-dialog.component.html',
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
  ],
})
export class CredentialEditDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(
    MatDialogRef<CredentialEditDialogComponent>,
  );
  private readonly store = inject(CredentialsStore);
  private readonly data = inject<Credential>(MAT_DIALOG_DATA);

  hidePassword = signal(true);

  readonly form = this.fb.nonNullable.group({
    name: [this.data.name, [Validators.required, Validators.maxLength(256)]],
    username: [
      this.data.username,
      [Validators.required, Validators.maxLength(256)],
    ],
    password: [''],
  });

  togglePasswordVisibility(): void {
    this.hidePassword.update((v) => !v);
  }

  async onSubmit(): Promise<void> {
    if (this.form.invalid) return;

    const values = this.form.getRawValue();
    await this.store.update(this.data.id, {
      id: this.data.id,
      name: values.name,
      username: values.username,
      password: values.password || null,
    });
    this.dialogRef.close(true);
  }
}
