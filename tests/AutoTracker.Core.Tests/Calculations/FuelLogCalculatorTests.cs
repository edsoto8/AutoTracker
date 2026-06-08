using AutoTracker.Core.Calculations;
using AutoTracker.Core.Models;

namespace AutoTracker.Core.Tests.Calculations;

public class FuelLogCalculatorTests
{
    [Fact]
    public void SingleEntry_AllComputedValuesAreNull()
    {
        var logs = new[] { MakeLog(1, new DateTime(2024, 1, 1), odometer: 10000, gallons: 10, totalCost: 40) };

        var result = FuelLogCalculator.Calculate(logs);

        Assert.Null(result[0].MilesSinceLastFillup);
        Assert.Null(result[0].Mpg);
        Assert.Null(result[0].CostPerMile);
    }

    [Fact]
    public void TwoEntries_SecondEntryHasCorrectComputedValues()
    {
        var logs = new[]
        {
            MakeLog(1, new DateTime(2024, 1, 1), odometer: 10000, gallons: 10, totalCost: 40),
            MakeLog(2, new DateTime(2024, 1, 15), odometer: 10300, gallons: 10, totalCost: 40),
        };

        var result = FuelLogCalculator.Calculate(logs);

        Assert.Null(result[0].MilesSinceLastFillup);
        Assert.Equal(300, result[1].MilesSinceLastFillup);
        Assert.Equal(30.00m, result[1].Mpg);       // 300 / 10
        Assert.Equal(0.1333m, result[1].CostPerMile); // 40 / 300
    }

    [Fact]
    public void UnsortedInput_OrderedByDateThenOdometer()
    {
        var logs = new[]
        {
            MakeLog(3, new DateTime(2024, 3, 1), odometer: 10600, gallons: 10, totalCost: 40),
            MakeLog(1, new DateTime(2024, 1, 1), odometer: 10000, gallons: 10, totalCost: 40),
            MakeLog(2, new DateTime(2024, 2, 1), odometer: 10300, gallons: 10, totalCost: 40),
        };

        var result = FuelLogCalculator.Calculate(logs);

        Assert.Equal(10000, result[0].Odometer);
        Assert.Equal(10300, result[1].Odometer);
        Assert.Equal(10600, result[2].Odometer);
        Assert.Null(result[0].MilesSinceLastFillup);
        Assert.Equal(300, result[1].MilesSinceLastFillup);
        Assert.Equal(300, result[2].MilesSinceLastFillup);
    }

    [Fact]
    public void SameDateEntries_TiedByOdometerAscending()
    {
        var date = new DateTime(2024, 1, 1);
        var logs = new[]
        {
            MakeLog(2, date, odometer: 10300, gallons: 10, totalCost: 40),
            MakeLog(1, date, odometer: 10000, gallons: 10, totalCost: 40),
        };

        var result = FuelLogCalculator.Calculate(logs);

        Assert.Equal(10000, result[0].Odometer);
        Assert.Equal(10300, result[1].Odometer);
        Assert.Null(result[0].MilesSinceLastFillup);
        Assert.Equal(300, result[1].MilesSinceLastFillup);
    }

    [Fact]
    public void ZeroMilesDriven_CostPerMileIsNull()
    {
        var date = new DateTime(2024, 1, 1);
        var logs = new[]
        {
            MakeLog(1, date, odometer: 10000, gallons: 10, totalCost: 40),
            MakeLog(2, date, odometer: 10000, gallons: 10, totalCost: 40),
        };

        var result = FuelLogCalculator.Calculate(logs);

        Assert.Equal(0, result[1].MilesSinceLastFillup);
        Assert.Null(result[1].CostPerMile);
    }

    [Fact]
    public void ZeroGallons_MpgIsNull()
    {
        var logs = new[]
        {
            MakeLog(1, new DateTime(2024, 1, 1), odometer: 10000, gallons: 10, totalCost: 40),
            MakeLog(2, new DateTime(2024, 2, 1), odometer: 10300, gallons: 0, totalCost: 0),
        };

        var result = FuelLogCalculator.Calculate(logs);

        Assert.Null(result[1].Mpg);
    }

    [Fact]
    public void EmptyInput_ReturnsEmptyList()
    {
        var result = FuelLogCalculator.Calculate([]);
        Assert.Empty(result);
    }

    private static FuelLog MakeLog(int id, DateTime date, int odometer, decimal gallons, decimal totalCost) =>
        new() { Id = id, VehicleId = 1, Date = date, Odometer = odometer, Gallons = gallons, TotalCost = totalCost };
}
