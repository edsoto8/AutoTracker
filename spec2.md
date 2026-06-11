# AutoTracker — Backlog & Improvement Spec

This document captures findings from a post-implementation codebase audit. Items are grouped by theme and ordered by priority within each group.

---

## Status

The core application is feature-complete. All four entity types have full CRUD across both the CLI and Web UI, import/export works for CSV/JSON/legacy formats, and the dashboard renders metrics and charts. The gaps below are quality and robustness improvements, not missing features.

---

## Testing

### CLI Command Tests

**Priority: High**

21 CLI commands exist with zero test coverage. Spectre.Console supports testing by injecting a test `IAnsiConsole` and mocking repository interfaces.

Commands to cover:

| Command Group | Commands |
|---|---|
| Vehicle | list, add, edit, delete |
| Fuel | list, add, edit, delete |
| Maintenance | list, add, edit, delete |
| Expense | list, add, edit, delete |
| Import/Export | export, import csv, import json, import legacy |
| Dashboard | summary |

Test approach:

* Create a `TestConsole` recorder to capture output
* Mock all four repository interfaces
* Assert on console output and repository call arguments
* Cover happy path and validation error path per command

### Blazor Page / Dialog Tests

**Priority: Medium**

8 pages and 5 dialogs have zero test coverage. Use [bUnit](https://bunit.dev) for component tests.

Components to cover:

| Component | Key behaviors to test |
|---|---|
| `Vehicles.razor` | Renders vehicle list; Add/Edit/Delete buttons trigger correct dialogs |
| `VehicleDialog.razor` | Required fields block submit; valid form calls repository |
| `Fuel.razor` | Vehicle filter narrows rows; Add opens dialog |
| `FuelLogDialog.razor` | $/gal calculated live; odometer warning appears on backward entry |
| `ImportExport.razor` | Export buttons trigger downloads; import shows summary counts |
| `Home.razor` | Empty state shown when no vehicles; metrics rendered when data present |

### Model-Level Validation Tests

**Priority: Low**

Only `FuelLog` has model-level unit tests. Add tests for `Vehicle`, `MaintenanceLog`, and `Expense` covering field constraints (required fields, string lengths, valid enum values).

---

## Data Integrity

### Wrap Imports in Transactions

**Priority: High**

`DataImporter.cs` inserts records one at a time inside a loop. If an exception occurs mid-import, the database is left in a partial state with no way to roll back. Wrap each import method body in a single `IDbTransaction` so the entire import either commits or rolls back atomically.

Affected methods:

* `ImportVehiclesCsvAsync`
* `ImportFuelLogsCsvAsync`
* `ImportMaintenanceLogsCsvAsync`
* `ImportExpensesCsvAsync`
* All four JSON equivalents

### Duplicate Detection in Standard Import

**Priority: High**

`LegacyFuelImporter` deduplicates on `(VehicleId, Date, Odometer, TotalCost)` before inserting. The standard `DataImporter` has no such check — re-importing the same CSV creates duplicate rows. Add deduplication to standard import, using the same key strategy per entity type:

| Entity | Dedup Key |
|---|---|
| FuelLog | `(VehicleId, Date, Odometer, TotalCost)` |
| MaintenanceLog | `(VehicleId, Date, Odometer, ServiceType)` |
| Expense | `(VehicleId, Date, Category, Description, Amount)` |
| Vehicle | `Name` (case-insensitive) |

### Add ON DELETE RESTRICT to Foreign Keys

**Priority: Medium**

`DatabaseInitializer.cs` enables `PRAGMA foreign_keys = ON` but the `FuelLogs`, `MaintenanceLogs`, and `Expenses` foreign key columns do not specify `ON DELETE RESTRICT`. The application-layer `HasDependentsAsync` check is the only guard. A direct database write bypasses this. Explicitly add `ON DELETE RESTRICT` to all child table foreign keys so the constraint is enforced at the database level regardless of how the data is accessed.

Affected lines: `DatabaseInitializer.cs:39`, `:65`, `:76`

---

## Error Handling

### Exception Handling in Blazor Pages

**Priority: High**

No Blazor page or dialog wraps repository calls in try/catch. An unhandled repository exception surfaces as a generic crash page rather than a user-friendly message. Add try/catch around all async data calls in each page and show a `MudSnackbar` error notification on failure.

Pattern to apply consistently:

```csharp
try
{
    await _repo.AddAsync(model);
    Snackbar.Add("Saved.", Severity.Success);
}
catch (Exception ex)
{
    Logger.LogError(ex, "Failed to save.");
    Snackbar.Add("Save failed. See logs for details.", Severity.Error);
}
```

---

## Improvements

### Dark/Light Theme Toggle

**Priority: Medium**

`spec.md` requires a dark/light theme toggle in the Web UI. The current `MainLayout.razor` uses a fixed theme with no toggle. Add a theme toggle button to the nav bar that switches between MudBlazor's built-in dark and light palettes and persists the preference in `localStorage`.

### Date Range Filtering on List Pages

**Priority: Medium**

`spec.md` requires filtering by vehicle and date range on data tables. Vehicle filtering is implemented on Fuel, Maintenance, and Expense pages. Date range filtering (from/to date pickers) is not yet wired up.

### Pagination on Data Grids

**Priority: Medium**

All records are loaded into memory and rendered in full. `MudDataGrid` supports built-in pagination. Add page-size controls (25 / 50 / 100 rows) to all list pages. This is a cosmetic no-op on small datasets but prevents performance degradation as data grows.

### Configurable Log Directory

**Priority: Low**

Both entry points hard-code `~/.autotracker/logs` as the Serilog output path:

* `src/AutoTracker.Cli/Program.cs:19–21`
* `src/AutoTracker.Web/Program.cs:11–13`

Allow override via an `AUTOTRACKER_LOG_PATH` environment variable. Fall back to the current default if unset.

### Model-Level Data Annotations

**Priority: Low**

Validation currently lives only in Spectre.Console prompts (CLI) and MudBlazor `<MudTextField>` attributes (Web). The Core domain models carry no `[Required]`, `[Range]`, or `[MaxLength]` attributes. Adding annotations to `Vehicle`, `FuelLog`, `MaintenanceLog`, and `Expense` gives defense-in-depth and makes field constraints discoverable from the model alone.

---

## Polish

### Group Import Errors by Type

**Priority: Low**

`ImportExport.razor` renders skipped rows as a flat unordered list. When importing a large file with many errors, the list is hard to scan. Group errors by reason (e.g. "Vehicle not found", "Invalid date", "Missing required field") and show a count per group with an expandable detail view.

### Odometer Validation — Decide and Document

**Priority: Low**

The soft odometer warning (allow backward entries with a warning) is intentional per `spec.md` and is implemented correctly in both CLI and Web. However, the reason is not explained in the code. Add a single inline comment at the validation site in `FuelAddCommand.cs` and `FuelLogDialog.razor` explaining that this is intentional to allow backfilling historical data.

---

## Completeness Matrix

| Area | Implemented | Gaps |
|---|---|---|
| Core Models | Complete | No data annotations |
| Core Interfaces | Complete | — |
| Data Layer | Complete | No `ON DELETE RESTRICT`, no import transactions |
| CLI Commands | Complete | No tests |
| Web Pages | Mostly complete | No dark/light toggle, no date range filter, no pagination, no error handling |
| Web Dialogs | Complete | No bUnit tests |
| Import/Export | Complete | No duplicate detection, no transactions |
| Tests — Core | Complete | Add Vehicle/Maintenance/Expense model tests |
| Tests — Data | Complete | — |
| Tests — ImportExport | Complete | — |
| Tests — CLI | Missing | 21 commands untested |
| Tests — Web | Missing | 8 pages, 5 dialogs untested |
