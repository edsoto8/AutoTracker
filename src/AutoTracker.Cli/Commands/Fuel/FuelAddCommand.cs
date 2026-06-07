using AutoTracker.Core.Interfaces;
using AutoTracker.Core.Models;

using Spectre.Console;
using Spectre.Console.Cli;

namespace AutoTracker.Cli.Commands.Fuel;

public class FuelAddCommand : AsyncCommand
{
    private readonly IVehicleRepository _vehicleRepo;
    private readonly IFuelLogRepository _fuelRepo;

    public FuelAddCommand(IVehicleRepository vehicleRepo, IFuelLogRepository fuelRepo)
    {
        _vehicleRepo = vehicleRepo;
        _fuelRepo = fuelRepo;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
    {
        var vehicles = (await _vehicleRepo.GetAllAsync()).ToList();

        if (vehicles.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No vehicles found.[/] Add a vehicle first with [blue]vehicle add[/].");
            return 1;
        }

        AnsiConsole.MarkupLine("[bold]Add Fuel Log[/]\n");

        var vehicle = AnsiConsole.Prompt(
            new SelectionPrompt<Core.Models.Vehicle>()
                .Title("Vehicle:")
                .UseConverter(v => $"{v.Name} ({v.Year} {v.Make} {v.Model})")
                .AddChoices(vehicles));

        var date = AnsiConsole.Prompt(
            new TextPrompt<string>("Date [grey](YYYY-MM-DD, Enter for today)[/]:")
                .DefaultValue(DateTime.Today.ToString("yyyy-MM-dd"))
                .Validate(s => DateTime.TryParse(s, out var d) && d <= DateTime.Today
                    ? ValidationResult.Success()
                    : ValidationResult.Error("Enter a valid date not in the future.")));
        var parsedDate = DateTime.Parse(date);

        var odometer = AnsiConsole.Prompt(
            new TextPrompt<int>("Odometer (miles):")
                .Validate(o => o > 0 ? ValidationResult.Success() : ValidationResult.Error("Must be > 0.")));

        // Odometer soft warning
        var latest = await _fuelRepo.GetLatestByVehicleIdAsync(vehicle.Id);
        if (latest != null && odometer <= latest.Odometer)
            AnsiConsole.MarkupLine($"[yellow]⚠ Warning:[/] Previous entry has odometer [bold]{latest.Odometer:N0}[/]. Entry will still be saved.");

        var gallons = AnsiConsole.Prompt(
            new TextPrompt<decimal>("Gallons:")
                .Validate(g => g > 0 ? ValidationResult.Success() : ValidationResult.Error("Must be > 0.")));

        var totalCost = AnsiConsole.Prompt(
            new TextPrompt<decimal>("Total cost ($):")
                .Validate(c => c > 0 ? ValidationResult.Success() : ValidationResult.Error("Must be > 0.")));

        var station = AnsiConsole.Ask<string>("Fuel station [grey](optional)[/]:", string.Empty);
        var notes = AnsiConsole.Ask<string>("Notes [grey](optional)[/]:", string.Empty);

        var log = new FuelLog
        {
            VehicleId = vehicle.Id,
            Date = parsedDate,
            Odometer = odometer,
            Gallons = gallons,
            TotalCost = totalCost,
            FuelStation = string.IsNullOrWhiteSpace(station) ? null : station,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes
        };

        await _fuelRepo.AddAsync(log);
        AnsiConsole.MarkupLine($"\n[green]✓[/] Fuel log added for [bold]{vehicle.Name}[/] on {parsedDate:yyyy-MM-dd}.");
        return 0;
    }
}
