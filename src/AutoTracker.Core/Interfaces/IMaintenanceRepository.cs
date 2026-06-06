using AutoTracker.Core.Models;

namespace AutoTracker.Core.Interfaces;

public interface IMaintenanceRepository
{
    Task<IEnumerable<MaintenanceLog>> GetAllAsync();
    Task<IEnumerable<MaintenanceLog>> GetByVehicleIdAsync(int vehicleId);
    Task<MaintenanceLog?> GetByIdAsync(int id);
    Task<int> AddAsync(MaintenanceLog log);
    Task UpdateAsync(MaintenanceLog log);
    Task DeleteAsync(int id);
}
