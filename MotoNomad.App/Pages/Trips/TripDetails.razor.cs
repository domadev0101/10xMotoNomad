using Microsoft.AspNetCore.Components;
using MotoNomad.Application.DTOs.Trips;
using MotoNomad.Application.DTOs.Companions;
using MotoNomad.Application.Interfaces;
using MotoNomad.Application.Exceptions;
using MotoNomad.App.Shared.Components;
using MotoNomad.App.Shared.Dialogs;
using MudBlazor;

namespace MotoNomad.App.Pages.Trips;

/// <summary>
/// Trip details page component - displays and manages single trip information.
/// Supports editing trip details and managing companions (Phase 3).
/// </summary>
public partial class TripDetails : ComponentBase
{
    #region Dependency Injection

    [Inject] private ITripService TripService { get; set; } = default!;
    [Inject] private ICompanionService CompanionService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;

    #endregion

    #region Parameters

    /// <summary>
    /// Trip unique identifier from route parameter.
    /// </summary>
    [Parameter]
    public Guid Id { get; set; }

    #endregion

    #region State Variables

    private TripDetailDto? trip = null;
    private List<CompanionListItemDto> companions = new();
    private bool isLoading = false;
    private bool isUpdatingTrip = false;
    private bool showCompanionForm = false;
    private bool isAddingCompanion = false;
    private int activeTabIndex = 0; // 0 = Details, 1 = Companions
    private string? errorMessage = null;
    private TripForm? tripFormRef = null;
    private bool canSubmitTrip = true; // Track form validation state

    #endregion

    #region UI Helpers

    /// <summary>
    /// Breadcrumb navigation items.
    /// </summary>
    private List<BreadcrumbItem> breadcrumbItems => new()
    {
        new BreadcrumbItem("My Trips", href: "trips"),
        new BreadcrumbItem(trip?.Name ?? "Trip", href: null, disabled: true)
    };

    #endregion

    #region Lifecycle Methods

    /// <summary>
    /// Initializes component and loads trip data with companions in parallel.
    /// Handles RLS security - redirects to /trips if trip not found or access denied.
    /// </summary>
    protected override async Task OnInitializedAsync()
 {
        await LoadTripDataAsync();
    }

    /// <summary>
    /// Handles route parameter changes (when navigating between different trips).
/// </summary>
    protected override async Task OnParametersSetAsync()
    {
        await LoadTripDataAsync();
    }

    #endregion

    #region Data Loading

    /// <summary>
    /// Loads trip and companions data in parallel using Task.WhenAll for optimization.
    /// </summary>
    private async Task LoadTripDataAsync()
    {
   isLoading = true;
        errorMessage = null;

        try
        {
// Parallel loading for better performance
 var tripTask = TripService.GetTripByIdAsync(Id);
var companionsTask = CompanionService.GetCompanionsByTripIdAsync(Id);

         await Task.WhenAll(tripTask, companionsTask);

trip = await tripTask;
    companions = (await companionsTask).ToList();
        }
        catch (NotFoundException)
        {
     // RLS security - user tried to access someone else's trip or trip doesn't exist
      Snackbar.Add("Trip not found.", Severity.Warning);
 NavigationManager.NavigateTo("trips");
      }
   catch (UnauthorizedException)
        {
      // Session expired
          Snackbar.Add("Session expired. Please log in again.", Severity.Warning);
        NavigationManager.NavigateTo("login");
        }
      catch (Exception ex)
        {
            Snackbar.Add("An error occurred while loading the trip.", Severity.Error);
     // TODO: Log error with ILogger
      errorMessage = "Failed to load trip.";
     }
        finally
        {
    isLoading = false;
            StateHasChanged();
        }
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Handles trip update form submission.
    /// Validates and saves changes to the trip.
    /// </summary>
    private async Task HandleUpdateTripAsync(object command)
    {
        if (command is not MotoNomad.Application.Commands.Trips.UpdateTripCommand updateCommand)
    {
            Snackbar.Add("Invalid form data.", Severity.Error);
     return;
        }

        isUpdatingTrip = true;
     errorMessage = null;

        try
        {
        trip = await TripService.UpdateTripAsync(updateCommand);
      Snackbar.Add("Changes saved successfully!", Severity.Success);
        }
 catch (ValidationException ex)
        {
          errorMessage = ex.Message;
            Snackbar.Add("Please check your input.", Severity.Warning);
        }
        catch (NotFoundException)
  {
       Snackbar.Add("Trip not found.", Severity.Warning);
     NavigationManager.NavigateTo("trips");
 }
        catch (UnauthorizedException)
    {
  Snackbar.Add("Session expired. Please log in again.", Severity.Warning);
            NavigationManager.NavigateTo("login");
  }
        catch (DatabaseException)
   {
     errorMessage = "Failed to save changes. Please try again.";
 Snackbar.Add(errorMessage, Severity.Error);
      }
        catch (Exception ex)
     {
  errorMessage = "An unexpected error occurred.";
   Snackbar.Add(errorMessage, Severity.Error);
     // TODO: Log error with ILogger
        }
        finally
        {
            isUpdatingTrip = false;
            StateHasChanged();
 }
    }

    /// <summary>
    /// Wrapper method to trigger form submission from external button.
    /// </summary>
    private async Task HandleUpdateTripClick()
    {
        if (tripFormRef != null)
        {
 await tripFormRef.SubmitAsync();
        }
 else
 {
    Snackbar.Add("Form is not ready. Please try again.", Severity.Warning);
  }
    }

    /// <summary>
    /// Handles CanSubmit state changes from TripForm.
  /// </summary>
    private void HandleCanSubmitChanged(bool canSubmit)
    {
        canSubmitTrip = canSubmit;
        StateHasChanged();
    }

    /// <summary>
    /// Handles trip deletion with confirmation dialog.
    /// Redirects to trip list after successful deletion.
    /// </summary>
    private async Task HandleDeleteTrip()
    {
      if (trip == null) return;

        var parameters = new DialogParameters<DeleteTripConfirmationDialog>
        {
 { x => x.TripName, trip.Name }
        };

  var dialog = await DialogService.ShowAsync<DeleteTripConfirmationDialog>(
      "Confirm Deletion",
    parameters,
      new DialogOptions { MaxWidth = MaxWidth.Small, CloseButton = true });

 var result = await dialog.Result;

        if (result.Canceled) return;

        try
    {
      await TripService.DeleteTripAsync(trip.Id);
            Snackbar.Add($"Trip '{trip.Name}' has been deleted.", Severity.Success);
   NavigationManager.NavigateTo("trips");
      }
 catch (NotFoundException)
        {
  Snackbar.Add("Trip not found.", Severity.Warning);
        NavigationManager.NavigateTo("trips");
        }
        catch (UnauthorizedException)
        {
        Snackbar.Add("Session expired. Please log in again.", Severity.Warning);
  NavigationManager.NavigateTo("login");
  }
        catch (DatabaseException)
    {
 Snackbar.Add("Failed to delete trip. Please try again.", Severity.Error);
  }
        catch (Exception ex)
    {
   Snackbar.Add("An unexpected error occurred.", Severity.Error);
   // TODO: Log error with ILogger
        }
    }

    #endregion

    #region Companion Event Handlers

    /// <summary>
    /// Handles adding a new companion to the trip.
    /// Validates input, calls API and refreshes companion list.
    /// </summary>
    private async Task HandleAddCompanionAsync(MotoNomad.Application.Commands.Companions.AddCompanionCommand command)
    {
        isAddingCompanion = true;
        showCompanionForm = false;

        try
   {
var companion = await CompanionService.AddCompanionAsync(command);
  
      // Refresh companion list
companions = (await CompanionService.GetCompanionsByTripIdAsync(Id)).ToList();
      
     Snackbar.Add($"Added companion: {companion.FirstName} {companion.LastName}", Severity.Success);
        }
        catch (ValidationException ex)
        {
 Snackbar.Add(ex.Message, Severity.Warning);
       showCompanionForm = true; // Show form again for correction
        }
        catch (DatabaseException)
        {
   Snackbar.Add("Failed to add companion. Please try again.", Severity.Error);
      showCompanionForm = true;
        }
        catch (Exception ex)
      {
       Snackbar.Add("An unexpected error occurred.", Severity.Error);
            // TODO: Log error with ILogger
        }
   finally
        {
    isAddingCompanion = false;
  StateHasChanged();
  }
    }

    /// <summary>
    /// Handles removing a companion from the trip with confirmation dialog.
    /// Refreshes companion list after successful deletion.
    /// </summary>
    private async Task HandleRemoveCompanionAsync(Guid companionId)
    {
        var companion = companions.FirstOrDefault(c => c.Id == companionId);
        if (companion == null) return;

        var parameters = new DialogParameters<DeleteCompanionConfirmationDialog>
        {
  { x => x.FirstName, companion.FirstName },
   { x => x.LastName, companion.LastName }
   };

        var dialog = await DialogService.ShowAsync<DeleteCompanionConfirmationDialog>(
  "Confirm Deletion",
     parameters,
            new DialogOptions { MaxWidth = MaxWidth.Small, CloseButton = true });

   var result = await dialog.Result;

        if (result.Canceled) return;

        try
        {
 await CompanionService.RemoveCompanionAsync(companionId);
            
 // Refresh companion list
  companions = (await CompanionService.GetCompanionsByTripIdAsync(Id)).ToList();
       
   Snackbar.Add("Companion has been removed.", Severity.Success);
        }
      catch (NotFoundException)
        {
  Snackbar.Add("Companion not found.", Severity.Warning);
            // Refresh list (may have been already deleted)
     companions = (await CompanionService.GetCompanionsByTripIdAsync(Id)).ToList();
        }
        catch (DatabaseException)
     {
  Snackbar.Add("Failed to remove companion. Please try again.", Severity.Error);
        }
        catch (Exception ex)
        {
Snackbar.Add("An unexpected error occurred.", Severity.Error);
            // TODO: Log error with ILogger
  }
        finally
    {
            StateHasChanged();
      }
    }

    #endregion
}
