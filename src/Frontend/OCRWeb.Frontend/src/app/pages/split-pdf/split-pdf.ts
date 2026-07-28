import { HttpClient, HttpErrorResponse, HttpEventType, HttpResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatMenu, MatMenuItem, MatMenuTrigger } from '@angular/material/menu';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { Button, ConfirmDialog } from '@orangepuff/portal-frontend-shared';
import * as pdfjsLib from 'pdfjs-dist';
import type { PDFDocumentProxy } from 'pdfjs-dist';
import { firstValueFrom, filter, map, tap } from 'rxjs';
import { I18nService } from '../../i18n/i18n.service';
import { PdfFileListItem } from '../../models/pdf-file-list-item';
import { FileCacheService } from '../../services/file-cache.service';
import { PdfService } from '../../services/pdf.service';
import { SNACK_DURATION_MS } from '../../ui-config';
import { AddSplitDialog, AddSplitDialogData } from './components/add-split-dialog/add-split-dialog';
import { SplitList } from './components/split-list/split-list';
import { IPdfDocument } from './models/pdf-document.model';
import { ISplit } from './models/split.model';
import { PDF_CACHE_SCOPE } from './split.constants';
import { extractSplitsFromOutline, formatBytes, generateSplitId } from './split.utils';

pdfjsLib.GlobalWorkerOptions.workerSrc = '/pdf.worker.min.mjs';

@Component({
  selector: 'app-split-pdf',
  imports: [Button, MatProgressSpinnerModule, MatMenu, MatMenuTrigger, MatMenuItem, SplitList],
  templateUrl: './split-pdf.html',
  styleUrl: './split-pdf.scss'
})
export class SplitPdf implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly pdfService = inject(PdfService);
  private readonly fileCache = inject(FileCacheService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly i18n = inject(I18nService);
  protected readonly loaded = signal(false);
  protected readonly loadError = signal(false);
  protected readonly loadingStep = signal('');
  protected readonly submitting = signal(false);
  protected readonly errorText = signal<string | null>(null);
  protected readonly downloadProgress = signal<{ loaded: number; total: number | null } | null>(null);

  protected readonly pdfDoc = signal<PDFDocumentProxy | null>(null);
  protected readonly pdfInfo = signal<IPdfDocument | null>(null);
  protected readonly splits = signal<ISplit[]>([]);

  // Snapshot taken on first load — used to restore on Reset.
  private originalSplits: ISplit[] = [];

  protected readonly allSelected = computed(() => {
    const list = this.splits();
    return list.length > 0 && list.every((s) => s.selected);
  });
  protected readonly someSelected = computed(() => this.splits().some((s) => s.selected) && !this.allSelected());
  protected readonly selectedCount = computed(() => this.splits().filter((s) => s.selected).length);
  protected readonly totalSplitPages = computed(() =>
    this.splits()
      .filter((s) => s.selected)
      .reduce((sum, s) => sum + Math.max(0, s.toPage - s.fromPage + 1), 0)
  );

  private projectId!: number;
  private sourceFileId!: number;

  protected readonly formatBytes = formatBytes;

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (!idParam) {
      this.router.navigate(['/home']);
      return;
    }
    this.projectId = Number(idParam);
    this.loadingStep.set(this.i18n.labels().split.loadingFiles);

    this.pdfService.list(this.projectId).subscribe({
      next: async (files) => {
        const file = files[0] ?? null;
        if (!file) {
          this.loadError.set(true);
          this.errorText.set(this.i18n.messages().split.noFileError);
          this.loaded.set(true);
          return;
        }
        this.sourceFileId = file.id;
        await this.loadPdf(file);
      },
      error: () => {
        this.loadError.set(true);
        this.errorText.set(this.i18n.messages().split.loadError);
        this.loaded.set(true);
      }
    });
  }

  private async loadPdf(file: PdfFileListItem): Promise<void> {
    try {
      this.loadingStep.set(this.i18n.labels().split.downloadingPdf);
      let bytes = await this.fileCache.get(PDF_CACHE_SCOPE, file.id);
      if (!bytes) {
        bytes = await this.downloadPdf(file.id, file.sizeBytes);
        void this.fileCache.put(PDF_CACHE_SCOPE, file.id, bytes);
      }

      this.loadingStep.set(this.i18n.labels().split.renderingPreview);
      const doc = await pdfjsLib.getDocument({ data: new Uint8Array(bytes) }).promise;
      this.pdfDoc.set(doc);

      const outline = await doc.getOutline();
      const bookmarkCount = outline?.length ?? 0;

      this.pdfInfo.set({
        id: file.id,
        fileName: file.fileName,
        sizeBytes: file.sizeBytes,
        totalPages: doc.numPages,
        bookmarkCount
      });

      const splits = await extractSplitsFromOutline(doc, outline ?? []);
      this.splits.set(splits);
      this.originalSplits = splits;
      this.loaded.set(true);
    } catch (err) {
      this.loadError.set(true);
      this.errorText.set(this.describeError(err));
      this.loaded.set(true);
    }
  }

  private async downloadPdf(fileId: number, sizeBytes: number): Promise<ArrayBuffer> {
    this.downloadProgress.set({ loaded: 0, total: sizeBytes > 0 ? sizeBytes : null });
    const bytes = await firstValueFrom(
      this.http
        .get(this.pdfService.contentUrl(fileId), { responseType: 'arraybuffer', reportProgress: true, observe: 'events' })
        .pipe(
          tap((event) => {
            if (event.type === HttpEventType.DownloadProgress) {
              this.downloadProgress.update((prev) => ({ loaded: event.loaded, total: event.total ?? prev?.total ?? null }));
            }
          }),
          filter((event): event is HttpResponse<ArrayBuffer> => event.type === HttpEventType.Response),
          map((event) => event.body!)
        )
    );
    this.downloadProgress.set(null);
    return bytes;
  }

  private describeError(err: unknown): string {
    const base = this.i18n.messages().split.loadError;
    const detail =
      err instanceof HttpErrorResponse
        ? `${err.status} ${err.statusText}`.trim()
        : err instanceof Error
          ? err.message
          : undefined;
    return detail ? `${base}: ${detail}` : base;
  }

  protected back(): void {
    this.router.navigate(['/home']);
  }

  protected openPdf(): void {
    window.open(this.pdfService.contentUrl(this.sourceFileId), '_blank', 'noopener');
  }

  protected onUpdateSplit(updated: ISplit): void {
    this.splits.update((list) => list.map((s) => (s.id === updated.id ? updated : s)));
  }

  protected onDeleteSplit(id: string): void {
    this.splits.update((list) => list.filter((s) => s.id !== id));
  }

  protected onDuplicateSplit(id: string): void {
    this.splits.update((list) => {
      const idx = list.findIndex((s) => s.id === id);
      if (idx === -1) {
        return list;
      }
      const copy: ISplit = { ...list[idx], id: generateSplitId(), name: `${list[idx].name} (copy)` };
      return [...list.slice(0, idx + 1), copy, ...list.slice(idx + 1)];
    });
  }

  protected onSelectAllChange(checked: boolean): void {
    this.splits.update((list) => list.map((s) => ({ ...s, selected: checked })));
  }

  protected openAddDialog(): void {
    const info = this.pdfInfo();
    if (!info) {
      return;
    }
    const data: AddSplitDialogData = { totalPages: info.totalPages };
    this.dialog
      .open(AddSplitDialog, { data, width: '420px' })
      .afterClosed()
      .subscribe((result: ISplit | undefined) => {
        if (result) {
          this.splits.update((list) => [...list, result]);
        }
      });
  }

  protected resetSplits(): void {
    const ref = this.dialog.open(ConfirmDialog, {
      data: {
        title: this.i18n.messages().split.resetConfirmTitle,
        message: this.i18n.messages().split.resetConfirmMessage,
        confirmLabel: this.i18n.labels().split.reset,
        cancelLabel: this.i18n.labels().common.cancel
      }
    });
    ref.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.splits.set(this.originalSplits);
      }
    });
  }

  protected saveSplits(): void {
    // Backend endpoint for saving splits is not yet implemented; placeholder for the API call.
    this.submitting.set(true);
    setTimeout(() => {
      this.submitting.set(false);
      this.snackBar.open(this.i18n.messages().split.saveSuccess, undefined, { duration: SNACK_DURATION_MS });
    }, 600);
  }
}
