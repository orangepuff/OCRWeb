namespace OrangepuffPortal.Config.Contract.Interfaces;

/// <summary>
/// Push path for a consuming app's own default sections/configs into the shared config catalog.
/// Called in-process at the consuming app's own startup, not over HTTP — mirrors
/// <c>IConfigTextWriter</c>'s insert-only/BtReplace seeding contract.
/// </summary>
public interface IConfigCatalogWriter
{
    /// <summary>
    /// Upserts <paramref name="module"/>'s sections and their nested configs. A section already
    /// present (matched by STextCode alone, globally — the caller is expected to prefix its own
    /// STextCode values, e.g. "ocrProjectManagement.general", so two consuming apps sharing this
    /// catalog don't collide) is left untouched unless it sets BtReplace; same for each config
    /// within it (matched by SConfigCode alone, globally). <paramref name="module"/> itself is only
    /// used for log attribution now.
    /// </summary>
    /// <param name="existingUserIds">
    /// Every user id that already exists, so a brand-new config's DefaultValue (if any) can be backfilled
    /// onto them — Config has no way to enumerate users itself, so the caller (a consuming app's own
    /// startup code, which does have access to Identity) supplies this. Ignored for configs that already
    /// existed before this call (a BtReplace re-seed never retroactively backfills anyone).
    /// </param>
    Task UpsertAsync(
        string module,
        IReadOnlyCollection<ConfigSectionSeedEntry> sections,
        IReadOnlyList<int>? existingUserIds = null,
        CancellationToken cancellationToken = default);
}
