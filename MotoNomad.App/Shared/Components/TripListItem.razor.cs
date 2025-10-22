using Microsoft.AspNetCore.Components;
using MotoNomad.Application.DTOs.Trips;
using MotoNomad.App.Infrastructure.Database.Entities;
using MudBlazor;

namespace MotoNomad.App.Shared.Components;

/// <summary>
/// Component representing a single trip as a clickable card in the trip list.
/// Displays key information: transport icon, name, dates, duration and companion count.
/// </summary>
public partial class TripListItem
{
    /// <summary>
    /// Trip data to display in the card.
    /// </summary>
    [Parameter]
    public TripListItemDto Trip { get; set; } = null!;

    /// <summary>
    /// Event callback triggered when the card is clicked.
    /// Returns the trip ID for navigation purposes.
    /// </summary>
    [Parameter]
    public EventCallback<Guid> OnTripClick { get; set; }

    /// <summary>
    /// Handles card click event and invokes the OnTripClick callback with trip ID.
    /// </summary>
    private async Task HandleClick()
    {
        await OnTripClick.InvokeAsync(Trip.Id);
    }

    /// <summary>
    /// Gets the appropriate MudBlazor icon based on transport type.
    /// </summary>
    /// <returns>Icon name for the transport type.</returns>
    private string GetTransportIcon() => Trip.TransportType switch
    {
        TransportType.Motorcycle => Icons.Material.Filled.TwoWheeler,
        TransportType.Airplane => Icons.Material.Filled.Flight,
        TransportType.Train => Icons.Material.Filled.Train,
        TransportType.Car => Icons.Material.Filled.DirectionsCar,
        _ => Icons.Material.Filled.TravelExplore
    };

    /// <summary>
    /// Formats duration with proper Polish grammar (dzie?/dni).
    /// </summary>
    /// <returns>Formatted duration string (e.g., "7 dni" or "1 dzie?").</returns>
    private string GetDurationLabel()
    {
        var daysLabel = Trip.DurationDays == 1 ? "day" : "days";
        return $"{Trip.DurationDays} {daysLabel}";
    }

    /// <summary>
    /// Formats companion count with proper Polish grammar.
    /// </summary>
    /// <returns>Formatted companion count string (e.g., "3 companions" or "No companions").</returns>
    private string GetCompanionLabel()
    {
        if (Trip.CompanionCount == 0) return "No companions";
        var label = Trip.CompanionCount == 1 ? "companion" : "companions";
        return $"{Trip.CompanionCount} {label}";
    }
}
