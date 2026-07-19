export const environment = {
  production: false,
  // Same-origin path (or full URL) the shell's iframe loads as the embedded body app —
  // e.g. a future OCR web app. Empty means no body app is deployed yet; Home shows a
  // placeholder instead of an empty iframe. Set per project/deployment; changing it
  // requires an Angular rebuild (ng build), not a runtime config swap — this URL is
  // expected to change rarely (once per project setup), not per environment restart.
  bodyAppUrl: ''
};
