using System.Text.Json;
using System.Text.Json.Serialization;

namespace JeppeStaerk.OnePasswordConnect.Sdk.Tests;

/// <summary>
/// Shared test utilities for SDK tests.
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// JSON serialization options that match the SDK's options.
    /// Includes JsonStringEnumConverter so enums serialize as strings.
    /// </summary>
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };
}
