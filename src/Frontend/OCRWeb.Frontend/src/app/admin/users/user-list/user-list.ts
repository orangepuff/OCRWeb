import { Component, OnInit, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { UserAdminService } from '../user-admin.service';
import { User } from '../user';
import { UserFormDialog, UserFormDialogData, UserFormDialogResult } from '../user-form-dialog/user-form-dialog';

@Component({
  selector: 'app-user-list',
  imports: [MatTableModule, MatButtonModule, MatIconModule, MatDialogModule],
  templateUrl: './user-list.html',
  styleUrl: './user-list.scss'
})
export class UserList implements OnInit {
  private readonly userAdminService = inject(UserAdminService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly users = signal<User[]>([]);
  protected readonly displayedColumns = ['username', 'email', 'displayName', 'isActive', 'template', 'actions'];

  ngOnInit(): void {
    this.reload();
  }

  private reload(): void {
    this.userAdminService.list().subscribe((users) => this.users.set(users));
  }

  protected templateName(user: User): string {
    if (user.isTemplateUser) {
      return '(is a template)';
    }
    if (user.parentId === null) {
      return '—';
    }
    return this.users().find((u) => u.id === user.parentId)?.username ?? `#${user.parentId}`;
  }

  protected openAddDialog(): void {
    const data: UserFormDialogData = { user: null, templateUsers: this.users().filter((u) => u.isTemplateUser) };

    this.dialog
      .open<UserFormDialog, UserFormDialogData, UserFormDialogResult>(UserFormDialog, { data })
      .afterClosed()
      .subscribe((result) => {
        if (!result) {
          return;
        }

        this.userAdminService
          .add({ username: result.username, email: result.email, displayName: result.displayName, templateUserId: result.parentId })
          .subscribe((res) => {
            if (res.success) {
              this.reload();
            } else {
              this.snackBar.open(`Could not add user: ${res.rejectionReason}`, 'Dismiss');
            }
          });
      });
  }

  protected openEditDialog(user: User): void {
    const data: UserFormDialogData = { user, templateUsers: this.users().filter((u) => u.isTemplateUser) };

    this.dialog
      .open<UserFormDialog, UserFormDialogData, UserFormDialogResult>(UserFormDialog, { data })
      .afterClosed()
      .subscribe((result) => {
        if (!result) {
          return;
        }

        this.userAdminService
          .update(user.id, {
            email: result.email,
            displayName: result.displayName,
            isTemplateUser: result.isTemplateUser,
            parentId: result.parentId
          })
          .subscribe((res) => {
            if (res.success) {
              this.reload();
            } else {
              this.snackBar.open(`Could not update user: ${res.rejectionReason}`, 'Dismiss');
            }
          });
      });
  }

  protected deleteUser(user: User): void {
    if (!confirm(`Delete user "${user.username}"?`)) {
      return;
    }

    this.userAdminService.delete(user.id).subscribe((res) => {
      if (res.success) {
        this.reload();
      } else {
        this.snackBar.open(`Could not delete user: ${res.rejectionReason}`, 'Dismiss');
      }
    });
  }
}
