using System.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace LessonFlow.IntegrationTests.TestInfrastructure;

public sealed class TestDatabase : IAsyncDisposable
{
    public string DatabaseName { get; }
    public string ConnectionString { get; }
    private readonly string _adminConnectionString; // points to 'postgres' database

    private TestDatabase(string databaseName, string connectionString, string adminConnectionString)
    {
        DatabaseName = databaseName;
        ConnectionString = connectionString;
        _adminConnectionString = adminConnectionString;
    }

    public static async Task<TestDatabase> CreateAsync(
        string adminConnectionString, // e.g. Host=localhost;Port=5432;Username=...;Password=...;Database=postgres
        string connectionStringTemplate // e.g. Host=localhost;Port=5432;Username=...;Password=...;Database={0};Pooling=true
    )
    {
        var dbName = $"lf_it_{Guid.NewGuid():N}";
        await CreateDatabaseAsync(adminConnectionString, dbName);
        var cs = string.Format(connectionStringTemplate, dbName);
        return new TestDatabase(dbName, cs, adminConnectionString);
    }

    public async Task MigrateAsync(Func<DbContext> dbContextFactory, CancellationToken ct = default)
    {
        await using var ctx = dbContextFactory();
        await ctx.Database.MigrateAsync(ct);
    }

    public async Task SeedAsync(Func<DbContext> dbContextFactory, Func<DbContext, Task> seeder, CancellationToken ct = default)
    {
        await using var ctx = dbContextFactory();
        await seeder(ctx);
        await ctx.SaveChangesAsync(ct);
    }

    public async ValueTask DisposeAsync()
    {
        await DropDatabaseAsync(_adminConnectionString, DatabaseName);
    }

    private static async Task CreateDatabaseAsync(string adminConnectionString, string dbName)
    {
        await using var conn = new NpgsqlConnection(adminConnectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand($"""CREATE DATABASE "{dbName}" TEMPLATE template0;""", conn);
        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task DropDatabaseAsync(string adminConnectionString, string dbName)
    {
        try
        {
            NpgsqlConnection.ClearAllPools(); // release any pooled connections
            await using var conn = new NpgsqlConnection(adminConnectionString);
            await conn.OpenAsync();

            // terminate active connections to the db
            await using (var terminate = new NpgsqlCommand($"""
                SELECT pg_terminate_backend(pid)
                FROM pg_stat_activity
                WHERE datname = @db AND pid <> pg_backend_pid();
                """, conn))
            {
                terminate.Parameters.AddWithValue("db", dbName);
                await terminate.ExecuteNonQueryAsync();
            }

            await using (var drop = new NpgsqlCommand($"""DROP DATABASE IF EXISTS "{dbName}";""", conn))
            {
                await drop.ExecuteNonQueryAsync();
            }
        }
        catch
        {
            // swallow; tests must not fail on teardown
        }
    }
}