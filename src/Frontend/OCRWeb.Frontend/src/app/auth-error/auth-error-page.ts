import { Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from '../auth/auth.service';

const REASON_MESSAGES: Record<string, string> = {
  email_not_verified: "Your Google account's email isn't verified.",
  registration_disabled: "This account isn't registered yet. Contact an administrator.",
};

@Component({
  selector: 'app-auth-error-page',
  imports: [],
  templateUrl: './auth-error-page.html',
  styleUrl: './auth-error-page.scss'
})
export class AuthErrorPage {
  private readonly route = inject(ActivatedRoute);
  private readonly authService = inject(AuthService);

  protected readonly message =
    REASON_MESSAGES[this.route.snapshot.queryParamMap.get('reason') ?? ''] ??
    'Something went wrong signing you in.';

  protected retry(): void {
    this.authService.login('/');
  }
}