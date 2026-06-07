using AutoTracker.Core.Interfaces;

using Spectre.Console;
using Spectre.Console.Cli;

namespace AutoTracker.Cli.Commands.Maintenance;

public class MaintenanceListCommand : AsyncCommand
{
    private readonly IVehicleRepository _vehicleRepo;
    private readonly IMaintenanceRepository _maintenanceRepo;

    public MaintenanceListCommand(IVehicleRepository vehicleRepo, IMaintenanceRepository maintenanceRepo)
    {
        _vehicleRepo = vehicleRepo;
        _maintenanceRepo = maintenanceRepo;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
    {
        var vehicleMap = (await _vehicleRepo.GetAllAsync()).ToDictionary(v => v.Id, v => v.Name);
        var logs = (await _maintenanceRepo.GetAllAsync())
            .OrderByDescending(l => l.Date).ToList();

        if (logs.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]No maintenance logs found. Use [blue]maintenance add[/] to add one.[/]");
            return 0;
        }

        var table = new Table().BorderColor(Color.Grey);
        table.AddColumn("[bold]Date[/]");
        table.AddColumn("[bold]Vehicle[/]");
        table.AddColumn("[bold]Odometer[/]");
        table.AddColumn("[bold]Service Type[/]");
        table.AddColumn("[bold]Cost[/]");
        table.AddColumn("[bold]Vendor[/]");
        table.AddColumn("[bold]Description[/]");

        foreach (var log in logs)
        {
            table.AddRow(
                log.Date.ToString("yyyy-MM-dd"),
                vehicleMap.TryGetValue(log.VehicleId, out var name) ? name : "?",
                log.Odometer.ToString("N0"),
                log.ServiceType.ToString(),
                $"${log.Cost:F2}",
                log.Vendor ?? "[grey]—[/]",
                log.Description ?? "[grey]—[/]"
            );
        }

        AnsiConsole.Write(table);
        return 0;
    }
}
