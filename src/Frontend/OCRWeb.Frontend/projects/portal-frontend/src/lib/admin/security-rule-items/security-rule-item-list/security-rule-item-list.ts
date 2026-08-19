import { Component, OnInit, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { ConfirmDialog, ConfirmDialogData } from '@orangepuff/portal-frontend-shared';
import { TranslatePipe } from '../../../translation/translate.pipe';
import { TranslationService } from '../../../translation/translation.service';
import { SecurityRuleCategoryAdminService } from '../../security-rule-categories/security-rule-category-admin.service';
import { SecurityRuleCategory } from '../../security-rule-categories/security-rule-category';
import { SecurityRuleItemAdminService } from '../security-rule-item-admin.service';
import { RULE_TYPES, SecurityRuleItem } from '../security-rule-item';
import {
  SecurityRuleItemFormDialog,
  SecurityRuleItemFormDialogData,
  SecurityRuleItemFormDialogResult
} from '../security-rule-item-form-dialog/security-rule-item-form-dialog';

const MODULE = 'OrangepuffPortal.Frontend';

@Component({
  selector: 'lib-portal-security-rule-item-list',
  imports: [MatTableModule, MatButtonModule, MatIconModule, MatDialogModule, TranslatePipe],
  templateUrl: './security-rule-item-list.html',
  styleUrl: './security-rule-item-list.scss'
})
export class SecurityRuleItemList implements OnInit {
  private readonly itemAdminService = inject(SecurityRuleItemAdminService);
  private readonly categoryAdminService = inject(SecurityRuleCategoryAdminService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translationService = inject(TranslationService);

  protected readonly module = MODULE;
  protected readonly items = signal<SecurityRuleItem[]>([]);
  protected readonly categories = signal<SecurityRuleCategory[]>([]);
  protected readonly displayedColumns = ['category', 'code', 'description', 'ruleType', 'sortOrder', 'hidden', 'actions'];

  ngOnInit(): void {
    this.categoryAdminService.list().subscribe((categories) => this.categories.set(categories));
    this.reload();
  }

  private reload(): void {
    this.itemAdminService.list().subscribe((items) => this.items.set(items));
  }

  private dismissLabel(): string {
    return this.translationService.get('dismiss', 'OrangepuffPortal.Common');
  }

  protected categoryName(item: SecurityRuleItem): string {
    return this.categories().find((c) => c.id === item.categoryId)?.categoryDesc ?? `#${item.categoryId}`;
  }

  protected ruleTypeName(item: SecurityRuleItem): string {
    const ruleType = RULE_TYPES.find((rt) => rt.name === item.ruleType);
    return ruleType ? this.translationService.get(ruleType.textCode, MODULE) : item.ruleType;
  }

  protected openAddDialog(): void {
    const data: SecurityRuleItemFormDialogData = { item: null, categories: this.categories() };

    this.dialog
      .open<SecurityRuleItemFormDialog, SecurityRuleItemFormDialogData, SecurityRuleItemFormDialogResult>(SecurityRuleItemFormDialog, { data })
      .afterClosed()
      .subscribe((result) => {
        if (!result) {
          return;
        }

        this.itemAdminService
          .add({
            categoryId: result.categoryId,
            code: result.code,
            description: result.description,
            ruleType: result.ruleType,
            textCode: result.textCode,
            sortOrder: result.sortOrder
          })
          .subscribe((res) => {
            if (res.success) {
              this.snackBar.open(res.successMessage!, this.dismissLabel());
              this.reload();
            } else {
              this.snackBar.open(`${this.translationService.get('admin.securityRuleItems.msg.addFailed', MODULE)}: ${res.rejectionReason}`, this.dismissLabel());
            }
          });
      });
  }

  protected openEditDialog(item: SecurityRuleItem): void {
    const data: SecurityRuleItemFormDialogData = { item, categories: this.categories() };

    this.dialog
      .open<SecurityRuleItemFormDialog, SecurityRuleItemFormDialogData, SecurityRuleItemFormDialogResult>(SecurityRuleItemFormDialog, { data })
      .afterClosed()
      .subscribe((result) => {
        if (!result) {
          return;
        }

        this.itemAdminService
          .update(item.id, {
            categoryId: result.categoryId,
            description: result.description,
            ruleType: result.ruleType,
            textCode: result.textCode,
            sortOrder: result.sortOrder,
            hidden: result.hidden
          })
          .subscribe((res) => {
            if (res.success) {
              this.snackBar.open(res.successMessage!, this.dismissLabel());
              this.reload();
            } else {
              this.snackBar.open(`${this.translationService.get('admin.securityRuleItems.msg.updateFailed', MODULE)}: ${res.rejectionReason}`, this.dismissLabel());
            }
          });
      });
  }

  protected deleteItem(item: SecurityRuleItem): void {
    const data: ConfirmDialogData = {
      title: this.translationService.get('admin.securityRuleItems.confirmDelete.title', MODULE),
      message: this.translationService.getFormatted('admin.securityRuleItems.confirmDelete.message', MODULE, item.code),
      confirmLabel: this.translationService.get('delete', 'OrangepuffPortal.Common')
    };

    this.dialog
      .open<ConfirmDialog, ConfirmDialogData, boolean>(ConfirmDialog, { data })
      .afterClosed()
      .subscribe((confirmed) => {
        if (!confirmed) {
          return;
        }

        this.itemAdminService.delete(item.id).subscribe((res) => {
          if (res.success) {
            this.snackBar.open(res.successMessage!, this.dismissLabel());
            this.reload();
          } else {
            this.snackBar.open(`${this.translationService.get('admin.securityRuleItems.msg.deleteFailed', MODULE)}: ${res.rejectionReason}`, this.dismissLabel());
          }
        });
      });
  }
}
