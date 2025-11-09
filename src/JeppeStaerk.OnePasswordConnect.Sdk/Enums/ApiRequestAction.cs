namespace JeppeStaerk.OnePasswordConnect.Sdk.Enums;

/// <summary>
/// Represents the action performed in an API request.
/// Enum values match the 1Password Connect API naming convention.
/// </summary>
public enum ApiRequestAction
{
    /// <summary>
    /// Read operation
    /// </summary>
    READ,

    /// <summary>
    /// Create operation
    /// </summary>
    CREATE,

    /// <summary>
    /// Update operation
    /// </summary>
    UPDATE,

    /// <summary>
    /// Delete operation
    /// </summary>
    DELETE
}
