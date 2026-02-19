import { Component, inject } from '@angular/core';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';

export interface ClusterDetectedDialogData {
  clusterName: string;
  nodeHostnames: string[];
}

@Component({
  selector: 'app-cluster-detected-dialog',
  templateUrl: './cluster-detected-dialog.component.html',
  imports: [
    MatDialogModule,
    MatButtonModule,
    MatListModule,
    MatIconModule,
  ],
})
export class ClusterDetectedDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<ClusterDetectedDialogComponent>);
  readonly data = inject<ClusterDetectedDialogData>(MAT_DIALOG_DATA);

  addCluster(): void {
    this.dialogRef.close(true);
  }

  addHostOnly(): void {
    this.dialogRef.close(false);
  }
}
