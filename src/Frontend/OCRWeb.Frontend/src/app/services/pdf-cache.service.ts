import { Injectable } from '@angular/core';

const DB_NAME = 'ocrweb-pdf-cache';
const DB_VERSION = 1;
const STORE_NAME = 'pdfContent';
const CACHED_AT_INDEX = 'cachedAt';

/**
 * How long a cached PDF's bytes stay usable before a fresh download is required - matches the
 * 1 hour Cache-Control max-age already set server-side (GetPdfFileContentEndpoint), so both
 * layers agree on freshness even though this cache exists specifically to cover files too large
 * for the browser's own HTTP disk cache to store at all (it has a per-entry size ceiling that
 * IndexedDB doesn't).
 */
export const PDF_CACHE_TTL_MS = 60 * 60 * 1000;

interface CachedPdfEntry {
  fileId: number;
  bytes: ArrayBuffer;
  cachedAt: number;
}

/**
 * IndexedDB-backed cache for downloaded PDF bytes, keyed by file id. A file's bytes never change
 * once uploaded (upload/crop always mint a new id rather than mutating an existing one), so the
 * only freshness concern is age, not content changes.
 */
@Injectable({ providedIn: 'root' })
export class PdfCacheService {
  private readonly dbPromise = this.openDatabase();

  constructor() {
    // Fire-and-forget: prunes entries left behind by files the user never revisited before they
    // expired, so the cache doesn't grow unbounded across many different documents over time.
    void this.sweepExpired();
  }

  async get(fileId: number): Promise<ArrayBuffer | null> {
    const db = await this.dbPromise;
    const entry = await promisify<CachedPdfEntry | undefined>(
      db.transaction(STORE_NAME, 'readonly').objectStore(STORE_NAME).get(fileId)
    );

    if (!entry) {
      return null;
    }

    if (Date.now() - entry.cachedAt > PDF_CACHE_TTL_MS) {
      void this.remove(fileId);
      return null;
    }

    return entry.bytes;
  }

  async put(fileId: number, bytes: ArrayBuffer): Promise<void> {
    const db = await this.dbPromise;
    const entry: CachedPdfEntry = { fileId, bytes, cachedAt: Date.now() };
    await promisify(db.transaction(STORE_NAME, 'readwrite').objectStore(STORE_NAME).put(entry));
  }

  async remove(fileId: number): Promise<void> {
    const db = await this.dbPromise;
    await promisify(db.transaction(STORE_NAME, 'readwrite').objectStore(STORE_NAME).delete(fileId));
  }

  private async sweepExpired(): Promise<void> {
    const db = await this.dbPromise;
    const store = db.transaction(STORE_NAME, 'readwrite').objectStore(STORE_NAME);
    const index = store.index(CACHED_AT_INDEX);
    const cutoff = Date.now() - PDF_CACHE_TTL_MS;

    await new Promise<void>((resolve, reject) => {
      const request = index.openCursor(IDBKeyRange.upperBound(cutoff));
      request.onsuccess = () => {
        const cursor = request.result;
        if (!cursor) {
          resolve();
          return;
        }
        cursor.delete();
        cursor.continue();
      };
      request.onerror = () => reject(request.error as DOMException);
    });
  }

  private openDatabase(): Promise<IDBDatabase> {
    return new Promise((resolve, reject) => {
      const request = indexedDB.open(DB_NAME, DB_VERSION);
      request.onupgradeneeded = () => {
        const store = request.result.createObjectStore(STORE_NAME, { keyPath: 'fileId' });
        store.createIndex(CACHED_AT_INDEX, 'cachedAt');
      };
      request.onsuccess = () => resolve(request.result);
      request.onerror = () => reject(request.error as DOMException);
    });
  }
}

function promisify<T>(request: IDBRequest<T>): Promise<T> {
  return new Promise((resolve, reject) => {
    request.onsuccess = () => resolve(request.result);
    request.onerror = () => reject(request.error as DOMException);
  });
}
