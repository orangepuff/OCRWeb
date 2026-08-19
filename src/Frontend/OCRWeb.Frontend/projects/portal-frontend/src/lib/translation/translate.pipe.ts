import { Pipe, PipeTransform, inject } from '@angular/core';
import { TranslationService } from './translation.service';

/** `{{ 'admin.config.title' | translate:'OrangepuffPortal.Frontend' }}` */
@Pipe({ name: 'translate', pure: false })
export class TranslatePipe implements PipeTransform {
  private readonly translationService = inject(TranslationService);

  transform(code: string, module: string): string {
    return this.translationService.get(code, module);
  }
}
