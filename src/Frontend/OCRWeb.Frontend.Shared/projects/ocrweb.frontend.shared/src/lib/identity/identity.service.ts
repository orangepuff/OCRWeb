import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, of, tap } from 'rxjs';
import { CurrentUser } from './current-user';
import { EffectivePermission } from './effective-permission';

/**
 * Reads the current user and their effective permissions from the shell's Bff
 * (`/bff/me`, `/bff/me/permissions`) — same-origin, cookie-authenticated, so any Angular
 * body app embedded in the shell's iframe gets this for free with no extra wiring.
 * The shell itself also consumes this (see OCRWeb.Frontend's AuthService) rather than
 * duplicating the logic.
 */
@Injectable({ providedIn: 'root' })
export class IdentityService {
  private readonly http = inject(HttpClient);

  private readonly _currentUser = signal<CurrentUser | null>(null);
  private readonly _checked = signal(false);
  private readonly _permissions = signal<EffectivePermission[]>([]);

  readonly currentUser = this._currentUser.asReadonly();
  readonly isAuthenticated = computed(() => this._currentUser() !== null);
  readonly checked = this._checked.asReadonly();
  readonly permissions = this._permissions.asReadonly();

  checkSession() {
    return this.http.get<CurrentUser>('/bff/me').pipe(
      tap((user) => {
        this._currentUser.set(user);
        this._checked.set(true);
      }),
      catchError(() => {
        this._currentUser.set(null);
        this._checked.set(true);
        return of(null);
      })
    );
  }

  loadPermissions() {
    return this.http.get<EffectivePermission[]>('/bff/me/permissions').pipe(
      tap((permissions) => this._permissions.set(permissions)),
      catchError(() => {
        this._permissions.set([]);
        return of([]);
      })
    );
  }

  /** True only for Boolean-type rules where Allowed = 1. Integer/Decimal limits: read `permissions()` directly. */
  hasPermission(code: string): boolean {
    return this._permissions().find((p) => p.code === code)?.allowed === 1;
  }

  logout() {
    return this.http.post('/bff/logout', {}).pipe(tap(() => this._currentUser.set(null)));
  }
}
