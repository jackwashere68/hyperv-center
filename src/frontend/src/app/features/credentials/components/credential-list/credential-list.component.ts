import { Component, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDialog } from '@angular/material/dialog';
import { CredentialsStore } from '../../store/credentials.store';
import { PageHeaderComponent } from '@shared/components/page-header/page-header.component';
import { Credential } from '@core/models/credential.model';
import {
  ConfirmDialogComponent,
  ConfirmDialogData,
} from '@shared/components/confirm-dialog/confirm-dialog.component';
import { CredentialCreateDialogComponent } from '../credential-create-dialog/credential-create-dialog.component';
import { CredentialEditDialogComponent } from '../credential-edit-dialog/credential-edit-dialog.component';

@Component({
  selector: 'app-credential-list',
  templateUrl: './credential-list.component.html',
  imports: [
    DatePipe,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    PageHeaderComponent,
  ],
})
export class CredentialListComponent {
  readonly store = inject(CredentialsStore);
  private readonly dialog = inject(MatDialog);

  readonly displayedColumns = ['name', 'username', 'createdAt', 'actions'];

  openCreateDialog(): void {
    this.dialog.open(CredentialCreateDialogComponent, { width: '480px' });
  }

  editCredential(credential: Credential): void {
    this.dialog.open(CredentialEditDialogComponent, {
      width: '480px',
      data: credential,
    });
  }

  deleteCredential(id: string, name: string): void {
    const ref = this.dialog.open<ConfirmDialogComponent, ConfirmDialogData, boolean>(
      ConfirmDialogComponent,
      {
        data: {
          title: 'Delete Credential',
          message: `Are you sure you want to delete "${name}"? This action cannot be undone.`,
        },
      },
    );
    ref.afterClosed().subscribe((confirmed) => {
      if (confirmed) this.store.remove(id);
    });
  }
}
