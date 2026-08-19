namespace OrangepuffPortal.Config.Domain.Entity;

/// <summary>
/// An immutable snapshot of a <see cref="ConfigUser"/> row's value from just before it was
/// overwritten. Maps to [config].[ConfigUsersHistory].
/// </summary>
/// <remarks>
/// Append-only by construction: there is no setter/update method, only the constructor — once
/// created a history row can never change. [config].[ConfigUsersHistory] still has
/// iUpdatedUserId/dtUpdatedTime columns (matching the rest of the portal's audit shape) but they stay
/// NULL forever; see <see cref="Infrastructure.Configurations.ConfigUserHistoryConfiguration"/> for how
/// they're mapped as unused shadow properties.
/// </remarks>
public class ConfigUserHistory
{
    public int Id { get; private set; }
    public int ConfigUserId { get; private set; }
    public int UserId { get; private set; }
    public int ConfigId { get; private set; }
    public string? StringValue { get; private set; }
    public int? IntValue { get; private set; }
    public decimal? DecimalValue { get; private set; }
    public bool? BoolValue { get; private set; }
    public bool Active { get; private set; }
    public int InsertedUserId { get; private set; }
    public DateTime InsertedTime { get; private set; }

    private ConfigUserHistory() { } // EF

    public ConfigUserHistory(int configUserId, int userId, int configId, string? stringValue, int? intValue, decimal? decimalValue, bool? boolValue, bool active, int insertedUserId, DateTime utcNow)
    {
        ConfigUserId = configUserId;
        UserId = userId;
        ConfigId = configId;
        StringValue = stringValue;
        IntValue = intValue;
        DecimalValue = decimalValue;
        BoolValue = boolValue;
        Active = active;
        InsertedUserId = insertedUserId;
        InsertedTime = utcNow;
    }
}
