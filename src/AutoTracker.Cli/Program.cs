using AutoTracker.Cli.Commands.Fuel;
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
services.AddTransient<FuelListCommand>();
services.AddTransient<FuelAddCommand>();
services.AddTransient<FuelEditCommand>();
services.AddTransient<FuelDeleteCommand>();

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

    config.AddBranch("fuel", fuel =>
    {
        fuel.SetDescription("Manage fuel logs");
        fuel.AddCommand<FuelListCommand>("list").WithDescription("List all fuel logs");
        fuel.AddCommand<FuelAddCommand>("add").WithDescription("Add a fuel log entry");
        fuel.AddCommand<FuelEditCommand>("edit").WithDescription("Edit a fuel log entry");
        fuel.AddCommand<FuelDeleteCommand>("delete").WithDescription("Delete a fuel log entry");
    });
});

return await app.RunAsync(args);
