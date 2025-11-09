using System.Text.Json.Serialization;

namespace JeppeStaerk.OnePasswordConnect.Sdk.Models;

/// <summary>
/// Represents a reference to a vault.
/// </summary>
public class VaultReference
{
    /// <summary>
    /// The vault ID (26-character lowercase alphanumeric string).
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}
