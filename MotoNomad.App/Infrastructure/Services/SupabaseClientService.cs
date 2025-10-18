using Supabase;
using MotoNomad.App.Infrastructure.Configuration;
using MotoNomad.App.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace MotoNomad.App.Infrastructure.Services;

public class SupabaseClientService : ISupabaseClientService
{
    private readonly Client _client;
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
            var options = new SupabaseOptions
            {
                AutoConnectRealtime = true,
                AutoRefreshToken = true
            };

            _client = new Client(settings.Url, settings.AnonKey, options);
            _logger.LogInformation("Supabase client created successfully. URL: {Url}", settings.Url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Supabase client");
            throw;
        }
    }

    public Client GetClient()
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
