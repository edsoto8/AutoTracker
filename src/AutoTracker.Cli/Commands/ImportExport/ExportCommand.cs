using AutoTracker.Core.Interfaces;
using AutoTracker.ImportExport;

using Spectre.Console;
using Spectre.Console.Cli;

namespace AutoTracker.Cli.Commands.ImportExport;

public class ExportCommand : AsyncCommand
{
    private readonly IVehicleRepository _vehicleRepo;
    private readonly IFuelLogRepository _fuelRepo;
    private readonly IMaintenanceRepository _maintenanceRepo;
    private readonly IExpenseRepository _expenseRepo;

    public ExportCommand(IVehicleRepository vehicleRepo, IFuelLogRepository fuelRepo,
        IMaintenanceRepository maintenanceRepo, IExpenseRepository expenseRepo)
    {
        _vehicleRepo = vehicleRepo;
        _fuelRepo = fuelRepo;
        _maintenanceRepo = maintenanceRepo;
        _expenseRepo = expenseRepo;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
    {
        var format = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Export format:")
                .AddChoices("CSV", "JSON"));

        var outputDir = AnsiConsole.Prompt(
            new TextPrompt<string>("Output directory:")
                .DefaultValue(Directory.GetCurrentDirectory())
                .Validate(d => Directory.Exists(d) || TryCreate(d)
                    ? ValidationResult.Success()
                    : ValidationResult.Error("Directory does not exist and could not be created.")));

        Directory.CreateDirectory(outputDir);

        var vehicles = (await _vehicleRepo.GetAllAsync()).ToList();
        var fuelLogs = (await _fuelRepo.GetAllAsync()).ToList();
        var maintenanceLogs = (await _maintenanceRepo.GetAllAsync()).ToList();
        var expenses = (await _expenseRepo.GetAllAsync()).ToList();

        var ext = format == "CSV" ? "csv" : "json";
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        if (format == "CSV")
        {
            Write(outputDir, $"vehicles_{timestamp}.csv", DataExporter.ExportVehiclesCsv(vehicles));
            Write(outputDir, $"fuel_logs_{timestamp}.csv", DataExporter.ExportFuelLogsCsv(fuelLogs));
            Write(outputDir, $"maintenance_logs_{timestamp}.csv", DataExporter.ExportMaintenanceLogsCsv(maintenanceLogs));
            Write(outputDir, $"expenses_{timestamp}.csv", DataExporter.ExportExpensesCsv(expenses));
        }
        else
        {
            Write(outputDir, $"vehicles_{timestamp}.json", DataExporter.ExportVehiclesJson(vehicles));
            Write(outputDir, $"fuel_logs_{timestamp}.json", DataExporter.ExportFuelLogsJson(fuelLogs));
            Write(outputDir, $"maintenance_logs_{timestamp}.json", DataExporter.ExportMaintenanceLogsJson(maintenanceLogs));
            Write(outputDir, $"expenses_{timestamp}.json", DataExporter.ExportExpensesJson(expenses));
        }

        AnsiConsole.MarkupLine($"\n[green]✓[/] Exported 4 files ({ext.ToUpper()}) to [bold]{outputDir}[/]");
        AnsiConsole.MarkupLine($"  Vehicles: {vehicles.Count}, Fuel logs: {fuelLogs.Count}, Maintenance: {maintenanceLogs.Count}, Expenses: {expenses.Count}");
        return 0;
    }

    private static void Write(string dir, string file, string content) =>
        File.WriteAllText(Path.Combine(dir, file), content);

    private static bool TryCreate(string path)
    {
        try { Directory.CreateDirectory(path); return true; }
        catch { return false; }
    }
}
