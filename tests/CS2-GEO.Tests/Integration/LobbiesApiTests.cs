using System.Net;
using System.Net.Http.Json;
using CS2_GEO.Dtos;

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
