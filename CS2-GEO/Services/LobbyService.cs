using System.Security.Cryptography;
using CS2_GEO.Data.Entities;
using CS2_GEO.Data.Repositories;
using CS2_GEO.Dtos;
using CS2_GEO.Exceptions;
using CS2_GEO.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CS2_GEO.Services;

public class LobbyService(ILobbyRepository lobbies, LobbyJoinGate joinGate, IHubContext<MultiplayerHub> hub) : ILobbyService
{
    private const string JoinCodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // no 0/O/1/I - avoids ambiguity when read aloud

    public async Task<CreateLobbyResult> CreateAsync(int hostPlayerId, string? mapCode, int roundCount)
    {
        if (roundCount < 1)
            return new CreateLobbyResult(null, CreateLobbyError.TooFewRounds);

        var lobby = await lobbies.CreateAsync(hostPlayerId, GenerateJoinCode(), mapCode, roundCount);

        return new CreateLobbyResult(ToDto(lobby), null);
    }

    public async Task<JoinLobbyResult> JoinAsync(string joinCode, int playerId)
    {
        var lobby = await lobbies.GetByJoinCodeAsync(joinCode);
        if (lobby is null)
            return new JoinLobbyResult(null, JoinLobbyError.NotFound);
        if (lobby.Status != LobbyStatus.Waiting)
            return new JoinLobbyResult(null, JoinLobbyError.AlreadyStarted);

        // Serialize joins to this lobby: two players racing for the last slot must not both succeed.
        var gate = joinGate.For(lobby.Id);
        var addedNewMember = false;
        await gate.WaitAsync();
        try
        {
            lobby = await lobbies.GetByJoinCodeAsync(joinCode);
            if (lobby!.Members.Any(m => m.PlayerId == playerId))
                return new JoinLobbyResult(ToDto(lobby), null); // already a member - joining again is a no-op, not an error

            if (lobby.Members.Count >= lobby.MaxPlayers)
                throw new LobbyFullException(lobby.Id);

            await lobbies.AddMemberAsync(lobby.Id, playerId);
            addedNewMember = true;
        }
        finally
        {
            gate.Release();
        }

        var refreshed = await lobbies.GetByJoinCodeAsync(joinCode);
        var dto = ToDto(refreshed!);

        // Push the fresh member list to everyone already connected to this lobby's group.
        if (addedNewMember)
            await hub.Clients.Group(MultiplayerHub.GroupName(refreshed!.Id)).SendAsync("LobbyUpdated", dto);

        return new JoinLobbyResult(dto, null);
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
