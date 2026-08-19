import { Component, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { TranslatePipe } from '../../../translation/translate.pipe';
import { ConfigTextRow } from '../config-text';

export interface ConfigTextFormDialogData {
  row: ConfigTextRow | null;
}

export interface ConfigTextFormDialogResult {
  sModule: string;
  sTextCode: string;
  sCultureCode: string;
  sTextType: string;
  sText: string;
  sNote: string | null;
}

@Component({
  selector: 'lib-portal-config-text-form-dialog',
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule, TranslatePipe],
  templateUrl: './config-text-form-dialog.html',
  styleUrl: './config-text-form-dialog.scss'
})
export class ConfigTextFormDialog {
  private readonly dialogRef = inject(MatDialogRef<ConfigTextFormDialog, ConfigTextFormDialogResult>);
  protected readonly data = inject<ConfigTextFormDialogData>(MAT_DIALOG_DATA);

  protected readonly module = 'OrangepuffPortal.Frontend';
  protected readonly isEdit = this.data.row !== null;

  protected readonly form = new FormGroup({
    sModule: new FormControl(this.data.row?.sModule ?? '', { nonNullable: true, validators: [Validators.required] }),
    sTextCode: new FormControl(this.data.row?.sTextCode ?? '', { nonNullable: true, validators: [Validators.required] }),
    sCultureCode: new FormControl(this.data.row?.sCultureCode ?? '*', { nonNullable: true, validators: [Validators.required] }),
    sTextType: new FormControl(this.data.row?.sTextType ?? 'lbl', { nonNullable: true, validators: [Validators.required] }),
    sText: new FormControl(this.data.row?.sText ?? '', { nonNullable: true, validators: [Validators.required] }),
    sNote: new FormControl(this.data.row?.sNote ?? '')
  });

  protected save(): void {
    if (this.form.invalid) {
      return;
    }

    const value = this.form.getRawValue();
    this.dialogRef.close({
      sModule: value.sModule,
      sTextCode: value.sTextCode,
      sCultureCode: value.sCultureCode,
      sTextType: value.sTextType,
      sText: value.sText,
      sNote: value.sNote || null
    });
  }

  protected cancel(): void {
    this.dialogRef.close();
  }
}
