using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

public static class NarrativeJsonParser
{
    public static Dictionary<EncounterOption, ChoiceNarrative> ParseChoiceResponse(string response, List<EncounterOption> choices)
    {
        Dictionary<EncounterOption, ChoiceNarrative> result = new Dictionary<EncounterOption, ChoiceNarrative>();

        if (string.IsNullOrWhiteSpace(response))
        {
            return result;
        }

        try
        {
            // Check if the entire response is wrapped in quotes, and remove them
            if (response.StartsWith("\"") && response.EndsWith("\"") && response.Length > 2)
            {
                response = response.Substring(1, response.Length - 2);
            }

            // Clean up the response to remove markdown code blocks
            response = response.Replace("```json", "").Replace("```", "");

            // Extract the JSON content
            string jsonContent = ExtractJsonContent(response);
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                return result;
            }

            // Try hardcore manual parsing first (most resilient)
            if (TryParseChoicesManually(jsonContent, choices, result))
            {
                NormalizeAllDescriptions(result);
                return result;
            }

            // Try to fix the JSON and parse it properly
            try
            {
                // Pre-process to handle the unescaped quotes
                jsonContent = PreProcessJson(jsonContent);

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

                NormalizeAllDescriptions(result);
            }
            catch (JsonException ex)
            {
                // If JSON parsing fails, try to extract with regex
                if (!ExtractChoicesWithRegex(jsonContent, choices, result))
                {
                    // If regex fails too, try the most aggressive line-by-line parsing
                    FallbackLineParser(jsonContent, choices, result);
                }

                NormalizeAllDescriptions(result);
            }
        }
        catch (Exception ex)
        {
            // Last-ditch attempt with most aggressive parser
            try
            {
                FallbackLineParser(response, choices, result);
                NormalizeAllDescriptions(result);
            }
            catch
            {
                // Nothing more we can do
            }
        }

        return result;
    }

    private static void NormalizeAllDescriptions(Dictionary<EncounterOption, ChoiceNarrative> choices)
    {
        foreach (EncounterOption key in choices.Keys.ToList())
        {
            ChoiceNarrative narrative = choices[key];
            if (!string.IsNullOrEmpty(narrative.FullDescription))
            {
                narrative.FullDescription = NormalizeText(narrative.FullDescription);
                choices[key] = narrative;
            }
        }
    }

    private static string NormalizeText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        // Remove carriage returns
        text = text.Replace("\r", "");

        // Replace multiple consecutive newlines with a single newline
        text = Regex.Replace(text, @"\n{2,}", "\n");

        // Remove line breaks that split sentences or paragraphs incorrectly
        // This regex looks for lines that don't end with typical sentence endings
        text = Regex.Replace(text, @"([^\.\?\!\""])\n", "$1 ");

        // Clean up any double spaces
        text = Regex.Replace(text, @"\s{2,}", " ");

        // Fix spacing after periods, question marks, and exclamation points
        text = Regex.Replace(text, @"([\.\?\!])\s*([A-Z])", "$1 $2");

        // Ensure standard apostrophes are used
        text = text.Replace("doesn\"t", "doesn't")
                   .Replace("don\"t", "don't")
                   .Replace("isn\"t", "isn't")
                   .Replace("wouldn\"t", "wouldn't")
                   .Replace("couldn\"t", "couldn't")
                   .Replace("shouldn\"t", "shouldn't")
                   .Replace("hasn\"t", "hasn't")
                   .Replace("haven\"t", "haven't")
                   .Replace("won\"t", "won't")
                   .Replace("didn\"t", "didn't");

        // Convert any strange quote characters to standard ones
        text = Regex.Replace(text, @"[""]", "\"");

        return text.Trim();
    }

    private static bool TryParseChoicesManually(string jsonContent, List<EncounterOption> choices, Dictionary<EncounterOption, ChoiceNarrative> result)
    {
        try
        {
            // Extract choices by finding all name/description pairs
            List<KeyValuePair<string, string>> extractedChoices = new List<KeyValuePair<string, string>>();

            // First find the choices array
            int choicesStart = jsonContent.IndexOf("\"choices\"");
            if (choicesStart < 0) return false;

            int arrayStart = jsonContent.IndexOf('[', choicesStart);
            if (arrayStart < 0) return false;

            int arrayEnd = -1;
            int openBrackets = 1;
            bool inString = false;
            bool escaped = false;

            // Find the end of the choices array
            for (int i = arrayStart + 1; i < jsonContent.Length; i++)
            {
                char c = jsonContent[i];

                if (escaped)
                {
                    escaped = false;
                    continue;
                }

                if (c == '\\')
                {
                    escaped = true;
                    continue;
                }

                if (c == '"' && !escaped)
                {
                    inString = !inString;
                    continue;
                }

                if (!inString)
                {
                    if (c == '[') openBrackets++;
                    else if (c == ']')
                    {
                        openBrackets--;
                        if (openBrackets == 0)
                        {
                            arrayEnd = i;
                            break;
                        }
                    }
                }
            }

            if (arrayEnd < 0) return false;

            // Extract the choices array content
            string choicesArray = jsonContent.Substring(arrayStart + 1, arrayEnd - arrayStart - 1);

            // Split into individual choice objects
            List<string> choiceObjects = new List<string>();
            int objectStart = -1;
            openBrackets = 0;
            inString = false;
            escaped = false;

            for (int i = 0; i < choicesArray.Length; i++)
            {
                char c = choicesArray[i];

                if (escaped)
                {
                    escaped = false;
                    continue;
                }

                if (c == '\\')
                {
                    escaped = true;
                    continue;
                }

                if (c == '"' && !escaped)
                {
                    inString = !inString;
                    continue;
                }

                if (!inString)
                {
                    if (c == '{')
                    {
                        if (openBrackets == 0)
                        {
                            objectStart = i;
                        }
                        openBrackets++;
                    }
                    else if (c == '}')
                    {
                        openBrackets--;
                        if (openBrackets == 0 && objectStart >= 0)
                        {
                            choiceObjects.Add(choicesArray.Substring(objectStart, i - objectStart + 1));
                            objectStart = -1;
                        }
                    }
                }
            }

            // Extract name and description from each choice
            foreach (string choiceObj in choiceObjects)
            {
                string name = "";
                string description = "";

                // Extract name
                int nameStart = choiceObj.IndexOf("\"name\"");
                if (nameStart >= 0)
                {
                    nameStart = choiceObj.IndexOf('"', nameStart + 6);
                    if (nameStart >= 0)
                    {
                        int nameEnd = FindStringEnd(choiceObj, nameStart + 1);
                        if (nameEnd > nameStart)
                        {
                            name = choiceObj.Substring(nameStart + 1, nameEnd - nameStart - 1);
                        }
                    }
                }

                // Extract description
                int descStart = choiceObj.IndexOf("\"description\"");
                if (descStart >= 0)
                {
                    descStart = choiceObj.IndexOf('"', descStart + 13);
                    if (descStart >= 0)
                    {
                        int descEnd = FindStringEnd(choiceObj, descStart + 1);
                        if (descEnd > descStart)
                        {
                            description = choiceObj.Substring(descStart + 1, descEnd - descStart - 1);
                            // Unescape quotes
                            description = description.Replace("\\\"", "\"").Replace("\\\\", "\\");
                        }
                    }
                }

                if (!string.IsNullOrEmpty(name) || !string.IsNullOrEmpty(description))
                {
                    extractedChoices.Add(new KeyValuePair<string, string>(name, description));
                }
            }

            // Map the extracted choices to the provided ones
            for (int i = 0; i < Math.Min(extractedChoices.Count, choices.Count); i++)
            {
                result[choices[i]] = new ChoiceNarrative(
                    extractedChoices[i].Key,
                    NormalizeText(extractedChoices[i].Value)
                );
            }

            return extractedChoices.Count > 0;
        }
        catch
        {
            return false;
        }
    }

    private static int FindStringEnd(string text, int startIndex)
    {
        bool escaped = false;

        for (int i = startIndex; i < text.Length; i++)
        {
            if (escaped)
            {
                escaped = false;
                continue;
            }

            if (text[i] == '\\')
            {
                escaped = true;
                continue;
            }

            if (text[i] == '"')
            {
                return i;
            }
        }

        return -1;
    }

    private static int FindClosingQuote(string text)
    {
        for (int i = 0; i < text.Length; i++)
        {
            // If we find a non-escaped quote, it's the end
            if (text[i] == '"' && (i == 0 || text[i - 1] != '\\'))
            {
                return i;
            }
        }
        return -1;
    }

    private static string PreProcessJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return "{}";

        // Initial pass to handle embedded quotes in descriptions
        StringBuilder initialPass = new StringBuilder();
        bool inString = false;
        bool inDescription = false;
        bool escaped = false;

        for (int i = 0; i < json.Length; i++)
        {
            char c = json[i];

            if (escaped)
            {
                initialPass.Append(c);
                escaped = false;
                continue;
            }

            if (c == '\\')
            {
                initialPass.Append(c);
                escaped = true;
                continue;
            }

            if (c == '"' && !escaped)
            {
                if (!inString)
                {
                    inString = true;
                    initialPass.Append(c);

                    // Check if we're entering a description field
                    if (i >= 14 && json.Substring(Math.Max(0, i - 14), Math.Min(14, i)).Contains("description"))
                    {
                        inDescription = true;
                    }
                }
                else
                {
                    // If we're in a description, be more careful about ending the string
                    if (inDescription)
                    {
                        // Look ahead to see if this is really the end of the description
                        int j = i + 1;
                        while (j < json.Length && char.IsWhiteSpace(json[j])) j++;

                        if (j < json.Length && (json[j] == ',' || json[j] == '}'))
                        {
                            // This is the actual end of the description
                            inString = false;
                            inDescription = false;
                            initialPass.Append(c);
                        }
                        else
                        {
                            // This is an embedded quote in the description, escape it
                            initialPass.Append('\\').Append(c);
                        }
                    }
                    else
                    {
                        // Standard string end detection
                        inString = false;
                        initialPass.Append(c);
                    }
                }
            }
            else
            {
                initialPass.Append(c);
            }
        }

        json = initialPass.ToString();

        // Second pass with original logic
        StringBuilder processedJson = new StringBuilder();
        inString = false;
        escaped = false;

        for (int i = 0; i < json.Length; i++)
        {
            char c = json[i];

            if (escaped)
            {
                processedJson.Append(c);
                escaped = false;
                continue;
            }

            if (c == '\\')
            {
                processedJson.Append(c);
                escaped = true;
                continue;
            }

            if (c == '"')
            {
                if (!inString)
                {
                    // Start of a string
                    inString = true;
                    processedJson.Append(c);
                }
                else
                {
                    // Potentially end of a string - check if next non-whitespace is a colon or comma
                    int j = i + 1;
                    while (j < json.Length && char.IsWhiteSpace(json[j])) j++;

                    if (j < json.Length && (json[j] == ',' || json[j] == '}' || json[j] == ']' || json[j] == ':'))
                    {
                        // This is an actual end of a JSON string
                        inString = false;
                        processedJson.Append(c);
                    }
                    else
                    {
                        // This is an embedded quote - escape it
                        processedJson.Append('\\').Append(c);
                    }
                }
            }
            else
            {
                processedJson.Append(c);
            }
        }

        json = processedJson.ToString();

        // Fix common issues
        json = FixCommonJsonErrors(json);

        return json;
    }

    private static void ProcessJsonElement(JsonElement root, List<EncounterOption> choices, Dictionary<EncounterOption, ChoiceNarrative> result)
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

    private static void ProcessChoicesArray(JsonElement arrayElement, List<EncounterOption> choices, Dictionary<EncounterOption, ChoiceNarrative> result)
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
                result[choices[index]] = new ChoiceNarrative(name, NormalizeText(description));
            }

            index++;
        }
    }

    private static void ProcessIndividualChoices(JsonElement root, List<EncounterOption> choices, Dictionary<EncounterOption, ChoiceNarrative> result)
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
                        result[choices[i]] = new ChoiceNarrative(name, NormalizeText(description));
                        break; // Found a match for this choice, move to next
                    }
                }
            }
        }
    }

    private static bool ExtractChoicesWithRegex(string jsonContent, List<EncounterOption> choices, Dictionary<EncounterOption, ChoiceNarrative> result)
    {
        try
        {
            // Simple pattern to match "name": "value" pairs
            Regex namePattern = new Regex(@"""name""\s*:\s*""([^""\\]*(\\.[^""\\]*)*)"",?\s*", RegexOptions.Singleline);
            Regex descPattern = new Regex(@"""description""\s*:\s*""([^""\\]*(\\.[^""\\]*)*)"",?\s*", RegexOptions.Singleline);

            // Find all name matches
            MatchCollection nameMatches = namePattern.Matches(jsonContent);

            // Find all description matches
            MatchCollection descMatches = descPattern.Matches(jsonContent);

            // Use the smaller collection to avoid index out of bounds
            int matchCount = Math.Min(nameMatches.Count, descMatches.Count);
            matchCount = Math.Min(matchCount, choices.Count);

            // Extract and create choice narratives
            for (int i = 0; i < matchCount; i++)
            {
                string name = nameMatches[i].Groups[1].Value;
                string description = descMatches[i].Groups[1].Value;

                if (!string.IsNullOrWhiteSpace(name) || !string.IsNullOrWhiteSpace(description))
                {
                    result[choices[i]] = new ChoiceNarrative(name, NormalizeText(description));
                }
            }

            return matchCount > 0;
        }
        catch
        {
            return false;
        }
    }

    private static void FallbackLineParser(string content, List<EncounterOption> choices, Dictionary<EncounterOption, ChoiceNarrative> result)
    {
        // This is the most aggressive parser for when all else fails
        // It simply looks for name/description patterns line by line

        List<string> names = new List<string>();
        List<string> descriptions = new List<string>();

        string[] lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        bool inName = false;
        bool inDescription = false;
        string currentName = "";
        string currentDescription = "";

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();

            // Check for name start
            if (trimmedLine.Contains("\"name\"") || trimmedLine.Contains("\"title\""))
            {
                if (inDescription && !string.IsNullOrWhiteSpace(currentDescription))
                {
                    descriptions.Add(currentDescription);
                    currentDescription = "";
                }

                inName = true;
                inDescription = false;
                currentName = ExtractValue(trimmedLine);

                if (!string.IsNullOrWhiteSpace(currentName))
                {
                    names.Add(currentName);
                    currentName = "";
                    inName = false;
                }

                continue;
            }

            // Check for description start
            if (trimmedLine.Contains("\"description\"") || trimmedLine.Contains("\"text\""))
            {
                if (inName && !string.IsNullOrWhiteSpace(currentName))
                {
                    names.Add(currentName);
                    currentName = "";
                }

                inName = false;
                inDescription = true;
                currentDescription = ExtractValue(trimmedLine);

                if (!string.IsNullOrWhiteSpace(currentDescription) &&
                    (trimmedLine.EndsWith("\",") || trimmedLine.EndsWith("\"")))
                {
                    descriptions.Add(currentDescription);
                    currentDescription = "";
                    inDescription = false;
                }

                continue;
            }

            // Continue collecting multiline values
            if (inName)
            {
                currentName += " " + trimmedLine.Replace("\"", "").Replace(",", "");

                if (trimmedLine.EndsWith("\",") || trimmedLine.EndsWith("\""))
                {
                    names.Add(currentName);
                    currentName = "";
                    inName = false;
                }
            }
            else if (inDescription)
            {
                currentDescription += " " + trimmedLine.Replace("\"", "").Replace(",", "");

                if (trimmedLine.EndsWith("\",") || trimmedLine.EndsWith("\""))
                {
                    descriptions.Add(currentDescription);
                    currentDescription = "";
                    inDescription = false;
                }
            }
        }

        // Add any remaining items
        if (inName && !string.IsNullOrWhiteSpace(currentName))
        {
            names.Add(currentName);
        }

        if (inDescription && !string.IsNullOrWhiteSpace(currentDescription))
        {
            descriptions.Add(currentDescription);
        }

        // Assemble the results
        int count = Math.Min(Math.Min(names.Count, descriptions.Count), choices.Count);

        for (int i = 0; i < count; i++)
        {
            result[choices[i]] = new ChoiceNarrative(names[i], NormalizeText(descriptions[i]));
        }
    }

    private static string ExtractValue(string line)
    {
        int colonPos = line.IndexOf(':');
        if (colonPos < 0) return "";

        string afterColon = line.Substring(colonPos + 1).Trim();

        if (afterColon.StartsWith("\""))
        {
            int endQuote = afterColon.LastIndexOf('"');
            if (endQuote > 0)
            {
                return afterColon.Substring(1, endQuote - 1);
            }
            else if (afterColon.Length > 1)
            {
                return afterColon.Substring(1);
            }
        }

        return "";
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
            string narrativeText = text.Substring(0, jsonStart).Trim();
            return NormalizeText(narrativeText);
        }

        // If no JSON found, return the whole text normalized
        return NormalizeText(text);
    }
}