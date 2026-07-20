# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Keep these docs in sync

Two files are the source of truth for project history and stay updated whenever this repo changes:

- **`development-plan.docx`** — architecture and per-project status (Implemented / Partial Implemented / Scaffold / Planned). Whenever a structural or architectural change is made (new module, new bounded context, schema/DbContext change, status change, dependency-direction change, etc.), update the relevant section of this document.
- **`ChangeLogs.txt`** — one entry per dependency/version add/upgrade/downgrade/removal, using the template already in the file (Date, Area, Item, From, To, Reason, Updated By).

Both docs currently describe some projects with names/status that differ slightly from what exists in `src/` (e.g. the plan's "OCRWeb.DocumentProcessing" and "OCRWeb.Contract" correspond to the actual `OCRWeb.Document*` and `*.Contract` projects). Reconcile naming as you touch each area rather than assuming the docx is fully current.

- **`docs/`** — whenever a new feature is implemented, add a design/reference doc for it here (a Markdown design doc, plus a `.sql` file if the feature owns database objects — see the `diagnostics-logging-design.md` + `diagnostics-logs-schema.sql` pair in the separate `DiagnosticLog` repo for the style to follow). Do this as part of implementing the feature, not as an afterthought.

## Commit messages

This repo uses **Conventional Commits**: `type(scope): summary`, e.g. `feat(backend): implement DocumentProcessing and Identity modules`, `refactor(backend)!: per-module contract/api projects, shared PDF lib, rename Document`, `docs: add OCR web development plan and changelog`. Common types seen in history: `feat`, `fix`, `refactor`, `docs`, `chore`, `test`. Use `!` after the type/scope and a `BREAKING CHANGE:` footer for breaking changes. After making a change, always suggest a commit message in this format (with an appropriate scope such as `backend`, `frontend`, `docs`, `document`, `identity`, etc.) — don't just describe the change in prose.

## Code style

One top-level/public type per file (class, record, interface, enum), filename matching the type name. Private nested helper types (e.g. a small `private sealed class`/`record` used only internally, like `CorrelationContext.State`/`RestoreScope`) are exempt and may stay in the same file as their containing type.

FastEndpoints Request/Response DTOs are not exempt from the one-type-per-file rule: give each its own file (e.g. `GoogleProvisionRequest.cs`, `GoogleProvisionResponse.cs`), separate from the `Endpoint` class itself. Some existing endpoint files (e.g. `UploadPdfEndpoint.cs`) colocate them and predate this rule; apply the split to new endpoints you write, don't reformat old ones just to conform.

Group every file belonging to a single operation into one subfolder named after that operation, even though the files inside serve different roles (Request, Response, Endpoint) — e.g. an `AddUser/` folder holding `AddUserRequest.cs`, `AddUserResponse.cs`, `AddUserEndpoint.cs` (see the `orangepuffportal` repo's `Application/Commands/<Name>/` folders for the pattern this mirrors, extended to endpoints). Some existing endpoint folders instead group several different operations under one shared area folder (e.g. `OCRWeb.Document.Api/Endpoints/PdfFiles/` holding Upload/Crop/List/Get endpoints together) and predate this rule; apply the per-operation split to new endpoints you write.

Once a project/area has more than 1 interface, move interfaces into an `Interfaces/` subfolder, with the namespace matching the folder (e.g. `Diagnostics.Abstractions.Interfaces` for files under `Diagnostics.Abstractions/Interfaces/`) — update every consuming file's `using` accordingly, not just the moved files.

Once a parent folder has more than 1 class of the same role (e.g. resolvers, policies, validators), group them into a role-named subfolder under that parent, with the namespace matching the folder (e.g. multiple `*Resolver` classes under `Diagnostics.NLog/Lookups/` move into `Diagnostics.NLog/Lookups/Resolvers/`, namespace `Diagnostics.NLog.Lookups.Resolvers`) — update every consuming file's `using` accordingly, not just the moved files.

Always brace `if`/`else`/`for`/`foreach`/`while` bodies, even single-statement ones — no braceless one-liners (`if (x) return;`). Many existing files predate this rule; apply it to code you touch rather than reformatting files wholesale just to conform.

XML doc `<summary>` text should be short and precise: one line per sentence, no run-on multi-clause paragraphs.

`<summary>` and `</summary>` each go on their own line, never inline with the text — wrong: `/// <summary>Opts an <see cref="IHttpClientBuilder"/> into outbound X-Correlation-ID propagation.</summary>`; right: `<summary>` on its own `///` line, the sentence(s) below it, `</summary>` on its own `///` line.

Every `ILogger` call includes a `LogPrefix` identifying the class and method as the first structured parameter, built from `nameof(...)` so renames stay safe: `private const string LogPrefix = nameof(ProvisionGoogleUserCommandHandler) + "." + nameof(Handle);` for a single-method class (e.g. a MediatR handler's `Handle`), or a local `const string LogPrefix = ...` declared inside each method for classes with more than one. Usage: `logger.LogInformation("{LogPrefix}: did the thing", LogPrefix)`.

## Commands

Backend (.NET 10, solution file is `OCRWeb.slnx`):

```powershell
dotnet build OCRWeb.slnx
dotnet test OCRWeb.slnx                                   # all unit + integration tests
dotnet test tests/unit/OCRWeb.Document.UnitTests           # single test project
dotnet test --filter "FullyQualifiedName~ClassName.MethodName"  # single test
```

Run the stack via Docker Compose (single API host — it serves `/bff/*` too now; SQL Server is external, not managed by compose):

```powershell
docker compose up -d --build                # API only (also serves the /bff/* auth routes)
docker compose --profile frontend up -d --build   # + Angular (needs a Dockerfile first — not added yet; run `ng serve` from src/Frontend/OCRWeb.Frontend instead)
docker compose down
```

- API: http://localhost:5101 / https://localhost:7101 (OpenAPI at `/openapi/v1.json`; also serves `/bff/login`, `/bff/logout`, `/bff/me`, `/bff/admin/*`)
- Frontend (via `ng serve` in `src/Frontend/OCRWeb.Frontend`): https://localhost:4200

SQL Server setup (external instance, must already exist — the app never creates the database):

```powershell
sqlcmd -S localhost,1433 -U sa -P "Your_strong_password123!" -C -Q "IF DB_ID('OCRWeb') IS NULL CREATE DATABASE OCRWeb;"
```

Connection strings live in `src/Backend/OCRWeb.API/appsettings.Development.json`: `ConnectionStrings:OCRWeb` (Document/OCR's own modules) and `ConnectionStrings:Portal` (required by `OrangepuffPortal.Identity`, hardcoded key name) — both point at the same physical database, just different schemas. From inside containers the host SQL Server is reached via `host.docker.internal`.

Dev HTTPS cert (mounted into the API container):

```powershell
dotnet dev-certs https -ep .\certs\ocrweb-devcert.pfx -p "Your_dev_cert_password123!"
```

EF Core migrations run automatically on API startup **only** when `DoMigration: true` in appsettings (applies to all modules at once — every registered `IPortalModule` via `OrangepuffPortal.Host`'s `MigratePortalModulesAsync()`, plus `OCRWeb.Document`'s own DbContext migrated manually since it isn't an `IPortalModule`; the flag is off by default in `appsettings.json`, on in `appsettings.Development.json`). Migrations create schema/tables, never the database itself.

Frontend (Angular 22 + Angular Material, standalone components + signals) is scaffolded and has working cookie-based auth via the API's `/bff/*` routes: `src/Frontend/OCRWeb.Frontend/src/app` has `auth/` (`AuthService` hitting `/bff/me`, `/bff/login`, `/bff/logout`; `authGuard` route guard), `landing/`, `home/` (guarded), `auth-error/`, and `header/`. Runs via `ng serve` (not yet in Docker — no Dockerfile at `src/Frontend/OCRWeb.Frontend/Dockerfile` yet, so the `frontend` compose profile isn't usable). `src/Frontend/OCRWeb.Frontend.Shared` is still an untouched `ng generate library` stub, not wired into anything.

## Architecture

The backend is a **modular monolith**: a single API host (`OCRWeb.API`) exposes FastEndpoints that delegate to MediatR handlers living inside per-bounded-context class libraries. Each context is split into `Domain / Application / Infrastructure` (see `src/Backend/OCRWeb.Document` for the reference layout). Contexts reference each other **by id only** — no cross-context entity references.

Dependency direction points inward: `API -> Application -> Domain`; `Infrastructure` implements the ports/repository interfaces defined in `Application`/`Domain`. Business libraries (Domain/Application) never depend on transport (FastEndpoints) or a specific vendor package (e.g. PDFsharp) — vendor code stays behind a port in `Infrastructure`.

**Identity, cookie/Google-OAuth auth, and the `/bff/*` routes are no longer in-repo.** They come from the `OrangepuffPortal.Host` NuGet package (published from the sibling `orangepuffportal` repo, itself originally extracted from this repo's old `OCRWeb.Shared`/`OCRWeb.Identity`/`OCRWeb.Bff`). `OCRWeb.API` calls `AddOrangepuffPortal()`/`MapOrangepuffPortal()`/`MigratePortalModulesAsync()` in `Program.cs` and gets identity/security behavior centrally maintained in that repo — a version bump instead of duplicated logic. This also collapsed OCRWeb from two processes (a separate `OCRWeb.API` + `OCRWeb.Bff` talking over a hand-rolled RS256 "internal token" JWT hop) into one process, matching the `OrangepuffPortal.SampleHost` reference pattern.

Persistence is a **single SQL Server database** (`OCRWeb`), one schema + one EF Core DbContext (with its own migrations history table) per bounded context:

| Project | Schema | Status |
|---|---|---|
| `OCRWeb.Document` (+ `.Api`, `.Contract`) | `docproc` | Partial Implemented — PDF upload/crop; `PDFFiles` (metadata) and `PDFFileContents` (binary, 1:1 shared PK) are split so listing never loads the blob; split/section workflow is planned. Endpoints now require the `OrangepuffPortal.Host` cookie auth scheme (`CookieAuthenticationDefaults.AuthenticationScheme`), not the old internal-token JWT bearer scheme |
| `OCRWeb.ProjectManagement` (+ `.Contract`) | `project` (planned) | Planned — project lifecycle (create/open/archive); scaffolded only, no domain/DbContext yet |
| `OCRWeb.Pdf` (+ `.Contract`) | none | cross-cutting technical library: PDF manipulation via PDFsharp behind an `IPdfManipulator` port |
| `OCRWeb.OCR` (+ `.Contract`) | none | future context — page extraction, job queue, recognition, indexing are out of scope for now |
| `OCRWeb.API` | none of its own | web host / composition root; FastEndpoints → MediatR; runs migrations + admin seeding on startup when `DoMigration` is true (via `OrangepuffPortal.Host`'s `MigratePortalModulesAsync()` for Identity, plus a manual `DocumentDbContext` migrate call); also hosts the `/bff/*` routes and real cookie/Google-OAuth auth via `OrangepuffPortal.Host` — `ICurrentUser`/`CurrentUser` come from that package now, no longer a local stub |
| `OCRWeb.Frontend` | — | Partial Implemented — Angular 22 + Angular Material scaffolded, cookie-based auth via the API's `/bff/*` routes (login/logout/session guard) working; admin UI for user/permission management not yet built |
| `OCRWeb.Frontend.Shared` | — | Scaffold — untouched `ng generate library` stub, not referenced by `OCRWeb.Frontend` yet |

`identity` schema (owns `Users`) is now created/migrated by the `OrangepuffPortal.Identity` package, not an in-repo project — see the `orangepuffportal` repo for its source.

Column naming convention: every DB column, with no exceptions, uses a type prefix (`s` string, `i` int, `dt` datetime, `bt` bool, `x` xml, etc.) — this includes a table's own primary key (`iId`, not bare `Id`); domain models keep clean names — prefixes live only in the EF mapping layer. Audit fields (`iInsertedUserId`/`dtInsertedTime`, nullable `iUpdatedUserId`/`dtUpdatedTime`) are set explicitly in command handlers, not via an EF interceptor.

Tests live under `tests/unit` (xUnit + Moq, one project per bounded context) and `tests/integration` (FastEndpoints contract tests for the API, including its `/bff/*` routes; Playwright for the frontend once it exists); `tests/component` also exists.

### Diagnostics logging (external dependency)

The observability library (NLog-backed logging to a `DiagnosticLogs` database, `[Logs]`/`[Transactions]`
sinks via `SqlBulkCopy`, `X-Correlation-ID` propagation) used to live in-repo under `src/Shared/Diagnostics.*`;
it has been extracted into its own repo, **`DiagnosticLog`** (https://github.com/orangepuff/DiagnosticLog),
and is now consumed by `OCRWeb.API` as a NuGet `PackageReference` published to nuget.org (versions pinned in
`Directory.Packages.props`). Its design doc, DB schema script, and tests all live in that repo now — see
`DiagnosticLog`'s own `docs/diagnostics-logging-design.md` and README for details.

`OCRWeb.API` is a single process again (see the Architecture section above), so there's no more inter-process
HTTP hop within this repo needing `X-Correlation-ID` propagation between an API and a Bff — that outbound
propagation feature is only relevant now if `OCRWeb.API` itself calls another HTTP service.

**Package IDs differ from the C# namespaces.** The NuGet `PackageId`s are `Orangepuff.Diagnostics.Abstractions`/
`.NLog`/`.AspNetCore` (prefixed with the GitHub username) because nuget.org rejected the unprefixed
`Diagnostics.*` IDs — that prefix is reserved by another, unrelated owner, even though no package is
actually published under it. The C# namespaces and assembly names are unaffected and remain plain
`Diagnostics.Abstractions`/`.NLog`/`.AspNetCore`, so existing `using Diagnostics.Abstractions;` etc.
statements need no changes — only `PackageReference`/`PackageVersion` entries use the `Orangepuff.*` IDs.

`DiagnosticLog`'s own repo publishes new versions via a GitHub Actions workflow
(`.github/workflows/publish.yml`) using NuGet Trusted Publishing (OIDC, no stored API key): pushing a
branch named `Release/vX.Y.Z` packs and publishes that exact version automatically.
