using AutoTracker.Core.Calculations;
using AutoTracker.Core.Interfaces;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AutoTracker.Cli.Commands.Fuel;

public class FuelListCommand : AsyncCommand
{
    private readonly IVehicleRepository _vehicleRepo;
    private readonly IFuelLogRepository _fuelRepo;

    public FuelListCommand(IVehicleRepository vehicleRepo, IFuelLogRepository fuelRepo)
    {
        _vehicleRepo = vehicleRepo;
        _fuelRepo = fuelRepo;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
    {
        var vehicles = (await _vehicleRepo.GetAllAsync()).ToList();

        if (vehicles.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]No vehicles found.[/]");
            return 0;
        }

        var vehicleMap = vehicles.ToDictionary(v => v.Id, v => v.Name);

        // Compute values per vehicle, then flatten and sort for display
        var allLogs = new List<Core.Models.FuelLog>();
        foreach (var vehicle in vehicles)
        {
            var logs = await _fuelRepo.GetByVehicleIdAsync(vehicle.Id);
            allLogs.AddRange(FuelLogCalculator.Calculate(logs));
        }

        var sorted = allLogs.OrderByDescending(l => l.Date).ThenByDescending(l => l.Odometer).ToList();

        if (sorted.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]No fuel logs found. Use [blue]fuel add[/] to add one.[/]");
            return 0;
        }

        var table = new Table().BorderColor(Color.Grey);
        table.AddColumn("[bold]Date[/]");
        table.AddColumn("[bold]Vehicle[/]");
        table.AddColumn("[bold]Odometer[/]");
        table.AddColumn("[bold]Gallons[/]");
        table.AddColumn("[bold]Total Cost[/]");
        table.AddColumn("[bold]MPG[/]");
        table.AddColumn("[bold]¢/Mile[/]");
        table.AddColumn("[bold]Mi Since Fill[/]");
        table.AddColumn("[bold]Station[/]");

        foreach (var log in sorted)
        {
            table.AddRow(
                log.Date.ToString("yyyy-MM-dd"),
                vehicleMap.TryGetValue(log.VehicleId, out var name) ? name : "?",
                log.Odometer.ToString("N0"),
                log.Gallons.ToString("F2"),
                $"${log.TotalCost:F2}",
                log.Mpg.HasValue ? log.Mpg.Value.ToString("F1") : "[grey]N/A[/]",
                log.CostPerMile.HasValue ? $"{log.CostPerMile.Value * 100:F1}" : "[grey]N/A[/]",
                log.MilesSinceLastFillup.HasValue ? log.MilesSinceLastFillup.Value.ToString("N0") : "[grey]N/A[/]",
                log.FuelStation ?? "[grey]—[/]"
            );
        }

        AnsiConsole.Write(table);
        return 0;
    }
}
