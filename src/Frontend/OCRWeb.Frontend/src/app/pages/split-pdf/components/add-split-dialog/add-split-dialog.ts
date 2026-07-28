import { Component, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { Button } from '@orangepuff/portal-frontend-shared';
import { I18nService } from '../../../../i18n/i18n.service';
import { ISplit } from '../../models/split.model';
import { generateSplitId } from '../../split.utils';

export interface AddSplitDialogData {
  totalPages: number;
}

@Component({
  selector: 'app-add-split-dialog',
  imports: [ReactiveFormsModule, MatDialogModule, Button],
  templateUrl: './add-split-dialog.html',
  styleUrl: './add-split-dialog.scss'
})
export class AddSplitDialog {
  private readonly dialogRef = inject(MatDialogRef<AddSplitDialog>);
  readonly data = inject<AddSplitDialogData>(MAT_DIALOG_DATA);
  protected readonly i18n = inject(I18nService);

  protected readonly nameControl = new FormControl('', { nonNullable: true, validators: [Validators.required] });
  protected readonly fromPageControl = new FormControl(1, {
    nonNullable: true,
    validators: [Validators.required, Validators.min(1)]
  });
  protected readonly toPageControl = new FormControl(1, {
    nonNullable: true,
    validators: [Validators.required, Validators.min(1)]
  });

  protected readonly form = new FormGroup({
    name: this.nameControl,
    fromPage: this.fromPageControl,
    toPage: this.toPageControl
  });

  protected add(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const result: ISplit = {
      id: generateSplitId(),
      bookmarkName: '',
      fromPage: this.fromPageControl.value,
      toPage: this.toPageControl.value,
      name: this.nameControl.value,
      selected: true
    };

    this.dialogRef.close(result);
  }

  protected cancel(): void {
    this.dialogRef.close();
  }
}
