using Microsoft.Playwright;
using MotoNomad.E2ETests.Configuration;
using MotoNomad.E2ETests.Fixtures;
using MotoNomad.E2ETests.PageObjects;
using Xunit;

namespace MotoNomad.E2ETests.Tests;

/// <summary>
/// E2E tests for trip management (CRUD operations).
/// Test cases: TC-TRIP-01, TC-TRIP-02, TC-TRIP-03, TC-TRIP-04
/// </summary>
public class TripManagementTests : IAsyncLifetime
{
    private PlaywrightFixture _fixture = null!;
    private LoginPage _loginPage = null!;
    private TripsPage _tripsPage = null!;
    private TripFormPage _tripFormPage = null!;

    public async Task InitializeAsync()
    {
        _fixture = new PlaywrightFixture();
        await _fixture.InitializeAsync();

        // Initialize page objects
        _loginPage = new LoginPage(_fixture.Page!);
        _tripsPage = new TripsPage(_fixture.Page!);
        _tripFormPage = new TripFormPage(_fixture.Page!);

        // Login as User A before each test
        await LoginAsTestUserAsync();
    }

    public async Task DisposeAsync()
    {
        await _fixture.DisposeAsync();
    }

    private async Task LoginAsTestUserAsync()
    {
        await _loginPage.LoginAsync(
            _fixture.Config.UserAEmail,
            _fixture.Config.UserAPassword
        );
        await _loginPage.WaitForSuccessfulLoginAsync();
    }

    /// <summary>
    /// TC-TRIP-01: Create a New Trip with Valid Data
    /// Steps:
    /// 1. Login
    /// 2. Navigate to /trip/create
    /// 3. Fill in all required fields (Name, Start Date, End Date, Transport Type)
    /// 4. Submit the form
    /// Expected: Trip is created successfully, user is redirected to /trips,
    /// and the new trip appears in the "Upcoming" tab
    /// </summary>
    [Fact(Skip = "Temporary disabled")]
    public async Task TC_TRIP_01_Create_New_Trip_With_Valid_Data()
    {
        // Arrange
        var tripName = $"[TEST] Summer Adventure {Guid.NewGuid().ToString().Substring(0, 8)}";
        var startDate = "2025-12-01";
        var endDate = "2025-12-15";
        var transport = "Motorcycle";
        var description = "A wonderful motorcycle trip through scenic routes.";

        // Act: Create a new trip
        await _tripFormPage.CreateTripAsync(tripName, startDate, endDate, transport, description);

        // Wait for redirect to trips page
        await _tripFormPage.WaitForSuccessfulSaveAsync();

        // Assert: Should be on trips page
        Assert.True(_tripsPage.IsOnTripsPage(),
            "User should be redirected to /trips after creating a trip");

        // Assert: Trip should appear in the list
        await _tripsPage.NavigateAsync(); // Refresh the page to ensure trip is loaded

        var isTripVisible = await _tripsPage.IsTripVisibleByNameAsync(tripName, timeoutMs: 10000);
        Assert.True(isTripVisible,
            $"Trip '{tripName}' should be visible in the trips list");

        // Verify trip appears in Upcoming tab
        await _tripsPage.SwitchToUpcomingTabAsync();
        var tripNames = await _tripsPage.GetAllTripNamesAsync();
        Assert.Contains(tripName, tripNames);
    }

    /// <summary>
    /// TC-TRIP-02: Create a Trip with Invalid Data
    /// Steps:
    /// 1. Login
    /// 2. Navigate to /trip/create
    /// 3. Attempt to submit the form with an end date that is before the start date
    /// Expected: Form validation prevents submission, and error message
    /// "End date must be after start date" is displayed
    /// </summary>
    [Fact(Skip = "Temporary disabled")]
    public async Task TC_TRIP_02_Create_Trip_With_Invalid_Data()
    {
        // Arrange
        var tripName = $"[TEST] Invalid Trip {Guid.NewGuid().ToString().Substring(0, 8)}";
        var startDate = "2025-12-15"; // Later date
        var endDate = "2025-12-01";   // Earlier date (INVALID!)
        var transport = "Car";

        // Act: Navigate to create page
        await _tripFormPage.NavigateToCreateAsync();

        // Fill in the form with invalid dates
        await _tripFormPage.FillNameAsync(tripName);
        await _tripFormPage.FillStartDateAsync(startDate);
        await _tripFormPage.FillEndDateAsync(endDate);
        await _tripFormPage.SelectTransportAsync(transport);

        // Try to submit
        await _tripFormPage.ClickSubmitAsync();

        // Assert: Validation error should be displayed
        var hasError = await _tripFormPage.HasValidationErrorAsync();
        Assert.True(hasError,
            "Validation error should be displayed for invalid date range");

        var errorMessage = await _tripFormPage.GetValidationErrorAsync();
        Assert.Contains("after", errorMessage?.ToLower() ?? "");

        // Assert: Should still be on the create page (not redirected)
        Assert.Contains("/trip/create", _fixture.Page!.Url);
    }

    /// <summary>
    /// TC-TRIP-03: Edit an Existing Trip
    /// Steps:
    /// 1. Login
    /// 2. Create a trip
    /// 3. Navigate to edit page
    /// 4. Update the trip name
    /// 5. Submit the form
    /// Expected: Trip is updated successfully
    /// </summary>
    [Fact(Skip = "Not implemented yet - add when edit functionality is ready")]
    public async Task TC_TRIP_03_Edit_Existing_Trip()
    {
        // TODO: Implement this test
        // 1. Create a trip
        // 2. Click on the trip to navigate to details/edit
        // 3. Update fields
        // 4. Save
        // 5. Verify changes

        await Task.CompletedTask;
    }

    /// <summary>
    /// TC-TRIP-04: Delete a Trip
    /// Steps:
    /// 1. Login
    /// 2. Create a trip
    /// 3. Delete the trip
    /// Expected: Trip is removed from the list
    /// </summary>
    [Fact(Skip = "Not implemented yet - add when delete functionality is ready")]
    public async Task TC_TRIP_04_Delete_Trip()
    {
        // TODO: Implement this test
        // 1. Create a trip
        // 2. Find delete button/option
        // 3. Confirm deletion
        // 4. Verify trip is gone from list

        await Task.CompletedTask;
    }
}