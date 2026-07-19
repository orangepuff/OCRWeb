import { Component, OnInit, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { ConfirmDialog, ConfirmDialogData } from 'ocrweb.frontend.shared';
import { SecurityRuleCategoryAdminService } from '../../security-rule-categories/security-rule-category-admin.service';
import { SecurityRuleCategory } from '../../security-rule-categories/security-rule-category';
import { SecurityRuleItemAdminService } from '../security-rule-item-admin.service';
import { SecurityRuleItem } from '../security-rule-item';
import {
  SecurityRuleItemFormDialog,
  SecurityRuleItemFormDialogData,
  SecurityRuleItemFormDialogResult
} from '../security-rule-item-form-dialog/security-rule-item-form-dialog';

@Component({
  selector: 'app-security-rule-item-list',
  imports: [MatTableModule, MatButtonModule, MatIconModule, MatDialogModule],
  templateUrl: './security-rule-item-list.html',
  styleUrl: './security-rule-item-list.scss'
})
export class SecurityRuleItemList implements OnInit {
  private readonly itemAdminService = inject(SecurityRuleItemAdminService);
  private readonly categoryAdminService = inject(SecurityRuleCategoryAdminService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

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

  protected categoryName(item: SecurityRuleItem): string {
    return this.categories().find((c) => c.id === item.categoryId)?.categoryDesc ?? `#${item.categoryId}`;
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
              this.reload();
            } else {
              this.snackBar.open(`Could not add rule item: ${res.rejectionReason}`, 'Dismiss');
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
              this.reload();
            } else {
              this.snackBar.open(`Could not update rule item: ${res.rejectionReason}`, 'Dismiss');
            }
          });
      });
  }

  protected deleteItem(item: SecurityRuleItem): void {
    const data: ConfirmDialogData = { title: 'Delete rule item', message: `Delete rule item "${item.code}"?`, confirmLabel: 'Delete' };

    this.dialog
      .open<ConfirmDialog, ConfirmDialogData, boolean>(ConfirmDialog, { data })
      .afterClosed()
      .subscribe((confirmed) => {
        if (!confirmed) {
          return;
        }

        this.itemAdminService.delete(item.id).subscribe((res) => {
          if (res.success) {
            this.reload();
          } else {
            this.snackBar.open(`Could not delete rule item: ${res.rejectionReason}`, 'Dismiss');
          }
        });
      });
  }
}
