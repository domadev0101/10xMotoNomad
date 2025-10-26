using System.Text.Json.Serialization;

namespace MotoNomad.App.Application.DTOs.OpenRouter;

/// <summary>
/// Represents a single choice in the completion response
/// </summary>
public record ChatChoice
{
  /// <summary>
    /// Index of this choice in the list
    /// </summary>
    [JsonPropertyName("index")]
  public int Index { get; init; }

    /// <summary>
    /// The generated message
  /// </summary>
    [JsonPropertyName("message")]
    public ChatMessage? Message { get; init; }

    /// <summary>
    /// Reason why the model stopped generating (e.g., "stop", "length", "content_filter")
    /// </summary>
    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; init; }
}
