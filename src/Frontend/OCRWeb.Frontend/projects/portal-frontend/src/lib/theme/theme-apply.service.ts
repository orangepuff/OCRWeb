import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, map, of, switchMap, tap } from 'rxjs';

export interface ActiveTheme {
  id: number;
  themeCode: string;
  description: string;
}

interface ThemeVarsResponse {
  themeId: number;
  themeCode: string;
  description: string;
  cssVars: Record<string, string>;
}

@Injectable({ providedIn: 'root' })
export class ThemeApplyService {
  private readonly http = inject(HttpClient);
  private static readonly VAR_PREFIX = '--op-';

  /** The theme currently applied to this browser tab. Null until first load or after logout. */
  readonly currentTheme = signal<ActiveTheme | null>(null);

  /** Fetches the user's resolved theme CSS vars from the BFF and stamps them on :root. */
  load(): Observable<void> {
    return this.http.get<ThemeVarsResponse>('/bff/theme').pipe(
      tap(res => {
        this.applyVars(res.cssVars);
        this.currentTheme.set({ id: res.themeId, themeCode: res.themeCode, description: res.description });
      }),
      map(() => void 0),
      catchError(() => of(void 0))
    );
  }

  /** Removes all --op-* and --mat-sys-* custom properties from :root and clears the current-theme signal. */
  clear(): void {
    const root = document.documentElement;
    const toRemove = Array.from(root.style).filter(p =>
      p.startsWith(ThemeApplyService.VAR_PREFIX) || p.startsWith('--mat-sys-')
    );
    toRemove.forEach(p => root.style.removeProperty(p));
    this.currentTheme.set(null);
  }

  /**
   * Persists the user's theme selection to the BFF, then immediately re-loads and re-applies CSS vars.
   * Pass 0 to revert to the Default theme.
   */
  setTheme(themeId: number): Observable<void> {
    return this.http.put<void>('/bff/me/theme', { themeId }).pipe(
      switchMap(() => this.load())
    );
  }

  /** Returns active themes the user can choose from. */
  listActiveThemes(): Observable<ActiveTheme[]> {
    return this.http.get<ActiveTheme[]>('/bff/themes');
  }

  private applyVars(cssVars: Record<string, string>): void {
    const root = document.documentElement;
    for (const [key, value] of Object.entries(cssVars)) {
      root.style.setProperty(key, value);
    }
  }
}
