import { Component, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { SecurityRuleCategory } from '../../security-rule-categories/security-rule-category';
import { RULE_TYPES, SecurityRuleItem } from '../security-rule-item';

export interface SecurityRuleItemFormDialogData {
  item: SecurityRuleItem | null;
  categories: SecurityRuleCategory[];
}

export interface SecurityRuleItemFormDialogResult {
  categoryId: number;
  code: string;
  description: string;
  ruleType: number;
  textCode: string | null;
  sortOrder: number | null;
  hidden: boolean;
}

@Component({
  selector: 'app-security-rule-item-form-dialog',
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatCheckboxModule, MatSelectModule],
  templateUrl: './security-rule-item-form-dialog.html',
  styleUrl: './security-rule-item-form-dialog.scss'
})
export class SecurityRuleItemFormDialog {
  private readonly dialogRef = inject(MatDialogRef<SecurityRuleItemFormDialog, SecurityRuleItemFormDialogResult>);
  protected readonly data = inject<SecurityRuleItemFormDialogData>(MAT_DIALOG_DATA);

  protected readonly ruleTypes = RULE_TYPES;
  protected readonly isEdit = this.data.item !== null;

  protected readonly form = new FormGroup({
    categoryId: new FormControl(this.data.item?.categoryId ?? this.data.categories[0]?.id ?? null, { validators: [Validators.required] }),
    code: new FormControl(this.data.item?.code ?? '', { nonNullable: true, validators: [Validators.required] }),
    description: new FormControl(this.data.item?.description ?? '', { nonNullable: true, validators: [Validators.required] }),
    ruleType: new FormControl(this.ruleTypeValue(this.data.item?.ruleType), { nonNullable: true, validators: [Validators.required] }),
    textCode: new FormControl(this.data.item?.textCode ?? ''),
    sortOrder: new FormControl<number | null>(this.data.item?.sortOrder ?? null),
    hidden: new FormControl(this.data.item?.hidden ?? false, { nonNullable: true })
  });

  private ruleTypeValue(name: string | undefined): number {
    return this.ruleTypes.find((rt) => rt.name === name)?.value ?? 0;
  }

  protected save(): void {
    if (this.form.invalid) {
      return;
    }

    const value = this.form.getRawValue();
    this.dialogRef.close({
      categoryId: value.categoryId!,
      code: value.code,
      description: value.description,
      ruleType: value.ruleType,
      textCode: value.textCode || null,
      sortOrder: value.sortOrder,
      hidden: value.hidden
    });
  }

  protected cancel(): void {
    this.dialogRef.close();
  }
}
