using MotoNomad.Application.Commands.Companions;
using MotoNomad.Application.DTOs.Companions;
using MotoNomad.Application.Exceptions;
using MotoNomad.Application.Interfaces;
using MotoNomad.App.Application.Interfaces;
using MotoNomad.App.Infrastructure.Database.Entities;

namespace MotoNomad.Infrastructure.Services;

/// <summary>
/// Companion service implementation for managing trip participants.
/// </summary>
public class CompanionService : ICompanionService
{
    private readonly ISupabaseClientService _supabaseClient;
    private readonly ILogger<CompanionService> _logger;

    public CompanionService(
        ISupabaseClientService supabaseClient,
        ILogger<CompanionService> logger)
    {
        _supabaseClient = supabaseClient ?? throw new ArgumentNullException(nameof(supabaseClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<CompanionListItemDto>> GetCompanionsByTripIdAsync(Guid tripId)
    {
        var userId = GetCurrentUserId();

        // Verify trip belongs to user
        await VerifyTripOwnership(tripId, userId);

        try
        {
            var client = _supabaseClient.GetClient();

            var companions = await client
                .From<Companion>()
                .Where(c => c.TripId == tripId)
                .Order(c => c.FirstName, Postgrest.Constants.Ordering.Ascending)
                .Get();

            if (companions?.Models == null || !companions.Models.Any())
            {
                return Enumerable.Empty<CompanionListItemDto>();
            }

            _logger.LogInformation("Retrieved {Count} companions for trip {TripId}", companions.Models.Count, tripId);

            return companions.Models.Select(MapToListItemDto);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve companions for trip {TripId}", tripId);
            throw new DatabaseException("GetCompanionsByTripId", "Failed to retrieve companions from database.", ex);
        }
    }

    public async Task<CompanionDto> GetCompanionByIdAsync(Guid companionId)
    {
        var userId = GetCurrentUserId();

        try
        {
            var client = _supabaseClient.GetClient();

            var companion = await client
                .From<Companion>()
                .Where(c => c.Id == companionId)
                .Single();

            if (companion == null)
            {
                throw new NotFoundException("Companion", companionId);
            }

            // Verify trip belongs to user
            await VerifyTripOwnership(companion.TripId, userId);

            _logger.LogInformation("Retrieved companion {CompanionId}", companionId);

            return MapToDto(companion);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (UnauthorizedException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve companion {CompanionId}", companionId);
            throw new DatabaseException("GetCompanionById", "Failed to retrieve companion from database.", ex);
        }
    }

    public async Task<CompanionDto> AddCompanionAsync(AddCompanionCommand command)
    {
        var userId = GetCurrentUserId();

        // Validate command
        ValidateAddCompanionCommand(command);

        // Verify trip exists and belongs to user
        await VerifyTripOwnership(command.TripId, userId);

        try
        {
            var client = _supabaseClient.GetClient();

            var companion = new Companion
            {
                Id = Guid.NewGuid(),
                TripId = command.TripId,
                UserId = null, // MVP doesn't link companions to users
                FirstName = command.FirstName.Trim(),
                LastName = command.LastName.Trim(),
                Contact = string.IsNullOrWhiteSpace(command.Contact) ? string.Empty : command.Contact.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            var response = await client
                .From<Companion>()
                .Insert(companion);

            var createdCompanion = response?.Models?.FirstOrDefault();
            if (createdCompanion == null)
            {
                throw new DatabaseException("AddCompanion", "Failed to add companion - no response from database.");
            }

            _logger.LogInformation("Added companion {CompanionId} to trip {TripId}", createdCompanion.Id, command.TripId);

            return MapToDto(createdCompanion);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add companion to trip {TripId}", command.TripId);
            throw new DatabaseException("AddCompanion", "Failed to add companion to database.", ex);
        }
    }

    public async Task<CompanionDto> UpdateCompanionAsync(UpdateCompanionCommand command)
    {
        var userId = GetCurrentUserId();

        // Validate command
        ValidateUpdateCompanionCommand(command);

        try
        {
            var client = _supabaseClient.GetClient();

            // Fetch existing companion
            var existingCompanion = await client
                .From<Companion>()
                .Where(c => c.Id == command.Id)
                .Single();

            if (existingCompanion == null)
            {
                throw new NotFoundException("Companion", command.Id);
            }

            // Verify trip belongs to user
            await VerifyTripOwnership(existingCompanion.TripId, userId);

            // Update companion properties
            existingCompanion.FirstName = command.FirstName.Trim();
            existingCompanion.LastName = command.LastName.Trim();
            existingCompanion.Contact = string.IsNullOrWhiteSpace(command.Contact) ? string.Empty : command.Contact.Trim();

            await client
                .From<Companion>()
                .Update(existingCompanion);

            _logger.LogInformation("Updated companion {CompanionId}", command.Id);

            return MapToDto(existingCompanion);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (UnauthorizedException)
        {
            throw;
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update companion {CompanionId}", command.Id);
            throw new DatabaseException("UpdateCompanion", "Failed to update companion in database.", ex);
        }
    }

    public async Task RemoveCompanionAsync(Guid companionId)
    {
        var userId = GetCurrentUserId();

        try
        {
            var client = _supabaseClient.GetClient();

            // Fetch existing companion
            var existingCompanion = await client
                .From<Companion>()
                .Where(c => c.Id == companionId)
                .Single();

            if (existingCompanion == null)
            {
                throw new NotFoundException("Companion", companionId);
            }

            // Verify trip belongs to user
            await VerifyTripOwnership(existingCompanion.TripId, userId);

            // Delete companion
            await client
                .From<Companion>()
                .Where(c => c.Id == companionId)
                .Delete();

            _logger.LogInformation("Removed companion {CompanionId}", companionId);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (UnauthorizedException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove companion {CompanionId}", companionId);
            throw new DatabaseException("RemoveCompanion", "Failed to remove companion from database.", ex);
        }
    }

    public async Task<int> GetCompanionCountAsync(Guid tripId)
    {
        var userId = GetCurrentUserId();

        // Verify trip belongs to user
        await VerifyTripOwnership(tripId, userId);

        try
        {
            var client = _supabaseClient.GetClient();

            var companions = await client
                .From<Companion>()
                .Where(c => c.TripId == tripId)
                .Get();

            var count = companions?.Models?.Count ?? 0;

            _logger.LogInformation("Counted {Count} companions for trip {TripId}", count, tripId);

            return count;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to count companions for trip {TripId}", tripId);
            throw new DatabaseException("GetCompanionCount", "Failed to count companions from database.", ex);
        }
    }

    #region Validation

    private void ValidateAddCompanionCommand(AddCompanionCommand command)
    {
        var errors = new Dictionary<string, string[]>();

        // First name validation
        if (string.IsNullOrWhiteSpace(command.FirstName))
        {
            errors["FirstName"] = new[] { "First name is required" };
        }
        else if (command.FirstName.Trim().Length > 100)
        {
            errors["FirstName"] = new[] { "First name cannot exceed 100 characters" };
        }

        // Last name validation
        if (string.IsNullOrWhiteSpace(command.LastName))
        {
            errors["LastName"] = new[] { "Last name is required" };
        }
        else if (command.LastName.Trim().Length > 100)
        {
            errors["LastName"] = new[] { "Last name cannot exceed 100 characters" };
        }

        // Contact validation (optional but if provided must be valid)
        if (!string.IsNullOrWhiteSpace(command.Contact) && command.Contact.Length > 255)
        {
            errors["Contact"] = new[] { "Contact information cannot exceed 255 characters" };
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }

    private void ValidateUpdateCompanionCommand(UpdateCompanionCommand command)
    {
        var errors = new Dictionary<string, string[]>();

        // First name validation
        if (string.IsNullOrWhiteSpace(command.FirstName))
        {
            errors["FirstName"] = new[] { "First name is required" };
        }
        else if (command.FirstName.Trim().Length > 100)
        {
            errors["FirstName"] = new[] { "First name cannot exceed 100 characters" };
        }

        // Last name validation
        if (string.IsNullOrWhiteSpace(command.LastName))
        {
            errors["LastName"] = new[] { "Last name is required" };
        }
        else if (command.LastName.Trim().Length > 100)
        {
            errors["LastName"] = new[] { "Last name cannot exceed 100 characters" };
        }

        // Contact validation (optional but if provided must be valid)
        if (!string.IsNullOrWhiteSpace(command.Contact) && command.Contact.Length > 255)
        {
            errors["Contact"] = new[] { "Contact information cannot exceed 255 characters" };
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }

    #endregion

    #region Mapping

    private static CompanionListItemDto MapToListItemDto(Companion companion)
    {
        return new CompanionListItemDto
        {
            Id = companion.Id,
            TripId = companion.TripId,
            FirstName = companion.FirstName,
            LastName = companion.LastName,
            Contact = companion.Contact
        };
    }

    private static CompanionDto MapToDto(Companion companion)
    {
        return new CompanionDto
        {
            Id = companion.Id,
            TripId = companion.TripId,
            UserId = companion.UserId,
            FirstName = companion.FirstName,
            LastName = companion.LastName,
            Contact = companion.Contact,
            CreatedAt = companion.CreatedAt
        };
    }

    #endregion

    #region Helper Methods

    private Guid GetCurrentUserId()
    {
        var client = _supabaseClient.GetClient();
        var currentUser = client.Auth.CurrentUser;

        if (currentUser == null)
        {
            throw new UnauthorizedException("You must be logged in to manage companions.");
        }

        return Guid.Parse(currentUser.Id);
    }

    private async Task VerifyTripOwnership(Guid tripId, Guid userId)
    {
        try
        {
            var client = _supabaseClient.GetClient();

            var trip = await client
                .From<Trip>()
                .Where(t => t.Id == tripId && t.UserId == userId)
                .Single();

            if (trip == null)
            {
                throw new NotFoundException("Trip", tripId);
            }
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify trip ownership for trip {TripId}", tripId);
            throw new DatabaseException("VerifyTripOwnership", "Failed to verify trip ownership.", ex);
        }
    }

    #endregion
}
