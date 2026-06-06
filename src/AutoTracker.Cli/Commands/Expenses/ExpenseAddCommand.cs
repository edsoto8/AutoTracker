using AutoTracker.Core.Enums;
using AutoTracker.Core.Interfaces;
using AutoTracker.Core.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AutoTracker.Cli.Commands.Expenses;

public class ExpenseAddCommand : AsyncCommand
{
    private readonly IVehicleRepository _vehicleRepo;
    private readonly IExpenseRepository _expenseRepo;

    public ExpenseAddCommand(IVehicleRepository vehicleRepo, IExpenseRepository expenseRepo)
    {
        _vehicleRepo = vehicleRepo;
        _expenseRepo = expenseRepo;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
    {
        var vehicles = (await _vehicleRepo.GetAllAsync()).ToList();
        if (vehicles.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No vehicles found.[/] Add a vehicle first with [blue]vehicle add[/].");
            return 1;
        }

        AnsiConsole.MarkupLine("[bold]Add Expense[/]\n");

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

        var category = AnsiConsole.Prompt(
            new SelectionPrompt<ExpenseCategory>()
                .Title("Category:")
                .AddChoices(Enum.GetValues<ExpenseCategory>()));

        var description = AnsiConsole.Prompt(
            new TextPrompt<string>("Description:")
                .Validate(s => !string.IsNullOrWhiteSpace(s)
                    ? ValidationResult.Success()
                    : ValidationResult.Error("Description is required.")));

        var amount = AnsiConsole.Prompt(
            new TextPrompt<decimal>("Amount ($):")
                .Validate(a => a > 0 ? ValidationResult.Success() : ValidationResult.Error("Must be > 0.")));

        var notes = AnsiConsole.Ask<string>("Notes [grey](optional)[/]:", string.Empty);

        var expense = new Expense
        {
            VehicleId = vehicle.Id,
            Date = DateTime.Parse(date),
            Category = category,
            Description = description,
            Amount = amount,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes
        };

        await _expenseRepo.AddAsync(expense);
        AnsiConsole.MarkupLine($"\n[green]✓[/] Expense added for [bold]{vehicle.Name}[/] on {expense.Date:yyyy-MM-dd}.");
        return 0;
    }
}
