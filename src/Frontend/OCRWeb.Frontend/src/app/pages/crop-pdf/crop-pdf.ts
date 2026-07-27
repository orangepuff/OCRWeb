import { HttpClient, HttpErrorResponse, HttpEventType, HttpResponse } from '@angular/common/http';
import { Component, ElementRef, HostListener, OnInit, computed, inject, signal, viewChild } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { Button, Slider } from '@orangepuff/portal-frontend-shared';
import * as pdfjsLib from 'pdfjs-dist';
import type { PDFDocumentProxy } from 'pdfjs-dist';
import { firstValueFrom, filter, map, tap } from 'rxjs';
import { I18nService } from '../../i18n/i18n.service';
import { PdfFileListItem } from '../../models/pdf-file-list-item';
import { DEFAULT_CROP_FILL, SNACK_DURATION_MS } from '../../ui-config';
import { FileCacheService } from '../../services/file-cache.service';
import { PdfService } from '../../services/pdf.service';

pdfjsLib.GlobalWorkerOptions.workerSrc = '/pdf.worker.min.mjs';

// FileCacheService scope for PdfFile content - keeps its ids from colliding with any other
// resource type that ends up sharing the same cache store.
const PDF_FILE_CACHE_SCOPE = 'pdf';

interface CanvasRect {
  x: number;
  y: number;
  width: number;
  height: number;
}

type ResizeHandle = 'nw' | 'ne' | 'sw' | 'se';

function clamp(value: number, min: number, max: number): number {
  return Math.min(Math.max(value, min), max);
}

interface PageSizePts {
  width: number;
  height: number;
}

function formatBytes(bytes: number): string {
  if (bytes < 1024) {
    return `${bytes} B`;
  }

  const units = ['KB', 'MB', 'GB'];
  let value = bytes / 1024;
  let unitIndex = 0;
  while (value >= 1024 && unitIndex < units.length - 1) {
    value /= 1024;
    unitIndex++;
  }

  return `${value.toFixed(1)} ${units[unitIndex]}`;
}

/**
 * Renders a project's PDF page-by-page (via pdf.js) and lets the user drag a crop
 * rectangle over the current page. Confirming replaces the project's file: the
 * backend's crop endpoint persists the cropped file and removes the source itself.
 */
@Component({
  selector: 'app-crop-pdf',
  imports: [ReactiveFormsModule, MatProgressSpinnerModule, Button, Slider],
  templateUrl: './crop-pdf.html',
  styleUrl: './crop-pdf.scss'
})
export class CropPdf implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly pdfService = inject(PdfService);
  private readonly fileCache = inject(FileCacheService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly i18n = inject(I18nService);
  protected readonly canvasRef = viewChild.required<ElementRef<HTMLCanvasElement>>('pdfCanvas');

  private readonly renderScale = 1.5; // canvas pixels per PDF point
  private pdfDocument: PDFDocumentProxy | null = null;

  // Drag state, all in canvas-internal-pixel space (see toCanvasPoint):
  // - draw: dragStart is the corner the drag started from, the other corner follows the mouse.
  // - resize: dragStart is the fixed opposite corner (the handle's anchor).
  // - move: dragStart is the mouse position at drag start; selectionAtDragStart is the rect to offset from.
  private dragMode: 'draw' | 'move' | 'resize' | null = null;
  private dragStart: { x: number; y: number } | null = null;
  private selectionAtDragStart: CanvasRect | null = null;

  protected readonly loaded = signal(false);
  protected readonly loadError = signal(false);
  protected readonly pageBusy = signal(false);
  protected readonly submitting = signal(false);
  protected readonly errorText = signal<string | null>(null);
  protected readonly loadingStep = signal('');
  protected readonly downloadProgress = signal<{ loaded: number; total: number | null } | null>(null);
  protected readonly downloadProgressText = computed(() => {
    const progress = this.downloadProgress();
    if (!progress) {
      return null;
    }

    const loaded = formatBytes(progress.loaded);
    if (progress.total === null) {
      return loaded;
    }

    const percent = Math.round((progress.loaded / progress.total) * 100);
    return `${loaded} / ${formatBytes(progress.total)} (${percent}%)`;
  });

  protected readonly sourceFile = signal<PdfFileListItem | null>(null);
  protected readonly totalPages = signal(1);
  protected readonly pageControl = new FormControl<number>(1, { nonNullable: true });
  protected readonly pageSizePts = signal<PageSizePts | null>(null);

  protected readonly selection = signal<CanvasRect | null>(null);
  protected readonly canCrop = computed(() => {
    const sel = this.selection();
    return !this.submitting() && !!sel && sel.width >= 5 && sel.height >= 5;
  });
  protected readonly cropButtonLabel = computed(() =>
    this.submitting() ? this.i18n.labels().project.croppingButton : this.i18n.labels().project.cropButton
  );

  private projectId!: number;

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (!idParam) {
      this.router.navigate(['/home']);
      return;
    }

    const id = Number(idParam);
    this.projectId = id;
    this.loadingStep.set(this.i18n.labels().project.loadingFiles);
    this.pdfService.list(id).subscribe({
      next: (files) => {
        const file = files[0] ?? null;
        if (!file) {
          this.loadError.set(true);
          this.errorText.set(this.i18n.messages().project.noFileError);
          this.loaded.set(true);
          return;
        }
        this.sourceFile.set(file);
        this.loadPdf(file.id, file.sizeBytes);
      },
      error: (err: unknown) => {
        this.loadError.set(true);
        this.errorText.set(this.describeLoadError(err));
        this.loaded.set(true);
      }
    });

    this.pageControl.valueChanges.subscribe((pageNo) => this.renderPage(pageNo));
  }

  private async loadPdf(fileId: number, sizeBytes: number): Promise<void> {
    try {
      this.loadingStep.set(this.i18n.labels().project.downloadingPdf);

      // Large PDFs (100+ MB) routinely exceed the browser's own HTTP disk cache's per-entry
      // size limit, so relying on Cache-Control/ETag alone still re-downloads the whole file on
      // every page load - IndexedDB has no comparable cap, hence this separate cache.
      let bytes = await this.fileCache.get(PDF_FILE_CACHE_SCOPE, fileId);
      if (!bytes) {
        bytes = await this.downloadPdf(fileId, sizeBytes);
        // Cache before handing the buffer to pdf.js - pdf.js posts it to its worker thread and
        // may transfer (detach) the underlying ArrayBuffer, so caching it afterwards could store
        // an already-emptied buffer.
        void this.fileCache.put(PDF_FILE_CACHE_SCOPE, fileId, bytes);
      }

      this.loadingStep.set(this.i18n.labels().project.renderingPreview);
      this.pdfDocument = await pdfjsLib.getDocument({ data: new Uint8Array(bytes) }).promise;
      this.totalPages.set(this.pdfDocument.numPages);
      this.pageControl.setValue(1, { emitEvent: false });
      await this.renderPage(1);
      this.setDefaultSelection();
      this.loaded.set(true);
    } catch (err) {
      this.loadError.set(true);
      this.errorText.set(this.describeLoadError(err));
      this.loaded.set(true);
    }
  }

  private async downloadPdf(fileId: number, sizeBytes: number): Promise<ArrayBuffer> {
    // Seed the total from the list metadata we already have - on a fast/local
    // download the browser may never fire a progress event before completion,
    // so waiting on event.total for the total would leave nothing to show at all.
    this.downloadProgress.set({ loaded: 0, total: sizeBytes > 0 ? sizeBytes : null });
    const bytes = await firstValueFrom(
      this.http
        .get(this.pdfService.contentUrl(fileId), {
          responseType: 'arraybuffer',
          reportProgress: true,
          observe: 'events'
        })
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

  // Surfaces the underlying HTTP status or exception message alongside the generic
  // message, since a bare "failed to load" gives the user nothing to act on or report.
  private describeLoadError(err: unknown): string {
    const base = this.i18n.messages().project.cropLoadError;
    const detail =
      err instanceof HttpErrorResponse
        ? `${err.status} ${err.statusText}`.trim()
        : err instanceof Error
          ? err.message
          : undefined;
    return detail ? `${base}: ${detail}` : base;
  }

  private async renderPage(pageNo: number): Promise<void> {
    if (!this.pdfDocument) {
      return;
    }

    this.pageBusy.set(true);

    const page = await this.pdfDocument.getPage(pageNo);
    const viewport = page.getViewport({ scale: this.renderScale });
    const canvas = this.canvasRef().nativeElement;
    // Record the old drawing-buffer size before resizing so we can scale the selection
    // proportionally to the new page's dimensions rather than just clamping it.
    const prevWidth = canvas.width;
    const prevHeight = canvas.height;
    canvas.width = viewport.width;
    canvas.height = viewport.height;
    const ctx = canvas.getContext('2d')!;
    await page.render({ canvas, canvasContext: ctx, viewport }).promise;

    this.pageSizePts.set({
      width: viewport.width / this.renderScale,
      height: viewport.height / this.renderScale
    });

    this.rescaleSelectionToCanvas(prevWidth, prevHeight);

    this.pageBusy.set(false);
  }

  private setDefaultSelection(): void {
    const canvas = this.canvasRef().nativeElement;
    const w = canvas.width * DEFAULT_CROP_FILL;
    const h = canvas.height * DEFAULT_CROP_FILL;
    this.selection.set({
      x: (canvas.width - w) / 2,
      y: (canvas.height - h) / 2,
      width: w,
      height: h
    });
  }

  // Scales the selection from the canvas's previous drawing-buffer dimensions to the new ones
  // so its relative position is preserved when switching pages. Falls back to a no-op when
  // there's no selection or the previous dimensions are 0 (first render, canvas was default-sized).
  private rescaleSelectionToCanvas(prevWidth: number, prevHeight: number): void {
    const sel = this.selection();
    if (!sel || prevWidth === 0 || prevHeight === 0) {
      return;
    }
    const canvas = this.canvasRef().nativeElement;
    const scaleX = canvas.width / prevWidth;
    const scaleY = canvas.height / prevHeight;
    const width = Math.min(sel.width * scaleX, canvas.width);
    const height = Math.min(sel.height * scaleY, canvas.height);
    this.selection.set({
      x: clamp(sel.x * scaleX, 0, canvas.width - width),
      y: clamp(sel.y * scaleY, 0, canvas.height - height),
      width,
      height
    });
  }

  // toDisplayRect (called from the template) derives the selection box's on-screen size from a
  // live canvas.getBoundingClientRect() read, so it self-corrects whenever Angular re-renders -
  // but browser zoom resizes the canvas without changing any signal Angular tracks, so nothing
  // triggers that re-render on its own. Re-setting the selection signal here (even to the same
  // canvas-internal-pixel values) is what forces the template to recompute it against the
  // canvas's new on-screen size.
  @HostListener('window:resize')
  protected clampSelectionToCanvas(): void {
    const sel = this.selection();
    if (!sel) {
      return;
    }

    const canvas = this.canvasRef().nativeElement;
    const width = Math.min(sel.width, canvas.width);
    const height = Math.min(sel.height, canvas.height);
    this.selection.set({
      x: clamp(sel.x, 0, canvas.width - width),
      y: clamp(sel.y, 0, canvas.height - height),
      width,
      height
    });
  }

  // Starts a brand-new selection, discarding any previous one. Only fires from the empty
  // overlay background - onSelectionMouseDown/onHandleMouseDown stop propagation so clicking
  // the existing box or its handles doesn't also land here.
  protected onOverlayMouseDown(event: MouseEvent): void {
    const p = this.toCanvasPoint(event);
    this.dragMode = 'draw';
    this.dragStart = p;
    this.selection.set({ x: p.x, y: p.y, width: 0, height: 0 });
  }

  // Drags the existing selection as a whole, keeping its size fixed.
  protected onSelectionMouseDown(event: MouseEvent): void {
    event.stopPropagation();
    const sel = this.selection();
    if (!sel) {
      return;
    }

    this.dragMode = 'move';
    this.dragStart = this.toCanvasPoint(event);
    this.selectionAtDragStart = { ...sel };
  }

  // Resizes from one corner, keeping the opposite corner (the anchor) fixed in place.
  protected onHandleMouseDown(handle: ResizeHandle, event: MouseEvent): void {
    event.stopPropagation();
    const sel = this.selection();
    if (!sel) {
      return;
    }

    this.dragMode = 'resize';
    this.dragStart = {
      x: handle === 'nw' || handle === 'sw' ? sel.x + sel.width : sel.x,
      y: handle === 'nw' || handle === 'ne' ? sel.y + sel.height : sel.y
    };
  }

  protected onOverlayMouseMove(event: MouseEvent): void {
    if (!this.dragMode || !this.dragStart) {
      return;
    }

    const canvas = this.canvasRef().nativeElement;
    const p = this.toCanvasPoint(event);

    if (this.dragMode === 'move' && this.selectionAtDragStart) {
      const start = this.selectionAtDragStart;
      const x = clamp(start.x + (p.x - this.dragStart.x), 0, canvas.width - start.width);
      const y = clamp(start.y + (p.y - this.dragStart.y), 0, canvas.height - start.height);
      this.selection.set({ x, y, width: start.width, height: start.height });
      return;
    }

    // draw and resize both define a rect between a fixed anchor point (dragStart) and the
    // current mouse position, clamped to the canvas bounds.
    const x = Math.max(0, Math.min(this.dragStart.x, p.x));
    const y = Math.max(0, Math.min(this.dragStart.y, p.y));
    const width = Math.min(canvas.width, Math.max(this.dragStart.x, p.x)) - x;
    const height = Math.min(canvas.height, Math.max(this.dragStart.y, p.y)) - y;
    this.selection.set({ x, y, width, height });
  }

  protected onOverlayMouseUp(): void {
    this.dragMode = null;
    this.dragStart = null;
    this.selectionAtDragStart = null;
  }

  private toCanvasPoint(event: MouseEvent): { x: number; y: number } {
    const canvas = this.canvasRef().nativeElement;
    const rect = canvas.getBoundingClientRect();
    const displayScale = canvas.width / rect.width;
    return {
      x: (event.clientX - rect.left) * displayScale,
      y: (event.clientY - rect.top) * displayScale
    };
  }

  // The selection rect is stored in canvas-internal-pixel space (matching toCanvasPoint, and
  // what confirmCrop's PDF-point math expects). The canvas's on-screen size can be smaller than
  // that internal resolution (max-height/max-width scale it down), so rendering the overlay box
  // needs its own conversion back to on-screen CSS pixels - binding sel.x/y/width/height directly
  // would draw a box scaled to the wrong size and overflowing past the visible page.
  protected toDisplayRect(sel: CanvasRect): CanvasRect {
    const canvas = this.canvasRef().nativeElement;
    const scale = canvas.width > 0 ? canvas.getBoundingClientRect().width / canvas.width : 1;
    return { x: sel.x * scale, y: sel.y * scale, width: sel.width * scale, height: sel.height * scale };
  }

  protected confirmCrop(): void {
    const sel = this.selection();
    const file = this.sourceFile();
    const pageSize = this.pageSizePts();
    if (!sel || !file || !pageSize || !this.canCrop()) {
      return;
    }

    const scale = this.renderScale;
    let cropX = sel.x / scale;
    let cropWidth = sel.width / scale;
    let cropHeight = sel.height / scale;
    let cropY = pageSize.height - (sel.y / scale + sel.height / scale);

    cropX = Math.max(0, cropX);
    cropY = Math.max(0, cropY);
    cropWidth = Math.min(cropWidth, pageSize.width - cropX);
    cropHeight = Math.min(cropHeight, pageSize.height - cropY);

    this.submitting.set(true);
    this.errorText.set(null);

    this.pdfService
      .crop(file.id, {
        pageNo: this.pageControl.value,
        cropX: Math.round(cropX),
        cropY: Math.round(cropY),
        width: Math.round(cropWidth),
        height: Math.round(cropHeight),
        fileName: file.fileName
      })
      .subscribe({
        next: () => {
          // The source file id is gone after a successful crop (replaced by a new id) - drop its
          // cached bytes rather than let them sit unused until they expire on their own.
          void this.fileCache.remove(PDF_FILE_CACHE_SCOPE, file.id);
          this.snackBar.open(this.i18n.messages().project.cropSuccess, undefined, { duration: SNACK_DURATION_MS });
          this.router.navigate(['/projects', this.projectId, 'split']);
        },
        error: () => {
          this.submitting.set(false);
          this.errorText.set(this.i18n.messages().project.cropError);
        }
      });
  }

  protected cancel(): void {
    this.router.navigate(['/home']);
  }
}
