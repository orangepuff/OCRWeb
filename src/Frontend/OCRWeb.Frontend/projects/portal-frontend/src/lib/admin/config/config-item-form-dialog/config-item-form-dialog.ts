import { Component, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { TranslatePipe } from '../../../translation/translate.pipe';
import { CONFIG_VALUE_TYPES, ConfigItemRow, ConfigSection } from '../config-item';

export interface ConfigItemFormDialogData {
  item: ConfigItemRow | null;
  sections: ConfigSection[];
}

export interface ConfigItemFormDialogResult {
  iSectionId: number;
  sConfigCode: string;
  sConfigName: string;
  sTextCode: string;
  iConfigType: number;
  btShow: boolean;
  btAllowUserEdit: boolean;
  iSortOrder: number | null;
  sDefaultValue: string | null;
  iDefaultValue: number | null;
  nDefaultValue: number | null;
  btDefaultValue: boolean | null;
}

@Component({
  selector: 'lib-portal-config-item-form-dialog',
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatCheckboxModule, MatSelectModule, TranslatePipe],
  templateUrl: './config-item-form-dialog.html',
  styleUrl: './config-item-form-dialog.scss'
})
export class ConfigItemFormDialog {
  private readonly dialogRef = inject(MatDialogRef<ConfigItemFormDialog, ConfigItemFormDialogResult>);
  protected readonly data = inject<ConfigItemFormDialogData>(MAT_DIALOG_DATA);

  protected readonly module = 'OrangepuffPortal.Frontend';
  protected readonly isEdit = this.data.item !== null;
  protected readonly configValueTypes = CONFIG_VALUE_TYPES;

  protected readonly form = new FormGroup({
    iSectionId: new FormControl(this.data.item?.iSectionId ?? this.data.sections[0]?.id ?? null, { validators: [Validators.required] }),
    sConfigCode: new FormControl(this.data.item?.sConfigCode ?? '', { nonNullable: true, validators: [Validators.required] }),
    sConfigName: new FormControl(this.data.item?.sConfigName ?? '', { nonNullable: true, validators: [Validators.required] }),
    sTextCode: new FormControl(this.data.item?.sTextCode ?? '', { nonNullable: true, validators: [Validators.required] }),
    iConfigType: new FormControl(this.data.item?.iConfigType ?? 0, { nonNullable: true, validators: [Validators.required] }),
    btShow: new FormControl(this.data.item?.btShow ?? true, { nonNullable: true }),
    btAllowUserEdit: new FormControl(this.data.item?.btAllowUserEdit ?? false, { nonNullable: true }),
    iSortOrder: new FormControl<number | null>(this.data.item?.iSortOrder ?? null),
    sDefaultValue: new FormControl(this.data.item?.sDefaultValue ?? ''),
    iDefaultValue: new FormControl<number | null>(this.data.item?.iDefaultValue ?? null),
    nDefaultValue: new FormControl<number | null>(this.data.item?.nDefaultValue ?? null),
    btDefaultValue: new FormControl(this.data.item?.btDefaultValue ?? false, { nonNullable: true })
  });

  protected save(): void {
    if (this.form.invalid) {
      return;
    }

    const value = this.form.getRawValue();
    this.dialogRef.close({
      iSectionId: value.iSectionId!,
      sConfigCode: value.sConfigCode,
      sConfigName: value.sConfigName,
      sTextCode: value.sTextCode,
      iConfigType: value.iConfigType,
      btShow: value.btShow,
      btAllowUserEdit: value.btAllowUserEdit,
      iSortOrder: value.iSortOrder,
      sDefaultValue: value.iConfigType === 0 ? value.sDefaultValue || null : null,
      iDefaultValue: value.iConfigType === 1 ? value.iDefaultValue : null,
      nDefaultValue: value.iConfigType === 2 ? value.nDefaultValue : null,
      btDefaultValue: value.iConfigType === 3 ? value.btDefaultValue : null
    });
  }

  protected cancel(): void {
    this.dialogRef.close();
  }
}
