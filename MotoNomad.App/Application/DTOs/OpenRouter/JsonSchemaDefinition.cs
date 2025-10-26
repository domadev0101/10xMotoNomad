using System.Text.Json.Serialization;

namespace MotoNomad.App.Application.DTOs.OpenRouter;

/// <summary>
/// JSON Schema definition wrapper
/// </summary>
public record JsonSchemaDefinition
{
    /// <summary>
    /// Schema name (must be a-z, A-Z, 0-9, underscores and dashes)
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Enable strict schema validation (recommended: true)
    /// </summary>
 [JsonPropertyName("strict")]
 public required bool Strict { get; init; }

  /// <summary>
    /// The JSON Schema object
    /// </summary>
 [JsonPropertyName("schema")]
    public required JsonSchemaObject Schema { get; init; }
}
