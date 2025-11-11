namespace MotoNomad.App.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for OpenRouter API integration
/// </summary>
public class OpenRouterSettings
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "OpenRouter";

    /// <summary>
    /// OpenRouter API key (required) - używany tylko jeśli UseEdgeFunctionProxy = false
    /// </summary>
    public required string ApiKey { get; set; }

    /// <summary>
    /// OpenRouter API base URL - używany tylko jeśli UseEdgeFunctionProxy = false
    /// </summary>
    public string BaseUrl { get; set; } = "https://openrouter.ai/api/v1";

    /// <summary>
    /// URL Supabase Edge Function proxy (jeśli UseEdgeFunctionProxy = true)
    /// </summary>
    public string? EdgeFunctionUrl { get; set; }

    /// <summary>
    /// Czy używać Supabase Edge Function jako proxy (zalecane dla produkcji)
    /// </summary>
    public bool UseEdgeFunctionProxy { get; set; } = false;

    /// <summary>
    /// HTTP Referer header for OpenRouter API requests
    /// </summary>
    public string HttpReferer { get; set; } = "https://github.com/domadev0101/10xMotoNomad";

    /// <summary>
    /// Application title for OpenRouter API requests
    /// </summary>
    public string AppTitle { get; set; } = "MotoNomad - Travel Planning App";

    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// Maximum number of retry attempts for failed requests
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Maximum number of concurrent requests allowed
    /// </summary>
    public int MaxConcurrentRequests { get; set; } = 5;

    /// <summary>
    /// Minimum delay between requests in milliseconds (rate limiting)
    /// </summary>
    public int MinRequestDelayMs { get; set; } = 100;
}
