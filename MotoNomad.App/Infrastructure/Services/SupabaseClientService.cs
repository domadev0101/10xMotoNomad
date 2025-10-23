using MotoNomad.App.Infrastructure.Configuration;
using MotoNomad.App.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace MotoNomad.App.Infrastructure.Services;

public class SupabaseClientService : ISupabaseClientService
{
    private readonly Supabase.Client _client;
    private readonly ILogger<SupabaseClientService> _logger;
    private bool _isInitialized;

    public SupabaseClientService(SupabaseSettings settings, ILogger<SupabaseClientService> logger)
    {
        _logger = logger;

        if (!settings.IsValid())
        {
            throw new InvalidOperationException(
                "Supabase configuration is invalid. Please check appsettings.json for Url and AnonKey.");
        }

        try
        {
            var options = new Supabase.SupabaseOptions
            {
                AutoConnectRealtime = true,
                AutoRefreshToken = true,
                // Session persistence is handled automatically by supabase-csharp
                // It stores session in browser localStorage by default
            };

            _client = new Supabase.Client(settings.Url, settings.AnonKey, options);
            _logger.LogInformation("Supabase client created successfully. URL: {Url}", settings.Url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Supabase client");
            throw;
        }
    }

    public Supabase.Client GetClient()
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException(
                "Supabase client is not initialized. Call InitializeAsync() first.");
        }

        return _client;
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            _logger.LogWarning("Supabase client is already initialized");
            return;
        }

        try
        {
            _logger.LogInformation("Initializing Supabase client connection...");
            await _client.InitializeAsync();
            _isInitialized = true;

            // Try to restore session from storage
            var session = await _client.Auth.RetrieveSessionAsync();
            if (session != null)
            {
                _logger.LogInformation("Session restored for user: {Email}", session.User?.Email);
            }
            else
            {
                _logger.LogInformation("No existing session found");
            }

            _logger.LogInformation("Supabase client initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Supabase client");
            throw;
        }
    }

    public bool IsInitialized => _isInitialized;
}
