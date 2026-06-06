using AutoTracker.Cli.Commands.Vehicles;
using AutoTracker.Cli.Infrastructure;
using AutoTracker.Core.Interfaces;
using AutoTracker.Data;
using AutoTracker.Data.Database;
using AutoTracker.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

var connectionString = DatabasePath.GetConnectionString();
DatabaseInitializer.Initialize(connectionString);

var services = new ServiceCollection();
services.AddSingleton<IDbConnectionFactory>(new SqliteConnectionFactory(connectionString));
services.AddTransient<IVehicleRepository, VehicleRepository>();
services.AddTransient<IFuelLogRepository, FuelLogRepository>();
services.AddTransient<IMaintenanceRepository, MaintenanceRepository>();
services.AddTransient<IExpenseRepository, ExpenseRepository>();
services.AddTransient<VehicleListCommand>();
services.AddTransient<VehicleAddCommand>();
services.AddTransient<VehicleEditCommand>();
services.AddTransient<VehicleDeleteCommand>();

var registrar = new TypeRegistrar(services);
var app = new CommandApp(registrar);

app.Configure(config =>
{
    config.SetApplicationName("AutoTracker");

    config.AddBranch("vehicle", vehicle =>
    {
        vehicle.SetDescription("Manage vehicles");
        vehicle.AddCommand<VehicleListCommand>("list").WithDescription("List all vehicles");
        vehicle.AddCommand<VehicleAddCommand>("add").WithDescription("Add a new vehicle");
        vehicle.AddCommand<VehicleEditCommand>("edit").WithDescription("Edit an existing vehicle");
        vehicle.AddCommand<VehicleDeleteCommand>("delete").WithDescription("Delete a vehicle");
    });
});

return await app.RunAsync(args);
