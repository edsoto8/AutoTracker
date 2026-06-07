using AutoTracker.Data;
using AutoTracker.Data.Database;

using Microsoft.Data.Sqlite;

namespace AutoTracker.Data.Tests;

/// <summary>
/// Creates a fresh named in-memory SQLite database per instance.
/// The keeper connection keeps the database alive for the test's lifetime.
/// </summary>
public sealed class DatabaseFixture : IDisposable
{
    private readonly SqliteConnection _keeper;
    public IDbConnectionFactory ConnectionFactory { get; }

    public DatabaseFixture()
    {
        var dbName = $"testdb_{Guid.NewGuid():N}";
        var connectionString = $"Data Source={dbName};Mode=Memory;Cache=Shared";

        _keeper = new SqliteConnection(connectionString);
        _keeper.Open();

        DatabaseInitializer.Initialize(connectionString);

        ConnectionFactory = new SqliteConnectionFactory(connectionString);
    }

    public void Dispose() => _keeper.Dispose();
}
