import { Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { ConfirmDialog, ConfirmDialogData } from '@orangepuff/portal-frontend-shared';
import { TranslatePipe } from '../../../translation/translate.pipe';
import { TranslationService } from '../../../translation/translation.service';
import { ConfigTextAdminService } from '../config-text-admin.service';
import { ConfigTextFilter, ConfigTextRow } from '../config-text';
import { ConfigTextFormDialog, ConfigTextFormDialogData, ConfigTextFormDialogResult } from '../config-text-form-dialog/config-text-form-dialog';

const MODULE = 'OrangepuffPortal.Frontend';

@Component({
  selector: 'lib-portal-config-text-list',
  imports: [
    ReactiveFormsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatPaginatorModule,
    TranslatePipe
  ],
  templateUrl: './config-text-list.html',
  styleUrl: './config-text-list.scss'
})
export class ConfigTextList implements OnInit {
  private readonly configTextAdminService = inject(ConfigTextAdminService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translationService = inject(TranslationService);

  protected readonly module = MODULE;
  protected readonly rows = signal<ConfigTextRow[]>([]);
  protected readonly totalCount = signal(0);
  protected readonly displayedColumns = ['sModule', 'sTextCode', 'sCultureCode', 'sTextType', 'sText', 'sNote', 'actions'];
  protected readonly pageSizeOptions = [50, 100, 200, 500];

  private page = 1;
  private pageSize = 50;
  private appliedFilter: ConfigTextFilter = {};

  protected readonly filterForm = new FormGroup({
    module: new FormControl(''),
    textCode: new FormControl(''),
    cultureCode: new FormControl(''),
    textType: new FormControl(''),
    text: new FormControl('')
  });

  ngOnInit(): void {
    this.reload();
  }

  protected applyFilter(): void {
    const value = this.filterForm.getRawValue();
    this.appliedFilter = {
      module: value.module || undefined,
      textCode: value.textCode || undefined,
      cultureCode: value.cultureCode || undefined,
      textType: value.textType || undefined,
      text: value.text || undefined
    };
    this.page = 1;
    this.reload();
  }

  protected clearFilter(): void {
    this.filterForm.reset();
    this.appliedFilter = {};
    this.page = 1;
    this.reload();
  }

  protected onPage(event: PageEvent): void {
    this.page = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.reload();
  }

  private reload(): void {
    this.configTextAdminService.list(this.appliedFilter, this.page, this.pageSize).subscribe((result) => {
      this.rows.set(result.items);
      this.totalCount.set(result.totalCount);
    });
  }

  private dismissLabel(): string {
    return this.translationService.get('dismiss', 'OrangepuffPortal.Common');
  }

  protected openAddDialog(): void {
    const data: ConfigTextFormDialogData = { row: null };

    this.dialog
      .open<ConfigTextFormDialog, ConfigTextFormDialogData, ConfigTextFormDialogResult>(ConfigTextFormDialog, { data })
      .afterClosed()
      .subscribe((result) => {
        if (!result) {
          return;
        }

        this.configTextAdminService.add(result).subscribe((res) => {
          if (res.success) {
            this.snackBar.open(res.successMessage!, this.dismissLabel());
            this.reload();
          } else {
            this.snackBar.open(`${this.translationService.get('admin.configText.msg.addFailed', MODULE)}: ${res.rejectionReason}`, this.dismissLabel());
          }
        });
      });
  }

  protected openEditDialog(row: ConfigTextRow): void {
    const data: ConfigTextFormDialogData = { row };

    this.dialog
      .open<ConfigTextFormDialog, ConfigTextFormDialogData, ConfigTextFormDialogResult>(ConfigTextFormDialog, { data })
      .afterClosed()
      .subscribe((result) => {
        if (!result) {
          return;
        }

        this.configTextAdminService.update(row.id, result).subscribe((res) => {
          if (res.success) {
            this.snackBar.open(res.successMessage!, this.dismissLabel());
            this.reload();
          } else {
            this.snackBar.open(`${this.translationService.get('admin.configText.msg.updateFailed', MODULE)}: ${res.rejectionReason}`, this.dismissLabel());
          }
        });
      });
  }

  protected deleteRow(row: ConfigTextRow): void {
    const data: ConfirmDialogData = {
      title: this.translationService.get('admin.configText.confirmDelete.title', MODULE),
      message: this.translationService.getFormatted(
        'admin.configText.confirmDelete.message',
        MODULE,
        `${row.sModule}/${row.sTextCode} (${row.sCultureCode})`
      ),
      confirmLabel: this.translationService.get('delete', 'OrangepuffPortal.Common')
    };

    this.dialog
      .open<ConfirmDialog, ConfirmDialogData, boolean>(ConfirmDialog, { data })
      .afterClosed()
      .subscribe((confirmed) => {
        if (!confirmed) {
          return;
        }

        this.configTextAdminService.delete(row.id).subscribe((res) => {
          if (res.success) {
            this.snackBar.open(res.successMessage!, this.dismissLabel());
            this.reload();
          } else {
            this.snackBar.open(`${this.translationService.get('admin.configText.msg.deleteFailed', MODULE)}: ${res.rejectionReason}`, this.dismissLabel());
          }
        });
      });
  }
}
