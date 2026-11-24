using CS2_GEO.Data.Entities;
using CS2_GEO.Data.Repositories;
using CS2_GEO.Dtos;
using CS2_GEO.Exceptions;
using CS2_GEO.Game;
using CS2_GEO.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CS2_GEO.Services;

public class MatchService(
    IMatchRepository matches,
    ILobbyRepository lobbies,
    ILocationRepository locations,
    IScoreCalculator calculator,
    LobbyGate gate,
    ConnectionRegistry connections,
    IHubContext<MultiplayerHub> hub) : IMatchService
{
    // Every method that reads lobby/match state and then acts on it takes the lobby's lock first and
    // only then loads that state, so the read is fresh and no other request can change it mid-way.
    // Hub broadcasts go out while still holding the lock, so clients receive them in the order the
    // changes happened.
    //
    // The request's cancellationToken covers only the reads and the wait for the lock. Every write,
    // and everything after it, uses CancellationToken.None (see IMatchService): a match or round
    // must never be left half-done because one player closed their tab mid-request.

    public async Task<StartMatchResult> StartAsync(string joinCode, int requestingPlayerId, CancellationToken cancellationToken)
    {
        var lobbyId = await lobbies.GetIdByJoinCodeAsync(joinCode, cancellationToken);
        if (lobbyId is null)
            return new StartMatchResult(null, StartMatchError.LobbyNotFound);

        using var _ = await gate.AcquireAsync(lobbyId.Value, cancellationToken);

        var lobby = (await lobbies.GetByJoinCodeAsync(joinCode, cancellationToken))!;
        if (lobby.HostPlayerId != requestingPlayerId)
            return new StartMatchResult(null, StartMatchError.NotHost);
        if (lobby.Status != LobbyStatus.Waiting)
            return new StartMatchResult(null, StartMatchError.AlreadyStarted);

        var roundLocations = await locations.GetRandomAsync(lobby.MapCode, lobby.RoundCount, cancellationToken);
        if (roundLocations.Count == 0)
            return new StartMatchResult(null, StartMatchError.NoLocationsFound);

        var match = await matches.CreateAsync(lobby, roundLocations, CancellationToken.None);
        var dto = ToStartDto(match);

        await hub.Clients.Group(MultiplayerHub.GroupName(lobby.Id)).SendAsync("MatchStarted", dto);

        return new StartMatchResult(dto, null);
    }

    public async Task<SubmitGuessResult> SubmitGuessAsync(int matchId, int playerId, int roundNumber, double x, double y, CancellationToken cancellationToken)
    {
        var lobbyId = await matches.GetLobbyIdAsync(matchId, cancellationToken);
        if (lobbyId is null)
            return new SubmitGuessResult(null, SubmitGuessError.MatchNotFound);

        using var _ = await gate.AcquireAsync(lobbyId.Value, cancellationToken);

        var match = (await matches.GetByIdAsync(matchId, cancellationToken))!;
        if (match.Status != MatchStatus.InProgress)
            return new SubmitGuessResult(null, SubmitGuessError.MatchFinished);

        var participant = match.Participants.FirstOrDefault(p => p.PlayerId == playerId);
        if (participant is null)
            return new SubmitGuessResult(null, SubmitGuessError.NotAParticipant);

        var currentRound = CurrentRound(match);
        if (currentRound is null)
            return new SubmitGuessResult(null, SubmitGuessError.MatchFinished);
        if (currentRound.RoundNumber != roundNumber)
            throw new RoundClosedException(roundNumber);
        if (currentRound.Guesses.Any(g => g.PlayerId == playerId))
            return new SubmitGuessResult(null, SubmitGuessError.AlreadyGuessed);

        var result = calculator.Calculate(new Point(x, y), currentRound.Location.ToPoint());
        var guess = new PlayerGuess
        {
            MatchRoundId = currentRound.Id,
            PlayerId = playerId,
            X = x,
            Y = y,
            Distance = currentRound.Location.ToMapUnits(result.Distance),
            Score = result.Score,
            SubmittedAt = DateTime.UtcNow,
        };

        participant.TotalScore += result.Score;
        await matches.AddGuessAsync(guess, CancellationToken.None);

        await CompleteRoundIfDoneAsync(match, currentRound);

        return new SubmitGuessResult(new SubmitGuessAckDto(result.Score), null);
    }

    // Read-only, so unlike the methods above this doesn't take the lobby's lock: a resuming client
    // briefly seeing a round that's about to close is fine, it'll be corrected by the next broadcast.
    public async Task<StartMatchResultDto?> GetCurrentStateAsync(int lobbyId, CancellationToken cancellationToken)
    {
        var match = await matches.GetInProgressByLobbyIdAsync(lobbyId, cancellationToken);
        if (match is null)
            return null;

        var currentRound = CurrentRound(match);
        return currentRound is null ? null : ToStartDto(match, currentRound);
    }

    public async Task HandlePlayerLeftAsync(int lobbyId)
    {
        using var _ = await gate.AcquireAsync(lobbyId, CancellationToken.None);

        var match = await matches.GetInProgressByLobbyIdAsync(lobbyId, CancellationToken.None);
        var currentRound = match is null ? null : CurrentRound(match);
        if (match is null || currentRound is null)
            return;

        await CompleteRoundIfDoneAsync(match, currentRound);
    }

    /// <summary>The first round whose results haven't gone out yet, or <c>null</c> once every round has.</summary>
    private static MatchRound? CurrentRound(Match match) =>
        match.Rounds.OrderBy(r => r.RoundNumber).FirstOrDefault(r => r.CompletedAt is null);

    /// <summary>
    /// Closes the round once nobody still connected is left to guess. Players who disconnected don't
    /// hold it open (they just score nothing for it), but a round with no guesses at all stays open,
    /// so a lobby where everyone dropped doesn't skip through its remaining rounds.
    /// </summary>
    private async Task CompleteRoundIfDoneAsync(Match match, MatchRound round)
    {
        // A fresh DB read, not the in-memory round.Guesses collection: EF Core's automatic
        // fixup of an already-loaded navigation after Add() isn't guaranteed to have happened yet,
        // so checking the in-memory collection here made round completion miss broadcasts.
        var guesses = await matches.GetGuessesForRoundAsync(round.Id, CancellationToken.None);
        if (guesses.Count == 0)
            return;

        var guessed = guesses.Select(g => g.PlayerId).ToHashSet();
        var online = connections.OnlinePlayers(match.LobbyId);
        var stillWaitingOnSomeone = match.Participants.Any(p => !guessed.Contains(p.PlayerId) && online.Contains(p.PlayerId));
        if (stillWaitingOnSomeone)
            return;

        await CompleteRoundAsync(match, round, guesses);
    }

    private async Task CompleteRoundAsync(Match match, MatchRound round, List<PlayerGuess> guesses)
    {
        round.CompletedAt = DateTime.UtcNow;
        await matches.SaveChangesAsync(CancellationToken.None);

        var results = guesses
            .Select(g => new PlayerRoundResultDto(
                g.PlayerId,
                match.Participants.First(p => p.PlayerId == g.PlayerId).Player.Nickname,
                g.X,
                g.Y,
                g.Distance,
                g.Score))
            .ToList();

        var nextRound = match.Rounds
            .OrderBy(r => r.RoundNumber)
            .FirstOrDefault(r => r.RoundNumber > round.RoundNumber);

        var payload = new RoundCompletedDto(
            round.RoundNumber,
            round.Location.X,
            round.Location.Y,
            results,
            nextRound is null ? null : ToRoundDto(nextRound));

        await hub.Clients.Group(MultiplayerHub.GroupName(match.LobbyId)).SendAsync("RoundCompleted", payload);

        if (nextRound is null)
            await FinishMatchAsync(match);
    }

    private async Task FinishMatchAsync(Match match)
    {
        match.Status = MatchStatus.Finished;
        match.FinishedAt = DateTime.UtcNow;
        match.Lobby.Status = LobbyStatus.Finished;
        await matches.SaveChangesAsync(CancellationToken.None);

        var standings = match.Participants
            .OrderByDescending(p => p.TotalScore)
            .Select(p => new PlayerStandingDto(p.PlayerId, p.Player.Nickname, p.TotalScore))
            .ToList();

        await hub.Clients.Group(MultiplayerHub.GroupName(match.LobbyId)).SendAsync("MatchFinished", standings);

        // Nothing changes a finished lobby any more, so its lock can go.
        gate.Forget(match.LobbyId);
    }

    private static StartMatchResultDto ToStartDto(Match match) =>
        ToStartDto(match, match.Rounds.OrderBy(r => r.RoundNumber).First());

    private static StartMatchResultDto ToStartDto(Match match, MatchRound round) => new(
        match.Id,
        match.LobbyId,
        match.Rounds.Count,
        ToRoundDto(round));

    private static MatchRoundDto ToRoundDto(MatchRound round) =>
        new(round.RoundNumber, round.LocationId, round.Location.ImageUrl, round.Location.Map.MinimapUrl);
}
