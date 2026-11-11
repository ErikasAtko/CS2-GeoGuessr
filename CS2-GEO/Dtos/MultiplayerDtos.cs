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
