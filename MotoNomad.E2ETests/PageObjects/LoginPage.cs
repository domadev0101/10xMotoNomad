using Microsoft.Playwright;

namespace MotoNomad.E2ETests.PageObjects;

/// <summary>
/// Page Object Model for the Login page.
/// </summary>
public class LoginPage : BasePage
{
    private const string PagePath = "/login";

    public LoginPage(IPage page) : base(page) { }

    // Locators
    private ILocator EmailInput => Page.GetByTestId("login-email");
    private ILocator PasswordInput => Page.GetByTestId("login-password");
    private ILocator SubmitButton => Page.GetByTestId("login-submit");
    private ILocator ErrorMessage => Page.GetByTestId("login-error");
    private ILocator RegisterLink => Page.GetByTestId("register-link");

    /// <summary>
    /// Navigates to the login page.
    /// </summary>
    public async Task NavigateAsync()
    {
        await Page.GotoAsync(PagePath, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });
        await WaitForPageLoadAsync();
    }

    /// <summary>
    /// Fills in email field.
    /// </summary>
    public async Task FillEmailAsync(string email)
    {
        await EmailInput.ClickAsync(); // Focus
        await EmailInput.FillAsync(email);
    }

    /// <summary>
    /// Fills in password field.
    /// </summary>
    public async Task FillPasswordAsync(string password)
    {
        await PasswordInput.ClickAsync(); // Focus
        await PasswordInput.FillAsync(password);
    }

    /// <summary>
    /// Clicks the login button.
    /// </summary>
    public async Task ClickLoginAsync()
    {
        await SubmitButton.ClickAsync();
        await WaitForPageLoadAsync();
    }

    /// <summary>
    /// Performs complete login flow: navigate, fill credentials, submit.
    /// </summary>
    public async Task LoginAsync(string email, string password)
    {
        await NavigateAsync();
        await FillEmailAsync(email);
        await FillPasswordAsync(password);
        await ClickLoginAsync();
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
    /// Clicks the "Register" link to go to registration page.
    /// </summary>
    public async Task ClickRegisterLinkAsync()
    {
        await RegisterLink.ClickAsync();
        await WaitForPageLoadAsync();
    }

    /// <summary>
    /// Verifies that we are on the login page.
    /// </summary>
    public bool IsOnLoginPage()
    {
        return IsOnPage(PagePath);
    }

    /// <summary>
    /// Waits for successful login (redirect to /trips).
    /// </summary>
    public async Task WaitForSuccessfulLoginAsync(int timeoutMs = 10000)
    {
        await WaitForNavigationToAsync("/trips", timeoutMs);
    }
}
