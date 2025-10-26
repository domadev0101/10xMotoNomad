using System.Text.Json.Serialization;

namespace MotoNomad.App.Application.DTOs.OpenRouter;

/// <summary>
/// Information about an available AI model
/// </summary>
public record ModelInfo
{
    /// <summary>
    /// Model identifier (e.g., "anthropic/claude-3.5-sonnet")
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// Human-readable model name
  /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    /// Model description
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// Model context length (maximum tokens)
    /// </summary>
    [JsonPropertyName("context_length")]
    public int? ContextLength { get; init; }

    /// <summary>
    /// Pricing information
    /// </summary>
    [JsonPropertyName("pricing")]
    public ModelPricing? Pricing { get; init; }

    /// <summary>
    /// Model capabilities and features
    /// </summary>
    [JsonPropertyName("top_provider")]
    public TopProvider? TopProvider { get; init; }
}

/// <summary>
/// Model pricing information
/// </summary>
public record ModelPricing
{
    /// <summary>
    /// Cost per prompt token (in USD)
    /// </summary>
    [JsonPropertyName("prompt")]
    public string? Prompt { get; init; }

    /// <summary>
  /// Cost per completion token (in USD)
    /// </summary>
    [JsonPropertyName("completion")]
    public string? Completion { get; init; }
}

/// <summary>
/// Top provider information for the model
/// </summary>
public record TopProvider
{
    /// <summary>
    /// Maximum number of concurrent requests
    /// </summary>
    [JsonPropertyName("max_completion_tokens")]
    public int? MaxCompletionTokens { get; init; }

    /// <summary>
    /// Whether the model is currently available
    /// </summary>
 [JsonPropertyName("is_moderated")]
    public bool? IsModerated { get; init; }
}

/// <summary>
/// Response containing list of available models
/// </summary>
public record ModelsResponse
{
/// <summary>
    /// List of available models
    /// </summary>
    [JsonPropertyName("data")]
    public List<ModelInfo>? Data { get; init; }
}
