using Microsoft.Playwright;

namespace MotoNomad.E2ETests.PageObjects;

/// <summary>
/// Base class for all Page Object Models.
/// Provides common functionality for interacting with pages.
/// </summary>
public abstract class BasePage
{
    protected readonly IPage Page;

    protected BasePage(IPage page)
    {
        Page = page;
    }

    /// <summary>
    /// Waits for the page to be fully loaded.
    /// </summary>
    protected async Task WaitForPageLoadAsync()
    {
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Page.WaitForTimeoutAsync(300); // Give Blazor time to render
    }

    /// <summary>
    /// Clicks an element and waits for navigation.
    /// </summary>
    protected async Task ClickAndWaitForNavigationAsync(ILocator locator)
    {
        await locator.ClickAsync();
        await WaitForPageLoadAsync();
    }

    /// <summary>
    /// Fills a text input with optional delay between keystrokes.
    /// </summary>
    protected async Task FillTextAsync(ILocator locator, string text, int delayMs = 0)
    {
        await locator.ClickAsync(); // Focus the input
        await locator.FillAsync(text);
        
        if (delayMs > 0)
        {
            await Page.WaitForTimeoutAsync(delayMs);
        }
    }

    /// <summary>
    /// Checks if an element is visible on the page.
    /// </summary>
    protected async Task<bool> IsVisibleAsync(ILocator locator, int timeoutMs = 5000)
    {
        try
        {
            await locator.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = timeoutMs
            });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }

    /// <summary>
    /// Waits for an element to disappear (useful for loading indicators).
    /// </summary>
    protected async Task WaitForHiddenAsync(ILocator locator, int timeoutMs = 10000)
    {
        await locator.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Hidden,
            Timeout = timeoutMs
        });
    }

    /// <summary>
    /// Gets the text content of an element.
    /// </summary>
    protected async Task<string?> GetTextAsync(ILocator locator)
    {
        return await locator.TextContentAsync();
    }

    /// <summary>
    /// Gets the current URL of the page.
    /// </summary>
    protected string CurrentUrl => Page.Url;

    /// <summary>
    /// Checks if the current URL matches the expected path.
    /// </summary>
    protected bool IsOnPage(string expectedPath)
    {
        var uri = new Uri(CurrentUrl);
        return uri.AbsolutePath.Equals(expectedPath, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Waits for navigation to a specific path.
    /// </summary>
    protected async Task WaitForNavigationToAsync(string expectedPath, int timeoutMs = 10000)
    {
        var deadline = DateTime.Now.AddMilliseconds(timeoutMs);
        
        while (DateTime.Now < deadline)
        {
            if (IsOnPage(expectedPath))
            {
                await WaitForPageLoadAsync();
                return;
            }
            await Page.WaitForTimeoutAsync(100);
        }

        throw new TimeoutException($"Navigation to '{expectedPath}' timed out. Current URL: {CurrentUrl}");
    }

    /// <summary>
    /// Takes a screenshot of the current page state.
    /// Useful for debugging test failures.
    /// </summary>
    protected async Task<byte[]> TakeScreenshotAsync()
    {
        return await Page.ScreenshotAsync(new PageScreenshotOptions
        {
            FullPage = true
        });
    }
}
