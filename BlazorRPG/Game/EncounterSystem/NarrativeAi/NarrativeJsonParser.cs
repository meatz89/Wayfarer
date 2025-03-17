using System.Text.Json;
using System.Text.RegularExpressions;

public static class NarrativeJsonChoicesParser
{
    private static readonly Regex ChoiceRegex = new Regex(@"Choice\s*(\d+):\s*(.*?)\s*-\s*(.*)", RegexOptions.Compiled);

    public static Dictionary<IChoice, ChoiceNarrative> ParseChoiceResponse(string response, List<IChoice> choices)
    {
        // First, try JSON parsing
        var jsonResult = TryParseJson(response, choices);
        if (jsonResult.Count > 0) return jsonResult;

        // If JSON fails, try markdown-style parsing
        return ParseChoiceFromMarkdown(response, choices);
    }

    private static Dictionary<IChoice, ChoiceNarrative> TryParseJson(string response, List<IChoice> choices)
    {
        Dictionary<IChoice, ChoiceNarrative> result = new Dictionary<IChoice, ChoiceNarrative>();

        try
        {
            // Remove any text before or after the JSON if present
            response = response.Trim();
            if (!response.StartsWith("{"))
            {
                int jsonStartIndex = response.IndexOf('{');
                int jsonEndIndex = response.LastIndexOf('}');

                if (jsonStartIndex != -1 && jsonEndIndex != -1)
                {
                    response = response.Substring(jsonStartIndex, jsonEndIndex - jsonStartIndex + 1);
                }
            }

            // Parse the JSON
            using JsonDocument doc = JsonDocument.Parse(response);
            JsonElement root = doc.RootElement;

            // Ensure we have a 'choices' array
            if (root.TryGetProperty("choices", out JsonElement choicesArray))
            {
                for (int i = 0; i < Math.Min(choices.Count, choicesArray.GetArrayLength()); i++)
                {
                    JsonElement choiceObj = choicesArray[i];

                    // Extract name, defaulting to original choice name if not found
                    string name = choiceObj.TryGetProperty("name", out var nameElem)
                        ? nameElem.GetString()
                        : choices[i].Name;

                    // Extract description, using a fallback if not found
                    string description = choiceObj.TryGetProperty("description", out var descElem)
                        ? descElem.GetString()
                        : "No description available";

                    result[choices[i]] = new ChoiceNarrative(name, description);
                }
            }

            return result;
        }
        catch
        {
            return new Dictionary<IChoice, ChoiceNarrative>();
        }
    }

    private static Dictionary<IChoice, ChoiceNarrative> ParseChoiceFromMarkdown(string response, List<IChoice> choices)
    {
        Dictionary<IChoice, ChoiceNarrative> result = new Dictionary<IChoice, ChoiceNarrative>();

        // Extract all choice matches
        var matches = ChoiceRegex.Matches(response);

        for (int i = 0; i < Math.Min(matches.Count, choices.Count); i++)
        {
            Match match = matches[i];

            // Extract parts of the choice
            string name = match.Groups[2].Value.Trim();
            string description = match.Groups[3].Value.Trim();

            result[choices[i]] = new ChoiceNarrative(name, description);
        }

        return result;
    }

    public static string ExtractNarrativeContext(string fullText)
    {
        // Split the text by "Choice" to separate narrative from choices
        string[] parts = fullText.Split(new[] { "Choice" }, StringSplitOptions.RemoveEmptyEntries);

        // Return the first part (narrative context)
        return parts.Length > 0 ? parts[0].Trim() : string.Empty;
    }
}