import { Component, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { TranslatePipe } from '../../../translation/translate.pipe';
import { TranslationService } from '../../../translation/translation.service';

export interface SetPasswordDialogData {
  username: string;
}

@Component({
  selector: 'lib-portal-set-password-dialog',
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule, TranslatePipe],
  templateUrl: './set-password-dialog.html',
  styleUrl: './set-password-dialog.scss'
})
export class SetPasswordDialog {
  private readonly dialogRef = inject(MatDialogRef<SetPasswordDialog, string>);
  private readonly translationService = inject(TranslationService);
  protected readonly data = inject<SetPasswordDialogData>(MAT_DIALOG_DATA);
  protected readonly module = 'OrangepuffPortal.Frontend';
  protected readonly title = this.translationService.getFormatted('admin.users.setPasswordDialog.title', this.module, this.data.username);

  protected readonly form = new FormGroup({
    newPassword: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.minLength(8)] }),
    confirmPassword: new FormControl('', { nonNullable: true, validators: [Validators.required] })
  });

  protected mismatch(): boolean {
    const { newPassword, confirmPassword } = this.form.controls;
    return confirmPassword.value.length > 0 && newPassword.value !== confirmPassword.value;
  }

  protected save(): void {
    if (this.form.invalid || this.mismatch()) {
      return;
    }

    this.dialogRef.close(this.form.controls.newPassword.value);
  }

  protected cancel(): void {
    this.dialogRef.close();
  }
}
