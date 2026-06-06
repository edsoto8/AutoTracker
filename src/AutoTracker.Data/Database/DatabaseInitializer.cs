using Dapper;
using Microsoft.Data.Sqlite;

namespace AutoTracker.Data.Database;

public static class DatabaseInitializer
{
    public static void Initialize(string connectionString)
    {
        DapperConfig.Configure();

        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        connection.Execute("PRAGMA foreign_keys = ON");

        foreach (var statement in SchemaStatements)
            connection.Execute(statement);
    }

    private static readonly string[] SchemaStatements =
    [
        """
        CREATE TABLE IF NOT EXISTS Vehicles (
            Id           INTEGER PRIMARY KEY AUTOINCREMENT,
            Name         TEXT    NOT NULL,
            Year         INTEGER NOT NULL,
            Make         TEXT    NOT NULL,
            Model        TEXT    NOT NULL,
            VIN          TEXT,
            LicensePlate TEXT,
            FuelType     TEXT    NOT NULL,
            TankCapacity REAL
        )
        """,
        """
        CREATE TABLE IF NOT EXISTS FuelLogs (
            Id          INTEGER PRIMARY KEY AUTOINCREMENT,
            VehicleId   INTEGER NOT NULL REFERENCES Vehicles(Id),
            Date        TEXT    NOT NULL,
            Odometer    INTEGER NOT NULL,
            Gallons     REAL    NOT NULL,
            TotalCost   REAL    NOT NULL,
            FuelStation TEXT,
            Notes       TEXT
        )
        """,
        "CREATE INDEX IF NOT EXISTS IX_FuelLogs_VehicleId ON FuelLogs(VehicleId)",
        "CREATE INDEX IF NOT EXISTS IX_FuelLogs_Date ON FuelLogs(Date)",
        """
        CREATE TABLE IF NOT EXISTS MaintenanceLogs (
            Id          INTEGER PRIMARY KEY AUTOINCREMENT,
            VehicleId   INTEGER NOT NULL REFERENCES Vehicles(Id),
            Date        TEXT    NOT NULL,
            Odometer    INTEGER NOT NULL,
            ServiceType TEXT    NOT NULL,
            Description TEXT,
            Cost        REAL    NOT NULL,
            Vendor      TEXT,
            Notes       TEXT
        )
        """,
        "CREATE INDEX IF NOT EXISTS IX_MaintenanceLogs_VehicleId ON MaintenanceLogs(VehicleId)",
        "CREATE INDEX IF NOT EXISTS IX_MaintenanceLogs_Date ON MaintenanceLogs(Date)",
        """
        CREATE TABLE IF NOT EXISTS Expenses (
            Id          INTEGER PRIMARY KEY AUTOINCREMENT,
            VehicleId   INTEGER NOT NULL REFERENCES Vehicles(Id),
            Date        TEXT    NOT NULL,
            Category    TEXT    NOT NULL,
            Description TEXT    NOT NULL,
            Amount      REAL    NOT NULL,
            Notes       TEXT
        )
        """,
        "CREATE INDEX IF NOT EXISTS IX_Expenses_VehicleId ON Expenses(VehicleId)",
        "CREATE INDEX IF NOT EXISTS IX_Expenses_Date ON Expenses(Date)",
    ];
}
