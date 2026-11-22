namespace CS2_GEO.Dtos;

public record CreateGuestSessionRequestDto(string Nickname);

public record GuestSessionDto(int PlayerId, string Nickname, string Token);

public record CreateLobbyRequestDto(string? MapCode, int RoundCount);

public record LobbyMemberDto(int PlayerId, string Nickname, bool IsReady, bool IsHost);

public record LobbyDto(
    int Id,
    string JoinCode,
    string Status,
    string? MapCode,
    int RoundCount,
    int MaxPlayers,
    List<LobbyMemberDto> Members);

/// <summary>The caller's active lobby and, if a match is already running there, its current round - what a client needs to resume after a refresh or reconnect, since all it has is a guest token.</summary>
public record CurrentSessionDto(LobbyDto Lobby, StartMatchResultDto? Match);
