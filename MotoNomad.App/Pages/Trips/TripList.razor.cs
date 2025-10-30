using Microsoft.AspNetCore.Components;
using MotoNomad.Application.DTOs.Trips;
using MotoNomad.Application.Interfaces;
using MotoNomad.Application.Exceptions;
using MudBlazor;

namespace MotoNomad.App.Pages.Trips;

/// <summary>
/// Page displaying user's trips in two tabs: Upcoming and Past.
/// Loads trips in parallel for better performance.
/// Protected route - requires authentication.
/// </summary>
public partial class TripList
{
    [Inject] private ITripService TripService { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private List<TripListItemDto> upcomingTrips = new();
    private List<TripListItemDto> pastTrips = new();
    private bool isLoadingUpcoming = false;
    private bool isLoadingPast = false;

    /// <summary>
    /// Loads upcoming and past trips in parallel when component initializes.
    /// Handles authentication errors by redirecting to login.
    /// Shows error snackbar on database errors.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        try
        {
            isLoadingUpcoming = true;
            isLoadingPast = true;

            // Load trips in parallel for better performance
            var upcomingTask = TripService.GetUpcomingTripsAsync();
            var pastTask = TripService.GetPastTripsAsync();

            await Task.WhenAll(upcomingTask, pastTask);

            upcomingTrips = upcomingTask.Result.ToList();
            pastTrips = pastTask.Result.ToList();
        }
        catch (UnauthorizedException)
        {
            Snackbar.Add("Session expired. Please log in again.", Severity.Warning);
            NavigationManager.NavigateTo("login");
        }
        catch (DatabaseException)
        {
            Snackbar.Add("Failed to load trips. Please try again.", Severity.Error);
        }
        catch (Exception)
        {
            Snackbar.Add("An unexpected error occurred. Please try again.", Severity.Error);
        }
        finally
        {
            isLoadingUpcoming = false;
            isLoadingPast = false;
        }
    }

    /// <summary>
    /// Handles trip card click by navigating to trip details page.
    /// </summary>
    /// <param name="tripId">Unique trip identifier</param>
    private void HandleTripClick(Guid tripId)
    {
        NavigationManager.NavigateTo($"trip/{tripId}");
    }
}
