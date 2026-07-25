import { Injectable, computed, signal } from '@angular/core';
import { Culture, DEFAULT_CULTURE } from './culture';
import { labels as enUSLabels } from './en-US/labels';
import { messages as enUSMessages } from './en-US/messages';

const cultureLabels: Record<Culture, typeof enUSLabels> = { 'en-US': enUSLabels };
const cultureMessages: Record<Culture, typeof enUSMessages> = { 'en-US': enUSMessages };

/**
 * Lightweight custom i18n: nested-object label/message lookup per culture (default en-US),
 * synchronous and dependency-free. Nested property access over a flat t('key') lookup is
 * deliberate — a typo in a key becomes a compile error instead of a silent runtime miss.
 */
@Injectable({ providedIn: 'root' })
export class I18nService {
  private readonly currentCulture = signal<Culture>(DEFAULT_CULTURE);

  readonly labels = computed(() => cultureLabels[this.currentCulture()]);
  readonly messages = computed(() => cultureMessages[this.currentCulture()]);
}
