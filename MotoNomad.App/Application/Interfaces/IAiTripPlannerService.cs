using MotoNomad.App.Application.DTOs;
using MotoNomad.App.Application.Exceptions;

namespace MotoNomad.App.Application.Interfaces;

/// <summary>
/// Service interface for AI-powered trip planning and suggestions.
/// </summary>
public interface IAiTripPlannerService
{
    /// <summary>
    /// Generates AI-powered trip suggestions based on trip parameters.
    /// </summary>
    /// <param name="tripName">The name of the trip.</param>
    /// <param name="startDate">The start date of the trip.</param>
    /// <param name="endDate">The end date of the trip.</param>
    /// <param name="transportType">The type of transport (e.g., motorcycle, airplane, train).</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing trip suggestions.</returns>
    /// <exception cref="OpenRouterException">Thrown when the AI service fails to generate suggestions.</exception>
    Task<TripSuggestionDto> GenerateTripSuggestionAsync(
    string tripName,
        DateTime startDate,
     DateTime endDate,
        string transportType,
        CancellationToken cancellationToken = default);
}
