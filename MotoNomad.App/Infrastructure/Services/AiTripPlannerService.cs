using Microsoft.Extensions.Logging;
using MotoNomad.App.Application.DTOs;
using MotoNomad.App.Application.DTOs.OpenRouter;
using MotoNomad.App.Application.Interfaces;

namespace MotoNomad.App.Infrastructure.Services;

/// <summary>
/// AI-powered trip planning service using OpenRouter API.
/// </summary>
public class AiTripPlannerService : IAiTripPlannerService
{
    private readonly IOpenRouterService _openRouterService;
    private readonly ILogger<AiTripPlannerService> _logger;

    public AiTripPlannerService(
        IOpenRouterService openRouterService,
        ILogger<AiTripPlannerService> logger)
    {
        _openRouterService = openRouterService;
        _logger = logger;
    }

    /// <summary>
    /// Generates AI-powered trip suggestions based on trip parameters.
    /// </summary>
    /// <param name="tripName">The name of the trip.</param>
    /// <param name="startDate">The start date of the trip.</param>
    /// <param name="endDate">The end date of the trip.</param>
  /// <param name="transportType">The type of transport (e.g., motorcycle, airplane, train).</param>
 /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
 /// <returns>A task that represents the asynchronous operation, containing trip suggestions.</returns>
    /// <exception cref="OpenRouterException">Thrown when the AI service fails to generate suggestions.</exception>
    public async Task<TripSuggestionDto> GenerateTripSuggestionAsync(
        string tripName,
  DateTime startDate,
        DateTime endDate,
        string transportType,
    CancellationToken cancellationToken = default)
    {
        // Calculate trip duration
        var duration = (endDate - startDate).Days + 1;

        // Build prompt messages
        var systemMessage = "You are a travel assistant. Respond in Polish.";
        
        var userMessage = $@"Planuję wycieczkę {transportType} '{tripName}' od {startDate:dd.MM.yyyy} do {endDate:dd.MM.yyyy} ({duration} dni).
Zasugeruj:
1) Krótki opis trasy (3-5 zdań)
2) Top 3-5 miejsc do zobaczenia

Odpowiedz w następującym formacie:
OPIS:
[opis trasy]

ATRAKCJE:
- [atrakcja 1]
- [atrakcja 2]
- [atrakcja 3]
- [atrakcja 4]
- [atrakcja 5]";

        _logger.LogInformation("Generating trip suggestions for trip: {TripName}, Duration: {Duration} days", tripName, duration);

        try
    {
            // Create chat completion request
            var request = new ChatCompletionRequest
            {
                Model = "google/gemma-3-27b-it:free", // Free model
                Messages = new List<ChatMessage>
    {
        ChatMessage.System(systemMessage),
     ChatMessage.User(userMessage)
},
                Temperature = 0.7,
                MaxTokens = 500
            };

      // Call OpenRouter API
         var response = await _openRouterService.SendChatCompletionAsync(request, cancellationToken);

         // Extract response text
    var responseText = response.Choices.FirstOrDefault()?.Message.Content ?? string.Empty;

     // Parse response into TripSuggestionDto
   var suggestion = ParseResponse(responseText);

            _logger.LogInformation("Successfully generated trip suggestions for: {TripName}", tripName);

       return suggestion;
        }
    catch (Exception ex)
        {
    _logger.LogError(ex, "Failed to generate trip suggestions for: {TripName}", tripName);
            throw;
        }
    }

    /// <summary>
    /// Parses the AI response text into a TripSuggestionDto.
    /// </summary>
    /// <param name="responseText">The raw AI response text.</param>
    /// <returns>Parsed trip suggestion DTO.</returns>
    private TripSuggestionDto ParseResponse(string responseText)
    {
        var suggestion = new TripSuggestionDto();

        if (string.IsNullOrWhiteSpace(responseText))
        {
         return suggestion;
        }

        var lines = responseText.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        bool parsingDescription = false;
        bool parsingHighlights = false;
        var descriptionLines = new List<string>();

        foreach (var line in lines)
     {
       if (line.StartsWith("OPIS:", StringComparison.OrdinalIgnoreCase))
            {
         parsingDescription = true;
    parsingHighlights = false;
         continue;
       }
     
            if (line.StartsWith("ATRAKCJE:", StringComparison.OrdinalIgnoreCase))
        {
          parsingDescription = false;
    parsingHighlights = true;
           continue;
       }

   if (parsingDescription)
   {
 descriptionLines.Add(line);
  }
 else if (parsingHighlights && line.StartsWith("-"))
          {
  var highlight = line.TrimStart('-').Trim();
        if (!string.IsNullOrWhiteSpace(highlight))
      {
          suggestion.Highlights.Add(highlight);
    }
            }
        }

    // Combine description lines
        suggestion.SuggestedDescription = string.Join(" ", descriptionLines);

     // Limit highlights to top 5
        suggestion.Highlights = suggestion.Highlights.Take(5).ToList();

        return suggestion;
    }
}
