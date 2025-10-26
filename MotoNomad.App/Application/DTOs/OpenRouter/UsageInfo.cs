using System.Text.Json.Serialization;

namespace MotoNomad.App.Application.DTOs.OpenRouter;

/// <summary>
/// Token usage information for a completion request
/// </summary>
public record UsageInfo
{
    /// <summary>
    /// Number of tokens in the prompt
    /// </summary>
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; init; }

    /// <summary>
    /// Number of tokens in the generated completion
    /// </summary>
    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; init; }

    /// <summary>
    /// Total number of tokens used (prompt + completion)
    /// </summary>
 [JsonPropertyName("total_tokens")]
  public int TotalTokens { get; init; }
}
