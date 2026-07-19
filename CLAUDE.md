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

Group every file belonging to a single operation into one subfolder named after that operation, even though the files inside serve different roles (Request, Response, Endpoint) — e.g. `Endpoints/Internal/IsUserActive/` holding `IsUserActiveRequest.cs`, `IsUserActiveResponse.cs`, `IsUserActiveEndpoint.cs`. This mirrors the existing `Application/Commands/<Name>/` and `Application/Queries/<Name>/` convention, extended to endpoints. Some existing endpoint folders instead group several different operations under one shared area folder (e.g. `OCRWeb.Document.Api/Endpoints/PdfFiles/` holding Upload/Crop/List/Get endpoints together) and predate this rule; apply the per-operation split to new endpoints you write.

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

Run the stack via Docker Compose (API + BFF; SQL Server is external, not managed by compose):

```powershell
docker compose up -d --build                # API + BFF only
docker compose --profile frontend up -d --build   # + Angular, once it's scaffolded
docker compose down
```

- BFF: http://localhost:5100 / https://localhost:7100
- API: http://localhost:5101 / https://localhost:7101 (OpenAPI at `/openapi/v1.json`)
- Frontend (when scaffolded): http://localhost:4200

SQL Server setup (external instance, must already exist — the app never creates the database):

```powershell
sqlcmd -S localhost,1433 -U sa -P "Your_strong_password123!" -C -Q "IF DB_ID('OCRWeb') IS NULL CREATE DATABASE OCRWeb;"
```

Connection string lives in `src/Backend/OCRWeb.API/appsettings.Development.json` (`ConnectionStrings:OCRWeb`). From inside containers the host SQL Server is reached via `host.docker.internal`.

Dev HTTPS cert (mounted into API/BFF containers):

```powershell
dotnet dev-certs https -ep .\certs\ocrweb-devcert.pfx -p "Your_dev_cert_password123!"
```

EF Core migrations run automatically on API startup **only** when `DoMigration: true` in appsettings (applies to all modules at once; the flag is off by default in `appsettings.json`, on in `appsettings.Development.json`). Migrations create schema/tables, never the database itself.

Frontend (Angular 22 + Angular Material) is not yet scaffolded — `src/Frontend/OCRWeb.Frontend` currently holds only a placeholder.

## Architecture

The backend is a **modular monolith**: a single API host (`OCRWeb.API`) exposes FastEndpoints that delegate to MediatR handlers living inside per-bounded-context class libraries. Each context is split into `Domain / Application / Infrastructure` (see `src/Backend/OCRWeb.Document` for the reference layout). Contexts reference each other **by id only** — no cross-context entity references.

Dependency direction points inward: `API -> Application -> Domain`; `Infrastructure` implements the ports/repository interfaces defined in `Application`/`Domain`. Business libraries (Domain/Application) never depend on transport (FastEndpoints) or a specific vendor package (e.g. PDFsharp) — vendor code stays behind a port in `Infrastructure`.

Persistence is a **single SQL Server database** (`OCRWeb`), one schema + one EF Core DbContext (with its own migrations history table) per bounded context:

| Project | Schema | Status |
|---|---|---|
| `OCRWeb.Identity` | `identity` | Partial Implemented — owns `Users`; other modules reference a user only by `int` id via `ICurrentUser` |
| `OCRWeb.Document` (+ `.Api`, `.Contract`) | `docproc` | Partial Implemented — PDF upload/crop; `PDFFiles` (metadata) and `PDFFileContents` (binary, 1:1 shared PK) are split so listing never loads the blob; split/section workflow is planned |
| `OCRWeb.ProjectManagement` (+ `.Contract`) | `project` (planned) | Planned — project lifecycle (create/open/archive); scaffolded only, no domain/DbContext yet |
| `OCRWeb.Pdf` (+ `.Contract`) | none | cross-cutting technical library: PDF manipulation via PDFsharp behind an `IPdfManipulator` port |
| `OCRWeb.OCR` (+ `.Contract`) | none | future context — page extraction, job queue, recognition, indexing are out of scope for now |
| `OCRWeb.Shared` | none | shared DDD primitives (`ICurrentUser`, `IAuditable`, `AuditableEntity` with `MarkInserted`/`MarkUpdated`) reused by all modules |
| `OCRWeb.API` | none of its own | web host / composition root; FastEndpoints → MediatR; runs migrations + admin seeding on startup when `DoMigration` is true; `CurrentUser` is a stub until real auth exists |
| `OCRWeb.Bff` | none | Scaffold — Backend-for-Frontend facade for Angular, meant to be the auth boundary; Angular talks only to the BFF, never directly to the API |
| `OCRWeb.Frontend` / `OCRWeb.Frontend.Shared` | — | Planned, not scaffolded |

Column naming convention: every DB column, with no exceptions, uses a type prefix (`s` string, `i` int, `dt` datetime, `bt` bool, `x` xml, etc.) — this includes a table's own primary key (`iId`, not bare `Id`); domain models keep clean names — prefixes live only in the EF mapping layer. Audit fields (`iInsertedUserId`/`dtInsertedTime`, nullable `iUpdatedUserId`/`dtUpdatedTime`) are set explicitly in command handlers, not via an EF interceptor.

Tests live under `tests/unit` (xUnit + Moq, one project per bounded context) and `tests/integration` (FastEndpoints contract tests for API/Bff, Playwright for the frontend once it exists); `tests/component` also exists.

### Diagnostics logging (external dependency)

The observability library (`Diagnostics.Abstractions`/`.NLog`/`.AspNetCore` — NLog-backed logging to a
`DiagnosticLogs` database, `[Logs]`/`[Transactions]` sinks via `SqlBulkCopy`, `X-Correlation-ID`
propagation) used to live in-repo under `src/Shared/Diagnostics.*`; it has been extracted into its own
repo, **`DiagnosticLog`**, and is now consumed by `OCRWeb.API`/`OCRWeb.Identity` as a NuGet
`PackageReference` (versions pinned in `Directory.Packages.props`). Its design doc, DB schema script,
and tests all live in that repo now — see `DiagnosticLog`'s own `docs/diagnostics-logging-design.md` and
README for details.

Until `Diagnostics.*` is published to nuget.org, `NuGet.config` here points at a temporary local-folder
source (`D:\DiagnosticLog\artifacts`, produced by `dotnet pack` in the `DiagnosticLog` repo) — remove
that source and re-pin the `Version` once the real nuget.org publish happens.
