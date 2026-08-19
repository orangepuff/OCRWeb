namespace OrangepuffPortal.Config.Contract;

/// <summary>
/// Matches <c>Configs.iConfigType</c> — selects which <c>ConfigUsers</c> value column is live for a
/// given config.
/// </summary>
public enum ConfigValueType
{
    String = 0,
    Int = 1,
    Decimal = 2,
    Boolean = 3,
}
