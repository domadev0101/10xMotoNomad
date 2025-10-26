using System.Text.Json.Serialization;

namespace MotoNomad.App.Application.DTOs.OpenRouter;

/// <summary>
/// JSON Schema property definition
/// </summary>
public record JsonSchemaProperty
{
    /// <summary>
    /// Property type (e.g., "string", "integer", "boolean", "array", "object")
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    /// <summary>
    /// Property description
    /// </summary>
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; init; }

    /// <summary>
    /// Array items schema (for type: "array")
    /// </summary>
    [JsonPropertyName("items")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
  public JsonSchemaProperty? Items { get; init; }

    /// <summary>
    /// Enum values for restricted choices
    /// </summary>
    [JsonPropertyName("enum")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Enum { get; init; }

    /// <summary>
    /// Minimum value for numeric types
    /// </summary>
    [JsonPropertyName("minimum")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Minimum { get; init; }

    /// <summary>
    /// Maximum value for numeric types
    /// </summary>
    [JsonPropertyName("maximum")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Maximum { get; init; }
}
