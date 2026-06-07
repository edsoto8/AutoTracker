using AutoTracker.Core.Enums;
using AutoTracker.Core.Interfaces;
using AutoTracker.Core.Models;

using Spectre.Console;
using Spectre.Console.Cli;

namespace AutoTracker.Cli.Commands.Vehicles;

public class VehicleEditCommand : AsyncCommand
{
    private readonly IVehicleRepository _repo;

    public VehicleEditCommand(IVehicleRepository repo)
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
            new SelectionPrompt<Vehicle>()
                .Title("Select vehicle to edit:")
                .UseConverter(v => $"{v.Name} ({v.Year} {v.Make} {v.Model})")
                .AddChoices(vehicles));

        AnsiConsole.MarkupLine($"\n[bold]Editing:[/] {selected.Name}\n[grey]Press Enter to keep current value.[/]\n");

        selected.Name = Prompt("Name", selected.Name);
        selected.Year = PromptInt("Year", selected.Year, 1885, 2100);
        selected.Make = Prompt("Make", selected.Make);
        selected.Model = Prompt("Model", selected.Model);
        selected.VIN = NullIfEmpty(Prompt("VIN", selected.VIN ?? string.Empty));
        selected.LicensePlate = NullIfEmpty(Prompt("License plate", selected.LicensePlate ?? string.Empty));
        selected.FuelType = AnsiConsole.Prompt(
            new SelectionPrompt<FuelType>()
                .Title($"Fuel type [grey](current: {selected.FuelType})[/]:")
                .AddChoices(Enum.GetValues<FuelType>()));
        var tankStr = Prompt("Tank capacity in gallons", selected.TankCapacity?.ToString() ?? string.Empty);
        selected.TankCapacity = decimal.TryParse(tankStr, out var cap) && cap > 0 ? cap : null;

        await _repo.UpdateAsync(selected);
        AnsiConsole.MarkupLine($"\n[green]✓[/] Vehicle \"[bold]{selected.Name}[/]\" updated.");
        return 0;
    }

    private static string Prompt(string label, string current) =>
        AnsiConsole.Ask($"{label} [grey](current: {current})[/]:", current);

    private static int PromptInt(string label, int current, int min, int max) =>
        AnsiConsole.Prompt(
            new TextPrompt<int>($"{label} [grey](current: {current})[/]:")
                .DefaultValue(current)
                .Validate(v => v >= min && v <= max ? ValidationResult.Success() : ValidationResult.Error($"Must be {min}–{max}.")));

    private static string? NullIfEmpty(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}
