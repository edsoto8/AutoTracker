using AutoTracker.Core.Enums;
using AutoTracker.Core.Interfaces;
using AutoTracker.Core.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AutoTracker.Cli.Commands.Maintenance;

public class MaintenanceAddCommand : AsyncCommand
{
    private readonly IVehicleRepository _vehicleRepo;
    private readonly IMaintenanceRepository _maintenanceRepo;

    public MaintenanceAddCommand(IVehicleRepository vehicleRepo, IMaintenanceRepository maintenanceRepo)
    {
        _vehicleRepo = vehicleRepo;
        _maintenanceRepo = maintenanceRepo;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
    {
        var vehicles = (await _vehicleRepo.GetAllAsync()).ToList();
        if (vehicles.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No vehicles found.[/] Add a vehicle first with [blue]vehicle add[/].");
            return 1;
        }

        AnsiConsole.MarkupLine("[bold]Add Maintenance Log[/]\n");

        var vehicle = AnsiConsole.Prompt(
            new SelectionPrompt<Vehicle>()
                .Title("Vehicle:")
                .UseConverter(v => $"{v.Name} ({v.Year} {v.Make} {v.Model})")
                .AddChoices(vehicles));

        var date = AnsiConsole.Prompt(
            new TextPrompt<string>("Date [grey](YYYY-MM-DD, Enter for today)[/]:")
                .DefaultValue(DateTime.Today.ToString("yyyy-MM-dd"))
                .Validate(s => DateTime.TryParse(s, out var d) && d <= DateTime.Today
                    ? ValidationResult.Success()
                    : ValidationResult.Error("Enter a valid date not in the future.")));

        var odometer = AnsiConsole.Prompt(
            new TextPrompt<int>("Odometer (miles):")
                .Validate(o => o > 0 ? ValidationResult.Success() : ValidationResult.Error("Must be > 0.")));

        var serviceType = AnsiConsole.Prompt(
            new SelectionPrompt<ServiceType>()
                .Title("Service type:")
                .AddChoices(Enum.GetValues<ServiceType>()));

        var description = AnsiConsole.Ask<string>("Description [grey](optional)[/]:", string.Empty);

        var cost = AnsiConsole.Prompt(
            new TextPrompt<decimal>("Cost ($):")
                .Validate(c => c >= 0 ? ValidationResult.Success() : ValidationResult.Error("Must be ≥ 0.")));

        var vendor = AnsiConsole.Ask<string>("Vendor [grey](optional)[/]:", string.Empty);
        var notes = AnsiConsole.Ask<string>("Notes [grey](optional)[/]:", string.Empty);

        var log = new MaintenanceLog
        {
            VehicleId = vehicle.Id,
            Date = DateTime.Parse(date),
            Odometer = odometer,
            ServiceType = serviceType,
            Description = string.IsNullOrWhiteSpace(description) ? null : description,
            Cost = cost,
            Vendor = string.IsNullOrWhiteSpace(vendor) ? null : vendor,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes
        };

        await _maintenanceRepo.AddAsync(log);
        AnsiConsole.MarkupLine($"\n[green]✓[/] Maintenance log added for [bold]{vehicle.Name}[/] on {log.Date:yyyy-MM-dd}.");
        return 0;
    }
}
