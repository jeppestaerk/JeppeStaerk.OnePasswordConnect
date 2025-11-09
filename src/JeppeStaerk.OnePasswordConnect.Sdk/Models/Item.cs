using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using JeppeStaerk.OnePasswordConnect.Sdk.Enums;

namespace JeppeStaerk.OnePasswordConnect.Sdk.Models;

/// <summary>
/// Represents a 1Password item (base properties).
/// </summary>
public class Item
{
    /// <summary>
    /// The item ID (26-character lowercase alphanumeric string).
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// The title of the item.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Reference to the vault containing this item.
    /// </summary>
    [JsonPropertyName("vault")]
    public VaultReference Vault { get; set; } = new();

    /// <summary>
    /// The category of the item.
    /// </summary>
    [JsonPropertyName("category")]
    public ItemCategory Category { get; set; }

    /// <summary>
    /// URLs associated with this item.
    /// </summary>
    [JsonPropertyName("urls")]
    public List<ItemUrl>? Urls { get; set; }

    /// <summary>
    /// Whether this item is marked as a favorite.
    /// </summary>
    [JsonPropertyName("favorite")]
    public bool? Favorite { get; set; }

    /// <summary>
    /// Tags associated with this item.
    /// </summary>
    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }

    /// <summary>
    /// The version number of the item.
    /// </summary>
    [JsonPropertyName("version")]
    public int? Version { get; set; }

    /// <summary>
    /// The state of the item (read-only).
    /// </summary>
    [JsonPropertyName("state")]
    public ItemState? State { get; set; }

    /// <summary>
    /// When the item was created (read-only).
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// When the item was last updated (read-only).
    /// </summary>
    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Who last edited the item (read-only).
    /// </summary>
    [JsonPropertyName("lastEditedBy")]
    public string? LastEditedBy { get; set; }
}
