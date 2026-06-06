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

Pages:

```text
/
Dashboard

/vehicles
/fuel
/maintenance
/expenses
/import-export
```

Requirements:

* Responsive UI
* Form validation
* Search and filtering
* Data tables
* Confirmation dialogs

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
