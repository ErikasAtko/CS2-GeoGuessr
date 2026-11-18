using CS2_GEO.Hubs;

namespace CS2_GEO.Tests.Unit;

public class ConnectionRegistryTests
{
    private readonly ConnectionRegistry _registry = new();

    // After a network blip the client reconnects with a new connection id, often before the server
    // has noticed the old one is gone. The player must count as online the whole time, or the round
    // would close without them.
    [Fact]
    public void OnlinePlayers_PlayerReconnectedBeforeOldConnectionDropped_StaysOnline()
    {
        _registry.Add("old", lobbyId: 1, playerId: 7);
        _registry.Add("new", lobbyId: 1, playerId: 7);

        _registry.Remove("old");
        Assert.Contains(7, _registry.OnlinePlayers(1));

        _registry.Remove("new");
        Assert.DoesNotContain(7, _registry.OnlinePlayers(1));
    }

    [Fact]
    public void OnlinePlayers_OnlyCountsThatLobby()
    {
        _registry.Add("a", lobbyId: 1, playerId: 7);
        _registry.Add("b", lobbyId: 2, playerId: 8);

        Assert.Equal([7], _registry.OnlinePlayers(1));
    }

    [Fact]
    public void Remove_KnownConnection_ReturnsItsLobbyAndPlayer()
    {
        _registry.Add("a", lobbyId: 3, playerId: 9);

        Assert.Equal((3, 9), _registry.Remove("a"));
    }

    // OnDisconnectedAsync runs for every connection, including ones that never joined a lobby.
    [Fact]
    public void Remove_ConnectionThatNeverJoinedALobby_ReturnsNull()
    {
        Assert.Null(_registry.Remove("never-joined"));
    }
}
