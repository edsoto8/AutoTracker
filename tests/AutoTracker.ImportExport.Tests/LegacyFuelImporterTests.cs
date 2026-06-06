using AutoTracker.Core.Models;
using AutoTracker.ImportExport;

namespace AutoTracker.ImportExport.Tests;

public class LegacyFuelImporterTests
{
    private static readonly List<Vehicle> Vehicles =
    [
        new() { Id = 1, Name = "Truck", Year = 2020, Make = "Ford", Model = "F150" },
        new() { Id = 2, Name = "Car",   Year = 2019, Make = "Honda", Model = "Civic" }
    ];

    private const string ValidCsv =
        "Id,Date,PricePerGallon,Gallons,TotalCost,Location,Vehicle,Odometer,FuelType,Notes\n" +
        "abc,2024-01-15,3.50,10.0,35.00,Shell,Truck,50000,Gasoline,First fill\n" +
        "def,2024-02-01,3.60,12.5,45.00,,Car,30000,Gasoline,\n";

    [Fact]
    public void Import_ValidRows_ImportsBoth()
    {
        var (logs, result) = LegacyFuelImporter.Import(ValidCsv, Vehicles, []);

        Assert.Equal(2, result.Imported);
        Assert.Empty(result.Skipped);
        Assert.Equal(2, logs.Count);
    }

    [Fact]
    public void Import_MapsFieldsCorrectly()
    {
        var (logs, _) = LegacyFuelImporter.Import(ValidCsv, Vehicles, []);
        var log = logs[0];

        Assert.Equal(1, log.VehicleId);
        Assert.Equal(new DateTime(2024, 1, 15), log.Date);
        Assert.Equal(50000, log.Odometer);
        Assert.Equal(10.0m, log.Gallons);
        Assert.Equal(35.00m, log.TotalCost);
        Assert.Equal("Shell", log.FuelStation);
        Assert.Equal("First fill", log.Notes);
    }

    [Fact]
    public void Import_NullsOptionalFields()
    {
        var (logs, _) = LegacyFuelImporter.Import(ValidCsv, Vehicles, []);
        Assert.Null(logs[1].FuelStation);
        Assert.Null(logs[1].Notes);
    }

    [Fact]
    public void Import_UnknownVehicle_SkipsRow()
    {
        const string csv =
            "Id,Date,PricePerGallon,Gallons,TotalCost,Location,Vehicle,Odometer,FuelType,Notes\n" +
            "x,2024-01-01,3.50,10.0,35.00,,Unknown Vehicle,1000,Gasoline,\n";

        var (logs, result) = LegacyFuelImporter.Import(csv, Vehicles, []);

        Assert.Equal(0, result.Imported);
        Assert.Single(result.Skipped);
        Assert.Contains("not found", result.Skipped[0]);
        Assert.Empty(logs);
    }

    [Fact]
    public void Import_InvalidDate_SkipsRow()
    {
        const string csv =
            "Id,Date,PricePerGallon,Gallons,TotalCost,Location,Vehicle,Odometer,FuelType,Notes\n" +
            "x,not-a-date,3.50,10.0,35.00,,Truck,1000,Gasoline,\n";

        var (_, result) = LegacyFuelImporter.Import(csv, Vehicles, []);

        Assert.Equal(0, result.Imported);
        Assert.Single(result.Skipped);
        Assert.Contains("date", result.Skipped[0], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Import_ZeroGallons_SkipsRow()
    {
        const string csv =
            "Id,Date,PricePerGallon,Gallons,TotalCost,Location,Vehicle,Odometer,FuelType,Notes\n" +
            "x,2024-01-01,3.50,0,35.00,,Truck,1000,Gasoline,\n";

        var (_, result) = LegacyFuelImporter.Import(csv, Vehicles, []);

        Assert.Equal(0, result.Imported);
    }

    [Fact]
    public void Import_Deduplication_SkipsDuplicate()
    {
        var existing = new List<FuelLog>
        {
            new() { VehicleId = 1, Date = new DateTime(2024, 1, 15), Odometer = 50000, TotalCost = 35.00m }
        };

        var (logs, result) = LegacyFuelImporter.Import(ValidCsv, Vehicles, existing);

        Assert.Equal(1, result.Imported);
        Assert.Single(result.Skipped);
        Assert.Contains("Duplicate", result.Skipped[0]);
        Assert.Single(logs);
    }

    [Fact]
    public void Import_Idempotent_SecondRunSkipsAll()
    {
        var (firstBatch, _) = LegacyFuelImporter.Import(ValidCsv, Vehicles, []);
        var (secondBatch, result) = LegacyFuelImporter.Import(ValidCsv, Vehicles, firstBatch.ToList());

        Assert.Equal(0, result.Imported);
        Assert.Equal(2, result.SkippedCount);
        Assert.Empty(secondBatch);
    }

    [Fact]
    public void Import_EmptyContent_ReturnsEmpty()
    {
        var (logs, result) = LegacyFuelImporter.Import(string.Empty, Vehicles, []);
        Assert.Empty(logs);
        Assert.Equal(0, result.Imported);
    }

    [Fact]
    public void Import_CaseInsensitiveVehicleMatch()
    {
        const string csv =
            "Id,Date,PricePerGallon,Gallons,TotalCost,Location,Vehicle,Odometer,FuelType,Notes\n" +
            "x,2024-01-01,3.50,10.0,35.00,,TRUCK,50001,Gasoline,\n";

        var (logs, result) = LegacyFuelImporter.Import(csv, Vehicles, []);

        Assert.Equal(1, result.Imported);
        Assert.Single(logs);
        Assert.Equal(1, logs[0].VehicleId);
    }
}
