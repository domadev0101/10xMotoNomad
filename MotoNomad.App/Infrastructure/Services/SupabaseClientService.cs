using MotoNomad.App.Infrastructure.Configuration;
using MotoNomad.App.Application.Interfaces;
using Blazored.LocalStorage;
using Microsoft.Extensions.Logging;

namespace MotoNomad.App.Infrastructure.Services;

public class SupabaseClientService : ISupabaseClientService
{
    private readonly Supabase.Client _client;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SupabaseClientService> _logger;
    private bool _isInitialized;
    private const string SessionKey = "supabase.auth.session";

    public SupabaseClientService(
        SupabaseSettings settings,
        IServiceProvider serviceProvider,
        ILogger<SupabaseClientService> logger)
    {
        _serviceProvider = serviceProvider;
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
                AutoConnectRealtime = false, //disabled for MVP
                AutoRefreshToken = true
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

            // Try to restore session from localStorage BEFORE marking as initialized
            await RestoreSessionFromStorageAsync();

            _isInitialized = true;
            _logger.LogInformation("Supabase client initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Supabase client");
            throw;
        }
    }

    /// <summary>
    /// Restores the session from browser localStorage during initialization.
    /// This ensures the session is available before any components try to check authentication.
    /// </summary>
    private async Task RestoreSessionFromStorageAsync()
    {
        try
        {
            // Create a scope to resolve scoped ILocalStorageService
            using var scope = _serviceProvider.CreateScope();
            var localStorage = scope.ServiceProvider.GetRequiredService<ILocalStorageService>();

            var session = await localStorage.GetItemAsync<Supabase.Gotrue.Session>(SessionKey);

            if (session != null && session.AccessToken != null && session.RefreshToken != null)
            {
                await _client.Auth.SetSession(session.AccessToken, session.RefreshToken);
                _logger.LogInformation("Session restored from localStorage for user: {Email}", session.User?.Email);
            }
            else
            {
                _logger.LogInformation("No existing session found in localStorage");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to restore session from localStorage during initialization");
            // Don't throw - missing session is not a fatal error
        }
    }

    public bool IsInitialized => _isInitialized;
}
