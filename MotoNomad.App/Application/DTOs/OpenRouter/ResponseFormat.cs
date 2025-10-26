using System.Text.Json.Serialization;

namespace MotoNomad.App.Application.DTOs.OpenRouter;

/// <summary>
/// Specifies the format for structured responses
/// </summary>
public record ResponseFormat
{
    /// <summary>
    /// Format type: "json_schema"
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    /// <summary>
    /// JSON Schema definition for structured output
    /// </summary>
    [JsonPropertyName("json_schema")]
    public required JsonSchemaDefinition JsonSchema { get; init; }

    /// <summary>
    /// Creates a JSON schema response format
    /// </summary>
    /// <param name="schemaName">Name of the schema (alphanumeric, underscores, dashes only)</param>
    /// <param name="schema">JSON Schema object definition</param>
    /// <returns>Response format with JSON schema</returns>
    public static ResponseFormat CreateJsonSchema(string schemaName, JsonSchemaObject schema) => new()
    {
        Type = "json_schema",
        JsonSchema = new JsonSchemaDefinition
        {
            Name = schemaName,
            Strict = true,
            Schema = schema
        }
    };
}
