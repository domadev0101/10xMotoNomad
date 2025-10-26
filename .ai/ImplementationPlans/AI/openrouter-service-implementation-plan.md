# OpenRouter Service Implementation Plan

**Project:** MotoNomad MVP  
**Date:** October 2025  
**Status:** Ready for Implementation  
**Purpose:** AI-powered travel planning features using OpenRouter API

---

## 1. Service Description

### Overview

The OpenRouter service provides a clean, type-safe interface for integrating Large Language Models (LLMs) into MotoNomad. OpenRouter is an API gateway that provides unified access to multiple AI models (Claude, GPT-4, etc.) through a single interface.

### Key Capabilities

- **Chat Completions**: Send messages and receive AI-generated responses
- **Structured Outputs**: Request responses in specific JSON formats using JSON Schema
- **Model Selection**: Choose from various AI models based on capabilities and cost
- **Streaming Support**: Handle real-time streaming responses for better UX
- **Error Handling**: Robust error handling with typed exceptions
- **Rate Limiting**: Built-in rate limit handling with exponential backoff

### Architecture Position

```
Presentation Layer (Blazor Pages/Components)
    ↓
Application Layer (IOpenRouterService interface)
    ↓
Infrastructure Layer (OpenRouterService implementation)
    ↓
OpenRouter API Gateway
    ↓
AI Models (Claude, GPT-4, etc.)
```

### Use Cases in MotoNomad

1. **Trip Planning Suggestions**: Generate route recommendations based on user preferences
2. **Companion Matching**: Suggest compatible travel companions based on trip details
3. **Destination Information**: Provide insights about destinations, attractions, weather
4. **Itinerary Generation**: Create detailed day-by-day trip itineraries
5. **Budget Estimation**: Estimate costs based on trip parameters

---

## 2. Constructor Description

### IOpenRouterService Interface

Located in: `MotoNomad.App/Application/Interfaces/IOpenRouterService.cs`

```csharp
namespace MotoNomad.Application.Interfaces;

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
    Task<T> SendStructuredChatCompletionAsync<T>(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Streams a chat completion response chunk by chunk
    /// </summary>
    /// <param name="request">The chat completion request configuration</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Async enumerable of response chunks</returns>
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
    Task<IEnumerable<ModelInfo>> GetAvailableModelsAsync(
        CancellationToken cancellationToken = default);
}
```

### OpenRouterService Implementation

Located in: `MotoNomad.App/Infrastructure/Services/OpenRouterService.cs`

```csharp
namespace MotoNomad.Infrastructure.Services;

public class OpenRouterService : IOpenRouterService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenRouterService> _logger;
    private readonly OpenRouterSettings _settings;
    private readonly SemaphoreSlim _rateLimitSemaphore;
    private DateTime _lastRequestTime;
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
        
        // Validate settings
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new InvalidOperationException("OpenRouter API key is not configured");
        }

        // Configure HTTP client
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
        _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", _settings.HttpReferer);
        _httpClient.DefaultRequestHeaders.Add("X-Title", _settings.AppTitle);
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);

        // Initialize rate limiting
        _rateLimitSemaphore = new SemaphoreSlim(_settings.MaxConcurrentRequests);
        _lastRequestTime = DateTime.MinValue;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _rateLimitSemaphore?.Dispose();
            _disposed = true;
        }
    }
}
```

---

## 3. Public Methods and Fields

### ChatCompletionRequest Class

Located in: `MotoNomad.App/Application/DTOs/OpenRouter/ChatCompletionRequest.cs`

```csharp
namespace MotoNomad.Application.DTOs.OpenRouter;

/// <summary>
/// Request configuration for chat completions
/// </summary>
public record ChatCompletionRequest
{
    /// <summary>
    /// Model identifier (e.g., "anthropic/claude-3.5-sonnet")
    /// </summary>
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    /// <summary>
    /// List of messages in the conversation
    /// </summary>
    [JsonPropertyName("messages")]
    public required List<ChatMessage> Messages { get; init; }

    /// <summary>
    /// Optional response format for structured outputs
    /// </summary>
    [JsonPropertyName("response_format")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ResponseFormat? ResponseFormat { get; init; }

    /// <summary>
    /// Sampling temperature (0.0 to 2.0). Higher = more random.
    /// </summary>
    [JsonPropertyName("temperature")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Temperature { get; init; }

    /// <summary>
    /// Maximum tokens to generate in response
    /// </summary>
    [JsonPropertyName("max_tokens")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MaxTokens { get; init; }

    /// <summary>
    /// Top-p sampling parameter (0.0 to 1.0)
    /// </summary>
    [JsonPropertyName("top_p")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? TopP { get; init; }

    /// <summary>
    /// Frequency penalty (-2.0 to 2.0)
    /// </summary>
    [JsonPropertyName("frequency_penalty")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? FrequencyPenalty { get; init; }

    /// <summary>
    /// Presence penalty (-2.0 to 2.0)
    /// </summary>
    [JsonPropertyName("presence_penalty")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? PresencePenalty { get; init; }

    /// <summary>
    /// Enable streaming responses
    /// </summary>
    [JsonPropertyName("stream")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Stream { get; init; }

    /// <summary>
    /// OpenRouter-specific routing strategy
    /// </summary>
    [JsonPropertyName("route")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Route { get; init; }
}
```

### ChatMessage Class

```csharp
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
    public static ChatMessage System(string content) => new()
    {
        Role = "system",
        Content = content
    };

    /// <summary>
    /// Creates a user message
    /// </summary>
    public static ChatMessage User(string content) => new()
    {
        Role = "user",
        Content = content
    };

    /// <summary>
    /// Creates an assistant message
    /// </summary>
    public static ChatMessage Assistant(string content) => new()
    {
        Role = "assistant",
        Content = content
    };
}
```

### ResponseFormat Class

```csharp
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
    public static ResponseFormat JsonSchema(string schemaName, JsonSchemaObject schema) => new()
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
```

### JsonSchemaDefinition Class

```csharp
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
```

### JsonSchemaObject Class

```csharp
/// <summary>
/// JSON Schema object definition
/// </summary>
public record JsonSchemaObject
{
    /// <summary>
    /// Schema type (e.g., "object", "string", "integer", "array")
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    /// <summary>
    /// Object properties (for type: "object")
    /// </summary>
    [JsonPropertyName("properties")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, JsonSchemaProperty>? Properties { get; init; }

    /// <summary>
    /// Required property names (for type: "object")
    /// </summary>
    [JsonPropertyName("required")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Required { get; init; }

    /// <summary>
    /// Allow additional properties beyond defined ones
    /// </summary>
    [JsonPropertyName("additionalProperties")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? AdditionalProperties { get; init; }

    /// <summary>
    /// Array items schema (for type: "array")
    /// </summary>
    [JsonPropertyName("items")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonSchemaProperty? Items { get; init; }

    /// <summary>
    /// Description of the schema
    /// </summary>
    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; init; }
}
```

### JsonSchemaProperty Class

```csharp
/// <summary>
/// JSON Schema property definition
/// </summary>
public record JsonSchemaProperty
{
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("description")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; init; }

    [JsonPropertyName("items")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonSchemaProperty? Items { get; init; }

    [JsonPropertyName("enum")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Enum { get; init; }

    [JsonPropertyName("minimum")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Minimum { get; init; }

    [JsonPropertyName("maximum")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Maximum { get; init; }
}
```

### ChatCompletionResponse Class

```csharp
/// <summary>
/// Response from chat completion request
/// </summary>
public record ChatCompletionResponse
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("model")]
    public required string Model { get; init; }

    [JsonPropertyName("choices")]
    public required List<ChatChoice> Choices { get; init; }

    [JsonPropertyName("usage")]
    public UsageInfo? Usage { get; init; }

    [JsonPropertyName("created")]
    public long Created { get; init; }

    /// <summary>
    /// Gets the first choice content (most common use case)
    /// </summary>
    public string GetContent() => Choices.FirstOrDefault()?.Message?.Content ?? string.Empty;

    /// <summary>
    /// Deserializes structured JSON response
    /// </summary>
    public T? GetStructuredContent<T>() where T : class
    {
        var content = GetContent();
        if (string.IsNullOrWhiteSpace(content))
            return null;

        return JsonSerializer.Deserialize<T>(content);
    }
}
```

### ChatChoice Class

```csharp
public record ChatChoice
{
    [JsonPropertyName("index")]
    public int Index { get; init; }

    [JsonPropertyName("message")]
    public ChatMessage? Message { get; init; }

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; init; }
}
```

### UsageInfo Class

```csharp
public record UsageInfo
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; init; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; init; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; init; }
}
```

---

## 4. Private Methods and Fields

### Private Fields

```csharp
private readonly HttpClient _httpClient;
private readonly ILogger<OpenRouterService> _logger;
private readonly OpenRouterSettings _settings;
private readonly SemaphoreSlim _rateLimitSemaphore;
private readonly JsonSerializerOptions _jsonOptions;
private DateTime _lastRequestTime;
private int _consecutiveErrors;
private bool _disposed;
```

### Private Helper Methods

#### ValidateRequest

```csharp
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
```

#### ValidateJsonSchema

```csharp
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
    if (!System.Text.RegularExpressions.Regex.IsMatch(
        responseFormat.JsonSchema.Name, 
        @"^[a-zA-Z0-9_-]+$"))
    {
        throw new ValidationException(
            "Schema name must contain only letters, numbers, underscores, and dashes");
    }

    if (responseFormat.JsonSchema.Schema == null)
        throw new ValidationException("Schema object is required");

    if (responseFormat.JsonSchema.Schema.Type != "object")
        throw new ValidationException("Root schema type must be 'object'");
}
```

#### ApplyRateLimit

```csharp
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
```

#### HandleHttpError

```csharp
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
            throw new ValidationException(
                $"Invalid request: {content}");

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
```

#### RetryWithExponentialBackoff

```csharp
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
```

#### ParseStreamingChunk

```csharp
/// <summary>
/// Parses a single chunk from streaming response
/// </summary>
private ChatCompletionChunk? ParseStreamingChunk(string line)
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
```

---

## 5. Error Handling

### Custom Exception Hierarchy

Located in: `MotoNomad.App/Application/Exceptions/`

#### Base Exception

```csharp
/// <summary>
/// Base exception for all OpenRouter-related errors
/// </summary>
public class OpenRouterException : Exception
{
    public OpenRouterException(string message) : base(message) { }
    
    public OpenRouterException(string message, Exception innerException)
        : base(message, innerException) { }
}
```

#### Specific Exceptions

```csharp
/// <summary>
/// Thrown when API authentication fails
/// </summary>
public class OpenRouterAuthException : OpenRouterException
{
    public OpenRouterAuthException(string message) : base(message) { }
}

/// <summary>
/// Thrown when rate limit is exceeded
/// </summary>
public class OpenRouterRateLimitException : OpenRouterException
{
    public TimeSpan? RetryAfter { get; }

    public OpenRouterRateLimitException(string message, TimeSpan? retryAfter = null)
        : base(message)
    {
        RetryAfter = retryAfter;
    }
}

/// <summary>
/// Thrown when specified model is not found
/// </summary>
public class OpenRouterModelNotFoundException : OpenRouterException
{
    public OpenRouterModelNotFoundException(string message) : base(message) { }
}

/// <summary>
/// Thrown when account has insufficient credits
/// </summary>
public class OpenRouterInsufficientCreditsException : OpenRouterException
{
    public OpenRouterInsufficientCreditsException(string message) : base(message) { }
}

/// <summary>
/// Thrown when OpenRouter API experiences server errors
/// </summary>
public class OpenRouterServerException : OpenRouterException
{
    public OpenRouterServerException(string message) : base(message) { }
}

/// <summary>
/// Thrown when response validation fails
/// </summary>
public class OpenRouterResponseValidationException : OpenRouterException
{
    public string? ExpectedSchema { get; }
    public string? ActualResponse { get; }

    public OpenRouterResponseValidationException(
        string message,
        string? expectedSchema = null,
        string? actualResponse = null)
        : base(message)
    {
        ExpectedSchema = expectedSchema;
        ActualResponse = actualResponse;
    }
}
```

### Error Handling Patterns

#### In Service Methods

```csharp
public async Task<ChatCompletionResponse> SendChatCompletionAsync(
    ChatCompletionRequest request,
    CancellationToken cancellationToken = default)
{
    try
    {
        // Validate request
        ValidateRequest(request);

        // Apply rate limiting
        await ApplyRateLimitAsync(cancellationToken);

        // Send request with retry logic
        return await RetryWithExponentialBackoffAsync(
            async () =>
            {
                var response = await _httpClient.PostAsJsonAsync(
                    "chat/completions",
                    request,
                    _jsonOptions,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    await HandleHttpErrorAsync(response);
                }

                var result = await response.Content
                    .ReadFromJsonAsync<ChatCompletionResponse>(_jsonOptions, cancellationToken);

                if (result == null)
                {
                    throw new OpenRouterException("Received null response from API");
                }

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
        // Re-throw OpenRouter-specific exceptions
        throw;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error in SendChatCompletionAsync");
        throw new OpenRouterException("An unexpected error occurred", ex);
    }
}
```

#### In UI Components

```csharp
// Example usage in Blazor component
try
{
    var request = new ChatCompletionRequest
    {
        Model = "anthropic/claude-3.5-sonnet",
        Messages = new List<ChatMessage>
        {
            ChatMessage.System("You are a travel planning assistant."),
            ChatMessage.User("Suggest a 7-day motorcycle trip in Europe.")
        },
        Temperature = 0.7,
        MaxTokens = 2000
    };

    var response = await OpenRouterService.SendChatCompletionAsync(request);
    var suggestion = response.GetContent();
    
    // Display suggestion to user
    Snackbar.Add("Trip suggestion generated!", Severity.Success);
}
catch (OpenRouterAuthException ex)
{
    Snackbar.Add("Authentication error: Please check API key configuration.", Severity.Error);
    Logger.LogError(ex, "OpenRouter authentication failed");
}
catch (OpenRouterRateLimitException ex)
{
    var retryMessage = ex.RetryAfter.HasValue
        ? $"Please wait {ex.RetryAfter.Value.TotalSeconds:F0} seconds."
        : "Please try again later.";
    
    Snackbar.Add($"Rate limit exceeded. {retryMessage}", Severity.Warning);
}
catch (ValidationException ex)
{
    Snackbar.Add($"Invalid request: {ex.Message}", Severity.Warning);
}
catch (OpenRouterException ex)
{
    Snackbar.Add("An error occurred while generating suggestions.", Severity.Error);
    Logger.LogError(ex, "OpenRouter service error");
}
```

---

## 6. Security Considerations

### API Key Management

**Configuration Storage:**

```json
// appsettings.json (DO NOT COMMIT WITH REAL KEYS)
{
  "OpenRouter": {
    "ApiKey": "your-api-key-here",
    "BaseUrl": "https://openrouter.ai/api/v1",
    "HttpReferer": "https://yourdomain.github.io/MotoNomad",
    "AppTitle": "MotoNomad - Travel Planning App",
    "TimeoutSeconds": 60,
    "MaxRetries": 3,
    "MaxConcurrentRequests": 5,
    "MinRequestDelayMs": 100
  }
}
```

**Environment-Specific Configuration:**

```json
// appsettings.Development.json
{
  "OpenRouter": {
    "ApiKey": "sk-or-v1-dev-key-here"
  }
}

// appsettings.Production.json
{
  "OpenRouter": {
    "ApiKey": "sk-or-v1-prod-key-here"
  }
}
```

**GitHub Actions Secrets:**

Store production API key in GitHub Secrets and inject during deployment:

```yaml
# .github/workflows/deploy.yml
- name: Replace API key
  run: |
    sed -i 's/your-api-key-here/${{ secrets.OPENROUTER_API_KEY }}/g' release/wwwroot/appsettings.json
```

### Security Best Practices

1. **Never Commit Real API Keys**
   - Add `appsettings.*.json` to `.gitignore` (except `appsettings.json` template)
   - Use placeholder values in committed files
   - Document configuration requirements in README

2. **Client-Side Security Limitations**
   - OpenRouter API key will be visible in browser network tab
   - This is acceptable for Blazor WebAssembly as API key has limited scope
   - Consider implementing server-side proxy for production if stricter security needed

3. **Rate Limiting**
   - Implement client-side rate limiting to prevent abuse
   - Use `SemaphoreSlim` to limit concurrent requests
   - Add minimum delay between requests

4. **Input Validation**
   - Validate all user inputs before sending to API
   - Sanitize user-provided content
   - Prevent prompt injection attacks

5. **Error Message Sanitization**
   - Don't expose sensitive information in error messages
   - Log detailed errors server-side only
   - Show user-friendly messages in UI

6. **HTTPS Only**
   - Ensure all API communication uses HTTPS
   - GitHub Pages enforces HTTPS by default

---

## 7. Step-by-Step Implementation Plan

### Phase 1: Configuration and Setup

**Step 1.1: Create Configuration Class**

Create file: `MotoNomad.App/Infrastructure/Configuration/OpenRouterSettings.cs`

```csharp
namespace MotoNomad.Infrastructure.Configuration;

public class OpenRouterSettings
{
    public const string SectionName = "OpenRouter";

    public required string ApiKey { get; set; }
    public string BaseUrl { get; set; } = "https://openrouter.ai/api/v1";
    public string HttpReferer { get; set; } = "https://github.com/yourusername/MotoNomad";
    public string AppTitle { get; set; } = "MotoNomad";
    public int TimeoutSeconds { get; set; } = 60;
    public int MaxRetries { get; set; } = 3;
    public int MaxConcurrentRequests { get; set; } = 5;
    public int MinRequestDelayMs { get; set; } = 100;
}
```

**Step 1.2: Update appsettings.json**

Add OpenRouter configuration to `MotoNomad.App/wwwroot/appsettings.json`:

```json
{
  "Supabase": {
    "Url": "YOUR_SUPABASE_URL",
    "AnonKey": "YOUR_SUPABASE_ANON_KEY"
  },
  "OpenRouter": {
    "ApiKey": "your-api-key-here",
    "BaseUrl": "https://openrouter.ai/api/v1",
    "HttpReferer": "https://yourdomain.github.io/MotoNomad",
    "AppTitle": "MotoNomad - Travel Planning App",
    "TimeoutSeconds": 60,
    "MaxRetries": 3,
    "MaxConcurrentRequests": 5,
    "MinRequestDelayMs": 100
  }
}
```

**Step 1.3: Configure Dependency Injection**

Update `MotoNomad.App/Program.cs`:

```csharp
// Configure OpenRouter settings
builder.Services.Configure<OpenRouterSettings>(
    builder.Configuration.GetSection(OpenRouterSettings.SectionName));

// Register HttpClient for OpenRouter
builder.Services.AddHttpClient<IOpenRouterService, OpenRouterService>();

// Register OpenRouter service as Scoped
builder.Services.AddScoped<IOpenRouterService, OpenRouterService>();
```

### Phase 2: Create DTOs and Exceptions

**Step 2.1: Create DTO Directory Structure**

Create directories:
- `MotoNomad.App/Application/DTOs/OpenRouter/`
- `MotoNomad.App/Application/Exceptions/`

**Step 2.2: Create Request DTOs**

Create files in `Application/DTOs/OpenRouter/`:
- `ChatCompletionRequest.cs` (see Public Methods section)
- `ChatMessage.cs` (see Public Methods section)
- `ResponseFormat.cs` (see Public Methods section)
- `JsonSchemaDefinition.cs` (see Public Methods section)
- `JsonSchemaObject.cs` (see Public Methods section)
- `JsonSchemaProperty.cs` (see Public Methods section)

**Step 2.3: Create Response DTOs**

Create files in `Application/DTOs/OpenRouter/`:
- `ChatCompletionResponse.cs` (see Public Methods section)
- `ChatChoice.cs` (see Public Methods section)
- `UsageInfo.cs` (see Public Methods section)
- `ChatCompletionChunk.cs` (for streaming)
- `ModelInfo.cs` (for GetAvailableModelsAsync)

**Step 2.4: Create Custom Exceptions**

Create files in `Application/Exceptions/`:
- `OpenRouterException.cs`
- `OpenRouterAuthException.cs`
- `OpenRouterRateLimitException.cs`
- `OpenRouterModelNotFoundException.cs`
- `OpenRouterInsufficientCreditsException.cs`
- `OpenRouterServerException.cs`
- `OpenRouterResponseValidationException.cs`

### Phase 3: Create Service Interface

**Step 3.1: Create Interface**

Create file: `MotoNomad.App/Application/Interfaces/IOpenRouterService.cs`

Use interface definition from Constructor Description section.

### Phase 4: Implement Service

**Step 4.1: Create Service Class**

Create file: `MotoNomad.App/Infrastructure/Services/OpenRouterService.cs`

**Step 4.2: Implement Constructor**

```csharp
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

    public OpenRouterService(
        HttpClient httpClient,
        IOptions<OpenRouterSettings> settings,
        ILogger<OpenRouterService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new InvalidOperationException("OpenRouter API key is not configured");
        }

        // Configure HTTP client
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
        _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", _settings.HttpReferer);
        _httpClient.DefaultRequestHeaders.Add("X-Title", _settings.AppTitle);
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
}
```

**Step 4.3: Implement Private Helper Methods**

Add all private methods from Private Methods section:
- `ValidateRequest`
- `ValidateJsonSchema`
- `ApplyRateLimitAsync`
- `HandleHttpErrorAsync`
- `RetryWithExponentialBackoffAsync`
- `ParseStreamingChunk`

**Step 4.4: Implement SendChatCompletionAsync**

```csharp
public async Task<ChatCompletionResponse> SendChatCompletionAsync(
    ChatCompletionRequest request,
    CancellationToken cancellationToken = default)
{
    try
    {
        ValidateRequest(request);
        await ApplyRateLimitAsync(cancellationToken);

        return await RetryWithExponentialBackoffAsync(
            async () =>
            {
                _logger.LogInformation(
                    "Sending chat completion request to model {Model}",
                    request.Model);

                var response = await _httpClient.PostAsJsonAsync(
                    "chat/completions",
                    request,
                    _jsonOptions,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    await HandleHttpErrorAsync(response);
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
```

**Step 4.5: Implement SendStructuredChatCompletionAsync**

```csharp
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
```

**Step 4.6: Implement StreamChatCompletionAsync**

```csharp
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
```

**Step 4.7: Implement ValidateApiKeyAsync**

```csharp
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
```

**Step 4.8: Implement GetAvailableModelsAsync**

```csharp
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
```

**Step 4.9: Implement Dispose**

```csharp
public void Dispose()
{
    if (!_disposed)
    {
        _rateLimitSemaphore?.Dispose();
        _disposed = true;
    }
}
```

### Phase 5: Testing

**Step 5.1: Create Unit Tests**

Create file: `MotoNomad.Tests/Unit/Services/OpenRouterServiceTests.cs`

```csharp
public class OpenRouterServiceTests
{
    [Fact]
    public void Constructor_NullHttpClient_ThrowsArgumentNullException()
    {
        // Arrange
        var settings = Options.Create(new OpenRouterSettings
        {
            ApiKey = "test-key"
        });
        var logger = new Mock<ILogger<OpenRouterService>>().Object;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new OpenRouterService(null!, settings, logger));
    }

    [Fact]
    public void Constructor_EmptyApiKey_ThrowsInvalidOperationException()
    {
        // Arrange
        var httpClient = new HttpClient();
        var settings = Options.Create(new OpenRouterSettings
        {
            ApiKey = ""
        });
        var logger = new Mock<ILogger<OpenRouterService>>().Object;

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            new OpenRouterService(httpClient, settings, logger));
    }

    [Fact]
    public async Task SendChatCompletionAsync_NullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var service = CreateService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await service.SendChatCompletionAsync(null!));
    }

    [Fact]
    public async Task SendChatCompletionAsync_EmptyMessages_ThrowsValidationException()
    {
        // Arrange
        var service = CreateService();
        var request = new ChatCompletionRequest
        {
            Model = "test-model",
            Messages = new List<ChatMessage>()
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(async () =>
            await service.SendChatCompletionAsync(request));
    }

    // Helper method
    private OpenRouterService CreateService()
    {
        var httpClient = new HttpClient();
        var settings = Options.Create(new OpenRouterSettings
        {
            ApiKey = "test-key"
        });
        var logger = new Mock<ILogger<OpenRouterService>>().Object;

        return new OpenRouterService(httpClient, settings, logger);
    }
}
```

**Step 5.2: Create Integration Tests**

Create file: `MotoNomad.Tests/Integration/OpenRouterServiceIntegrationTests.cs`

```csharp
[Collection("Integration")]
public class OpenRouterServiceIntegrationTests
{
    [Fact(Skip = "Requires valid API key")]
    public async Task SendChatCompletionAsync_ValidRequest_ReturnsResponse()
    {
        // Arrange
        var service = CreateRealService();
        var request = new ChatCompletionRequest
        {
            Model = "openai/gpt-3.5-turbo",
            Messages = new List<ChatMessage>
            {
                ChatMessage.User("Say 'hello' in one word")
            },
            MaxTokens = 10
        };

        // Act
        var response = await service.SendChatCompletionAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.NotEmpty(response.Choices);
        Assert.Contains("hello", response.GetContent(), StringComparison.OrdinalIgnoreCase);
    }

    // Create service with real API key from environment variable
    private OpenRouterService CreateRealService()
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY")
            ?? throw new InvalidOperationException("OPENROUTER_API_KEY not set");

        var httpClient = new HttpClient();
        var settings = Options.Create(new OpenRouterSettings
        {
            ApiKey = apiKey
        });
        var logger = new NullLogger<OpenRouterService>();

        return new OpenRouterService(httpClient, settings, logger);
    }
}
```

### Phase 6: Documentation and Examples

**Step 6.1: Create Usage Examples**

Create file: `.ai/openrouter-usage-examples.md`

```markdown
# OpenRouter Service Usage Examples

## Basic Chat Completion

```csharp
var request = new ChatCompletionRequest
{
    Model = "anthropic/claude-3.5-sonnet",
    Messages = new List<ChatMessage>
    {
        ChatMessage.System("You are a travel planning assistant."),
        ChatMessage.User("Suggest a 7-day motorcycle trip through Europe.")
    },
    Temperature = 0.7,
    MaxTokens = 2000
};

var response = await openRouterService.SendChatCompletionAsync(request);
var suggestion = response.GetContent();
```

## Structured JSON Response

```csharp
var schema = new JsonSchemaObject
{
    Type = "object",
    Properties = new Dictionary<string, JsonSchemaProperty>
    {
        ["destination"] = new JsonSchemaProperty
        {
            Type = "string",
            Description = "Trip destination country"
        },
        ["duration_days"] = new JsonSchemaProperty
        {
            Type = "integer",
            Description = "Trip duration in days"
        },
        ["waypoints"] = new JsonSchemaProperty
        {
            Type = "array",
            Items = new JsonSchemaProperty { Type = "string" },
            Description = "List of cities to visit"
        }
    },
    Required = new[] { "destination", "duration_days", "waypoints" },
    AdditionalProperties = false
};

var request = new ChatCompletionRequest
{
    Model = "anthropic/claude-3.5-sonnet",
    Messages = new List<ChatMessage>
    {
        ChatMessage.User("Plan a 7-day trip through Italy")
    },
    ResponseFormat = ResponseFormat.JsonSchema("trip_plan", schema)
};

var tripPlan = await openRouterService.SendStructuredChatCompletionAsync<TripPlan>(request);
```

## Streaming Response

```csharp
var request = new ChatCompletionRequest
{
    Model = "anthropic/claude-3.5-sonnet",
    Messages = new List<ChatMessage>
    {
        ChatMessage.User("Write a travel blog post about motorcycle touring.")
    }
};

await foreach (var chunk in openRouterService.StreamChatCompletionAsync(request))
{
    var content = chunk.Choices.FirstOrDefault()?.Delta?.Content;
    if (!string.IsNullOrEmpty(content))
    {
        // Display content incrementally in UI
        Console.Write(content);
    }
}
```
```

**Step 6.2: Update README**

Add OpenRouter integration section to `README.md`:

```markdown
## AI Features (OpenRouter Integration)

MotoNomad uses OpenRouter API to provide AI-powered travel planning features.

### Setup

1. Get an API key from [OpenRouter](https://openrouter.ai)
2. Add to `appsettings.json`:
   ```json
   {
     "OpenRouter": {
       "ApiKey": "your-api-key-here"
     }
   }
   ```

### Features

- Trip route suggestions
- Destination recommendations
- Itinerary generation
- Budget estimation
```

### Phase 7: Deployment and Monitoring

**Step 7.1: Update GitHub Actions**

Update `.github/workflows/deploy.yml` to inject API key:

```yaml
- name: Configure OpenRouter API Key
  run: |
    sed -i 's/your-api-key-here/${{ secrets.OPENROUTER_API_KEY }}/g' release/wwwroot/appsettings.json
```

**Step 7.2: Add GitHub Secret**

1. Go to repository Settings → Secrets → Actions
2. Add new secret: `OPENROUTER_API_KEY`
3. Paste your OpenRouter API key

**Step 7.3: Implement Monitoring**

Add logging throughout the service for monitoring:

```csharp
_logger.LogInformation("OpenRouter request: Model={Model}, Tokens={MaxTokens}", 
    request.Model, request.MaxTokens);

_logger.LogWarning("Rate limit exceeded, attempt {Attempt} of {MaxAttempts}",
    attempt, maxRetries);

_logger.LogError(ex, "OpenRouter API error: Status={Status}, Message={Message}",
    statusCode, errorMessage);
```

---

## Summary

This implementation plan provides a complete guide for integrating OpenRouter service into MotoNomad. The service follows the project's architectural patterns, includes comprehensive error handling, and provides a clean interface for AI-powered features.

**Key Implementation Points:**

1. ✅ Layered architecture (Application → Infrastructure)
2. ✅ Dependency injection with IOptions pattern
3. ✅ Typed exceptions for error handling
4. ✅ Rate limiting and retry logic
5. ✅ Support for structured JSON responses
6. ✅ Streaming response capability
7. ✅ Comprehensive validation
8. ✅ Security best practices
9. ✅ Unit and integration tests
10. ✅ Complete documentation

**Next Steps:**

1. Implement DTOs and exceptions (Phase 2)
2. Create service interface (Phase 3)
3. Implement service with all methods (Phase 4)
4. Write comprehensive tests (Phase 5)
5. Document usage examples (Phase 6)
6. Configure deployment (Phase 7)

---

**Document Status:** ✅ Ready for Implementation  
**Project:** MotoNomad MVP  
**Date:** October 2025  
**Estimated Implementation Time:** 8-12 hours
