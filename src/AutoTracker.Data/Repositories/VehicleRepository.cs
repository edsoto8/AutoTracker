using AutoTracker.Core.Interfaces;
using AutoTracker.Core.Models;

using Dapper;

namespace AutoTracker.Data.Repositories;

public class VehicleRepository : IVehicleRepository
{
    private readonly IDbConnectionFactory _factory;

    public VehicleRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<IEnumerable<Vehicle>> GetAllAsync()
    {
        using var connection = _factory.CreateConnection();
        return await connection.QueryAsync<Vehicle>("SELECT * FROM Vehicles ORDER BY Name");
    }

    public async Task<Vehicle?> GetByIdAsync(int id)
    {
        using var connection = _factory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Vehicle>(
            "SELECT * FROM Vehicles WHERE Id = @id", new { id });
    }

    public async Task<Vehicle?> GetByNameAsync(string name)
    {
        using var connection = _factory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Vehicle>(
            "SELECT * FROM Vehicles WHERE LOWER(Name) = LOWER(@name)", new { name });
    }

    public async Task<int> AddAsync(Vehicle vehicle)
    {
        using var connection = _factory.CreateConnection();
        await connection.ExecuteAsync(
            """
            INSERT INTO Vehicles (Id, Name, Year, Make, Model, VIN, LicensePlate, FuelType, TankCapacity)
            VALUES (NULLIF(@Id, 0), @Name, @Year, @Make, @Model, @VIN, @LicensePlate, @FuelType, @TankCapacity)
            """, vehicle);
        return await connection.ExecuteScalarAsync<int>("SELECT last_insert_rowid()");
    }

    public async Task UpdateAsync(Vehicle vehicle)
    {
        using var connection = _factory.CreateConnection();
        await connection.ExecuteAsync(
            """
            UPDATE Vehicles
            SET Name = @Name, Year = @Year, Make = @Make, Model = @Model,
                VIN = @VIN, LicensePlate = @LicensePlate, FuelType = @FuelType, TankCapacity = @TankCapacity
            WHERE Id = @Id
            """, vehicle);
    }

    public async Task<bool> HasDependentsAsync(int id)
    {
        using var connection = _factory.CreateConnection();
        var result = await connection.ExecuteScalarAsync<int>(
            """
            SELECT CASE WHEN EXISTS (SELECT 1 FROM FuelLogs       WHERE VehicleId = @id)
                          OR EXISTS (SELECT 1 FROM MaintenanceLogs WHERE VehicleId = @id)
                          OR EXISTS (SELECT 1 FROM Expenses        WHERE VehicleId = @id)
                   THEN 1 ELSE 0 END
            """, new { id });
        return result == 1;
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _factory.CreateConnection();
        await connection.ExecuteAsync("DELETE FROM Vehicles WHERE Id = @id", new { id });
    }
}
