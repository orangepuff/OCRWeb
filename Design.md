# OCRWeb Frontend — UI/UX Design

> Design/planning doc for `OCRWeb.Frontend` (Angular 22 + Angular Material) — the template shell: auth, header, admin UI, and the body-content iframe. Companion to `Product.md` (what the eventual body app does), `development-plan.docx` (backend architecture), and `docs/Frontend/body-content-iframe-design.md` (the iframe itself). The auth shell (§5), admin UI, and Landing/Home (§2) are implemented; the body app that will actually do OCR work is a separate, not-yet-built project.

## 1. Stack & design principles

- Angular 22, standalone components, Angular Material M3 (`mat.theme()`, cyan primary / orange tertiary — see `OCRWeb.Frontend.Shared/theme.scss`), SCSS.
- `OCRWeb.Frontend` talks only to `OCRWeb.Bff` — never directly to `OCRWeb.API` (enforced architecturally, not just by convention).
- `OCRWeb.Frontend.Shared` holds reusable UI and session state shared with any body app embedded in the shell's iframe — see §4.
- **Every screen must be responsive.** Minimum target: usable at a 375px mobile viewport up to a 1280px+ desktop viewport, no horizontal scroll, no fixed pixel widths on layout containers. New screens pick one breakpoint (typically 600px, matching Landing's — see §2) rather than inventing bespoke ones per component. Verify with the browser's mobile and desktop presets, not just by eyeballing a wide window.

## 2. Screens

| Screen | Route | Purpose | Backing use case | Status |
|---|---|---|---|---|
| Landing | `''` (public) | Hero graphic + title/tagline + explicit "Sign in with Google" button. Not guarded — anyone can see it. Content is generic/template-branded, not OCR-specific — see §2.1. | Auth | Implemented |
| Auth error | `/auth-error` | Shown after a failed/rejected Google sign-in, with the rejection reason and a button back to sign-in | Auth (see §5) | Implemented |
| Home | `/home` (guarded) | Hosts the body-content iframe (a separate embedded Angular body app, e.g. a future OCR web) — see `docs/Frontend/body-content-iframe-design.md`. Shows a placeholder when no body app is configured. | — | Implemented |

This shell is a template: it owns auth, the admin UI (Users/SecurityRuleCategories/SecurityRuleItems, `/admin/*`), and the header — not project/document screens. Those are expected to live in whatever body app gets embedded at `/home`, each with its own backend, not in `OCRWeb.Frontend` itself. `OCRWeb.ProjectManagement`/project-upload/workspace screens described in earlier drafts of this doc no longer apply to this shell; if this specific project's OCR functionality is ever built as the body app, it gets its own design doc rather than extending this one.

### 2.1 Landing hero content

`Landing` (`src/app/landing/`) renders a `LandingContent` (`landing-content.ts`: `{ title, tagline, heroImageUrl? }`), currently the hardcoded `DEFAULT_LANDING_CONTENT`. Structured as its own type so a future site-settings feature (§6) can swap the constant for an HTTP-backed value with no template changes.

- **`heroImageUrl` set** — renders an `<img>`, letting a project built on this template (or, once site settings exists, a per-deployment override) supply a real brand image.
- **`heroImageUrl` unset (default)** — renders a built-in abstract graphic (four overlapping circles) instead of a literal illustration, so it never implies OCR/document functionality that belongs to the body app, not the shell. Colored entirely from Material system tokens (`var(--mat-sys-primary)`, `--mat-sys-primary-container`, `--mat-sys-secondary`) rather than hardcoded hex, so it re-themes automatically if Manage Themes (§6, planned) changes the palette.

Responsive: single breakpoint at 600px — hero graphic shrinks from 220px to 160px, tagline max-width narrows, everything stays centered in a single column at both sizes (see §1).

## 3. Navigation structure

- Top-level shell (`app.html`) is a fixed-height flex column: a 56px header bar, then a content area that fills the rest of the viewport (`router-outlet`) — see `docs/Frontend/body-content-iframe-design.md` for why (the iframe on `/home` needs to sit flush against the header with no gap).
- Header (`lib-avatar` + `IdentityService` from `OCRWeb.Frontend.Shared`) is session-aware: calls `GET /bff/me` on load; shows "Sign in with Google" if unauthenticated, or an avatar + "Hello, {name}" dropdown if authenticated — Settings for everyone, Manage users / Manage rule categories / Manage rule items / Manage themes for admins only (`CurrentUser.isAdmin`).
- `''` (Landing) and `/auth-error` are public — no guard. `/home` and `/settings` are behind `authGuard` (login required). `/admin/*` (four routes) is behind `adminGuard` (login **and** `isAdmin` required — a non-admin who navigates there directly is redirected to `/home` instead of hitting API calls that now 403; see `docs/Identity/security-rules-permission-design.md`).
- A manual "Sign in with Google" click (from the Landing page or the header) is not defending a specific blocked route, so both send `returnUrl='/home'` explicitly rather than relying on the current path.

## 4. Shared component library (`OCRWeb.Frontend.Shared`)

Consumed by both this shell and any future Angular body app embedded in its iframe, so they share auth state and UI with zero extra wiring. Packed via `npm pack` and depended on as a tarball, not a raw `file:` path — a symlinked directory resolves its own `@angular/core`, which breaks DI with a duplicate core instance (NG0203).

- **`IdentityService`** — current user + effective permissions via `/bff/me` and `/bff/me/permissions` (same-origin, cookie-authenticated). `OCRWeb.Frontend`'s own `AuthService` is a thin wrapper that adds `login()`.
- **`Avatar`** — renders a user's `binAvatar` image (self or another user's, by id) with initials fallback.
- **`ConfirmDialog`** — replaces the browser's `confirm()` in the three admin list screens' delete flows.
- **`theme.scss`** (`apply-theme` mixin) — the M3 baseline (`mat.theme()`) both this shell and any body app `@use`, so colors/typography match without duplicating the palette definition.

A CropPdf-style component, and other primitives specific to actual document/OCR work, are not built here — that functionality belongs in whatever body app eventually gets embedded, not in this template's shared library.

## 5. Auth UX flow (implemented)

Full backend design lives in `docs/Authentication/authentication-design.docx` — this section covers only what the frontend needs to do.

1. User clicks **Sign in with Google** (Landing page or header) → `AuthService.login('/home')` → full-page navigate (relative URL, `/bff/login?returnUrl=%2Fhome`) — reaches the Bff through the Angular dev-server's `proxy.conf.json`, exactly like a fetch call would; Google's own redirect afterward goes straight to Bff's real origin, not through the proxy.
2. Google handles auth; Bff handles the callback server-side. Angular is not involved in this part at all.
3. On success, the browser is redirected back into the Angular app at the original `returnUrl` (an absolute URL built from `Frontend:BaseUrl` on the Bff side).
4. On failure/rejection, the browser is redirected to `/auth-error?reason=<reason>`. The frontend maps known `reason` values to a readable message:
   - `email_not_verified` — "Your Google account's email isn't verified."
   - `registration_disabled` — "This account isn't registered yet. Contact an administrator."
   - anything else — generic "Something went wrong signing you in."
5. **Sign out**: `POST /bff/logout` (fetch, not navigation) → on `204`, clears local state and client-side routes back to `''` (Landing).
6. Session check: `GET /bff/me` → `200 { userId, email, displayName }` or `401`. Used both by the header (show signed-in state) and by the route guard (decide whether to redirect to `/bff/login`).

**Gotcha hit during manual testing:** `OCRWeb.Bff`'s `Program.cs` called `AddAuthentication()` but never `AddAuthorization()` — ASP.NET Core's minimal-hosting model auto-inserts the matching middleware for services you *did* register, so `UseAuthentication()` silently worked while `/bff/me`/`/bff/logout` (which use `.RequireAuthorization()`) threw `"a middleware was not found that supports authorization"` on every call. Fixed by adding `builder.Services.AddAuthorization()` plus explicit `app.UseAuthentication()`/`app.UseAuthorization()` calls rather than relying on the implicit insertion.

## 6. Open design questions

- Whether `/login` is a dedicated route or just a button shown inline wherever an unauthenticated user lands.
- **Site settings** (not built): a per-deployment source for `LandingContent` (§2.1), `environment.bodyAppUrl` (currently build-time, see `docs/Frontend/body-content-iframe-design.md`), and Manage Themes' palette/app-name (§ below) — likely one admin-editable DB-backed config rather than three separate mechanisms, but not designed yet.
- Manage Themes (placeholder page only): a single global `Theme` row (`PrimaryColor`, `TertiaryColor`, `DarkMode`, `AppName`) with an admin color-picker UI, applied at runtime via `--mat-sys-*` overrides, no rebuild needed. Scope still being refined (full color wheel vs. primary/tertiary only).
- Crop coordinate orientation, upload size limits, and other document/OCR-specific UX questions from earlier drafts of this doc belong to whatever body app eventually does that work — see `Product.md` for that scope — not to this shell.

## 7. Related documents

- `Product.md` — product scope and use cases
- `development-plan.docx` — backend architecture and status
- `docs/Authentication/authentication-design.docx` — Google sign-in backend design
