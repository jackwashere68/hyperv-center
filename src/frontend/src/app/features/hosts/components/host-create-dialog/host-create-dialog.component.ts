import { Component, inject } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatDialogModule, MatDialogRef, MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { HostsStore } from '../../store/hosts.store';
import { CredentialsStore } from '@features/credentials/store/credentials.store';
import { ClustersService } from '@features/clusters/services/clusters.service';
import { ClustersStore } from '@features/clusters/store/clusters.store';
import {
  ClusterDetectedDialogComponent,
  ClusterDetectedDialogData,
} from '@shared/components/cluster-detected-dialog/cluster-detected-dialog.component';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-host-create-dialog',
  templateUrl: './host-create-dialog.component.html',
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
  ],
})
export class HostCreateDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(
    MatDialogRef<HostCreateDialogComponent>,
  );
  private readonly dialog = inject(MatDialog);
  private readonly store = inject(HostsStore);
  private readonly clustersService = inject(ClustersService);
  private readonly clustersStore = inject(ClustersStore);
  readonly credentialsStore = inject(CredentialsStore);

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(256)]],
    hostname: ['', [Validators.required, Validators.maxLength(256)]],
    credentialId: ['', [Validators.required]],
    notes: [''],
  });

  async onSubmit(): Promise<void> {
    if (this.form.invalid) return;
    const values = this.form.getRawValue();

    try {
      const detection = await firstValueFrom(
        this.clustersService.detect({
          hostname: values.hostname,
          credentialId: values.credentialId,
        }),
      );

      if (detection) {
        const ref = this.dialog.open<
          ClusterDetectedDialogComponent,
          ClusterDetectedDialogData,
          boolean
        >(ClusterDetectedDialogComponent, {
          width: '480px',
          data: {
            clusterName: detection.clusterName,
            nodeHostnames: detection.nodeHostnames,
          },
        });

        const addCluster = await firstValueFrom(ref.afterClosed());
        if (addCluster) {
          await this.clustersStore.create({
            name: detection.clusterName,
            credentialId: values.credentialId,
            nodeHostnames: detection.nodeHostnames,
          });
          this.dialogRef.close(true);
          return;
        }
      }
    } catch {
      // Detection failed or returned 204 — proceed with normal host creation
    }

    await this.store.create(values);
    this.dialogRef.close(true);
  }
}
