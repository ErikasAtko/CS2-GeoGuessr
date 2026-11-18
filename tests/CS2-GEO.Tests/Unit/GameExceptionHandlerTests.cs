using System.Text.Json;
using CS2_GEO.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace CS2_GEO.Tests.Unit;

public class GameExceptionHandlerTests
{
    private readonly ServiceProvider _services = new ServiceCollection().AddOptions().AddProblemDetails().BuildServiceProvider();

    public static TheoryData<GameException> GameExceptions => new()
    {
        new LobbyFullException(7),
        new RoundClosedException(2),
    };

    [Theory]
    [MemberData(nameof(GameExceptions))]
    public async Task TryHandle_GameException_Returns409WithMessageAsDetail(GameException exception)
    {
        var context = CreateContext();

        var handled = await CreateHandler().TryHandleAsync(context, exception, TestContext.Current.CancellationToken);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
        var problem = await ReadProblemAsync(context);
        Assert.Equal(StatusCodes.Status409Conflict, problem.Status);
        Assert.Equal(exception.Message, problem.Detail);
    }

    [Fact]
    public async Task TryHandle_NormalException_IsNotHandled()
    {
        var context = CreateContext();

        var handled = await CreateHandler().TryHandleAsync(context, new InvalidOperationException("bug"), TestContext.Current.CancellationToken);

        Assert.False(handled);
        Assert.Equal(0, context.Response.Body.Length);
    }

    private GameExceptionHandler CreateHandler() => new(_services.GetRequiredService<IProblemDetailsService>());

    private DefaultHttpContext CreateContext() => new()
    {
        RequestServices = _services,
        Response = { Body = new MemoryStream() },
    };

    private static async Task<ProblemDetails> ReadProblemAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(
            context.Response.Body,
            JsonSerializerOptions.Web,
            TestContext.Current.CancellationToken);
        return problem!;
    }
}
