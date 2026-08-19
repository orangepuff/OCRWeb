using Diagnostics.Abstractions;
using Diagnostics.Abstractions.Interfaces;
using Diagnostics.NLog.Buffering;
using Diagnostics.NLog.Interfaces;
using Npgsql;
using NpgsqlTypes;

namespace Diagnostics.NLog.Targets;

/// <summary>
/// Sink for completed <see cref="ITransactionScope"/> spans: bounded/batched → <c>NpgsqlBinaryImporter</c> → <c>dbo.transactions</c> (design doc §3/§6).
/// Named "Target" to mirror <see cref="LogsTarget"/>, but fed directly by <c>TransactionScopeImpl.Dispose</c> (<see cref="Enqueue"/>) rather than through NLog's logger dispatch — a transaction always flushes exactly one row regardless of NLog min level rules, matching the unconditional "measures duration and flushes on dispose" semantics in §3.
/// </summary>
public sealed class TransactionsTarget : IAsyncDisposable
{
    private readonly string _connectionString;
    private readonly IEnvironmentResolver _environmentResolver;
    private readonly ICategoryResolver _categoryResolver;
    private readonly DiagnosticsOptions _options;
    private readonly WriteMetrics _metrics = new();
    private readonly Dictionary<string, int> _categoryIdCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _categoryLock = new();

    private BoundedBatchWriter<TransactionRecord>? _writer;
    private int _environmentId;

    public TransactionsTarget(
        string connectionString,
        IEnvironmentResolver environmentResolver,
        ICategoryResolver categoryResolver,
        DiagnosticsOptions options)
    {
        _connectionString = connectionString;
        _environmentResolver = environmentResolver;
        _categoryResolver = categoryResolver;
        _options = options;
    }

    public WriteMetrics Metrics => _metrics;

    /// <summary>
    /// Idempotent — safe to call repeatedly (e.g. on every DB-config poll).
    /// </summary>
    public void EnsureStarted()
    {
        if (_writer is not null)
        {
            return;
        }

        try
        {
            _environmentId = _environmentResolver
                .ResolveIdAsync(_options.EnvironmentName, _options.EnvironmentVersion, _options.EnvironmentUrl)
                .GetAwaiter().GetResult();
        }
        catch
        {
            // DiagnosticLogs unreachable at startup — never let this take the host down (§7).
            // Same fallback reasoning as LogsTarget.InitializeTarget.
        }

        _writer = new BoundedBatchWriter<TransactionRecord>(
            _options.MaxQueueSize,
            _options.FlushBatchSize,
            _options.FlushInterval,
            FlushBatchAsync,
            WriteFallback,
            _metrics);
    }

    public void Enqueue(TransactionRecord record)
    {
        EnsureStarted();
        _writer?.Enqueue(record);
    }

    private int ResolveCategoryIdCached(string category)
    {
        lock (_categoryLock)
        {
            if (_categoryIdCache.TryGetValue(category, out var cached))
            {
                return cached;
            }
        }

        var resolved = _categoryResolver.ResolveIdAsync(category).GetAwaiter().GetResult();

        lock (_categoryLock)
        {
            _categoryIdCache[category] = resolved;
        }

        return resolved;
    }

    private async Task FlushBatchAsync(IReadOnlyList<TransactionRecord> rows, CancellationToken ct)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(ct).ConfigureAwait(false);

        await using var writer = await connection.BeginBinaryImportAsync(
            "COPY dbo.transactions (sid, sparentid, ienvironmentid, icategoryid, scorrelationid, " +
            "smessage, surl, dtstarttime, iduration, xrequestxml, srequestjson, srequesttext, " +
            "xresponsexml, sresponsejson, sresponsetext, suser, scustomattributes, ssql, sbaseurl) " +
            "FROM STDIN (FORMAT BINARY)", ct).ConfigureAwait(false);

        foreach (var row in rows)
        {
            await writer.StartRowAsync(ct).ConfigureAwait(false);

            await writer.WriteAsync(row.Id, NpgsqlDbType.Uuid, ct).ConfigureAwait(false);

            if (row.ParentId.HasValue)
            {
                await writer.WriteAsync(row.ParentId.Value, NpgsqlDbType.Uuid, ct).ConfigureAwait(false);
            }
            else
            {
                await writer.WriteNullAsync(ct).ConfigureAwait(false);
            }

            await writer.WriteAsync(_environmentId, NpgsqlDbType.Integer, ct).ConfigureAwait(false);
            await writer.WriteAsync(ResolveCategoryIdCached(row.Category), NpgsqlDbType.Integer, ct).ConfigureAwait(false);
            await writer.WriteAsync(row.CorrelationId, NpgsqlDbType.Uuid, ct).ConfigureAwait(false);

            if (row.Message is not null) { await writer.WriteAsync(row.Message, NpgsqlDbType.Text, ct).ConfigureAwait(false); }
            else { await writer.WriteNullAsync(ct).ConfigureAwait(false); }

            if (row.Url is not null) { await writer.WriteAsync(row.Url, NpgsqlDbType.Text, ct).ConfigureAwait(false); }
            else { await writer.WriteNullAsync(ct).ConfigureAwait(false); }

            await writer.WriteAsync(row.StartTime, NpgsqlDbType.TimestampTz, ct).ConfigureAwait(false);

            if (row.DurationMs.HasValue) { await writer.WriteAsync(row.DurationMs.Value, NpgsqlDbType.Integer, ct).ConfigureAwait(false); }
            else { await writer.WriteNullAsync(ct).ConfigureAwait(false); }

            if (row.RequestXml is not null) { await writer.WriteAsync(row.RequestXml, NpgsqlDbType.Text, ct).ConfigureAwait(false); }
            else { await writer.WriteNullAsync(ct).ConfigureAwait(false); }

            if (row.RequestJson is not null) { await writer.WriteAsync(row.RequestJson, NpgsqlDbType.Text, ct).ConfigureAwait(false); }
            else { await writer.WriteNullAsync(ct).ConfigureAwait(false); }

            if (row.RequestText is not null) { await writer.WriteAsync(row.RequestText, NpgsqlDbType.Text, ct).ConfigureAwait(false); }
            else { await writer.WriteNullAsync(ct).ConfigureAwait(false); }

            if (row.ResponseXml is not null) { await writer.WriteAsync(row.ResponseXml, NpgsqlDbType.Text, ct).ConfigureAwait(false); }
            else { await writer.WriteNullAsync(ct).ConfigureAwait(false); }

            if (row.ResponseJson is not null) { await writer.WriteAsync(row.ResponseJson, NpgsqlDbType.Text, ct).ConfigureAwait(false); }
            else { await writer.WriteNullAsync(ct).ConfigureAwait(false); }

            if (row.ResponseText is not null) { await writer.WriteAsync(row.ResponseText, NpgsqlDbType.Text, ct).ConfigureAwait(false); }
            else { await writer.WriteNullAsync(ct).ConfigureAwait(false); }

            if (row.User is not null) { await writer.WriteAsync(row.User, NpgsqlDbType.Varchar, ct).ConfigureAwait(false); }
            else { await writer.WriteNullAsync(ct).ConfigureAwait(false); }

            if (row.CustomAttributesJson is not null) { await writer.WriteAsync(row.CustomAttributesJson, NpgsqlDbType.Text, ct).ConfigureAwait(false); }
            else { await writer.WriteNullAsync(ct).ConfigureAwait(false); }

            if (row.Sql is not null) { await writer.WriteAsync(row.Sql, NpgsqlDbType.Text, ct).ConfigureAwait(false); }
            else { await writer.WriteNullAsync(ct).ConfigureAwait(false); }

            if (row.BaseUrl is not null) { await writer.WriteAsync(row.BaseUrl, NpgsqlDbType.Text, ct).ConfigureAwait(false); }
            else { await writer.WriteNullAsync(ct).ConfigureAwait(false); }
        }

        await writer.CompleteAsync(ct).ConfigureAwait(false);
    }

    private void WriteFallback(IReadOnlyList<TransactionRecord> rows)
    {
        var path = ResolveFallbackPath(_options.LocalFallbackLogFile);
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.AppendAllLines(path, rows.Select(r => $"[TRANSACTION] {r.StartTime:O} id={r.Id} parent={r.ParentId} category={r.Category} " + $"duration={r.DurationMs}ms message={r.Message}"));
    }

    private static string ResolveFallbackPath(string template) => template.Replace("${shortdate}", DateTime.UtcNow.ToString("yyyy-MM-dd"));

    public async ValueTask DisposeAsync()
    {
        if (_writer is not null)
        {
            await _writer.DisposeAsync().ConfigureAwait(false);
        }
    }
}
