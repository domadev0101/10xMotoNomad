using System.Text.Json.Serialization;

namespace MotoNomad.App.Application.DTOs.OpenRouter;

/// <summary>
/// JSON Schema object definition
/// </summary>
public record JsonSchemaObject
{
    /// <summary>
    /// Schema type (e.g., "object", "string", "integer", "array")
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    /// <summary>
  /// Object properties (for type: "object")
    /// </summary>
    [JsonPropertyName("properties")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, JsonSchemaProperty>? Properties { get; init; }

    /// <summary>
    /// Required property names (for type: "object")
    /// </summary>
    [JsonPropertyName("required")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Required { get; init; }

    /// <summary>
    /// Allow additional properties beyond defined ones
/// </summary>
    [JsonPropertyName("additionalProperties")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? AdditionalProperties { get; init; }

    /// <summary>
    /// Array items schema (for type: "array")
    /// </summary>
    [JsonPropertyName("items")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonSchemaProperty? Items { get; init; }

    /// <summary>
    /// Description of the schema
    /// </summary>
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; init; }
}
