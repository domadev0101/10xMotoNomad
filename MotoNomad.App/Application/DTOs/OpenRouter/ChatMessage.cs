using System.Text.Json.Serialization;

namespace MotoNomad.App.Application.DTOs.OpenRouter;

/// <summary>
/// Represents a single message in a conversation
/// </summary>
public record ChatMessage
{
    /// <summary>
  /// Message role: "system", "user", or "assistant"
    /// </summary>
    [JsonPropertyName("role")]
    public required string Role { get; init; }

    /// <summary>
    /// Message content (text or structured content)
    /// </summary>
  [JsonPropertyName("content")]
    public required string Content { get; init; }

    /// <summary>
    /// Creates a system message
    /// </summary>
    /// <param name="content">System message content</param>
    /// <returns>System message</returns>
    public static ChatMessage System(string content) => new()
    {
        Role = "system",
     Content = content
    };

    /// <summary>
    /// Creates a user message
    /// </summary>
    /// <param name="content">User message content</param>
    /// <returns>User message</returns>
    public static ChatMessage User(string content) => new()
    {
   Role = "user",
        Content = content
    };

    /// <summary>
    /// Creates an assistant message
    /// </summary>
    /// <param name="content">Assistant message content</param>
    /// <returns>Assistant message</returns>
    public static ChatMessage Assistant(string content) => new()
    {
        Role = "assistant",
        Content = content
    };
}
