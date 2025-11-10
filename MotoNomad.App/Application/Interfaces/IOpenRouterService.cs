using MotoNomad.App.Application.DTOs.OpenRouter;
using MotoNomad.App.Application.Exceptions;

namespace MotoNomad.App.Application.Interfaces;

/// <summary>
/// Service for interacting with OpenRouter API to access various LLM models
/// </summary>
public interface IOpenRouterService
{
    /// <summary>
    /// Sends a chat completion request and returns the complete response
    /// </summary>
    /// <param name="request">The chat completion request configuration</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Complete chat completion response</returns>
    /// <exception cref="OpenRouterAuthException">Invalid API key</exception>
    /// <exception cref="OpenRouterRateLimitException">Rate limit exceeded</exception>
    /// <exception cref="ValidationException">Invalid request parameters</exception>
    /// <exception cref="OpenRouterModelNotFoundException">Specified model not found</exception>
    /// <exception cref="OpenRouterInsufficientCreditsException">Insufficient credits</exception>
    /// <exception cref="OpenRouterServerException">Server error occurred</exception>
    Task<ChatCompletionResponse> SendChatCompletionAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a chat completion request with structured JSON response
    /// </summary>
    /// <typeparam name="T">The expected response type</typeparam>
    /// <param name="request">The chat completion request with JSON schema</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Deserialized structured response</returns>
    /// <exception cref="OpenRouterAuthException">Invalid API key</exception>
    /// <exception cref="OpenRouterRateLimitException">Rate limit exceeded</exception>
    /// <exception cref="ValidationException">Invalid request parameters</exception>
    /// <exception cref="OpenRouterResponseValidationException">Response doesn't match schema</exception>
    Task<T> SendStructuredChatCompletionAsync<T>(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Streams a chat completion response chunk by chunk
    /// </summary>
    /// <param name="request">The chat completion request configuration</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Async enumerable of response chunks</returns>
    /// <exception cref="OpenRouterAuthException">Invalid API key</exception>
    /// <exception cref="OpenRouterRateLimitException">Rate limit exceeded</exception>
    /// <exception cref="ValidationException">Invalid request parameters</exception>
    IAsyncEnumerable<ChatCompletionChunk> StreamChatCompletionAsync(
        ChatCompletionRequest request,
  CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates an API key by making a test request
    /// </summary>
    /// <param name="apiKey">The API key to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if valid, false otherwise</returns>
    Task<bool> ValidateApiKeyAsync(
        string apiKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available models from OpenRouter
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of available models</returns>
    /// <exception cref="OpenRouterAuthException">Invalid API key</exception>
    /// <exception cref="OpenRouterException">Failed to fetch models</exception>
    Task<IEnumerable<ModelInfo>> GetAvailableModelsAsync(
   CancellationToken cancellationToken = default);
}
