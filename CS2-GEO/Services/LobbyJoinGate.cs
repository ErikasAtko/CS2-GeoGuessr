using System.Collections.Concurrent;

namespace CS2_GEO.Services;

/// <summary>
/// Serializes concurrent join attempts for the same lobby, so two players racing for the last slot
/// can't both succeed. Registered as a singleton on purpose: the lock has to outlive any one request's
/// scoped <see cref="LobbyService"/> instance to actually guard anything.
/// </summary>
public class LobbyJoinGate
{
    private readonly ConcurrentDictionary<int, SemaphoreSlim> locks = new();

    public SemaphoreSlim For(int lobbyId) => locks.GetOrAdd(lobbyId, _ => new SemaphoreSlim(1, 1));
}
