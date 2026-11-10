using CS2_GEO.Data;
using CS2_GEO.Data.Repositories;
using CS2_GEO.Endpoints;
using CS2_GEO.Game;
using CS2_GEO.Services;
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

// Scoring is stateless, so one instance serves every request. A difficulty level would register different options.
builder.Services.AddSingleton(new ScoringOptions());
builder.Services.AddSingleton<IScoreTierClassifier, ThresholdScoreTierClassifier>();
builder.Services.AddSingleton<IScoreCalculator, ExponentialScoreCalculator>();
builder.Services.AddSingleton<IGameScorer, GameScorer>();

// Repositories share the request's DbContext, so they are scoped like it.
builder.Services.AddScoped<IMapRepository, MapRepository>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();

builder.Services.AddScoped<IMapsService, MapsService>();
builder.Services.AddScoped<IGamesService, GamesService>();
builder.Services.AddScoped<IRoundsService, RoundsService>();

var app = builder.Build();

await app.InitializeDatabaseAsync();

app.UseCors("Frontend");
app.UseStaticFiles();

app.MapMapsApi();
app.MapRoundsApi();
app.MapGamesApi();

app.Run();
