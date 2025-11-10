using System.Text.Json;
using System.Text.Json.Serialization;

namespace MotoNomad.App.Application.DTOs.OpenRouter;

/// <summary>
/// Response from chat completion request
/// </summary>
public record ChatCompletionResponse
{
    /// <summary>
    /// Unique identifier for this completion
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// Model used for this completion
    /// </summary>
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    /// <summary>
    /// List of completion choices
    /// </summary>
    [JsonPropertyName("choices")]
    public required List<ChatChoice> Choices { get; init; }

    /// <summary>
    /// Token usage information
    /// </summary>
    [JsonPropertyName("usage")]
    public UsageInfo? Usage { get; init; }

    /// <summary>
    /// Unix timestamp of when the completion was created
    /// </summary>
    [JsonPropertyName("created")]
    public long Created { get; init; }

    /// <summary>
    /// Gets the first choice content (most common use case)
    /// </summary>
    /// <returns>Content from the first choice, or empty string if no choices</returns>
    public string GetContent() => Choices.FirstOrDefault()?.Message?.Content ?? string.Empty;

    /// <summary>
    /// Deserializes structured JSON response
    /// </summary>
    /// <typeparam name="T">Target type for deserialization</typeparam>
    /// <returns>Deserialized object, or null if content is empty or deserialization fails</returns>
    public T? GetStructuredContent<T>() where T : class
    {
        var content = GetContent();
        if (string.IsNullOrWhiteSpace(content))
            return null;

        return JsonSerializer.Deserialize<T>(content);
    }
}
