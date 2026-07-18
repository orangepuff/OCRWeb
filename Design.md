# OCRWeb Frontend — UI/UX Design

> Design/planning doc for `OCRWeb.Frontend` (Angular 22 + Angular Material). Companion to `Product.md` (what the product does) and `development-plan.docx` (backend architecture). The auth shell (§5) is implemented and tested end-to-end; the project/document screens (§2) are still planning-only.

## 1. Stack & design principles

- Angular 22, standalone components, Angular Material (theme TBD — see `ng add @angular/material` prompt), SCSS.
- `OCRWeb.Frontend` talks only to `OCRWeb.Bff` — never directly to `OCRWeb.API` (enforced architecturally, not just by convention).
- `OCRWeb.Frontend.Shared` holds reusable, business-rule-free UI primitives: Text, Label, Button, CropPdf, and popup/window/form primitives. Screen-level components live in `OCRWeb.Frontend`; only genuinely reusable pieces move to `.Shared`.

## 2. Screens

| Screen | Route | Purpose | Backing use case | Status |
|---|---|---|---|---|
| Landing | `''` (public) | App name/blurb + explicit "Sign in with Google" button. Not guarded — anyone can see it. | Auth | Implemented |
| Auth error | `/auth-error` | Shown after a failed/rejected Google sign-in, with the rejection reason and a button back to sign-in | Auth (see §5) | Implemented |
| Home | `/home` (guarded) | Placeholder post-login landing spot ("Signed in as X") until the real project list exists | — | Implemented (placeholder) |
| Project list | `/projects` | List existing projects, entry point to create a new one | Use Case 2 | Planned |
| New project (from PDF) | `/projects/new` | Upload a PDF to start a new project | Use Case 1 | Planned |
| Project workspace | `/projects/:id` | View a project's files, file detail, crop tool, download/stream content | Use Case 2 | Planned |

`/home` is a temporary stand-in — once the project list screen exists it should likely become the guard's actual redirect target instead of `/home`.

Project creation/lifecycle screens are provisional — `OCRWeb.ProjectManagement` (backend) is still Planned, so this list will likely shift once that module's real shape lands.

## 3. Navigation structure

- Top-level shell (`app.html`) with a header (app name, current signed-in user, sign-out) and a router outlet.
- Header is session-aware: calls `GET /bff/me` on load; shows "Sign in with Google" if unauthenticated, or the user's display name + sign-out button if authenticated.
- `''` (Landing) and `/auth-error` are public — no guard. Every other route (currently just `/home`) is behind `authGuard`: unauthenticated access redirects (full-page navigation) to `GET /bff/login?returnUrl=<the path the guard blocked>` so the user lands back where they were after signing in.
- A manual "Sign in with Google" click (from the Landing page or the header) is not defending a specific blocked route, so both send `returnUrl='/home'` explicitly rather than relying on the current path.

## 4. Shared component library (`OCRWeb.Frontend.Shared`)

Per its README, scope is UI reuse only (no business rules):

- **Text / Label / Button** — base presentational primitives, themed via the chosen Angular Material palette.
- **CropPdf** — the crop-region selection tool used on the project workspace screen. This is the component the crop-coordinate-orientation risk (`Product.md` §5) applies to: needs to agree with the backend's `CropX`/`CropY`/`Width`/`Height` coordinate system (top-left origin vs. PDF's bottom-left origin) before it's wired to `POST /pdf-files/{id}/crop`.
- **Popup / window / form primitives** — shared modal/dialog/form-field scaffolding referenced in the original implementation plan.

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

- Exact Angular Material prebuilt theme (or custom palette) — cosmetic, decide during scaffolding, trivially changeable later.
- Whether `/login` is a dedicated route or just a button shown inline wherever an unauthenticated user lands.
- Crop coordinate orientation between the frontend's `CropPdf` component and the backend's stored `CropX`/`CropY` — flagged as an open risk in `Product.md`, needs confirming before the crop screen is wired up for real.
- Upload size limits / streaming UX for large PDFs (backend risk noted in `Product.md` — frontend needs a progress/error UX once that's decided).

## 7. Related documents

- `Product.md` — product scope and use cases
- `development-plan.docx` — backend architecture and status
- `docs/Authentication/authentication-design.docx` — Google sign-in backend design
