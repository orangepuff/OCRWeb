import { Component, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { TranslatePipe } from '../../../translation/translate.pipe';
import { ConfigSection } from '../config-item';

export interface ConfigSectionFormDialogData {
  section: ConfigSection | null;
}

export interface ConfigSectionFormDialogResult {
  sSectionDesc: string;
  sTextCode: string;
  btShow: boolean;
  iSortOrder: number | null;
}

@Component({
  selector: 'lib-portal-config-section-form-dialog',
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatCheckboxModule, TranslatePipe],
  templateUrl: './config-section-form-dialog.html',
  styleUrl: './config-section-form-dialog.scss'
})
export class ConfigSectionFormDialog {
  private readonly dialogRef = inject(MatDialogRef<ConfigSectionFormDialog, ConfigSectionFormDialogResult>);
  protected readonly data = inject<ConfigSectionFormDialogData>(MAT_DIALOG_DATA);

  protected readonly module = 'OrangepuffPortal.Frontend';
  protected readonly isEdit = this.data.section !== null;

  protected readonly form = new FormGroup({
    sSectionDesc: new FormControl(this.data.section?.sSectionDesc ?? '', { nonNullable: true, validators: [Validators.required] }),
    sTextCode: new FormControl(this.data.section?.sTextCode ?? '', { nonNullable: true, validators: [Validators.required] }),
    btShow: new FormControl(this.data.section?.btShow ?? true, { nonNullable: true }),
    iSortOrder: new FormControl<number | null>(this.data.section?.iSortOrder ?? null)
  });

  protected save(): void {
    if (this.form.invalid) {
      return;
    }

    const value = this.form.getRawValue();
    this.dialogRef.close({
      sSectionDesc: value.sSectionDesc,
      sTextCode: value.sTextCode,
      btShow: value.btShow,
      iSortOrder: value.iSortOrder
    });
  }

  protected cancel(): void {
    this.dialogRef.close();
  }
}
