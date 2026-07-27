import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Button } from '@orangepuff/portal-frontend-shared';
import { I18nService } from '../../i18n/i18n.service';

/**
 * Placeholder for the split-into-sections workflow that follows a crop - not implemented yet.
 * Exists so crop-pdf has somewhere real to redirect to on success instead of a route that 404s.
 * The route still carries the project id (`projects/:id/split`) for when this is built out.
 */
@Component({
  selector: 'app-split-pdf',
  imports: [Button],
  templateUrl: './split-pdf.html',
  styleUrl: './split-pdf.scss'
})
export class SplitPdf {
  private readonly router = inject(Router);

  protected readonly i18n = inject(I18nService);

  protected back(): void {
    this.router.navigate(['/home']);
  }
}
