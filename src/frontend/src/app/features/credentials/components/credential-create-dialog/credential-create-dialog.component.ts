import { Component, inject, signal } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { CredentialsStore } from '../../store/credentials.store';

@Component({
  selector: 'app-credential-create-dialog',
  templateUrl: './credential-create-dialog.component.html',
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
  ],
})
export class CredentialCreateDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(
    MatDialogRef<CredentialCreateDialogComponent>,
  );
  private readonly store = inject(CredentialsStore);

  hidePassword = signal(true);

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(256)]],
    username: ['', [Validators.required, Validators.maxLength(256)]],
    password: ['', [Validators.required]],
  });

  togglePasswordVisibility(): void {
    this.hidePassword.update((v) => !v);
  }

  async onSubmit(): Promise<void> {
    if (this.form.invalid) return;
    await this.store.create(this.form.getRawValue());
    this.dialogRef.close(true);
  }
}
