import { Component, inject } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { PORTAL_SHELL_CONFIG } from '../config/portal-shell-config';
import { TranslatePipe } from '../translation/translate.pipe';

@Component({
  selector: 'lib-portal-home',
  imports: [TranslatePipe],
  templateUrl: './home.html',
  styleUrl: './home.scss'
})
export class Home {
  private readonly sanitizer = inject(DomSanitizer);
  private readonly config = inject(PORTAL_SHELL_CONFIG);

  protected readonly module = 'OrangepuffPortal.Frontend';
  protected readonly bodyAppUrl = this.config.bodyAppUrl;
  protected readonly safeBodyAppUrl: SafeResourceUrl | null = this.bodyAppUrl
    ? this.sanitizer.bypassSecurityTrustResourceUrl(this.bodyAppUrl)
    : null;
}
