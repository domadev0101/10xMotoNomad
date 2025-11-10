using MudBlazor;
using Microsoft.AspNetCore.Components;

namespace MotoNomad.App.Pages.Dev;

/// <summary>
/// Kitchen Sink page - demonstrates all MudBlazor components used in MotoNomad application.
/// </summary>
public partial class KitchenSink
{
    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    /// <summary>
    /// Demonstrates MudSnackbar by showing a sample notification.
    /// </summary>
    private void ShowSnackbar()
    {
        Snackbar.Add("This is a sample Snackbar!", Severity.Success);
    }
}