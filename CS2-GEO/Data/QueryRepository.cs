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
    public Task<TEntity?> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null)
    {
        var query = include is null ? db.Set<TEntity>() : include(db.Set<TEntity>());
        return query.FirstOrDefaultAsync(predicate);
    }

    /// <summary>Entities matching <paramref name="predicate"/>, keyed by whatever key the caller selects.</summary>
    public Task<Dictionary<TKey, TEntity>> ToDictionaryAsync<TKey>(
        Expression<Func<TEntity, bool>> predicate,
        Func<TEntity, TKey> keySelector,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null) where TKey : notnull
    {
        var query = include is null ? db.Set<TEntity>() : include(db.Set<TEntity>());
        return query.Where(predicate).ToDictionaryAsync(keySelector);
    }

    public void Add(TEntity entity) => db.Set<TEntity>().Add(entity);

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
