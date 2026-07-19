import { Component, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { User } from '../user';

export interface UserFormDialogData {
  user: User | null;
  templateUsers: User[];
}

export interface UserFormDialogResult {
  username: string;
  email: string | null;
  displayName: string | null;
  isTemplateUser: boolean;
  parentId: number | null;
}

@Component({
  selector: 'app-user-form-dialog',
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatCheckboxModule, MatSelectModule],
  templateUrl: './user-form-dialog.html',
  styleUrl: './user-form-dialog.scss'
})
export class UserFormDialog {
  private readonly dialogRef = inject(MatDialogRef<UserFormDialog, UserFormDialogResult>);
  protected readonly data = inject<UserFormDialogData>(MAT_DIALOG_DATA);

  protected readonly isEdit = this.data.user !== null;

  protected readonly form = new FormGroup({
    username: new FormControl(this.data.user?.username ?? '', { nonNullable: true, validators: [Validators.required] }),
    email: new FormControl(this.data.user?.email ?? ''),
    displayName: new FormControl(this.data.user?.displayName ?? ''),
    isTemplateUser: new FormControl(this.data.user?.isTemplateUser ?? false, { nonNullable: true }),
    parentId: new FormControl<number | null>(this.data.user?.parentId ?? null)
  });

  protected save(): void {
    if (this.form.invalid) {
      return;
    }

    const value = this.form.getRawValue();
    this.dialogRef.close({
      username: value.username,
      email: value.email || null,
      displayName: value.displayName || null,
      isTemplateUser: value.isTemplateUser,
      parentId: value.isTemplateUser ? null : value.parentId
    });
  }

  protected cancel(): void {
    this.dialogRef.close();
  }
}
