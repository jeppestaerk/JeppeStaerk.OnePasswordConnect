namespace JeppeStaerk.OnePasswordConnect.Sdk.Enums;

/// <summary>
/// Represents a JSON Patch operation type.
/// Enum values match the JSON Patch RFC6902 specification (lowercase).
/// </summary>
public enum PatchOperation
{
    /// <summary>
    /// Add operation
    /// </summary>
    add,

    /// <summary>
    /// Remove operation
    /// </summary>
    remove,

    /// <summary>
    /// Replace operation
    /// </summary>
    replace
}
