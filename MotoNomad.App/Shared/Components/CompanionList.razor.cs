using Microsoft.AspNetCore.Components;
using MotoNomad.Application.DTOs.Companions;

namespace MotoNomad.App.Shared.Components;

/// <summary>
/// Component displaying a list of trip companions.
/// Each companion is shown as a list item with name, contact and delete button.
/// </summary>
public partial class CompanionList
{
    /// <summary>
    /// List of companions to display.
    /// </summary>
    [Parameter]
    public List<CompanionListItemDto> Companions { get; set; } = new();

    /// <summary>
    /// Event callback triggered when user clicks delete button.
    /// Returns the companion ID to be removed.
    /// </summary>
    [Parameter]
    public EventCallback<Guid> OnRemove { get; set; }
}
