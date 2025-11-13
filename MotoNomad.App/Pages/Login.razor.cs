using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MotoNomad.Application.Commands.Auth;
using MotoNomad.Application.Exceptions;
using MotoNomad.Application.Interfaces;
using MudBlazor;

namespace MotoNomad.App.Pages;

/// <summary>
/// Login page code-behind - handles user authentication.
/// </summary>
public partial class Login
{
    #region Dependency Injection

    [Inject] private IAuthService AuthService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    #endregion

    #region State

    private MudForm _form = default!;
    private MudTextField<string> _emailField = default!;
    private LoginViewModel _model = new();
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
    /// Handles login form submission.
    /// Validates credentials, calls AuthService, and redirects on success.
    /// </summary>
    private async Task HandleLoginAsync()
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
            var command = new LoginCommand
            {
                Email = _model.Email.Trim(),
                Password = _model.Password
            };

            // Call AuthService
            // Authentication state change is handled automatically by CustomAuthenticationStateProvider listener
            var user = await AuthService.LoginAsync(command);

            // Success
            Snackbar.Add("Login successful!", Severity.Success);

            // Navigate to trips page
            NavigationManager.NavigateTo("trips");
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
    /// View model for login form data.
    /// </summary>
    private class LoginViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    #endregion
}
