using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace MotoNomad.App.Shared.Components;

/// <summary>
/// Component for displaying a friendly empty state message when no data is available.
/// </summary>
public partial class EmptyState
{
    /// <summary>
    /// The title text to display.
    /// </summary>
    [Parameter]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The message text to display below the title.
    /// </summary>
    [Parameter]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The MudBlazor icon name to display.
    /// </summary>
    [Parameter]
    public string IconName { get; set; } = Icons.Material.Filled.Info;

    /// <summary>
    /// Optional button text. If provided, a button will be displayed.
    /// </summary>
    [Parameter]
    public string? ButtonText { get; set; }

    /// <summary>
    /// Event callback triggered when the optional button is clicked.
    /// </summary>
    [Parameter]
    public EventCallback OnButtonClick { get; set; }
}
