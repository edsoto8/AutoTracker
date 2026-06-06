namespace AutoTracker.Core.Models;

public class FuelLog
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public DateTime Date { get; set; }
    public int Odometer { get; set; }
    public decimal Gallons { get; set; }
    public decimal TotalCost { get; set; }
    public string? FuelStation { get; set; }
    public string? Notes { get; set; }

    // Computed on read by FuelLogCalculator — null for the first entry per vehicle
    public int? MilesSinceLastFillup { get; set; }
    public decimal? Mpg { get; set; }
    public decimal? CostPerMile { get; set; }
}
