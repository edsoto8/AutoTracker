using AutoTracker.Core.Enums;

namespace AutoTracker.Core.Models;

public class MaintenanceLog
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public DateTime Date { get; set; }
    public int Odometer { get; set; }
    public ServiceType ServiceType { get; set; }
    public string? Description { get; set; }
    public decimal Cost { get; set; }
    public string? Vendor { get; set; }
    public string? Notes { get; set; }
}
