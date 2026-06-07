using AutoTracker.Core.Interfaces;

using Spectre.Console;
using Spectre.Console.Cli;

namespace AutoTracker.Cli.Commands.Vehicles;

public class VehicleDeleteCommand : AsyncCommand
{
    private readonly IVehicleRepository _repo;

    public VehicleDeleteCommand(IVehicleRepository repo)
    {
        _repo = repo;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
    {
        var vehicles = (await _repo.GetAllAsync()).ToList();

        if (vehicles.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]No vehicles found.[/]");
            return 0;
        }

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<Core.Models.Vehicle>()
                .Title("Select vehicle to delete:")
                .UseConverter(v => $"{v.Name} ({v.Year} {v.Make} {v.Model})")
                .AddChoices(vehicles));

        if (await _repo.HasDependentsAsync(selected.Id))
        {
            AnsiConsole.MarkupLine("[red]Cannot delete[/] — this vehicle has fuel logs, maintenance records, or expenses. Remove those first.");
            return 1;
        }

        var confirmed = AnsiConsole.Confirm($"Delete \"[bold]{selected.Name}[/]\"? This cannot be undone.");
        if (!confirmed)
        {
            AnsiConsole.MarkupLine("[grey]Cancelled.[/]");
            return 0;
        }

        await _repo.DeleteAsync(selected.Id);
        AnsiConsole.MarkupLine($"[green]✓[/] Vehicle \"[bold]{selected.Name}[/]\" deleted.");
        return 0;
    }
}
