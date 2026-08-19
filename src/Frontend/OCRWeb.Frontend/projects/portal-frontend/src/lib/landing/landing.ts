import { Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { AuthService } from '../auth/auth.service';
import { PORTAL_SHELL_CONFIG } from '../config/portal-shell-config';
import { TranslatePipe } from '../translation/translate.pipe';
import { TranslationService } from '../translation/translation.service';
import { LandingContent } from './landing-content';

const MODULE = 'OrangepuffPortal.Frontend';

@Component({
  selector: 'lib-portal-landing',
  imports: [ReactiveFormsModule, MatButtonModule, MatFormFieldModule, MatInputModule, TranslatePipe],
  templateUrl: './landing.html',
  styleUrl: './landing.scss'
})
export class Landing implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly config = inject(PORTAL_SHELL_CONFIG);
  private readonly translationService = inject(TranslationService);

  protected readonly module = MODULE;

  protected readonly content: LandingContent = {
    title: this.config.appName,
    tagline: this.config.landing?.tagline ?? this.translationService.get('landing.defaultTagline', MODULE),
    heroImageUrl: this.config.landing?.heroImageUrl
  };

  protected readonly showPasswordForm = signal(false);
  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly passwordForm = new FormGroup({
    usernameOrEmail: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    password: new FormControl('', { nonNullable: true, validators: [Validators.required] })
  });

  ngOnInit(): void {
    this.authService.checkSession().subscribe(() => {
      if (this.authService.isAuthenticated()) {
        this.router.navigateByUrl('/home');
      }
    });
  }

  protected signInWithGoogle(): void {
    this.authService.login('/home');
  }

  protected togglePasswordForm(): void {
    this.showPasswordForm.update((v) => !v);
    this.errorMessage.set(null);
  }

  protected submitPasswordSignIn(): void {
    if (this.passwordForm.invalid) {
      return;
    }

    const { usernameOrEmail, password } = this.passwordForm.getRawValue();

    this.submitting.set(true);
    this.errorMessage.set(null);

    this.authService.passwordSignIn(usernameOrEmail, password).subscribe({
      next: () => this.router.navigateByUrl('/home'),
      error: () => {
        this.submitting.set(false);
        this.errorMessage.set(this.translationService.get('landing.invalidCredentials', MODULE));
      }
    });
  }
}
