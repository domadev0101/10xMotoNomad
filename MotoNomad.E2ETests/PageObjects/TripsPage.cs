using Microsoft.Playwright;

namespace MotoNomad.E2ETests.PageObjects;

/// <summary>
/// Page Object Model for the Trips List page.
/// </summary>
public class TripsPage : BasePage
{
    private const string PagePath = "/trips";

    public TripsPage(IPage page) : base(page) { }

    // Locators
    private ILocator UpcomingTab => Page.GetByTestId("trips-tab-upcoming");
    private ILocator PastTab => Page.GetByTestId("trips-tab-past");
    private ILocator UpcomingTripsList => Page.GetByTestId("trips-list-upcoming");
    private ILocator PastTripsList => Page.GetByTestId("trips-list-past");
    private ILocator EmptyState => Page.GetByTestId("trips-empty-state");
    private ILocator CreateTripButton => Page.GetByTestId("create-trip-btn");

    /// <summary>
    /// Navigates to the trips list page.
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
    /// Clicks the "Create Trip" floating action button.
    /// </summary>
    public async Task ClickCreateTripButtonAsync()
    {
        await CreateTripButton.ClickAsync();
        await WaitForPageLoadAsync();
    }

    /// <summary>
    /// Switches to the "Upcoming" tab.
    /// </summary>
    public async Task SwitchToUpcomingTabAsync()
    {
        await UpcomingTab.ClickAsync();
        await Page.WaitForTimeoutAsync(500); // Wait for tab content to load
    }

    /// <summary>
    /// Switches to the "Past" tab.
    /// </summary>
    public async Task SwitchToPastTabAsync()
    {
        await PastTab.ClickAsync();
        await Page.WaitForTimeoutAsync(500); // Wait for tab content to load
    }

    /// <summary>
    /// Gets a trip card by its ID.
    /// </summary>
    private ILocator GetTripCard(Guid tripId)
    {
        return Page.GetByTestId($"trip-item-{tripId}");
    }

    /// <summary>
    /// Gets a trip card by its name (searches in all visible cards).
    /// </summary>
    private ILocator GetTripCardByName(string tripName)
    {
        // Find trip card that contains the name
        return Page.Locator($"[data-testid^='trip-item-']")
            .Filter(new LocatorFilterOptions { HasText = tripName });
    }

    /// <summary>
    /// Clicks on a trip card by its ID.
    /// </summary>
    public async Task ClickTripAsync(Guid tripId)
    {
        var tripCard = GetTripCard(tripId);
        await tripCard.ClickAsync();
        await WaitForPageLoadAsync();
    }

    /// <summary>
    /// Clicks on a trip card by its name.
    /// </summary>
    public async Task ClickTripByNameAsync(string tripName)
    {
        var tripCard = GetTripCardByName(tripName);
        await tripCard.First.ClickAsync();
        await WaitForPageLoadAsync();
    }

    /// <summary>
    /// Checks if a trip with the given ID is visible in the list.
    /// </summary>
    public async Task<bool> IsTripVisibleAsync(Guid tripId, int timeoutMs = 5000)
    {
        var tripCard = GetTripCard(tripId);
        return await IsVisibleAsync(tripCard, timeoutMs);
    }

    /// <summary>
    /// Checks if a trip with the given name is visible in the list.
    /// </summary>
    public async Task<bool> IsTripVisibleByNameAsync(string tripName, int timeoutMs = 5000)
    {
        var tripCard = GetTripCardByName(tripName);
        try
        {
            await tripCard.First.WaitForAsync(new LocatorWaitForOptions
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
    /// Gets the count of visible trips in the current tab.
    /// </summary>
    public async Task<int> GetVisibleTripsCountAsync()
    {
        var tripCards = Page.Locator("[data-testid^='trip-item-']");
        return await tripCards.CountAsync();
    }

    /// <summary>
    /// Checks if the empty state is displayed (no trips).
    /// </summary>
    public async Task<bool> IsEmptyStateVisibleAsync()
    {
        return await IsVisibleAsync(EmptyState, timeoutMs: 2000);
    }

    /// <summary>
    /// Gets the trip name from a trip card by its ID.
    /// </summary>
    public async Task<string?> GetTripNameAsync(Guid tripId)
    {
        var tripCard = GetTripCard(tripId);
        var nameElement = tripCard.GetByTestId("trip-name");
        return await nameElement.TextContentAsync();
    }

    /// <summary>
    /// Gets all trip names visible in the current tab.
    /// </summary>
    public async Task<List<string>> GetAllTripNamesAsync()
    {
        var tripNames = new List<string>();
        var nameElements = Page.GetByTestId("trip-name");
        var count = await nameElements.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var text = await nameElements.Nth(i).TextContentAsync();
            if (!string.IsNullOrEmpty(text))
            {
                tripNames.Add(text);
            }
        }

        return tripNames;
    }

    /// <summary>
    /// Waits for a trip with the given name to appear in the list.
    /// </summary>
    public async Task WaitForTripToAppearAsync(string tripName, int timeoutMs = 10000)
    {
        var tripCard = GetTripCardByName(tripName);
        await tripCard.First.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs
        });
    }

    /// <summary>
    /// Waits for a trip with the given ID to disappear from the list.
    /// </summary>
    public async Task WaitForTripToDisappearAsync(Guid tripId, int timeoutMs = 10000)
    {
        var tripCard = GetTripCard(tripId);
        await WaitForHiddenAsync(tripCard, timeoutMs);
    }

    /// <summary>
    /// Verifies that we are on the trips page.
    /// </summary>
    public bool IsOnTripsPage()
    {
        return IsOnPage(PagePath);
    }
}
