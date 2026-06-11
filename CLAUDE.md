# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Status

Spec is complete. Implementation is complete for all features in `spec.md`.

A post-implementation audit was captured in `spec2.md` — review it before resuming development. It covers testing gaps (CLI commands and Blazor pages have zero test coverage), data integrity issues (no import transactions, no duplicate detection, no `ON DELETE RESTRICT`), missing error handling in Blazor pages, and lower-priority polish items (dark/light theme toggle, date range filtering, pagination).

## Project

**AutoTracker** — a local-first vehicle management app for tracking fuel logs, maintenance records, and expenses. Stores data in SQLite via Dapper. Exposes a Spectre.Console CLI and a Blazor Web UI.

## Technology Stack

- **.NET 10**, C#
- **SQLite + Dapper** (no Entity Framework)
- **Spectre.Console** for the CLI
- **Blazor Web App** with **MudBlazor** for the UI
- **Serilog** for logging (console + file)
- **xUnit** for tests

## Solution Structure

```
AutoTracker.slnx
src/
  AutoTracker.Core          # Domain models, interfaces
  AutoTracker.Data          # Dapper repositories, SQLite schema
  AutoTracker.ImportExport  # CSV/JSON import and export
  AutoTracker.Cli           # Spectre.Console entry point
  AutoTracker.Web           # Blazor Web App
tests/
  AutoTracker.Core.Tests
  AutoTracker.Data.Tests
  AutoTracker.ImportExport.Tests
```

## Common Commands

```bash
# Build entire solution
dotnet build

# Run CLI
dotnet run --project src/AutoTracker.Cli

# Run web app
dotnet run --project src/AutoTracker.Web

# Run all tests
dotnet test

# Run a single test project
dotnet test tests/AutoTracker.Data.Tests

# Run a single test by name
dotnet test --filter "FullyQualifiedName~VehicleRepository"
```

## Formatting

```bash
# Format entire solution
dotnet format

# Format and verify (no changes written — useful in CI)
dotnet format --verify-no-changes

# Format a single project
dotnet format src/AutoTracker.Web
```

Run `dotnet format` before committing to keep style consistent.

## Package Management

```bash
# Check for outdated packages (built-in)
dotnet list package --outdated

# Install the dotnet-outdated global tool (one-time)
dotnet tool install -g dotnet-outdated-tool

# Show outdated packages
dotnet outdated

# Update minor/patch versions only (safer, avoids breaking major changes)
dotnet outdated -u:Minor

# Update all packages (review changes carefully)
dotnet outdated -u
```

After updating packages, always rebuild and test:
```bash
dotnet build && dotnet test
```

## Architecture Notes

**Data layer** (`AutoTracker.Data`): Repository pattern via interfaces defined in `AutoTracker.Core` (`IVehicleRepository`, `IFuelLogRepository`, `IMaintenanceRepository`, `IExpenseRepository`). Concrete implementations use Dapper against a single SQLite file (`autotracker.db`). Schema migrations are owned by this project.

**Core** (`AutoTracker.Core`): Domain models (Vehicle, FuelLog, MaintenanceLog, Expense) and computed values (MPG, cost-per-mile). No infrastructure dependencies.

**CLI** (`AutoTracker.Cli`): Spectre.Console commands follow the pattern `autotracker <noun> <verb>` (e.g. `autotracker vehicle list`). Commands depend on `AutoTracker.Core` interfaces; repositories are injected.

**Web** (`AutoTracker.Web`): Blazor pages at `/`, `/vehicles`, `/vehicles/{id}`, `/fuel`, `/maintenance`, `/expenses`, `/import-export`. Uses MudBlazor components — MudDataGrid for tables, MudDialog for add/edit forms and confirmations, MudSnackbar for feedback, MudChart for dashboard charts. Shares the same repository interfaces as the CLI.

**ImportExport** (`AutoTracker.ImportExport`): CSV and JSON round-trips for all four entity types. Must validate imported rows, skip invalid ones, and return an import summary.

**Logging**: Serilog is wired at the application entry points (CLI and Web). Log startup, errors, and import/export activity. Logs written to `~/.autotracker/logs/`.

## Database

Single file `autotracker.db` (at `~/.autotracker/autotracker.db`) with four tables: `Vehicles`, `FuelLogs`, `MaintenanceLogs`, `Expenses`. All access goes through Dapper repositories — raw SQL only, no ORM.

## Fuel Log Calculated Values

Computed on read, not stored: MPG, cost-per-mile, miles-since-last-fill-up, price-per-gallon. These belong in `AutoTracker.Core` and are unit-tested there.
