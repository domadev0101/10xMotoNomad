using Microsoft.Extensions.Configuration;

namespace MotoNomad.E2ETests.Configuration;

/// <summary>
/// Manages test configuration including Supabase connection and test user credentials.
/// Reads from User Secrets, environment variables, and optionally appsettings.Test.json.
/// </summary>
public class TestConfiguration
{
    private readonly IConfiguration _configuration;

    public TestConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Test.json", optional: true) // Optional config file
            .AddUserSecrets<TestConfiguration>() // User Secrets (local development)
            .AddEnvironmentVariables(); // Environment variables (CI/CD)

        _configuration = builder.Build();
    }

    // Supabase Connection
    public string SupabaseUrl => GetRequiredValue("Supabase:TestUrl");
    public string SupabasePublicKey => GetRequiredValue("Supabase:TestPublicKey");

    // Test User A (primary test user)
    public string UserAEmail => GetRequiredValue("E2E:UserA:Email");
    public string UserAPassword => GetRequiredValue("E2E:UserA:Password");
    public string? UserAId => _configuration["E2E:UserA:Id"]; // Optional - will be set after login

    // Test User B (for RLS/security tests)
    public string UserBEmail => GetRequiredValue("E2E:UserB:Email");
    public string UserBPassword => GetRequiredValue("E2E:UserB:Password");
    public string? UserBId => _configuration["E2E:UserB:Id"]; // Optional

    // Application Settings
    public string BaseUrl => _configuration["Application:BaseUrl"] ?? "http://localhost:5000";
    public int DefaultTimeout => int.Parse(_configuration["Playwright:DefaultTimeout"] ?? "30000");
    public bool Headless => bool.Parse(_configuration["Playwright:Headless"] ?? "false");
    public int SlowMo => int.Parse(_configuration["Playwright:SlowMo"] ?? "0");

    /// <summary>
    /// Gets a required configuration value. Throws if not found.
    /// </summary>
    private string GetRequiredValue(string key)
    {
        var value = _configuration[key];
        if (string.IsNullOrEmpty(value))
        {
            throw new InvalidOperationException(
                $"Required configuration value '{key}' not found. " +
                $"Please set it in User Secrets or environment variables.");
        }
        return value;
    }

    /// <summary>
    /// Validates that all required configuration values are present.
    /// Call this in test setup to fail fast if configuration is missing.
    /// </summary>
    public void Validate()
    {
        var requiredKeys = new[]
        {
            nameof(SupabaseUrl),
            nameof(SupabasePublicKey),
            nameof(UserAEmail),
            nameof(UserAPassword),
            nameof(UserBEmail),
            nameof(UserBPassword)
        };

        var missingKeys = new List<string>();

        foreach (var key in requiredKeys)
        {
            try
            {
                // Try to access each property to trigger validation
                var property = GetType().GetProperty(key);
                property?.GetValue(this);
            }
            catch
            {
                missingKeys.Add(key);
            }
        }

        if (missingKeys.Any())
        {
            throw new InvalidOperationException(
                $"Missing required configuration values: {string.Join(", ", missingKeys)}. " +
                "Please configure User Secrets with: dotnet user-secrets set \"Key\" \"Value\"");
        }
    }
}
