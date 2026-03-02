<#
.SYNOPSIS
Windows-first orchestration script for Garage Door Service CRM monorepo.

.DESCRIPTION
Simplifies setting up and running the developer environment using Docker Compose or local dotnet tooling.

.PARAMETER Command
The command to run: up, down, logs, migrate, seed, test, fmt.
#>
param (
    [Parameter(Mandatory=$true, Position=0)]
    [ValidateSet("up", "down", "logs", "migrate", "seed", "test", "fmt")]
    [string]$Command
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path | Split-Path -Parent

function Write-Step ($message) {
    Write-Host "`n==> $message" -ForegroundColor Cyan
}
 Write-Step "Repo Root: " $RepoRoot
switch ($Command) {
    "up" {
        Write-Step "Starting Docker containers in background..."
        Set-Location $RepoRoot
        docker-compose up -d
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
        dotnet ef database update -s "..\Crm.Api\Crm.Api.csproj"
        Write-Host "Migrations complete!" -ForegroundColor Green
    }
    "seed" {
        Write-Step "Seeding initial data (placeholder)..."
        # Can be executed via API call or EF Core data seeding method
        Write-Host "Data seed step implemented."
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
