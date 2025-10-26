using Microsoft.AspNetCore.Components;
using MotoNomad.App.Application.DTOs;
using MotoNomad.App.Application.Interfaces;
using MotoNomad.App.Application.Exceptions;
using MudBlazor;

namespace MotoNomad.App.Shared.Components;

/// <summary>
/// AI Assistant Panel component for generating trip suggestions using AI.
/// </summary>
public partial class AiAssistantPanel
{
    /// <summary>
    /// Gets or sets the trip name.
    /// </summary>
    [Parameter]
    public string? TripName { get; set; }

    /// <summary>
    /// Gets or sets the start date of the trip.
    /// </summary>
    [Parameter]
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Gets or sets the end date of the trip.
  /// </summary>
    [Parameter]
    public DateTime? EndDate { get; set; }

    /// <summary>
  /// Gets or sets the transport type.
/// </summary>
    [Parameter]
    public string? TransportType { get; set; }

    /// <summary>
    /// Event callback invoked when AI suggestions are successfully generated.
 /// </summary>
[Parameter]
    public EventCallback<TripSuggestionDto> OnSuggestionGenerated { get; set; }

    [Inject]
    private IAiTripPlannerService AiTripPlannerService { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private bool _isLoading;
    private string? _errorMessage;
    private TripSuggestionDto? _suggestion;

    /// <summary>
    /// Determines whether AI suggestions can be generated based on required fields.
    /// </summary>
    /// <returns>True if all required fields are filled; otherwise, false.</returns>
    private bool CanGenerateSuggestions()
    {
        return !string.IsNullOrWhiteSpace(TripName)
         && StartDate.HasValue
         && EndDate.HasValue
            && !string.IsNullOrWhiteSpace(TransportType);
    }

    /// <summary>
 /// Handles the Generate AI Suggestions button click.
    /// </summary>
    private async Task HandleGenerateSuggestions()
    {
     if (!CanGenerateSuggestions())
     {
      return;
        }

  _isLoading = true;
        _errorMessage = null;
   _suggestion = null;
      StateHasChanged();

   try
        {
      _suggestion = await AiTripPlannerService.GenerateTripSuggestionAsync(
       TripName!,
    StartDate!.Value,
  EndDate!.Value,
     TransportType!,
           CancellationToken.None);

       Snackbar.Add("AI suggestions generated successfully!", Severity.Success);
    }
    catch (OpenRouterAuthException ex)
        {
      _errorMessage = "⚠️ OpenRouter API key is not configured. " +
   "Please add your API key to appsettings.json. " +
   "Get a free key at: https://openrouter.ai/keys";
  Snackbar.Add("API key not configured", Severity.Warning);
    }
catch (OpenRouterRateLimitException)
        {
   _errorMessage = "Rate limit exceeded. Please try again in a few moments.";
       Snackbar.Add("Rate limit exceeded. Please wait before trying again.", Severity.Warning);
        }
   catch (OpenRouterException ex)
   {
         _errorMessage = $"AI service error: {ex.Message}";
    Snackbar.Add("Failed to generate AI suggestions. Please try again.", Severity.Error);
        }
   catch (Exception ex)
  {
       _errorMessage = "An unexpected error occurred. Please try again later.";
 Snackbar.Add("An unexpected error occurred", Severity.Error);
  // In production, log this error
      Console.Error.WriteLine($"Unexpected error in AiAssistantPanel: {ex}");
 }
        finally
    {
          _isLoading = false;
 StateHasChanged();
     }
    }

    /// <summary>
    /// Handles the Apply to Description button click.
    /// </summary>
    private async Task HandleApplySuggestion()
    {
        if (_suggestion == null)
        {
        return;
        }

        await OnSuggestionGenerated.InvokeAsync(_suggestion);
        Snackbar.Add("AI suggestions applied to description!", Severity.Success);
    }
}
