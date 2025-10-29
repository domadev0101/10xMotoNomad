using Microsoft.Playwright;
using MotoNomad.E2ETests.Configuration;

namespace MotoNomad.E2ETests.Fixtures;

/// <summary>
/// Fixture that manages Playwright browser lifecycle for E2E tests.
/// Each test gets a fresh browser context and page.
/// </summary>
public class PlaywrightFixture : IAsyncDisposable
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public TestConfiguration Config { get; }
    public IBrowserContext? Context { get; private set; }
    public IPage? Page { get; private set; }

    public PlaywrightFixture()
    {
        Config = new TestConfiguration();
        Config.Validate(); // Fail fast if configuration is missing
    }

    /// <summary>
    /// Initializes Playwright and launches the browser.
    /// Call this in [SetUp] or test constructor.
    /// </summary>
    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();

        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = Config.Headless,
            SlowMo = Config.SlowMo, // Useful for debugging
        });

        await CreateNewContextAsync();
    }

    /// <summary>
    /// Creates a new browser context and page.
    /// Useful for tests that need a fresh start.
    /// </summary>
    public async Task CreateNewContextAsync()
    {
        // Close existing context if any
        if (Context != null)
        {
            await Context.CloseAsync();
        }

        Context = await _browser!.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = Config.BaseUrl, // Set base URL for relative navigation
            ViewportSize = new ViewportSize
            {
                Width = 1920,
                Height = 1080
            },
            Locale = "pl-PL", // MotoNomad locale
            TimezoneId = "Europe/Warsaw"
        });

        // Set default timeout
        Context.SetDefaultTimeout(Config.DefaultTimeout);
        Context.SetDefaultNavigationTimeout(Config.DefaultTimeout);

        Page = await Context.NewPageAsync();
    }

    /// <summary>
    /// Navigates to the application base URL.
    /// </summary>
    public async Task NavigateToAppAsync()
    {
        if (Page == null)
        {
            throw new InvalidOperationException("Page not initialized. Call InitializeAsync first.");
        }

        await Page.GotoAsync(Config.BaseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });
    }

    /// <summary>
    /// Takes a screenshot for debugging. Useful in test failures.
    /// </summary>
    public async Task<byte[]?> TakeScreenshotAsync(string? name = null)
    {
        if (Page == null) return null;

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var filename = name != null ? $"{name}_{timestamp}.png" : $"screenshot_{timestamp}.png";
        var path = Path.Combine("TestResults", "Screenshots", filename);

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        return await Page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = path,
            FullPage = true
        });
    }

    /// <summary>
    /// Waits for the application to be ready (useful after navigation).
    /// </summary>
    public async Task WaitForAppReadyAsync()
    {
        if (Page == null) return;

        // Wait for Blazor to be ready
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Give Blazor a moment to render
        await Page.WaitForTimeoutAsync(500);
    }

    public async ValueTask DisposeAsync()
    {
        if (Page != null)
        {
            await Page.CloseAsync();
        }

        if (Context != null)
        {
            await Context.CloseAsync();
        }

        if (_browser != null)
        {
            await _browser.CloseAsync();
        }

        _playwright?.Dispose();
    }
}