namespace MotoNomad.App.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for Supabase connection
/// </summary>
public class SupabaseSettings
{
    /// <summary>
    /// Supabase project URL (e.g., https://xxxxx.supabase.co or http://127.0.0.1:54321 for local)
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Supabase anonymous (public) key - safe to use in client-side code
    /// </summary>
    public string AnonKey { get; set; } = string.Empty;

    /// <summary>
    /// Validates if required settings are configured
    /// </summary>
    public bool IsValid() => !string.IsNullOrWhiteSpace(Url) && !string.IsNullOrWhiteSpace(AnonKey);
}
