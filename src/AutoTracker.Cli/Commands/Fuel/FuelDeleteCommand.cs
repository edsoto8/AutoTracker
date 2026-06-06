using AutoTracker.Core.Interfaces;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AutoTracker.Cli.Commands.Fuel;

public class FuelDeleteCommand : AsyncCommand
{
    private readonly IVehicleRepository _vehicleRepo;
    private readonly IFuelLogRepository _fuelRepo;

    public FuelDeleteCommand(IVehicleRepository vehicleRepo, IFuelLogRepository fuelRepo)
    {
        _vehicleRepo = vehicleRepo;
        _fuelRepo = fuelRepo;
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

        var logs = (await _fuelRepo.GetByVehicleIdAsync(vehicle.Id))
            .OrderByDescending(l => l.Date).ThenByDescending(l => l.Odometer).ToList();

        if (logs.Count == 0) { AnsiConsole.MarkupLine("[grey]No fuel logs for this vehicle.[/]"); return 0; }

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<Core.Models.FuelLog>()
                .Title("Select entry to delete:")
                .UseConverter(l => $"{l.Date:yyyy-MM-dd}  {l.Odometer:N0} mi  {l.Gallons:F2} gal  ${l.TotalCost:F2}")
                .AddChoices(logs));

        if (!AnsiConsole.Confirm($"Delete entry for [bold]{selected.Date:yyyy-MM-dd}[/]? This cannot be undone."))
        {
            AnsiConsole.MarkupLine("[grey]Cancelled.[/]");
            return 0;
        }

        await _fuelRepo.DeleteAsync(selected.Id);
        AnsiConsole.MarkupLine("[green]✓[/] Fuel log deleted.");
        return 0;
    }
}
