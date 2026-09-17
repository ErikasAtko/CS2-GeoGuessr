namespace CS2_GEO.Dtos;

public class GameDtos   //Data Transfer Objects for Api
{
    public record MapDto(int Id, string Code, string DisplayName, string MinimapUrl, int LocationCount);

    public record RoundDto(int LocationId, string MapCode, string MinimapUrl, string ImageUrl);
}