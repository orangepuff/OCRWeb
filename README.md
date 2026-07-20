# OCRWeb

## Docker

Docker Compose is part of the solution (`docker-compose.dcproj` in `OCRWeb.slnx`),
so you can start the whole stack from Visual Studio by setting **docker-compose**
as the startup project and pressing F5, or from the command line below.

Run the API (single host — also serves the `/bff/*` auth routes via the `OrangepuffPortal.Host` package):

```powershell
docker compose up -d --build
```

Services:

- API: http://localhost:5101 or https://localhost:7101
- API OpenAPI: http://localhost:5101/openapi/v1.json
- `/bff/login`, `/bff/logout`, `/bff/me`, `/bff/admin/*` are served from the same API host/port

### Frontend (Angular)

The Angular frontend is not scaffolded yet, so it is guarded by the `frontend`
compose profile and skipped by a plain `docker compose up`. Once the app is
scaffolded and its `src/Frontend/OCRWeb.Frontend/Dockerfile` is added, include it
with:

```powershell
docker compose --profile frontend up -d --build
```

- Frontend: http://localhost:4200

### SQL Server

SQL Server is **not** managed by this compose file — it is expected to be running
as a permanent, external instance. The API reads its connection strings from
`src/Backend/OCRWeb.API/appsettings.Development.json`: `ConnectionStrings:OCRWeb`
(Document/OCR's own modules) and `ConnectionStrings:Portal` (required by the
`OrangepuffPortal.Identity` package) — both should point at the same database,
just different schemas; update the host, database, or credentials there.

From inside the containers the host machine's SQL Server is reachable via
`host.docker.internal` (the API service has a `host-gateway` mapping), e.g.:

```text
Server=host.docker.internal,1433;Database=OCRWeb;User Id=sa;Password=Your_strong_password123!;TrustServerCertificate=True;
```

> **Note:** Compose no longer starts SQL Server, so before running the stack make
> sure your external SQL Server has **TCP/IP enabled on port 1433** and that the
> **`OCRWeb` database exists** — the app does not create them for you.

**1. Enable TCP/IP on port 1433**

- Open **SQL Server Configuration Manager** → *SQL Server Network Configuration*
  → *Protocols for \<instance\>* → set **TCP/IP** to **Enabled**.
- In **TCP/IP → Properties → IP Addresses → IPAll**, set **TCP Port** to `1433`
  (clear **TCP Dynamic Ports**).
- Restart the **SQL Server** service (Configuration Manager → *SQL Server Services*).
- Make sure Windows Firewall allows inbound TCP on port 1433:

  ```powershell
  New-NetFirewallRule -DisplayName "SQL Server 1433" -Direction Inbound -Protocol TCP -LocalPort 1433 -Action Allow
  ```

Verify the port is listening:

```powershell
Test-NetConnection -ComputerName localhost -Port 1433
```

**2. Create the `OCRWeb` database**

```powershell
sqlcmd -S localhost,1433 -U sa -P "Your_strong_password123!" -C -Q "IF DB_ID('OCRWeb') IS NULL CREATE DATABASE OCRWeb;"
```

(SQL authentication also requires **Mixed Mode** to be enabled on the server, and
the `sa` login to be enabled — configure this in SSMS under *Server Properties →
Security* if needed.)

HTTPS in Docker uses `certs/ocrweb-devcert.pfx`, mounted into the API container.
Regenerate it if needed:

```powershell
dotnet dev-certs https -ep .\certs\ocrweb-devcert.pfx -p "Your_dev_cert_password123!"
```

Stop the stack:

```powershell
docker compose down
```

