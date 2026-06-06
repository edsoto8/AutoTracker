using AutoTracker.Core.Enums;
using AutoTracker.Core.Interfaces;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AutoTracker.Cli.Commands.Maintenance;

public class MaintenanceEditCommand : AsyncCommand
{
    private readonly IVehicleRepository _vehicleRepo;
    private readonly IMaintenanceRepository _maintenanceRepo;

    public MaintenanceEditCommand(IVehicleRepository vehicleRepo, IMaintenanceRepository maintenanceRepo)
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
                .Title("Select entry to edit:")
                .UseConverter(l => $"{l.Date:yyyy-MM-dd}  {l.ServiceType}  ${l.Cost:F2}  {l.Vendor ?? "—"}")
                .AddChoices(logs));

        AnsiConsole.MarkupLine($"\n[bold]Editing maintenance log[/] — {selected.Date:yyyy-MM-dd}\n[grey]Press Enter to keep current value.[/]\n");

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

        selected.ServiceType = AnsiConsole.Prompt(
            new SelectionPrompt<ServiceType>()
                .Title($"Service type [grey](current: {selected.ServiceType})[/]:")
                .AddChoices(Enum.GetValues<ServiceType>()));

        var desc = AnsiConsole.Ask($"Description [grey](current: {selected.Description ?? "none"})[/]:", selected.Description ?? string.Empty);
        selected.Description = string.IsNullOrWhiteSpace(desc) ? null : desc;

        selected.Cost = AnsiConsole.Prompt(
            new TextPrompt<decimal>($"Cost [grey](current: ${selected.Cost:F2})[/]:")
                .DefaultValue(selected.Cost)
                .Validate(c => c >= 0 ? ValidationResult.Success() : ValidationResult.Error("Must be ≥ 0.")));

        var vendor = AnsiConsole.Ask($"Vendor [grey](current: {selected.Vendor ?? "none"})[/]:", selected.Vendor ?? string.Empty);
        selected.Vendor = string.IsNullOrWhiteSpace(vendor) ? null : vendor;

        var notes = AnsiConsole.Ask($"Notes [grey](current: {selected.Notes ?? "none"})[/]:", selected.Notes ?? string.Empty);
        selected.Notes = string.IsNullOrWhiteSpace(notes) ? null : notes;

        await _maintenanceRepo.UpdateAsync(selected);
        AnsiConsole.MarkupLine("\n[green]✓[/] Maintenance log updated.");
        return 0;
    }
}
