using AutoTracker.Core.Interfaces;
using AutoTracker.Core.Models;
using Dapper;

namespace AutoTracker.Data.Repositories;

public class MaintenanceRepository : IMaintenanceRepository
{
    private readonly IDbConnectionFactory _factory;

    public MaintenanceRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<IEnumerable<MaintenanceLog>> GetAllAsync()
    {
        using var connection = _factory.CreateConnection();
        return await connection.QueryAsync<MaintenanceLog>(
            "SELECT * FROM MaintenanceLogs ORDER BY Date DESC");
    }

    public async Task<IEnumerable<MaintenanceLog>> GetByVehicleIdAsync(int vehicleId)
    {
        using var connection = _factory.CreateConnection();
        return await connection.QueryAsync<MaintenanceLog>(
            "SELECT * FROM MaintenanceLogs WHERE VehicleId = @vehicleId ORDER BY Date DESC",
            new { vehicleId });
    }

    public async Task<MaintenanceLog?> GetByIdAsync(int id)
    {
        using var connection = _factory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<MaintenanceLog>(
            "SELECT * FROM MaintenanceLogs WHERE Id = @id", new { id });
    }

    public async Task<int> AddAsync(MaintenanceLog log)
    {
        using var connection = _factory.CreateConnection();
        await connection.ExecuteAsync(
            """
            INSERT INTO MaintenanceLogs (VehicleId, Date, Odometer, ServiceType, Description, Cost, Vendor, Notes)
            VALUES (@VehicleId, @Date, @Odometer, @ServiceType, @Description, @Cost, @Vendor, @Notes)
            """, log);
        return await connection.ExecuteScalarAsync<int>("SELECT last_insert_rowid()");
    }

    public async Task UpdateAsync(MaintenanceLog log)
    {
        using var connection = _factory.CreateConnection();
        await connection.ExecuteAsync(
            """
            UPDATE MaintenanceLogs
            SET VehicleId = @VehicleId, Date = @Date, Odometer = @Odometer, ServiceType = @ServiceType,
                Description = @Description, Cost = @Cost, Vendor = @Vendor, Notes = @Notes
            WHERE Id = @Id
            """, log);
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _factory.CreateConnection();
        await connection.ExecuteAsync("DELETE FROM MaintenanceLogs WHERE Id = @id", new { id });
    }
}
