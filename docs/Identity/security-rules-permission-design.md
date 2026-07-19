# Security rule permission model

Permission catalog and per-user assignment for `OCRWeb.Identity`, ported from a legacy
`SecurityRuleCategory` / `SecurityRuleItems` / `SecurityUserRuleItems` schema. Owns the
`identity` schema alongside `Users` and `ExternalLogins`.

## Schema

- **`SecurityRuleCategory`** — groups related permission codes (`CategoryDesc`, `TextCode`
  for future i18n, `Hidden` to keep an entry DB-managed-only, out of any admin UI).
- **`SecurityRuleItems`** — one row per permission code (`Code`, unique; `Description`;
  `RuleType`; `SortOrder`; `TextCode`; `Hidden`), belongs to a category.
- **`SecurityUserRuleItems`** — assigns a rule item to a user directly. No `Role` entity —
  see "Template users" below for how grouping is done instead.

`RuleType` (`OCRWeb.Identity.Domain.Enums.RuleType`) decides which value column on
`SecurityUserRuleItems` is meaningful for a given rule item:

| `RuleType` | Meaning | Column used |
|---|---|---|
| `Boolean` (0, default) | plain allow/deny | `Allowed` holds 0/1 |
| `Integer` (1) | numeric permission limit | `Allowed` holds the limit value |
| `Decimal` (2) | numeric permission limit | `AllowedDecimal` holds the limit value |

## Template users (no Role entity)

`Users` gains two columns instead of a `Role`/`UserRole` join:

- `IsTemplateUser` (`btIsTemplateUser`) — marks a user as reusable as a permission template.
- `ParentId` (`iParentId`, self-FK, `Restrict`) — points at a template user this user
  inherits permissions from.

Rules enforced in the `Application` command handlers (not achievable as plain DB
constraints, since they depend on sibling rows):

1. `ParentId` may only reference a user with `IsTemplateUser = true`.
2. That referenced template user must itself have `ParentId = null` — chains are exactly
   one level deep, no template-of-a-template.
3. A user with `ParentId` set inherits 100% from the template; it may not also hold its own
   `SecurityUserRuleItems` rows (no partial override).
4. A user cannot become a child (`ParentId` set) while other users still depend on it as a
   parent, and cannot be un-marked as a template (`IsTemplateUser = false`) while any user
   still points at it.
5. Deleting a user is blocked while other users still have `ParentId` pointing at it
   (`Users.iParentId` FK is `Restrict`, checked explicitly before delete to avoid a raw
   `DbUpdateException`).

Permission resolution (read side, not yet implemented) follows: if `ParentId is null`, read
this user's own `SecurityUserRuleItems`; otherwise read the parent's.

## Application layer

`Application/Commands/` under `OCRWeb.Identity`, one folder per operation, matching
`ProvisionGoogleUser`'s style (`ITransactionLogger` transaction per command, `LogPrefix`
structured logging, result record with named factory methods):

- `AddUser` / `UpdateUser` / `DeleteUser`
- `AddSecurityRuleCategory` / `UpdateSecurityRuleCategory` / `DeleteSecurityRuleCategory`
- `AddSecurityRuleItem` / `UpdateSecurityRuleItem` / `DeleteSecurityRuleItem`

Each delete/update command re-validates its constraints explicitly (existence, uniqueness,
the template rules above) rather than relying on the caller to have checked first, and
rejects with a machine-readable reason string (`"has_dependent_users"`,
`"category_desc_taken"`, etc.) rather than throwing.

## API layer

FastEndpoints under `OCRWeb.API/Endpoints/Internal/`, one folder per operation
(`Request`/`Response`/`Endpoint` split per file), `JwtBearerDefaults.AuthenticationScheme`,
matching the existing `GoogleProvision`/`IsUserActive` internal endpoints:

| Method | Route |
|---|---|
| POST | `/internal/identity/users` |
| PUT | `/internal/identity/users/{id}` |
| DELETE | `/internal/identity/users/{id}` |
| POST | `/internal/identity/security-rule-categories` |
| PUT | `/internal/identity/security-rule-categories/{id}` |
| DELETE | `/internal/identity/security-rule-categories/{id}` |
| POST | `/internal/identity/security-rule-items` |
| PUT | `/internal/identity/security-rule-items/{id}` |
| DELETE | `/internal/identity/security-rule-items/{id}` |
| GET | `/internal/identity/users` |
| GET | `/internal/identity/security-rule-categories` |
| GET | `/internal/identity/security-rule-items` (optional `?categoryId=`) |
| GET | `/internal/identity/users/{id}/permissions` |

These are internal-only, same as the existing two — `OCRWeb.Bff` is the only caller, never
Angular directly against `OCRWeb.API`. List DTOs (`UserListItemDto`,
`SecurityRuleCategoryListItemDto`, `SecurityRuleItemListItemDto`, `EffectivePermissionDto`)
live in `OCRWeb.Identity.Contract` and flow straight through API → Bff unchanged (no
per-layer duplication, unlike the command Result types above) — same pattern as
`OCRWeb.Document.Contract`'s `PdfFileListItemDto`.

`GetEffectivePermissionsQuery` is the permission-resolution read side: given a user id, it
follows `ParentId` (if set) to the template user, reads that user's `SecurityUserRuleItems`,
and joins against `SecurityRuleItems` for `Code`/`Description`/`RuleType`. Callers never need
to know whether a user has their own rows or inherits from a template.

## Bff layer

`OCRWeb.Bff/Endpoints/`: `UserAdminEndpoints`, `SecurityRuleCategoryAdminEndpoints`,
`SecurityRuleItemAdminEndpoints` (full CRUD + list) are mapped under a single
`app.MapGroup("/bff/admin").RequireAuthorization("AdminOnly")` in `Program.cs` — the group
requires the `AdminOnly` policy (`RequireClaim("admin", "true")`), so every route under
`/bff/admin/*` needs the coarse `Users.IsAdmin` bypass, not just "is logged in". The `admin`
claim is added at Google sign-in (`OnCreatingTicket`) and refreshed every 5 minutes alongside
the existing `IsActive` check (`OnValidatePrincipal`) — same staleness window already accepted
there, so revoking `btAdmin` in the DB takes up to 5 minutes to take effect for an
already-signed-in session, not instantly. `AuthEndpoints` also gained `GET
/bff/me/permissions` (resolves the current user's id from the auth cookie, calls
`GetEffectivePermissionsQuery` via `IIdentityApiClient`).

The frontend also has its own `adminGuard` (`src/Frontend/OCRWeb.Frontend/src/app/auth/admin.guard.ts`)
on all four `/admin/*` routes — checks `CurrentUser.isAdmin` (from `/bff/me`) and redirects a
non-admin to `/home` instead of letting them hit a page whose API calls all 403. This is a UX
nicety layered on top of the real boundary; the Bff's `AdminOnly` policy above is what actually
enforces the permission.

## Frontend

`src/Frontend/OCRWeb.Frontend/src/app/admin/` — one feature per resource (`users/`,
`security-rule-categories/`, `security-rule-items/`), each with a model, an `HttpClient`
service, a Material-table list component, and an add/edit dialog. Routes under `/admin/*`,
guarded by the existing `authGuard`, linked from the header nav.

## Coarse admin flag and avatar

`Users.IsAdmin` (`btAdmin`, default false) is a coarse-grained super-admin bypass, separate
from the granular `SecurityRuleItems` system. It has no setter anywhere in application code —
granting it is a manual DB update on the seeded admin account, by design (same bootstrapping
philosophy as the rest of this system). `IsUserAdminQuery` / `GET
/internal/identity/users/{id}/is-admin` / `IIdentityApiClient.IsUserAdminAsync` resolve it;
`GET /bff/me` now includes `IsAdmin` in `MeResponse` so the frontend can gate UI.

Avatars are a separate `UserAvatars` table (`UserAvatar` entity, 1:1 shared PK with `Users`,
`binAvatar` + `sContentType`) — **not** a column on `Users` — so ordinary user lookups (the
login hot path especially) never drag the blob along, mirroring the `PDFFiles`/
`PDFFileContents` split. `UpdateUserAvatarCommand` is self-service only (2MB cap, rejects with
`"avatar_too_large"`), exposed as `PUT /internal/identity/users/{id}/avatar` (multipart,
`AllowFileUploads()`, no file clears it) and `GET .../avatar` (`Send.BytesAsync`, mirrors
`GetPdfFileContentEndpoint`). Bff: `PUT /bff/me/avatar` (self only, userId from the auth
cookie) and `GET /bff/users/{id}/avatar` (any authenticated user, so avatars can be shown
next to other people's names too).

## Not yet built

- Seed data / initial rule catalog.
- Manage Themes: a single global `Theme` row (`PrimaryColor`, `TertiaryColor`, `DarkMode`,
  `AppName`) with an admin color-picker UI; applied at runtime via the M3 system CSS custom
  properties (`--mat-sys-primary` etc.), no rebuild needed. Scope still being refined with
  the user (wants a full color wheel, not just primary/tertiary).
- Body-content iframe: the shell's content area below the header becomes a single iframe
  whose `src` is a deployment-level config value (not per-route) — the embedded body app
  owns 100% of its own navigation and UI; the shell contributes only the header bar. Body
  apps are expected to have their own backend/API (reusing `Orangepuff.Diagnostics.*`
  directly for their own logging, same as `OCRWeb.API` does) rather than proxying through
  this shell's Bff.

## Migrations

`20260719061336_AddSecurityRules` and `20260719111001_AddAdminFlagAndAvatar`
(`src/Backend/OCRWeb.Identity/Infrastructure/Migrations/`). See `security-rules-schema.sql`
in this folder for the `AddSecurityRules` schema in plain SQL form (not yet updated for the
admin flag / avatar table).
