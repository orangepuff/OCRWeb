import { Injectable } from '@angular/core';

const DB_NAME = 'ocrweb-file-cache';
const DB_VERSION = 1;
const STORE_NAME = 'fileContent';
const CACHED_AT_INDEX = 'cachedAt';

/**
 * How long a cached file's bytes stay usable before a fresh download is required - matches the
 * 1 hour Cache-Control max-age already set for PDF content (GetPdfFileContentEndpoint), so both
 * layers agree on freshness even though this cache exists specifically to cover files too large
 * for the browser's own HTTP disk cache to store at all (it has a per-entry size ceiling that
 * IndexedDB doesn't).
 */
export const FILE_CACHE_TTL_MS = 60 * 60 * 1000;

interface CachedFileEntry {
  key: string;
  bytes: ArrayBuffer;
  cachedAt: number;
}

/**
 * IndexedDB-backed cache for downloaded binary content, keyed by a caller-supplied scope + numeric
 * id (e.g. `('pdf', fileId)`). The scope exists because ids are only unique within one resource
 * type - without it, a PDF file id and some unrelated entity's id could collide in the same store.
 * Assumes a given (scope, id) pair's bytes never change once cached (true for PdfFile, whose ids
 * are minted fresh on every upload/crop rather than mutated in place); the only freshness concern
 * handled here is age, not content changes.
 */
@Injectable({ providedIn: 'root' })
export class FileCacheService {
  private readonly dbPromise = this.openDatabase();

  constructor() {
    // Fire-and-forget: prunes entries left behind by files the user never revisited before they
    // expired, so the cache doesn't grow unbounded across many different documents over time.
    void this.sweepExpired();
  }

  async get(scope: string, id: number): Promise<ArrayBuffer | null> {
    const db = await this.dbPromise;
    const entry = await promisify<CachedFileEntry | undefined>(
      db.transaction(STORE_NAME, 'readonly').objectStore(STORE_NAME).get(cacheKey(scope, id))
    );

    if (!entry) {
      return null;
    }

    if (Date.now() - entry.cachedAt > FILE_CACHE_TTL_MS) {
      void this.remove(scope, id);
      return null;
    }

    return entry.bytes;
  }

  async put(scope: string, id: number, bytes: ArrayBuffer): Promise<void> {
    const db = await this.dbPromise;
    const entry: CachedFileEntry = { key: cacheKey(scope, id), bytes, cachedAt: Date.now() };
    await promisify(db.transaction(STORE_NAME, 'readwrite').objectStore(STORE_NAME).put(entry));
  }

  async remove(scope: string, id: number): Promise<void> {
    const db = await this.dbPromise;
    await promisify(db.transaction(STORE_NAME, 'readwrite').objectStore(STORE_NAME).delete(cacheKey(scope, id)));
  }

  private async sweepExpired(): Promise<void> {
    const db = await this.dbPromise;
    const store = db.transaction(STORE_NAME, 'readwrite').objectStore(STORE_NAME);
    const index = store.index(CACHED_AT_INDEX);
    const cutoff = Date.now() - FILE_CACHE_TTL_MS;

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
        const store = request.result.createObjectStore(STORE_NAME, { keyPath: 'key' });
        store.createIndex(CACHED_AT_INDEX, 'cachedAt');
      };
      request.onsuccess = () => resolve(request.result);
      request.onerror = () => reject(request.error as DOMException);
    });
  }
}

function cacheKey(scope: string, id: number): string {
  return `${scope}:${id}`;
}

function promisify<T>(request: IDBRequest<T>): Promise<T> {
  return new Promise((resolve, reject) => {
    request.onsuccess = () => resolve(request.result);
    request.onerror = () => reject(request.error as DOMException);
  });
}
