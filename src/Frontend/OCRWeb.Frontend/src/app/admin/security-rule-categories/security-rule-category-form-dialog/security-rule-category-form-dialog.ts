import { Component, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { SecurityRuleCategory } from '../security-rule-category';

export interface SecurityRuleCategoryFormDialogData {
  category: SecurityRuleCategory | null;
}

export interface SecurityRuleCategoryFormDialogResult {
  categoryDesc: string;
  textCode: string | null;
  hidden: boolean;
}

@Component({
  selector: 'app-security-rule-category-form-dialog',
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatCheckboxModule],
  templateUrl: './security-rule-category-form-dialog.html',
  styleUrl: './security-rule-category-form-dialog.scss'
})
export class SecurityRuleCategoryFormDialog {
  private readonly dialogRef = inject(MatDialogRef<SecurityRuleCategoryFormDialog, SecurityRuleCategoryFormDialogResult>);
  protected readonly data = inject<SecurityRuleCategoryFormDialogData>(MAT_DIALOG_DATA);

  protected readonly isEdit = this.data.category !== null;

  protected readonly form = new FormGroup({
    categoryDesc: new FormControl(this.data.category?.categoryDesc ?? '', { nonNullable: true, validators: [Validators.required] }),
    textCode: new FormControl(this.data.category?.textCode ?? ''),
    hidden: new FormControl(this.data.category?.hidden ?? false, { nonNullable: true })
  });

  protected save(): void {
    if (this.form.invalid) {
      return;
    }

    const value = this.form.getRawValue();
    this.dialogRef.close({ categoryDesc: value.categoryDesc, textCode: value.textCode || null, hidden: value.hidden });
  }

  protected cancel(): void {
    this.dialogRef.close();
  }
}
