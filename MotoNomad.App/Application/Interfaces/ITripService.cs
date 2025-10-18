using MotoNomad.Application.Commands.Trips;
using MotoNomad.Application.DTOs.Trips;
using MotoNomad.Application.Exceptions;

namespace MotoNomad.Application.Interfaces;

/// <summary>
/// Trip management and business logic orchestration interface.
/// </summary>
public interface ITripService
{
    /// <summary>
    /// Retrieves all trips for currently authenticated user.
    /// </summary>
    /// <returns>List of trip summaries ordered by start date descending</returns>
    /// <exception cref="UnauthorizedException">User not authenticated</exception>
    /// <exception cref="DatabaseException">Database query failed</exception>
    Task<IEnumerable<TripListItemDto>> GetAllTripsAsync();

    /// <summary>
    /// Retrieves detailed information for specific trip including companions.
    /// </summary>
    /// <param name="tripId">Unique trip identifier</param>
    /// <returns>Complete trip details with companion list</returns>
    /// <exception cref="UnauthorizedException">User not authenticated or not trip owner</exception>
    /// <exception cref="NotFoundException">Trip does not exist</exception>
    /// <exception cref="DatabaseException">Database query failed</exception>
    Task<TripDetailDto> GetTripByIdAsync(Guid tripId);

    /// <summary>
    /// Creates new trip with validation and automatic duration calculation.
    /// </summary>
    /// <param name="command">Trip creation details (name, dates, description, transport type)</param>
    /// <returns>Created trip with generated ID and calculated duration</returns>
    /// <exception cref="UnauthorizedException">User not authenticated</exception>
    /// <exception cref="ValidationException">End date not after start date or invalid transport type</exception>
    /// <exception cref="DatabaseException">Database insert failed</exception>
    Task<TripDetailDto> CreateTripAsync(CreateTripCommand command);

    /// <summary>
    /// Updates existing trip with validation.
    /// </summary>
    /// <param name="command">Trip update details including trip ID</param>
    /// <returns>Updated trip with recalculated duration</returns>
    /// <exception cref="UnauthorizedException">User not authenticated or not trip owner</exception>
    /// <exception cref="NotFoundException">Trip does not exist</exception>
    /// <exception cref="ValidationException">Invalid date range or transport type</exception>
    /// <exception cref="DatabaseException">Database update failed</exception>
    Task<TripDetailDto> UpdateTripAsync(UpdateTripCommand command);

    /// <summary>
    /// Deletes trip and all associated companions via CASCADE.
    /// </summary>
    /// <param name="tripId">Unique trip identifier</param>
    /// <exception cref="UnauthorizedException">User not authenticated or not trip owner</exception>
    /// <exception cref="NotFoundException">Trip does not exist</exception>
    /// <exception cref="DatabaseException">Database delete failed</exception>
    Task DeleteTripAsync(Guid tripId);

    /// <summary>
    /// Retrieves upcoming trips (start date >= today) for current user.
    /// </summary>
    /// <returns>List of upcoming trips ordered by start date ascending</returns>
    /// <exception cref="UnauthorizedException">User not authenticated</exception>
    /// <exception cref="DatabaseException">Database query failed</exception>
    Task<IEnumerable<TripListItemDto>> GetUpcomingTripsAsync();

    /// <summary>
    /// Retrieves past trips (start date < today) for current user.
    /// </summary>
    /// <returns>List of past trips ordered by start date descending</returns>
    /// <exception cref="UnauthorizedException">User not authenticated</exception>
    /// <exception cref="DatabaseException">Database query failed</exception>
    Task<IEnumerable<TripListItemDto>> GetPastTripsAsync();
}
