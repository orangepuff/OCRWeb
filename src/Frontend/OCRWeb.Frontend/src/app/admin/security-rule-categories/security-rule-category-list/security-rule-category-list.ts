import { Component, OnInit, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { SecurityRuleCategoryAdminService } from '../security-rule-category-admin.service';
import { SecurityRuleCategory } from '../security-rule-category';
import {
  SecurityRuleCategoryFormDialog,
  SecurityRuleCategoryFormDialogData,
  SecurityRuleCategoryFormDialogResult
} from '../security-rule-category-form-dialog/security-rule-category-form-dialog';

@Component({
  selector: 'app-security-rule-category-list',
  imports: [MatTableModule, MatButtonModule, MatIconModule, MatDialogModule],
  templateUrl: './security-rule-category-list.html',
  styleUrl: './security-rule-category-list.scss'
})
export class SecurityRuleCategoryList implements OnInit {
  private readonly categoryAdminService = inject(SecurityRuleCategoryAdminService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly categories = signal<SecurityRuleCategory[]>([]);
  protected readonly displayedColumns = ['categoryDesc', 'textCode', 'hidden', 'actions'];

  ngOnInit(): void {
    this.reload();
  }

  private reload(): void {
    this.categoryAdminService.list().subscribe((categories) => this.categories.set(categories));
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
            this.reload();
          } else {
            this.snackBar.open(`Could not add category: ${res.rejectionReason}`, 'Dismiss');
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
              this.reload();
            } else {
              this.snackBar.open(`Could not update category: ${res.rejectionReason}`, 'Dismiss');
            }
          });
      });
  }

  protected deleteCategory(category: SecurityRuleCategory): void {
    if (!confirm(`Delete category "${category.categoryDesc}"?`)) {
      return;
    }

    this.categoryAdminService.delete(category.id).subscribe((res) => {
      if (res.success) {
        this.reload();
      } else {
        this.snackBar.open(`Could not delete category: ${res.rejectionReason}`, 'Dismiss');
      }
    });
  }
}
