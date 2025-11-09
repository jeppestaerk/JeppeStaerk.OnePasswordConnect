using System.Text.Json.Serialization;
using JeppeStaerk.OnePasswordConnect.Sdk.Enums;

namespace JeppeStaerk.OnePasswordConnect.Sdk.Models;

/// <summary>
/// Represents a field within a 1Password item.
/// </summary>
public class Field
{
    /// <summary>
    /// Unique identifier for the field.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The section this field belongs to.
    /// </summary>
    [JsonPropertyName("section")]
    public Section? Section { get; set; }

    /// <summary>
    /// The type of the field.
    /// </summary>
    [JsonPropertyName("type")]
    public FieldType Type { get; set; } = FieldType.STRING;

    /// <summary>
    /// The purpose of the field (used for autofill).
    /// </summary>
    [JsonPropertyName("purpose")]
    public FieldPurpose? Purpose { get; set; }

    /// <summary>
    /// The label/name of the field.
    /// </summary>
    [JsonPropertyName("label")]
    public string? Label { get; set; }

    /// <summary>
    /// The value of the field.
    /// </summary>
    [JsonPropertyName("value")]
    public string? Value { get; set; }

    /// <summary>
    /// If true and value is not present, a new value should be generated for this field.
    /// </summary>
    [JsonPropertyName("generate")]
    public bool? Generate { get; set; }

    /// <summary>
    /// Recipe for generating field values.
    /// </summary>
    [JsonPropertyName("recipe")]
    public GeneratorRecipe? Recipe { get; set; }

    /// <summary>
    /// For fields with a purpose of PASSWORD, this is the entropy of the value (read-only).
    /// </summary>
    [JsonPropertyName("entropy")]
    public double? Entropy { get; set; }
}
