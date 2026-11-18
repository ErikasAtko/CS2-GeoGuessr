using System.Collections.Concurrent;

namespace CS2_GEO.Services;

/// <summary>
/// One lock per lobby, held while anything changes that lobby's state: a player joining, the host
/// starting the match, a guess being scored, or a player dropping out of a round. Serializing those
/// stops two requests from both passing the same status check (two matches for one lobby, a join
/// slipping in after the match snapshot, a round completing twice). Registered as a singleton on
/// purpose: the lock has to outlive any one request's scoped services to actually guard anything.
/// </summary>
public class LobbyGate
{
    private readonly ConcurrentDictionary<int, SemaphoreSlim> _locks = new();

    /// <summary>Waits for the lobby's lock; dispose the result to release it.</summary>
    public async Task<IDisposable> AcquireAsync(int lobbyId)
    {
        var semaphore = _locks.GetOrAdd(lobbyId, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync();
        return new Releaser(semaphore);
    }

    /// <summary>
    /// Drops a finished lobby's lock so the dictionary doesn't grow for the life of the process.
    /// Safe to call while holding it: anyone already waiting keeps their reference and then sees
    /// the lobby's finished status.
    /// </summary>
    public void Forget(int lobbyId) => _locks.TryRemove(lobbyId, out _);

    private sealed class Releaser(SemaphoreSlim semaphore) : IDisposable
    {
        private int _released;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _released, 1) == 0)
                semaphore.Release();
        }
    }
}
