namespace OrangepuffPortal.ConfigText.Contract;

/// <summary>
/// One consuming app's default text for a single (module, code, type) key, as loaded from that
/// app's own source-controlled seed file. Field names mirror the DB column names deliberately —
/// this is a wire/seed contract, not a domain model.
/// </summary>
/// <param name="SModule">Owning app/module, e.g. "OCRWeb.ProjectManagement".</param>
/// <param name="STextCode">Key within the module, e.g. "pageTitle".</param>
/// <param name="STextType">"msg" or "lbl".</param>
/// <param name="SText">The default text.</param>
/// <param name="SNote">Optional remark describing where/how this text is used.</param>
/// <param name="BtReplace">
/// Seed-file-only override: when true, an existing row is updated instead of skipped. Never
/// persisted as a DB column.
/// </param>
public sealed record ConfigTextSeedEntry(
    string SModule,
    string STextCode,
    string STextType,
    string SText,
    string? SNote = null,
    bool BtReplace = false);
