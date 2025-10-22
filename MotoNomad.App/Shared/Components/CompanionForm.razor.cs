using Microsoft.AspNetCore.Components;
using MotoNomad.Application.Commands.Companions;
using MudBlazor;

namespace MotoNomad.App.Shared.Components;

/// <summary>
/// Form component for adding a new companion to a trip.
/// Contains fields: First Name (required), Last Name (required), Contact (optional).
/// </summary>
public partial class CompanionForm
{
    /// <summary>
    /// The ID of the trip to which the companion will be added.
    /// </summary>
    [Parameter]
    public Guid TripId { get; set; }

    /// <summary>
    /// Event callback triggered when form is submitted.
    /// Returns AddCompanionCommand with form data.
    /// </summary>
    [Parameter]
    public EventCallback<AddCompanionCommand> OnSubmit { get; set; }

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

    private MudForm form = null!;
    private CompanionFormViewModel model = new();
    private bool formValid;

    /// <summary>
    /// Internal view model for form data binding.
    /// </summary>
    private class CompanionFormViewModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Contact { get; set; }
    }

    /// <summary>
    /// Handles form submission. Validates form, creates AddCompanionCommand
    /// and invokes OnSubmit callback. Resets form after successful submit.
    /// </summary>
    private async Task HandleSubmit()
    {
        await form.Validate();
        if (!form.IsValid) return;

        var command = new AddCompanionCommand
        {
            TripId = TripId,
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            Contact = string.IsNullOrWhiteSpace(model.Contact) 
                ? null 
                : model.Contact.Trim()
        };

        await OnSubmit.InvokeAsync(command);
        
        // Reset form after successful submit
        model = new CompanionFormViewModel();
        await form.ResetAsync();
    }
}
