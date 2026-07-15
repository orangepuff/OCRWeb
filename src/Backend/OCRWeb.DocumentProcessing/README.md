# OCRWeb.DocumentProcessing

Document Processing bounded context — PDF handling (upload, crop, and later split/section).
Owns the `[docproc]` schema in the shared `OCRWeb` database.

Modular monolith: this is a class library (Domain / Application / Infrastructure). HTTP endpoints
live in `OCRWeb.API` (FastEndpoints) and delegate here via MediatR.

## Layout

```
Domain/         Entity/{PdfFile, PdfFileContent}, Repositories/IPdfFileRepository,
                ValueObjects/{FileChecksum, FileProperties}, Enums/PdfFileType
Application/     Commands/{UploadPdf, CropPdf}, Queries/{ListPdfFiles, GetPdfFileDetail,
                GetPdfFileContent}, Interfaces/IPdfManipulator
Infrastructure/ DocumentProcessingDbContext, Configurations/, Repositories/,
                PdfSharpManipulator, DesignTimeDbContextFactory, ModuleRegistration
```

PDF manipulation uses **PDFsharp** (MIT). The `IPdfManipulator` port lives in Application; the
`PdfSharpManipulator` adapter lives in Infrastructure.

## Database

The app does **not** create the database — it is assumed to already exist. Schema and tables
are created by EF Core migrations when `bMigration = true` (see `OCRWeb.API`).

Create the shared database once (if it does not exist):

```sql
IF DB_ID('OCRWeb') IS NULL
    CREATE DATABASE [OCRWeb];
GO
```

The `[docproc]` schema and tables below are created by migration — not by hand.

Add a migration with the **API as the startup project** (its appsettings supplies the
connection string to the shared design-time factory in `OCRWeb.Shared`):

```powershell
dotnet ef migrations add <Name> `
  -p src/Backend/OCRWeb.DocumentProcessing/OCRWeb.DocumentProcessing.csproj `
  -s src/Backend/OCRWeb.API/OCRWeb.API.csproj `
  --context DocumentProcessingDbContext -o Infrastructure/Migrations
```

### `[docproc].[PDFFiles]` (metadata)

| Column | Type | Notes |
|--------|------|-------|
| `Id` | UNIQUEIDENTIFIER | PK |
| `ProjectId` | UNIQUEIDENTIFIER | cross-context reference by id |
| `sFileName` | NVARCHAR(255) | |
| `sContentType` | NVARCHAR(100) | |
| `biSizeBytes` | BIGINT | |
| `binChecksum` | VARBINARY(32) | SHA-256 |
| `iFileType` | INT | 0=Original, 1=Cropped, 2=Section (default 0) |
| `sFileProperties` | NVARCHAR(MAX) | JSON crop/section params; null for originals |
| `iInsertedUserId` / `dtInsertedTime` | INT / DATETIME2(3) | audit |
| `iUpdatedUserId` / `dtUpdatedTime` | INT / DATETIME2(3) | audit (nullable) |

### `[docproc].[PDFFileContents]` (binary, 1:1 shared PK)

| Column | Type | Notes |
|--------|------|-------|
| `PdfFileId` | UNIQUEIDENTIFIER | PK + FK → PDFFiles.Id |
| `binContent` | VARBINARY(MAX) | the PDF bytes |
| audit columns | | as above |

Metadata and binary are split so listing/queries never load the blob.
