using AutoTracker.Core.Enums;
using AutoTracker.Core.Models;
using AutoTracker.ImportExport;

namespace AutoTracker.ImportExport.Tests;

public class DataExporterTests
{
    [Fact]
    public void ExportVehiclesCsv_ContainsHeader()
    {
        var csv = DataExporter.ExportVehiclesCsv([]);
        Assert.Contains("Id,Name,Year,Make,Model", csv);
    }

    [Fact]
    public void ExportVehiclesCsv_ContainsVehicleData()
    {
        var vehicles = new List<Vehicle>
        {
            new() { Id = 1, Name = "My Truck", Year = 2021, Make = "Ford", Model = "F150", FuelType = FuelType.Gasoline }
        };
        var csv = DataExporter.ExportVehiclesCsv(vehicles);
        Assert.Contains("My Truck", csv);
        Assert.Contains("2021", csv);
        Assert.Contains("Ford", csv);
    }

    [Fact]
    public void ExportVehiclesCsv_EscapesCommasInName()
    {
        var vehicles = new List<Vehicle>
        {
            new() { Id = 1, Name = "Smith, Jr.", Year = 2020, Make = "Ford", Model = "F150", FuelType = FuelType.Gasoline }
        };
        var csv = DataExporter.ExportVehiclesCsv(vehicles);
        Assert.Contains("\"Smith, Jr.\"", csv);
    }

    [Fact]
    public void ExportFuelLogsCsv_ContainsHeader()
    {
        var csv = DataExporter.ExportFuelLogsCsv([]);
        Assert.Contains("Id,VehicleId,Date,Odometer,Gallons,TotalCost", csv);
    }

    [Fact]
    public void ExportFuelLogsCsv_FormatsDateAsIso()
    {
        var logs = new List<FuelLog>
        {
            new() { Id = 1, VehicleId = 1, Date = new DateTime(2024, 6, 15), Odometer = 1000, Gallons = 10m, TotalCost = 35m }
        };
        var csv = DataExporter.ExportFuelLogsCsv(logs);
        Assert.Contains("2024-06-15", csv);
    }

    [Fact]
    public void ExportVehiclesJson_IsValidJson()
    {
        var vehicles = new List<Vehicle>
        {
            new() { Id = 1, Name = "Truck", Year = 2020, Make = "Ford", Model = "F150", FuelType = FuelType.Gasoline }
        };
        var json = DataExporter.ExportVehiclesJson(vehicles);
        Assert.StartsWith("[", json.Trim());
        Assert.Contains("Truck", json);
        Assert.Contains("Gasoline", json);
    }

    [Fact]
    public void ExportExpensesCsv_ContainsAllColumns()
    {
        var expenses = new List<Expense>
        {
            new() { Id = 1, VehicleId = 1, Date = new DateTime(2024, 1, 1), Category = ExpenseCategory.Insurance,
                    Description = "Annual premium", Amount = 1200m }
        };
        var csv = DataExporter.ExportExpensesCsv(expenses);
        Assert.Contains("Insurance", csv);
        Assert.Contains("Annual premium", csv);
        Assert.Contains("1200", csv);
    }
}
