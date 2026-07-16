# OCRWeb.OCR

OCR bounded context — text recognition over prepared PDF pages/sections.
Owns the `[ocr]` schema in the shared `OCRWeb` database.

Modular monolith: this is a class library (Domain / Application / Infrastructure). HTTP endpoints
live in `OCRWeb.API` (FastEndpoints) and delegate here via MediatR. Public DTOs live in
`OCRWeb.OCR.Contract`.

## Layout

```
Domain/         Aggregates, value objects, enums, repository interfaces (empty — add as needed)
Application/    Commands / Queries / Interfaces (MediatR handlers) (empty — add as needed)
Infrastructure/ OcrDbContext, DesignTimeDbContextFactory, ModuleRegistration
```

## Database

The app does **not** create the database — it is assumed to already exist. Schema and tables
are created by EF Core migrations when `bMigration = true` (see `OCRWeb.API`).

No migration exists yet (no aggregates are mapped). Once the first entity + configuration is
added, create the initial migration with the **API as the startup project**:

```powershell
dotnet ef migrations add InitialOcr `
  -p src/Backend/OCRWeb.OCR/OCRWeb.OCR.csproj `
  -s src/Backend/OCRWeb.API/OCRWeb.API.csproj `
  --context OcrDbContext -o Infrastructure/Migrations
```

Then add `OcrDbContext.Database.MigrateAsync()` to the migrate/seed block in `OCRWeb.API`.
