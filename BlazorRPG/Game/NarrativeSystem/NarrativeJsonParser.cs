using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;

public static class NarrativeJsonParser
{
    public static Dictionary<CardDefinition, ChoiceNarrative> ParseChoiceResponse(string response, List<CardDefinition> choices)
    {
        Dictionary<CardDefinition, ChoiceNarrative> result = new Dictionary<CardDefinition, ChoiceNarrative>();

        if (string.IsNullOrWhiteSpace(response) || choices == null || choices.Count == 0)
        {
            return result;
        }

        try
        {
            // Clean up the response to remove markdown code blocks
            response = response.Replace("```json", "").Replace("```", "");

            // Extract JSON content
            string jsonContent = ExtractJsonContent(response);
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                return result;
            }

            // Fix common JSON errors before parsing
            jsonContent = FixCommonJsonErrors(jsonContent);

            // Try to parse the JSON
            try
            {
                JsonDocumentOptions documentOptions = new JsonDocumentOptions
                {
                    AllowTrailingCommas = true,
                    CommentHandling = JsonCommentHandling.Skip
                };

                using (JsonDocument document = JsonDocument.Parse(jsonContent, documentOptions))
                {
                    JsonElement root = document.RootElement;
                    ProcessJsonElement(root, choices, result);
                }
            }
            catch (JsonException)
            {
                // If JSON parsing fails, try to extract individual choices using regex
                ExtractChoicesWithRegex(jsonContent, choices, result);
            }
        }
        catch (Exception ex)
        {
            // Handle any other exceptions
            // Consider logging the error here if needed
        }

        return result;
    }

    private static void ProcessJsonElement(JsonElement root, List<CardDefinition> choices, Dictionary<CardDefinition, ChoiceNarrative> result)
    {
        // Try to process as an object with a "choices" property
        if (root.TryGetProperty("choices", out JsonElement choicesElement) &&
            choicesElement.ValueKind == JsonValueKind.Array)
        {
            ProcessChoicesArray(choicesElement, choices, result);
            return;
        }

        // Try to process as a direct array
        if (root.ValueKind == JsonValueKind.Array)
        {
            ProcessChoicesArray(root, choices, result);
            return;
        }

        // Try to process as individual choice objects
        if (root.ValueKind == JsonValueKind.Object)
        {
            ProcessIndividualChoices(root, choices, result);
        }
    }

    private static void ProcessChoicesArray(JsonElement arrayElement, List<CardDefinition> choices, Dictionary<CardDefinition, ChoiceNarrative> result)
    {
        if (arrayElement.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        int index = 0;
        foreach (JsonElement choice in arrayElement.EnumerateArray())
        {
            if (index >= choices.Count) break;

            // Try to extract choice data using multiple approaches
            string name = "";
            string description = "";

            // Check for named properties
            name = GetStringProperty(choice, "name", "");
            description = GetStringProperty(choice, "description", "");

            // If name is empty, try "title" property
            if (string.IsNullOrWhiteSpace(name))
            {
                name = GetStringProperty(choice, "title", "");
            }

            // If description is empty, try "text" or "content" properties
            if (string.IsNullOrWhiteSpace(description))
            {
                description = GetStringProperty(choice, "text",
                               GetStringProperty(choice, "content", ""));
            }

            // If we have a name or description, create the ChoiceNarrative
            if (!string.IsNullOrWhiteSpace(name) || !string.IsNullOrWhiteSpace(description))
            {
                result[choices[index]] = new ChoiceNarrative(name, description);
            }

            index++;
        }
    }

    private static void ProcessIndividualChoices(JsonElement root, List<CardDefinition> choices, Dictionary<CardDefinition, ChoiceNarrative> result)
    {
        // This handles a non-standard format where the root object might have choice1, choice2 properties

        for (int i = 0; i < choices.Count; i++)
        {
            // Try different property name patterns for choices
            string[] possiblePropertyNames = new[]
            {
                "choice" + (i+1), "option" + (i+1), "choice_" + (i+1), "option_" + (i+1),
                "choice " + (i+1), "option " + (i+1), "choice-" + (i+1), "option-" + (i+1),
                "choice" + i, "option" + i, "choice_" + i, "option_" + i,
                "choice " + i, "option " + i, "choice-" + i, "option-" + i
            };

            foreach (string propName in possiblePropertyNames)
            {
                if (root.TryGetProperty(propName, out JsonElement choiceElement))
                {
                    string name = "";
                    string description = "";

                    if (choiceElement.ValueKind == JsonValueKind.Object)
                    {
                        name = GetStringProperty(choiceElement, "name", "");
                        description = GetStringProperty(choiceElement, "description", "");

                        // Try alternative property names
                        if (string.IsNullOrWhiteSpace(name))
                        {
                            name = GetStringProperty(choiceElement, "title", "");
                        }

                        if (string.IsNullOrWhiteSpace(description))
                        {
                            description = GetStringProperty(choiceElement, "text",
                                           GetStringProperty(choiceElement, "content", ""));
                        }
                    }
                    else if (choiceElement.ValueKind == JsonValueKind.String)
                    {
                        // If the property is a string, use it as the description
                        description = choiceElement.GetString() ?? "";
                    }

                    if (!string.IsNullOrWhiteSpace(name) || !string.IsNullOrWhiteSpace(description))
                    {
                        result[choices[i]] = new ChoiceNarrative(name, description);
                        break; // Found a match for this choice, move to next
                    }
                }
            }
        }
    }

    private static void ExtractChoicesWithRegex(string jsonContent, List<CardDefinition> choices, Dictionary<CardDefinition, ChoiceNarrative> result)
    {
        // Use regex to extract choices when JSON parsing fails completely
        try
        {
            // Pattern to extract name and description
            string pattern = @"""name""?\s*:\s*""([^""\\]*(?:\\.[^""\\]*)*)""[\s,]*(?:""description""?\s*:\s*""([^""\\]*(?:\\.[^""\\]*)*)""|""text""?\s*:\s*""([^""\\]*(?:\\.[^""\\]*)*)"")?";

            MatchCollection matches = Regex.Matches(jsonContent, pattern, RegexOptions.Singleline);

            for (int i = 0; i < Math.Min(matches.Count, choices.Count); i++)
            {
                Match match = matches[i];
                string name = match.Groups[1].Value.Replace("\\\"", "\"");

                // Get description from either group 2 or 3, whichever has a value
                string description = string.IsNullOrEmpty(match.Groups[2].Value) ?
                                     match.Groups[3].Value : match.Groups[2].Value;
                description = description.Replace("\\\"", "\"");

                if (!string.IsNullOrWhiteSpace(name) || !string.IsNullOrWhiteSpace(description))
                {
                    result[choices[i]] = new ChoiceNarrative(name, description);
                }
            }
        }
        catch
        {
            // Ignore regex errors - this is a last resort attempt
        }
    }

    private static string GetStringProperty(JsonElement element, string propertyName, string defaultValue)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property))
        {
            if (property.ValueKind == JsonValueKind.String)
            {
                string value = property.GetString() ?? defaultValue;
                return !string.IsNullOrWhiteSpace(value) ? value : defaultValue;
            }
            else if (property.ValueKind == JsonValueKind.Object ||
                     property.ValueKind == JsonValueKind.Array)
            {
                // Try to get the value as JSON string
                try
                {
                    return property.ToString();
                }
                catch
                {
                    return defaultValue;
                }
            }
        }
        return defaultValue;
    }

    private static string ExtractJsonContent(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "{}";

        string content = text.Trim();

        // Try to find JSON object or array
        int objectStart = content.IndexOf('{');
        int arrayStart = content.IndexOf('[');

        // Determine which comes first (if any)
        int startMarker = -1;
        char startChar = '{';

        if (objectStart >= 0 && (arrayStart < 0 || objectStart < arrayStart))
        {
            startMarker = objectStart;
            startChar = '{';
        }
        else if (arrayStart >= 0)
        {
            startMarker = arrayStart;
            startChar = '[';
        }

        if (startMarker < 0)
        {
            return "{}"; // Return empty object if no JSON found
        }

        content = content.Substring(startMarker);

        // Find corresponding closing bracket/brace
        char closeChar = (startChar == '{') ? '}' : ']';

        int openCount = 1;
        int closeCount = 0;
        int endMarker = -1;
        bool inString = false;

        for (int i = 1; i < content.Length; i++)
        {
            char current = content[i];

            // Handle string literals correctly (don't count braces inside strings)
            if (current == '"' && (i == 0 || content[i - 1] != '\\'))
            {
                inString = !inString;
                continue;
            }

            if (inString) continue; // Skip any character inside strings

            if (current == startChar) openCount++;
            else if (current == closeChar) closeCount++;

            if (openCount == closeCount)
            {
                endMarker = i;
                break;
            }
        }

        if (endMarker >= 0)
        {
            return content.Substring(0, endMarker + 1);
        }

        // If we couldn't find matching braces, return empty object
        return "{}";
    }

    private static string FixCommonJsonErrors(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return "{}";

        // Fix missing quotes around property names
        json = Regex.Replace(json, @"(\{|\,)\s*([a-zA-Z0-9_]+)\s*:", "$1\"$2\":");

        // Fix single quotes used instead of double quotes
        json = Regex.Replace(json, @"'([^']*)'", "\"$1\"");

        // Fix trailing commas in arrays and objects
        json = Regex.Replace(json, @",(\s*[\}\]])", "$1");

        // Fix common contractions
        json = json.Replace("isn\"t", "isn't")
                  .Replace("aren\"t", "aren't")
                  .Replace("doesn\"t", "doesn't")
                  .Replace("didn\"t", "didn't")
                  .Replace("won\"t", "won't")
                  .Replace("can\"t", "can't")
                  .Replace("don\"t", "don't")
                  .Replace("wouldn\"t", "wouldn't")
                  .Replace("couldn\"t", "couldn't")
                  .Replace("shouldn\"t", "shouldn't")
                  .Replace("hasn\"t", "hasn't")
                  .Replace("haven\"t", "haven't");

        // Fix apostrophes more generally (looking for word"s patterns)
        json = Regex.Replace(json, "([a-zA-Z])\"([a-zA-Z])", "$1'$2");

        // Replace escaped quotes that might be breaking the JSON
        json = json.Replace("\\\"", "'");

        return json;
    }

    public static string ExtractNarrativeContext(string fullText)
    {
        if (string.IsNullOrWhiteSpace(fullText))
            return "";

        string text = fullText.Trim();

        // Find the first JSON structure
        int jsonObjectStart = text.IndexOf('{');
        int jsonArrayStart = text.IndexOf('[');

        int jsonStart = -1;
        if (jsonObjectStart >= 0 && jsonArrayStart >= 0)
        {
            jsonStart = Math.Min(jsonObjectStart, jsonArrayStart);
        }
        else if (jsonObjectStart >= 0)
        {
            jsonStart = jsonObjectStart;
        }
        else if (jsonArrayStart >= 0)
        {
            jsonStart = jsonArrayStart;
        }

        // If we found a JSON structure, return everything before it
        if (jsonStart > 0)
        {
            return text.Substring(0, jsonStart).Trim();
        }

        // If no JSON found, return the whole text
        return text;
    }
}