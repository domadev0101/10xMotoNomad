using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MotoNomad.Application.Commands.Auth;
using MotoNomad.Application.Exceptions;
using MotoNomad.Application.Interfaces;
using MotoNomad.App.Infrastructure.Auth;
using MudBlazor;

namespace MotoNomad.App.Pages;

/// <summary>
/// Register page code-behind - handles user registration.
/// </summary>
public partial class Register
{
    #region Dependency Injection

    [Inject] private IAuthService AuthService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

    #endregion

    #region State

    private MudForm _form = default!;
    private MudTextField<string> _emailField = default!;
    private RegisterViewModel _model = new();
    private bool _isLoading = false;
    private bool _formValid = false;
    private string? _errorMessage = null;

    #endregion

    #region Lifecycle Methods

    /// <summary>
    /// Initializes component - redirects if user is already authenticated.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        // Redirect if already logged in
        if (await AuthService.IsAuthenticatedAsync())
        {
            NavigationManager.NavigateTo("trips");
        }
    }

    /// <summary>
    /// Sets focus on email field after first render.
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && _emailField != null)
        {
            await _emailField.FocusAsync();
        }
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Handles registration form submission.
    /// Validates input, calls AuthService, and redirects to login page on success.
    /// </summary>
    private async Task HandleRegisterAsync()
    {
        // Validate form
        await _form.Validate();
        if (!_form.IsValid)
        {
            return;
        }

        // Start loading
        _isLoading = true;
        _errorMessage = null;

        try
        {
            // Create command
            var command = new RegisterCommand
            {
                Email = _model.Email.Trim(),
                Password = _model.Password,
                DisplayName = string.IsNullOrWhiteSpace(_model.DisplayName)
                       ? null
            : _model.DisplayName.Trim()
            };

            // Call AuthService - creates account but does NOT automatically log in
            var user = await AuthService.RegisterAsync(command);

            // Success - show message and redirect to login
            var welcomeMessage = string.IsNullOrEmpty(user.DisplayName)
        ? $"Welcome, {user.Email}!"
    : $"Welcome, {user.DisplayName}!";

            Snackbar.Add($"{welcomeMessage} Your account has been created. Please log in.", Severity.Success);

            // Redirect to login page (user needs to manually log in)
            NavigationManager.NavigateTo("login");
        }
        catch (AuthException ex)
        {
            _errorMessage = ex.Message;
            Snackbar.Add(ex.Message, Severity.Error);
        }
        catch (ValidationException ex)
        {
            _errorMessage = ex.Message;
            Snackbar.Add(ex.Message, Severity.Warning);
        }
        catch (Exception ex)
        {
            _errorMessage = "An unexpected error occurred. Please try again.";
            Snackbar.Add(_errorMessage, Severity.Error);
            // TODO: Log exception
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    #endregion

    #region Validation

    /// <summary>
    /// Validates email address format and length.
    /// </summary>
    private Func<string, string?> EmailValidation => (value) =>
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Email is required";
        }

        if (!IsValidEmail(value))
        {
            return "Invalid email format";
        }

        if (value.Length > 255)
        {
            return "Email cannot exceed 255 characters";
        }

        return null;
    };

    /// <summary>
    /// Validates password length requirements.
    /// </summary>
    private Func<string, string?> PasswordValidation => (value) =>
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Password is required";
        }

        if (value.Length < 8)
        {
            return "Password must be at least 8 characters";
        }

        if (value.Length > 100)
        {
            return "Password cannot exceed 100 characters";
        }

        return null;
    };

    /// <summary>
    /// Custom validation function to ensure password and confirm password match.
    /// </summary>
    private Func<string, string?> MatchPasswordValidation => (value) =>
    {
        if (string.IsNullOrEmpty(value))
        {
            return "Password confirmation is required";
        }

        if (value != _model.Password)
        {
            return "Passwords must match";
        }

        return null;
    };

    /// <summary>
    /// Validates display name length (optional field).
    /// </summary>
    private Func<string, string?> DisplayNameValidation => (value) =>
    {
        if (!string.IsNullOrWhiteSpace(value) && value.Length > 100)
        {
            return "Display name cannot exceed 100 characters";
        }

        return null;
    };

    /// <summary>
    /// Validates email format using MailAddress.
    /// </summary>
    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region View Models

    /// <summary>
    /// View model for registration form data.
    /// </summary>
    private class RegisterViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
    }

    #endregion
}
