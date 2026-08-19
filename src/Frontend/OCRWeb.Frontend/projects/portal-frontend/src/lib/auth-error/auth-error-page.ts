import { Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../auth/auth.service';
import { TranslatePipe } from '../translation/translate.pipe';
import { TranslationService } from '../translation/translation.service';

const MODULE = 'OrangepuffPortal.Frontend';

@Component({
  selector: 'lib-portal-auth-error-page',
  imports: [MatButtonModule, TranslatePipe],
  templateUrl: './auth-error-page.html',
  styleUrl: './auth-error-page.scss'
})
export class AuthErrorPage {
  private readonly route = inject(ActivatedRoute);
  private readonly authService = inject(AuthService);
  private readonly translationService = inject(TranslationService);

  protected readonly module = MODULE;

  protected readonly message = (() => {
    const reason = this.route.snapshot.queryParamMap.get('reason');
    const code = reason ? `authError.reason.${reason}` : 'authError.reason.default';
    const translated = this.translationService.get(code, MODULE);
    // Falls through to the generic default if the reason isn't one of the codes we seed for —
    // TranslationService.get() itself only falls back to returning the raw code, which would leak
    // e.g. "authError.reason.some_unmapped_code" to the user otherwise.
    return translated === code ? this.translationService.get('authError.reason.default', MODULE) : translated;
  })();

  protected retry(): void {
    this.authService.login('/');
  }
}
