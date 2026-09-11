var builder = WebApplication.CreateBuilder(args);

// Registers OpenAPI so the endpoints can be tested during development.
builder.Services.AddOpenApi();

var app = builder.Build();

// Exposes the OpenAPI document only when running in development.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Stores the locations in memory instead of a database for now. Each location
// holds the image shown to the player and the answer position on the map in pixels.
var locations = new List<Location>
{
    new Location(1, "https://placehold.co/800x450?text=Location+1", 320, 210),
    new Location(2, "https://placehold.co/800x450?text=Location+2", 540, 380),
    new Location(3, "https://placehold.co/800x450?text=Location+3", 150, 470),
};

// Returns a random location without its answer coordinates, so the player cannot see the answer.
app.MapGet("/rounds/random", () =>
{
    var location = locations[Random.Shared.Next(locations.Count)];
    return Results.Ok(new { location.Id, location.ImageUrl });
});

// Receives the player's guess and calculates the score for that round.
app.MapPost("/rounds/guess", (GuessRequest guess) =>
{
    var location = locations.FirstOrDefault(l => l.Id == guess.LocationId);
    if (location is null)
        return Results.NotFound($"No location with id {guess.LocationId}");

    // Calculates the distance in pixels between the guess and the real position.
    var dx = guess.GuessX - location.AnswerX;
    var dy = guess.GuessY - location.AnswerY;
    var distance = Math.Sqrt(dx * dx + dy * dy);

    // Gives up to 5000 points, decreasing as the distance grows.
    var points = (int)Math.Round(5000 * Math.Exp(-distance / 200));

    return Results.Ok(new
    {
        Points = points,
        Distance = distance,
        AnswerX = location.AnswerX,
        AnswerY = location.AnswerY
    });
});

app.Run();

// Represents a single location in the game.
record Location(int Id, string ImageUrl, int AnswerX, int AnswerY);

// Represents the data the browser sends when submitting a guess.
record GuessRequest(int LocationId, int GuessX, int GuessY);
