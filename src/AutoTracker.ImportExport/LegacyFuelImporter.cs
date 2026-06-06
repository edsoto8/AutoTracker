using System.Globalization;
using AutoTracker.Core.Models;

namespace AutoTracker.ImportExport;

public static class LegacyFuelImporter
{
    // Source format: Id,Date,PricePerGallon,Gallons,TotalCost,Location,Vehicle,Odometer,FuelType,Notes

    public static (IReadOnlyList<FuelLog> ToImport, ImportResult Result) Import(
        string content,
        IReadOnlyList<Vehicle> vehicles,
        IReadOnlyList<FuelLog> existingLogs)
    {
        var vehicleMap = vehicles.ToDictionary(
            v => v.Name.Trim(), v => v.Id, StringComparer.OrdinalIgnoreCase);

        var existingKeys = existingLogs
            .Select(l => (l.VehicleId, l.Date.Date, l.Odometer, l.TotalCost))
            .ToHashSet();

        var result = new ImportResult();
        var toImport = new List<FuelLog>();

        var rows = CsvParser.ReadRows(content).ToList();
        if (rows.Count == 0) return (toImport, result);

        var header = rows[0].Select((col, i) => (col.Trim(), i))
                            .ToDictionary(x => x.Item1, x => x.i, StringComparer.OrdinalIgnoreCase);

        for (int i = 1; i < rows.Count; i++)
        {
            var row = rows[i];
            var rowNum = i + 1;

            var vehicleName = Get(row, header, "Vehicle");
            if (!vehicleMap.TryGetValue(vehicleName, out var vehicleId))
            {
                result.Skipped.Add($"Row {rowNum}: Vehicle '{vehicleName}' not found");
                continue;
            }

            var dateStr = Get(row, header, "Date");
            if (!DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                result.Skipped.Add($"Row {rowNum}: Invalid date '{dateStr}'");
                continue;
            }

            var gallonsStr = Get(row, header, "Gallons");
            if (!decimal.TryParse(gallonsStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var gallons) || gallons <= 0)
            {
                result.Skipped.Add($"Row {rowNum}: Invalid gallons '{gallonsStr}'");
                continue;
            }

            var totalCostStr = Get(row, header, "TotalCost");
            if (!decimal.TryParse(totalCostStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var totalCost) || totalCost <= 0)
            {
                result.Skipped.Add($"Row {rowNum}: Invalid total cost '{totalCostStr}'");
                continue;
            }

            var odometerStr = Get(row, header, "Odometer");
            if (!int.TryParse(odometerStr, out var odometer) || odometer <= 0)
            {
                result.Skipped.Add($"Row {rowNum}: Invalid odometer '{odometerStr}'");
                continue;
            }

            var key = (vehicleId, date.Date, odometer, totalCost);
            if (existingKeys.Contains(key))
            {
                result.Skipped.Add($"Row {rowNum}: Duplicate (already imported)");
                continue;
            }

            toImport.Add(new FuelLog
            {
                VehicleId = vehicleId,
                Date = date,
                Odometer = odometer,
                Gallons = gallons,
                TotalCost = totalCost,
                FuelStation = NullIfEmpty(Get(row, header, "Location")),
                Notes = NullIfEmpty(Get(row, header, "Notes"))
            });

            existingKeys.Add(key);
            result.Imported++;
        }

        return (toImport, result);
    }

    private static string Get(string[] row, Dictionary<string, int> header, string col)
    {
        if (!header.TryGetValue(col, out var idx) || idx >= row.Length) return string.Empty;
        return row[idx].Trim();
    }

    private static string? NullIfEmpty(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}
