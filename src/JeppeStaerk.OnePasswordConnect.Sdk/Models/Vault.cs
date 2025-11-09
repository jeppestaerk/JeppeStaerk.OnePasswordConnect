using System;
using System.Text.Json.Serialization;
using JeppeStaerk.OnePasswordConnect.Sdk.Enums;

namespace JeppeStaerk.OnePasswordConnect.Sdk.Models;

/// <summary>
/// Represents a 1Password vault.
/// </summary>
public class Vault
{
    /// <summary>
    /// The vault ID (26-character lowercase alphanumeric string).
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// The name of the vault.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Description of the vault.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// The vault version.
    /// </summary>
    [JsonPropertyName("attributeVersion")]
    public int? AttributeVersion { get; set; }

    /// <summary>
    /// The version of the vault contents.
    /// </summary>
    [JsonPropertyName("contentVersion")]
    public int? ContentVersion { get; set; }

    /// <summary>
    /// Number of active items in the vault.
    /// </summary>
    [JsonPropertyName("items")]
    public int? Items { get; set; }

    /// <summary>
    /// The type of vault.
    /// </summary>
    [JsonPropertyName("type")]
    public VaultType? Type { get; set; }

    /// <summary>
    /// When the vault was created (read-only).
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// When the vault was last updated (read-only).
    /// </summary>
    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }
}
