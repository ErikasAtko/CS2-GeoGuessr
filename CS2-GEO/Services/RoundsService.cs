using CS2_GEO.Data;
using CS2_GEO.Dtos;
using CS2_GEO.Game;
using Microsoft.EntityFrameworkCore;

namespace CS2_GEO.Services;

public class RoundsService(GameDbContext db) : IRoundsService
{
    public Task<GameDtos.RoundDto?> GetRandomRoundAsync(string? map)
    {
        var query = db.Locations.AsQueryable();

        if (map is not null)
            query = query.Where(l => l.Map.Code == map);

        return query
            .OrderBy(_ => EF.Functions.Random())
            .Select(l => new GameDtos.RoundDto(l.Id, l.Map.Code, l.Map.MinimapUrl, l.ImageUrl))
            .FirstOrDefaultAsync();
    }

    public async Task<GameDtos.GuessResultDto?> SubmitGuessAsync(int locationId, GameDtos.GuessRequestDto guess)
    {
        var location = await db.Locations.Include(l => l.Map).FirstOrDefaultAsync(l => l.Id == locationId);
        if (location is null)
            return null;

        var result = Scoring.CalculateResult(new Point(guess.X, guess.Y), new Point(location.X, location.Y));
        var distanceUnits = result.Distance * location.Map.SizeUnits;

        return new GameDtos.GuessResultDto(
            result.ActualLocation.X, result.ActualLocation.Y, distanceUnits, result.Score, result.Tier.ToString());
    }
}
