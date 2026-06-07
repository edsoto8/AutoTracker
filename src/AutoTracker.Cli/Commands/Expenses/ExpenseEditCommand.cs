using AutoTracker.Core.Enums;
using AutoTracker.Core.Interfaces;

using Spectre.Console;
using Spectre.Console.Cli;

namespace AutoTracker.Cli.Commands.Expenses;

public class ExpenseEditCommand : AsyncCommand
{
    private readonly IVehicleRepository _vehicleRepo;
    private readonly IExpenseRepository _expenseRepo;

    public ExpenseEditCommand(IVehicleRepository vehicleRepo, IExpenseRepository expenseRepo)
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
                .Title("Select expense to edit:")
                .UseConverter(e => $"{e.Date:yyyy-MM-dd}  {e.Category}  ${e.Amount:F2}  {e.Description}")
                .AddChoices(expenses));

        AnsiConsole.MarkupLine($"\n[bold]Editing expense[/] — {selected.Date:yyyy-MM-dd}\n[grey]Press Enter to keep current value.[/]\n");

        var dateStr = AnsiConsole.Prompt(
            new TextPrompt<string>($"Date [grey](current: {selected.Date:yyyy-MM-dd})[/]:")
                .DefaultValue(selected.Date.ToString("yyyy-MM-dd"))
                .Validate(s => DateTime.TryParse(s, out var d) && d <= DateTime.Today
                    ? ValidationResult.Success() : ValidationResult.Error("Enter a valid date not in the future.")));
        selected.Date = DateTime.Parse(dateStr);

        selected.Category = AnsiConsole.Prompt(
            new SelectionPrompt<ExpenseCategory>()
                .Title($"Category [grey](current: {selected.Category} — press Enter to keep)[/]:")
                .AddChoices(Enum.GetValues<ExpenseCategory>()
                    .OrderBy(c => c != selected.Category)
                    .ThenBy(c => c)));

        var desc = AnsiConsole.Prompt(
            new TextPrompt<string>($"Description [grey](current: {selected.Description})[/]:")
                .DefaultValue(selected.Description)
                .Validate(s => !string.IsNullOrWhiteSpace(s)
                    ? ValidationResult.Success()
                    : ValidationResult.Error("Description is required.")));
        selected.Description = desc;

        selected.Amount = AnsiConsole.Prompt(
            new TextPrompt<decimal>($"Amount [grey](current: ${selected.Amount:F2})[/]:")
                .DefaultValue(selected.Amount)
                .Validate(a => a > 0 ? ValidationResult.Success() : ValidationResult.Error("Must be > 0.")));

        var notes = AnsiConsole.Ask($"Notes [grey](current: {selected.Notes ?? "none"})[/]:", selected.Notes ?? string.Empty);
        selected.Notes = string.IsNullOrWhiteSpace(notes) ? null : notes;

        await _expenseRepo.UpdateAsync(selected);
        AnsiConsole.MarkupLine("\n[green]✓[/] Expense updated.");
        return 0;
    }
}
