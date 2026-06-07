using AutoTracker.Core.Interfaces;

using Spectre.Console;
using Spectre.Console.Cli;

namespace AutoTracker.Cli.Commands.Expenses;

public class ExpenseDeleteCommand : AsyncCommand
{
    private readonly IVehicleRepository _vehicleRepo;
    private readonly IExpenseRepository _expenseRepo;

    public ExpenseDeleteCommand(IVehicleRepository vehicleRepo, IExpenseRepository expenseRepo)
    {
        _vehicleRepo = vehicleRepo;
        _expenseRepo = expenseRepo;
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

        var expenses = (await _expenseRepo.GetByVehicleIdAsync(vehicle.Id)).ToList();
        if (expenses.Count == 0) { AnsiConsole.MarkupLine("[grey]No expenses for this vehicle.[/]"); return 0; }

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<Core.Models.Expense>()
                .Title("Select expense to delete:")
                .UseConverter(e => $"{e.Date:yyyy-MM-dd}  {e.Category}  ${e.Amount:F2}  {e.Description}")
                .AddChoices(expenses));

        if (!AnsiConsole.Confirm($"Delete [bold]{selected.Description}[/] on [bold]{selected.Date:yyyy-MM-dd}[/]? This cannot be undone."))
        {
            AnsiConsole.MarkupLine("[grey]Cancelled.[/]");
            return 0;
        }

        await _expenseRepo.DeleteAsync(selected.Id);
        AnsiConsole.MarkupLine("[green]✓[/] Expense deleted.");
        return 0;
    }
}
