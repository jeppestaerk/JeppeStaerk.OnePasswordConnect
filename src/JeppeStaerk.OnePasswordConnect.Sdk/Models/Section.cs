using System.Text.Json.Serialization;

namespace JeppeStaerk.OnePasswordConnect.Sdk.Models;

/// <summary>
/// Represents a section within a 1Password item.
/// </summary>
public class Section
{
    /// <summary>
    /// Unique identifier for the section.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Label/name of the section.
    /// </summary>
    [JsonPropertyName("label")]
    public string? Label { get; set; }
}
