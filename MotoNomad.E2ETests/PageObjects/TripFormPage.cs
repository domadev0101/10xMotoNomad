using Microsoft.Playwright;

namespace MotoNomad.E2ETests.PageObjects;

/// <summary>
/// Page Object Model for the Trip Create/Edit form page.
/// Handles both /trip/create and /trip/edit/{id} pages.
/// </summary>
public class TripFormPage : BasePage
{
    private const string CreatePath = "/trip/create";

    public TripFormPage(IPage page) : base(page) { }

    // Locators
    private ILocator NameInput => Page.GetByTestId("trip-name");
    private ILocator StartDateInput => Page.GetByTestId("trip-start-date");
    private ILocator EndDateInput => Page.GetByTestId("trip-end-date");
    private ILocator TransportSelect => Page.GetByTestId("trip-transport");
    private ILocator DescriptionInput => Page.GetByTestId("trip-description");
    private ILocator SubmitButton => Page.GetByTestId("trip-submit");
    private ILocator ValidationError => Page.GetByTestId("trip-validation-error");
    
    // AI Assistant (if present)
    private ILocator AiGenerateButton => Page.GetByTestId("ai-generate-btn");
    private ILocator AiApplyButton => Page.GetByTestId("ai-apply-btn");
    private ILocator AiSuggestions => Page.GetByTestId("ai-suggestions");
    private ILocator AiError => Page.GetByTestId("ai-error");

    /// <summary>
    /// Navigates to the create trip page.
    /// </summary>
    public async Task NavigateToCreateAsync()
    {
        await Page.GotoAsync(CreatePath, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });
        await WaitForPageLoadAsync();
    }

    /// <summary>
    /// Navigates to the edit trip page.
    /// </summary>
    public async Task NavigateToEditAsync(Guid tripId)
    {
        await Page.GotoAsync($"/trip/edit/{tripId}", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });
        await WaitForPageLoadAsync();
    }

    /// <summary>
    /// Fills in the trip name.
    /// </summary>
    public async Task FillNameAsync(string name)
    {
        await NameInput.ClickAsync();
        await NameInput.FillAsync(name);
    }

    /// <summary>
    /// Fills in the start date.
    /// Format: yyyy-MM-dd (e.g., "2025-12-01")
    /// </summary>
    public async Task FillStartDateAsync(string date)
    {
        await StartDateInput.ClickAsync();
        await StartDateInput.FillAsync(date);
    }

    /// <summary>
    /// Fills in the end date.
    /// Format: yyyy-MM-dd (e.g., "2025-12-15")
    /// </summary>
    public async Task FillEndDateAsync(string date)
    {
        await EndDateInput.ClickAsync();
        await EndDateInput.FillAsync(date);
    }

    /// <summary>
    /// Selects a transport type.
    /// Options: "Motorcycle", "Airplane", "Train", "Car", "Other"
    /// </summary>
    public async Task SelectTransportAsync(string transportType)
    {
        await TransportSelect.ClickAsync();
        
        // MudSelect opens a dropdown - wait for it and click the option
        await Page.WaitForTimeoutAsync(300);
        
        // Find and click the option in the dropdown
        var option = Page.Locator($".mud-list-item").Filter(new LocatorFilterOptions 
        { 
            HasText = transportType 
        });
        
        await option.ClickAsync();
    }

    /// <summary>
    /// Fills in the description.
    /// </summary>
    public async Task FillDescriptionAsync(string description)
    {
        await DescriptionInput.ClickAsync();
        await DescriptionInput.FillAsync(description);
    }

    /// <summary>
    /// Clicks the submit button (Create or Save).
    /// </summary>
    public async Task ClickSubmitAsync()
    {
        await SubmitButton.ClickAsync();
        await WaitForPageLoadAsync();
    }

    /// <summary>
    /// Creates a new trip with the given details.
    /// </summary>
    public async Task CreateTripAsync(
        string name,
        string startDate,
        string endDate,
        string transportType,
        string? description = null)
    {
        await NavigateToCreateAsync();
        await FillNameAsync(name);
        await FillStartDateAsync(startDate);
        await FillEndDateAsync(endDate);
        await SelectTransportAsync(transportType);
        
        if (!string.IsNullOrEmpty(description))
        {
            await FillDescriptionAsync(description);
        }
        
        await ClickSubmitAsync();
    }

    /// <summary>
    /// Gets the validation error message if displayed.
    /// Returns null if no error is visible.
    /// </summary>
    public async Task<string?> GetValidationErrorAsync()
    {
        try
        {
            if (await IsVisibleAsync(ValidationError, timeoutMs: 2000))
            {
                return await ValidationError.TextContentAsync();
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Checks if a validation error is displayed.
    /// </summary>
    public async Task<bool> HasValidationErrorAsync()
    {
        return await IsVisibleAsync(ValidationError, timeoutMs: 2000);
    }

    /// <summary>
    /// Checks if the submit button is disabled.
    /// </summary>
    public async Task<bool> IsSubmitButtonDisabledAsync()
    {
        return await SubmitButton.IsDisabledAsync();
    }

    /// <summary>
    /// Gets the current value of the name input.
    /// </summary>
    public async Task<string?> GetNameValueAsync()
    {
        return await NameInput.InputValueAsync();
    }

    /// <summary>
    /// Gets the current value of the description input.
    /// </summary>
    public async Task<string?> GetDescriptionValueAsync()
    {
        return await DescriptionInput.InputValueAsync();
    }

    /// <summary>
    /// Waits for successful trip creation/update (redirect to /trips or /trip/{id}).
    /// </summary>
    public async Task WaitForSuccessfulSaveAsync(int timeoutMs = 10000)
    {
        var deadline = DateTime.Now.AddMilliseconds(timeoutMs);
        
        while (DateTime.Now < deadline)
        {
            if (IsOnPage("/trips") || CurrentUrl.Contains("/trip/"))
            {
                await WaitForPageLoadAsync();
                return;
            }
            await Page.WaitForTimeoutAsync(100);
        }

        throw new TimeoutException($"Trip save did not complete. Current URL: {CurrentUrl}");
    }

    // === AI Assistant Methods ===

    /// <summary>
    /// Clicks the "Generate AI Suggestions" button.
    /// </summary>
    public async Task ClickGenerateAiSuggestionsAsync()
    {
        await AiGenerateButton.ClickAsync();
        
        // Wait for suggestions to load
        await Page.WaitForTimeoutAsync(1000);
    }

    /// <summary>
    /// Clicks the "Apply to Description" button for AI suggestions.
    /// </summary>
    public async Task ClickApplyAiSuggestionsAsync()
    {
        await AiApplyButton.ClickAsync();
        await Page.WaitForTimeoutAsync(500);
    }

    /// <summary>
    /// Gets the AI suggestions text if displayed.
    /// </summary>
    public async Task<string?> GetAiSuggestionsAsync()
    {
        try
        {
            if (await IsVisibleAsync(AiSuggestions, timeoutMs: 5000))
            {
                return await AiSuggestions.TextContentAsync();
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the AI error message if displayed.
    /// </summary>
    public async Task<string?> GetAiErrorAsync()
    {
        try
        {
            if (await IsVisibleAsync(AiError, timeoutMs: 2000))
            {
                return await AiError.TextContentAsync();
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Checks if AI suggestions are visible.
    /// </summary>
    public async Task<bool> HasAiSuggestionsAsync()
    {
        return await IsVisibleAsync(AiSuggestions, timeoutMs: 5000);
    }

    /// <summary>
    /// Checks if AI error is visible.
    /// </summary>
    public async Task<bool> HasAiErrorAsync()
    {
        return await IsVisibleAsync(AiError, timeoutMs: 2000);
    }
}
