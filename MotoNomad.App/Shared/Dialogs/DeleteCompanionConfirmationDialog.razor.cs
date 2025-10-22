using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace MotoNomad.App.Shared.Dialogs;

/// <summary>
/// Code-behind for DeleteCompanionConfirmationDialog component.
/// Displays a confirmation dialog before removing a companion from a trip.
/// </summary>
public partial class DeleteCompanionConfirmationDialog
{
    /// <summary>
    /// Cascading parameter from MudBlazor dialog system.
    /// Provides methods to close the dialog and return results.
    /// </summary>
    [CascadingParameter]
    private IDialogReference Dialog { get; set; } = null!;

    /// <summary>
    /// Companion first name to display in the confirmation message.
    /// </summary>
    [Parameter]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Companion last name to display in the confirmation message.
    /// </summary>
    [Parameter]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Cancels the dialog and returns canceled result.
    /// User clicked "Cancel" button.
    /// </summary>
    private void Cancel() => Dialog.Close(DialogResult.Cancel());

    /// <summary>
    /// Confirms the deletion and returns success result.
    /// User clicked "Delete" button.
    /// </summary>
    private void Confirm() => Dialog.Close(DialogResult.Ok(true));
}
