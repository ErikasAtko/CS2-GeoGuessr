using System.Net;
using System.Net.Http.Json;
using CS2_GEO.Dtos;
using CS2_GEO.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CS2_GEO.Tests.Integration;

public class LobbiesApiTests(Cs2GeoApiFactory factory) : IAsyncDisposable
{
    private readonly List<TestPlayer> _crowd = [];

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    [Fact]
    public async Task CreateGuestSession_BlankNickname_Returns400()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/guest-sessions", new { nickname = "   " }, Cancellation);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // No password backs this yet: typing a taken name reclaims that player, token and all. Uses the
    // raw client, not TestPlayer.CreateAsync, which uniquifies nicknames so unrelated tests sharing
    // the one database (Cs2GeoApiFactory) don't reconnect into each other's players - here the
    // collision is exactly what's under test, so both calls need the identical literal nickname.
    [Fact]
    public async Task CreateGuestSession_SameNicknameAgain_ReconnectsToTheSamePlayer()
    {
        var nickname = $"Bob-{Guid.NewGuid():N}"[..32];
        var first = await CreateSessionAsync(nickname);

        var second = await CreateSessionAsync(nickname);

        Assert.Equal(first.PlayerId, second.PlayerId);
        Assert.NotEqual(first.Token, second.Token);
    }

    // Matching is case-insensitive, and the response always carries the name as it was first
    // stored - so a client that typed "bob" sees "Bob" and can't silently fork a second player.
    [Fact]
    public async Task CreateGuestSession_SameNicknameDifferentCase_ReconnectsToTheSamePlayer()
    {
        var nickname = $"Bob-{Guid.NewGuid():N}"[..32];
        var first = await CreateSessionAsync(nickname);

        var second = await CreateSessionAsync(nickname.ToUpperInvariant());

        Assert.Equal(first.PlayerId, second.PlayerId);
        Assert.Equal(nickname, second.Nickname);
    }

    private async Task<GuestSessionDto> CreateSessionAsync(string nickname)
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/guest-sessions", new { nickname }, Cancellation);
        return (await response.Content.ReadFromJsonAsync<GuestSessionDto>(Cancellation))!;
    }

    // The token from the guest session is what identifies the player on every later request.
    [Fact]
    public async Task CreateLobby_WithGuestToken_CallerBecomesTheHost()
    {
        await using var host = await TestPlayer.CreateAsync(factory);

        var lobby = await host.CreateLobbyAsync(rounds: 3);

        var member = Assert.Single(lobby.Members);
        Assert.Equal(host.Id, member.PlayerId);
        Assert.True(member.IsHost);
        Assert.Equal("Waiting", lobby.Status);
    }

    [Fact]
    public async Task CreateLobby_WithoutGuestToken_Returns401()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/lobbies", new { roundCount = 3 }, Cancellation);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateLobby_ZeroRounds_Returns400()
    {
        await using var host = await TestPlayer.CreateAsync(factory);

        var response = await host.Http.PostAsJsonAsync("/api/lobbies", new { roundCount = 0 }, Cancellation);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Join_UnknownCode_Returns404()
    {
        await using var player = await TestPlayer.CreateAsync(factory);

        var response = await player.JoinAsync("NOPE00");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // The frontend may send the join again (a double click, a retry); that must not add a second membership.
    [Fact]
    public async Task Join_SamePlayerTwice_IsOnlyAMemberOnce()
    {
        await using var host = await TestPlayer.CreateAsync(factory);
        await using var guest = await TestPlayer.CreateAsync(factory);
        var lobby = await host.CreateLobbyAsync(rounds: 3);

        await guest.JoinAsync(lobby.JoinCode);
        var second = await guest.JoinAsync(lobby.JoinCode);

        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        var members = (await second.Content.ReadFromJsonAsync<LobbyDto>(Cancellation))!.Members;
        Assert.Single(members, m => m.PlayerId == guest.Id);
    }

    // LobbyFullException, thrown by LobbyService when the cap is reached, becomes 409 Conflict with a message.
    [Fact]
    public async Task Join_FullLobby_Returns409()
    {
        var players = await CreatePlayersAsync(9);
        var lobby = await players[0].CreateLobbyAsync(rounds: 3);
        foreach (var player in players[1..8])
            (await player.JoinAsync(lobby.JoinCode)).EnsureSuccessStatusCode();

        var response = await players[8].JoinAsync(lobby.JoinCode);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains("full", await response.Content.ReadAsStringAsync(Cancellation));
    }

    // Three players ask for the last free slot at the same moment. Without LobbyGate all three could
    // pass the "is there room?" check before any of them is saved, and the lobby would end up with 10.
    [Fact]
    public async Task Join_ThreePlayersRaceForTheLastSlot_ExactlyOneGetsIn()
    {
        var players = await CreatePlayersAsync(10);
        var host = players[0];
        var lobby = await host.CreateLobbyAsync(rounds: 3);
        foreach (var player in players[1..7])
            (await player.JoinAsync(lobby.JoinCode)).EnsureSuccessStatusCode();

        var responses = await Task.WhenAll(players[7..].Select(p => p.JoinAsync(lobby.JoinCode)));

        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.OK);
        Assert.Equal(2, responses.Count(r => r.StatusCode == HttpStatusCode.Conflict));
        var final = await (await host.JoinAsync(lobby.JoinCode)).Content.ReadFromJsonAsync<LobbyDto>(Cancellation);
        Assert.Equal(lobby.MaxPlayers, final!.Members.Count);
    }

    // What a reloaded page sees: it only has the guest token, so this is how it finds out whether
    // it was in a lobby or a match at all.
    [Fact]
    public async Task GetCurrent_NotInAnyLobby_Returns404()
    {
        await using var player = await TestPlayer.CreateAsync(factory);

        var response = await player.GetCurrentAsync();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetCurrent_WithoutGuestToken_Returns401()
    {
        var response = await factory.CreateClient().GetAsync("/api/lobbies/current", Cancellation);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCurrent_LobbyNotStartedYet_ReturnsLobbyAndNoMatch()
    {
        await using var host = await TestPlayer.CreateAsync(factory);
        var lobby = await host.CreateLobbyAsync(rounds: 2);

        var response = await host.GetCurrentAsync();

        response.EnsureSuccessStatusCode();
        var session = (await response.Content.ReadFromJsonAsync<CurrentSessionDto>(Cancellation))!;
        Assert.Equal(lobby.Id, session.Lobby.Id);
        Assert.Null(session.Match);
    }

    // Also proves this returns the round that's actually open, not always round 1: by the time Alice
    // asks, round 1 has already closed and round 2 is current.
    [Fact]
    public async Task GetCurrent_MatchInProgress_ReturnsLobbyAndTheOpenRound()
    {
        var alice = await PlayerAsync("Alice");
        var bob = await PlayerAsync("Bob");
        var match = await alice.HostMatchAsync(rounds: 2, bob);
        (await alice.GuessAsync(match.MatchId, roundNumber: 1)).EnsureSuccessStatusCode();
        (await bob.GuessAsync(match.MatchId, roundNumber: 1)).EnsureSuccessStatusCode();
        await alice.WaitForAsync<RoundCompletedDto>("RoundCompleted");

        var response = await alice.GetCurrentAsync();

        response.EnsureSuccessStatusCode();
        var session = (await response.Content.ReadFromJsonAsync<CurrentSessionDto>(Cancellation))!;
        Assert.Equal(match.LobbyId, session.Lobby.Id);
        Assert.NotNull(session.Match);
        Assert.Equal(match.MatchId, session.Match!.MatchId);
        Assert.Equal(2, session.Match.Round.RoundNumber);
    }

    [Fact]
    public async Task GetCurrent_MatchAlreadyFinished_Returns404()
    {
        var alice = await PlayerAsync("Alice");
        var bob = await PlayerAsync("Bob");
        var match = await alice.HostMatchAsync(rounds: 1, bob);
        (await alice.GuessAsync(match.MatchId, roundNumber: 1)).EnsureSuccessStatusCode();
        (await bob.GuessAsync(match.MatchId, roundNumber: 1)).EnsureSuccessStatusCode();
        await alice.WaitForAsync<List<PlayerStandingDto>>("MatchFinished");

        var response = await alice.GetCurrentAsync();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // A player who closes the tab while their join is still waiting for the lobby's lock must not
    // be added once the lock frees up: the request's token reaches LobbyGate, so the wait itself is
    // cancelled. (ASP.NET answers such a request with 499 and only a Debug log, not a 500 error.)
    [Fact]
    public async Task Join_CancelledWhileWaitingForTheLobby_DoesNotJoin()
    {
        var host = await PlayerAsync("Host");
        var quitter = await PlayerAsync("Quitter");
        var lobby = await host.CreateLobbyAsync(rounds: 1);
        var gate = factory.Services.GetRequiredService<LobbyGate>();
        var busy = await gate.AcquireAsync(lobby.Id, Cancellation); // another request is busy with this lobby
        using var closeTab = CancellationTokenSource.CreateLinkedTokenSource(Cancellation);
        try
        {
            var join = quitter.Http.PostAsync($"/api/lobbies/{lobby.JoinCode}/join", content: null, closeTab.Token);
            // Long enough for the request to reach the lock. A slower server only makes the test weaker
            // (cancelled earlier, still not joined), never fail by accident.
            await Task.Delay(TimeSpan.FromMilliseconds(300), Cancellation);
            await closeTab.CancelAsync();

            // If LobbyGate ignored the token, the request would keep waiting for the lock we hold and
            // this would time out instead of seeing the cancellation.
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => join.WaitAsync(TimeSpan.FromSeconds(5), Cancellation));
        }
        finally
        {
            busy.Dispose(); // even on failure, or a request stuck on the lock would hang the test run
        }

        // Take the lock once more: a join that was still queued behind it (if the token had been
        // ignored) has now run, so the check below can't pass just by being early.
        (await gate.AcquireAsync(lobby.Id, Cancellation)).Dispose();
        var current = await quitter.GetCurrentAsync();

        Assert.Equal(HttpStatusCode.NotFound, current.StatusCode);
    }

    private async Task<TestPlayer> PlayerAsync(string nickname)
    {
        var player = await TestPlayer.CreateAsync(factory, nickname);
        _crowd.Add(player);
        return player;
    }

    private async Task<TestPlayer[]> CreatePlayersAsync(int count)
    {
        var players = await Task.WhenAll(Enumerable.Range(1, count).Select(i => TestPlayer.CreateAsync(factory, $"Player {i}")));
        _crowd.AddRange(players);
        return players;
    }

    // xUnit creates a new instance of this class per test and disposes it afterwards, pass or fail.
    public async ValueTask DisposeAsync()
    {
        foreach (var player in _crowd)
            await player.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
