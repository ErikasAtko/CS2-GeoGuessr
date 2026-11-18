using CS2_GEO.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: AssemblyFixture(typeof(CS2_GEO.Tests.Integration.Cs2GeoApiFactory))]

namespace CS2_GEO.Tests.Integration;

/// <summary>
/// The real application (Program.cs) started in memory, against its own empty PostgreSQL database
/// (see <see cref="TestDatabase"/>). Startup applies the migrations and the seed file, exactly as it
/// does for a developer.
/// <para>
/// An assembly fixture: one database and one running app for the whole test run, because starting
/// them per test would be slow. Every test therefore creates its own players and lobbies and never
/// relies on what another test left behind; the seeded maps are the only shared data, and no test
/// changes them.
/// </para>
/// </summary>
public class Cs2GeoApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private TestDatabase? _database;

    public async ValueTask InitializeAsync()
    {
        _database = await TestDatabase.CreateAsync();

        // Started here, once, instead of lazily on the first CreateClient: test classes run in
        // parallel, and two of them starting the app at the same moment would migrate twice at once.
        StartServer();

        // Program.cs also reads appsettings.Local.json, the developer's own database. Make sure the
        // test connection string won before any test writes anything.
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GameDbContext>();
        if (db.Database.GetConnectionString() != _database.ConnectionString)
            throw new InvalidOperationException("The app under test is not using the test database; refusing to run tests against it.");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = _database!.ConnectionString,
            // Development logs every SQL command; in test output that buries the actual failure. The
            // command log is off entirely because the first migration on an empty database always logs
            // a failed query (the migrations history table doesn't exist yet); real failures still
            // reach the tests as exceptions.
            ["Logging:LogLevel:Default"] = "Warning",
            ["Logging:LogLevel:Microsoft.EntityFrameworkCore.Database.Command"] = "None",
        }));

    /// <summary>
    /// A hub connection that goes through the in-memory test server instead of a real port.
    /// Long polling, because it only needs plain HTTP requests, which the test server handles directly.
    /// </summary>
    public HubConnection CreateHubConnection() =>
        new HubConnectionBuilder()
            .WithUrl(new Uri(Server.BaseAddress, "hubs/multiplayer"), options =>
            {
                options.HttpMessageHandlerFactory = _ => Server.CreateHandler();
                options.Transports = HttpTransportType.LongPolling;
            })
            .Build();

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        if (_database is not null)
            await _database.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
