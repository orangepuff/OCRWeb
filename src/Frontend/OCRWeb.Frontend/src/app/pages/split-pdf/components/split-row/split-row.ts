import { Component, computed, inject, input, output } from '@angular/core';
import { CdkDragHandle } from '@angular/cdk/drag-drop';
import { MatCheckbox } from '@angular/material/checkbox';
import { Menu, MenuItem } from '@orangepuff/portal-frontend-shared';
import type { PDFDocumentProxy } from 'pdfjs-dist';
import { I18nService } from '../../../../i18n/i18n.service';
import { ISplit } from '../../models/split.model';
import { PdfPreview } from '../pdf-preview/pdf-preview';

@Component({
  selector: 'app-split-row',
  imports: [CdkDragHandle, MatCheckbox, Menu, PdfPreview],
  templateUrl: './split-row.html',
  styleUrl: './split-row.scss'
})
export class SplitRow {
  readonly split = input.required<ISplit>();
  readonly pdfDoc = input<PDFDocumentProxy | null>(null);
  readonly index = input.required<number>();
  readonly totalPages = input.required<number>();

  readonly splitChange = output<ISplit>();
  readonly delete = output<void>();
  readonly duplicate = output<void>();

  protected readonly i18n = inject(I18nService);

  protected readonly menuItems = computed<MenuItem[]>(() => [
    { id: 'duplicate', label: this.i18n.labels().split.duplicate },
    { id: 'delete', label: this.i18n.labels().split.deleteRow }
  ]);

  protected onMenuAction(itemId: string): void {
    if (itemId === 'delete') {
      this.delete.emit();
    } else if (itemId === 'duplicate') {
      this.duplicate.emit();
    }
  }

  protected onSelectedChange(checked: boolean): void {
    this.splitChange.emit({ ...this.split(), selected: checked });
  }

  protected onFromPageChange(event: Event): void {
    const val = Number((event.target as HTMLInputElement).value);
    if (!isNaN(val) && val >= 1 && val <= this.totalPages()) {
      this.splitChange.emit({ ...this.split(), fromPage: val });
    }
  }

  protected onToPageChange(event: Event): void {
    const val = Number((event.target as HTMLInputElement).value);
    if (!isNaN(val) && val >= 1 && val <= this.totalPages()) {
      this.splitChange.emit({ ...this.split(), toPage: val });
    }
  }

  protected onNameChange(event: Event): void {
    const val = (event.target as HTMLInputElement).value;
    this.splitChange.emit({ ...this.split(), name: val });
  }
}
