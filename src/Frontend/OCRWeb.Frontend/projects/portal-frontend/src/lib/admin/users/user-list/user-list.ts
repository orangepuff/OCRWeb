import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { ConfirmDialog, ConfirmDialogData } from '@orangepuff/portal-frontend-shared';
import { TranslatePipe } from '../../../translation/translate.pipe';
import { TranslationService } from '../../../translation/translation.service';
import { UserAdminService } from '../user-admin.service';
import { User } from '../user';
import { UserFormDialog, UserFormDialogData, UserFormDialogResult } from '../user-form-dialog/user-form-dialog';
import { SetPasswordDialog, SetPasswordDialogData } from '../set-password-dialog/set-password-dialog';

const MODULE = 'OrangepuffPortal.Frontend';

@Component({
  selector: 'lib-portal-user-list',
  imports: [MatTableModule, MatButtonModule, MatIconModule, MatDialogModule, RouterModule, TranslatePipe],
  templateUrl: './user-list.html',
  styleUrl: './user-list.scss'
})
export class UserList implements OnInit {
  private readonly userAdminService = inject(UserAdminService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly translationService = inject(TranslationService);

  protected readonly module = MODULE;
  protected readonly users = signal<User[]>([]);
  protected readonly displayedColumns = ['username', 'email', 'displayName', 'isActive', 'template', 'actions'];

  ngOnInit(): void {
    this.reload();
  }

  private reload(): void {
    this.userAdminService.list().subscribe((users) => this.users.set(users));
  }

  private dismissLabel(): string {
    return this.translationService.get('dismiss', 'OrangepuffPortal.Common');
  }

  protected templateName(user: User): string {
    if (user.isTemplateUser) {
      return this.translationService.get('admin.users.templateSuffix', MODULE);
    }
    if (user.parentId === null) {
      return '—';
    }
    return this.users().find((u) => u.id === user.parentId)?.username ?? `#${user.parentId}`;
  }

  protected openAddDialog(): void {
    const data: UserFormDialogData = { templateUsers: this.users().filter((u) => u.isTemplateUser) };

    this.dialog
      .open<UserFormDialog, UserFormDialogData, UserFormDialogResult>(UserFormDialog, { data })
      .afterClosed()
      .subscribe((result) => {
        if (!result) {
          return;
        }

        this.userAdminService
          .add({
            username: result.username,
            email: result.email,
            displayName: result.displayName,
            templateUserId: result.parentId,
            password: result.password
          })
          .subscribe((res) => {
            if (res.success) {
              this.snackBar.open(res.successMessage!, this.dismissLabel());
              this.reload();
            } else {
              this.snackBar.open(`${this.translationService.get('admin.users.msg.addFailed', MODULE)}: ${res.rejectionReason}`, this.dismissLabel());
            }
          });
      });
  }

  protected openSetPasswordDialog(user: User): void {
    const data: SetPasswordDialogData = { username: user.username };

    this.dialog
      .open<SetPasswordDialog, SetPasswordDialogData, string>(SetPasswordDialog, { data })
      .afterClosed()
      .subscribe((newPassword) => {
        if (!newPassword) {
          return;
        }

        this.userAdminService.setPassword(user.id, newPassword).subscribe((res) => {
          if (res.success) {
            this.snackBar.open(this.translationService.getFormatted('admin.users.msg.setPasswordSucceeded', MODULE, user.username), this.dismissLabel());
          } else {
            this.snackBar.open(`${this.translationService.get('admin.users.msg.setPasswordFailed', MODULE)}: ${res.rejectionReason}`, this.dismissLabel());
          }
        });
      });
  }

  protected deleteUser(user: User): void {
    const data: ConfirmDialogData = {
      title: this.translationService.get('admin.users.confirmDelete.title', MODULE),
      message: this.translationService.getFormatted('admin.users.confirmDelete.message', MODULE, user.username),
      confirmLabel: this.translationService.get('delete', 'OrangepuffPortal.Common')
    };

    this.dialog
      .open<ConfirmDialog, ConfirmDialogData, boolean>(ConfirmDialog, { data })
      .afterClosed()
      .subscribe((confirmed) => {
        if (!confirmed) {
          return;
        }

        this.userAdminService.delete(user.id).subscribe((res) => {
          if (res.success) {
            this.snackBar.open(res.successMessage!, this.dismissLabel());
            this.reload();
          } else {
            this.snackBar.open(`${this.translationService.get('admin.users.msg.deleteFailed', MODULE)}: ${res.rejectionReason}`, this.dismissLabel());
          }
        });
      });
  }
}
