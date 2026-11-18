using CS2_GEO.Services;

namespace CS2_GEO.Tests.Unit;

// LobbyGate is the one lock every request that changes a lobby goes through (join, start, guess,
// leave). These tests pin down what the services rely on: one holder per lobby at a time, lobbies
// independent of each other, and a release that can't be repeated by accident.
public class LobbyGateTests
{
    private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(5);

    private readonly LobbyGate _gate = new();

    [Fact]
    public async Task Acquire_SameLobby_SecondCallerWaitsUntilTheFirstReleases()
    {
        var first = await _gate.AcquireAsync(1);

        var second = _gate.AcquireAsync(1);
        Assert.False(second.IsCompleted);

        first.Dispose();
        (await second.WaitAsync(_timeout, TestContext.Current.CancellationToken)).Dispose();
    }

    [Fact]
    public async Task Acquire_DifferentLobbies_DoNotWaitForEachOther()
    {
        using var lobby1 = await _gate.AcquireAsync(1);

        var lobby2 = _gate.AcquireAsync(2);

        Assert.True(lobby2.IsCompletedSuccessfully);
        (await lobby2).Dispose();
    }

    // Releasing twice must not hand the lock to two callers: without the guard, a stale second
    // Dispose would release the lock someone else is now holding.
    [Fact]
    public async Task Dispose_CalledTwice_DoesNotReleaseSomeoneElsesLock()
    {
        var stale = await _gate.AcquireAsync(1);
        stale.Dispose();
        using var current = await _gate.AcquireAsync(1);

        stale.Dispose();
        var next = _gate.AcquireAsync(1);

        Assert.False(next.IsCompleted);
    }

    // FinishMatchAsync forgets a finished lobby while still holding its lock. Anyone already
    // waiting must still get the lock instead of waiting forever.
    [Fact]
    public async Task Forget_WhileHeld_WaitersStillGetTheLock()
    {
        var holder = await _gate.AcquireAsync(1);
        var waiter = _gate.AcquireAsync(1);

        _gate.Forget(1);
        holder.Dispose();

        (await waiter.WaitAsync(_timeout, TestContext.Current.CancellationToken)).Dispose();
    }
}
