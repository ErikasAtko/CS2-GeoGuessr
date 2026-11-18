using System.Globalization;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Testcontainers.PostgreSql;

namespace CS2_GEO.Tests.Integration;

/// <summary>
/// The PostgreSQL database the integration tests run against: always a new, empty one created for
/// this test run and removed after it, never a developer's own database (the tests would see their
/// data, and they would see the tests'). It comes from one of two places:
/// <list type="bullet">
/// <item>
/// The team's database server, when <c>appsettings.Local.json</c> has a connection string (every
/// developer machine): a fresh <c>cs2guessr_test_*</c> database is created there with the same
/// credentials and dropped afterwards. No Docker needed.
/// </item>
/// <item>
/// A PostgreSQL container started with Testcontainers otherwise (CI, or when the
/// <c>CS2GEO_TEST_DATABASE</c> environment variable is <c>docker</c>). Needs Docker.
/// </item>
/// </list>
/// </summary>
public abstract class TestDatabase : IAsyncDisposable
{
    public abstract string ConnectionString { get; }

    public abstract ValueTask DisposeAsync();

    public static async Task<TestDatabase> CreateAsync()
    {
        var serverConnectionString = Environment.GetEnvironmentVariable("CS2GEO_TEST_DATABASE") == "docker"
            ? null
            : ReadDeveloperConnectionString();

        return serverConnectionString is null
            ? await ContainerDatabase.StartAsync()
            : await ServerDatabase.CreateAsync(serverConnectionString);
    }

    // The same file Program.cs reads; the API project copies it into this project's output folder.
    private static string? ReadDeveloperConnectionString() =>
        new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.Local.json"), optional: true)
            .Build()
            .GetConnectionString("Default");

    private sealed class ContainerDatabase(PostgreSqlContainer container) : TestDatabase
    {
        public override string ConnectionString => container.GetConnectionString();

        public static async Task<ContainerDatabase> StartAsync()
        {
            // The same major version as the team's server, so both modes test the same database.
            var container = new PostgreSqlBuilder("postgres:16-alpine").Build();
            await container.StartAsync();
            return new ContainerDatabase(container);
        }

        public override ValueTask DisposeAsync() => container.DisposeAsync();
    }

    private sealed class ServerDatabase(string adminConnectionString, string name, string connectionString) : TestDatabase
    {
        private const string NamePrefix = "cs2guessr_test_";
        private const string TimestampFormat = "yyyyMMddHHmmss";

        // A test run that was killed (debugger stopped, terminal closed) never drops its database.
        // Anything this old is certainly not still running, so the next run cleans it up.
        private static readonly TimeSpan _abandonedAfter = TimeSpan.FromHours(6);

        public override string ConnectionString => connectionString;

        public static async Task<ServerDatabase> CreateAsync(string developerConnectionString)
        {
            // Connects to the server's maintenance database: a database can't be created or
            // dropped from a connection to itself.
            var admin = new NpgsqlConnectionStringBuilder(developerConnectionString) { Database = "postgres", Pooling = false };
            var name = $"{NamePrefix}{DateTime.UtcNow.ToString(TimestampFormat, CultureInfo.InvariantCulture)}_{Guid.NewGuid():N}";

            await using var connection = new NpgsqlConnection(admin.ConnectionString);
            await connection.OpenAsync();
            await DropAbandonedAsync(connection);
            await ExecuteAsync(connection, $"""CREATE DATABASE "{name}" """);

            var test = new NpgsqlConnectionStringBuilder(developerConnectionString) { Database = name };
            return new ServerDatabase(admin.ConnectionString, name, test.ConnectionString);
        }

        public override async ValueTask DisposeAsync()
        {
            // The app's pooled connections would otherwise keep the database in use.
            NpgsqlConnection.ClearAllPools();

            await using var connection = new NpgsqlConnection(adminConnectionString);
            await connection.OpenAsync();
            await DropAsync(connection, name);
        }

        private static async Task DropAbandonedAsync(NpgsqlConnection connection)
        {
            var names = new List<string>();
            await using (var command = new NpgsqlCommand("SELECT datname FROM pg_database WHERE starts_with(datname, $1)", connection))
            {
                command.Parameters.AddWithValue(NamePrefix);
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                    names.Add(reader.GetString(0));
            }

            foreach (var abandoned in names.Where(IsAbandoned))
                await DropAsync(connection, abandoned);
        }

        private static bool IsAbandoned(string databaseName)
        {
            var timestamp = databaseName.Substring(NamePrefix.Length, TimestampFormat.Length);
            return DateTime.TryParseExact(timestamp, TimestampFormat, CultureInfo.InvariantCulture,
                       DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var createdAt)
                   && DateTime.UtcNow - createdAt > _abandonedAfter;
        }

        // Only ever called with names this class generated (prefix + timestamp + GUID), so the
        // identifier is safe to put into the SQL text; DDL can't take it as a parameter.
        private static Task DropAsync(NpgsqlConnection connection, string databaseName) =>
            ExecuteAsync(connection, $"""DROP DATABASE IF EXISTS "{databaseName}" WITH (FORCE)""");

        private static async Task ExecuteAsync(NpgsqlConnection connection, string sql)
        {
            await using var command = new NpgsqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync();
        }
    }
}
