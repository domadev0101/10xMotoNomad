using MotoNomad.Application.Commands.Profiles;
using MotoNomad.Application.DTOs.Profiles;
using MotoNomad.Application.Exceptions;
using MotoNomad.Application.Interfaces;
using MotoNomad.App.Application.Interfaces;
using MotoNomad.App.Infrastructure.Database.Entities;

namespace MotoNomad.Infrastructure.Services;

/// <summary>
/// Profile service implementation for managing user profiles.
/// </summary>
public class ProfileService : IProfileService
{
    private readonly ISupabaseClientService _supabaseClient;
    private readonly ILogger<ProfileService> _logger;

    public ProfileService(
        ISupabaseClientService supabaseClient,
        ILogger<ProfileService> logger)
    {
        _supabaseClient = supabaseClient ?? throw new ArgumentNullException(nameof(supabaseClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ProfileDto> GetCurrentProfileAsync()
    {
        var client = _supabaseClient.GetClient();
        var currentUser = client.Auth.CurrentUser;

        if (currentUser == null)
        {
            throw new UnauthorizedException("You must be logged in to view your profile.");
        }

        var userId = Guid.Parse(currentUser.Id);

        try
        {
            var profile = await client
                .From<Profile>()
                .Select("*")
                .Filter("id", Postgrest.Constants.Operator.Equals, userId.ToString())
                .Single();

            if (profile == null)
            {
                // Profile should always exist (created by database trigger)
                // If it doesn't, something went wrong during registration
                throw new NotFoundException("Profile", userId);
            }

            _logger.LogInformation("Retrieved profile for user {UserId}", userId);

            return new ProfileDto
            {
                Id = profile.Id,
                Email = profile.Email ?? currentUser.Email ?? string.Empty,
                DisplayName = profile.DisplayName,
                AvatarUrl = profile.AvatarUrl,
                CreatedAt = profile.CreatedAt,
                UpdatedAt = profile.UpdatedAt
            };
        }
        catch (UnauthorizedException)
        {
            throw;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve profile for user {UserId}", userId);
            throw new DatabaseException("GetProfile", "Failed to retrieve profile from database.", ex);
        }
    }

    public async Task<ProfileDto> UpdateProfileAsync(UpdateProfileCommand command)
    {
        // Validate command
        ValidateUpdateProfileCommand(command);

        var client = _supabaseClient.GetClient();
        var currentUser = client.Auth.CurrentUser;

        if (currentUser == null)
        {
            throw new UnauthorizedException("You must be logged in to update your profile.");
        }

        var userId = Guid.Parse(currentUser.Id);

        try
        {
            // Fetch existing profile
            var profile = await client
                .From<Profile>()
                .Select("*")
                .Filter("id", Postgrest.Constants.Operator.Equals, userId.ToString())
                .Single();

            if (profile == null)
            {
                throw new NotFoundException("Profile", userId);
            }

            // Update only provided fields (partial update)
            bool hasChanges = false;

            if (command.DisplayName != null)
            {
                profile.DisplayName = command.DisplayName.Trim();
                hasChanges = true;
            }

            if (command.AvatarUrl != null)
            {
                profile.AvatarUrl = command.AvatarUrl.Trim();
                hasChanges = true;
            }

            if (!hasChanges)
            {
                _logger.LogWarning("UpdateProfile called with no changes for user {UserId}", userId);
                // Return current profile without update
                return new ProfileDto
                {
                    Id = profile.Id,
                    Email = profile.Email ?? currentUser.Email ?? string.Empty,
                    DisplayName = profile.DisplayName,
                    AvatarUrl = profile.AvatarUrl,
                    CreatedAt = profile.CreatedAt,
                    UpdatedAt = profile.UpdatedAt
                };
            }

            profile.UpdatedAt = DateTime.UtcNow;

            await client
                .From<Profile>()
                .Update(profile);

            _logger.LogInformation("Updated profile for user {UserId}", userId);

            return new ProfileDto
            {
                Id = profile.Id,
                Email = profile.Email ?? currentUser.Email ?? string.Empty,
                DisplayName = profile.DisplayName,
                AvatarUrl = profile.AvatarUrl,
                CreatedAt = profile.CreatedAt,
                UpdatedAt = profile.UpdatedAt
            };
        }
        catch (UnauthorizedException)
        {
            throw;
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
            _logger.LogError(ex, "Failed to update profile for user {UserId}", userId);
            throw new DatabaseException("UpdateProfile", "Failed to update profile in database.", ex);
        }
    }

    #region Validation

    private void ValidateUpdateProfileCommand(UpdateProfileCommand command)
    {
        var errors = new Dictionary<string, string[]>();

        // At least one field must be provided
        if (command.DisplayName == null && command.AvatarUrl == null)
        {
            errors["Command"] = new[] { "At least one field must be provided for update" };
        }

        // Display name validation (if provided)
        if (command.DisplayName != null)
        {
            if (string.IsNullOrWhiteSpace(command.DisplayName))
            {
                errors["DisplayName"] = new[] { "Display name cannot be empty" };
            }
            else if (command.DisplayName.Trim().Length > 100)
            {
                errors["DisplayName"] = new[] { "Display name cannot exceed 100 characters" };
            }
        }

        // Avatar URL validation (if provided)
        if (command.AvatarUrl != null)
        {
            if (string.IsNullOrWhiteSpace(command.AvatarUrl))
            {
                errors["AvatarUrl"] = new[] { "Avatar URL cannot be empty" };
            }
            else if (command.AvatarUrl.Length > 500)
            {
                errors["AvatarUrl"] = new[] { "Avatar URL cannot exceed 500 characters" };
            }
            else if (!IsValidUrl(command.AvatarUrl))
            {
                errors["AvatarUrl"] = new[] { "Avatar URL format is invalid" };
            }
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }

    private static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    #endregion
}
