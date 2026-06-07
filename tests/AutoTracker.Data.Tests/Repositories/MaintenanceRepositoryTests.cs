using AutoTracker.Core.Enums;
using AutoTracker.Core.Models;
using AutoTracker.Data.Repositories;

namespace AutoTracker.Data.Tests.Repositories;

public class MaintenanceRepositoryTests : IDisposable
{
    private readonly DatabaseFixture _fixture;
    private readonly MaintenanceRepository _repo;
    private readonly int _vehicleId;

    public MaintenanceRepositoryTests()
    {
        _fixture = new DatabaseFixture();
        _repo = new MaintenanceRepository(_fixture.ConnectionFactory);

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
    public async Task Add_ThenGetById_ReturnsLog()
    {
        var log = MakeLog(new DateTime(2024, 1, 1), ServiceType.OilChange, 50m);
        var id = await _repo.AddAsync(log);

        var result = await _repo.GetByIdAsync(id);

        Assert.NotNull(result);
        Assert.Equal(ServiceType.OilChange, result.ServiceType);
        Assert.Equal(50m, result.Cost);
    }

    [Fact]
    public async Task GetByVehicleId_ReturnsLogsDescending()
    {
        await _repo.AddAsync(MakeLog(new DateTime(2024, 1, 1), ServiceType.OilChange, 50m));
        await _repo.AddAsync(MakeLog(new DateTime(2024, 6, 1), ServiceType.TireRotation, 30m));

        var results = (await _repo.GetByVehicleIdAsync(_vehicleId)).ToList();

        Assert.Equal(new DateTime(2024, 6, 1), results[0].Date);
        Assert.Equal(new DateTime(2024, 1, 1), results[1].Date);
    }

    [Fact]
    public async Task FreeCost_IsAllowed()
    {
        var id = await _repo.AddAsync(MakeLog(new DateTime(2024, 1, 1), ServiceType.Inspection, 0m));

        var result = await _repo.GetByIdAsync(id);

        Assert.Equal(0m, result!.Cost);
    }

    [Fact]
    public async Task Update_PersistsChanges()
    {
        var id = await _repo.AddAsync(MakeLog(new DateTime(2024, 1, 1), ServiceType.OilChange, 50m));
        var log = await _repo.GetByIdAsync(id);
        log!.Vendor = "Jiffy Lube";

        await _repo.UpdateAsync(log);

        Assert.Equal("Jiffy Lube", (await _repo.GetByIdAsync(id))!.Vendor);
    }

    [Fact]
    public async Task Delete_RemovesLog()
    {
        var id = await _repo.AddAsync(MakeLog(new DateTime(2024, 1, 1), ServiceType.OilChange, 50m));

        await _repo.DeleteAsync(id);

        Assert.Null(await _repo.GetByIdAsync(id));
    }

    private MaintenanceLog MakeLog(DateTime date, ServiceType serviceType, decimal cost) => new()
    {
        VehicleId = _vehicleId,
        Date = date,
        Odometer = 10000,
        ServiceType = serviceType,
        Cost = cost
    };
}
