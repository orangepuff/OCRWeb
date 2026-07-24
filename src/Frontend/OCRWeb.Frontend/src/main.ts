import { bootstrapApplication } from '@angular/platform-browser';
import { PortalShell } from '@orangepuff/portal-frontend';
import { appConfig } from './app/app.config';

bootstrapApplication(PortalShell, appConfig)
  .catch((err) => console.error(err));
