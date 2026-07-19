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

These are internal-only, same as the existing two — the Bff (or a future admin UI calling
through it) is the intended caller, never Angular directly against `OCRWeb.API`.

## Not yet built

- Permission resolution query (read side: given a user id, compute effective rule set
  following `ParentId`).
- Bff-facing endpoints/proxy for the above.
- Frontend admin UI component.
- Seed data / initial rule catalog.

## Migration

`20260719061336_AddSecurityRules` (`src/Backend/OCRWeb.Identity/Infrastructure/Migrations/`).
See `security-rules-schema.sql` in this folder for the resulting schema in plain SQL form.
