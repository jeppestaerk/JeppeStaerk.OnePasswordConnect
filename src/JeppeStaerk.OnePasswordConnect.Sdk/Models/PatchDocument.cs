using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace JeppeStaerk.OnePasswordConnect.Sdk.Models;

/// <summary>
/// Represents a JSON Patch operation (RFC6902).
/// </summary>
public class PatchOperation
{
    /// <summary>
    /// The operation to perform (add, remove, replace).
    /// </summary>
    /// <example>
    /// <code>
    /// new PatchOperation
    /// {
    ///     Op = PatchOperation.Replace,
    ///     Path = "/title",
    ///     Value = "New Title"
    /// }
    /// </code>
    /// </example>
    [JsonPropertyName("op")]
    public Enums.PatchOperation Op { get; set; }

    /// <summary>
    /// An RFC6901 JSON Pointer pointing to the Item document, an Item Attribute,
    /// an Item Field by Field ID, or an Item Field Attribute.
    /// Must start with a forward slash (/).
    /// </summary>
    /// <example>
    /// <para>"/title" - Points to the item's title</para>
    /// <para>"/favorite" - Points to the favorite flag</para>
    /// <para>"/fields/06gnn2b95example10q91512p5/label" - Points to a field's label</para>
    /// </example>
    [JsonPropertyName("path")]
    [Required(ErrorMessage = "Path is required for JSON Patch operations.")]
    [RegularExpression(@"^\/.*", ErrorMessage = "Path must start with a forward slash (/) as per RFC6901 JSON Pointer syntax.")]
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// The value for the operation.
    /// Required for 'add' and 'replace' operations, not used for 'remove' operations.
    /// </summary>
    /// <example>
    /// <code>
    /// // String value
    /// new PatchOperation { Op = PatchOperation.Replace, Path = "/title", Value = "New Title" }
    ///
    /// // Boolean value
    /// new PatchOperation { Op = PatchOperation.Replace, Path = "/favorite", Value = true }
    ///
    /// // Complex object
    /// new PatchOperation
    /// {
    ///     Op = PatchOperation.Add,
    ///     Path = "/fields",
    ///     Value = new Field { Label = "API Key", Type = FieldType.Concealed, Value = "secret" }
    /// }
    /// </code>
    /// </example>
    [JsonPropertyName("value")]
    public object? Value { get; set; }
}
