using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using AutoTracker.Core.Models;

namespace AutoTracker.ImportExport;

public static class DataExporter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    // ── CSV ──────────────────────────────────────────────────────────────────

    public static string ExportVehiclesCsv(IEnumerable<Vehicle> vehicles)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Id,Name,Year,Make,Model,FuelType,TankCapacity,VIN,LicensePlate");
        foreach (var v in vehicles)
        {
            sb.AppendLine(string.Join(",",
                v.Id,
                CsvParser.Escape(v.Name),
                v.Year,
                CsvParser.Escape(v.Make),
                CsvParser.Escape(v.Model),
                v.FuelType,
                v.TankCapacity?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                CsvParser.Escape(v.VIN),
                CsvParser.Escape(v.LicensePlate)));
        }
        return sb.ToString();
    }

    public static string ExportFuelLogsCsv(IEnumerable<FuelLog> logs)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Id,VehicleId,Date,Odometer,Gallons,TotalCost,FuelStation,Notes");
        foreach (var l in logs)
        {
            sb.AppendLine(string.Join(",",
                l.Id,
                l.VehicleId,
                l.Date.ToString("yyyy-MM-dd"),
                l.Odometer,
                l.Gallons.ToString(CultureInfo.InvariantCulture),
                l.TotalCost.ToString(CultureInfo.InvariantCulture),
                CsvParser.Escape(l.FuelStation),
                CsvParser.Escape(l.Notes)));
        }
        return sb.ToString();
    }

    public static string ExportMaintenanceLogsCsv(IEnumerable<MaintenanceLog> logs)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Id,VehicleId,Date,Odometer,ServiceType,Description,Cost,Vendor,Notes");
        foreach (var l in logs)
        {
            sb.AppendLine(string.Join(",",
                l.Id,
                l.VehicleId,
                l.Date.ToString("yyyy-MM-dd"),
                l.Odometer,
                l.ServiceType,
                CsvParser.Escape(l.Description),
                l.Cost.ToString(CultureInfo.InvariantCulture),
                CsvParser.Escape(l.Vendor),
                CsvParser.Escape(l.Notes)));
        }
        return sb.ToString();
    }

    public static string ExportExpensesCsv(IEnumerable<Expense> expenses)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Id,VehicleId,Date,Category,Description,Amount,Notes");
        foreach (var e in expenses)
        {
            sb.AppendLine(string.Join(",",
                e.Id,
                e.VehicleId,
                e.Date.ToString("yyyy-MM-dd"),
                e.Category,
                CsvParser.Escape(e.Description),
                e.Amount.ToString(CultureInfo.InvariantCulture),
                CsvParser.Escape(e.Notes)));
        }
        return sb.ToString();
    }

    // ── JSON ─────────────────────────────────────────────────────────────────

    public static string ExportVehiclesJson(IEnumerable<Vehicle> vehicles) =>
        JsonSerializer.Serialize(vehicles, JsonOptions);

    public static string ExportFuelLogsJson(IEnumerable<FuelLog> logs) =>
        JsonSerializer.Serialize(logs, JsonOptions);

    public static string ExportMaintenanceLogsJson(IEnumerable<MaintenanceLog> logs) =>
        JsonSerializer.Serialize(logs, JsonOptions);

    public static string ExportExpensesJson(IEnumerable<Expense> expenses) =>
        JsonSerializer.Serialize(expenses, JsonOptions);
}
