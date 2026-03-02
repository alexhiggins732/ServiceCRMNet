# Garage Door Service CRM

A multi-tenant, SaaS CRM optimized for Garage Door Services (Housecall Pro-like).
Built using the Microsoft stack (.NET 10, C#, ASP.NET Core API, Blazor Web App, SQL Server, EF Core).

## Architecture & Features

- **Multi-tenant SaaS**: Isolated data at the database layer using EF Core Global Query Filters. Tenants identified via JWT `tid` claim or header overrides.
- **Pluggable Modular System**: Integrations (Meta, Twilio), AI Assistants (OpenAI, Gemini, Grok), Pricing, and Core CRM are independent class libraries implementing `ICrmModule`.
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

5. **Access the Application**:
   > **Note**: Default seed creates tenant "Demo Garage Doors" and user `admin@example.com` / `YourStrong@Passw0rd!`.
   - Web UI (MudBlazor UI): [http://localhost:5002](http://localhost:5002)
   - API Swagger: [http://localhost:5000/swagger](http://localhost:5000/swagger)
   - Hangfire Dashboard: [http://localhost:5000/hangfire](http://localhost:5000/hangfire)
   - Health Check: [http://localhost:5000/health](http://localhost:5000/health)

---

## How to Add a New Module

Adding a new feature domain (e.g., Quickbooks Integration, Advanced Dispatching) is easy:
1. Create a new class library project (e.g., `Crm.Modules.Dispatch`).
2. Add a project reference to `Crm.Shared`.
3. Create a class implementing `ICrmModule`:
   ```csharp
   public class DispatchModule : ICrmModule
   {
       public void RegisterServices(IServiceCollection services, IConfiguration configuration) { ... }
       public void MapEndpoints(IEndpointRouteBuilder endpoints) { ... }
       public void ConfigureBackgroundJobs(IRecurringJobManager jobs) { ... }
   }
   ```
4. Add the project reference to `Crm.Api` and instantiate the module in `Program.cs`.

---

## Testing & Automation

Run the test suite and verify code format:

```powershell
# Run Unit & Integration Tests
.\scripts\dev.ps1 test

# Ensure codebase is formatted correctly
.\scripts\dev.ps1 fmt

# Reset the database to factory settings
.\scripts\dev.ps1 reset-db
```

Continuous Integration automatically runs tests and checks formatting using GitHub Actions.

---

## Roadmap

- [ ] **Payments Integration**: Stripe/Square modules for processing invoice payments online or in the field.
- [ ] **Advanced Dispatching**: Route optimization algorithms and calendar-based map views.
- [ ] **Photos & Attachments**: Upload before/after job photos via S3/Azure Blob integrations.
- [ ] **Reporting Module**: BI dashboards for revenue, tech performance, and lead conversion rates.
