import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, of, tap } from 'rxjs';
import { CurrentUser } from './current-user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  private readonly _currentUser = signal<CurrentUser | null>(null);
  private readonly _checked = signal(false);

  readonly currentUser = this._currentUser.asReadonly();
  readonly isAuthenticated = computed(() => this._currentUser() !== null);
  readonly checked = this._checked.asReadonly();

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

  login(returnUrl: string): void {
    // Absolute URL to the Bff's real origin, not a relative path through the dev-server proxy.
    // /bff/login sets an OAuth correlation cookie that must round-trip back on /signin-google,
    // and Google redirects straight to that path on the Bff's own origin — if /bff/login itself
    // went through the proxy (localhost:4200), the cookie would be scoped to the wrong origin
    // and the correlation check on the way back would fail.
    const bffBaseUrl = 'https://localhost:7100';
    window.location.href = `${bffBaseUrl}/bff/login?returnUrl=${encodeURIComponent(returnUrl)}`;
  }

  logout() {
    return this.http.post('/bff/logout', {}).pipe(
      tap(() => this._currentUser.set(null))
    );
  }
}