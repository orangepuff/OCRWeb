# Consuming ConfigTextDefinition

Label and validation-message strings for OCRWeb, previously hardcoded in
`src/Frontend/OCRWeb.Frontend/src/app/i18n/*`, are moving to the shared `ConfigTextDefinition` table owned by
the **`orangepuffportal`** repo (schema `configtext`, alongside `identity`). The table, its unique key,
insert-only/`btReplace` seeding semantics, and the culture `"*"` fallback are all documented there — see
`orangepuffportal`'s own `docs/config-text-design.md` and `docs/config-text-schema.sql`. This doc covers only
the OCRWeb-specific half: where OCRWeb's own default text lives and how it gets pushed into that table.

## Seed files live next to the feature that owns the text

Each OCRWeb module ships its own default text as an embedded JSON file, grouped the same way the old
`labels.ts`/`messages.ts` already grouped keys by feature:

| Module | Seed file |
|---|---|
| `OCRWeb.ProjectManagement.Api` | `ConfigText/en-US.json` (project labels/messages, ported from the old `labels.ts`/`messages.ts`) |
| `OCRWeb.Document.Api` | none yet — no document-specific frontend text exists today; add `ConfigText/en-US.json` there when it does, `ConfigTextSeed.LoadAll` already scans that assembly |
| `OCRWeb.API` (cross-module `OCRWeb.Common.*` text) | `ConfigText/en-US.json` |

Entry shape mirrors the DB row 1:1, minus the audit columns (which the writer/DB fill in) and minus
`sCultureCode` (implied by the file name — `en-US.json` seeds the `en-US` culture):

```json
[
  {
    "sModule": "OCRWeb.ProjectManagement",
    "sTextCode": "pageTitle",
    "sTextType": "lbl",
    "sText": "My Projects",
    "sNote": "Page title for the project list page"
  },
  {
    "sModule": "OCRWeb.ProjectManagement",
    "sTextCode": "addProject",
    "sTextType": "lbl",
    "sText": "Add Project",
    "sNote": "Button label to open the Add Project dialog",
    "btReplace": true
  }
]
```

`sModule` is namespaced with an `OCRWeb.` prefix (`OCRWeb.ProjectManagement`, `OCRWeb.Document`,
`OCRWeb.Common`) since the table is shared across portal apps — without the prefix, two apps could pick the
same module name and collide on the table's unique key. `btReplace` defaults to `false` when omitted; only
set it to `true` when intentionally pushing a corrected value into an already-seeded environment, then set it
back to `false` and commit (see the orangepuffportal design doc for why).

## Startup wiring

`OCRWeb.API`'s `Program.cs` is the single place that pushes all of these into the shared table, alongside the
existing manual `DocumentDbContext` migration step (both run after `MigratePortalModulesAsync()`, which is
what actually creates/updates the `configtext` schema itself — see that project's `IPortalModule`):

```csharp
await app.MigratePortalModulesAsync();

if (builder.Configuration.GetValue<bool>("DoMigration"))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<DocumentDbContext>().Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<ProjectDbContext>().Database.MigrateAsync();

    var configTextWriter = scope.ServiceProvider.GetRequiredService<IConfigTextWriter>();
    await configTextWriter.UpsertManyAsync("en-US", ConfigTextSeed.LoadAll("en-US"));
}
```

`ConfigTextSeed.LoadAll(cultureCode)` (in `OCRWeb.API`) scans every module assembly
(`OCRWeb.API`, `OCRWeb.ProjectManagement.Api`, `OCRWeb.Document.Api`) for an embedded resource ending in
`ConfigText.{cultureCode}.json`, skips modules that don't have one yet, and deserializes what it finds into
`ConfigTextSeedEntry` records — see `OCRWeb.API/ConfigText/ConfigTextSeed.cs`. This is an in-process call
through `IConfigTextWriter` (`OrangepuffPortal.ConfigText.Contract`), not an HTTP round-trip — the same DI
container that `AddOrangepuffPortal()` wires up already has it registered. Gated by the same `DoMigration`
flag as the rest of the migrate/seed block, since it is the same category of "only touch schema/seed data
when explicitly enabled" concern.

## Current blocker: not wired up yet

`IConfigTextWriter`/`IConfigTextReader` ship in a **new** NuGet package,
`OrangepuffPortal.ConfigText.Contract`, from the `orangepuffportal` repo. That package has not been published
to nuget.org yet (`orangepuffportal`'s `Directory.Packages.props` still only defines `OrangepuffPortal.Host`
and `OrangepuffPortal.Shared` at `1.0.3`). Until a new version is released (pushing a `Release/vX.Y.Z` branch
in that repo, which is a real published-package action requiring explicit sign-off, not something done as a
side effect of this doc), OCRWeb cannot add a `PackageReference` to it and this feature cannot build
end-to-end. The seed JSON files and the loader/startup-wiring code are written ahead of that so the OCRWeb
side is ready the moment a version exists.
