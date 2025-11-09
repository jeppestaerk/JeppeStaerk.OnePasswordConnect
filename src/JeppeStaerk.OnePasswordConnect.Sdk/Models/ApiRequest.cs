using System;
using System.Text.Json.Serialization;
using JeppeStaerk.OnePasswordConnect.Sdk.Enums;

namespace JeppeStaerk.OnePasswordConnect.Sdk.Models;

/// <summary>
/// Represents an API request that was made to the Connect server.
/// </summary>
public class ApiRequest
{
    /// <summary>
    /// The unique ID used to identify a single request.
    /// </summary>
    [JsonPropertyName("requestId")]
    public string? RequestId { get; set; }

    /// <summary>
    /// The time at which the request was processed by the server (read-only).
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime? Timestamp { get; set; }

    /// <summary>
    /// The action performed in the request.
    /// </summary>
    [JsonPropertyName("action")]
    public ApiRequestAction? Action { get; set; }

    /// <summary>
    /// The result of the request.
    /// </summary>
    [JsonPropertyName("result")]
    public ApiRequestResult? Result { get; set; }

    /// <summary>
    /// Information about who made the request.
    /// </summary>
    [JsonPropertyName("actor")]
    public Actor? Actor { get; set; }

    /// <summary>
    /// Information about the resource that was accessed.
    /// </summary>
    [JsonPropertyName("resource")]
    public Resource? Resource { get; set; }
}

/// <summary>
/// Represents the actor (user/token) that made an API request.
/// </summary>
public class Actor
{
    /// <summary>
    /// The actor ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// The account associated with the actor.
    /// </summary>
    [JsonPropertyName("account")]
    public string? Account { get; set; }

    /// <summary>
    /// JWT ID (jti claim).
    /// </summary>
    [JsonPropertyName("jti")]
    public string? Jti { get; set; }

    /// <summary>
    /// User agent of the request.
    /// </summary>
    [JsonPropertyName("userAgent")]
    public string? UserAgent { get; set; }

    /// <summary>
    /// IP address of the requester.
    /// </summary>
    [JsonPropertyName("requestIp")]
    public string? RequestIp { get; set; }
}

/// <summary>
/// Represents a resource that was accessed in an API request.
/// </summary>
public class Resource
{
    /// <summary>
    /// The type of resource.
    /// </summary>
    [JsonPropertyName("type")]
    public ResourceType? Type { get; set; }

    /// <summary>
    /// Reference to the vault (if applicable).
    /// </summary>
    [JsonPropertyName("vault")]
    public ResourceVaultReference? Vault { get; set; }

    /// <summary>
    /// Reference to the item (if applicable).
    /// </summary>
    [JsonPropertyName("item")]
    public ResourceItemReference? Item { get; set; }

    /// <summary>
    /// The item version (if applicable).
    /// </summary>
    [JsonPropertyName("itemVersion")]
    public int? ItemVersion { get; set; }
}

/// <summary>
/// Represents a vault reference in a resource.
/// </summary>
public class ResourceVaultReference
{
    /// <summary>
    /// The vault ID (26-character lowercase alphanumeric string).
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

/// <summary>
/// Represents an item reference in a resource.
/// </summary>
public class ResourceItemReference
{
    /// <summary>
    /// The item ID (26-character lowercase alphanumeric string).
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}
