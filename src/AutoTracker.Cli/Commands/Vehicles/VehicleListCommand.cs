using AutoTracker.Core.Interfaces;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AutoTracker.Cli.Commands.Vehicles;

public class VehicleListCommand : AsyncCommand
{
    private readonly IVehicleRepository _repo;

    public VehicleListCommand(IVehicleRepository repo)
    {
        _repo = repo;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
    {
        var vehicles = (await _repo.GetAllAsync()).ToList();

        if (vehicles.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]No vehicles found. Use [blue]vehicle add[/] to add one.[/]");
            return 0;
        }

        var table = new Table().BorderColor(Color.Grey);
        table.AddColumn("[bold]Name[/]");
        table.AddColumn("[bold]Year[/]");
        table.AddColumn("[bold]Make[/]");
        table.AddColumn("[bold]Model[/]");
        table.AddColumn("[bold]Fuel Type[/]");
        table.AddColumn("[bold]License Plate[/]");

        foreach (var v in vehicles)
            table.AddRow(v.Name, v.Year.ToString(), v.Make, v.Model,
                v.FuelType.ToString(), v.LicensePlate ?? "[grey]—[/]");

        AnsiConsole.Write(table);
        return 0;
    }
}
