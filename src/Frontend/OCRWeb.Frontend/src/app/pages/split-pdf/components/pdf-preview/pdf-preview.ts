import { Component, ElementRef, OnDestroy, computed, effect, input, signal, viewChild } from '@angular/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import type { PDFDocumentProxy, RenderTask } from 'pdfjs-dist';
import { PREVIEW_SCALE } from '../../split.constants';

@Component({
  selector: 'app-pdf-preview',
  imports: [MatProgressSpinnerModule],
  templateUrl: './pdf-preview.html',
  styleUrl: './pdf-preview.scss'
})
export class PdfPreview implements OnDestroy {
  readonly pdfDoc = input<PDFDocumentProxy | null>(null);
  readonly fromPage = input.required<number>();
  readonly toPage = input.required<number>();

  // Non-required so the signal is undefined before the view initializes, which lets the render
  // effect track it as a dependency and re-run automatically once the canvas is in the DOM.
  private readonly canvasRef = viewChild<ElementRef<HTMLCanvasElement>>('canvas');

  protected readonly offset = signal(0);
  protected readonly rendering = signal(false);

  protected readonly pageCount = computed(() => Math.max(1, this.toPage() - this.fromPage() + 1));
  protected readonly currentPage = computed(() => this.fromPage() + this.offset());
  protected readonly canPrev = computed(() => this.offset() > 0);
  protected readonly canNext = computed(() => this.offset() < this.pageCount() - 1);
  protected readonly pageLabel = computed(() => `${this.offset() + 1} / ${this.pageCount()}`);

  private renderTask: RenderTask | null = null;

  constructor() {
    // Reset to the first page of the range whenever the input range changes.
    effect(
      () => {
        this.fromPage();
        this.toPage();
        this.offset.set(0);
      },
      { allowSignalWrites: true }
    );

    // Re-render whenever the document, current page, or canvas reference changes.
    // canvasRef() is undefined until the view initializes, so the effect is initially a no-op
    // but automatically re-runs once the ElementRef becomes available.
    effect(
      () => {
        const doc = this.pdfDoc();
        const page = this.currentPage();
        const canvasEl = this.canvasRef()?.nativeElement;
        if (!canvasEl || !doc) {
          return;
        }
        void this.render(doc, page, canvasEl);
      },
      { allowSignalWrites: true }
    );
  }

  protected prev(): void {
    if (this.canPrev()) {
      this.offset.update((o) => o - 1);
    }
  }

  protected next(): void {
    if (this.canNext()) {
      this.offset.update((o) => o + 1);
    }
  }

  private async render(doc: PDFDocumentProxy, pageNo: number, canvasEl: HTMLCanvasElement): Promise<void> {
    if (this.renderTask) {
      try {
        this.renderTask.cancel();
      } catch {
        // pdfjs throws if already complete — safe to ignore
      }
      this.renderTask = null;
    }

    if (pageNo < 1 || pageNo > doc.numPages) {
      return;
    }

    this.rendering.set(true);
    try {
      const page = await doc.getPage(pageNo);
      const viewport = page.getViewport({ scale: PREVIEW_SCALE });
      canvasEl.width = viewport.width;
      canvasEl.height = viewport.height;
      const ctx = canvasEl.getContext('2d')!;
      this.renderTask = page.render({ canvas: canvasEl, canvasContext: ctx, viewport });
      await this.renderTask.promise;
    } catch {
      // Render was cancelled by a newer request — ignore.
    } finally {
      this.renderTask = null;
      this.rendering.set(false);
    }
  }

  ngOnDestroy(): void {
    if (this.renderTask) {
      try {
        this.renderTask.cancel();
      } catch {
        // safe to ignore
      }
    }
  }
}
