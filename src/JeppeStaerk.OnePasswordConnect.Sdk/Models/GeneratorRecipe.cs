using System.Collections.Generic;
using System.Text.Json.Serialization;
using JeppeStaerk.OnePasswordConnect.Sdk.Enums;

namespace JeppeStaerk.OnePasswordConnect.Sdk.Models;

/// <summary>
/// The recipe used in conjunction with the "generate" property to set the character set used to generate a new secure value.
/// </summary>
public class GeneratorRecipe
{
    /// <summary>
    /// Length of the generated value (1-64 characters).
    /// Default: 32
    /// </summary>
    [JsonPropertyName("length")]
    public int Length { get; set; } = 32;

    /// <summary>
    /// Character sets to use for generation.
    /// </summary>
    [JsonPropertyName("characterSets")]
    public List<CharacterSet>? CharacterSets { get; set; }

    /// <summary>
    /// List of all characters that should be excluded from generated passwords.
    /// </summary>
    [JsonPropertyName("excludeCharacters")]
    public string? ExcludeCharacters { get; set; }
}
