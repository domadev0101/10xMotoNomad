using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MotoNomad.App.Application.DTOs.OpenRouter;
using MotoNomad.App.Application.Exceptions;
using MotoNomad.Application.Exceptions;
using MotoNomad.App.Application.Interfaces;
using MotoNomad.App.Infrastructure.Configuration;

namespace MotoNomad.App.Infrastructure.Services;

/// <summary>
/// Service for interacting with OpenRouter API to access various LLM models
/// </summary>
public class OpenRouterService : IOpenRouterService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenRouterService> _logger;
    private readonly OpenRouterSettings _settings;
    private readonly SemaphoreSlim _rateLimitSemaphore;
    private readonly JsonSerializerOptions _jsonOptions;
    private DateTime _lastRequestTime;
    private int _consecutiveErrors;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of OpenRouterService
    /// </summary>
    /// <param name="httpClient">HTTP client for API requests (injected via IHttpClientFactory)</param>
    /// <param name="settings">OpenRouter configuration settings</param>
    /// <param name="logger">Logger for diagnostics</param>
  public OpenRouterService(
   HttpClient httpClient,
        IOptions<OpenRouterSettings> settings,
        ILogger<OpenRouterService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

   // Sprawdź tryb działania: Edge Function proxy vs Direct API
        if (_settings.UseEdgeFunctionProxy)
      {
            // Tryb Edge Function Proxy (zalecany dla produkcji)
            if (string.IsNullOrWhiteSpace(_settings.EdgeFunctionUrl))
            {
      throw new InvalidOperationException(
          "EdgeFunctionUrl is required when UseEdgeFunctionProxy is true");
   }

     _logger.LogInformation(
        "OpenRouter configured to use Edge Function proxy at: {EdgeFunctionUrl}",
         _settings.EdgeFunctionUrl);

 // Ustaw URL Edge Function jako BaseAddress
  _httpClient.BaseAddress = new Uri(_settings.EdgeFunctionUrl.TrimEnd('/') + "/");
            
// Edge Function nie potrzebuje API key w nagłówkach (jest po stronie serwera)
            // Dodaj tylko dodatkowe nagłówki dla identyfikacji
            _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", _settings.HttpReferer);
            _httpClient.DefaultRequestHeaders.Add("X-Title", _settings.AppTitle);
        }
  else
        {
            // Tryb Direct API (dla developmentu/testów)
            // Validate settings
    if (string.IsNullOrWhiteSpace(_settings.ApiKey) ||
        _settings.ApiKey == "your-api-key-here" ||
                _settings.ApiKey.StartsWith("your-"))
     {
         _logger.LogWarning("OpenRouter API key is not configured properly. AI features will not work.");
     _logger.LogWarning("Please configure a valid API key in appsettings.json under OpenRouter:ApiKey");
              // Don't throw - allow app to run but AI features won't work
     }

      // Configure HTTP client
       var baseUrl = _settings.BaseUrl.TrimEnd('/') + "/";
            _httpClient.BaseAddress = new Uri(baseUrl);

            if (!string.IsNullOrWhiteSpace(_settings.ApiKey) &&
         _settings.ApiKey != "your-api-key-here" &&
        !_settings.ApiKey.StartsWith("your-"))
        {
          _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
      }

 _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", _settings.HttpReferer);
   _httpClient.DefaultRequestHeaders.Add("X-Title", _settings.AppTitle);
        }

        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);

        // Configure JSON options
        _jsonOptions = new JsonSerializerOptions
        {
         PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
          DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
   };

   // Initialize rate limiting
_rateLimitSemaphore = new SemaphoreSlim(_settings.MaxConcurrentRequests);
    _lastRequestTime = DateTime.MinValue;
        _consecutiveErrors = 0;
    }

    #region Public Methods

    /// <summary>
    /// Sends a chat completion request and returns the complete response
    /// </summary>
    public async Task<ChatCompletionResponse> SendChatCompletionAsync(
  ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
   // Check if API is configured (both modes)
        if (_settings.UseEdgeFunctionProxy)
   {
 if (string.IsNullOrWhiteSpace(_settings.EdgeFunctionUrl))
   {
    throw new OpenRouterAuthException(
     "Edge Function proxy URL not configured. Please set EdgeFunctionUrl in appsettings.json");
    }
        }
else
     {
 // Check if API key is configured for Direct API mode
    if (string.IsNullOrWhiteSpace(_settings.ApiKey) ||
_settings.ApiKey == "your-api-key-here" ||
      _settings.ApiKey.StartsWith("your-"))
    {
  throw new OpenRouterAuthException(
       "OpenRouter API key is not configured. Please add your API key to appsettings.json under OpenRouter:ApiKey. " +
         "Get your free API key at: https://openrouter.ai/keys");
    }
 }

        try
        {
  ValidateRequest(request);
      await ApplyRateLimitAsync(cancellationToken);

            return await RetryWithExponentialBackoffAsync(
     async () =>
  {
   _logger.LogInformation(
  "Sending chat completion request to model {Model} (via {Mode})",
      request.Model,
    _settings.UseEdgeFunctionProxy ? "Edge Function Proxy" : "Direct API");

  // Endpoint różni się w zależności od trybu
     var endpoint = _settings.UseEdgeFunctionProxy 
           ? "" // Edge Function oczekuje POST na root path
        : "chat/completions";

            // Edge Function mode: Add Supabase Anon Key as Authorization header (per request)
      HttpRequestMessage? httpRequest = null;
            if (_settings.UseEdgeFunctionProxy)
 {
             httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint);
       
                // Get Supabase Anon Key from configuration (hardcoded for now - later from ISupabaseClientService)
      var supabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InlzY2dyd2ZrdWlpY3FsZW1xemVtIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjE1ODI2MjEsImV4cCI6MjA3NzE1ODYyMX0.GZ1NA1DRqA-HSeB1ApXkxWoI9hHMBHOEW-Ak2EjaTHw";
httpRequest.Headers.Add("Authorization", $"Bearer {supabaseAnonKey}");
      httpRequest.Content = JsonContent.Create(request, options: _jsonOptions);
            }

  var response = httpRequest != null 
          ? await _httpClient.SendAsync(httpRequest, cancellationToken)
: await _httpClient.PostAsJsonAsync(endpoint, request, _jsonOptions, cancellationToken);

  if (!response.IsSuccessStatusCode)
      {
         await HandleHttpErrorAsync(response);
         }

 // Log response content type for debugging
    var contentType = response.Content.Headers.ContentType?.MediaType;
        _logger.LogDebug("Response Content-Type: {ContentType}", contentType);

          // Check if response is JSON
         if (contentType != null && !contentType.Contains("application/json"))
           {
  var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogError("Received non-JSON response: {Content}", rawContent.Substring(0, Math.Min(500, rawContent.Length)));
  throw new OpenRouterException($"Received non-JSON response (Content-Type: {contentType}). This usually indicates an authentication or configuration error.");
      }

        var result = await response.Content
 .ReadFromJsonAsync<ChatCompletionResponse>(_jsonOptions, cancellationToken);

             if (result == null)
  {
       throw new OpenRouterException("Received null response from API");
      }

      _logger.LogInformation(
     "Successfully received response: Tokens={TotalTokens}",
       result.Usage?.TotalTokens ?? 0);

  return result;
         },
     maxRetries: _settings.MaxRetries,
      cancellationToken);
 }
    catch (OperationCanceledException)
     {
      _logger.LogInformation("Request cancelled by user");
      throw;
        }
      catch (OpenRouterException)
   {
      throw;
}
        catch (Exception ex)
      {
    _logger.LogError(ex, "Unexpected error in SendChatCompletionAsync");
      throw new OpenRouterException("An unexpected error occurred", ex);
      }
    }

    /// <summary>
    /// Sends a chat completion request with structured JSON response
    /// </summary>
    public async Task<T> SendStructuredChatCompletionAsync<T>(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default) where T : class
    {
        if (request.ResponseFormat == null)
        {
            throw new ValidationException("ResponseFormat is required for structured completions");
        }

        var response = await SendChatCompletionAsync(request, cancellationToken);
        var content = response.GetContent();

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new OpenRouterResponseValidationException(
             "Received empty response content");
        }

        try
        {
            var result = JsonSerializer.Deserialize<T>(content, _jsonOptions);

            if (result == null)
            {
                throw new OpenRouterResponseValidationException(
       "Failed to deserialize response to requested type");
            }

            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize structured response");
            throw new OpenRouterResponseValidationException(
                "Response does not match expected JSON schema",
             request.ResponseFormat.JsonSchema.Schema.ToString(),
              content);
        }
    }

    /// <summary>
    /// Streams a chat completion response chunk by chunk
    /// </summary>
    public async IAsyncEnumerable<ChatCompletionChunk> StreamChatCompletionAsync(
        ChatCompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        // Force streaming mode
        var streamingRequest = request with { Stream = true };

        await ApplyRateLimitAsync(cancellationToken);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = JsonContent.Create(streamingRequest, options: _jsonOptions)
        };

        using var response = await _httpClient.SendAsync(
         httpRequest,
      HttpCompletionOption.ResponseHeadersRead,
     cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await HandleHttpErrorAsync(response);
        }

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var line = await reader.ReadLineAsync();
            var chunk = ParseStreamingChunk(line);

            if (chunk != null)
            {
                yield return chunk;
            }
        }
    }

    /// <summary>
    /// Validates an API key by making a test request
    /// </summary>
    public async Task<bool> ValidateApiKeyAsync(
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return false;

        try
        {
            using var testClient = new HttpClient();
            testClient.BaseAddress = new Uri(_settings.BaseUrl);
            testClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            testClient.Timeout = TimeSpan.FromSeconds(10);

            var testRequest = new ChatCompletionRequest
            {
                Model = "openai/gpt-3.5-turbo",
                Messages = new List<ChatMessage>
     {
        ChatMessage.User("test")
                },
                MaxTokens = 1
            };

            var response = await testClient.PostAsJsonAsync(
                           "chat/completions",
                    testRequest,
                           _jsonOptions,
                      cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets available models from OpenRouter
    /// </summary>
    public async Task<IEnumerable<ModelInfo>> GetAvailableModelsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("models", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                await HandleHttpErrorAsync(response);
            }

            var result = await response.Content
           .ReadFromJsonAsync<ModelsResponse>(_jsonOptions, cancellationToken);

            return result?.Data ?? Enumerable.Empty<ModelInfo>();
        }
        catch (OpenRouterException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching available models");
            throw new OpenRouterException("Failed to fetch available models", ex);
        }
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Validates a chat completion request before sending
    /// </summary>
    private void ValidateRequest(ChatCompletionRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.Model))
            throw new ValidationException("Model name is required");

        if (request.Messages == null || !request.Messages.Any())
            throw new ValidationException("At least one message is required");

        // Validate message roles
        var validRoles = new[] { "system", "user", "assistant" };
        foreach (var message in request.Messages)
        {
            if (!validRoles.Contains(message.Role))
                throw new ValidationException($"Invalid message role: {message.Role}");

            if (string.IsNullOrWhiteSpace(message.Content))
                throw new ValidationException("Message content cannot be empty");
        }

        // Validate parameters
        if (request.Temperature.HasValue && (request.Temperature < 0 || request.Temperature > 2))
            throw new ValidationException("Temperature must be between 0 and 2");

        if (request.MaxTokens.HasValue && request.MaxTokens <= 0)
            throw new ValidationException("MaxTokens must be positive");

        if (request.TopP.HasValue && (request.TopP < 0 || request.TopP > 1))
            throw new ValidationException("TopP must be between 0 and 1");

        // Validate JSON schema if provided
        if (request.ResponseFormat != null)
        {
            ValidateJsonSchema(request.ResponseFormat);
        }
    }

    /// <summary>
    /// Validates JSON schema definition
    /// </summary>
    private void ValidateJsonSchema(ResponseFormat responseFormat)
    {
        if (responseFormat.Type != "json_schema")
            throw new ValidationException("Response format type must be 'json_schema'");

        if (responseFormat.JsonSchema == null)
            throw new ValidationException("JsonSchema definition is required");

        if (string.IsNullOrWhiteSpace(responseFormat.JsonSchema.Name))
            throw new ValidationException("Schema name is required");

        // Validate schema name format
        if (!Regex.IsMatch(responseFormat.JsonSchema.Name, @"^[a-zA-Z0-9_-]+$"))
        {
            throw new ValidationException(
        "Schema name must contain only letters, numbers, underscores, and dashes");
        }

        if (responseFormat.JsonSchema.Schema == null)
            throw new ValidationException("Schema object is required");

        if (responseFormat.JsonSchema.Schema.Type != "object")
            throw new ValidationException("Root schema type must be 'object'");
    }

    /// <summary>
    /// Applies rate limiting before making requests
    /// </summary>
    private async Task ApplyRateLimitAsync(CancellationToken cancellationToken)
    {
        await _rateLimitSemaphore.WaitAsync(cancellationToken);

        try
        {
            var timeSinceLastRequest = DateTime.UtcNow - _lastRequestTime;
            var minimumDelay = TimeSpan.FromMilliseconds(_settings.MinRequestDelayMs);

            if (timeSinceLastRequest < minimumDelay)
            {
                var delayNeeded = minimumDelay - timeSinceLastRequest;
                await Task.Delay(delayNeeded, cancellationToken);
            }

            _lastRequestTime = DateTime.UtcNow;
        }
        finally
        {
            _rateLimitSemaphore.Release();
        }
    }

    /// <summary>
    /// Handles HTTP error responses from OpenRouter API
    /// </summary>
    private async Task HandleHttpErrorAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        var statusCode = (int)response.StatusCode;

        _logger.LogError(
         "OpenRouter API error: Status={StatusCode}, Content={Content}",
       statusCode,
            content);

        switch (statusCode)
        {
            case 401:
                throw new OpenRouterAuthException(
                            "Invalid API key. Please check your OpenRouter configuration.");

            case 429:
                var retryAfter = response.Headers.RetryAfter?.Delta;
                throw new OpenRouterRateLimitException(
               "Rate limit exceeded. Please wait before making more requests.",
     retryAfter);

            case 400:
                throw new ValidationException($"Invalid request: {content}");

            case 404:
                throw new OpenRouterModelNotFoundException(
                 "Model not found. Please check the model name and try again.");

            case 402:
                throw new OpenRouterInsufficientCreditsException(
                      "Insufficient credits in OpenRouter account. Please add credits to continue.");

            case 500:
            case 502:
            case 503:
                throw new OpenRouterServerException(
              "OpenRouter server error. Please try again later.");

            default:
                throw new OpenRouterException(
                   $"Unexpected error from OpenRouter: {statusCode} - {content}");
        }
    }

    /// <summary>
    /// Executes an operation with exponential backoff retry logic
    /// </summary>
    private async Task<T> RetryWithExponentialBackoffAsync<T>(
        Func<Task<T>> operation,
        int maxRetries,
      CancellationToken cancellationToken)
    {
        var attempt = 0;
        while (true)
        {
            try
            {
                var result = await operation();
                _consecutiveErrors = 0; // Reset error counter on success
                return result;
            }
            catch (Exception ex) when (
           ex is OpenRouterServerException ||
            ex is HttpRequestException ||
        ex is TaskCanceledException)
            {
                attempt++;
                _consecutiveErrors++;

                if (attempt >= maxRetries)
                {
                    _logger.LogError(
                       ex,
               "Operation failed after {Attempts} attempts",
                attempt);
                    throw;
                }

                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                _logger.LogWarning(
                   "Attempt {Attempt} failed, retrying in {Delay}s: {Error}",
                   attempt,
                   delay.TotalSeconds,
              ex.Message);

                await Task.Delay(delay, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Parses a single chunk from streaming response
    /// </summary>
    private ChatCompletionChunk? ParseStreamingChunk(string? line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return null;

        if (!line.StartsWith("data: "))
            return null;

        var data = line.Substring(6).Trim();

        if (data == "[DONE]")
            return null;

        try
        {
            return JsonSerializer.Deserialize<ChatCompletionChunk>(data, _jsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse streaming chunk: {Data}", data);
            return null;
        }
    }

    #endregion

    /// <summary>
    /// Disposes resources used by the service
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _rateLimitSemaphore?.Dispose();
            _disposed = true;
        }
    }
}
