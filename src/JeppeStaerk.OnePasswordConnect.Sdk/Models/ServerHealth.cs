using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace JeppeStaerk.OnePasswordConnect.Sdk.Models;

/// <summary>
/// Represents the health status of the 1Password Connect server.
/// </summary>
public class ServerHealth
{
    /// <summary>
    /// Name of the service.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Version of the Connect server.
    /// </summary>
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    /// <summary>
    /// Status of registered server dependencies.
    /// </summary>
    [JsonPropertyName("dependencies")]
    public List<ServiceDependency>? Dependencies { get; set; }
}

/// <summary>
/// Represents the state of a registered server dependency.
/// </summary>
public class ServiceDependency
{
    /// <summary>
    /// Name of the service.
    /// </summary>
    [JsonPropertyName("service")]
    public string? Service { get; set; }

    /// <summary>
    /// Status of the service.
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>
    /// Human-readable message explaining the current state.
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
