using AutoTracker.Core.Interfaces;
using AutoTracker.ImportExport;

using Spectre.Console;
using Spectre.Console.Cli;

namespace AutoTracker.Cli.Commands.ImportExport;

public class ImportJsonCommand : AsyncCommand
{
    private readonly IVehicleRepository _vehicleRepo;
    private readonly IFuelLogRepository _fuelRepo;
    private readonly IMaintenanceRepository _maintenanceRepo;
    private readonly IExpenseRepository _expenseRepo;

    public ImportJsonCommand(IVehicleRepository vehicleRepo, IFuelLogRepository fuelRepo,
        IMaintenanceRepository maintenanceRepo, IExpenseRepository expenseRepo)
    {
        _vehicleRepo = vehicleRepo;
        _fuelRepo = fuelRepo;
        _maintenanceRepo = maintenanceRepo;
        _expenseRepo = expenseRepo;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
    {
        var entityType = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Import type:")
                .AddChoices("Vehicles", "Fuel Logs", "Maintenance Logs", "Expenses"));

        var filePath = AnsiConsole.Prompt(
            new TextPrompt<string>("JSON file path:")
                .Validate(p => File.Exists(p)
                    ? ValidationResult.Success()
                    : ValidationResult.Error($"File not found: {p}")));

        var content = await File.ReadAllTextAsync(filePath, cancellationToken);
        var vehicles = (await _vehicleRepo.GetAllAsync()).ToList();
        var vehicleIds = vehicles.Select(v => v.Id);

        ImportResult result;

        switch (entityType)
        {
            case "Vehicles":
                var (importedVehicles, vr) = DataImporter.ImportVehiclesJson(content);
                result = vr;
                foreach (var v in importedVehicles)
                    await _vehicleRepo.AddAsync(v);
                break;

            case "Fuel Logs":
                var (fuelLogs, fr) = DataImporter.ImportFuelLogsJson(content, vehicleIds);
                result = fr;
                foreach (var l in fuelLogs)
                    await _fuelRepo.AddAsync(l);
                break;

            case "Maintenance Logs":
                var (maintLogs, mr) = DataImporter.ImportMaintenanceLogsJson(content, vehicleIds);
                result = mr;
                foreach (var l in maintLogs)
                    await _maintenanceRepo.AddAsync(l);
                break;

            case "Expenses":
                var (expenses, er) = DataImporter.ImportExpensesJson(content, vehicleIds);
                result = er;
                foreach (var ex in expenses)
                    await _expenseRepo.AddAsync(ex);
                break;

            default:
                return 1;
        }

        AnsiConsole.MarkupLine($"\n[bold]{entityType} JSON Import Summary[/]");
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
