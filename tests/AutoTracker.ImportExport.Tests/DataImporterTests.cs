using AutoTracker.Core.Enums;
using AutoTracker.ImportExport;

namespace AutoTracker.ImportExport.Tests;

public class DataImporterTests
{
    private static readonly int[] VehicleIds = [1, 2];

    // ── Vehicles ──────────────────────────────────────────────────────────────

    [Fact]
    public void ImportVehiclesCsv_ValidRow_Imports()
    {
        const string csv =
            "Id,Name,Year,Make,Model,FuelType,TankCapacity,VIN,LicensePlate\n" +
            "1,My Truck,2020,Ford,F150,Gasoline,,1HGBH41JXMN109186,ABC-123\n";

        var (vehicles, result) = DataImporter.ImportVehiclesCsv(csv);

        Assert.Equal(1, result.Imported);
        Assert.Empty(result.Skipped);
        Assert.Single(vehicles);
        Assert.Equal("My Truck", vehicles[0].Name);
        Assert.Equal(2020, vehicles[0].Year);
    }

    [Fact]
    public void ImportVehiclesCsv_InvalidFuelType_SkipsRow()
    {
        const string csv =
            "Id,Name,Year,Make,Model,FuelType,TankCapacity,VIN,LicensePlate\n" +
            "1,Truck,2020,Ford,F150,InvalidType,,\n";

        var (vehicles, result) = DataImporter.ImportVehiclesCsv(csv);

        Assert.Equal(0, result.Imported);
        Assert.Single(result.Skipped);
        Assert.Empty(vehicles);
    }

    [Fact]
    public void ImportVehiclesCsv_InvalidYear_SkipsRow()
    {
        const string csv =
            "Id,Name,Year,Make,Model,FuelType,TankCapacity,VIN,LicensePlate\n" +
            "1,Truck,not-a-year,Ford,F150,Gasoline,,\n";

        var (_, result) = DataImporter.ImportVehiclesCsv(csv);

        Assert.Equal(0, result.Imported);
    }

    [Fact]
    public void ImportVehiclesCsv_PreservesOriginalId()
    {
        const string csv =
            "Id,Name,Year,Make,Model,FuelType,TankCapacity,VIN,LicensePlate\n" +
            "42,My Truck,2020,Ford,F150,Gasoline,,\n";

        var (vehicles, _) = DataImporter.ImportVehiclesCsv(csv);

        Assert.Equal(42, vehicles[0].Id);
    }

    [Fact]
    public void ImportVehiclesCsv_MissingId_DefaultsToZero()
    {
        const string csv =
            "Name,Year,Make,Model,FuelType\n" +
            "My Truck,2020,Ford,F150,Gasoline\n";

        var (vehicles, _) = DataImporter.ImportVehiclesCsv(csv);

        Assert.Equal(0, vehicles[0].Id);
    }

    // ── Fuel Logs ─────────────────────────────────────────────────────────────

    [Fact]
    public void ImportFuelLogsCsv_ValidRow_Imports()
    {
        const string csv =
            "Id,VehicleId,Date,Odometer,Gallons,TotalCost,FuelStation,Notes\n" +
            "1,1,2024-03-15,50000,10.5,38.50,Shell,Notes here\n";

        var (logs, result) = DataImporter.ImportFuelLogsCsv(csv, VehicleIds);

        Assert.Equal(1, result.Imported);
        Assert.Equal(1, logs[0].VehicleId);
        Assert.Equal(new DateTime(2024, 3, 15), logs[0].Date);
        Assert.Equal(50000, logs[0].Odometer);
        Assert.Equal(10.5m, logs[0].Gallons);
        Assert.Equal(38.50m, logs[0].TotalCost);
        Assert.Equal("Shell", logs[0].FuelStation);
    }

    [Fact]
    public void ImportFuelLogsCsv_UnknownVehicleId_SkipsRow()
    {
        const string csv =
            "Id,VehicleId,Date,Odometer,Gallons,TotalCost,FuelStation,Notes\n" +
            "1,99,2024-01-01,1000,10,35,,\n";

        var (_, result) = DataImporter.ImportFuelLogsCsv(csv, VehicleIds);

        Assert.Equal(0, result.Imported);
        Assert.Single(result.Skipped);
    }

    [Fact]
    public void ImportFuelLogsCsv_ZeroGallons_SkipsRow()
    {
        const string csv =
            "Id,VehicleId,Date,Odometer,Gallons,TotalCost,FuelStation,Notes\n" +
            "1,1,2024-01-01,1000,0,35,,\n";

        var (_, result) = DataImporter.ImportFuelLogsCsv(csv, VehicleIds);

        Assert.Equal(0, result.Imported);
    }

    [Fact]
    public void ImportFuelLogsCsv_InvalidDate_SkipsRow()
    {
        const string csv =
            "Id,VehicleId,Date,Odometer,Gallons,TotalCost,FuelStation,Notes\n" +
            "1,1,not-a-date,50000,10.5,38.50,,\n";

        var (_, result) = DataImporter.ImportFuelLogsCsv(csv, VehicleIds);

        Assert.Equal(0, result.Imported);
        Assert.Single(result.Skipped);
    }

    [Fact]
    public void ImportFuelLogsCsv_ZeroOdometer_SkipsRow()
    {
        const string csv =
            "Id,VehicleId,Date,Odometer,Gallons,TotalCost,FuelStation,Notes\n" +
            "1,1,2024-01-01,0,10.5,38.50,,\n";

        var (_, result) = DataImporter.ImportFuelLogsCsv(csv, VehicleIds);

        Assert.Equal(0, result.Imported);
        Assert.Single(result.Skipped);
    }

    [Fact]
    public void ImportFuelLogsCsv_EmptyContent_ReturnsEmpty()
    {
        var (logs, result) = DataImporter.ImportFuelLogsCsv(string.Empty, VehicleIds);
        Assert.Empty(logs);
        Assert.Equal(0, result.Imported);
    }

    // ── Expenses ──────────────────────────────────────────────────────────────

    [Fact]
    public void ImportExpensesCsv_ValidRow_Imports()
    {
        const string csv =
            "Id,VehicleId,Date,Category,Description,Amount,Notes\n" +
            "1,1,2024-04-01,Insurance,Annual premium,1200,\n";

        var (expenses, result) = DataImporter.ImportExpensesCsv(csv, VehicleIds);

        Assert.Equal(1, result.Imported);
        Assert.Equal("Annual premium", expenses[0].Description);
        Assert.Equal(1200m, expenses[0].Amount);
    }

    [Fact]
    public void ImportExpensesCsv_MissingDescription_SkipsRow()
    {
        const string csv =
            "Id,VehicleId,Date,Category,Description,Amount,Notes\n" +
            "1,1,2024-04-01,Insurance,,500,\n";

        var (_, result) = DataImporter.ImportExpensesCsv(csv, VehicleIds);

        Assert.Equal(0, result.Imported);
        Assert.Single(result.Skipped);
    }

    [Fact]
    public void ImportExpensesCsv_ZeroAmount_SkipsRow()
    {
        const string csv =
            "Id,VehicleId,Date,Category,Description,Amount,Notes\n" +
            "1,1,2024-04-01,Insurance,Premium,0,\n";

        var (_, result) = DataImporter.ImportExpensesCsv(csv, VehicleIds);

        Assert.Equal(0, result.Imported);
    }

    // ── Maintenance Logs ──────────────────────────────────────────────────────

    [Fact]
    public void ImportMaintenanceLogsCsv_ValidRow_Imports()
    {
        const string csv =
            "Id,VehicleId,Date,Odometer,ServiceType,Description,Cost,Vendor,Notes\n" +
            "1,1,2024-05-10,55000,OilChange,Full synthetic,45.00,Jiffy Lube,\n";

        var (logs, result) = DataImporter.ImportMaintenanceLogsCsv(csv, VehicleIds);

        Assert.Equal(1, result.Imported);
        Assert.Single(logs);
        Assert.Equal(ServiceType.OilChange, logs[0].ServiceType);
        Assert.Equal(55000, logs[0].Odometer);
        Assert.Equal(45.00m, logs[0].Cost);
        Assert.Equal("Full synthetic", logs[0].Description);
    }

    [Fact]
    public void ImportMaintenanceLogsCsv_InvalidServiceType_SkipsRow()
    {
        const string csv =
            "Id,VehicleId,Date,Odometer,ServiceType,Description,Cost,Vendor,Notes\n" +
            "1,1,2024-05-10,55000,NotAServiceType,Desc,45.00,,\n";

        var (_, result) = DataImporter.ImportMaintenanceLogsCsv(csv, VehicleIds);

        Assert.Equal(0, result.Imported);
        Assert.Single(result.Skipped);
    }

    [Fact]
    public void ImportMaintenanceLogsCsv_UnknownVehicleId_SkipsRow()
    {
        const string csv =
            "Id,VehicleId,Date,Odometer,ServiceType,Description,Cost,Vendor,Notes\n" +
            "1,99,2024-05-10,55000,OilChange,Desc,45.00,,\n";

        var (_, result) = DataImporter.ImportMaintenanceLogsCsv(csv, VehicleIds);

        Assert.Equal(0, result.Imported);
        Assert.Single(result.Skipped);
    }

    [Fact]
    public void ImportMaintenanceLogsCsv_InvalidDate_SkipsRow()
    {
        const string csv =
            "Id,VehicleId,Date,Odometer,ServiceType,Description,Cost,Vendor,Notes\n" +
            "1,1,not-a-date,55000,OilChange,Desc,45.00,,\n";

        var (_, result) = DataImporter.ImportMaintenanceLogsCsv(csv, VehicleIds);

        Assert.Equal(0, result.Imported);
        Assert.Single(result.Skipped);
    }

    [Fact]
    public void ImportMaintenanceLogsCsv_ZeroCost_Imports()
    {
        const string csv =
            "Id,VehicleId,Date,Odometer,ServiceType,Description,Cost,Vendor,Notes\n" +
            "1,1,2024-05-10,55000,Inspection,Free inspection,0.00,,\n";

        var (logs, result) = DataImporter.ImportMaintenanceLogsCsv(csv, VehicleIds);

        Assert.Equal(1, result.Imported);
        Assert.Single(logs);
        Assert.Equal(0m, logs[0].Cost);
    }

    [Fact]
    public void ImportMaintenanceLogsCsv_NegativeCost_SkipsRow()
    {
        const string csv =
            "Id,VehicleId,Date,Odometer,ServiceType,Description,Cost,Vendor,Notes\n" +
            "1,1,2024-05-10,55000,OilChange,Desc,-10.00,,\n";

        var (_, result) = DataImporter.ImportMaintenanceLogsCsv(csv, VehicleIds);

        Assert.Equal(0, result.Imported);
        Assert.Single(result.Skipped);
    }

    // ── JSON Import ───────────────────────────────────────────────────────────

    [Fact]
    public void ImportVehiclesJson_ValidJson_Imports()
    {
        const string json =
            "[{\"Id\":1,\"Name\":\"My Truck\",\"Year\":2020,\"Make\":\"Ford\",\"Model\":\"F150\",\"FuelType\":\"Gasoline\"}]";

        var (vehicles, result) = DataImporter.ImportVehiclesJson(json);

        Assert.Equal(1, result.Imported);
        Assert.Single(vehicles);
        Assert.Equal("My Truck", vehicles[0].Name);
    }

    [Fact]
    public void ImportVehiclesJson_InvalidJson_ReturnsSkipped()
    {
        var (vehicles, result) = DataImporter.ImportVehiclesJson("not json");

        Assert.Empty(vehicles);
        Assert.Equal(0, result.Imported);
        Assert.Single(result.Skipped);
    }

    [Fact]
    public void ImportFuelLogsJson_UnknownVehicleId_Filtered()
    {
        const string json =
            "[{\"Id\":1,\"VehicleId\":1,\"Date\":\"2024-01-01\",\"Odometer\":1000,\"Gallons\":10.0,\"TotalCost\":35.0}," +
            "{\"Id\":2,\"VehicleId\":99,\"Date\":\"2024-01-02\",\"Odometer\":1100,\"Gallons\":10.0,\"TotalCost\":35.0}]";

        var (logs, result) = DataImporter.ImportFuelLogsJson(json, VehicleIds);

        Assert.Equal(1, result.Imported);
        Assert.Single(logs);
        Assert.Single(result.Skipped);
    }

    [Fact]
    public void ImportExpensesJson_ValidJson_Imports()
    {
        const string json =
            "[{\"Id\":1,\"VehicleId\":1,\"Date\":\"2024-04-01\",\"Category\":\"Insurance\"," +
            "\"Description\":\"Annual premium\",\"Amount\":1200.0}]";

        var (expenses, result) = DataImporter.ImportExpensesJson(json, VehicleIds);

        Assert.Equal(1, result.Imported);
        Assert.Single(expenses);
        Assert.Equal(ExpenseCategory.Insurance, expenses[0].Category);
    }

    [Fact]
    public void ImportExpensesJson_InvalidJson_ReturnsSkipped()
    {
        var (expenses, result) = DataImporter.ImportExpensesJson("not json", VehicleIds);

        Assert.Empty(expenses);
        Assert.Equal(0, result.Imported);
        Assert.Single(result.Skipped);
    }

    [Fact]
    public void ImportMaintenanceLogsJson_ValidJson_Imports()
    {
        const string json =
            "[{\"Id\":1,\"VehicleId\":1,\"Date\":\"2024-05-10\",\"Odometer\":55000," +
            "\"ServiceType\":\"OilChange\",\"Cost\":45.00}]";

        var (logs, result) = DataImporter.ImportMaintenanceLogsJson(json, VehicleIds);

        Assert.Equal(1, result.Imported);
        Assert.Single(logs);
        Assert.Equal(ServiceType.OilChange, logs[0].ServiceType);
        Assert.Equal(55000, logs[0].Odometer);
    }

    [Fact]
    public void ImportMaintenanceLogsJson_UnknownVehicleId_Filtered()
    {
        const string json =
            "[{\"Id\":1,\"VehicleId\":1,\"Date\":\"2024-05-10\",\"Odometer\":55000,\"ServiceType\":\"OilChange\",\"Cost\":45.00}," +
            "{\"Id\":2,\"VehicleId\":99,\"Date\":\"2024-05-11\",\"Odometer\":56000,\"ServiceType\":\"OilChange\",\"Cost\":45.00}]";

        var (logs, result) = DataImporter.ImportMaintenanceLogsJson(json, VehicleIds);

        Assert.Equal(1, result.Imported);
        Assert.Single(logs);
        Assert.Single(result.Skipped);
    }

    [Fact]
    public void ImportMaintenanceLogsJson_InvalidJson_ReturnsSkipped()
    {
        var (logs, result) = DataImporter.ImportMaintenanceLogsJson("not json", VehicleIds);

        Assert.Empty(logs);
        Assert.Equal(0, result.Imported);
        Assert.Single(result.Skipped);
    }
}
