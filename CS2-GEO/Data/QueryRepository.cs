using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace CS2_GEO.Data;

/// <summary>
/// The id-keyed lookups every entity repository needs (find one, batch-lookup many, add, save),
/// implemented once instead of per entity (DRY). Entity-specific repositories compose this
/// for their plumbing and add the domain-shaped methods callers actually ask for.
/// </summary>
public class QueryRepository<TEntity>(GameDbContext db) where TEntity : class
{
    // The CancellationToken is always the last parameter (the .NET convention), so the optional
    // "include" is an overload rather than a default parameter that would have to come after it.

    public Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken) =>
        db.Set<TEntity>().FirstOrDefaultAsync(predicate, cancellationToken);

    /// <summary>Like <see cref="FindAsync(Expression{Func{TEntity, bool}}, CancellationToken)"/>, with related data loaded by <paramref name="include"/>.</summary>
    public Task<TEntity?> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> include,
        CancellationToken cancellationToken) =>
        include(db.Set<TEntity>()).FirstOrDefaultAsync(predicate, cancellationToken);

    /// <summary>Entities matching <paramref name="predicate"/>, with related data loaded by <paramref name="include"/>, keyed by whatever key the caller selects.</summary>
    public Task<Dictionary<TKey, TEntity>> ToDictionaryAsync<TKey>(
        Expression<Func<TEntity, bool>> predicate,
        Func<TEntity, TKey> keySelector,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> include,
        CancellationToken cancellationToken) where TKey : notnull =>
        include(db.Set<TEntity>()).Where(predicate).ToDictionaryAsync(keySelector, cancellationToken);

    public void Add(TEntity entity) => db.Set<TEntity>().Add(entity);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
