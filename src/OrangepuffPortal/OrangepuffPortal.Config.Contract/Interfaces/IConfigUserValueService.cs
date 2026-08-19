namespace OrangepuffPortal.Config.Contract.Interfaces;

/// <summary>
/// Read/write path for a user's own config values. Unlike the catalog, these are live
/// application/user data — never seeded.
/// </summary>
public interface IConfigUserValueService
{
    Task<ConfigUserValueDto?> GetValueAsync(int userId, string configCode, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ConfigUserValueDto>> GetValuesAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Every visible section and its visible configs, each annotated with this user's current value
    /// (all four value fields null if they've never set one) — the read path a settings UI needs to
    /// render the full catalog, not just configs the user already has a value for.
    /// </summary>
    Task<IReadOnlyList<UserConfigSectionDto>> GetSectionsForUserAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a user's value for <paramref name="configCode"/>. If a value already exists, its current
    /// (about-to-be-replaced) contents are snapshotted into ConfigUsersHistory first. Does not check
    /// Configs.BtAllowUserEdit — that is the caller's responsibility.
    /// </summary>
    Task SetValueAsync(int userId, string configCode, ConfigValueInput value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a ConfigUsers row for every config that has a default configured, for a newly-created
    /// user — skips any config the user already has a value for (idempotency; this can in principle run
    /// more than once for the same user). Never overwrites an existing value, and never writes history
    /// (nothing to snapshot on a first-ever insert).
    /// </summary>
    Task ApplyDefaultsForNewUserAsync(int userId, int actorUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// The mirror image of <see cref="ApplyDefaultsForNewUserAsync"/>: when a newly-created config has a
    /// default value, inserts a ConfigUsers row for every existing user so they see that default
    /// immediately instead of a blank value until they explicitly save one. No-op if the config has no
    /// default configured. Skips any user that already has a value for it (idempotency), same reasoning
    /// as the new-user path.
    /// </summary>
    Task ApplyDefaultForNewConfigAsync(int configId, int actorUserId, CancellationToken cancellationToken = default);
}
