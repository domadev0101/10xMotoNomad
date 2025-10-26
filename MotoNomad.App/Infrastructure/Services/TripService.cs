using MotoNomad.Application.Commands.Trips;
using MotoNomad.Application.DTOs.Companions;
using MotoNomad.Application.DTOs.Trips;
using MotoNomad.Application.Exceptions;
using MotoNomad.Application.Interfaces;
using MotoNomad.App.Application.Interfaces;
using MotoNomad.App.Infrastructure.Database.Entities;

namespace MotoNomad.Infrastructure.Services;

/// <summary>
/// Trip service implementation for managing trips with business logic validation.
/// </summary>
public class TripService : ITripService
{
    private readonly ISupabaseClientService _supabaseClient;
    private readonly ILogger<TripService> _logger;

    public TripService(
        ISupabaseClientService supabaseClient,
        ILogger<TripService> logger)
    {
        _supabaseClient = supabaseClient ?? throw new ArgumentNullException(nameof(supabaseClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<TripListItemDto>> GetAllTripsAsync()
    {
        var userId = GetCurrentUserId();

        try
        {
            var client = _supabaseClient.GetClient();
            
            var trips = await client
                .From<Trip>()
                .Where(t => t.UserId == userId)
                .Order(t => t.StartDate, Postgrest.Constants.Ordering.Descending)
                .Get();

            if (trips?.Models == null || !trips.Models.Any())
            {
                return Enumerable.Empty<TripListItemDto>();
            }

            // Get companion counts for each trip
            var tripIds = trips.Models.Select(t => t.Id).ToList();
            var companions = await client
                .From<Companion>()
                .Where(c => tripIds.Contains(c.TripId))
                .Get();

            var companionCounts = companions?.Models?
                .GroupBy(c => c.TripId)
                .ToDictionary(g => g.Key, g => g.Count())
                ?? new Dictionary<Guid, int>();

            _logger.LogInformation("Retrieved {Count} trips for user {UserId}", trips.Models.Count, userId);

            return trips.Models.Select(trip => MapToListItemDto(trip, companionCounts.GetValueOrDefault(trip.Id, 0)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve trips for user {UserId}", userId);
            throw new DatabaseException("GetAllTrips", "Failed to retrieve trips from database.", ex);
        }
    }

    public async Task<TripDetailDto> GetTripByIdAsync(Guid tripId)
    {
        var userId = GetCurrentUserId();

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

            // Get companions for this trip
            var companions = await client
                .From<Companion>()
                .Where(c => c.TripId == tripId)
                .Get();

            var companionDtos = companions?.Models?
                .Select(MapToCompanionListItemDto)
                .ToList() ?? new List<CompanionListItemDto>();

            _logger.LogInformation("Retrieved trip {TripId} with {CompanionCount} companions", tripId, companionDtos.Count);

            return MapToDetailDto(trip, companionDtos);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve trip {TripId}", tripId);
            throw new DatabaseException("GetTripById", "Failed to retrieve trip from database.", ex);
        }
    }

    public async Task<TripDetailDto> CreateTripAsync(CreateTripCommand command)
    {    
        // Validate command
        ValidateCreateTripCommand(command);

        var userId = GetCurrentUserId();

        try
        {
            var client = _supabaseClient.GetClient();

            var trip = new Trip
          {
        Id = Guid.NewGuid(),
       UserId = userId,
                Name = command.Name.Trim(),
         StartDate = command.StartDate,
           EndDate = command.EndDate,
   Description = command.Description?.Trim(),
       TransportType = command.TransportType,
            CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
       };

            var response = await client
                .From<Trip>()
                .Insert(trip);

            var createdTrip = response?.Models?.FirstOrDefault();
            if (createdTrip == null)
            {
                throw new DatabaseException("CreateTrip", "Failed to create trip - no response from database.");
            }

            _logger.LogInformation("Created trip {TripId} for user {UserId}", createdTrip.Id, userId);

            return MapToDetailDto(createdTrip, new List<CompanionListItemDto>());
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create trip for user {UserId}", userId);
            throw new DatabaseException("CreateTrip", "Failed to create trip in database.", ex);
        }
    }

    public async Task<TripDetailDto> UpdateTripAsync(UpdateTripCommand command)
    {
        // Validate command
        ValidateUpdateTripCommand(command);

        var userId = GetCurrentUserId();        

        try
        {
            var client = _supabaseClient.GetClient();
            
            // Fetch existing trip
            var existingTrip = await client
                .From<Trip>()
                .Where(t => t.Id == command.Id && t.UserId == userId)
                .Single();

            if (existingTrip == null)
            {
                throw new NotFoundException("Trip", command.Id);
            }

            // Update trip properties
            existingTrip.Name = command.Name.Trim();
            existingTrip.StartDate = command.StartDate;
            existingTrip.EndDate = command.EndDate;
            existingTrip.Description = command.Description?.Trim();
            existingTrip.TransportType = command.TransportType;
            existingTrip.UpdatedAt = DateTime.UtcNow;

            await client
                .From<Trip>()
                .Update(existingTrip);

            // Get companions for the updated trip
            var companions = await client
                .From<Companion>()
                .Where(c => c.TripId == command.Id)
                .Get();

            var companionDtos = companions?.Models?
                .Select(MapToCompanionListItemDto)
                .ToList() ?? new List<CompanionListItemDto>();

            _logger.LogInformation("Updated trip {TripId}", command.Id);

            return MapToDetailDto(existingTrip, companionDtos);
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
            _logger.LogError(ex, "Failed to update trip {TripId}", command.Id);
            throw new DatabaseException("UpdateTrip", "Failed to update trip in database.", ex);
        }
    }

    public async Task DeleteTripAsync(Guid tripId)
    {
        var userId = GetCurrentUserId();

        try
        {
            var client = _supabaseClient.GetClient();
            
            // Verify trip exists and belongs to user
            var existingTrip = await client
                .From<Trip>()
                .Where(t => t.Id == tripId && t.UserId == userId)
                .Single();

            if (existingTrip == null)
            {
                throw new NotFoundException("Trip", tripId);
            }

            // Delete trip (companions will be cascade deleted by database)
            await client
                .From<Trip>()
                .Where(t => t.Id == tripId)
                .Delete();

            _logger.LogInformation("Deleted trip {TripId}", tripId);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete trip {TripId}", tripId);
            throw new DatabaseException("DeleteTrip", "Failed to delete trip from database.", ex);
        }
    }

    public async Task<IEnumerable<TripListItemDto>> GetUpcomingTripsAsync()
    {
        var userId = GetCurrentUserId();
     var today = DateOnly.FromDateTime(DateTime.UtcNow);

        try
    {
        var client = _supabaseClient.GetClient();
    
            var trips = await client
     .From<Trip>()
         .Where(t => t.UserId == userId)
        .Order(t => t.StartDate, Postgrest.Constants.Ordering.Ascending)
      .Get();

if (trips?.Models == null || !trips.Models.Any())
   {
           return Enumerable.Empty<TripListItemDto>();
       }

    // Filter upcoming trips (start date >= today)
        var upcomingTrips = trips.Models.Where(t => t.StartDate >= today).ToList();

            if (!upcomingTrips.Any())
   {
    return Enumerable.Empty<TripListItemDto>();
    }

      // Get companion counts for each trip
            var companionCounts = new Dictionary<Guid, int>();
            foreach (var trip in upcomingTrips)
       {
       var companions = await client
         .From<Companion>()
        .Where(c => c.TripId == trip.Id)
          .Get();
      
      companionCounts[trip.Id] = companions?.Models?.Count ?? 0;
            }

            _logger.LogInformation("Retrieved {Count} upcoming trips for user {UserId}", upcomingTrips.Count, userId);

          return upcomingTrips.Select(trip => MapToListItemDto(trip, companionCounts.GetValueOrDefault(trip.Id, 0)));
        }
        catch (Exception ex)
        {
 _logger.LogError(ex, "Failed to retrieve upcoming trips for user {UserId}", userId);
          throw new DatabaseException("GetUpcomingTrips", "Failed to retrieve upcoming trips from database.", ex);
        }
    }

    public async Task<IEnumerable<TripListItemDto>> GetPastTripsAsync()
    {
        var userId = GetCurrentUserId();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        try
  {
      var client = _supabaseClient.GetClient();
         
       var trips = await client
     .From<Trip>()
   .Where(t => t.UserId == userId)
      .Order(t => t.StartDate, Postgrest.Constants.Ordering.Descending)
         .Get();

      if (trips?.Models == null || !trips.Models.Any())
            {
    return Enumerable.Empty<TripListItemDto>();
 }

            // Filter past trips (start date < today)
          var pastTrips = trips.Models.Where(t => t.StartDate < today).ToList();

    if (!pastTrips.Any())
         {
      return Enumerable.Empty<TripListItemDto>();
        }

    // Get companion counts for each trip
            var companionCounts = new Dictionary<Guid, int>();
         foreach (var trip in pastTrips)
    {
             var companions = await client
 .From<Companion>()
     .Where(c => c.TripId == trip.Id)
    .Get();
      
       companionCounts[trip.Id] = companions?.Models?.Count ?? 0;
         }

            _logger.LogInformation("Retrieved {Count} past trips for user {UserId}", pastTrips.Count, userId);

            return pastTrips.Select(trip => MapToListItemDto(trip, companionCounts.GetValueOrDefault(trip.Id, 0)));
        }
        catch (Exception ex)
     {
            _logger.LogError(ex, "Failed to retrieve past trips for user {UserId}", userId);
 throw new DatabaseException("GetPastTrips", "Failed to retrieve past trips from database.", ex);
        }
    }

    #region Validation

    private void ValidateCreateTripCommand(CreateTripCommand command)
    {
        var errors = new Dictionary<string, string[]>();

        // Name validation
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            errors["Name"] = new[] { "Trip name is required" };
        }
        else if (command.Name.Trim().Length > 200)
        {
            errors["Name"] = new[] { "Trip name cannot exceed 200 characters" };
        }

        // Date validation
        if (command.EndDate <= command.StartDate)
        {
            errors["EndDate"] = new[] { "End date must be after start date" };
        }

        // Description validation
        if (!string.IsNullOrWhiteSpace(command.Description) && command.Description.Length > 2000)
        {
            errors["Description"] = new[] { "Description cannot exceed 2000 characters" };
        }

        // Transport type validation
        if (!Enum.IsDefined(typeof(TransportType), command.TransportType))
        {
            errors["TransportType"] = new[] { "Invalid transport type" };
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }

    private void ValidateUpdateTripCommand(UpdateTripCommand command)
    {
        var errors = new Dictionary<string, string[]>();

        // Name validation
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            errors["Name"] = new[] { "Trip name is required" };
        }
        else if (command.Name.Trim().Length > 200)
        {
            errors["Name"] = new[] { "Trip name cannot exceed 200 characters" };
        }

        // Date validation
        if (command.EndDate <= command.StartDate)
        {
            errors["EndDate"] = new[] { "End date must be after start date" };
        }

        // Description validation
        if (!string.IsNullOrWhiteSpace(command.Description) && command.Description.Length > 2000)
        {
            errors["Description"] = new[] { "Description cannot exceed 2000 characters" };
        }

        // Transport type validation
        if (!Enum.IsDefined(typeof(TransportType), command.TransportType))
        {
            errors["TransportType"] = new[] { "Invalid transport type" };
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }

    #endregion

    #region Mapping

    private static TripListItemDto MapToListItemDto(Trip trip, int companionCount)
    {
        return new TripListItemDto
        {
            Id = trip.Id,
            Name = trip.Name,
            StartDate = trip.StartDate,
            EndDate = trip.EndDate,
            DurationDays = trip.EndDate.DayNumber - trip.StartDate.DayNumber,
            TransportType = trip.TransportType,
            CompanionCount = companionCount,
            CreatedAt = trip.CreatedAt
        };
    }

    private static TripDetailDto MapToDetailDto(Trip trip, List<CompanionListItemDto> companions)
    {
        return new TripDetailDto
        {
            Id = trip.Id,
            UserId = trip.UserId,
            Name = trip.Name,
            StartDate = trip.StartDate,
            EndDate = trip.EndDate,
            Description = trip.Description,
            TransportType = trip.TransportType,
            DurationDays = trip.EndDate.DayNumber - trip.StartDate.DayNumber,
            Companions = companions,
            CreatedAt = trip.CreatedAt,
            UpdatedAt = trip.UpdatedAt
        };
    }

    private static CompanionListItemDto MapToCompanionListItemDto(Companion companion)
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

    #endregion

    #region Helper Methods

    private Guid GetCurrentUserId()
    {
        var client = _supabaseClient.GetClient();
        var currentUser = client.Auth.CurrentUser;

        if (currentUser == null)
        {
            throw new UnauthorizedException("You must be logged in to manage trips.");
        }

        return Guid.Parse(currentUser.Id);
    }

    #endregion
}
