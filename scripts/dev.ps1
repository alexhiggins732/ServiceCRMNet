<#
.SYNOPSIS
Windows-first orchestration script for Garage Door Service CRM monorepo.

.DESCRIPTION
Simplifies setting up and running the developer environment using Docker Compose or local dotnet tooling.

.PARAMETER Command
The command to run: up, down, logs, migrate, seed, test, fmt, reset-db.
#>
param (
    [Parameter(Mandatory=$true, Position=0)]
    [ValidateSet("up", "down", "logs", "migrate", "seed", "test", "fmt", "reset-db")]
    [string]$Command
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path | Split-Path -Parent

function Write-Step ($message) {
    Write-Host "`n==> $message" -ForegroundColor Cyan
}

switch ($Command) {
    "up" {
        Write-Step "Starting Docker containers in background..."
        Set-Location $RepoRoot
        docker-compose up --build -d
        Write-Host "Containers starting. API will be on http://localhost:5000, Web on http://localhost:5002"
    }
    "down" {
        Write-Step "Stopping Docker containers..."
        Set-Location $RepoRoot
        docker-compose down
    }
    "logs" {
        Write-Step "Tailing Docker logs..."
        Set-Location $RepoRoot
        docker-compose logs -f
    }
    "migrate" {
        Write-Step "Running Entity Framework migrations..."
        Set-Location "$RepoRoot\src\Crm.Infrastructure"

        # Try migration, catch and give instructions to reset
        try {
            dotnet ef database update -s "..\Crm.Api\Crm.Api.csproj"
            Write-Host "Migrations complete!" -ForegroundColor Green
        } catch {
            Write-Host "Migration failed! The database might contain old tables. Try running: .\scripts\dev.ps1 reset-db" -ForegroundColor Red
            throw
        }
    }
    "seed" {
        Write-Step "Seeding initial data..."
        Write-Host "Data seed is executed automatically on API startup in Development."
    }
    "reset-db" {
        Write-Step "Resetting local database..."
        Set-Location "$RepoRoot\src\Crm.Infrastructure"
        dotnet ef database drop -f -s "..\Crm.Api\Crm.Api.csproj"
        dotnet ef database update -s "..\Crm.Api\Crm.Api.csproj"
        Write-Host "Database reset complete!" -ForegroundColor Green
    }
    "test" {
        Write-Step "Running all tests..."
        Set-Location $RepoRoot
        dotnet test Crm.sln
    }
    "fmt" {
        Write-Step "Formatting code..."
        Set-Location $RepoRoot
        dotnet format Crm.sln
    }
}
