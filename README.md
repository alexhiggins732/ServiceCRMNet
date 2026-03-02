# Garage Door Service CRM

A multi-tenant, SaaS CRM optimized for Garage Door Services (Housecall Pro-like).
Built using the Microsoft stack (.NET 10, C#, ASP.NET Core API, Blazor Server, SQL Server, EF Core).

## Architecture & Features

- **Multi-tenant SaaS**: Isolated data at the database layer using EF Core Global Query Filters. Tenants identified via JWT `tid` claim or header overrides.
- **Pluggable Modular System**: Integrations (Meta, Twilio), AI Assistants (OpenAI, Gemini, Grok), and domain modules are independent class libraries implementing `IModule`.
- **Reliability Primitives**: Incoming webhook requests use `Idempotency-Key` and Hangfire for robust job queueing and background processing. Structured logging via `X-Correlation-ID`.
- **Unified Communications Inbox**: Normalize inbound messages across SMS, Email, and Meta via webhook processors.
- **Pricing & Estimates Book**: Dedicated module for Service items, customizable pricing templates, and estimate-to-invoice pipeline.
- **Docker/Windows First**: Simplified bootstrapping for developers using Windows and Docker.

---

## Prerequisites (Windows)

- [Docker Desktop for Windows](https://docs.docker.com/desktop/install/windows-install/) (Make sure it is running)
- [.NET 10.0 SDK](https://dotnet.microsoft.com/en-us/download)
- [PowerShell](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell-on-windows)
- EF Core CLI Tools: `dotnet tool install --global dotnet-ef`

---

## Quickstart: Docker + Windows (Recommended)

Boot up the entire stack using Docker and our `dev.ps1` orchestrator. The API, Web UI, SQL Server 2022, and Hangfire Dashboard will be containerized.

1. **Clone the repository**:
   ```powershell
   git clone <repo-url>
   cd <repo-dir>
   ```

2. **Configure Environment Variables**:
   ```powershell
   Copy-Item .env.example .env
   # Edit .env and supply your desired SA_PASSWORD, Secrets, and API Keys
   ```

3. **Start the containers**:
   ```powershell
   .\scripts\dev.ps1 up
   ```

4. **Run EF Core Migrations**:
   Wait a few seconds for SQL Server to boot completely, then run:
   ```powershell
   .\scripts\dev.ps1 migrate
   ```

5. **Seed Initial Data**:
   ```powershell
   .\scripts\dev.ps1 seed
   ```
   > **Note**: Seed creates a default tenant (`default`) and admin (`admin@example.com` / `YourStrong@Passw0rd!`).

6. **Access the Application**:
   - Web UI: [http://localhost:5002](http://localhost:5002)
   - API Swagger: [http://localhost:5000/swagger](http://localhost:5000/swagger)
   - Hangfire Dashboard: [http://localhost:5000/hangfire](http://localhost:5000/hangfire)

---

## Non-Docker Local Development

If you prefer to run the dotnet projects directly on your host machine against a local SQL Server Express instance:

1. **Update Connection String**:
   In `src/Crm.Api/appsettings.Development.json` (or using User Secrets), update the database connection string:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CrmDb;Trusted_Connection=True;MultipleActiveResultSets=true"
   }
   ```

2. **Apply Migrations**:
   ```powershell
   cd src\Crm.Infrastructure
   dotnet ef database update -s ..\Crm.Api\Crm.Api.csproj
   ```

3. **Run the API**:
   ```powershell
   cd src\Crm.Api
   dotnet run
   ```

4. **Run the Web App**:
   ```powershell
   cd src\Crm.Web
   dotnet run
   ```

---

## Testing & Automation

Run the test suite and verify code format:

```powershell
# Run Unit & Integration Tests
.\scripts\dev.ps1 test

# Ensure codebase is formatted correctly
.\scripts\dev.ps1 fmt
```

Continuous Integration automatically runs tests and checks formatting using GitHub Actions.
