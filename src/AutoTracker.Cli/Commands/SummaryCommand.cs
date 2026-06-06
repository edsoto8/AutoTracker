using AutoTracker.Core.Calculations;
using AutoTracker.Core.Interfaces;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AutoTracker.Cli.Commands;

public class SummaryCommand : AsyncCommand
{
    private readonly IVehicleRepository _vehicleRepo;
    private readonly IFuelLogRepository _fuelRepo;
    private readonly IMaintenanceRepository _maintenanceRepo;

    public SummaryCommand(IVehicleRepository vehicleRepo, IFuelLogRepository fuelRepo, IMaintenanceRepository maintenanceRepo)
    {
        _vehicleRepo = vehicleRepo;
        _fuelRepo = fuelRepo;
        _maintenanceRepo = maintenanceRepo;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
    {
        var vehicles = (await _vehicleRepo.GetAllAsync()).ToList();
        var allFuelLogs = (await _fuelRepo.GetAllAsync()).ToList();
        var allMaintenanceLogs = (await _maintenanceRepo.GetAllAsync()).ToList();

        var calculated = FuelLogCalculator.Calculate(allFuelLogs);
        var totalMiles = calculated.Where(l => l.MilesSinceLastFillup.HasValue).Sum(l => l.MilesSinceLastFillup!.Value);
        var totalFuelCost = allFuelLogs.Sum(l => l.TotalCost);
        var totalMaintenanceCost = allMaintenanceLogs.Sum(l => l.Cost);
        var mpgValues = calculated.Where(l => l.Mpg.HasValue).Select(l => l.Mpg!.Value).ToList();
        var avgMpg = mpgValues.Count > 0 ? mpgValues.Average() : (decimal?)null;
        var costPerMile = totalMiles > 0 ? (totalFuelCost + totalMaintenanceCost) / totalMiles : (decimal?)null;

        AnsiConsole.MarkupLine("[bold]AutoTracker Summary[/]\n");

        var overallTable = new Table().BorderColor(Color.Grey).Expand();
        overallTable.AddColumn("[grey]Metric[/]");
        overallTable.AddColumn("[grey]Value[/]");
        overallTable.AddRow("Vehicles", vehicles.Count.ToString());
        overallTable.AddRow("Fuel Logs", allFuelLogs.Count.ToString());
        overallTable.AddRow("Total Fuel Cost", $"${totalFuelCost:F2}");
        overallTable.AddRow("Maintenance Logs", allMaintenanceLogs.Count.ToString());
        overallTable.AddRow("Total Maintenance Cost", $"${totalMaintenanceCost:F2}");
        overallTable.AddRow("Average MPG", avgMpg.HasValue ? avgMpg.Value.ToString("F1") : "[grey]N/A[/]");
        overallTable.AddRow("Total Miles Driven", totalMiles > 0 ? totalMiles.ToString("N0") : "[grey]N/A[/]");
        overallTable.AddRow("Cost Per Mile", costPerMile.HasValue ? $"${costPerMile.Value:F4}" : "[grey]N/A[/]");
        AnsiConsole.Write(overallTable);

        if (vehicles.Count == 0) return 0;

        AnsiConsole.MarkupLine("\n[bold]Per-Vehicle Breakdown[/]\n");

        var vehicleTable = new Table().BorderColor(Color.Grey);
        vehicleTable.AddColumn("[bold]Vehicle[/]");
        vehicleTable.AddColumn("[bold]Fill-ups[/]");
        vehicleTable.AddColumn("[bold]Fuel Cost[/]");
        vehicleTable.AddColumn("[bold]Maintenance[/]");
        vehicleTable.AddColumn("[bold]Avg MPG[/]");
        vehicleTable.AddColumn("[bold]Miles Driven[/]");

        foreach (var vehicle in vehicles)
        {
            var vFuelLogs = allFuelLogs.Where(l => l.VehicleId == vehicle.Id).ToList();
            var vCalc = FuelLogCalculator.Calculate(vFuelLogs);
            var vMpgValues = vCalc.Where(l => l.Mpg.HasValue).Select(l => l.Mpg!.Value).ToList();
            var vAvgMpg = vMpgValues.Count > 0 ? vMpgValues.Average() : (decimal?)null;
            var vMiles = vCalc.Where(l => l.MilesSinceLastFillup.HasValue).Sum(l => l.MilesSinceLastFillup!.Value);
            var vFuelCost = vFuelLogs.Sum(l => l.TotalCost);
            var vMaintenanceCost = allMaintenanceLogs.Where(l => l.VehicleId == vehicle.Id).Sum(l => l.Cost);

            vehicleTable.AddRow(
                $"{vehicle.Name} [grey]({vehicle.Year} {vehicle.Make})[/]",
                vFuelLogs.Count.ToString(),
                $"${vFuelCost:F2}",
                $"${vMaintenanceCost:F2}",
                vAvgMpg.HasValue ? vAvgMpg.Value.ToString("F1") : "[grey]N/A[/]",
                vMiles > 0 ? vMiles.ToString("N0") : "[grey]N/A[/]"
            );
        }

        AnsiConsole.Write(vehicleTable);
        return 0;
    }
}
