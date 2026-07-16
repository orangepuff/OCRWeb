using System.Collections.Concurrent;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Diagnostics.NLog.Lookups;

/// <inheritdoc cref="IEnvironmentCategoryResolver"/>
public sealed class EnvironmentCategoryResolver(string connectionString) : IEnvironmentCategoryResolver
{
    private readonly ConcurrentDictionary<string, int> _environmentCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, int> _categoryCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly SemaphoreSlim _gate = new(1, 1);

    public async Task<int> ResolveEnvironmentIdAsync(string name, string? version, string? url, CancellationToken ct = default)
    {
        if (_environmentCache.TryGetValue(name, out var cached))
        {
            return cached;
        }
        
        await _gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (_environmentCache.TryGetValue(name, out cached))
            {
                return cached;
            }
            
            await using var connection = new SqlConnection(connectionString);

            var id = await connection.QuerySingleOrDefaultAsync<int?>(
                "SELECT TOP 1 iId FROM dbo.Environments WHERE sName = @name",
                new { name }).ConfigureAwait(false);

            if (id is null)
            {
                id = await connection.QuerySingleAsync<int>(
                    """
                    INSERT INTO dbo.Environments (sName, sVersion, sUrl)
                    OUTPUT INSERTED.iId
                    VALUES (@name, @version, @url)
                    """,
                    new { name, version, url }).ConfigureAwait(false);
            }

            _environmentCache[name] = id.Value;
            return id.Value;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<int> ResolveCategoryIdAsync(string name, CancellationToken ct = default)
    {
        if (_categoryCache.TryGetValue(name, out var cached))
        {
            return cached;
        }
        
        await _gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (_categoryCache.TryGetValue(name, out cached))
            {
                return cached;
            }
            
            await using var connection = new SqlConnection(connectionString);

            var id = await connection.QuerySingleOrDefaultAsync<int?>(
                "SELECT TOP 1 iId FROM dbo.Categories WHERE sName = @name",
                new { name }).ConfigureAwait(false);

            if (id is null)
            {
                id = await connection.QuerySingleAsync<int>(
                    """
                    INSERT INTO dbo.Categories (sName)
                    OUTPUT INSERTED.iId
                    VALUES (@name)
                    """,
                    new { name }).ConfigureAwait(false);
            }

            _categoryCache[name] = id.Value;
            return id.Value;
        }
        finally
        {
            _gate.Release();
        }
    }
}
