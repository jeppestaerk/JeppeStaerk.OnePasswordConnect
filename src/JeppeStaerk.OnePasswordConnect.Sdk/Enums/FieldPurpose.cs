using System.Text.Json.Serialization;

namespace JeppeStaerk.OnePasswordConnect.Sdk.Enums;

/// <summary>
/// Represents the purpose of a field, used for autofill functionality.
/// Enum values match the 1Password Connect API naming convention.
/// </summary>
public enum FieldPurpose
{
    /// <summary>
    /// No specific purpose (empty string in API)
    /// </summary>
    [JsonPropertyName("")]
    NONE,

    /// <summary>
    /// Username field
    /// </summary>
    USERNAME,

    /// <summary>
    /// Password field
    /// </summary>
    PASSWORD,

    /// <summary>
    /// Notes field
    /// </summary>
    NOTES
}
