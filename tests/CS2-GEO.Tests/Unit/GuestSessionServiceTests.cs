using CS2_GEO.Services;
using CS2_GEO.Tests.Unit.Fakes;

namespace CS2_GEO.Tests.Unit;

public class GuestSessionServiceTests
{
    private readonly GuestSessionService _service = new(new InMemoryGuestSessionRepository());

    [Fact]
    public async Task Create_NicknameWithSurroundingSpaces_IsTrimmed()
    {
        var result = await _service.CreateAsync("  Ieva  ");

        Assert.Equal("Ieva", result.Session!.Nickname);
    }

    // null: a request body without a nickname still binds, because System.Text.Json doesn't
    // enforce non-nullable reference types.
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abcdefghijklmnopqrstuvwxyz0123456")] // 33 characters
    public async Task Create_InvalidNickname_ReturnsInvalidNickname(string? nickname)
    {
        var result = await _service.CreateAsync(nickname!);

        Assert.Equal(CreateGuestSessionError.InvalidNickname, result.Error);
        Assert.Null(result.Session);
    }

    [Fact]
    public async Task Create_NicknameOfExactly32Characters_IsAccepted()
    {
        var result = await _service.CreateAsync(new string('a', 32));

        Assert.Null(result.Error);
    }

    // The token is the only thing that proves who a guest is, so it has to be long and random.
    [Fact]
    public async Task Create_TwoGuests_GetDifferent256BitTokens()
    {
        var first = await _service.CreateAsync("Ieva");
        var second = await _service.CreateAsync("Tomas");

        Assert.NotEqual(first.Session!.Token, second.Session!.Token);
        Assert.Matches("^[0-9A-F]{64}$", first.Session.Token);
    }

    [Fact]
    public async Task ResolvePlayerId_TokenFromCreate_ReturnsThatPlayer()
    {
        var created = await _service.CreateAsync("Ieva");

        var playerId = await _service.ResolvePlayerIdAsync(created.Session!.Token);

        Assert.Equal(created.Session.PlayerId, playerId);
    }

    [Fact]
    public async Task ResolvePlayerId_UnknownToken_ReturnsNull()
    {
        Assert.Null(await _service.ResolvePlayerIdAsync("not-a-real-token"));
    }
}
