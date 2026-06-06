using AutoTracker.Core.Enums;
using AutoTracker.Core.Models;
using AutoTracker.Data.Repositories;

namespace AutoTracker.Data.Tests.Repositories;

public class VehicleRepositoryTests : IDisposable
{
    private readonly DatabaseFixture _fixture;
    private readonly VehicleRepository _repo;

    public VehicleRepositoryTests()
    {
        _fixture = new DatabaseFixture();
        _repo = new VehicleRepository(_fixture.ConnectionFactory);
    }

    public void Dispose() => _fixture.Dispose();

    [Fact]
    public async Task Add_ThenGetById_ReturnsVehicle()
    {
        var vehicle = MakeVehicle("Toyota Camry");
        var id = await _repo.AddAsync(vehicle);

        var result = await _repo.GetByIdAsync(id);

        Assert.NotNull(result);
        Assert.Equal("Toyota Camry", result.Name);
        Assert.Equal(2022, result.Year);
        Assert.Equal(FuelType.Gasoline, result.FuelType);
    }

    [Fact]
    public async Task GetAll_ReturnsOrderedByName()
    {
        await _repo.AddAsync(MakeVehicle("Toyota Camry"));
        await _repo.AddAsync(MakeVehicle("Honda Civic"));
        await _repo.AddAsync(MakeVehicle("Ford F-150"));

        var results = (await _repo.GetAllAsync()).ToList();

        Assert.Equal("Ford F-150", results[0].Name);
        Assert.Equal("Honda Civic", results[1].Name);
        Assert.Equal("Toyota Camry", results[2].Name);
    }

    [Fact]
    public async Task GetByName_IsCaseInsensitive()
    {
        await _repo.AddAsync(MakeVehicle("Toyota Camry"));

        var result = await _repo.GetByNameAsync("toyota camry");

        Assert.NotNull(result);
        Assert.Equal("Toyota Camry", result.Name);
    }

    [Fact]
    public async Task Update_PersistsChanges()
    {
        var id = await _repo.AddAsync(MakeVehicle("Toyota Camry"));
        var vehicle = await _repo.GetByIdAsync(id);
        vehicle!.Name = "Toyota Camry SE";

        await _repo.UpdateAsync(vehicle);

        var updated = await _repo.GetByIdAsync(id);
        Assert.Equal("Toyota Camry SE", updated!.Name);
    }

    [Fact]
    public async Task Delete_RemovesVehicle()
    {
        var id = await _repo.AddAsync(MakeVehicle("Toyota Camry"));

        await _repo.DeleteAsync(id);

        Assert.Null(await _repo.GetByIdAsync(id));
    }

    [Fact]
    public async Task HasDependents_ReturnsFalse_WhenNoChildRecords()
    {
        var id = await _repo.AddAsync(MakeVehicle("Toyota Camry"));

        Assert.False(await _repo.HasDependentsAsync(id));
    }

    [Fact]
    public async Task HasDependents_ReturnsTrue_WhenFuelLogExists()
    {
        var vehicleId = await _repo.AddAsync(MakeVehicle("Toyota Camry"));
        var fuelRepo = new FuelLogRepository(_fixture.ConnectionFactory);
        await fuelRepo.AddAsync(new FuelLog
        {
            VehicleId = vehicleId,
            Date = new DateTime(2024, 1, 1),
            Odometer = 10000,
            Gallons = 10,
            TotalCost = 40
        });

        Assert.True(await _repo.HasDependentsAsync(vehicleId));
    }

    private static Vehicle MakeVehicle(string name) => new()
    {
        Name = name,
        Year = 2022,
        Make = "Toyota",
        Model = "Camry",
        FuelType = FuelType.Gasoline
    };
}
