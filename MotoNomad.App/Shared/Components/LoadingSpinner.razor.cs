using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace MotoNomad.App.Shared.Components;

/// <summary>
/// Universal loading spinner component with optional message.
/// </summary>
public partial class LoadingSpinner
{
    /// <summary>
    /// Optional message to display below the spinner.
    /// </summary>
    [Parameter]
    public string? Message { get; set; }

    /// <summary>
    /// Size of the progress circular spinner.
    /// </summary>
    [Parameter]
    public Size Size { get; set; } = Size.Large;
}
