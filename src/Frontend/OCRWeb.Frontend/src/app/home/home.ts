import { Component, inject } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-home',
  imports: [],
  templateUrl: './home.html',
  styleUrl: './home.scss'
})
export class Home {
  private readonly sanitizer = inject(DomSanitizer);

  protected readonly bodyAppUrl = environment.bodyAppUrl;
  protected readonly safeBodyAppUrl: SafeResourceUrl | null = this.bodyAppUrl
    ? this.sanitizer.bypassSecurityTrustResourceUrl(this.bodyAppUrl)
    : null;
}
