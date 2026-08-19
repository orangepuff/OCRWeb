import { Component, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { TranslatePipe } from '../../../translation/translate.pipe';
import { ConfigDataRow, ConfigDataUpsertRequest } from '../config-data';

export interface ConfigDataFormDialogData {
  row: ConfigDataRow | null;
}

@Component({
  selector: 'lib-portal-config-data-form-dialog',
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatCheckboxModule, TranslatePipe],
  templateUrl: './config-data-form-dialog.html',
  styleUrl: './config-data-form-dialog.scss'
})
export class ConfigDataFormDialog {
  private readonly dialogRef = inject(MatDialogRef<ConfigDataFormDialog, ConfigDataUpsertRequest>);
  protected readonly data = inject<ConfigDataFormDialogData>(MAT_DIALOG_DATA);

  protected readonly module = 'OrangepuffPortal.Frontend';
  protected readonly isEdit = this.data.row !== null;

  protected readonly form = new FormGroup({
    sKey: new FormControl(this.data.row?.key ?? '', { nonNullable: true, validators: [Validators.required] }),
    sValue: new FormControl(this.data.row?.value ?? ''),
    bAllowEditByScreen: new FormControl(this.data.row?.allowEditByScreen ?? false, { nonNullable: true }),
    sDescription: new FormControl(this.data.row?.description ?? '')
  });

  protected save(): void {
    if (this.form.invalid) {
      return;
    }

    const value = this.form.getRawValue();
    this.dialogRef.close({
      sKey: value.sKey,
      sValue: value.sValue || null,
      bAllowEditByScreen: value.bAllowEditByScreen,
      sDescription: value.sDescription || null
    });
  }

  protected cancel(): void {
    this.dialogRef.close();
  }
}
