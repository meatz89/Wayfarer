using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

public static class NarrativeJsonParser
{
    public static Dictionary<IChoice, ChoiceNarrative> ParseChoiceResponse(string response, List<IChoice> choices)
    {
        // Extract JSON content
        string jsonContent = ExtractJsonContent(response);

        // Parse the JSON
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        ChoicesResponse parsedResponse = JsonSerializer.Deserialize<ChoicesResponse>(jsonContent, options);

        // Map to choices
        Dictionary<IChoice, ChoiceNarrative> result = new Dictionary<IChoice, ChoiceNarrative>();

        if (parsedResponse?.Choices != null)
        {
            for (int i = 0; i < choices.Count && i < parsedResponse.Choices.Count; i++)
            {
                ChoiceData choiceData = parsedResponse.Choices[i];
                result[choices[i]] = new ChoiceNarrative(choiceData.Name, choiceData.Description);
            }
        }

        return result;
    }

    private static string ExtractJsonContent(string text)
    {
        // Remove markdown code blocks if present
        string content = text.Trim();

        // Remove leading ```json or similar
        int startMarker = content.IndexOf('{');
        if (startMarker > 0)
        {
            content = content.Substring(startMarker);
        }

        // Remove trailing ``` if present
        int endMarker = content.LastIndexOf('}');
        if (endMarker >= 0 && endMarker < content.Length - 1)
        {
            content = content.Substring(0, endMarker + 1);
        }

        return content;
    }

    public static string ExtractNarrativeContext(string fullText)
    {
        int jsonStart = fullText.IndexOf('{');
        if (jsonStart <= 0)
        {
            return fullText.Trim();
        }

        return fullText.Substring(0, jsonStart).Trim();
    }
}