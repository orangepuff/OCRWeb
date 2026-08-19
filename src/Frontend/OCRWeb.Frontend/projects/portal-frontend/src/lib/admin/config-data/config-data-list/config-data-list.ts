import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { ConfirmDialog, ConfirmDialogData } from '@orangepuff/portal-frontend-shared';
import { TranslatePipe } from '../../../translation/translate.pipe';
import { TranslationService } from '../../../translation/translation.service';
import { ConfigDataAdminService } from '../config-data-admin.service';
import { ConfigDataRow, ConfigDataUpsertRequest } from '../config-data';
import { ConfigDataFormDialog, ConfigDataFormDialogData } from '../config-data-form-dialog/config-data-form-dialog';

const MODULE = 'OrangepuffPortal.Frontend';

@Component({
  selector: 'lib-portal-config-data-list',
  imports: [
    ReactiveFormsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    TranslatePipe
  ],
  templateUrl: './config-data-list.html',
  styleUrl: './config-data-list.scss'
})
export class ConfigDataList implements OnInit {
  private readonly service = inject(ConfigDataAdminService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translationService = inject(TranslationService);

  protected readonly module = MODULE;
  protected readonly allRows = signal<ConfigDataRow[]>([]);
  protected readonly keyFilter = new FormControl('');
  protected readonly displayedColumns = ['key', 'value', 'allowEditByScreen', 'description', 'actions'];

  protected readonly rows = computed(() => {
    const filter = this.keyFilter.value?.trim().toLowerCase() ?? '';
    return filter
      ? this.allRows().filter(r => r.key.toLowerCase().includes(filter))
      : this.allRows();
  });

  ngOnInit(): void {
    this.reload();
  }

  private reload(): void {
    this.service.getAll().subscribe(rows => this.allRows.set(rows));
  }

  private dismissLabel(): string {
    return this.translationService.get('dismiss', 'OrangepuffPortal.Common');
  }

  protected openAddDialog(): void {
    const data: ConfigDataFormDialogData = { row: null };

    this.dialog
      .open<ConfigDataFormDialog, ConfigDataFormDialogData, ConfigDataUpsertRequest>(ConfigDataFormDialog, { data })
      .afterClosed()
      .subscribe(result => {
        if (!result) {
          return;
        }

        this.service.create(result).subscribe(res => {
          if (res.success) {
            this.snackBar.open(res.successMessage!, this.dismissLabel());
            this.reload();
          } else {
            this.snackBar.open(
              `${this.translationService.get('admin.configData.msg.addFailed', MODULE)}: ${res.rejectionReason}`,
              this.dismissLabel()
            );
          }
        });
      });
  }

  protected openEditDialog(row: ConfigDataRow): void {
    const data: ConfigDataFormDialogData = { row };

    this.dialog
      .open<ConfigDataFormDialog, ConfigDataFormDialogData, ConfigDataUpsertRequest>(ConfigDataFormDialog, { data })
      .afterClosed()
      .subscribe(result => {
        if (!result) {
          return;
        }

        this.service.update(row.id, result).subscribe(res => {
          if (res.success) {
            this.snackBar.open(res.successMessage!, this.dismissLabel());
            this.reload();
          } else {
            this.snackBar.open(
              `${this.translationService.get('admin.configData.msg.updateFailed', MODULE)}: ${res.rejectionReason}`,
              this.dismissLabel()
            );
          }
        });
      });
  }

  protected deleteRow(row: ConfigDataRow): void {
    const data: ConfirmDialogData = {
      title: this.translationService.get('admin.configData.confirmDelete.title', MODULE),
      message: this.translationService.getFormatted('admin.configData.confirmDelete.message', MODULE, row.key),
      confirmLabel: this.translationService.get('delete', 'OrangepuffPortal.Common')
    };

    this.dialog
      .open<ConfirmDialog, ConfirmDialogData, boolean>(ConfirmDialog, { data })
      .afterClosed()
      .subscribe(confirmed => {
        if (!confirmed) {
          return;
        }

        this.service.delete(row.id).subscribe(res => {
          if (res.success) {
            this.snackBar.open(res.successMessage!, this.dismissLabel());
            this.reload();
          } else {
            this.snackBar.open(
              `${this.translationService.get('admin.configData.msg.deleteFailed', MODULE)}: ${res.rejectionReason}`,
              this.dismissLabel()
            );
          }
        });
      });
  }
}
