using AutoTracker.Core.Interfaces;

using Spectre.Console;
using Spectre.Console.Cli;

namespace AutoTracker.Cli.Commands.Fuel;

public class FuelEditCommand : AsyncCommand
{
    private readonly IVehicleRepository _vehicleRepo;
    private readonly IFuelLogRepository _fuelRepo;

    public FuelEditCommand(IVehicleRepository vehicleRepo, IFuelLogRepository fuelRepo)
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
                .Title("Select entry to edit:")
                .UseConverter(l => $"{l.Date:yyyy-MM-dd}  {l.Odometer:N0} mi  {l.Gallons:F2} gal  ${l.TotalCost:F2}")
                .AddChoices(logs));

        AnsiConsole.MarkupLine($"\n[bold]Editing fuel log[/] — {selected.Date:yyyy-MM-dd}\n[grey]Press Enter to keep current value.[/]\n");

        var dateStr = AnsiConsole.Prompt(
            new TextPrompt<string>($"Date [grey](current: {selected.Date:yyyy-MM-dd})[/]:")
                .DefaultValue(selected.Date.ToString("yyyy-MM-dd"))
                .Validate(s => DateTime.TryParse(s, out var d) && d <= DateTime.Today
                    ? ValidationResult.Success() : ValidationResult.Error("Enter a valid date not in the future.")));
        selected.Date = DateTime.Parse(dateStr);

        selected.Odometer = AnsiConsole.Prompt(
            new TextPrompt<int>($"Odometer [grey](current: {selected.Odometer:N0})[/]:")
                .DefaultValue(selected.Odometer)
                .Validate(o => o > 0 ? ValidationResult.Success() : ValidationResult.Error("Must be > 0.")));

        selected.Gallons = AnsiConsole.Prompt(
            new TextPrompt<decimal>($"Gallons [grey](current: {selected.Gallons:F2})[/]:")
                .DefaultValue(selected.Gallons)
                .Validate(g => g > 0 ? ValidationResult.Success() : ValidationResult.Error("Must be > 0.")));

        selected.TotalCost = AnsiConsole.Prompt(
            new TextPrompt<decimal>($"Total cost [grey](current: ${selected.TotalCost:F2})[/]:")
                .DefaultValue(selected.TotalCost)
                .Validate(c => c > 0 ? ValidationResult.Success() : ValidationResult.Error("Must be > 0.")));

        var station = AnsiConsole.Ask($"Fuel station [grey](current: {selected.FuelStation ?? "none"})[/]:", selected.FuelStation ?? string.Empty);
        selected.FuelStation = string.IsNullOrWhiteSpace(station) ? null : station;

        var notes = AnsiConsole.Ask($"Notes [grey](current: {selected.Notes ?? "none"})[/]:", selected.Notes ?? string.Empty);
        selected.Notes = string.IsNullOrWhiteSpace(notes) ? null : notes;

        await _fuelRepo.UpdateAsync(selected);
        AnsiConsole.MarkupLine($"\n[green]✓[/] Fuel log updated.");
        return 0;
    }
}
