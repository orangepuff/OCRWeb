import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ConfigTextEntry } from './config-text-entry';

/** Modules this library's own components (admin screens, header, landing, settings, ...) use. */
export const DEFAULT_TRANSLATION_MODULES = ['OrangepuffPortal.Common', 'OrangepuffPortal.Frontend'];

/**
 * Client-side mirror of the backend's ITranslation: resolves (code, module) to display text from
 * ConfigTextDefinition, falling back to the code itself if nothing is loaded yet or nothing matches
 * — same fallback rule as the backend, so a missing translation is visible instead of blank.
 *
 * The whole culture's text set is fetched once via {@link preload} (called by an APP_INITIALIZER
 * registered in `providePortalShell()`) and kept in memory — {@link get} is a synchronous lookup
 * against that cache, since Angular templates need a synchronous pipe, not an async one.
 */
@Injectable({ providedIn: 'root' })
export class TranslationService {
  private readonly http = inject(HttpClient);
  private entries = new Map<string, string>();
  private loadedCulture = 'en-US';
  private loadedModules: string[] = DEFAULT_TRANSLATION_MODULES;

  /**
   * @param modules Only these modules' rows are fetched — the shared ConfigTextDefinition table
   * holds every module ever seeded by every app sharing the portal, so an unfiltered fetch would
   * grow without bound as more apps/modules onboard (see docs/config-text-design.md). Pass an empty
   * array to fetch everything (rarely what you want). Defaults to the modules this library's own
   * components use; a consuming app should merge in its own module name(s) via
   * `PortalShellConfig.translationModules`.
   */
  async preload(cultureCode: string, modules: string[] = DEFAULT_TRANSLATION_MODULES): Promise<void> {
    try {
      // Runs at app bootstrap, before any HTTP interceptor/error-handling setup is guaranteed to be
      // ready — a failure here (network hiccup, backend not up yet) must not block the whole app
      // from rendering, so fall through to the get()-level code fallback instead.
      let params = new HttpParams().set('culture', cultureCode);
      if (modules.length > 0) {
        params = params.set('modules', modules.join(','));
      }

      const rows = await firstValueFrom(this.http.get<ConfigTextEntry[]>('/bff/config-text', { params }));
      this.entries = new Map(rows.map((row) => [this.key(row.sModule, row.sTextCode), row.sText]));
    } catch {
      this.entries = new Map();
    } finally {
      this.loadedCulture = cultureCode;
      this.loadedModules = modules;
    }
  }

  /**
   * Re-preloads with a different culture, reusing whatever module list the initial bootstrap
   * {@link preload} call was given — used by AuthService to switch from the bootstrap-time
   * "en-US" (before the signed-in user's culture is known) to the user's own `CurrentUser.cultureCode`
   * once login/session-check resolves, and back again on logout. No-op if the culture is unchanged.
   */
  async reloadForCulture(cultureCode: string): Promise<void> {
    if (cultureCode === this.loadedCulture) {
      return;
    }

    await this.preload(cultureCode, this.loadedModules);
  }

  get(code: string, module: string): string {
    return this.entries.get(this.key(module, code)) ?? code;
  }

  /** Substitutes {0}, {1}, ... placeholders in the resolved text with `args`, in order. */
  getFormatted(code: string, module: string, ...args: string[]): string {
    return args.reduce((text, arg, i) => text.replace(`{${i}}`, arg), this.get(code, module));
  }

  private key(module: string, code: string): string {
    return `${module} ${code}`;
  }
}
