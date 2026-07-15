# OCRWeb.Identity

Identity bounded context — application users and authentication data.
Owns the `[identity]` schema in the shared `OCRWeb` database.

Other modules reference a user only by its **INT id** (via `ICurrentUser` in `OCRWeb.Shared`);
they never reference this module directly.

## Layout

```
Domain/         Entity/User, Repositories/IUserRepository
Infrastructure/ UserDbContext, Configurations/, Repositories/,
                Seeding/ (SeedOptions, UserDbSeeder), DesignTimeDbContextFactory,
                ModuleRegistration
```

## Database

The app does **not** create the database — it is assumed to already exist. Schema and
tables are created by EF Core migrations when `bMigration = true` (see `OCRWeb.API`).

Create the shared database once (if it does not exist):

```sql
IF DB_ID('OCRWeb') IS NULL
    CREATE DATABASE [OCRWeb];
GO
```

The `[identity]` schema and `[identity].[Users]` table are created by migration —
you do not create them by hand.

Add a migration with the **API as the startup project** (its appsettings supplies the
connection string to the shared design-time factory in `OCRWeb.Shared`):

```powershell
dotnet ef migrations add <Name> `
  -p src/Backend/OCRWeb.Identity/OCRWeb.Identity.csproj `
  -s src/Backend/OCRWeb.API/OCRWeb.API.csproj `
  --context UserDbContext -o Infrastructure/Migrations
```

### Table `[identity].[Users]`

| Column | Type | Notes |
|--------|------|-------|
| `Id` | INT IDENTITY | PK; matches audit user-id columns across modules |
| `sUsername` | NVARCHAR(100) | unique |
| `sEmail` | NVARCHAR(256) | nullable |
| `sDisplayName` | NVARCHAR(200) | nullable |
| `sPasswordHash` | NVARCHAR(MAX) | hashed via `PasswordHasher<User>` |
| `btIsActive` | BIT | default 1 |
| `dtInsertedTime` | DATETIME2(3) | |
| `dtUpdatedTime` | DATETIME2(3) | nullable |

## Seeding the admin

On first run (empty `Users` table), `UserDbSeeder` creates the default admin from the
`Seed` config section, hashing the password. It is idempotent (runs only when no users exist).

```json
"Seed": {
  "AdminUsername": "admin",
  "AdminEmail": "admin@ocrweb.local",
  "AdminDisplayName": "Administrator",
  "AdminPassword": "<dev-only; use a secret in production>"
}
```
