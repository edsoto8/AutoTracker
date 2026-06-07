using AutoTracker.Core.Interfaces;

using Spectre.Console;
using Spectre.Console.Cli;

namespace AutoTracker.Cli.Commands.Maintenance;

public class MaintenanceDeleteCommand : AsyncCommand
{
    private readonly IVehicleRepository _vehicleRepo;
    private readonly IMaintenanceRepository _maintenanceRepo;

    public MaintenanceDeleteCommand(IVehicleRepository vehicleRepo, IMaintenanceRepository maintenanceRepo)
    {
        _vehicleRepo = vehicleRepo;
        _maintenanceRepo = maintenanceRepo;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
    {
        var vehicles = (await _vehicleRepo.GetAllAsync()).ToList();
        if (vehicles.Count == 0) { AnsiConsole.MarkupLine("[grey]No vehicles found.[/]"); return 0; }

        var vehicle = AnsiConsole.Prompt(
            new SelectionPrompt<Core.Models.Vehicle>()
                .Title("Select vehicle:")
                .UseConverter(v => $"{v.Name} ({v.Year} {v.Make} {v.Model})")
                .AddChoices(vehicles));

        var logs = (await _maintenanceRepo.GetByVehicleIdAsync(vehicle.Id)).ToList();
        if (logs.Count == 0) { AnsiConsole.MarkupLine("[grey]No maintenance logs for this vehicle.[/]"); return 0; }

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<Core.Models.MaintenanceLog>()
                .Title("Select entry to delete:")
                .UseConverter(l => $"{l.Date:yyyy-MM-dd}  {l.ServiceType}  ${l.Cost:F2}  {l.Vendor ?? "—"}")
                .AddChoices(logs));

        if (!AnsiConsole.Confirm($"Delete [bold]{selected.ServiceType}[/] on [bold]{selected.Date:yyyy-MM-dd}[/]? This cannot be undone."))
        {
            AnsiConsole.MarkupLine("[grey]Cancelled.[/]");
            return 0;
        }

        await _maintenanceRepo.DeleteAsync(selected.Id);
        AnsiConsole.MarkupLine("[green]✓[/] Maintenance log deleted.");
        return 0;
    }
}
