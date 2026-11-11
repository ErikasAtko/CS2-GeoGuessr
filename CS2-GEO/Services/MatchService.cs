using CS2_GEO.Data.Entities;
using CS2_GEO.Data.Repositories;
using CS2_GEO.Dtos;
using CS2_GEO.Game;
using CS2_GEO.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CS2_GEO.Services;

public class MatchService(
    IMatchRepository matches,
    ILobbyRepository lobbies,
    ILocationRepository locations,
    IScoreCalculator calculator,
    IHubContext<MultiplayerHub> hub) : IMatchService
{
    public async Task<StartMatchResult> StartAsync(string joinCode, int requestingPlayerId)
    {
        var lobby = await lobbies.GetByJoinCodeAsync(joinCode);
        if (lobby is null)
            return new StartMatchResult(null, StartMatchError.LobbyNotFound);
        if (lobby.HostPlayerId != requestingPlayerId)
            return new StartMatchResult(null, StartMatchError.NotHost);
        if (lobby.Status != LobbyStatus.Waiting)
            return new StartMatchResult(null, StartMatchError.AlreadyStarted);

        var roundLocations = await locations.GetRandomAsync(lobby.MapCode, lobby.RoundCount);
        if (roundLocations.Count == 0)
            return new StartMatchResult(null, StartMatchError.NoLocationsFound);

        var match = await matches.CreateAsync(lobby, roundLocations);
        var dto = ToStartDto(match);

        await hub.Clients.Group(MultiplayerHub.GroupName(lobby.Id)).SendAsync("MatchStarted", dto);

        return new StartMatchResult(dto, null);
    }

    public async Task<SubmitGuessResult> SubmitGuessAsync(int matchId, int playerId, double x, double y)
    {
        var match = await matches.GetByIdAsync(matchId);
        if (match is null)
            return new SubmitGuessResult(null, SubmitGuessError.MatchNotFound);
        if (match.Status != MatchStatus.InProgress)
            return new SubmitGuessResult(null, SubmitGuessError.MatchFinished);

        var participant = match.Participants.FirstOrDefault(p => p.PlayerId == playerId);
        if (participant is null)
            return new SubmitGuessResult(null, SubmitGuessError.NotAParticipant);

        // The current round is whichever hasn't collected a guess from every participant yet.
        var currentRound = match.Rounds
            .OrderBy(r => r.RoundNumber)
            .FirstOrDefault(r => r.Guesses.Count < match.Participants.Count);
        if (currentRound is null)
            return new SubmitGuessResult(null, SubmitGuessError.MatchFinished);

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
        await matches.AddGuessAsync(guess);

        // A fresh DB read, not the in-memory currentRound.Guesses collection: EF Core's automatic
        // fixup of an already-loaded navigation after Add() isn't guaranteed to have happened yet,
        // so checking the in-memory collection here made round completion miss broadcasts.
        var roundGuesses = await matches.GetGuessesForRoundAsync(currentRound.Id);
        if (roundGuesses.Count >= match.Participants.Count)
            await CompleteRoundAsync(match, currentRound, roundGuesses);

        return new SubmitGuessResult(new SubmitGuessAckDto(result.Score), null);
    }

    private async Task CompleteRoundAsync(Match match, MatchRound round, List<PlayerGuess> guesses)
    {
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
        await matches.SaveChangesAsync();

        var standings = match.Participants
            .OrderByDescending(p => p.TotalScore)
            .Select(p => new PlayerStandingDto(p.PlayerId, p.Player.Nickname, p.TotalScore))
            .ToList();

        await hub.Clients.Group(MultiplayerHub.GroupName(match.LobbyId)).SendAsync("MatchFinished", standings);
    }

    private static StartMatchResultDto ToStartDto(Match match) => new(
        match.Id,
        match.LobbyId,
        match.Rounds.Count,
        ToRoundDto(match.Rounds.OrderBy(r => r.RoundNumber).First()));

    private static MatchRoundDto ToRoundDto(MatchRound round) =>
        new(round.RoundNumber, round.LocationId, round.Location.ImageUrl, round.Location.Map.MinimapUrl);
}
