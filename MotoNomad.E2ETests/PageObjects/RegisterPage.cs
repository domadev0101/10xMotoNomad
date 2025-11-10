using Microsoft.Playwright;

namespace MotoNomad.E2ETests.PageObjects;

/// <summary>
/// Page Object Model for the Register page.
/// </summary>
public class RegisterPage : BasePage
{
    private const string PagePath = "/register";

    public RegisterPage(IPage page) : base(page) { }

    // Locators
    private ILocator EmailInput => Page.GetByTestId("register-email");
    private ILocator PasswordInput => Page.GetByTestId("register-password");
    private ILocator ConfirmPasswordInput => Page.GetByTestId("register-confirm-password");
    private ILocator DisplayNameInput => Page.GetByTestId("register-display-name");
    private ILocator SubmitButton => Page.GetByTestId("register-submit");
    private ILocator ErrorMessage => Page.GetByTestId("register-error");
    private ILocator LoginLink => Page.GetByTestId("login-link");

    /// <summary>
    /// Navigates to the register page.
    /// </summary>
    public async Task NavigateAsync()
    {
        await Page.GotoAsync(PagePath, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });
        await WaitForPageLoadAsync();
        await Page.WaitForTimeoutAsync(1000); // Extra wait for Blazor to render
    }

    /// <summary>
    /// Fills in email field.
    /// </summary>
    public async Task FillEmailAsync(string email)
    {
        await EmailInput.ClickAsync();
        await EmailInput.FillAsync(email);
    }

    /// <summary>
    /// Fills in password field.
    /// </summary>
    public async Task FillPasswordAsync(string password)
    {
        await PasswordInput.ClickAsync();
        await PasswordInput.FillAsync(password);
    }

    /// <summary>
    /// Fills in confirm password field.
    /// </summary>
    public async Task FillConfirmPasswordAsync(string confirmPassword)
    {
        await ConfirmPasswordInput.ClickAsync();
        await ConfirmPasswordInput.FillAsync(confirmPassword);
    }

    /// <summary>
    /// Fills in display name field (if present).
    /// </summary>
    public async Task FillDisplayNameAsync(string displayName)
    {
        try
        {
            await DisplayNameInput.ClickAsync();
            await DisplayNameInput.FillAsync(displayName);
        }
        catch
        {
            // Display name might be optional or not present
        }
    }

    /// <summary>
    /// Clicks the register button.
    /// </summary>
    public async Task ClickRegisterAsync()
    {
        await SubmitButton.ClickAsync();
        await WaitForPageLoadAsync();
    }

    /// <summary>
    /// Performs complete registration flow: navigate, fill fields, submit.
    /// </summary>
    public async Task RegisterAsync(string email, string password, string confirmPassword, string? displayName = null)
    {
        await NavigateAsync();
        await FillEmailAsync(email);
        await FillPasswordAsync(password);
        await FillConfirmPasswordAsync(confirmPassword);

        if (!string.IsNullOrEmpty(displayName))
        {
            await FillDisplayNameAsync(displayName);
        }

        await ClickRegisterAsync();
        await WaitForSuccessfulRegistrationAsync();
    }

    /// <summary>
    /// Gets the error message text if displayed.
    /// Returns null if no error is visible.
    /// </summary>
    public async Task<string?> GetErrorMessageAsync()
    {
        try
        {
            if (await IsVisibleAsync(ErrorMessage, timeoutMs: 2000))
            {
                return await ErrorMessage.TextContentAsync();
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Checks if an error message is displayed.
    /// </summary>
    public async Task<bool> HasErrorAsync()
    {
        return await IsVisibleAsync(ErrorMessage, timeoutMs: 2000);
    }

    /// <summary>
    /// Checks if the submit button is disabled.
    /// </summary>
    public async Task<bool> IsSubmitButtonDisabledAsync()
    {
        return await SubmitButton.IsDisabledAsync();
    }

    /// <summary>
    /// Clicks the "Login" link to go to login page.
    /// </summary>
    public async Task ClickLoginLinkAsync()
    {
        try
        {
            await LoginLink.ClickAsync();
            await WaitForPageLoadAsync();
        }
        catch
        {
            // Link might not be present
        }
    }

    /// <summary>
    /// Verifies that we are on the register page.
    /// </summary>
    public bool IsOnRegisterPage()
    {
        return IsOnPage(PagePath);
    }

    /// <summary>
    /// Waits for successful registration (redirect to /trips or /login).
    /// </summary>
    public async Task WaitForSuccessfulRegistrationAsync(int timeoutMs = 10000)
    {
        // After registration, might redirect to /trips or /login
        var deadline = DateTime.Now.AddMilliseconds(timeoutMs);

        while (DateTime.Now < deadline)
        {
            if (IsOnPage("/trips") || IsOnPage("/login"))
            {
                await WaitForPageLoadAsync();
                return;
            }
            await Page.WaitForTimeoutAsync(100);
        }

        throw new TimeoutException($"Registration did not complete. Current URL: {CurrentUrl}");
    }
}
