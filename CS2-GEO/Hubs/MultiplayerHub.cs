using Microsoft.AspNetCore.SignalR;

namespace CS2_GEO.Hubs;

/// <summary>
/// Live push channel for lobby/match updates. Mutations still go through the REST endpoints
/// (so they stay easy to test and keep one source of truth); this hub only fans the result out
/// to everyone else already connected to that lobby's group.
/// </summary>
public class MultiplayerHub(ConnectionRegistry connections) : Hub
{
    /// <summary>Subscribes the caller's connection to a lobby's live updates.</summary>
    public async Task JoinLobbyGroup(int lobbyId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(lobbyId));
        connections.Add(Context.ConnectionId, lobbyId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var lobbyId = connections.Remove(Context.ConnectionId);
        if (lobbyId is not null)
            await Clients.Group(GroupName(lobbyId.Value)).SendAsync("PlayerDisconnected", Context.ConnectionId);

        await base.OnDisconnectedAsync(exception);
    }

    public static string GroupName(int lobbyId) => $"lobby-{lobbyId}";
}
