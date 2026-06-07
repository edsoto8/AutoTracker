using AutoTracker.Core.Interfaces;
using AutoTracker.Core.Models;

using Dapper;

namespace AutoTracker.Data.Repositories;

public class ExpenseRepository : IExpenseRepository
{
    private readonly IDbConnectionFactory _factory;

    public ExpenseRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<IEnumerable<Expense>> GetAllAsync()
    {
        using var connection = _factory.CreateConnection();
        return await connection.QueryAsync<Expense>(
            "SELECT * FROM Expenses ORDER BY Date DESC");
    }

    public async Task<IEnumerable<Expense>> GetByVehicleIdAsync(int vehicleId)
    {
        using var connection = _factory.CreateConnection();
        return await connection.QueryAsync<Expense>(
            "SELECT * FROM Expenses WHERE VehicleId = @vehicleId ORDER BY Date DESC",
            new { vehicleId });
    }

    public async Task<Expense?> GetByIdAsync(int id)
    {
        using var connection = _factory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Expense>(
            "SELECT * FROM Expenses WHERE Id = @id", new { id });
    }

    public async Task<int> AddAsync(Expense expense)
    {
        using var connection = _factory.CreateConnection();
        await connection.ExecuteAsync(
            """
            INSERT INTO Expenses (VehicleId, Date, Category, Description, Amount, Notes)
            VALUES (@VehicleId, @Date, @Category, @Description, @Amount, @Notes)
            """, expense);
        return await connection.ExecuteScalarAsync<int>("SELECT last_insert_rowid()");
    }

    public async Task UpdateAsync(Expense expense)
    {
        using var connection = _factory.CreateConnection();
        await connection.ExecuteAsync(
            """
            UPDATE Expenses
            SET VehicleId = @VehicleId, Date = @Date, Category = @Category,
                Description = @Description, Amount = @Amount, Notes = @Notes
            WHERE Id = @Id
            """, expense);
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _factory.CreateConnection();
        await connection.ExecuteAsync("DELETE FROM Expenses WHERE Id = @id", new { id });
    }
}
