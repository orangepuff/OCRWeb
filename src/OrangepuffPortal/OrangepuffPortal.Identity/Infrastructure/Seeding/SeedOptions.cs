namespace OrangepuffPortal.Identity.Infrastructure.Seeding;

/// <summary>
/// Bound from the "Seed" configuration section. Provides the default admin profile
/// created on first run when the Users table is empty.
/// </summary>
public class SeedOptions
{
    public const string SectionName = "Seed";

    public string AdminUsername { get; set; } = "admin";
    public string? AdminEmail { get; set; } = "admin@example.local";
    public string? AdminDisplayName { get; set; } = "Administrator";
    public string AdminPassword { get; set; } = string.Empty;
}
