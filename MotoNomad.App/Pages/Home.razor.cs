using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MotoNomad.Application.DTOs.Trips;
using MotoNomad.Application.Interfaces;
using System.Security.Claims;

namespace MotoNomad.App.Pages;

/// <summary>
/// Home page component showing marketing page for non-authenticated users
/// and dashboard for authenticated users.
/// </summary>
public partial class Home
{
    [Inject] private ITripService TripService { get; set; } = default!;
    [Inject] private IAuthService AuthService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

    private List<TripListItemDto> recentTrips = new();
    private int upcomingTripsCount = 0;
    private int totalTripsCount = 0;
    private int totalCompanionsCount = 0;
    private bool isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        var isAuthenticated = await AuthService.IsAuthenticatedAsync();
        
        if (isAuthenticated)
        {
            await LoadDashboardData();
        }
        else
        {
            isLoading = false;
        }
    }

    /// <summary>
    /// Loads dashboard data for authenticated users.
    /// </summary>
    private async Task LoadDashboardData()
    {
        try
        {
            isLoading = true;
          StateHasChanged(); // Force UI update to show loading spinner

     // Load all trips
      var allTrips = await TripService.GetAllTripsAsync();
            recentTrips = allTrips.OrderByDescending(t => t.CreatedAt).ToList();

            // Calculate stats
   var upcomingTrips = await TripService.GetUpcomingTripsAsync();
   upcomingTripsCount = upcomingTrips.Count();
        
            totalTripsCount = recentTrips.Count;
            totalCompanionsCount = recentTrips.Sum(t => t.CompanionCount);
        }
        catch
    {
     // Silently fail - user will see empty state
        }
        finally
        {
            isLoading = false;
    StateHasChanged(); // Force UI update with loaded data
        }
    }

    /// <summary>
    /// Handles navigation to create first trip.
    /// </summary>
    private void HandleCreateFirstTrip()
    {
        NavigationManager.NavigateTo("/trip/create");
    }

    /// <summary>
    /// Handles trip card click navigation.
    /// </summary>
    private void HandleTripClick(Guid tripId)
    {
   NavigationManager.NavigateTo($"/trip/{tripId}");
    }

    /// <summary>
 /// Gets the display name from the authentication context.
    /// Falls back to email if display name is not available.
    /// </summary>
    private string GetUserDisplayName(AuthenticationState context)
    {
        var user = context.User;
    
        // Try to get display name from user metadata
    var displayName = user.FindFirst("user_metadata_display_name")?.Value;
    
        if (!string.IsNullOrEmpty(displayName))
        {
            return displayName;
        }
        
     // Fallback to email
        var email = user.Identity?.Name ?? user.FindFirst(ClaimTypes.Email)?.Value;
        
        if (!string.IsNullOrEmpty(email))
        {
            // Return first part of email (before @)
         return email.Split('@')[0];
        }
     
        return "Traveler";
    }
}
