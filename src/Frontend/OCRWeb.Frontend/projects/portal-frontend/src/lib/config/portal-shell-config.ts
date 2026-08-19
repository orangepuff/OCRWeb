import { EnvironmentProviders, InjectionToken, inject, makeEnvironmentProviders, provideAppInitializer } from '@angular/core';
import { DEFAULT_TRANSLATION_MODULES, TranslationService } from '../translation/translation.service';

export interface PortalShellConfig {
  /** Header brand string and Landing's default title. */
  appName: string;
  /**
   * Absolute origin of the OrangepuffPortal Bff, e.g. 'https://localhost:7100'.
   * Must be absolute (not a relative/proxied path) — the OAuth correlation cookie set by
   * /bff/login has to round-trip to the real origin Google redirects back to.
   */
  bffOrigin: string;
  /** Home's iframe src. Unset renders the "no body app configured" placeholder. */
  bodyAppUrl?: string;
  landing?: {
    tagline?: string;
    heroImageUrl?: string;
  };
  /**
   * This app's own ConfigTextDefinition module name(s) (e.g. "OCRWeb.ProjectManagement"), merged
   * with the shell library's own modules when preloading translations at bootstrap — lets the
   * consuming app's own components use TranslationService/the `translate` pipe for their own text
   * too, without fetching every module ever seeded by every app sharing the portal.
   */
  translationModules?: string[];
}

export const PORTAL_SHELL_CONFIG = new InjectionToken<PortalShellConfig>('PORTAL_SHELL_CONFIG');

export function providePortalShell(config: PortalShellConfig): EnvironmentProviders {
  return makeEnvironmentProviders([
    { provide: PORTAL_SHELL_CONFIG, useValue: config },
    // Preloads ConfigTextDefinition once at bootstrap so the `translate` pipe/TranslationService.get()
    // have data ready before the first render — automatic for every consuming app, no extra wiring
    // required there, mirroring the backend's PortalShellTextPortalModule seeding automatically.
    provideAppInitializer(() => {
      const modules = [...DEFAULT_TRANSLATION_MODULES, ...(config.translationModules ?? [])];
      return inject(TranslationService).preload('en-US', modules);
    })
  ]);
}
