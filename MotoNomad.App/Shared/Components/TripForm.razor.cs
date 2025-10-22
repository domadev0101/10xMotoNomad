using Microsoft.AspNetCore.Components;
using MotoNomad.Application.DTOs.Trips;
using MotoNomad.Application.Commands.Trips;
using MotoNomad.App.Infrastructure.Database.Entities;
using MudBlazor;

namespace MotoNomad.App.Shared.Components;

/// <summary>
/// Reusable trip form component used for both creating and editing trips.
/// In create mode (Trip=null), fields are empty.
/// In edit mode (Trip!=null), fields are pre-filled with trip data.
/// </summary>
public partial class TripForm
{
    /// <summary>
    /// Trip data for edit mode. Null for create mode.
    /// </summary>
    [Parameter]
    public TripDetailDto? Trip { get; set; }

    /// <summary>
    /// Event callback triggered when form is submitted.
    /// Returns CreateTripCommand (create mode) or UpdateTripCommand (edit mode).
    /// </summary>
    [Parameter]
    public EventCallback<object> OnSubmit { get; set; }

    /// <summary>
    /// Event callback triggered when user cancels the form.
    /// </summary>
    [Parameter]
    public EventCallback OnCancel { get; set; }

    /// <summary>
    /// Indicates whether the form is in loading state (during API call).
    /// </summary>
    [Parameter]
    public bool IsLoading { get; set; }

    /// <summary>
    /// Whether to show action buttons (Save, Cancel) in the form.
    /// Set to false if buttons are rendered by parent component.
    /// </summary>
    [Parameter]
    public bool ShowButtons { get; set; } = true;

    private MudForm form = null!;
    private TripFormViewModel model = new();
    private bool formValid;

    /// <summary>
    /// Internal view model for form data binding.
    /// Uses DateTime? for MudDatePicker compatibility (converted to DateOnly before submit).
    /// </summary>
    private class TripFormViewModel
    {
        public string Name { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Description { get; set; }
        public TransportType TransportType { get; set; } = TransportType.Motorcycle;
    }

    /// <summary>
    /// Initializes form data. In edit mode, pre-fills fields with trip data.
    /// </summary>
    protected override void OnInitialized()
    {
        if (Trip != null)
        {
            // Edit mode - pre-fill form with trip data
            model.Name = Trip.Name;
            model.StartDate = Trip.StartDate.ToDateTime(TimeOnly.MinValue);
            model.EndDate = Trip.EndDate.ToDateTime(TimeOnly.MinValue);
            model.Description = Trip.Description;
            model.TransportType = Trip.TransportType;
        }
        // Create mode - fields remain empty (default values)
    }

    /// <summary>
    /// Custom validation function for end date field.
    /// Ensures end date is after start date.
    /// </summary>
    private Func<DateTime?, string?> ValidateEndDate => (endDate) =>
    {
        if (!endDate.HasValue)
            return "End date is required";
        
        if (!model.StartDate.HasValue)
            return null; // Don't validate if start date not selected yet
        
        if (endDate.Value <= model.StartDate.Value)
            return "End date must be after start date";
        
        return null;
    };

    /// <summary>
    /// Handles form submission. Validates form, creates appropriate command object
    /// (CreateTripCommand or UpdateTripCommand) and invokes OnSubmit callback.
    /// </summary>
    private async Task HandleSubmit()
    {
        await form.Validate();
        if (!form.IsValid) return;

        object command;
        
        if (Trip == null)
        {
            // Create mode - CreateTripCommand
            command = new CreateTripCommand
            {
                Name = model.Name.Trim(),
                StartDate = DateOnly.FromDateTime(model.StartDate!.Value),
                EndDate = DateOnly.FromDateTime(model.EndDate!.Value),
                Description = string.IsNullOrWhiteSpace(model.Description) 
                    ? null 
                    : model.Description.Trim(),
                TransportType = model.TransportType
            };
        }
        else
        {
            // Edit mode - UpdateTripCommand
            command = new UpdateTripCommand
            {
                Id = Trip.Id,
                Name = model.Name.Trim(),
                StartDate = DateOnly.FromDateTime(model.StartDate!.Value),
                EndDate = DateOnly.FromDateTime(model.EndDate!.Value),
                Description = string.IsNullOrWhiteSpace(model.Description) 
                    ? null 
                    : model.Description.Trim(),
                TransportType = model.TransportType
            };
        }

        await OnSubmit.InvokeAsync(command);
    }

    /// <summary>
    /// Public method to trigger form submission from parent component.
    /// Validates and submits the form programmatically.
    /// </summary>
    public async Task SubmitAsync()
    {
        await HandleSubmit();
    }
}
