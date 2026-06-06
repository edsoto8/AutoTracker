using AutoTracker.Core.Enums;
using AutoTracker.Core.Interfaces;
using AutoTracker.Core.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AutoTracker.Cli.Commands.Vehicles;

public class VehicleAddCommand : AsyncCommand
{
    private readonly IVehicleRepository _repo;

    public VehicleAddCommand(IVehicleRepository repo)
    {
        _repo = repo;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
    {
        AnsiConsole.MarkupLine("[bold]Add Vehicle[/]\n");

        var name = AnsiConsole.Ask<string>("Vehicle [green]name[/]:");
        var year = AnsiConsole.Prompt(
            new TextPrompt<int>("Year:")
                .Validate(y => y is >= 1885 and <= 2100 ? ValidationResult.Success() : ValidationResult.Error("Enter a valid year.")));
        var make = AnsiConsole.Ask<string>("Make:");
        var model = AnsiConsole.Ask<string>("Model:");
        var vin = AnsiConsole.Ask<string>("VIN [grey](optional — press Enter to skip)[/]:", string.Empty);
        var plate = AnsiConsole.Ask<string>("License plate [grey](optional)[/]:", string.Empty);
        var fuelType = AnsiConsole.Prompt(
            new SelectionPrompt<FuelType>()
                .Title("Fuel type:")
                .AddChoices(Enum.GetValues<FuelType>()));
        var tankCapStr = AnsiConsole.Ask<string>("Tank capacity in gallons [grey](optional)[/]:", string.Empty);

        decimal? tankCap = null;
        if (!string.IsNullOrWhiteSpace(tankCapStr) && decimal.TryParse(tankCapStr, out var parsed) && parsed > 0)
            tankCap = parsed;

        var vehicle = new Vehicle
        {
            Name = name,
            Year = year,
            Make = make,
            Model = model,
            VIN = string.IsNullOrWhiteSpace(vin) ? null : vin,
            LicensePlate = string.IsNullOrWhiteSpace(plate) ? null : plate,
            FuelType = fuelType,
            TankCapacity = tankCap
        };

        await _repo.AddAsync(vehicle);
        AnsiConsole.MarkupLine($"\n[green]✓[/] Vehicle \"[bold]{name}[/]\" added.");
        return 0;
    }
}
