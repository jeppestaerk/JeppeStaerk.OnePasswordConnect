namespace JeppeStaerk.OnePasswordConnect.Sdk.Enums;

/// <summary>
/// Represents the type of a 1Password vault.
/// Enum values match the 1Password Connect API naming convention.
/// </summary>
public enum VaultType
{
    /// <summary>
    /// User-created vault
    /// </summary>
    USER_CREATED,

    /// <summary>
    /// Personal vault
    /// </summary>
    PERSONAL,

    /// <summary>
    /// Everyone vault (shared)
    /// </summary>
    EVERYONE,

    /// <summary>
    /// Transfer vault
    /// </summary>
    TRANSFER
}
