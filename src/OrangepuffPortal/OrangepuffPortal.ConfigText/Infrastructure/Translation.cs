using OrangepuffPortal.ConfigText.Contract.Interfaces;
using OrangepuffPortal.Shared.Auditing;
using OrangepuffPortal.Shared.Translation;

namespace OrangepuffPortal.ConfigText.Infrastructure;

/// <summary>
/// <see cref="ITranslation"/> backed by <see cref="IConfigTextReader"/>'s cached, culture-resolved read
/// path — registered once by <c>AddConfigTextModule</c>, so it's automatically available to this
/// module, every other portal module, and any consuming app's own modules.
/// </summary>
internal class Translation(IConfigTextReader reader, ICurrentUser currentUser) : ITranslation
{
    public async Task<string> TranslateAsync(string code, string module, CancellationToken cancellationToken = default)
    {
        var entries = await reader.GetAllAsync(currentUser.CultureCode, modules: [module], cancellationToken: cancellationToken);
        var match = entries.FirstOrDefault(x => x.SModule == module && x.STextCode == code);
        return match?.SText ?? code;
    }
}
