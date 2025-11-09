using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace JeppeStaerk.OnePasswordConnect.Sdk.Models;

/// <summary>
/// Represents a complete 1Password item with all details including sections, fields, and files.
/// </summary>
public class FullItem : Item
{
    /// <summary>
    /// Sections within the item.
    /// </summary>
    [JsonPropertyName("sections")]
    public List<Section>? Sections { get; set; }

    /// <summary>
    /// Fields within the item.
    /// </summary>
    [JsonPropertyName("fields")]
    public List<Field>? Fields { get; set; }

    /// <summary>
    /// Files attached to the item.
    /// </summary>
    [JsonPropertyName("files")]
    public List<File>? Files { get; set; }
}
