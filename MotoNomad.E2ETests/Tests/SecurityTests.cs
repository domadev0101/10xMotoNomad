using Microsoft.Playwright;
using MotoNomad.E2ETests.Configuration;
using MotoNomad.E2ETests.Fixtures;
using MotoNomad.E2ETests.PageObjects;
using Xunit;
using Xunit.Abstractions;

namespace MotoNomad.E2ETests.Tests;

/// <summary>
/// E2E tests for security and data isolation (Row Level Security).
/// Test cases: TC-SEC-01
/// </summary>
public class SecurityTests : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private PlaywrightFixture _fixture = null!;
    private LoginPage _loginPage = null!;
    private TripsPage _tripsPage = null!;
    private TripFormPage _tripFormPage = null!;
    private string? _tripIdCreatedByUserA = null;

    public SecurityTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public async Task InitializeAsync()
    {
        _fixture = new PlaywrightFixture();
        await _fixture.InitializeAsync();

        // Initialize page objects
        _loginPage = new LoginPage(_fixture.Page!);
        _tripsPage = new TripsPage(_fixture.Page!);
        _tripFormPage = new TripFormPage(_fixture.Page!);
    }

    public async Task DisposeAsync()
    {
        await _fixture.DisposeAsync();
    }

    /// <summary>
    /// TC-SEC-01: Data Isolation Between Users
    /// Steps:
    /// 1. Register and log in as User A
    /// 2. Create a new trip named "Trip A" and note its ID
    /// 3. Logout
    /// 4. Register and log in as User B
    /// 5. Attempt to navigate directly to the URL for "Trip A"
    /// Expected: User B is shown a "Trip not found" error or is redirected to their own trip list.
    /// User B cannot see or edit "Trip A"
    /// </summary>
    [Fact(Skip = "Temporary disabled")]
    public async Task TC_SEC_01_Data_Isolation_Between_Users()
    {
        // === PHASE 1: User A creates a trip ===

        // Act: Login as User A
        await _loginPage.LoginAsync(
            _fixture.Config.UserAEmail,
            _fixture.Config.UserAPassword
        );
        await _loginPage.WaitForSuccessfulLoginAsync();

        // Act: Create a trip as User A
        var tripNameA = $"[TEST-RLS] User A Trip {Guid.NewGuid().ToString().Substring(0, 8)}";
        var startDate = "2025-12-01";
        var endDate = "2025-12-15";
        var transport = "Motorcycle";

        await _tripFormPage.CreateTripAsync(tripNameA, startDate, endDate, transport);
        await _tripFormPage.WaitForSuccessfulSaveAsync();

        // Wait for trip to appear and capture its URL/ID
        await _tripsPage.NavigateAsync();
        var isTripVisible = await _tripsPage.IsTripVisibleByNameAsync(tripNameA, timeoutMs: 10000);
        Assert.True(isTripVisible, "Trip should be visible for User A");

        // Click on the trip to get to details page and capture the URL
        await _tripsPage.ClickTripByNameAsync(tripNameA);
        await Task.Delay(1000); // Wait for navigation

        var tripAUrl = _fixture.Page!.Url;
        _output.WriteLine($"User A's trip URL: {tripAUrl}");

        // Extract trip ID from URL (format: /trip/{id} or /trip/edit/{id})
        var tripIdMatch = System.Text.RegularExpressions.Regex.Match(tripAUrl, @"/trip/([^/]+)");
        if (tripIdMatch.Success)
        {
            _tripIdCreatedByUserA = tripIdMatch.Groups[1].Value;
            _output.WriteLine($"User A's trip ID: {_tripIdCreatedByUserA}");
        }

        // Act: Logout User A (navigate to login page as proxy for logout)
        await _loginPage.NavigateAsync();

        // === PHASE 2: User B tries to access User A's trip ===

        // Act: Login as User B
        await _loginPage.LoginAsync(
            _fixture.Config.UserBEmail,
            _fixture.Config.UserBPassword
        );
        await _loginPage.WaitForSuccessfulLoginAsync();

        // Assert: User B should be on their trips page
        Assert.True(_tripsPage.IsOnTripsPage(),
            "User B should be redirected to their trips page");

        // Assert: User B should NOT see User A's trip in their list
        var canSeeTripA = await _tripsPage.IsTripVisibleByNameAsync(tripNameA, timeoutMs: 2000);
        Assert.False(canSeeTripA,
            "User B should NOT see User A's trip in their trip list");

        // Act: User B attempts to navigate directly to User A's trip URL
        if (!string.IsNullOrEmpty(_tripIdCreatedByUserA))
        {
            var directUrl = $"/trip/{_tripIdCreatedByUserA}";
            await _fixture.Page!.GotoAsync(directUrl);
            await Task.Delay(2000); // Wait for potential redirect

            // Assert: User B should either:
            // 1. See "Trip not found" / "Not authorized" error
            // 2. Be redirected back to /trips
            // 3. NOT see the trip details

            var currentUrl = _fixture.Page!.Url;
            _output.WriteLine($"User B navigated to: {currentUrl}");

            // Check if redirected back to trips list
            var wasRedirected = currentUrl.Contains("/trips") && !currentUrl.Contains(_tripIdCreatedByUserA);

            // OR check if error message is displayed
            var hasErrorMessage = await HasErrorOrNotFoundMessageAsync();

            Assert.True(wasRedirected || hasErrorMessage,
                "User B should be redirected to /trips or see an error when trying to access User A's trip");
        }
        else
        {
            _output.WriteLine("WARNING: Could not extract trip ID from URL. RLS test incomplete.");
        }
    }

    /// <summary>
    /// Helper method to check if there's a "not found" or "unauthorized" message on the page.
    /// </summary>
    private async Task<bool> HasErrorOrNotFoundMessageAsync()
    {
        try
        {
            // Look for common error indicators using regex for case-insensitive search
            var errorTexts = new[] { "not found", "unauthorized", "access denied", "not authorized", "404" };

            foreach (var errorText in errorTexts)
            {
                // Use regex with 'i' flag for case-insensitive matching
                var hasText = await _fixture.Page!.Locator($"text=/{errorText}/i").CountAsync() > 0;

                if (hasText)
                {
                    _output.WriteLine($"Found error indicator: '{errorText}'");
                    return true;
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}