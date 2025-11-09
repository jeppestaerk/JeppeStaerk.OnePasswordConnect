namespace JeppeStaerk.OnePasswordConnect.Sdk.Enums;

/// <summary>
/// Represents the type of a field in a 1Password item.
/// Enum values match the 1Password Connect API naming convention.
/// </summary>
public enum FieldType
{
    /// <summary>
    /// String/text field
    /// </summary>
    STRING,

    /// <summary>
    /// Email address field
    /// </summary>
    EMAIL,

    /// <summary>
    /// Concealed/password field
    /// </summary>
    CONCEALED,

    /// <summary>
    /// URL field
    /// </summary>
    URL,

    /// <summary>
    /// One-time password field
    /// </summary>
    OTP,

    /// <summary>
    /// Date field
    /// </summary>
    DATE,

    /// <summary>
    /// Month and year field
    /// </summary>
    MONTH_YEAR,

    /// <summary>
    /// Menu/dropdown field
    /// </summary>
    MENU
}
