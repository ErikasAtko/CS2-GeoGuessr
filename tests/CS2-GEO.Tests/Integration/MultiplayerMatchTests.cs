using System.Net;
using System.Net.Http.Json;
using CS2_GEO.Dtos;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace CS2_GEO.Tests.Integration;

// Whole matches with real hub connections: the REST actions, the round-completion rules in
// MatchService, LobbyGate under concurrent requests, and what each player is told over SignalR.
//
// SignalR delivers one connection's messages in the order they were sent, so once a later
// broadcast has arrived, every earlier one has too. That is how these tests check that something
// happened exactly once without sleeping and hoping.
public class MultiplayerMatchTests(Cs2GeoApiFactory factory) : IAsyncDisposable
{
    private readonly List<TestPlayer> _players = [];

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    private async Task<TestPlayer> PlayerAsync(string nickname)
    {
        var player = await TestPlayer.CreateAsync(factory, nickname);
        _players.Add(player);
        return player;
    }

    [Fact]
    public async Task JoinLobbyGroup_UnknownGuestToken_IsRejected()
    {
        var host = await PlayerAsync("Host");
        var lobby = await host.CreateLobbyAsync(rounds: 1);
        await using var connection = factory.CreateHubConnection();
        await connection.StartAsync(Cancellation);

        await Assert.ThrowsAsync<HubException>(() => connection.InvokeAsync("JoinLobbyGroup", lobby.Id, "not-a-token", Cancellation));
    }

    // Bob is online and hasn't guessed, so Alice's guess must not close the round. The proof is
    // that Bob can still guess it; a closed round would answer 409.
    [Fact]
    public async Task Guess_AnotherOnlinePlayerHasNotGuessedYet_RoundStaysOpenForThem()
    {
        var alice = await PlayerAsync("Alice");
        var bob = await PlayerAsync("Bob");
        var match = await alice.HostMatchAsync(rounds: 2, bob);

        (await alice.GuessAsync(match.MatchId, roundNumber: 1)).EnsureSuccessStatusCode();
        var bobsGuess = await bob.GuessAsync(match.MatchId, roundNumber: 1);

        Assert.Equal(HttpStatusCode.OK, bobsGuess.StatusCode);
        foreach (var player in new[] { alice, bob })
        {
            var completed = await player.WaitForAsync<RoundCompletedDto>("RoundCompleted");
            Assert.Equal(1, completed.RoundNumber);
            Assert.Equal(2, completed.Results.Count);
            Assert.Equal(2, completed.NextRound!.RoundNumber);
        }
    }

    [Fact]
    public async Task Guess_SameRoundTwice_Returns400()
    {
        var alice = await PlayerAsync("Alice");
        var bob = await PlayerAsync("Bob");
        var match = await alice.HostMatchAsync(rounds: 1, bob);
        (await alice.GuessAsync(match.MatchId, roundNumber: 1)).EnsureSuccessStatusCode();

        var again = await alice.GuessAsync(match.MatchId, roundNumber: 1);

        Assert.Equal(HttpStatusCode.BadRequest, again.StatusCode);
    }

    // A client that missed the RoundCompleted broadcast still shows the old round. Its guess must be
    // rejected, not scored against the round that is open now.
    [Fact]
    public async Task Guess_ForARoundThatAlreadyClosed_Returns409()
    {
        var alice = await PlayerAsync("Alice");
        var bob = await PlayerAsync("Bob");
        var match = await alice.HostMatchAsync(rounds: 2, bob);
        (await alice.GuessAsync(match.MatchId, roundNumber: 1)).EnsureSuccessStatusCode();
        (await bob.GuessAsync(match.MatchId, roundNumber: 1)).EnsureSuccessStatusCode();

        var late = await alice.GuessAsync(match.MatchId, roundNumber: 1);

        Assert.Equal(HttpStatusCode.Conflict, late.StatusCode);
    }

    // Both players send the last guess of a round at the same moment, every round. Without the lobby
    // lock both requests could see "everyone has guessed" and close the round (and the match) twice.
    // A race doesn't happen on every run, which is why all five rounds race.
    [Fact]
    public async Task Guess_BothPlayersAtTheSameMoment_EachRoundCompletesOnce()
    {
        var alice = await PlayerAsync("Alice");
        var bob = await PlayerAsync("Bob");
        var match = await alice.HostMatchAsync(rounds: 5, bob);

        for (var round = 1; round <= match.TotalRounds; round++)
        {
            var responses = await Task.WhenAll(
                alice.GuessAsync(match.MatchId, round, 0.2, 0.2),
                bob.GuessAsync(match.MatchId, round, 0.8, 0.8));
            Assert.All(responses, r => Assert.Equal(HttpStatusCode.OK, r.StatusCode));
        }

        foreach (var player in new[] { alice, bob })
        {
            // MatchFinished is the last broadcast of the match, so everything before it has arrived.
            await player.WaitForAsync<List<PlayerStandingDto>>("MatchFinished");
            Assert.Equal([1, 2, 3, 4, 5], player.Received<RoundCompletedDto>("RoundCompleted").Select(r => r.RoundNumber));
            Assert.Single(player.Received<List<PlayerStandingDto>>("MatchFinished"));
        }
    }

    // Bob closes his tab while Alice waits for him. The round closes without him, the match finishes,
    // and the last round's results reach Alice before the final standings.
    [Fact]
    public async Task PlayerLeaves_WhileOthersWaitForThem_RoundAndMatchFinishWithoutThem()
    {
        var alice = await PlayerAsync("Alice");
        var bob = await PlayerAsync("Bob");
        var match = await alice.HostMatchAsync(rounds: 1, bob);
        var guess = await alice.GuessAsync(match.MatchId, roundNumber: 1);
        var alicesScore = (await guess.Content.ReadFromJsonAsync<SubmitGuessAckDto>(Cancellation))!.Score;

        await bob.LeaveAsync();

        var round = await alice.WaitForAsync<RoundCompletedDto>("RoundCompleted");
        var standings = await alice.WaitForAsync<List<PlayerStandingDto>>("MatchFinished");
        Assert.Null(round.NextRound);
        Assert.Equal(alice.Id, Assert.Single(round.Results).PlayerId);
        Assert.Equal(["MatchStarted", "RoundCompleted", "MatchFinished"], alice.EventNames);
        Assert.Equal(alicesScore, standings.Single(s => s.PlayerId == alice.Id).TotalScore);
        Assert.Equal(0, standings.Single(s => s.PlayerId == bob.Id).TotalScore);
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var player in _players)
            await player.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
