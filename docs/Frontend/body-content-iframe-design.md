# Body-content iframe

`OCRWeb.Frontend` is a template shell: the header bar owns session/admin UI, and everything
below it is a single embedded "body app" — a separate Angular application (e.g. a future OCR
web) that owns 100% of its own navigation and UI. The shell never proxies the body app's API
calls through its own Bff; a body app is expected to talk to its own backend directly.

## Layout

`app.html` wraps `<app-header />` and `<router-outlet />` in a flex column (`app.scss`,
`.app-shell` / `.app-shell__content`) so the header stays a fixed 56px bar and the routed
content fills the rest of the viewport. `/home` (the post-login landing route, guarded by
`authGuard`) is the only route that renders the iframe — `Home` (`src/app/home/home.ts`) fills
its host element edge-to-edge (`:host { height: 100% }`, iframe `width/height: 100%`,
`border: none`) so it sits flush against the header with no gap, per the approved design.

## Configuring the body app URL

The iframe `src` is `environment.bodyAppUrl` (`src/environments/environment.ts` /
`environment.production.ts`, wired via `fileReplacements` in `angular.json`'s `production`
build configuration). This is a **build-time** value, not a runtime-fetched config file —
deliberate: the body app URL is expected to change rarely (once per project that forks this
template, not per deployment), so the simplicity of a plain Angular environment file won out
over a runtime `config.json` fetched via `APP_INITIALIZER`. Changing it requires an Angular
rebuild (`ng build`) and redeploy, not a hot-swappable file.

When `bodyAppUrl` is empty (the default — no body app exists yet for this project), `Home`
renders a placeholder message instead of an empty iframe.

`DomSanitizer.bypassSecurityTrustResourceUrl` is required to set the iframe `src` — Angular
sanitizes `iframe[src]` bindings by default and would otherwise strip the URL.

## Not yet built

- No actual body app exists yet to point `bodyAppUrl` at (OCR web itself hasn't been built).
- Same-origin reverse-proxying so a body app's own domain can be exposed under this shell's
  origin (avoids third-party-cookie issues for the body app's own auth, if it has any) is not
  wired up — today `bodyAppUrl` would need to already be same-origin or the body app would need
  to tolerate being framed cross-origin.
