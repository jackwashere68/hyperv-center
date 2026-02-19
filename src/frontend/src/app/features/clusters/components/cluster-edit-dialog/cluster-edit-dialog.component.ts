import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { ClustersStore } from '../../store/clusters.store';
import { CredentialsStore } from '@features/credentials/store/credentials.store';
import { Cluster } from '@core/models/cluster.model';

@Component({
  selector: 'app-cluster-edit-dialog',
  templateUrl: './cluster-edit-dialog.component.html',
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
  ],
})
export class ClusterEditDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<ClusterEditDialogComponent>);
  private readonly store = inject(ClustersStore);
  private readonly data = inject<Cluster>(MAT_DIALOG_DATA);
  readonly credentialsStore = inject(CredentialsStore);

  readonly form = this.fb.nonNullable.group({
    name: [this.data.name, [Validators.required, Validators.maxLength(256)]],
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
