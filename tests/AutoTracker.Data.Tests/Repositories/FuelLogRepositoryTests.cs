using AutoTracker.Core.Enums;
using AutoTracker.Core.Models;
using AutoTracker.Data.Repositories;

namespace AutoTracker.Data.Tests.Repositories;

public class FuelLogRepositoryTests : IDisposable
{
    private readonly DatabaseFixture _fixture;
    private readonly FuelLogRepository _repo;
    private readonly int _vehicleId;

    public FuelLogRepositoryTests()
    {
        _fixture = new DatabaseFixture();
        _repo = new FuelLogRepository(_fixture.ConnectionFactory);

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
        var log = MakeLog(new DateTime(2024, 1, 1), 10000);
        var id = await _repo.AddAsync(log);

        var result = await _repo.GetByIdAsync(id);

        Assert.NotNull(result);
        Assert.Equal(10000, result.Odometer);
        Assert.Equal(10m, result.Gallons);
        Assert.Equal(new DateTime(2024, 1, 1), result.Date);
    }

    [Fact]
    public async Task GetByVehicleId_ReturnsLogsAscending()
    {
        await _repo.AddAsync(MakeLog(new DateTime(2024, 3, 1), 10600));
        await _repo.AddAsync(MakeLog(new DateTime(2024, 1, 1), 10000));
        await _repo.AddAsync(MakeLog(new DateTime(2024, 2, 1), 10300));

        var results = (await _repo.GetByVehicleIdAsync(_vehicleId)).ToList();

        Assert.Equal(10000, results[0].Odometer);
        Assert.Equal(10300, results[1].Odometer);
        Assert.Equal(10600, results[2].Odometer);
    }

    [Fact]
    public async Task GetAll_ReturnsLogsDescending()
    {
        await _repo.AddAsync(MakeLog(new DateTime(2024, 1, 1), 10000));
        await _repo.AddAsync(MakeLog(new DateTime(2024, 3, 1), 10600));

        var results = (await _repo.GetAllAsync()).ToList();

        Assert.Equal(10600, results[0].Odometer);
        Assert.Equal(10000, results[1].Odometer);
    }

    [Fact]
    public async Task GetLatestByVehicleId_ReturnsMostRecent()
    {
        await _repo.AddAsync(MakeLog(new DateTime(2024, 1, 1), 10000));
        await _repo.AddAsync(MakeLog(new DateTime(2024, 3, 1), 10600));
        await _repo.AddAsync(MakeLog(new DateTime(2024, 2, 1), 10300));

        var result = await _repo.GetLatestByVehicleIdAsync(_vehicleId);

        Assert.Equal(10600, result!.Odometer);
    }

    [Fact]
    public async Task Update_PersistsChanges()
    {
        var id = await _repo.AddAsync(MakeLog(new DateTime(2024, 1, 1), 10000));
        var log = await _repo.GetByIdAsync(id);
        log!.FuelStation = "Shell";

        await _repo.UpdateAsync(log);

        Assert.Equal("Shell", (await _repo.GetByIdAsync(id))!.FuelStation);
    }

    [Fact]
    public async Task Delete_RemovesLog()
    {
        var id = await _repo.AddAsync(MakeLog(new DateTime(2024, 1, 1), 10000));

        await _repo.DeleteAsync(id);

        Assert.Null(await _repo.GetByIdAsync(id));
    }

    private FuelLog MakeLog(DateTime date, int odometer) => new()
    {
        VehicleId = _vehicleId,
        Date = date,
        Odometer = odometer,
        Gallons = 10m,
        TotalCost = 40m
    };
}
