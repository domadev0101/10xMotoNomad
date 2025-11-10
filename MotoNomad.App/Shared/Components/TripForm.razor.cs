using Microsoft.AspNetCore.Components;
using MotoNomad.Application.DTOs.Trips;
using MotoNomad.Application.Commands.Trips;
using MotoNomad.App.Infrastructure.Database.Entities;
using MotoNomad.App.Application.DTOs;
using MudBlazor;
using System.ComponentModel;

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

    /// <summary>
    /// Event callback triggered when CanSubmit state changes.
    /// Used by parent components to update external button states.
    /// </summary>
    [Parameter]
    public EventCallback<bool> OnCanSubmitChanged { get; set; }

    private MudForm form = null!;
    private TripFormViewModel model = new();
    private bool formValid;
    private bool canSubmit = false;
    private bool _nameTouched = false;
    private bool _startDateTouched = false;
    private bool _endDateTouched = false;
    private bool _descriptionTouched = false;

    /// <summary>
    /// Public property indicating if form can be submitted based on validation rules.
    /// Used by parent components to control external submit buttons.
    /// </summary>
    public bool CanSubmit => canSubmit;

    /// <summary>
    /// Validates form and updates canSubmit flag
    /// </summary>
    private void UpdateCanSubmit()
    {
        var previousCanSubmit = canSubmit;

        // Check all required fields
        if (string.IsNullOrWhiteSpace(model.Name) || model.Name.Length > 200)
        {
            canSubmit = false;
        }
        else if (!model.StartDate.HasValue || !model.EndDate.HasValue)
        {
            canSubmit = false;
        }
        else if (model.EndDate.Value <= model.StartDate.Value)
        {
            canSubmit = false;
        }
        else if (!string.IsNullOrEmpty(model.Description) && model.Description.Length > 2000)
        {
            canSubmit = false;
        }
        else
        {
            canSubmit = true;
        }

        // Notify parent if CanSubmit state changed
        if (previousCanSubmit != canSubmit && OnCanSubmitChanged.HasDelegate)
        {
            _ = OnCanSubmitChanged.InvokeAsync(canSubmit);
        }
    }

    /// <summary>
    /// Internal view model for form data binding.
    /// Uses DateTime? for MudDatePicker compatibility (converted to DateOnly before submit).
    /// </summary>
    private class TripFormViewModel : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private DateTime? _startDate;
        private DateTime? _endDate;
        private string? _description;
        private TransportType _transportType = TransportType.Motorcycle;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public DateTime? StartDate
        {
            get => _startDate;
            set
            {
                if (_startDate != value)
                {
                    _startDate = value;
                    OnPropertyChanged(nameof(StartDate));
                }
            }
        }

        public DateTime? EndDate
        {
            get => _endDate;
            set
            {
                if (_endDate != value)
                {
                    _endDate = value;
                    OnPropertyChanged(nameof(EndDate));
                }
            }
        }

        public string? Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

        public TransportType TransportType
        {
            get => _transportType;
            set
            {
                if (_transportType != value)
                {
                    _transportType = value;
                    OnPropertyChanged(nameof(TransportType));
                }
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
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

            // Mark all fields as touched in edit mode (user can see validation immediately)
            _nameTouched = true;
            _startDateTouched = true;
            _endDateTouched = true;
            _descriptionTouched = true;
        }
        // Create mode - fields remain empty (default values)

        // Subscribe to model property changes to trigger UI update
        model.PropertyChanged += OnModelPropertyChanged;

        // Initial validation
        UpdateCanSubmit();
    }

    /// <summary>
    /// Handler for model property changes. Triggers re-render and form validation.
    /// </summary>
    private void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Update CanSubmit whenever any property changes
        UpdateCanSubmit();

        // Always re-render to update UI
        StateHasChanged();

        // Schedule validation asynchronously
        if (form != null && (e.PropertyName == nameof(TripFormViewModel.StartDate) || e.PropertyName == nameof(TripFormViewModel.EndDate)))
        {
            _ = form.Validate();
        }
    }

    /// <summary>
    /// Custom validation function for name field.
    /// Ensures name is not empty and not longer than 200 characters.
    /// Only shows errors after user interaction in create mode.
    /// </summary>
    private Func<string, string?> ValidateName => (name) =>
    {
        // Don't show errors until user has interacted with the field (create mode only)
        if (!_nameTouched)
            return null;

        if (string.IsNullOrWhiteSpace(name))
            return "Trip name is required";

        if (name.Length > 200)
            return "Trip name must not exceed 200 characters";

        return null;
    };

    /// <summary>
    /// Custom validation function for start date field.
    /// Ensures start date is selected.
    /// Only shows errors after user interaction in create mode.
    /// </summary>
    private Func<DateTime?, string?> ValidateStartDate => (startDate) =>
    {
        // Don't show errors until user has interacted with the field (create mode only)
        if (!_startDateTouched)
            return null;

        if (!startDate.HasValue)
            return "Start date is required";

        return null;
    };

    /// <summary>
    /// Custom validation function for end date field.
    /// Ensures end date is after start date.
    /// Only shows errors after user interaction in create mode.
    /// </summary>
    private Func<DateTime?, string?> ValidateEndDate => (endDate) =>
    {
        // Don't show errors until user has interacted with the field (create mode only)
        if (!_endDateTouched)
            return null;

        if (!endDate.HasValue)
            return "End date is required";

        if (!model.StartDate.HasValue)
            return null; // Don't validate if start date not selected yet

        if (endDate.Value <= model.StartDate.Value)
            return "End date must be after start date";

        return null;
    };

    /// <summary>
    /// Custom validation function for description field.
    /// Ensures description is not longer than 2000 characters.
    /// Only shows errors after user interaction in create mode.
    /// </summary>
    private Func<string?, string?> ValidateDescription => (description) =>
    {
        // Don't show errors until user has interacted with the field (create mode only)
        if (!_descriptionTouched)
            return null;

        if (!string.IsNullOrEmpty(description) && description.Length > 2000)
            return "Description must not exceed 2000 characters";

        return null;
    };

    /// <summary>
    /// Handles form submission. Validates form, creates appropriate command object
    /// (CreateTripCommand or UpdateTripCommand) and invokes OnSubmit callback.
    /// </summary>
    private async Task HandleSubmit()
    {
        // Mark all fields as touched before validation
        _nameTouched = true;
        _startDateTouched = true;
        _endDateTouched = true;
        _descriptionTouched = true;

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

    /// <summary>
    /// Callback triggered when Name changes. Re-validates the form.
    /// </summary>
    private async Task OnNameChanged(string value)
    {
        _nameTouched = true;
        model.Name = value;
        UpdateCanSubmit();
        if (form != null)
        {
            await form.Validate();
        }
        StateHasChanged();
    }

    /// <summary>
    /// Callback triggered when StartDate changes. Re-validates the form.
    /// </summary>
    private async Task OnStartDateChanged(DateTime? value)
    {
        _startDateTouched = true;
        model.StartDate = value;
        UpdateCanSubmit();
        if (form != null)
        {
            await form.Validate();
        }
        StateHasChanged();
    }

    /// <summary>
    /// Callback triggered when EndDate changes. Re-validates the form.
    /// </summary>
    private async Task OnEndDateChanged(DateTime? value)
    {
        _endDateTouched = true;
        model.EndDate = value;
        UpdateCanSubmit();
        if (form != null)
        {
            await form.Validate();
        }
        StateHasChanged();
    }

    /// <summary>
    /// Callback triggered when TransportType changes. Re-validates the form.
    /// </summary>
    private async Task OnTransportTypeChanged(TransportType value)
    {
        model.TransportType = value;
        UpdateCanSubmit();
        if (form != null)
        {
            await form.Validate();
        }
        StateHasChanged();
    }

    /// <summary>
    /// Callback triggered when Description changes.
    /// </summary>
    private async Task OnDescriptionChanged(string? value)
    {
        _descriptionTouched = true;
        model.Description = value;
        UpdateCanSubmit();
        if (form != null)
        {
            await form.Validate();
        }
        StateHasChanged();
    }

    /// <summary>
    /// Handles AI-generated trip suggestions and formats them for the description field.
    /// </summary>
    /// <param name="suggestion">The AI-generated trip suggestion.</param>
    private async Task HandleAiSuggestion(TripSuggestionDto suggestion)
    {
        var formattedDescription = suggestion.SuggestedDescription;

        // Add highlights if available
        if (suggestion.Highlights.Any())
        {
            formattedDescription += "\n\nMiejsca warte odwiedzenia:";
            foreach (var highlight in suggestion.Highlights)
            {
                formattedDescription += $"\n- {highlight}";
            }
        }

        // Update description field
        await OnDescriptionChanged(formattedDescription);
    }

    /// <summary>
    /// Gets the display label for a transport type.
    /// </summary>
    /// <param name="transportType">The transport type.</param>
    /// <returns>The display label.</returns>
    private string GetTransportTypeLabel(TransportType transportType) => transportType switch
    {
        TransportType.Motorcycle => "Motorcycle",
        TransportType.Airplane => "Airplane",
        TransportType.Train => "Train",
        TransportType.Car => "Car",
        TransportType.Other => "Other",
        _ => "Unknown"
    };
}
