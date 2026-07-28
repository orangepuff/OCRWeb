import { Component, inject, input, output } from '@angular/core';
import { CdkDrag, CdkDragDrop, CdkDragPlaceholder, CdkDropList, moveItemInArray } from '@angular/cdk/drag-drop';
import { MatCheckbox } from '@angular/material/checkbox';
import type { PDFDocumentProxy } from 'pdfjs-dist';
import { I18nService } from '../../../../i18n/i18n.service';
import { ISplit } from '../../models/split.model';
import { SplitRow } from '../split-row/split-row';

@Component({
  selector: 'app-split-list',
  imports: [CdkDropList, CdkDrag, CdkDragPlaceholder, MatCheckbox, SplitRow],
  templateUrl: './split-list.html',
  styleUrl: './split-list.scss'
})
export class SplitList {
  readonly splits = input.required<ISplit[]>();
  readonly pdfDoc = input<PDFDocumentProxy | null>(null);
  readonly totalPages = input.required<number>();
  readonly allSelected = input.required<boolean>();
  readonly someSelected = input.required<boolean>();

  readonly splitsChange = output<ISplit[]>();
  readonly updateSplit = output<ISplit>();
  readonly deleteSplit = output<string>();
  readonly duplicateSplit = output<string>();
  readonly selectAllChange = output<boolean>();

  protected readonly i18n = inject(I18nService);

  protected onDrop(event: CdkDragDrop<ISplit[]>): void {
    const list = [...this.splits()];
    moveItemInArray(list, event.previousIndex, event.currentIndex);
    this.splitsChange.emit(list);
  }
}
