using System.Net;

namespace CS2_GEO.Tests.Integration;

// Who may start a match and guess in it, through the real API. Live behavior (rounds closing,
// broadcasts) is in MultiplayerMatchTests.
public class MatchesApiTests(Cs2GeoApiFactory factory)
{
    [Fact]
    public async Task Start_ByAPlayerWhoIsNotTheHost_Returns403()
    {
        await using var host = await TestPlayer.CreateAsync(factory);
        await using var guest = await TestPlayer.CreateAsync(factory);
        var lobby = await host.CreateLobbyAsync(rounds: 2);
        await guest.JoinAsync(lobby.JoinCode);

        var response = await guest.StartAsync(lobby.JoinCode);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // A double-clicked Start button. Exactly one match may come out of it, or players end up
    // guessing in different matches that never finish.
    [Fact]
    public async Task Start_SeveralRequestsAtOnce_StartsExactlyOneMatch()
    {
        await using var host = await TestPlayer.CreateAsync(factory);
        var lobby = await host.CreateLobbyAsync(rounds: 2);

        var responses = await Task.WhenAll(Enumerable.Range(0, 3).Select(_ => host.StartAsync(lobby.JoinCode)));

        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.OK);
        Assert.All(responses.Where(r => r.StatusCode != HttpStatusCode.OK),
            r => Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode));
    }

    // The match snapshots the lobby's members when it starts; someone joining later would be in
    // the lobby but not in the match, stuck on "waiting for the host".
    [Fact]
    public async Task Join_AfterTheMatchStarted_Returns400()
    {
        await using var host = await TestPlayer.CreateAsync(factory);
        await using var late = await TestPlayer.CreateAsync(factory);
        var lobby = await host.CreateLobbyAsync(rounds: 2);
        (await host.StartAsync(lobby.JoinCode)).EnsureSuccessStatusCode();

        var response = await late.JoinAsync(lobby.JoinCode);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Guess_ByAPlayerNotInTheMatch_Returns403()
    {
        await using var host = await TestPlayer.CreateAsync(factory);
        await using var outsider = await TestPlayer.CreateAsync(factory);
        var match = await host.HostMatchAsync(rounds: 2);

        var response = await outsider.GuessAsync(match.MatchId, roundNumber: 1);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Guess_UnknownMatch_Returns404()
    {
        await using var player = await TestPlayer.CreateAsync(factory);

        var response = await player.GuessAsync(matchId: 999999, roundNumber: 1);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
