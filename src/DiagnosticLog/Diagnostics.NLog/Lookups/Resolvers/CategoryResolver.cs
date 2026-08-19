using System.Collections.Concurrent;
using Dapper;
using Diagnostics.NLog.Interfaces;
using Npgsql;

namespace Diagnostics.NLog.Lookups.Resolvers;

/// <inheritdoc cref="ICategoryResolver"/>
public sealed class CategoryResolver(string connectionString) : ICategoryResolver
{
    private readonly ConcurrentDictionary<string, Lazy<Task<int>>> _cache = new(StringComparer.OrdinalIgnoreCase);

    public Task<int> ResolveIdAsync(string name, CancellationToken ct = default)
    {
        var lazy = _cache.GetOrAdd(
            name,
            key => new Lazy<Task<int>>(
                () => ResolveAndCacheAsync(key, ct),
                LazyThreadSafetyMode.ExecutionAndPublication));

        return lazy.Value;
    }

    private async Task<int> ResolveAndCacheAsync(string name, CancellationToken ct)
    {
        try
        {
            await using var connection = new NpgsqlConnection(connectionString);

            var id = await connection.QuerySingleOrDefaultAsync<int?>(
                "SELECT iid FROM dbo.categories WHERE sname = @name LIMIT 1",
                new { name }).ConfigureAwait(false);

            if (id is null)
            {
                id = await connection.QuerySingleAsync<int>(
                    """
                    INSERT INTO dbo.categories (sname)
                    VALUES (@name)
                    RETURNING iid
                    """,
                    new { name }).ConfigureAwait(false);
            }

            return id.Value;
        }
        catch
        {
            _cache.TryRemove(name, out _);
            throw;
        }
    }
}
