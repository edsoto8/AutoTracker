using AutoTracker.Core.Enums;
using AutoTracker.Core.Models;
using AutoTracker.Data.Repositories;

namespace AutoTracker.Data.Tests.Repositories;

public class ExpenseRepositoryTests : IDisposable
{
    private readonly DatabaseFixture _fixture;
    private readonly ExpenseRepository _repo;
    private readonly int _vehicleId;

    public ExpenseRepositoryTests()
    {
        _fixture = new DatabaseFixture();
        _repo = new ExpenseRepository(_fixture.ConnectionFactory);

        var vehicleRepo = new VehicleRepository(_fixture.ConnectionFactory);
        _vehicleId = vehicleRepo.AddAsync(new Vehicle
        {
            Name = "Test Vehicle",
            Year = 2022,
            Make = "Toyota",
            Model = "Camry",
            FuelType = FuelType.Gasoline
        }).GetAwaiter().GetResult();
    }

    public void Dispose() => _fixture.Dispose();

    [Fact]
    public async Task Add_ThenGetById_ReturnsExpense()
    {
        var expense = MakeExpense(new DateTime(2024, 1, 1), ExpenseCategory.Insurance, 120m);
        var id = await _repo.AddAsync(expense);

        var result = await _repo.GetByIdAsync(id);

        Assert.NotNull(result);
        Assert.Equal(ExpenseCategory.Insurance, result.Category);
        Assert.Equal(120m, result.Amount);
    }

    [Fact]
    public async Task GetByVehicleId_ReturnsExpensesDescending()
    {
        await _repo.AddAsync(MakeExpense(new DateTime(2024, 1, 1), ExpenseCategory.Insurance, 120m));
        await _repo.AddAsync(MakeExpense(new DateTime(2024, 6, 1), ExpenseCategory.Registration, 80m));

        var results = (await _repo.GetByVehicleIdAsync(_vehicleId)).ToList();

        Assert.Equal(new DateTime(2024, 6, 1), results[0].Date);
        Assert.Equal(new DateTime(2024, 1, 1), results[1].Date);
    }

    [Fact]
    public async Task Update_PersistsChanges()
    {
        var id = await _repo.AddAsync(MakeExpense(new DateTime(2024, 1, 1), ExpenseCategory.Insurance, 120m));
        var expense = await _repo.GetByIdAsync(id);
        expense!.Amount = 150m;

        await _repo.UpdateAsync(expense);

        Assert.Equal(150m, (await _repo.GetByIdAsync(id))!.Amount);
    }

    [Fact]
    public async Task Delete_RemovesExpense()
    {
        var id = await _repo.AddAsync(MakeExpense(new DateTime(2024, 1, 1), ExpenseCategory.Insurance, 120m));

        await _repo.DeleteAsync(id);

        Assert.Null(await _repo.GetByIdAsync(id));
    }

    private Expense MakeExpense(DateTime date, ExpenseCategory category, decimal amount) => new()
    {
        VehicleId = _vehicleId,
        Date = date,
        Category = category,
        Description = "Test expense",
        Amount = amount
    };
}
