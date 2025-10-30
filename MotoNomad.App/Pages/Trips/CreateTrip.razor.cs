using Microsoft.AspNetCore.Components;
using MotoNomad.Application.Commands.Trips;
using MotoNomad.Application.Interfaces;
using MotoNomad.Application.Exceptions;
using MudBlazor;

namespace MotoNomad.App.Pages.Trips;

/// <summary>
/// Page for creating a new trip with basic information.
/// Protected route - requires authentication.
/// </summary>
public partial class CreateTrip
{
    [Inject] private ITripService TripService { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private bool isLoading = false;
    private string? errorMessage = null;

    /// <summary>
    /// Handles trip creation by calling the TripService API.
    /// Shows success snackbar and navigates to trips list on success.
    /// Shows error alert on validation or database errors.
    /// </summary>
    /// <param name="command">Trip creation command with validated data</param>
    private async Task HandleCreateTripAsync(object commandObj)
    {
        var command = commandObj as CreateTripCommand;
        if (command == null) return;

        isLoading = true;
        errorMessage = null;

        try
        {
            var trip = await TripService.CreateTripAsync(command);
            Snackbar.Add($"Trip '{trip.Name}' has been created!", Severity.Success);
            NavigationManager.NavigateTo("trips");
        }
        catch (UnauthorizedException)
        {
            Snackbar.Add("Session expired. Please log in again.", Severity.Warning);
            NavigationManager.NavigateTo("login");
        }
        catch (ValidationException ex)
        {
            errorMessage = ex.Message;
        }
        catch (DatabaseException)
        {
            errorMessage = "Failed to create trip. Please try again.";
        }
        catch (Exception)
        {
            errorMessage = "An unexpected error occurred. Please try again.";
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Handles cancellation by navigating back to trips list without saving.
    /// </summary>
    private void HandleCancel()
    {
        NavigationManager.NavigateTo("trips");
    }
}
