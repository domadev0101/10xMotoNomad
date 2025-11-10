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
    private bool _firstNameTouched = false;
    private bool _lastNameTouched = false;
    private bool _contactTouched = false;

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
    /// Custom validation function for first name field.
    /// Ensures first name is not empty and not longer than 100 characters.
    /// Only shows errors after user interaction.
    /// </summary>
    private Func<string, string?> ValidateFirstName => (firstName) =>
    {
        // Don't show errors until user has interacted with the field
        if (!_firstNameTouched)
            return null;

        if (string.IsNullOrWhiteSpace(firstName))
            return "First name is required";

        if (firstName.Length > 100)
            return "First name cannot exceed 100 characters";

        return null;
    };

    /// <summary>
    /// Custom validation function for last name field.
    /// Ensures last name is not empty and not longer than 100 characters.
    /// Only shows errors after user interaction.
    /// </summary>
    private Func<string, string?> ValidateLastName => (lastName) =>
    {
        // Don't show errors until user has interacted with the field
        if (!_lastNameTouched)
            return null;

        if (string.IsNullOrWhiteSpace(lastName))
            return "Last name is required";

        if (lastName.Length > 100)
            return "Last name cannot exceed 100 characters";

        return null;
    };

    /// <summary>
    /// Custom validation function for contact field (optional).
    /// Ensures contact does not exceed 255 characters.
    /// Only shows errors after user interaction.
    /// </summary>
    private Func<string?, string?> ValidateContact => (contact) =>
    {
        // Don't show errors until user has interacted with the field
        if (!_contactTouched)
            return null;

        if (!string.IsNullOrEmpty(contact) && contact.Length > 255)
            return "Contact cannot exceed 255 characters";

        return null;
    };

    /// <summary>
    /// Callback triggered when FirstName changes. Re-validates the form.
    /// </summary>
    private async Task OnFirstNameChanged(string value)
    {
        _firstNameTouched = true;
        model.FirstName = value;
        if (form != null)
        {
            await form.Validate();
        }
        StateHasChanged();
    }

    /// <summary>
    /// Callback triggered when LastName changes. Re-validates the form.
    /// </summary>
    private async Task OnLastNameChanged(string value)
    {
        _lastNameTouched = true;
        model.LastName = value;
        if (form != null)
        {
            await form.Validate();
        }
        StateHasChanged();
    }

    /// <summary>
    /// Callback triggered when Contact changes. Re-validates the form.
    /// </summary>
    private async Task OnContactChanged(string? value)
    {
        _contactTouched = true;
        model.Contact = value;
        if (form != null)
        {
            await form.Validate();
        }
        StateHasChanged();
    }

    /// <summary>
    /// Handles form submission. Validates form, creates AddCompanionCommand
    /// and invokes OnSubmit callback. Resets form after successful submit.
    /// </summary>
    private async Task HandleSubmit()
    {
        // Mark all fields as touched before validation
        _firstNameTouched = true;
        _lastNameTouched = true;
        _contactTouched = true;

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
        _firstNameTouched = false;
        _lastNameTouched = false;
        _contactTouched = false;
        await form.ResetAsync();
    }
}
