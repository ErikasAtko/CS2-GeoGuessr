using CS2_GEO.Data.Entities;
using CS2_GEO.Data.Repositories;
using CS2_GEO.Dtos;
using CS2_GEO.Game;
using CS2_GEO.Services;
using CS2_GEO.Tests.Unit.Fakes;
using NSubstitute;

namespace CS2_GEO.Tests.Unit;

// GamesService with a fake repository instead of the database. The scorer and the calculator
// are the real ones: they're fast and deterministic, so replacing them would only tie the
// tests to how the service calls them.
public class GamesServiceTests
{
    private static readonly Map _dust2 = new() { Id = 1, Code = "de_dust2", DisplayName = "Dust II", MinimapUrl = "/maps/dust2.png", SizeUnits = 4500 };
    private static readonly Map _mirage = new() { Id = 2, Code = "de_mirage", DisplayName = "Mirage", MinimapUrl = "/maps/mirage.png", SizeUnits = 5000 };

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    private readonly InMemoryLocationRepository _locations = new();
    private readonly ExponentialScoreCalculator _calculator;
    private readonly GamesService _service;

    public GamesServiceTests()
    {
        var options = new ScoringOptions();
        _calculator = new ExponentialScoreCalculator(options, new ThresholdScoreTierClassifier(options));
        _service = new GamesService(_locations, new GameScorer(_calculator), _calculator);
    }

    // The request's token has to reach the repository: if the service dropped it, a closed tab
    // would no longer stop the database query it started.
    [Fact]
    public async Task CreateNewGame_PassesTheRequestsCancellationTokenToTheRepository()
    {
        var locations = Substitute.For<ILocationRepository>();
        locations.GetRandomAsync(Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<Location>()));
        var service = new GamesService(locations, new GameScorer(_calculator), _calculator);
        using var request = new CancellationTokenSource();

        await service.CreateNewGameAsync("de_dust2", 5, request.Token);

        await locations.Received(1).GetRandomAsync("de_dust2", 5, request.Token);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public async Task CreateNewGame_LessThanOneRound_ReturnsTooFewRounds(int rounds)
    {
        _locations.Add(_dust2, 0.5, 0.5);

        var result = await _service.CreateNewGameAsync("de_dust2", rounds, Cancellation);

        Assert.Equal(NewGameError.TooFewRounds, result.Error);
    }

    [Fact]
    public async Task CreateNewGame_MapWithoutLocations_ReturnsNoLocationsFound()
    {
        _locations.Add(_dust2, 0.5, 0.5);

        var result = await _service.CreateNewGameAsync("de_mirage", 5, Cancellation);

        Assert.Equal(NewGameError.NoLocationsFound, result.Error);
    }

    [Fact]
    public async Task CreateNewGame_ForOneMap_OnlyUsesThatMapsLocations()
    {
        _locations.Add(_dust2, 0.1, 0.1);
        _locations.Add(_mirage, 0.2, 0.2);
        _locations.Add(_dust2, 0.3, 0.3);

        var result = await _service.CreateNewGameAsync("de_dust2", 5, Cancellation);

        Assert.Null(result.Error);
        Assert.All(result.Game!.Rounds, r => Assert.Equal("de_dust2", r.MapCode));
        Assert.Equal(2, result.Game.Rounds.Count);
    }

    [Fact]
    public async Task BuildSummary_NoGuesses_ReturnsNoGuesses()
    {
        var result = await _service.BuildSummaryAsync(new GameSummaryRequestDto([]), Cancellation);

        Assert.Equal(SummaryError.NoGuesses, result.Error);
    }

    // Guessing the same location twice would let a player count one good round twice.
    [Fact]
    public async Task BuildSummary_SameLocationTwice_ReturnsDuplicateGuess()
    {
        var location = _locations.Add(_dust2, 0.5, 0.5);

        var result = await _service.BuildSummaryAsync(new GameSummaryRequestDto(
            [new RoundGuessDto(location.Id, 0.5, 0.5), new RoundGuessDto(location.Id, 0.4, 0.4)]), Cancellation);

        Assert.Equal(SummaryError.DuplicateGuess, result.Error);
    }

    [Fact]
    public async Task BuildSummary_UnknownLocation_ReportsWhichOne()
    {
        var location = _locations.Add(_dust2, 0.5, 0.5);

        var result = await _service.BuildSummaryAsync(new GameSummaryRequestDto(
            [new RoundGuessDto(location.Id, 0.5, 0.5), new RoundGuessDto(99, 0.5, 0.5)]), Cancellation);

        Assert.Equal(SummaryError.LocationNotFound, result.Error);
        Assert.Equal(99, result.MissingLocationId);
    }

    [Fact]
    public async Task BuildSummary_OnePerfectAndOneMissedGuess_AddsUpTheScores()
    {
        var hit = _locations.Add(_dust2, 0.5, 0.5);
        var missed = _locations.Add(_dust2, 0.1, 0.1);

        var result = await _service.BuildSummaryAsync(new GameSummaryRequestDto(
            [new RoundGuessDto(missed.Id, 0.9, 0.9), new RoundGuessDto(hit.Id, 0.5, 0.5)]), Cancellation);

        var summary = result.Summary!;
        Assert.Equal(5000, summary.TotalScore);
        Assert.Equal(10000, summary.MaxPossibleScore);
        Assert.Equal(2500, summary.AverageScore);
        // Best round first, still numbered in the order it was played.
        Assert.Equal([2, 1], summary.Rounds.Select(r => r.RoundNumber));
        Assert.Equal(["Perfect", "Miss"], summary.Rounds.Select(r => r.Tier));
    }

    // Distances are computed on the 0-1 minimap, but players see game units, which differ per map.
    [Fact]
    public async Task BuildSummary_Distance_IsInTheMapsOwnUnits()
    {
        var onDust2 = _locations.Add(_dust2, 0.5, 0.5);
        var onMirage = _locations.Add(_mirage, 0.5, 0.5);

        var result = await _service.BuildSummaryAsync(new GameSummaryRequestDto(
            [new RoundGuessDto(onDust2.Id, 0.6, 0.5), new RoundGuessDto(onMirage.Id, 0.6, 0.5)]), Cancellation);

        var distances = result.Summary!.Rounds.Select(r => r.DistanceUnits).Order().ToList();
        Assert.Equal(450, distances[0], precision: 6); // 0.1 of Dust II's 4500 units
        Assert.Equal(500, distances[1], precision: 6); // 0.1 of Mirage's 5000 units
    }
}
