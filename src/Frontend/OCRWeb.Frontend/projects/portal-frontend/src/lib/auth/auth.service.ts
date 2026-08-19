import { Injectable, inject } from '@angular/core';
import { CurrentUser, IdentityService } from '@orangepuff/portal-frontend-shared';
import { Observable, catchError, map, of, switchMap, tap } from 'rxjs';
import { PORTAL_SHELL_CONFIG } from '../config/portal-shell-config';
import { ThemeApplyService } from '../theme/theme-apply.service';
import { TranslationService } from '../translation/translation.service';

const GUEST_CULTURE_CODE = 'en-US';

/**
 * Shell-specific wrapper around the shared IdentityService — adds login() (the OAuth
 * redirect, only the shell ever needs to trigger this) and re-exposes the shared
 * currentUser/isAuthenticated/checked/checkSession/logout so existing call sites don't
 * need to change.
 *
 * Also keeps TranslationService in step with the signed-in user: providePortalShell's
 * APP_INITIALIZER always preloads the guest "en-US" set (nobody is known yet at bootstrap),
 * so once checkSession()/logout() resolve here we switch it to the user's own
 * CurrentUser.cultureCode, or back to the guest culture on logout.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly identityService = inject(IdentityService);
  private readonly config = inject(PORTAL_SHELL_CONFIG);
  private readonly translationService = inject(TranslationService);

  private readonly themeApplyService = inject(ThemeApplyService);

  readonly currentUser = this.identityService.currentUser;
  readonly isAuthenticated = this.identityService.isAuthenticated;
  readonly checked = this.identityService.checked;

  checkSession(): Observable<CurrentUser | null> {
    return this.identityService.checkSession().pipe(
      tap(user => this.syncTranslationCulture(user)),
      switchMap(user =>
        user
          ? this.themeApplyService.load().pipe(map(() => user))
          : of(user)
      )
    );
  }

  passwordSignIn(usernameOrEmail: string, password: string) {
    return this.identityService.passwordSignIn(usernameOrEmail, password);
  }

  login(returnUrl: string): void {
    // Absolute URL to the Bff host's real origin, not a relative path through the dev-server proxy.
    // /bff/login sets an OAuth correlation cookie that must round-trip back on /signin-google,
    // and Google redirects straight to that path on the host's own origin — if /bff/login itself
    // went through the proxy (localhost:5599 or wherever the consumer runs), the cookie would be
    // scoped to the wrong origin and the correlation check on the way back would fail.
    window.location.href = `${this.config.bffOrigin}/bff/login?returnUrl=${encodeURIComponent(returnUrl)}`;
  }

  logout() {
    return this.identityService.logout().pipe(
      tap(() => {
        this.translationService.reloadForCulture(GUEST_CULTURE_CODE);
        this.themeApplyService.clear();
      })
    );
  }

  private syncTranslationCulture(user: CurrentUser | null): void {
    this.translationService.reloadForCulture(user?.cultureCode ?? GUEST_CULTURE_CODE);
  }
}
