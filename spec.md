# Auto Tracker

## Overview

Auto Tracker is a lightweight vehicle management application for tracking:

* Vehicles
* Fuel Logs
* Maintenance Records
* Vehicle Expenses

The application will provide both:

* A Command Line Interface (CLI) using Spectre.Console
* A Blazor Web UI

All data will be stored locally in SQLite and accessed through Dapper.

---

# Technology Stack

## Platform

* .NET 10
* C#

## Applications

* Blazor Web App
* Spectre.Console CLI

## UI

* MudBlazor

## Data

* SQLite
* Dapper

## Logging

* Serilog

## Testing

* xUnit

---

# Solution Structure

```text
AutoTracker.sln

src/
├── AutoTracker.Core
├── AutoTracker.Data
├── AutoTracker.ImportExport
├── AutoTracker.Cli
└── AutoTracker.Web

tests/
├── AutoTracker.Core.Tests
├── AutoTracker.Data.Tests
└── AutoTracker.ImportExport.Tests
```

---

# Features

## Vehicle Profiles

Store information about vehicles.

### Fields

* Name
* Year
* Make
* Model
* VIN
* License Plate
* Fuel Type
* Tank Capacity

### Actions

* Add Vehicle
* Edit Vehicle
* Delete Vehicle
* View Vehicle List

---

## Fuel Logs

Track fuel purchases and mileage.

### Fields

* Vehicle
* Date
* Odometer
* Gallons
* Total Cost
* Fuel Station
* Notes

### Actions

* Add Fuel Entry
* Edit Fuel Entry
* Delete Fuel Entry
* View Fuel History

### Calculated Values

* MPG
* Cost Per Mile
* Miles Since Last Fill-Up

---

## Maintenance Logs

Track vehicle maintenance.

### Fields

* Vehicle
* Date
* Odometer
* Service Type
* Description
* Cost
* Vendor
* Notes

### Actions

* Add Maintenance Record
* Edit Maintenance Record
* Delete Maintenance Record

---

## Expense Tracking

Track vehicle-related expenses.

### Categories

* Fuel
* Maintenance
* Insurance
* Registration
* Parking
* Tolls
* Repairs
* Other

### Fields

* Vehicle
* Date
* Category
* Description
* Amount
* Notes

### Actions

* Add Expense
* Edit Expense
* Delete Expense

---

## Dashboard

Display summary information.

### Metrics

* Total Fuel Cost
* Total Maintenance Cost
* Total Expenses
* Average MPG
* Total Miles Driven
* Cost Per Mile

### Views

* Recent Fuel Logs
* Recent Maintenance Records
* Expense Breakdown

---

# Import / Export

Support:

## Import

* CSV
* JSON

## Export

* CSV
* JSON

### Supported Data

* Vehicles
* Fuel Logs
* Maintenance Logs
* Expenses

### Requirements

* Validate imported data
* Skip invalid rows
* Show import summary
* Support data backup and restore

---

## Legacy Import

Support a one-time migration from a legacy `fuel_export.csv` file exported by a prior app.

### Source Format

```text
Id,Date,PricePerGallon,Gallons,TotalCost,Location,Vehicle,Odometer,FuelType,Notes
```

| Legacy Column   | Type            | Maps To                          |
|-----------------|-----------------|----------------------------------|
| `Id`            | GUID string     | Ignored (new ID generated)       |
| `Date`          | ISO 8601 string | `FuelLog.Date`                   |
| `PricePerGallon`| decimal         | Derived — not stored directly    |
| `Gallons`       | decimal         | `FuelLog.Gallons`                |
| `TotalCost`     | decimal         | `FuelLog.TotalCost`              |
| `Location`      | string          | `FuelLog.FuelStation`            |
| `Vehicle`       | string (name)   | Matched to `Vehicle.Name`        |
| `Odometer`      | integer         | `FuelLog.Odometer`               |
| `FuelType`      | string          | Informational / ignored          |
| `Notes`         | string          | `FuelLog.Notes`                  |

### Vehicle Matching

* Match the `Vehicle` column to an existing `Vehicle` by name (case-insensitive).
* If no match is found, skip the row and include it in the import summary as unresolved.
* Do not auto-create vehicles during legacy import.

### Validation Rules

* `Date` must parse as a valid date.
* `Gallons` and `TotalCost` must be positive decimals.
* `Odometer` must be a positive integer.
* Rows failing validation are skipped and counted in the summary.

### Import Summary

Report per-row outcomes:

* Imported
* Skipped — vehicle not found
* Skipped — validation error (with reason)

### CLI Command

```bash
AutoTracker import legacy --file fuel_export.csv
```

### Idempotency

Re-running the legacy import on the same file should not create duplicates. Deduplicate on `(VehicleId, Date, Odometer, TotalCost)`.

---

# Database

## Database File

```text
AutoTracker.db
```

## Tables

* Vehicles
* FuelLogs
* MaintenanceLogs
* Expenses

## Schema

SQLite has no native date type. Dates are stored as `TEXT` in ISO 8601 format (`YYYY-MM-DD`); Dapper maps these to C# `DateTime` automatically.

`FuelType` and `ServiceType` are stored as `TEXT` but constrained to fixed enum values enforced at the application layer.

### Vehicles

| Column       | Type    | Constraints               |
|--------------|---------|---------------------------|
| Id           | INTEGER | PRIMARY KEY AUTOINCREMENT |
| Name         | TEXT    | NOT NULL                  |
| Year         | INTEGER | NOT NULL                  |
| Make         | TEXT    | NOT NULL                  |
| Model        | TEXT    | NOT NULL                  |
| VIN          | TEXT    | NULL                      |
| LicensePlate | TEXT    | NULL                      |
| FuelType     | TEXT    | NOT NULL                  |
| TankCapacity | REAL    | NULL (informational only) |

**FuelType enum values:** `Gasoline`, `Diesel`, `Electric`, `Hybrid`, `Other`

### FuelLogs

| Column      | Type    | Constraints                 |
|-------------|---------|-----------------------------|
| Id          | INTEGER | PRIMARY KEY AUTOINCREMENT   |
| VehicleId   | INTEGER | NOT NULL, FK → Vehicles(Id) |
| Date        | TEXT    | NOT NULL (ISO 8601)         |
| Odometer    | INTEGER | NOT NULL                    |
| Gallons     | REAL    | NOT NULL                    |
| TotalCost   | REAL    | NOT NULL                    |
| FuelStation | TEXT    | NULL                        |
| Notes       | TEXT    | NULL                        |

**Indexes:** `VehicleId`, `Date`

### MaintenanceLogs

| Column      | Type    | Constraints                 |
|-------------|---------|-----------------------------|
| Id          | INTEGER | PRIMARY KEY AUTOINCREMENT   |
| VehicleId   | INTEGER | NOT NULL, FK → Vehicles(Id) |
| Date        | TEXT    | NOT NULL (ISO 8601)         |
| Odometer    | INTEGER | NOT NULL                    |
| ServiceType | TEXT    | NOT NULL                    |
| Description | TEXT    | NULL                        |
| Cost        | REAL    | NOT NULL                    |
| Vendor      | TEXT    | NULL                        |
| Notes       | TEXT    | NULL                        |

**ServiceType enum values:** `Oil Change`, `Tire Rotation`, `Brake Service`, `Air Filter`, `Battery`, `Inspection`, `Other`

**Indexes:** `VehicleId`, `Date`

### Expenses

| Column      | Type    | Constraints                 |
|-------------|---------|-----------------------------|
| Id          | INTEGER | PRIMARY KEY AUTOINCREMENT   |
| VehicleId   | INTEGER | NOT NULL, FK → Vehicles(Id) |
| Date        | TEXT    | NOT NULL (ISO 8601)         |
| Category    | TEXT    | NOT NULL                    |
| Description | TEXT    | NOT NULL                    |
| Amount      | REAL    | NOT NULL                    |
| Notes       | TEXT    | NULL                        |

**Category enum values:** `Fuel`, `Maintenance`, `Insurance`, `Registration`, `Parking`, `Tolls`, `Repairs`, `Other`

**Indexes:** `VehicleId`, `Date`

## Delete Behavior

Deleting a vehicle is **blocked** if it has any associated fuel logs, maintenance logs, or expenses. The user must remove all child records first. No cascade deletes — orphaned data is not acceptable.

Foreign keys are enforced via `PRAGMA foreign_keys = ON` at connection open time.

Deleting a fuel log, maintenance log, or expense is always permitted (no dependents).

## Field Validation Rules

### Vehicles

| Field        | Rule                                              |
|--------------|---------------------------------------------------|
| Name         | Required, max 100 chars                           |
| Year         | Required, 1885 – current year + 1                 |
| Make         | Required, max 100 chars                           |
| Model        | Required, max 100 chars                           |
| VIN          | Optional, max 17 chars                            |
| LicensePlate | Optional, max 20 chars                            |
| FuelType     | Required, must be a valid enum value              |
| TankCapacity | Optional, must be > 0 if provided                 |

### Fuel Logs

| Field       | Rule                                               |
|-------------|----------------------------------------------------|
| Date        | Required, valid date, not in the future            |
| Odometer    | Required, must be > 0                              |
| Gallons     | Required, must be > 0                              |
| TotalCost   | Required, must be > 0                              |
| FuelStation | Optional, max 200 chars                            |
| Notes       | Optional, max 200 chars                            |

**Odometer ordering:** If the entered odometer is less than or equal to the most recent prior entry for that vehicle, show a **warning** (not a hard block) — the record is still saved. This accommodates backfilling historical data. During legacy import, the ordering check is skipped entirely.

### Maintenance Logs

| Field       | Rule                                               |
|-------------|----------------------------------------------------|
| Date        | Required, valid date, not in the future            |
| Odometer    | Required, must be > 0                              |
| ServiceType | Required, must be a valid enum value               |
| Cost        | Required, must be ≥ 0 (free services are allowed)  |
| Description | Optional, max 200 chars                            |
| Vendor      | Optional, max 200 chars                            |
| Notes       | Optional, max 200 chars                            |

### Expenses

| Field       | Rule                                               |
|-------------|----------------------------------------------------|
| Date        | Required, valid date, not in the future            |
| Category    | Required, must be a valid enum value               |
| Description | Required, max 200 chars                            |
| Amount      | Required, must be > 0                              |
| Notes       | Optional, max 200 chars                            |

## Computed Value Formulas

Computed on read in `CommandVault.Core`, never stored in the database.

### Miles Since Last Fill-Up

```
MilesSinceLastFillup = CurrentOdometer - PreviousOdometer
```

- Entries ordered by `Date` ascending, then `Odometer` ascending as a tiebreaker.
- First entry for a vehicle has no previous odometer — value is `null`, displayed as `N/A`.

### MPG

```
MPG = MilesSinceLastFillup / Gallons
```

- `null` (displayed as `N/A`) if `MilesSinceLastFillup` is null (first entry).

### Cost Per Mile

```
CostPerMile = TotalCost / MilesSinceLastFillup
```

- `null` (displayed as `N/A`) if `MilesSinceLastFillup` is null or zero (first entry).

## Default Sort Orders

| View                        | Default Sort                              |
|-----------------------------|-------------------------------------------|
| Vehicles                    | Name ascending                            |
| Fuel Logs                   | Date descending, Odometer descending      |
| Maintenance Logs            | Date descending                           |
| Expenses                    | Date descending                           |
| Dashboard recent activity   | Date descending, limit 5 rows per section |

## Post-Action Navigation

| Action                        | Result                                                                 |
|-------------------------------|------------------------------------------------------------------------|
| Add any record                | Close dialog, stay on current list, success snackbar (auto-dismiss)    |
| Edit any record               | Close dialog, stay on current page, success snackbar (auto-dismiss)    |
| Delete any record             | Close confirmation dialog, stay on current page, record disappears     |
| Delete vehicle (blocked)      | Show error snackbar, dialog stays open, no navigation                  |

Success snackbars auto-dismiss after 3 seconds. Error snackbars persist until dismissed.

## Empty / Zero State

| View                                      | Empty State                                                                 |
|-------------------------------------------|-----------------------------------------------------------------------------|
| Dashboard — no vehicles                   | "Add your first vehicle to get started" with an Add Vehicle button          |
| Dashboard — vehicles exist, no logs       | Metrics show `N/A`, charts hidden with "No data yet" message                |
| Vehicle list — no vehicles                | "Add your first vehicle to get started" with an Add Vehicle button          |
| Fuel / Maintenance / Expense lists        | "No records yet. Add one to get started."                                   |
| Vehicle detail tabs — no records          | "No [fuel logs / maintenance records / expenses] for this vehicle yet."     |

## CLI UX Mode

Interactive — Spectre.Console prompts for each field one at a time. No flags required. Commands guide the user through input with validation at each step, re-prompting on invalid input.

---

# Data Access

Use Dapper repositories.

Examples:

```csharp
IVehicleRepository
IFuelLogRepository
IMaintenanceRepository
IExpenseRepository
```

No Entity Framework.

---

# CLI Requirements

Use Spectre.Console.

Example Commands:

```bash
AutoTracker vehicle list
AutoTracker vehicle add

AutoTracker fuel add
AutoTracker fuel list

AutoTracker maintenance add

AutoTracker expense add

AutoTracker dashboard

AutoTracker import
AutoTracker export
```

---

# Web Application Requirements

Use MudBlazor as the component library.

## Pages

```text
/                   Dashboard
/vehicles           Vehicle list
/vehicles/{id}      Vehicle detail (fuel, maintenance, expenses in tabs)
/fuel               Fuel log list (all vehicles)
/maintenance        Maintenance log list (all vehicles)
/expenses           Expense list (all vehicles)
/import-export      Import and export
```

## Requirements

* Responsive layout using MudBlazor's grid system
* Form validation with inline field errors
* Success/failure feedback via MudSnackbar
* Confirmation dialogs via MudDialog before deletes
* Data tables with sorting, filtering by vehicle and date range, and pagination
* Add/edit forms open in MudDialog overlays (not separate pages)
* Dark/light theme toggle

## Dashboard Charts (MudChart)

* MPG over time — line chart, per vehicle
* Monthly fuel cost — bar chart
* Expense breakdown by category — donut chart

---

# Logging

Use Serilog.

Log to:

* Console
* File

Log:

* Application startup
* Errors
* Database operations
* Import/export activity

---

# Testing

Use xUnit.

Cover:

* Calculations
* Repository CRUD operations
* Import/Export functionality
* Validation rules

---

# Initial Development Order

1. Create solution and projects
2. Create domain models
3. Build SQLite database
4. Implement Dapper repositories
5. Build Fuel Log functionality
6. Build Maintenance functionality
7. Build Expense Tracking
8. Create Dashboard
9. Add Import/Export
10. Add CLI commands
11. Build Blazor UI
12. Add Logging and Tests

```
```
