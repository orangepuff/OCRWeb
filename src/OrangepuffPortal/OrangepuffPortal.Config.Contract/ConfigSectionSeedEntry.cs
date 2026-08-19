namespace OrangepuffPortal.Config.Contract;

/// <summary>
/// One consuming app's default section, with its configs nested inside so the writer can resolve
/// each config's section without a second lookup. Field names mirror the DB column names
/// deliberately — this is a wire/seed contract, not a domain model.
/// </summary>
/// <param name="SSectionDesc">Plain fallback description, always present.</param>
/// <param name="STextCode">Pointer into ConfigTextDefinition.sTextCode for the localized label.</param>
/// <param name="Configs">Configs belonging to this section.</param>
/// <param name="BtShow">Whether this section is shown in a settings UI.</param>
/// <param name="ISortOrder">Display order among sections; null sorts after any ordered sections, by Id.</param>
/// <param name="BtReplace">
/// Seed-file-only override: when true, an existing row (matched by STextCode alone, globally) has
/// its SSectionDesc/BtShow updated instead of skipped. Never persisted as a DB column.
/// </param>
public sealed record ConfigSectionSeedEntry(
    string SSectionDesc,
    string STextCode,
    IReadOnlyCollection<ConfigSeedEntry> Configs,
    bool BtShow = true,
    int? ISortOrder = null,
    bool BtReplace = false);
