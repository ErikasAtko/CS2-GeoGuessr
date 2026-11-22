using System.Net.Http.Json;
using CS2_GEO.Dtos;
using Microsoft.AspNetCore.SignalR.Client;

namespace CS2_GEO.Tests.Integration;

/// <summary>
/// One guest player using the multiplayer API the way the frontend does: actions over REST with
/// the guest token header, and a hub connection that records every broadcast it receives, in order.
/// Every test creates its own players, so tests never see each other's lobbies or matches.
/// </summary>
public sealed class TestPlayer : IAsyncDisposable
{
    private static readonly TimeSpan _eventTimeout = TimeSpan.FromSeconds(10);

    private readonly Cs2GeoApiFactory _factory;
    private readonly List<(string Name, object Payload)> _events = [];
    private HubConnection? _hub;

    private TestPlayer(Cs2GeoApiFactory factory, HttpClient http, GuestSessionDto session)
    {
        _factory = factory;
        Http = http;
        Id = session.PlayerId;
        Token = session.Token;
    }

    public HttpClient Http { get; }
    public int Id { get; }
    public string Token { get; }

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    public static async Task<TestPlayer> CreateAsync(Cs2GeoApiFactory factory, string nickname = "Player")
    {
        var http = factory.CreateClient();
        var response = await http.PostAsJsonAsync("/api/guest-sessions", new { nickname }, Cancellation);
        response.EnsureSuccessStatusCode();
        var session = (await response.Content.ReadFromJsonAsync<GuestSessionDto>(Cancellation))!;

        http.DefaultRequestHeaders.Add("X-Guest-Token", session.Token);
        return new TestPlayer(factory, http, session);
    }

    public async Task<LobbyDto> CreateLobbyAsync(int rounds)
    {
        var response = await Http.PostAsJsonAsync("/api/lobbies", new { mapCode = (string?)null, roundCount = rounds }, Cancellation);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LobbyDto>(Cancellation))!;
    }

    public Task<HttpResponseMessage> GetCurrentAsync() => Http.GetAsync("/api/lobbies/current", Cancellation);

    public Task<HttpResponseMessage> JoinAsync(string joinCode) =>
        Http.PostAsync($"/api/lobbies/{joinCode}/join", content: null, Cancellation);

    public Task<HttpResponseMessage> StartAsync(string joinCode) =>
        Http.PostAsJsonAsync("/api/matches/start", new { joinCode }, Cancellation);

    public Task<HttpResponseMessage> GuessAsync(int matchId, int roundNumber, double x = 0.5, double y = 0.5) =>
        Http.PostAsJsonAsync($"/api/matches/{matchId}/guess", new { roundNumber, x, y }, Cancellation);

    /// <summary>
    /// Creates a lobby, lets the other players join, connects everyone to the hub (which is what makes
    /// them count as online), starts the match and waits until everyone has been told it started.
    /// </summary>
    public async Task<StartMatchResultDto> HostMatchAsync(int rounds, params TestPlayer[] others)
    {
        var lobby = await CreateLobbyAsync(rounds);
        foreach (var player in others)
            (await player.JoinAsync(lobby.JoinCode)).EnsureSuccessStatusCode();

        TestPlayer[] everyone = [this, .. others];
        foreach (var player in everyone)
            await player.ConnectAsync(lobby.Id);

        var response = await StartAsync(lobby.JoinCode);
        response.EnsureSuccessStatusCode();
        foreach (var player in everyone)
            await player.WaitForAsync<StartMatchResultDto>("MatchStarted");

        return (await response.Content.ReadFromJsonAsync<StartMatchResultDto>(Cancellation))!;
    }

    public async Task ConnectAsync(int lobbyId)
    {
        _hub = _factory.CreateHubConnection();
        _hub.On<StartMatchResultDto>("MatchStarted", e => Record("MatchStarted", e));
        _hub.On<RoundCompletedDto>("RoundCompleted", e => Record("RoundCompleted", e));
        _hub.On<List<PlayerStandingDto>>("MatchFinished", e => Record("MatchFinished", e));

        await _hub.StartAsync(Cancellation);
        await _hub.InvokeAsync("JoinLobbyGroup", lobbyId, Token, Cancellation);
    }

    /// <summary>Closes the hub connection, like closing the browser tab.</summary>
    public Task LeaveAsync() => _hub!.StopAsync(Cancellation);

    /// <summary>Waits for the first broadcast of that name (and matching the condition), received so far or still to come.</summary>
    public async Task<T> WaitForAsync<T>(string eventName, Func<T, bool>? matches = null)
    {
        var deadline = DateTime.UtcNow + _eventTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var found = Received(eventName, matches).FirstOrDefault();
            if (found is not null)
                return found;
            await Task.Delay(20, Cancellation);
        }

        Assert.Fail($"Player {Id} never received {eventName}. Received: {string.Join(", ", EventNames)}");
        return default!;
    }

    public List<T> Received<T>(string eventName, Func<T, bool>? matches = null)
    {
        lock (_events)
        {
            return _events
                .Where(e => e.Name == eventName)
                .Select(e => (T)e.Payload)
                .Where(e => matches?.Invoke(e) ?? true)
                .ToList();
        }
    }

    /// <summary>Every broadcast received so far, in arrival order.</summary>
    public List<string> EventNames
    {
        get
        {
            lock (_events)
                return _events.Select(e => e.Name).ToList();
        }
    }

    // Hub callbacks run on SignalR's threads while the test thread reads the list.
    private void Record(string name, object payload)
    {
        lock (_events)
            _events.Add((name, payload));
    }

    public async ValueTask DisposeAsync()
    {
        if (_hub is not null)
            await _hub.DisposeAsync();
        Http.Dispose();
    }
}
