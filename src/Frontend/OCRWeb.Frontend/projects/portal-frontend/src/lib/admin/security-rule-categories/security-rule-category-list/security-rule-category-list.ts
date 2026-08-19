import { Component, OnInit, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { ConfirmDialog, ConfirmDialogData } from '@orangepuff/portal-frontend-shared';
import { TranslatePipe } from '../../../translation/translate.pipe';
import { TranslationService } from '../../../translation/translation.service';
import { SecurityRuleCategoryAdminService } from '../security-rule-category-admin.service';
import { SecurityRuleCategory } from '../security-rule-category';
import {
  SecurityRuleCategoryFormDialog,
  SecurityRuleCategoryFormDialogData,
  SecurityRuleCategoryFormDialogResult
} from '../security-rule-category-form-dialog/security-rule-category-form-dialog';

const MODULE = 'OrangepuffPortal.Frontend';

@Component({
  selector: 'lib-portal-security-rule-category-list',
  imports: [MatTableModule, MatButtonModule, MatIconModule, MatDialogModule, TranslatePipe],
  templateUrl: './security-rule-category-list.html',
  styleUrl: './security-rule-category-list.scss'
})
export class SecurityRuleCategoryList implements OnInit {
  private readonly categoryAdminService = inject(SecurityRuleCategoryAdminService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translationService = inject(TranslationService);

  protected readonly module = MODULE;
  protected readonly categories = signal<SecurityRuleCategory[]>([]);
  protected readonly displayedColumns = ['categoryDesc', 'textCode', 'hidden', 'actions'];

  ngOnInit(): void {
    this.reload();
  }

  private reload(): void {
    this.categoryAdminService.list().subscribe((categories) => this.categories.set(categories));
  }

  private dismissLabel(): string {
    return this.translationService.get('dismiss', 'OrangepuffPortal.Common');
  }

  protected openAddDialog(): void {
    const data: SecurityRuleCategoryFormDialogData = { category: null };

    this.dialog
      .open<SecurityRuleCategoryFormDialog, SecurityRuleCategoryFormDialogData, SecurityRuleCategoryFormDialogResult>(
        SecurityRuleCategoryFormDialog,
        { data }
      )
      .afterClosed()
      .subscribe((result) => {
        if (!result) {
          return;
        }

        this.categoryAdminService.add({ categoryDesc: result.categoryDesc, textCode: result.textCode }).subscribe((res) => {
          if (res.success) {
            this.snackBar.open(res.successMessage!, this.dismissLabel());
            this.reload();
          } else {
            this.snackBar.open(`${this.translationService.get('admin.securityRuleCategories.msg.addFailed', MODULE)}: ${res.rejectionReason}`, this.dismissLabel());
          }
        });
      });
  }

  protected openEditDialog(category: SecurityRuleCategory): void {
    const data: SecurityRuleCategoryFormDialogData = { category };

    this.dialog
      .open<SecurityRuleCategoryFormDialog, SecurityRuleCategoryFormDialogData, SecurityRuleCategoryFormDialogResult>(
        SecurityRuleCategoryFormDialog,
        { data }
      )
      .afterClosed()
      .subscribe((result) => {
        if (!result) {
          return;
        }

        this.categoryAdminService
          .update(category.id, { categoryDesc: result.categoryDesc, textCode: result.textCode, hidden: result.hidden })
          .subscribe((res) => {
            if (res.success) {
              this.snackBar.open(res.successMessage!, this.dismissLabel());
              this.reload();
            } else {
              this.snackBar.open(`${this.translationService.get('admin.securityRuleCategories.msg.updateFailed', MODULE)}: ${res.rejectionReason}`, this.dismissLabel());
            }
          });
      });
  }

  protected deleteCategory(category: SecurityRuleCategory): void {
    const data: ConfirmDialogData = {
      title: this.translationService.get('admin.securityRuleCategories.confirmDelete.title', MODULE),
      message: this.translationService.getFormatted('admin.securityRuleCategories.confirmDelete.message', MODULE, category.categoryDesc),
      confirmLabel: this.translationService.get('delete', 'OrangepuffPortal.Common')
    };

    this.dialog
      .open<ConfirmDialog, ConfirmDialogData, boolean>(ConfirmDialog, { data })
      .afterClosed()
      .subscribe((confirmed) => {
        if (!confirmed) {
          return;
        }

        this.categoryAdminService.delete(category.id).subscribe((res) => {
          if (res.success) {
            this.snackBar.open(res.successMessage!, this.dismissLabel());
            this.reload();
          } else {
            this.snackBar.open(`${this.translationService.get('admin.securityRuleCategories.msg.deleteFailed', MODULE)}: ${res.rejectionReason}`, this.dismissLabel());
          }
        });
      });
  }
}
