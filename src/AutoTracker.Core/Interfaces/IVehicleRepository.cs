using AutoTracker.Core.Models;

namespace AutoTracker.Core.Interfaces;

public interface IVehicleRepository
{
    Task<IEnumerable<Vehicle>> GetAllAsync();
    Task<Vehicle?> GetByIdAsync(int id);
    Task<Vehicle?> GetByNameAsync(string name);
    Task<int> AddAsync(Vehicle vehicle);
    Task UpdateAsync(Vehicle vehicle);
    Task<bool> HasDependentsAsync(int id);
    Task DeleteAsync(int id);
}
