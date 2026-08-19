using OrangepuffPortal.Shared.Domain;

namespace OrangepuffPortal.Config.Domain.Entity;

/// <summary>
/// A user's current value for one <see cref="ConfigItem"/>. Maps to [config].[ConfigUsers]. Exactly
/// one of the four value properties is meaningful, matching the config's <c>ConfigType</c>.
/// </summary>
/// <remarks>
/// Unlike <see cref="ConfigSection"/>/<see cref="ConfigItem"/>, this uses the shared portal
/// <c>AuditableEntity</c>: rows are only ever written during a real authenticated request (a user
/// changing their own or, if permitted, another user's setting), never by unattended startup seeding,
/// so a real actor is always available to attribute the change to.
/// </remarks>
public class ConfigUser : AuditableEntity
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public int ConfigId { get; private set; }
    public string? StringValue { get; private set; }
    public int? IntValue { get; private set; }
    public decimal? DecimalValue { get; private set; }
    public bool? BoolValue { get; private set; }
    public bool Active { get; private set; } = true;

    private ConfigUser() { } // EF

    public ConfigUser(int userId, int configId, string? stringValue, int? intValue, decimal? decimalValue, bool? boolValue, int actorUserId, DateTime utcNow)
    {
        UserId = userId;
        ConfigId = configId;
        StringValue = stringValue;
        IntValue = intValue;
        DecimalValue = decimalValue;
        BoolValue = boolValue;
        MarkInserted(actorUserId, utcNow);
    }

    /// <summary>
    /// Overwrite this row's value. Caller (<see cref="Infrastructure.ConfigUserValueService"/>) is
    /// responsible for snapshotting the current value into ConfigUsersHistory first.
    /// </summary>
    public void SetValue(string? stringValue, int? intValue, decimal? decimalValue, bool? boolValue, int actorUserId, DateTime utcNow)
    {
        StringValue = stringValue;
        IntValue = intValue;
        DecimalValue = decimalValue;
        BoolValue = boolValue;
        MarkUpdated(actorUserId, utcNow);
    }
}
