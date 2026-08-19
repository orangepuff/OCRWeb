using Microsoft.Extensions.DependencyInjection;
using OrangepuffPortal.ConfigText.Contract.Interfaces;
using OrangepuffPortal.Shared.Modules;

namespace OrangepuffPortal.Host.ConfigText;

/// <summary>
/// Seeds the portal shell's own default text (see <see cref="PortalShellTextSeed"/>) into
/// [configtext].[ConfigTextDefinition] at every consuming app's startup, insert-only unless a seed
/// entry sets <c>BtReplace</c> — same semantics as <see cref="IConfigTextWriter"/> always has. Runs
/// automatically through <c>MigratePortalModulesAsync()</c>, so no consuming app needs its own call
/// site for this (unlike a consuming app's own module text, which is each app's own responsibility).
/// </summary>
/// <remarks>
/// <see cref="IPortalModule"/> instances are registered as singletons, but <see cref="IConfigTextWriter"/>
/// is scoped — so it's resolved from the scoped <paramref name="serviceProvider"/> passed into
/// <see cref="SeedAsync"/> (same pattern every other <see cref="IPortalModule"/> uses for its scoped
/// DbContext), never injected via this class's own constructor, which would be a captive-dependency
/// DI validation error at startup.
/// </remarks>
internal sealed class PortalShellTextPortalModule : IPortalModule
{
    private const string CultureCode = "en-US";

    public string Name => "PortalShellText";

    public Task MigrateAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default) =>
        serviceProvider.GetRequiredService<IConfigTextWriter>().UpsertManyAsync(CultureCode, PortalShellTextSeed.Load(CultureCode), cancellationToken);
}
