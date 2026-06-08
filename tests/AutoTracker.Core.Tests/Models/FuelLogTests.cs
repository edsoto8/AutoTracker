using AutoTracker.Core.Models;

namespace AutoTracker.Core.Tests.Models;

public class FuelLogTests
{
    [Fact]
    public void PricePerGallon_IsComputedFromTotalCostAndGallons()
    {
        var log = new FuelLog { TotalCost = 45m, Gallons = 10m };

        Assert.Equal(4.5m, log.PricePerGallon);
    }

    [Fact]
    public void PricePerGallon_ZeroGallons_ReturnsZero()
    {
        var log = new FuelLog { TotalCost = 45m, Gallons = 0m };

        Assert.Equal(0m, log.PricePerGallon);
    }
}
