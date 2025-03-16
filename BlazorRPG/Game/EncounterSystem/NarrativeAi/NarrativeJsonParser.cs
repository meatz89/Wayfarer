using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;

/// <summary>
/// Parses JSON narrative responses from the AI
/// </summary>
public static class NarrativeJsonParser
{
    private static readonly Regex JsonExtractor = new Regex(@"\{(?:[^{}]|(?<Open>\{)|(?<-Open>\}))*(?(Open)(?!))\}", RegexOptions.Compiled);

    /// <summary>
    /// Parse a JSON narrative response into a NarrativeResponse object
    /// </summary>
    public static NarrativeResponse ParseNarrativeResponse(string jsonResponse)
    {
        // First try to extract JSON if it's embedded in other text
        string jsonString = ExtractJson(jsonResponse);

        // Parse the JSON into a NarrativeResponse object
        NarrativeResponse? narrativeResponse = JsonSerializer.Deserialize<NarrativeResponse>(jsonString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return narrativeResponse;
    }

    /// <summary>
    /// Parse a JSON choices response into a list of choice narratives
    /// </summary>
    public static Dictionary<IChoice, ChoiceNarrative> ParseChoiceResponse(
        string jsonResponse,
        List<IChoice> choices)
    {
        // First try to extract JSON if it's embedded in other text
        string jsonString = ExtractJson(jsonResponse);

        // Parse the JSON into a ChoicesResponse object
        var choicesResponse = JsonSerializer.Deserialize<ChoicesResponse>(jsonString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (choicesResponse == null || choicesResponse.Choices == null || choicesResponse.Choices.Count < choices.Count)
        {
            return CreateFallbackChoiceDescriptions(choices);
        }

        // Map the parsed choices to the actual choice objects
        Dictionary<IChoice, ChoiceNarrative> result = new Dictionary<IChoice, ChoiceNarrative>();
        for (int i = 0; i < choices.Count && i < choicesResponse.Choices.Count; i++)
        {
            var parsedChoice = choicesResponse.Choices[i];
            if (parsedChoice != null && !string.IsNullOrEmpty(parsedChoice.Name) && !string.IsNullOrEmpty(parsedChoice.Description))
            {
                result[choices[i]] = new ChoiceNarrative(parsedChoice.Name, parsedChoice.Description);
            }
        }

        // Fill in any missing choices with fallbacks
        if (result.Count < choices.Count)
        {
            var fallbackResults = CreateFallbackChoiceDescriptions(choices);
            foreach (var choice in choices)
            {
                if (!result.ContainsKey(choice) && fallbackResults.ContainsKey(choice))
                {
                    result[choice] = fallbackResults[choice];
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Create a combined narrative string from a NarrativeResponse
    /// </summary>
    public static string CreateCombinedNarrative(NarrativeResponse response)
    {
        return $"{response.ActionOutcome}\n\n{response.NewSituation}";
    }

    /// <summary>
    /// Extract JSON from a string that might contain non-JSON content
    /// </summary>
    private static string ExtractJson(string input)
    {
        var match = JsonExtractor.Match(input);
        return match.Success ? match.Value : input;
    }

    /// <summary>
    /// Create fallback choice descriptions
    /// </summary>
    private static Dictionary<IChoice, ChoiceNarrative> CreateFallbackChoiceDescriptions(List<IChoice> choices)
    {
        Dictionary<IChoice, ChoiceNarrative> results = new Dictionary<IChoice, ChoiceNarrative>();

        for (int i = 0; i < choices.Count; i++)
        {
            IChoice choice = choices[i];
            string name = $"I use {choice.Approach} approach";
            string description = $"I focus on {choice.Focus} using a {choice.Approach} approach to address the current situation.";

            results[choice] = new ChoiceNarrative(name, description);
        }

        return results;
    }
}
