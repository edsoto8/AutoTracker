using AutoTracker.Core.Enums;

namespace AutoTracker.Core.Models;

public class Vehicle
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string? VIN { get; set; }
    public string? LicensePlate { get; set; }
    public FuelType FuelType { get; set; }
    public decimal? TankCapacity { get; set; }
}
