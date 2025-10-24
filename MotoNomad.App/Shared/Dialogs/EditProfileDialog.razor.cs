using Microsoft.AspNetCore.Components;
using MotoNomad.Application.Commands.Profiles;
using MotoNomad.Application.DTOs.Profiles;
using MotoNomad.Application.Exceptions;
using MotoNomad.Application.Interfaces;
using MudBlazor;

namespace MotoNomad.App.Shared.Dialogs;

/// <summary>
/// Dialog for editing user profile information.
/// </summary>
public partial class EditProfileDialog : ComponentBase
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    [Parameter]
    public ProfileDto Profile { get; set; } = default!;

    [Inject]
    private IProfileService ProfileService { get; set; } = default!;

    [Inject]
  private ISnackbar Snackbar { get; set; } = default!;

    [Inject]
    private ILogger<EditProfileDialog> Logger { get; set; } = default!;

    private MudForm _form = default!;
private string? _displayName;
    private string? _avatarUrl;
    private bool _isSaving;
    private Dictionary<string, string[]> _validationErrors = new();

    /// <summary>
    /// Initializes the dialog with current profile data.
/// </summary>
    protected override void OnInitialized()
    {
        _displayName = Profile.DisplayName;
        _avatarUrl = Profile.AvatarUrl;
    }

    /// <summary>
    /// Validates the display name field.
    /// </summary>
    private string? ValidateDisplayName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null; // Optional field
        }

        if (value.Trim().Length > 100)
        {
            return "Display name cannot exceed 100 characters";
        }

        return null;
    }

    /// <summary>
    /// Validates the avatar URL field.
    /// </summary>
private string? ValidateAvatarUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
          return null; // Optional field
        }

        if (value.Length > 500)
        {
            return "Avatar URL cannot exceed 500 characters";
        }

 if (!Uri.TryCreate(value, UriKind.Absolute, out var uriResult) ||
         (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
    {
            return "Avatar URL must be a valid HTTP/HTTPS URL";
      }

  return null;
    }

    /// <summary>
    /// Saves profile changes.
    /// </summary>
    private async Task SaveChanges()
    {
        // Validate form first
        await _form.Validate();

    if (!_form.IsValid)
        {
       Snackbar.Add("Please fix validation errors before saving", Severity.Warning);
            return;
        }

        // Check if any changes were made
        var hasChanges = _displayName?.Trim() != Profile.DisplayName?.Trim() ||
  _avatarUrl?.Trim() != Profile.AvatarUrl?.Trim();

        if (!hasChanges)
        {
     Snackbar.Add("No changes to save", Severity.Info);
            MudDialog.Close(DialogResult.Ok(false));
 return;
      }

        _isSaving = true;
        _validationErrors.Clear();

        try
        {
   var command = new UpdateProfileCommand
       {
                DisplayName = string.IsNullOrWhiteSpace(_displayName) ? null : _displayName.Trim(),
       AvatarUrl = string.IsNullOrWhiteSpace(_avatarUrl) ? null : _avatarUrl.Trim()
  };

    var updatedProfile = await ProfileService.UpdateProfileAsync(command);

      Logger.LogInformation("Profile updated successfully for user {UserId}", updatedProfile.Id);

      Snackbar.Add("Profile updated successfully!", Severity.Success);
       MudDialog.Close(DialogResult.Ok(true));
  }
        catch (ValidationException ex)
        {
            _validationErrors = ex.ValidationErrors;
            Snackbar.Add("Validation failed. Please check the form.", Severity.Warning);
          Logger.LogWarning(ex, "Validation error updating profile");
      }
   catch (UnauthorizedException)
        {
            Snackbar.Add("Please log in to update your profile", Severity.Error);
         Logger.LogWarning("Unauthorized attempt to update profile");
   MudDialog.Close(DialogResult.Cancel());
        }
        catch (DatabaseException ex)
     {
     Snackbar.Add("Failed to update profile. Please try again.", Severity.Error);
            Logger.LogError(ex, "Database error updating profile");
     }
 catch (Exception ex)
        {
          Snackbar.Add("An unexpected error occurred. Please try again.", Severity.Error);
       Logger.LogError(ex, "Unexpected error updating profile");
        }
 finally
      {
            _isSaving = false;
        }
    }

    /// <summary>
    /// Cancels the edit operation.
    /// </summary>
    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
