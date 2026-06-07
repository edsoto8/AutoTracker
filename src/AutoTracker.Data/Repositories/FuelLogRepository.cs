using AutoTracker.Core.Interfaces;
using AutoTracker.Core.Models;

using Dapper;

namespace AutoTracker.Data.Repositories;

public class FuelLogRepository : IFuelLogRepository
{
    private readonly IDbConnectionFactory _factory;

    public FuelLogRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<IEnumerable<FuelLog>> GetAllAsync()
    {
        using var connection = _factory.CreateConnection();
        return await connection.QueryAsync<FuelLog>(
            "SELECT * FROM FuelLogs ORDER BY Date DESC, Odometer DESC");
    }

    public async Task<IEnumerable<FuelLog>> GetByVehicleIdAsync(int vehicleId)
    {
        using var connection = _factory.CreateConnection();
        return await connection.QueryAsync<FuelLog>(
            "SELECT * FROM FuelLogs WHERE VehicleId = @vehicleId ORDER BY Date ASC, Odometer ASC",
            new { vehicleId });
    }

    public async Task<FuelLog?> GetByIdAsync(int id)
    {
        using var connection = _factory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<FuelLog>(
            "SELECT * FROM FuelLogs WHERE Id = @id", new { id });
    }

    public async Task<FuelLog?> GetLatestByVehicleIdAsync(int vehicleId)
    {
        using var connection = _factory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<FuelLog>(
            "SELECT * FROM FuelLogs WHERE VehicleId = @vehicleId ORDER BY Date DESC, Odometer DESC LIMIT 1",
            new { vehicleId });
    }

    public async Task<int> AddAsync(FuelLog log)
    {
        using var connection = _factory.CreateConnection();
        await connection.ExecuteAsync(
            """
            INSERT INTO FuelLogs (VehicleId, Date, Odometer, Gallons, TotalCost, FuelStation, Notes)
            VALUES (@VehicleId, @Date, @Odometer, @Gallons, @TotalCost, @FuelStation, @Notes)
            """, log);
        return await connection.ExecuteScalarAsync<int>("SELECT last_insert_rowid()");
    }

    public async Task UpdateAsync(FuelLog log)
    {
        using var connection = _factory.CreateConnection();
        await connection.ExecuteAsync(
            """
            UPDATE FuelLogs
            SET VehicleId = @VehicleId, Date = @Date, Odometer = @Odometer,
                Gallons = @Gallons, TotalCost = @TotalCost, FuelStation = @FuelStation, Notes = @Notes
            WHERE Id = @Id
            """, log);
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _factory.CreateConnection();
        await connection.ExecuteAsync("DELETE FROM FuelLogs WHERE Id = @id", new { id });
    }
}
