namespace OrangepuffPortal.Shared.Translation;

/// <summary>
/// Resolves a (code, module) pair to display text from ConfigTextDefinition. A single shared
/// service — not scoped to one module — so any consuming app's own modules (e.g. OCRWeb's) can use
/// it for their own text too, the same way they call <c>IConfigTextWriter</c> to seed it.
/// Lives in Shared so any module can depend on it without referencing the ConfigText module.
/// </summary>
public interface ITranslation
{
    /// <summary>
    /// Looks up <paramref name="module"/>/<paramref name="code"/> for the current user's culture
    /// (<see cref="Auditing.ICurrentUser.CultureCode"/>), falling back to the "*" wildcard row, and
    /// finally to <paramref name="code"/> itself if no row exists at all — so a missing translation
    /// is visible (the raw code shows up) instead of blank.
    /// </summary>
    Task<string> TranslateAsync(string code, string module, CancellationToken cancellationToken = default);
}
