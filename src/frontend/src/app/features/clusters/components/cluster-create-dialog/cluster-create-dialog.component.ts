import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { ClustersStore } from '../../store/clusters.store';
import { CredentialsStore } from '@features/credentials/store/credentials.store';

@Component({
  selector: 'app-cluster-create-dialog',
  templateUrl: './cluster-create-dialog.component.html',
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
  ],
})
export class ClusterCreateDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<ClusterCreateDialogComponent>);
  private readonly store = inject(ClustersStore);
  readonly credentialsStore = inject(CredentialsStore);

  readonly nodeHostnames = signal<string[]>([]);
  readonly hostnameInput = signal('');

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(256)]],
    credentialId: ['', [Validators.required]],
    notes: [''],
  });

  addHostname(): void {
    const value = this.hostnameInput().trim();
    if (value && !this.nodeHostnames().includes(value)) {
      this.nodeHostnames.update((list) => [...list, value]);
      this.hostnameInput.set('');
    }
  }

  removeHostname(hostname: string): void {
    this.nodeHostnames.update((list) => list.filter((h) => h !== hostname));
  }

  onHostnameKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter') {
      event.preventDefault();
      this.addHostname();
    }
  }

  get canSubmit(): boolean {
    return this.form.valid && this.nodeHostnames().length > 0;
  }

  async onSubmit(): Promise<void> {
    if (!this.canSubmit) return;
    const values = this.form.getRawValue();
    await this.store.create({
      ...values,
      nodeHostnames: this.nodeHostnames(),
    });
    this.dialogRef.close(true);
  }
}
