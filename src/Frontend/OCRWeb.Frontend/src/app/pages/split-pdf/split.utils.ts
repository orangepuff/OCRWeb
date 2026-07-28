import type { PDFDocumentProxy } from 'pdfjs-dist';
import { ISplit } from './models/split.model';

export function generateSplitId(): string {
  return Math.random().toString(36).slice(2) + Date.now().toString(36);
}

export function formatBytes(bytes: number): string {
  if (bytes < 1024) {
    return `${bytes} B`;
  }
  const units = ['KB', 'MB', 'GB'];
  let value = bytes / 1024;
  let i = 0;
  while (value >= 1024 && i < units.length - 1) {
    value /= 1024;
    i++;
  }
  return `${value.toFixed(1)} ${units[i]}`;
}

async function resolveDestPage(doc: PDFDocumentProxy, dest: unknown): Promise<number | null> {
  if (!dest) {
    return null;
  }
  try {
    let arr: unknown[];
    if (typeof dest === 'string') {
      const resolved = await doc.getDestination(dest);
      if (!resolved) {
        return null;
      }
      arr = resolved;
    } else {
      arr = dest as unknown[];
    }
    return await doc.getPageIndex(arr[0] as { num: number; gen: number });
  } catch {
    return null;
  }
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export async function extractSplitsFromOutline(doc: PDFDocumentProxy, outline: any[]): Promise<ISplit[]> {
  if (!outline || outline.length === 0) {
    return [];
  }

  const pageIndices = await Promise.all(
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    outline.map((item: any) => resolveDestPage(doc, item.dest))
  );

  const totalPages = doc.numPages;
  const splits: ISplit[] = [];

  for (let i = 0; i < outline.length; i++) {
    const idx = pageIndices[i];
    if (idx === null) {
      continue;
    }

    const fromPage = idx + 1;
    let toPage = totalPages;
    for (let j = i + 1; j < pageIndices.length; j++) {
      if (pageIndices[j] !== null) {
        // The next bookmark's 0-based index equals the 1-based last page of the current section.
        toPage = pageIndices[j] as number;
        break;
      }
    }

    splits.push({
      id: generateSplitId(),
      bookmarkName: outline[i].title ?? `Section ${i + 1}`,
      fromPage,
      toPage,
      name: outline[i].title ?? `Section ${i + 1}`,
      selected: true
    });
  }

  return splits;
}
