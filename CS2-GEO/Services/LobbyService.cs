using System.Security.Cryptography;
using CS2_GEO.Data.Entities;
using CS2_GEO.Data.Repositories;
using CS2_GEO.Dtos;
using CS2_GEO.Exceptions;
using CS2_GEO.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CS2_GEO.Services;

public class LobbyService(ILobbyRepository lobbies, LobbyGate gate, IHubContext<MultiplayerHub> hub) : ILobbyService
{
    private const string JoinCodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // no 0/O/1/I - avoids ambiguity when read aloud

    public async Task<CreateLobbyResult> CreateAsync(int hostPlayerId, string? mapCode, int roundCount, CancellationToken cancellationToken)
    {
        if (roundCount < 1)
            return new CreateLobbyResult(null, CreateLobbyError.TooFewRounds);

        // One save that nothing else waits on, so the request's token can cancel it whole.
        var lobby = await lobbies.CreateAsync(hostPlayerId, GenerateJoinCode(), mapCode, roundCount, cancellationToken);

        return new CreateLobbyResult(ToDto(lobby), null);
    }

    public async Task<JoinLobbyResult> JoinAsync(string joinCode, int playerId, CancellationToken cancellationToken)
    {
        var lobbyId = await lobbies.GetIdByJoinCodeAsync(joinCode, cancellationToken);
        if (lobbyId is null)
            return new JoinLobbyResult(null, JoinLobbyError.NotFound);

        // Same lock as starting the match: two players racing for the last slot must not both
        // succeed, and nobody may join after the host's start has already snapshotted the members.
        using var _ = await gate.AcquireAsync(lobbyId.Value, cancellationToken);

        var lobby = (await lobbies.GetByJoinCodeAsync(joinCode, cancellationToken))!;
        if (lobby.Status != LobbyStatus.Waiting)
            return new JoinLobbyResult(null, JoinLobbyError.AlreadyStarted);
        if (lobby.Members.Any(m => m.PlayerId == playerId))
            return new JoinLobbyResult(ToDto(lobby), null); // already a member - joining again is a no-op, not an error
        if (lobby.Members.Count >= lobby.MaxPlayers)
            throw new LobbyFullException(lobby.Id);

        // From here on the lobby has changed, so finish even if the caller gives up: everyone else
        // still needs the LobbyUpdated broadcast with the new member.
        await lobbies.AddMemberAsync(lobby.Id, playerId, CancellationToken.None);

        var refreshed = await lobbies.GetByJoinCodeAsync(joinCode, CancellationToken.None);
        var dto = ToDto(refreshed!);

        // Push the fresh member list to everyone already connected to this lobby's group. Sent while
        // still holding the lock so it can't arrive after a MatchStarted that came later.
        await hub.Clients.Group(MultiplayerHub.GroupName(lobby.Id)).SendAsync("LobbyUpdated", dto);

        return new JoinLobbyResult(dto, null);
    }

    public async Task<LobbyDto?> GetCurrentAsync(int playerId, CancellationToken cancellationToken)
    {
        var lobby = await lobbies.GetActiveForPlayerAsync(playerId, cancellationToken);
        return lobby is null ? null : ToDto(lobby);
    }

    private static LobbyDto ToDto(Lobby lobby) => new(
        lobby.Id,
        lobby.JoinCode,
        lobby.Status.ToString(),
        lobby.MapCode,
        lobby.RoundCount,
        lobby.MaxPlayers,
        lobby.Members
            .Select(m => new LobbyMemberDto(m.PlayerId, m.Player.Nickname, m.IsReady, m.PlayerId == lobby.HostPlayerId))
            .ToList());

    private static string GenerateJoinCode()
    {
        var bytes = RandomNumberGenerator.GetBytes(6);
        return new string(bytes.Select(b => JoinCodeAlphabet[b % JoinCodeAlphabet.Length]).ToArray());
    }
}
