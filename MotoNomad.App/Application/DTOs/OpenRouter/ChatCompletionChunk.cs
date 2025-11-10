using System.Text.Json.Serialization;

namespace MotoNomad.App.Application.DTOs.OpenRouter;

/// <summary>
/// Represents a chunk of streaming response
/// </summary>
public record ChatCompletionChunk
{
    /// <summary>
    /// Unique identifier for this chunk
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// Model used for this completion
    /// </summary>
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    /// <summary>
    /// List of streaming choices
    /// </summary>
    [JsonPropertyName("choices")]
    public required List<StreamingChoice> Choices { get; init; }

    /// <summary>
    /// Unix timestamp of when the chunk was created
    /// </summary>
    [JsonPropertyName("created")]
    public long Created { get; init; }
}

/// <summary>
/// Represents a single streaming choice
/// </summary>
public record StreamingChoice
{
    /// <summary>
    /// Index of this choice
    /// </summary>
    [JsonPropertyName("index")]
    public int Index { get; init; }

    /// <summary>
    /// Delta content for this chunk
    /// </summary>
    [JsonPropertyName("delta")]
    public DeltaMessage? Delta { get; init; }

    /// <summary>
    /// Reason why streaming stopped (null while streaming, set on last chunk)
    /// </summary>
    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; init; }
}

/// <summary>
/// Represents a delta message in streaming
/// </summary>
public record DeltaMessage
{
    /// <summary>
    /// Role (only present in first chunk)
    /// </summary>
    [JsonPropertyName("role")]
    public string? Role { get; init; }

    /// <summary>
    /// Content delta for this chunk
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; init; }
}
