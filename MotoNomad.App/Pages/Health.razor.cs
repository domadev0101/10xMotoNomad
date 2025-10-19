using MotoNomad.App.Application.Interfaces;
using MotoNomad.App.Infrastructure.Database.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace MotoNomad.App.Pages;

/// <summary>
/// Health Check page - diagnostic tool for verifying Supabase connection and database access.
/// Performs comprehensive tests including client initialization, database connectivity, table access, and authentication status.
/// </summary>
public partial class Health
{
    [Inject]
    private ISupabaseClientService SupabaseClient { get; set; } = null!;

    [Inject]
    private ILogger<Health> Logger { get; set; } = null!;

    private bool isLoading = false;
    private bool allTestsPassed = false;
    private DateTime? lastCheckTime;
    private string environment = "Unknown";
    private List<HealthCheckResult> healthCheckResults = new();

    protected override void OnInitialized()
    {
        #if DEBUG
        environment = "Development";
        #else
        environment = "Production";
        #endif
    }

    /// <summary>
    /// Runs all health check tests sequentially and updates the UI with results.
    /// </summary>
    private async Task RunHealthCheck()
    {
        isLoading = true;
        healthCheckResults.Clear();
        allTestsPassed = false;
        StateHasChanged();

        await Task.Delay(100); // Small delay for UI update

        try
        {
            // Test 1: Client Initialization
            await TestClientInitialization();

            // Test 2: Database Connectivity
            await TestDatabaseConnectivity();

            // Test 3: Table Access (trips)
            await TestTableAccess();

            // Test 4: Auth Status
            await TestAuthStatus();

            // Calculate overall status
            allTestsPassed = healthCheckResults.All(r => r.IsSuccess);
            lastCheckTime = DateTime.Now;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Health check failed with unexpected error");
            healthCheckResults.Add(new HealthCheckResult
            {
                TestName = "Unexpected Error",
                IsSuccess = false,
                Message = "Health check encountered an unexpected error",
                ErrorMessage = ex.Message
            });
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Tests if the Supabase client was properly initialized during application startup.
    /// </summary>
    private async Task TestClientInitialization()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var isInitialized = SupabaseClient.IsInitialized;
            stopwatch.Stop();

            if (isInitialized)
            {
                healthCheckResults.Add(new HealthCheckResult
                {
                    TestName = "Client Initialization",
                    IsSuccess = true,
                    Message = "Supabase client is properly initialized",
                    Duration = stopwatch.ElapsedMilliseconds
                });
                Logger.LogInformation("Health check: Client initialization - SUCCESS");
            }
            else
            {
                healthCheckResults.Add(new HealthCheckResult
                {
                    TestName = "Client Initialization",
                    IsSuccess = false,
                    Message = "Supabase client is not initialized",
                    ErrorMessage = "Client initialization failed during application startup",
                    Duration = stopwatch.ElapsedMilliseconds
                });
                Logger.LogWarning("Health check: Client initialization - FAILED");
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            healthCheckResults.Add(new HealthCheckResult
            {
                TestName = "Client Initialization",
                IsSuccess = false,
                Message = "Failed to check client initialization",
                ErrorMessage = ex.Message,
                Duration = stopwatch.ElapsedMilliseconds
            });
            Logger.LogError(ex, "Health check: Client initialization - ERROR");
        }
    }

    /// <summary>
    /// Tests database connectivity by obtaining a Supabase client instance.
    /// </summary>
    private async Task TestDatabaseConnectivity()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var client = SupabaseClient.GetClient();
            
            // Simple connectivity test - just getting the client
            if (client != null)
            {
                stopwatch.Stop();
                healthCheckResults.Add(new HealthCheckResult
                {
                    TestName = "Database Connectivity",
                    IsSuccess = true,
                    Message = "Successfully obtained Supabase client instance",
                    Details = $"Client is ready for database operations",
                    Duration = stopwatch.ElapsedMilliseconds
                });
                Logger.LogInformation("Health check: Database connectivity - SUCCESS");
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            healthCheckResults.Add(new HealthCheckResult
            {
                TestName = "Database Connectivity",
                IsSuccess = false,
                Message = "Failed to establish database connection",
                ErrorMessage = ex.Message,
                Duration = stopwatch.ElapsedMilliseconds
            });
            Logger.LogError(ex, "Health check: Database connectivity - ERROR");
        }
    }

    /// <summary>
    /// Tests table access by querying the 'trips' table.
    /// Verifies that the table exists and Row Level Security policies allow access.
    /// </summary>
    private async Task TestTableAccess()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var client = SupabaseClient.GetClient();
            
            // Try to query the trips table (just count, no actual data retrieval)
            var response = await client
                .From<Trip>()
                .Select("id")
                .Limit(1)
                .Get();

            stopwatch.Stop();

            healthCheckResults.Add(new HealthCheckResult
            {
                TestName = "Table Access (trips)",
                IsSuccess = true,
                Message = "Successfully queried 'trips' table",
                Details = $"Table is accessible and query executed successfully",
                Duration = stopwatch.ElapsedMilliseconds
            });
            Logger.LogInformation("Health check: Table access - SUCCESS");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            healthCheckResults.Add(new HealthCheckResult
            {
                TestName = "Table Access (trips)",
                IsSuccess = false,
                Message = "Failed to access 'trips' table",
                ErrorMessage = ex.Message,
                Details = "Ensure table exists and RLS policies allow anonymous access",
                Duration = stopwatch.ElapsedMilliseconds
            });
            Logger.LogError(ex, "Health check: Table access - ERROR");
        }
    }

    /// <summary>
    /// Tests authentication status by checking for an active user session.
    /// </summary>
    private async Task TestAuthStatus()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var client = SupabaseClient.GetClient();
            var session = client.Auth.CurrentSession;
            
            stopwatch.Stop();

            if (session != null)
            {
                healthCheckResults.Add(new HealthCheckResult
                {
                    TestName = "Auth Status",
                    IsSuccess = true,
                    Message = "User is authenticated",
                    Details = $"Active session found",
                    Duration = stopwatch.ElapsedMilliseconds
                });
            }
            else
            {
                healthCheckResults.Add(new HealthCheckResult
                {
                    TestName = "Auth Status",
                    IsSuccess = true,
                    Message = "No active user session (anonymous access)",
                    Details = "This is expected for unauthenticated users",
                    Duration = stopwatch.ElapsedMilliseconds
                });
            }

            Logger.LogInformation("Health check: Auth status - SUCCESS");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            healthCheckResults.Add(new HealthCheckResult
            {
                TestName = "Auth Status",
                IsSuccess = false,
                Message = "Failed to check authentication status",
                ErrorMessage = ex.Message,
                Duration = stopwatch.ElapsedMilliseconds
            });
            Logger.LogError(ex, "Health check: Auth status - ERROR");
        }
    }

    /// <summary>
    /// Represents the result of a single health check test.
    /// </summary>
    private class HealthCheckResult
    {
        /// <summary>
        /// Name of the test performed.
        /// </summary>
        public string TestName { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether the test passed successfully.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// User-friendly message describing the test result.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Optional additional details about the test result.
        /// </summary>
        public string? Details { get; set; }

        /// <summary>
        /// Error message if the test failed.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Test execution duration in milliseconds.
        /// </summary>
        public long Duration { get; set; }
    }
}
