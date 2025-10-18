using MotoNomad.Application.Commands.Companions;
using MotoNomad.Application.DTOs.Companions;
using MotoNomad.Application.Exceptions;

namespace MotoNomad.Application.Interfaces;

/// <summary>
/// Companion management interface for trip participants.
/// </summary>
public interface ICompanionService
{
    /// <summary>
    /// Retrieves all companions for specific trip.
    /// </summary>
    /// <param name="tripId">Unique trip identifier</param>
    /// <returns>List of companions associated with trip</returns>
    /// <exception cref="UnauthorizedException">User not authenticated or not trip owner</exception>
    /// <exception cref="NotFoundException">Trip does not exist</exception>
    /// <exception cref="DatabaseException">Database query failed</exception>
    Task<IEnumerable<CompanionListItemDto>> GetCompanionsByTripIdAsync(Guid tripId);

    /// <summary>
    /// Retrieves detailed information for specific companion.
    /// </summary>
    /// <param name="companionId">Unique companion identifier</param>
    /// <returns>Complete companion details</returns>
    /// <exception cref="UnauthorizedException">User not authenticated or not trip owner</exception>
    /// <exception cref="NotFoundException">Companion does not exist</exception>
    /// <exception cref="DatabaseException">Database query failed</exception>
    Task<CompanionDto> GetCompanionByIdAsync(Guid companionId);

    /// <summary>
    /// Adds new companion to specific trip with validation.
    /// </summary>
    /// <param name="command">Companion details (trip ID, first name, last name, contact)</param>
    /// <returns>Created companion with generated ID</returns>
    /// <exception cref="UnauthorizedException">User not authenticated or not trip owner</exception>
    /// <exception cref="NotFoundException">Trip does not exist</exception>
    /// <exception cref="ValidationException">Missing required fields or invalid contact format</exception>
    /// <exception cref="DatabaseException">Database insert failed</exception>
    Task<CompanionDto> AddCompanionAsync(AddCompanionCommand command);

    /// <summary>
    /// Updates existing companion information.
    /// </summary>
    /// <param name="command">Companion update details including companion ID</param>
    /// <returns>Updated companion information</returns>
    /// <exception cref="UnauthorizedException">User not authenticated or not trip owner</exception>
    /// <exception cref="NotFoundException">Companion does not exist</exception>
    /// <exception cref="ValidationException">Invalid update data</exception>
    /// <exception cref="DatabaseException">Database update failed</exception>
    Task<CompanionDto> UpdateCompanionAsync(UpdateCompanionCommand command);

    /// <summary>
    /// Removes companion from trip.
    /// </summary>
    /// <param name="companionId">Unique companion identifier</param>
    /// <exception cref="UnauthorizedException">User not authenticated or not trip owner</exception>
    /// <exception cref="NotFoundException">Companion does not exist</exception>
    /// <exception cref="DatabaseException">Database delete failed</exception>
    Task RemoveCompanionAsync(Guid companionId);

    /// <summary>
    /// Counts total companions for specific trip.
    /// </summary>
    /// <param name="tripId">Unique trip identifier</param>
    /// <returns>Number of companions associated with trip</returns>
    /// <exception cref="UnauthorizedException">User not authenticated or not trip owner</exception>
    /// <exception cref="NotFoundException">Trip does not exist</exception>
    /// <exception cref="DatabaseException">Database query failed</exception>
    Task<int> GetCompanionCountAsync(Guid tripId);
}
