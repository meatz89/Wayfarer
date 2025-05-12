using System.Text.Json;
using System.Text.RegularExpressions;

public static class NarrativeJsonParser
{
    public static Dictionary<CardDefinition, ChoiceNarrative> ParseChoiceResponse(string response, List<CardDefinition> choices)
    {
        Dictionary<CardDefinition, ChoiceNarrative> result = new Dictionary<CardDefinition, ChoiceNarrative>();

        // Extract JSON content
        string jsonContent = ExtractJsonContent(response);
        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            return result;
        }

        // Verify JSON structure before attempting to parse
        if (!IsValidJsonStructure(jsonContent))
        {
            // Try to extract individual choices if full JSON is malformed
            return ExtractChoicesManually(jsonContent, choices);
        }

        // Try to parse as a complete ChoicesResponse
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        using (JsonDocument document = JsonDocument.Parse(jsonContent, new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        }))
        {
            JsonElement root = document.RootElement;

            // Look for "choices" array property
            if (root.TryGetProperty("choices", out JsonElement choicesElement) &&
                choicesElement.ValueKind == JsonValueKind.Array)
            {
                // Process each choice in the array
                int index = 0;
                foreach (JsonElement choice in choicesElement.EnumerateArray())
                {
                    if (index >= choices.Count) break;

                    string name = "";
                    string description = "";

                    if (choice.TryGetProperty("name", out JsonElement nameElement) &&
                        nameElement.ValueKind == JsonValueKind.String)
                    {
                        name = nameElement.GetString() ?? "";
                    }

                    if (choice.TryGetProperty("description", out JsonElement descElement) &&
                        descElement.ValueKind == JsonValueKind.String)
                    {
                        description = descElement.GetString() ?? "";
                    }

                    if (!string.IsNullOrWhiteSpace(name) || !string.IsNullOrWhiteSpace(description))
                    {
                        result[choices[index]] = new ChoiceNarrative(name, description);
                    }

                    index++;
                }
            }
            else
            {
                // If there's no "choices" property, try to parse as a direct array
                if (root.ValueKind == JsonValueKind.Array)
                {
                    int index = 0;
                    foreach (JsonElement choice in root.EnumerateArray())
                    {
                        if (index >= choices.Count) break;

                        string name = "";
                        string description = "";

                        if (choice.TryGetProperty("name", out JsonElement nameElement) &&
                            nameElement.ValueKind == JsonValueKind.String)
                        {
                            name = nameElement.GetString() ?? "";
                        }

                        if (choice.TryGetProperty("description", out JsonElement descElement) &&
                            descElement.ValueKind == JsonValueKind.String)
                        {
                            description = descElement.GetString() ?? "";
                        }

                        if (!string.IsNullOrWhiteSpace(name) || !string.IsNullOrWhiteSpace(description))
                        {
                            result[choices[index]] = new ChoiceNarrative(name, description);
                        }

                        index++;
                    }
                }
            }
        }

        return result;
    }

    private static bool IsValidJsonStructure(string jsonContent)
    {
        try
        {
            using (JsonDocument.Parse(jsonContent, new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            }))
            {
                return true;
            }
        }
        catch
        {
            return false;
        }
    }

    private static Dictionary<CardDefinition, ChoiceNarrative> ExtractChoicesManually(string content, List<CardDefinition> choices)
    {
        Dictionary<CardDefinition, ChoiceNarrative> result = new Dictionary<CardDefinition, ChoiceNarrative>();

        // Use regex to find choice objects even in malformed JSON
        string pattern = @"\{[^{}]*""name""[^{}]*""description""[^{}]*\}|\{[^{}]*""description""[^{}]*""name""[^{}]*\}";
        MatchCollection matches = Regex.Matches(content, pattern);

        for (int i = 0; i < matches.Count && i < choices.Count; i++)
        {
            string choiceJson = matches[i].Value;

            // Extract name and description with regex
            string name = ExtractProperty(choiceJson, "name");
            string description = ExtractProperty(choiceJson, "description");

            if (!string.IsNullOrWhiteSpace(name) || !string.IsNullOrWhiteSpace(description))
            {
                result[choices[i]] = new ChoiceNarrative(name, description);
            }
        }

        return result;
    }

    private static string ExtractProperty(string json, string propertyName)
    {
        string pattern = $@"""{propertyName}""[\s:]*""([^""\\]*(\\.[^""\\]*)*)""";
        Match match = Regex.Match(json, pattern);

        if (match.Success && match.Groups.Count > 1)
        {
            return match.Groups[1].Value;
        }

        return "";
    }

    private static string ExtractJsonContent(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "";

        string content = text.Trim();

        // Find the first occurrence of '{' that could be the start of JSON
        int startMarker = content.IndexOf('{');

        // Check if we also have a JSON array
        int arrayStart = content.IndexOf('[');
        if (arrayStart >= 0 && (arrayStart < startMarker || startMarker < 0))
        {
            startMarker = arrayStart;
        }

        if (startMarker < 0)
        {
            return "";
        }

        content = content.Substring(startMarker);

        // Find corresponding closing bracket/brace
        char openChar = content[0];
        char closeChar = (openChar == '{') ? '}' : ']';

        int openCount = 1;
        int closeCount = 0;
        int endMarker = -1;

        for (int i = 1; i < content.Length; i++)
        {
            if (content[i] == openChar) openCount++;
            else if (content[i] == closeChar) closeCount++;

            if (openCount == closeCount)
            {
                endMarker = i;
                break;
            }
        }

        if (endMarker >= 0)
        {
            content = content.Substring(0, endMarker + 1);
        }

        // Remove any markdown code block markers
        content = Regex.Replace(content, @"^```\w*\s*", "");
        content = Regex.Replace(content, @"\s*```$", "");

        return content;
    }

    public static string ExtractNarrativeContext(string fullText)
    {
        if (string.IsNullOrWhiteSpace(fullText))
            return "";

        string text = fullText.Trim();

        // Find the first occurrence of either '{' or '['
        int jsonStart = text.IndexOf('{');
        int arrayStart = text.IndexOf('[');

        if (arrayStart >= 0 && (arrayStart < jsonStart || jsonStart < 0))
        {
            jsonStart = arrayStart;
        }

        if (jsonStart <= 0)
        {
            return text;
        }

        return text.Substring(0, jsonStart).Trim();
    }
}
