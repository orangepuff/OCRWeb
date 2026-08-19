import { Component, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { TranslatePipe } from '../../../translation/translate.pipe';
import { User } from '../user';

export interface UserFormDialogData {
  templateUsers: User[];
}

export interface UserFormDialogResult {
  username: string;
  email: string | null;
  displayName: string | null;
  isTemplateUser: boolean;
  parentId: number | null;
  password: string | null;
}

/** Add-user only — editing an existing user's account now lives on its Settings page. */
@Component({
  selector: 'lib-portal-user-form-dialog',
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatCheckboxModule, MatSelectModule, TranslatePipe],
  templateUrl: './user-form-dialog.html',
  styleUrl: './user-form-dialog.scss'
})
export class UserFormDialog {
  private readonly dialogRef = inject(MatDialogRef<UserFormDialog, UserFormDialogResult>);
  protected readonly data = inject<UserFormDialogData>(MAT_DIALOG_DATA);
  protected readonly module = 'OrangepuffPortal.Frontend';

  protected readonly form = new FormGroup({
    username: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    email: new FormControl(''),
    displayName: new FormControl(''),
    isTemplateUser: new FormControl(false, { nonNullable: true }),
    parentId: new FormControl<number | null>(null),
    password: new FormControl('')
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
      parentId: value.isTemplateUser ? null : value.parentId,
      password: value.password || null
    });
  }

  protected cancel(): void {
    this.dialogRef.close();
  }
}
