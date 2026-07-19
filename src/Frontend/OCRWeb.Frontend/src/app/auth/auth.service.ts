import { Injectable, inject } from '@angular/core';
import { IdentityService } from 'ocrweb.frontend.shared';

/**
 * Shell-specific wrapper around the shared IdentityService — adds login() (the OAuth
 * redirect, only the shell ever needs to trigger this) and re-exposes the shared
 * currentUser/isAuthenticated/checked/checkSession/logout so existing call sites don't
 * need to change.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly identityService = inject(IdentityService);

  readonly currentUser = this.identityService.currentUser;
  readonly isAuthenticated = this.identityService.isAuthenticated;
  readonly checked = this.identityService.checked;

  checkSession() {
    return this.identityService.checkSession();
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
    return this.identityService.logout();
  }
}
