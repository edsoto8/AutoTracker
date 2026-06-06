using AutoTracker.Core.Interfaces;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AutoTracker.Cli.Commands.Expenses;

public class ExpenseListCommand : AsyncCommand
{
    private readonly IVehicleRepository _vehicleRepo;
    private readonly IExpenseRepository _expenseRepo;

    public ExpenseListCommand(IVehicleRepository vehicleRepo, IExpenseRepository expenseRepo)
    {
        _vehicleRepo = vehicleRepo;
        _expenseRepo = expenseRepo;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
    {
        var vehicleMap = (await _vehicleRepo.GetAllAsync()).ToDictionary(v => v.Id, v => v.Name);
        var expenses = (await _expenseRepo.GetAllAsync()).ToList();

        if (expenses.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]No expenses found. Use [blue]expense add[/] to add one.[/]");
            return 0;
        }

        var table = new Table().BorderColor(Color.Grey);
        table.AddColumn("[bold]Date[/]");
        table.AddColumn("[bold]Vehicle[/]");
        table.AddColumn("[bold]Category[/]");
        table.AddColumn("[bold]Description[/]");
        table.AddColumn("[bold]Amount[/]");
        table.AddColumn("[bold]Notes[/]");

        foreach (var expense in expenses)
        {
            table.AddRow(
                expense.Date.ToString("yyyy-MM-dd"),
                vehicleMap.TryGetValue(expense.VehicleId, out var name) ? name : "?",
                expense.Category.ToString(),
                expense.Description,
                $"${expense.Amount:F2}",
                expense.Notes ?? "[grey]—[/]"
            );
        }

        AnsiConsole.Write(table);
        return 0;
    }
}
