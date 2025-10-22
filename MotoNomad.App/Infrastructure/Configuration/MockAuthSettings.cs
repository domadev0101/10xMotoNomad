namespace MotoNomad.App.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for mock authentication (development/testing only).
/// ?? WARNING: Never enable this in production!
/// </summary>
public class MockAuthSettings
{
    /// <summary>
    /// Enables mock authentication mode.
    /// When true, the application will use MockAuthenticationStateProvider instead of real Supabase auth.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Mock user ID - should match a real user ID in your Supabase database for RLS policies to work.
    /// </summary>
    public string UserId { get; set; } = "00000000-0000-0000-0000-000000000000";

    /// <summary>
    /// Mock user email for display purposes.
    /// </summary>
    public string Email { get; set; } = "test@example.com";

    /// <summary>
    /// Mock user display name for UI.
    /// </summary>
    public string DisplayName { get; set; } = "Test User";
}
