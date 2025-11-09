using System.Text.Json.Serialization;

namespace JeppeStaerk.OnePasswordConnect.Sdk.Models;

/// <summary>
/// Represents a file attached to a 1Password item.
/// </summary>
public class File
{
    /// <summary>
    /// ID of the file.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Name of the file.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Size in bytes of the file.
    /// </summary>
    [JsonPropertyName("size")]
    public int? Size { get; set; }

    /// <summary>
    /// Path of the Connect API that can be used to download the contents of this file (read-only).
    /// </summary>
    [JsonPropertyName("content_path")]
    public string? ContentPath { get; set; }

    /// <summary>
    /// For files that are in a section, this field describes the section.
    /// </summary>
    [JsonPropertyName("section")]
    public Section? Section { get; set; }

    /// <summary>
    /// Base64-encoded contents of the file.
    /// Only set if size is small enough and inline_files is set to true.
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }
}
