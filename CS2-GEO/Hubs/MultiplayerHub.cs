using CS2_GEO.Services;
using Microsoft.AspNetCore.SignalR;

namespace CS2_GEO.Hubs;

/// <summary>
/// Live push channel for lobby/match updates. Mutations still go through the REST endpoints
/// (so they stay easy to test and keep one source of truth); this hub only fans the result out
/// to everyone else already connected to that lobby's group.
/// </summary>
public class MultiplayerHub(ConnectionRegistry connections, IGuestSessionService sessions, IMatchService matchService) : Hub
{
    /// <summary>
    /// Subscribes the caller's connection to a lobby's live updates. Called again after every automatic
    /// reconnect, because a reconnected client gets a new connection id that isn't in any group yet.
    /// </summary>
    public async Task JoinLobbyGroup(int lobbyId, string guestToken)
    {
        var playerId = await sessions.ResolvePlayerIdAsync(guestToken, Context.ConnectionAborted)
            ?? throw new HubException("Unknown guest token");

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(lobbyId), Context.ConnectionAborted);
        connections.Add(Context.ConnectionId, lobbyId, playerId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var entry = connections.Remove(Context.ConnectionId);
        if (entry is { } left)
        {
            await Clients.Group(GroupName(left.LobbyId)).SendAsync("PlayerDisconnected", left.PlayerId);

            // If they were the last player the current round was waiting on, finish it without them
            // instead of leaving everyone else stuck.
            if (!connections.OnlinePlayers(left.LobbyId).Contains(left.PlayerId))
                await matchService.HandlePlayerLeftAsync(left.LobbyId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public static string GroupName(int lobbyId) => $"lobby-{lobbyId}";
}
