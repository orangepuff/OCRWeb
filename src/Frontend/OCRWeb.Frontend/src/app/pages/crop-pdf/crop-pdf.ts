import { HttpClient } from '@angular/common/http';
import { Component, ElementRef, OnInit, computed, inject, signal, viewChild } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { Button, Slider } from '@orangepuff/portal-frontend-shared';
import * as pdfjsLib from 'pdfjs-dist';
import type { PDFDocumentProxy } from 'pdfjs-dist';
import { firstValueFrom } from 'rxjs';
import { I18nService } from '../../i18n/i18n.service';
import { PdfFileListItem } from '../../models/pdf-file-list-item';
import { PdfService } from '../../services/pdf.service';

pdfjsLib.GlobalWorkerOptions.workerSrc = '/pdf.worker.min.mjs';

interface CanvasRect {
  x: number;
  y: number;
  width: number;
  height: number;
}

interface PageSizePts {
  width: number;
  height: number;
}

/**
 * Renders a project's PDF page-by-page (via pdf.js) and lets the user drag a crop
 * rectangle over the current page. Confirming replaces the project's file: the
 * backend's crop endpoint always creates a NEW derived file, so "replace" here means
 * crop then delete the original — no dedicated backend replace endpoint exists.
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
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly i18n = inject(I18nService);
  protected readonly canvasRef = viewChild.required<ElementRef<HTMLCanvasElement>>('pdfCanvas');

  private readonly renderScale = 1.5; // canvas pixels per PDF point
  private pdfDocument: PDFDocumentProxy | null = null;
  private dragStart: { x: number; y: number } | null = null;

  protected readonly loaded = signal(false);
  protected readonly loadError = signal(false);
  protected readonly pageBusy = signal(false);
  protected readonly submitting = signal(false);
  protected readonly errorText = signal<string | null>(null);

  protected readonly sourceFile = signal<PdfFileListItem | null>(null);
  protected readonly totalPages = signal(1);
  protected readonly pageControl = new FormControl<number>(1, { nonNullable: true });
  protected readonly pageSizePts = signal<PageSizePts | null>(null);

  protected readonly selection = signal<CanvasRect | null>(null);
  protected readonly canCrop = computed(() => {
    const sel = this.selection();
    return !this.submitting() && !!sel && sel.width >= 5 && sel.height >= 5;
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.router.navigate(['/home']);
      return;
    }

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
        this.loadPdf(file.id);
      },
      error: () => {
        this.loadError.set(true);
        this.errorText.set(this.i18n.messages().project.cropLoadError);
        this.loaded.set(true);
      }
    });

    this.pageControl.valueChanges.subscribe((pageNo) => this.renderPage(pageNo));
  }

  private async loadPdf(fileId: string): Promise<void> {
    try {
      const bytes = await firstValueFrom(
        this.http.get(this.pdfService.contentUrl(fileId), { responseType: 'arraybuffer' })
      );
      this.pdfDocument = await pdfjsLib.getDocument({ data: new Uint8Array(bytes) }).promise;
      this.totalPages.set(this.pdfDocument.numPages);
      this.pageControl.setValue(1, { emitEvent: false });
      await this.renderPage(1);
      this.loaded.set(true);
    } catch {
      this.loadError.set(true);
      this.errorText.set(this.i18n.messages().project.cropLoadError);
      this.loaded.set(true);
    }
  }

  private async renderPage(pageNo: number): Promise<void> {
    if (!this.pdfDocument) {
      return;
    }

    this.pageBusy.set(true);
    this.selection.set(null);

    const page = await this.pdfDocument.getPage(pageNo);
    const viewport = page.getViewport({ scale: this.renderScale });
    const canvas = this.canvasRef().nativeElement;
    canvas.width = viewport.width;
    canvas.height = viewport.height;
    const ctx = canvas.getContext('2d')!;
    await page.render({ canvas, canvasContext: ctx, viewport }).promise;

    this.pageSizePts.set({
      width: viewport.width / this.renderScale,
      height: viewport.height / this.renderScale
    });
    this.pageBusy.set(false);
  }

  protected onOverlayMouseDown(event: MouseEvent): void {
    const p = this.toCanvasPoint(event);
    this.dragStart = p;
    this.selection.set({ x: p.x, y: p.y, width: 0, height: 0 });
  }

  protected onOverlayMouseMove(event: MouseEvent): void {
    if (!this.dragStart) {
      return;
    }

    const p = this.toCanvasPoint(event);
    const canvas = this.canvasRef().nativeElement;
    const x = Math.max(0, Math.min(this.dragStart.x, p.x));
    const y = Math.max(0, Math.min(this.dragStart.y, p.y));
    const width = Math.min(canvas.width, Math.max(this.dragStart.x, p.x)) - x;
    const height = Math.min(canvas.height, Math.max(this.dragStart.y, p.y)) - y;
    this.selection.set({ x, y, width, height });
  }

  protected onOverlayMouseUp(): void {
    this.dragStart = null;
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
        next: ({ id: newFileId }) => this.deleteOldFile(file.id, newFileId),
        error: () => {
          this.submitting.set(false);
          this.errorText.set(this.i18n.messages().project.cropError);
        }
      });
  }

  private deleteOldFile(oldId: string, _newFileId: string): void {
    this.pdfService.delete(oldId).subscribe({
      next: () => {
        this.snackBar.open(this.i18n.messages().project.cropSuccess, undefined, { duration: 3000 });
        this.router.navigate(['/home']);
      },
      error: () => {
        // Crop succeeded but deleting the original failed — the project now has two
        // files. Stay here with a persistent error rather than navigating away and
        // hiding the inconsistency; ProjectForm only ever shows files[0], so this page
        // is currently the only place a user could notice/retry.
        this.submitting.set(false);
        this.errorText.set(this.i18n.messages().project.cropCleanupError);
      }
    });
  }

  protected cancel(): void {
    this.router.navigate(['/home']);
  }
}
