using AutoTracker.Core.Interfaces;
using AutoTracker.ImportExport;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AutoTracker.Cli.Commands.ImportExport;

public class ImportLegacyCommand : AsyncCommand
{
    private readonly IVehicleRepository _vehicleRepo;
    private readonly IFuelLogRepository _fuelRepo;

    public ImportLegacyCommand(IVehicleRepository vehicleRepo, IFuelLogRepository fuelRepo)
    {
        _vehicleRepo = vehicleRepo;
        _fuelRepo = fuelRepo;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
    {
        var filePath = AnsiConsole.Prompt(
            new TextPrompt<string>("Legacy CSV file path:")
                .Validate(p => File.Exists(p)
                    ? ValidationResult.Success()
                    : ValidationResult.Error($"File not found: {p}")));

        var content = await File.ReadAllTextAsync(filePath, cancellationToken);
        var vehicles = (await _vehicleRepo.GetAllAsync()).ToList();
        var existingLogs = (await _fuelRepo.GetAllAsync()).ToList();

        var (toImport, result) = LegacyFuelImporter.Import(content, vehicles, existingLogs);

        foreach (var log in toImport)
            await _fuelRepo.AddAsync(log);

        AnsiConsole.MarkupLine("\n[bold]Legacy Import Summary[/]");
        AnsiConsole.MarkupLine($"  [green]Imported:[/]  {result.Imported}");
        AnsiConsole.MarkupLine($"  [yellow]Skipped:[/]   {result.SkippedCount}");

        if (result.Skipped.Count > 0)
        {
            AnsiConsole.MarkupLine("\n[yellow]Skipped rows:[/]");
            foreach (var msg in result.Skipped)
                AnsiConsole.MarkupLine($"  [grey]•[/] {msg}");
        }

        return 0;
    }
}
