import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withFetch } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { providePortalShell } from '@orangepuff/portal-frontend';

import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withFetch()),
    provideAnimationsAsync(),
    providePortalShell({
      appName: 'OCRWeb',
      // Absolute origin of the OrangepuffPortal Bff — see OCRWeb.API's appsettings (port 7201).
      bffOrigin: 'https://localhost:7201'
      // bodyAppUrl intentionally unset: the OCR-specific Angular body app (Document/OCR UI)
      // isn't built yet — Home renders the "no body app configured" placeholder until then.
    })
  ]
};
