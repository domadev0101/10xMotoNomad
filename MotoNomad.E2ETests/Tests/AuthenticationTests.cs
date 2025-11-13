using Microsoft.Playwright;
using MotoNomad.E2ETests.Configuration;
using MotoNomad.E2ETests.Fixtures;
using MotoNomad.E2ETests.PageObjects;
using Xunit;

namespace MotoNomad.E2ETests.Tests;

/// <summary>
/// E2E tests for authentication flows (login, register, logout).
/// Test cases: TC-AUTH-01, TC-AUTH-02, TC-AUTH-03, TC-AUTH-04
/// </summary>
public class AuthenticationTests : IAsyncLifetime
{
    private PlaywrightFixture _fixture = null!;
    private LoginPage _loginPage = null!;
    private RegisterPage _registerPage = null!;
    private TripsPage _tripsPage = null!;

    public async Task InitializeAsync()
    {
        _fixture = new PlaywrightFixture();
        await _fixture.InitializeAsync();

        // Initialize page objects
        _loginPage = new LoginPage(_fixture.Page!);
        _registerPage = new RegisterPage(_fixture.Page!);
        _tripsPage = new TripsPage(_fixture.Page!);
    }

    public async Task DisposeAsync()
    {
        await _fixture.DisposeAsync();
    }

    /// <summary>
    /// TC-AUTH-01: Successful User Registration (Redirect to Login)
    /// Steps:
    /// 1. Navigate to /register
    /// 2. Fill in valid email, password, and optional display name
    /// 3. Submit the form
    /// 4. Verify redirect to /login page (no automatic login)
    /// Expected: User is successfully registered and redirected to login page
    /// </summary>
    [Fact]
    public async Task TC_AUTH_01_Successful_Registration_Redirect_To_Login()
    {
        // Arrange
        var testEmail = $"test-{Guid.NewGuid()}@motonomad.test";
        var testPassword = "TestPassword123!";
        var displayName = "Test User";

        // Act - Register with valid credentials
        await _registerPage.NavigateAsync();
        await _registerPage.FillEmailAsync(testEmail);
        await _registerPage.FillPasswordAsync(testPassword);
        await _registerPage.FillConfirmPasswordAsync(testPassword);

        if (!string.IsNullOrEmpty(displayName))
        {
            await _registerPage.FillDisplayNameAsync(displayName);
        }

        // Click register button
        await _registerPage.ClickRegisterAsync();

        // Wait for navigation to complete (should redirect to /login)
        await _fixture.Page!.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);
        await Task.Delay(2000); // Give Blazor time to process registration and redirect

        // Assert: Verify redirect to /login after registration (NO automatic login)
        Assert.True(_loginPage.IsOnLoginPage(),
            $"User should be redirected to /login after successful registration. Current URL: {_fixture.Page!.Url}");

        // Optional: Now test that user can actually log in with the new account
        await _loginPage.FillEmailAsync(testEmail);
        await _loginPage.FillPasswordAsync(testPassword);
        await _loginPage.ClickLoginAsync();
        await _loginPage.WaitForSuccessfulLoginAsync();

        Assert.True(_tripsPage.IsOnTripsPage(),
            "User should be able to log in with newly created account");
    }

    /// <summary>
    /// TC-AUTH-02: Registration with an Existing Email
    /// Steps:
    /// 1. Attempt to register with an email that is already in use
    /// Expected: Error message "This email is already registered." is displayed
    /// </summary>
    [Fact]
    public async Task TC_AUTH_02_Registration_With_Existing_Email()
    {
        // Arrange - Use User A which already exists
        var existingEmail = _fixture.Config.UserAEmail;
        var password = "AnyPassword123!";

        // Act: Navigate and fill the form manually (don't use RegisterAsync which waits for redirect)
        await _registerPage.NavigateAsync();
        await _registerPage.FillEmailAsync(existingEmail);
        await _registerPage.FillPasswordAsync(password);
        await _registerPage.FillConfirmPasswordAsync(password);
        await _registerPage.ClickRegisterAsync();

        // Wait for error message to appear
        await Task.Delay(2000);

        // Assert: Error message should be displayed
        var hasError = await _registerPage.HasErrorAsync();
        Assert.True(hasError, "Error message should be displayed for duplicate email");

        var errorMessage = await _registerPage.GetErrorMessageAsync();
        Assert.Contains("already", errorMessage?.ToLower() ?? "",
            StringComparison.OrdinalIgnoreCase);

        // Verify we're still on the register page
        Assert.True(_registerPage.IsOnRegisterPage(),
            "User should remain on /register page when registration fails");
    }

    /// <summary>
    /// TC-AUTH-03: Login with Invalid Credentials
    /// Steps:
    /// 1. Navigate to /login
    /// 2. Enter a valid email but an incorrect password
    /// 3. Submit the form
    /// Expected: Error message "Invalid email or password." is displayed
    /// </summary>
    [Fact]
    public async Task TC_AUTH_03_Login_With_Invalid_Credentials()
    {
        // Arrange
        var validEmail = _fixture.Config.UserAEmail;
        var wrongPassword = "WrongPassword123!";

        // Act: Attempt to login with wrong password
        await _loginPage.LoginAsync(validEmail, wrongPassword);

        // Assert: Error message should be displayed
        var hasError = await _loginPage.HasErrorAsync();
        Assert.True(hasError, "Error message should be displayed");

        var errorMessage = await _loginPage.GetErrorMessageAsync();
        Assert.Contains("invalid", errorMessage?.ToLower() ?? "");

        // Verify still on login page
        Assert.True(_loginPage.IsOnLoginPage(),
            "User should still be on login page after failed login");
    }

    /// <summary>
    /// TC-AUTH-04: Accessing a Protected Route while Unauthorized
    /// Steps:
    /// 1. Ensure the user is logged out
    /// 2. Attempt to navigate directly to /trips or /profile
    /// Expected: User is redirected to the /login page
    /// </summary>
    [Fact]
    public async Task TC_AUTH_04_Accessing_Protected_Route_Unauthorized()
    {
        // Act: Try to navigate directly to protected route without logging in
        await _tripsPage.NavigateAsync();

        // Assert: Should be redirected to login page
        await Task.Delay(1000); // Wait for redirect

        Assert.True(_loginPage.IsOnLoginPage(),
            "Unauthorized user should be redirected to /login when accessing protected route");
    }

    /// <summary>
    /// TC-AUTH-HELPER: Successful login with valid credentials (helper for other tests)
    /// </summary>
    [Fact]
    public async Task TC_AUTH_HELPER_Successful_Login_With_Valid_Credentials()
    {
        // Arrange
        var email = _fixture.Config.UserAEmail;
        var password = _fixture.Config.UserAPassword;

        // Act
        await _loginPage.LoginAsync(email, password);
        await _loginPage.WaitForSuccessfulLoginAsync();

        // Assert
        Assert.True(_tripsPage.IsOnTripsPage(),
            "User should be on /trips after successful login");
    }
}