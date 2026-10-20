using CS2_GEO.Data;
using CS2_GEO.Endpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddDbContext<GameDbContext>(options => options
    .UseNpgsql(builder.Configuration.GetConnectionString("Default"))
    .UseSnakeCaseNamingConvention());

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy => policy
        .WithOrigins("http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

//EF UPDATE
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GameDbContext>();

    await db.Database.MigrateAsync();

    var seedPath = Path.Combine(
        AppContext.BaseDirectory,
        "Data",
        "Seed",
        "seed.json");

    await DatabaseSeeder.SeedAsync(db, seedPath);
}

app.UseCors("Frontend");
app.UseStaticFiles();

app.MapMapsApi();
app.MapRoundsApi();
app.MapGamesApi();

app.Run();
