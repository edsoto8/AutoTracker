using AutoTracker.Core.Models;

namespace AutoTracker.Core.Interfaces;

public interface IFuelLogRepository
{
    Task<IEnumerable<FuelLog>> GetAllAsync();
    Task<IEnumerable<FuelLog>> GetByVehicleIdAsync(int vehicleId);
    Task<FuelLog?> GetByIdAsync(int id);
    Task<FuelLog?> GetLatestByVehicleIdAsync(int vehicleId);
    Task<int> AddAsync(FuelLog log);
    Task UpdateAsync(FuelLog log);
    Task DeleteAsync(int id);
}
