using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

using AutoTracker.Core.Enums;
using AutoTracker.Core.Models;

namespace AutoTracker.ImportExport;

public static class DataImporter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
        PropertyNameCaseInsensitive = true
    };

    // ── CSV Import ────────────────────────────────────────────────────────────

    public static (IReadOnlyList<Vehicle> Vehicles, ImportResult Result) ImportVehiclesCsv(string content)
    {
        var result = new ImportResult();
        var vehicles = new List<Vehicle>();
        var rows = CsvParser.ReadRows(content).ToList();
        if (rows.Count == 0) return (vehicles, result);

        var header = MapHeader(rows[0]);

        for (int i = 1; i < rows.Count; i++)
        {
            var row = rows[i];
            var rowNum = i + 1;
            try
            {
                if (!TryGetInt(row, header, "Year", out var year))
                { result.Skipped.Add($"Row {rowNum}: Invalid Year"); continue; }
                var fuelTypeStr = GetField(row, header, "FuelType");
                if (!Enum.TryParse<FuelType>(fuelTypeStr, true, out var fuelType))
                { result.Skipped.Add($"Row {rowNum}: Invalid FuelType '{fuelTypeStr}'"); continue; }

                decimal? tankCapacity = null;
                var tankStr = GetField(row, header, "TankCapacity");
                if (!string.IsNullOrWhiteSpace(tankStr))
                {
                    if (!decimal.TryParse(tankStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var tc) || tc <= 0)
                    { result.Skipped.Add($"Row {rowNum}: Invalid TankCapacity '{tankStr}'"); continue; }
                    tankCapacity = tc;
                }

                int.TryParse(GetField(row, header, "Id"), out var csvId);
                vehicles.Add(new Vehicle
                {
                    Id = csvId,
                    Name = GetField(row, header, "Name"),
                    Year = year,
                    Make = GetField(row, header, "Make"),
                    Model = GetField(row, header, "Model"),
                    FuelType = fuelType,
                    TankCapacity = tankCapacity,
                    VIN = NullIfEmpty(GetField(row, header, "VIN")),
                    LicensePlate = NullIfEmpty(GetField(row, header, "LicensePlate"))
                });
                result.Imported++;
            }
            catch (Exception ex)
            {
                result.Skipped.Add($"Row {rowNum}: {ex.Message}");
            }
        }
        return (vehicles, result);
    }

    public static (IReadOnlyList<FuelLog> Logs, ImportResult Result) ImportFuelLogsCsv(
        string content, IEnumerable<int> validVehicleIds)
    {
        var vehicleIdSet = validVehicleIds.ToHashSet();
        var result = new ImportResult();
        var logs = new List<FuelLog>();
        var rows = CsvParser.ReadRows(content).ToList();
        if (rows.Count == 0) return (logs, result);

        var header = MapHeader(rows[0]);

        for (int i = 1; i < rows.Count; i++)
        {
            var row = rows[i];
            var rowNum = i + 1;
            try
            {
                if (!TryGetInt(row, header, "VehicleId", out var vehicleId) || !vehicleIdSet.Contains(vehicleId))
                { result.Skipped.Add($"Row {rowNum}: Unknown VehicleId"); continue; }
                if (!TryGetDate(row, header, "Date", out var date))
                { result.Skipped.Add($"Row {rowNum}: Invalid Date"); continue; }
                if (!TryGetInt(row, header, "Odometer", out var odometer) || odometer <= 0)
                { result.Skipped.Add($"Row {rowNum}: Invalid Odometer"); continue; }
                if (!TryGetDecimal(row, header, "Gallons", out var gallons) || gallons <= 0)
                { result.Skipped.Add($"Row {rowNum}: Invalid Gallons"); continue; }
                if (!TryGetDecimal(row, header, "TotalCost", out var totalCost) || totalCost <= 0)
                { result.Skipped.Add($"Row {rowNum}: Invalid TotalCost"); continue; }

                logs.Add(new FuelLog
                {
                    VehicleId = vehicleId,
                    Date = date,
                    Odometer = odometer,
                    Gallons = gallons,
                    TotalCost = totalCost,
                    FuelStation = NullIfEmpty(GetField(row, header, "FuelStation")),
                    Notes = NullIfEmpty(GetField(row, header, "Notes"))
                });
                result.Imported++;
            }
            catch (Exception ex)
            {
                result.Skipped.Add($"Row {rowNum}: {ex.Message}");
            }
        }
        return (logs, result);
    }

    public static (IReadOnlyList<MaintenanceLog> Logs, ImportResult Result) ImportMaintenanceLogsCsv(
        string content, IEnumerable<int> validVehicleIds)
    {
        var vehicleIdSet = validVehicleIds.ToHashSet();
        var result = new ImportResult();
        var logs = new List<MaintenanceLog>();
        var rows = CsvParser.ReadRows(content).ToList();
        if (rows.Count == 0) return (logs, result);

        var header = MapHeader(rows[0]);

        for (int i = 1; i < rows.Count; i++)
        {
            var row = rows[i];
            var rowNum = i + 1;
            try
            {
                if (!TryGetInt(row, header, "VehicleId", out var vehicleId) || !vehicleIdSet.Contains(vehicleId))
                { result.Skipped.Add($"Row {rowNum}: Unknown VehicleId"); continue; }
                if (!TryGetDate(row, header, "Date", out var date))
                { result.Skipped.Add($"Row {rowNum}: Invalid Date"); continue; }
                if (!TryGetInt(row, header, "Odometer", out var odometer) || odometer <= 0)
                { result.Skipped.Add($"Row {rowNum}: Invalid Odometer"); continue; }
                var serviceTypeStr = GetField(row, header, "ServiceType");
                if (!Enum.TryParse<ServiceType>(serviceTypeStr, true, out var serviceType))
                { result.Skipped.Add($"Row {rowNum}: Invalid ServiceType '{serviceTypeStr}'"); continue; }
                if (!TryGetDecimal(row, header, "Cost", out var cost) || cost < 0)
                { result.Skipped.Add($"Row {rowNum}: Invalid Cost"); continue; }

                logs.Add(new MaintenanceLog
                {
                    VehicleId = vehicleId,
                    Date = date,
                    Odometer = odometer,
                    ServiceType = serviceType,
                    Description = NullIfEmpty(GetField(row, header, "Description")),
                    Cost = cost,
                    Vendor = NullIfEmpty(GetField(row, header, "Vendor")),
                    Notes = NullIfEmpty(GetField(row, header, "Notes"))
                });
                result.Imported++;
            }
            catch (Exception ex)
            {
                result.Skipped.Add($"Row {rowNum}: {ex.Message}");
            }
        }
        return (logs, result);
    }

    public static (IReadOnlyList<Expense> Expenses, ImportResult Result) ImportExpensesCsv(
        string content, IEnumerable<int> validVehicleIds)
    {
        var vehicleIdSet = validVehicleIds.ToHashSet();
        var result = new ImportResult();
        var expenses = new List<Expense>();
        var rows = CsvParser.ReadRows(content).ToList();
        if (rows.Count == 0) return (expenses, result);

        var header = MapHeader(rows[0]);

        for (int i = 1; i < rows.Count; i++)
        {
            var row = rows[i];
            var rowNum = i + 1;
            try
            {
                if (!TryGetInt(row, header, "VehicleId", out var vehicleId) || !vehicleIdSet.Contains(vehicleId))
                { result.Skipped.Add($"Row {rowNum}: Unknown VehicleId"); continue; }
                if (!TryGetDate(row, header, "Date", out var date))
                { result.Skipped.Add($"Row {rowNum}: Invalid Date"); continue; }
                var categoryStr = GetField(row, header, "Category");
                if (!Enum.TryParse<ExpenseCategory>(categoryStr, true, out var category))
                { result.Skipped.Add($"Row {rowNum}: Invalid Category '{categoryStr}'"); continue; }
                var description = GetField(row, header, "Description");
                if (string.IsNullOrWhiteSpace(description))
                { result.Skipped.Add($"Row {rowNum}: Description is required"); continue; }
                if (!TryGetDecimal(row, header, "Amount", out var amount) || amount <= 0)
                { result.Skipped.Add($"Row {rowNum}: Invalid Amount"); continue; }

                expenses.Add(new Expense
                {
                    VehicleId = vehicleId,
                    Date = date,
                    Category = category,
                    Description = description,
                    Amount = amount,
                    Notes = NullIfEmpty(GetField(row, header, "Notes"))
                });
                result.Imported++;
            }
            catch (Exception ex)
            {
                result.Skipped.Add($"Row {rowNum}: {ex.Message}");
            }
        }
        return (expenses, result);
    }

    // ── JSON Import ───────────────────────────────────────────────────────────

    public static (IReadOnlyList<Vehicle> Vehicles, ImportResult Result) ImportVehiclesJson(string content)
    {
        var result = new ImportResult();
        try
        {
            var vehicles = JsonSerializer.Deserialize<List<Vehicle>>(content, JsonOptions) ?? [];
            result.Imported = vehicles.Count;
            return (vehicles, result);
        }
        catch (Exception ex)
        {
            result.Skipped.Add($"JSON parse error: {ex.Message}");
            return ([], result);
        }
    }

    public static (IReadOnlyList<FuelLog> Logs, ImportResult Result) ImportFuelLogsJson(
        string content, IEnumerable<int> validVehicleIds)
    {
        var vehicleIdSet = validVehicleIds.ToHashSet();
        var result = new ImportResult();
        try
        {
            var all = JsonSerializer.Deserialize<List<FuelLog>>(content, JsonOptions) ?? [];
            var valid = all.Where(l => vehicleIdSet.Contains(l.VehicleId)).ToList();
            result.Imported = valid.Count;
            result.Skipped.AddRange(all.Where(l => !vehicleIdSet.Contains(l.VehicleId))
                .Select(l => $"FuelLog {l.Id}: Unknown VehicleId {l.VehicleId}"));
            return (valid, result);
        }
        catch (Exception ex)
        {
            result.Skipped.Add($"JSON parse error: {ex.Message}");
            return ([], result);
        }
    }

    public static (IReadOnlyList<MaintenanceLog> Logs, ImportResult Result) ImportMaintenanceLogsJson(
        string content, IEnumerable<int> validVehicleIds)
    {
        var vehicleIdSet = validVehicleIds.ToHashSet();
        var result = new ImportResult();
        try
        {
            var all = JsonSerializer.Deserialize<List<MaintenanceLog>>(content, JsonOptions) ?? [];
            var valid = all.Where(l => vehicleIdSet.Contains(l.VehicleId)).ToList();
            result.Imported = valid.Count;
            result.Skipped.AddRange(all.Where(l => !vehicleIdSet.Contains(l.VehicleId))
                .Select(l => $"MaintenanceLog {l.Id}: Unknown VehicleId {l.VehicleId}"));
            return (valid, result);
        }
        catch (Exception ex)
        {
            result.Skipped.Add($"JSON parse error: {ex.Message}");
            return ([], result);
        }
    }

    public static (IReadOnlyList<Expense> Expenses, ImportResult Result) ImportExpensesJson(
        string content, IEnumerable<int> validVehicleIds)
    {
        var vehicleIdSet = validVehicleIds.ToHashSet();
        var result = new ImportResult();
        try
        {
            var all = JsonSerializer.Deserialize<List<Expense>>(content, JsonOptions) ?? [];
            var valid = all.Where(e => vehicleIdSet.Contains(e.VehicleId)).ToList();
            result.Imported = valid.Count;
            result.Skipped.AddRange(all.Where(e => !vehicleIdSet.Contains(e.VehicleId))
                .Select(e => $"Expense {e.Id}: Unknown VehicleId {e.VehicleId}"));
            return (valid, result);
        }
        catch (Exception ex)
        {
            result.Skipped.Add($"JSON parse error: {ex.Message}");
            return ([], result);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Dictionary<string, int> MapHeader(string[] headerRow) =>
        headerRow.Select((name, idx) => (name.Trim(), idx))
                 .ToDictionary(x => x.Item1, x => x.idx, StringComparer.OrdinalIgnoreCase);

    private static string GetField(string[] row, Dictionary<string, int> header, string column)
    {
        if (!header.TryGetValue(column, out var idx) || idx >= row.Length) return string.Empty;
        return row[idx].Trim();
    }

    private static bool TryGetInt(string[] row, Dictionary<string, int> header, string column, out int value) =>
        int.TryParse(GetField(row, header, column), out value);

    private static bool TryGetDecimal(string[] row, Dictionary<string, int> header, string column, out decimal value) =>
        decimal.TryParse(GetField(row, header, column), NumberStyles.Any, CultureInfo.InvariantCulture, out value);

    private static bool TryGetDate(string[] row, Dictionary<string, int> header, string column, out DateTime value)
    {
        var s = GetField(row, header, column);
        return DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out value);
    }

    private static string? NullIfEmpty(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}
