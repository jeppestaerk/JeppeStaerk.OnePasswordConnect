using System.Text.Json.Serialization;

namespace JeppeStaerk.OnePasswordConnect.Sdk.Models;

/// <summary>
/// Represents a URL associated with an item.
/// </summary>
public class ItemUrl
{
    /// <summary>
    /// Optional label for the URL.
    /// </summary>
    [JsonPropertyName("label")]
    public string? Label { get; set; }

    /// <summary>
    /// Indicates if this is the primary URL.
    /// </summary>
    [JsonPropertyName("primary")]
    public bool? Primary { get; set; }

    /// <summary>
    /// The URL.
    /// </summary>
    [JsonPropertyName("href")]
    public string Href { get; set; } = string.Empty;
}
