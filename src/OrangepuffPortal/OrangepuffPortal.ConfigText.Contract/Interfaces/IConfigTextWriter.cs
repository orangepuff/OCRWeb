namespace OrangepuffPortal.ConfigText.Contract.Interfaces;

/// <summary>
/// Push path for a consuming app's own default text into the shared ConfigTextDefinition table.
/// Called in-process at the consuming app's own startup, not over HTTP.
/// </summary>
public interface IConfigTextWriter
{
    /// <summary>
    /// Upserts one culture's worth of entries. Writes two rows per entry: one at
    /// <paramref name="cultureCode"/> and one at the wildcard culture "*". A row that already
    /// exists is left untouched unless <see cref="ConfigTextSeedEntry.BtReplace"/> is true.
    /// </summary>
    Task UpsertManyAsync(
        string cultureCode,
        IReadOnlyCollection<ConfigTextSeedEntry> entries,
        CancellationToken cancellationToken = default);
}
