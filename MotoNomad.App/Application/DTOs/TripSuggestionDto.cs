namespace MotoNomad.App.Application.DTOs;

/// <summary>
/// Data Transfer Object for AI-generated trip suggestions.
/// </summary>
public class TripSuggestionDto
{
    /// <summary>
    /// Gets or sets the main description of the suggested trip route.
    /// </summary>
    public string SuggestedDescription { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the top 3-5 highlights and attractions of the trip.
    /// </summary>
    public List<string> Highlights { get; set; } = new();
}
