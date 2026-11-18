using System.Net;
using System.Net.Http.Json;
using CS2_GEO.Dtos;

namespace CS2_GEO.Tests.Integration;

// The singleplayer endpoints through real HTTP, the real Program.cs and the seeded PostgreSQL
// database: routing, query binding, JSON, the DI registrations and the SQL behind random picks.
// The seeded maps are read, never changed, so these tests can share them.
public class SingleplayerApiTests(Cs2GeoApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    private async Task<MapDto> AnyMapAsync() =>
        (await _client.GetFromJsonAsync<List<MapDto>>("/api/maps", Cancellation))!.First();

    // Guessing a location reveals where it actually is; the tests use that to make a perfect guess.
    private async Task<GuessResultDto> GuessAsync(int locationId, double x, double y)
    {
        var response = await _client.PostAsJsonAsync($"/api/rounds/{locationId}/guess", new { x, y }, Cancellation);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<GuessResultDto>(Cancellation))!;
    }

    [Fact]
    public async Task GetMaps_ReturnsTheSeededMapsWithTheirLocationCounts()
    {
        var maps = await _client.GetFromJsonAsync<List<MapDto>>("/api/maps", Cancellation);

        Assert.NotEmpty(maps!);
        Assert.All(maps!, m => Assert.True(m.LocationCount > 0, $"{m.Code} has no locations"));
    }

    [Fact]
    public async Task GetMap_UnknownCode_Returns404()
    {
        var response = await _client.GetAsync("/api/maps/de_nowhere", Cancellation);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // Asks for more rounds than the map has: every location comes back once, none twice, none from
    // another map. Checks the map filter and ORDER BY random() + LIMIT in real SQL.
    [Fact]
    public async Task NewGame_MoreRoundsThanTheMapHasLocations_ReturnsEachOfThemOnce()
    {
        var map = await AnyMapAsync();

        var game = await _client.GetFromJsonAsync<GameDto>($"/api/games/new?map={map.Code}&rounds={map.LocationCount + 5}", Cancellation);

        Assert.Equal(map.LocationCount, game!.Rounds.Count);
        Assert.Equal(map.LocationCount, game.Rounds.Select(r => r.LocationId).Distinct().Count());
        Assert.All(game.Rounds, r => Assert.Equal(map.Code, r.MapCode));
    }

    [Fact]
    public async Task NewGame_ZeroRounds_Returns400()
    {
        var response = await _client.GetAsync("/api/games/new?rounds=0", Cancellation);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task NewGame_UnknownMap_Returns404()
    {
        var response = await _client.GetAsync("/api/games/new?map=de_nowhere", Cancellation);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Guess_ExactlyAtTheActualSpot_ScoresTheMaximum()
    {
        var round = await _client.GetFromJsonAsync<RoundDto>("/api/rounds/random", Cancellation);
        var firstTry = await GuessAsync(round!.LocationId, 0, 0);

        var perfect = await GuessAsync(round.LocationId, firstTry.ActualX, firstTry.ActualY);

        Assert.Equal(5000, perfect.Score);
        Assert.Equal("Perfect", perfect.Tier);
        Assert.Equal(0, perfect.DistanceUnits);
    }

    [Fact]
    public async Task Guess_UnknownLocation_Returns404()
    {
        var response = await _client.PostAsJsonAsync("/api/rounds/999999/guess", new { x = 0.5, y = 0.5 }, Cancellation);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // The whole singleplayer flow: start a game, guess every round, get the summary.
    [Fact]
    public async Task Summary_EveryGuessAtTheActualSpot_TotalIsTheMaximumPossible()
    {
        var game = await _client.GetFromJsonAsync<GameDto>("/api/games/new?rounds=3", Cancellation);
        var guesses = new List<object>();
        foreach (var round in game!.Rounds)
        {
            var actual = await GuessAsync(round.LocationId, 0, 0);
            guesses.Add(new { locationId = round.LocationId, x = actual.ActualX, y = actual.ActualY });
        }

        var response = await _client.PostAsJsonAsync("/api/games/summary", new { guesses }, Cancellation);
        var summary = await response.Content.ReadFromJsonAsync<GameSummaryDto>(Cancellation);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3, summary!.Rounds.Count);
        Assert.Equal(15000, summary.MaxPossibleScore);
        Assert.Equal(summary.MaxPossibleScore, summary.TotalScore);
    }

    [Fact]
    public async Task Summary_SameLocationTwice_Returns400()
    {
        var round = await _client.GetFromJsonAsync<RoundDto>("/api/rounds/random", Cancellation);
        var guess = new { locationId = round!.LocationId, x = 0.5, y = 0.5 };

        var response = await _client.PostAsJsonAsync("/api/games/summary", new { guesses = new[] { guess, guess } }, Cancellation);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Summary_UnknownLocation_Returns404NamingIt()
    {
        var response = await _client.PostAsJsonAsync("/api/games/summary",
            new { guesses = new[] { new { locationId = 999999, x = 0.5, y = 0.5 } } }, Cancellation);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("999999", await response.Content.ReadAsStringAsync(Cancellation));
    }
}
