using System.Text.Json;
using System.Text.RegularExpressions;

public class ConversationChoiceResponseParser
{
    private ILogger<ConversationChoiceResponseParser> _logger;

    // Add these fields to ConversationChoiceResponseParser
    private readonly string _templateFile = "UniqueTemplates.txt";
    private readonly string _skillNameFile = "UniqueSkillNames.txt";
    private readonly string _effectFile = "UniqueEffects.txt";
    private readonly string _templateUsedPurposeFile = "UniqueTemplateUsedPurposes.txt";

    public ConversationChoiceResponseParser(ILogger<ConversationChoiceResponseParser> logger = null)
    {
        _logger = logger;
    }

    public List<ConversationChoice> ParseMultipleChoicesResponse(string responseString)
    {
        List<ConversationChoice> choices = new List<ConversationChoice>();

        // Clean up the response to remove markdown code block formatting
        string cleanedResponse = CleanMarkdownCodeBlocks(responseString);

        // Try to extract JSON object
        string jsonContent = ExtractJsonContent(cleanedResponse);

        if (string.IsNullOrEmpty(jsonContent))
        {
            _logger?.LogWarning("Could not extract valid JSON content from response.");
            return choices;
        }

        // Parse the JSON
        try
        {
            // Try to parse the entire response as a JSON array
            JsonDocument document = JsonDocument.Parse(jsonContent);

            if (document.RootElement.ValueKind == JsonValueKind.Array)
            {
                // Response is an array of choices
                foreach (JsonElement choiceElement in document.RootElement.EnumerateArray())
                {
                    ConversationChoice choice = ParseChoiceFromJson(choiceElement);
                    if (choice != null)
                    {
                        choices.Add(choice);
                    }
                }
            }
            else if (document.RootElement.ValueKind == JsonValueKind.Object &&
                        document.RootElement.TryGetProperty("choices", out JsonElement choicesArray) &&
                        choicesArray.ValueKind == JsonValueKind.Array)
            {
                // Response is an object with a "choices" property
                foreach (JsonElement choiceElement in choicesArray.EnumerateArray())
                {
                    ConversationChoice choice = ParseChoiceFromJson(choiceElement);
                    if (choice != null)
                    {
                        choices.Add(choice);
                    }
                }
            }
            else if (document.RootElement.ValueKind == JsonValueKind.Object)
            {
                // Response is a single choice object
                ConversationChoice choice = ParseChoiceFromJson(document.RootElement);
                if (choice != null)
                {
                    choices.Add(choice);
                }
            }
        }
        catch (JsonException jsonEx)
        {
            _logger?.LogWarning("Failed to parse response as JSON: {Error}", jsonEx.Message);

            // If JSON parsing fails, try to extract JSON from text
            choices = ExtractChoicesFromText(responseString);
        }

        // Add IDs to choices that don't have them
        for (int i = 0; i < choices.Count; i++)
        {
            if (string.IsNullOrEmpty(choices[i].ChoiceID))
            {
                choices[i].ChoiceID = $"choice_{i + 1}";
            }
        }

        _logger?.LogInformation("Successfully parsed {Count} choices", choices.Count);

        return choices;
    }

    private string CleanMarkdownCodeBlocks(string response)
    {
        // Remove markdown code block delimiters
        string cleaned = Regex.Replace(response, "```json", "", RegexOptions.IgnoreCase);
        cleaned = Regex.Replace(cleaned, "```", "");

        // Trim any whitespace
        return cleaned.Trim();
    }

    private string ExtractJsonContent(string text)
    {
        // Look for content between { and }
        Match match = Regex.Match(text, @"\{.*\}", RegexOptions.Singleline);
        if (match.Success)
        {
            return match.Value;
        }

        return string.Empty;
    }

    private ConversationChoice ParseChoiceFromJson(JsonElement choiceElement)
    {
        ConversationChoice choice = new ConversationChoice();

        // Parse the choice ID
        if (choiceElement.TryGetProperty("choiceID", out JsonElement choiceIDElement) ||
            choiceElement.TryGetProperty("ChoiceID", out choiceIDElement))
        {
            choice.ChoiceID = choiceIDElement.GetString();
        }

        // Parse the narrative text
        if (choiceElement.TryGetProperty("narrativeText", out JsonElement narrativeTextElement) ||
            choiceElement.TryGetProperty("NarrativeText", out narrativeTextElement))
        {
            choice.NarrativeText = narrativeTextElement.GetString();
        }

        // Parse the focus cost
        if (choiceElement.TryGetProperty("focusCost", out JsonElement focusCostElement) ||
            choiceElement.TryGetProperty("FocusCost", out focusCostElement))
        {
            choice.FocusCost = focusCostElement.GetInt32();
        }

        if (choiceElement.TryGetProperty("template", out JsonElement templateElement))
        {
            choice.TemplateUsed = templateElement.GetString();

            if (!string.IsNullOrWhiteSpace(choice.TemplateUsed))
            {
                AppendUniqueValueToFile(_templateFile, choice.TemplateUsed);
            }
        }
        else if (choiceElement.TryGetProperty("templateUsed", out JsonElement templateUsedElement))
        {
            choice.TemplateUsed = templateUsedElement.GetString();

            if (!string.IsNullOrWhiteSpace(choice.TemplateUsed))
            {
                AppendUniqueValueToFile(_templateFile, choice.TemplateUsed);
            }
        }

        if (choiceElement.TryGetProperty("templatePurpose", out JsonElement templatePurposeElement))
        {
            choice.TemplatePurpose = templatePurposeElement.GetString();

            if (!string.IsNullOrWhiteSpace(choice.TemplateUsed) && !string.IsNullOrWhiteSpace(choice.TemplatePurpose))
            {
                AppendUniqueTemplateUsedPurposeToFile(_templateUsedPurposeFile, choice.TemplateUsed, choice.TemplatePurpose);
            }
        }

        if (choiceElement.TryGetProperty("successEffect", out JsonElement successEffectElement) ||
            choiceElement.TryGetProperty("SuccessEffect", out successEffectElement))
        {
            choice.SuccessNarrative = successEffectElement.ToString();
            if (choice.SuccessNarrative != null && !string.IsNullOrWhiteSpace(choice.SuccessNarrative))
            {
                AppendUniqueValueToFile(_effectFile, choice.SuccessNarrative);
            }
        }

        if (choiceElement.TryGetProperty("failureEffect", out JsonElement failureEffectElement) ||
            choiceElement.TryGetProperty("FailureEffect", out failureEffectElement))
        {
            choice.FailureNarrative = failureEffectElement.ToString();
            if (choice.FailureNarrative != null && !string.IsNullOrWhiteSpace(choice.FailureNarrative))
            {
                AppendUniqueValueToFile(_effectFile, choice.FailureNarrative);
            }
        }

        if (choiceElement.TryGetProperty("requiresSkillCheck", out JsonElement requiresSkillCheckElement))
        {
            choice.RequiresSkillCheck = EvaluateBool(requiresSkillCheckElement);
        }
        else
        {
            choice.RequiresSkillCheck = choiceElement.TryGetProperty("skillOption", out JsonElement _);
        }

        if (choice.RequiresSkillCheck && choiceElement.TryGetProperty("skillOption", out JsonElement skillOptionElement))
        {
            choice.SkillOption = ParseSkillOptionFromJson(choice, skillOptionElement);
        }

        return choice;
    }

    private static bool EvaluateBool(JsonElement requiresSkillCheckElement)
    {
        bool requiresSkillCheck = false;

        if (requiresSkillCheckElement.ValueKind == JsonValueKind.True)
            requiresSkillCheck = true;
        else if (requiresSkillCheckElement.ValueKind == JsonValueKind.False)
            requiresSkillCheck = false;
        else if (requiresSkillCheckElement.ValueKind == JsonValueKind.String)
        {
            string? str = requiresSkillCheckElement.GetString();
            if (bool.TryParse(str, out bool parsed))
                requiresSkillCheck = parsed;
        }
        else if (requiresSkillCheckElement.ValueKind == JsonValueKind.Number)
        {
            if (requiresSkillCheckElement.TryGetInt32(out int num))
                requiresSkillCheck = num != 0;
        }

        return requiresSkillCheck;
    }

    private SkillOption ParseSkillOptionFromJson(ConversationChoice choice, JsonElement skillOptionElement)
    {
        SkillOption skillOption = new SkillOption();

        // Parse the skill name
        string skillNameValue = null;
        if (skillOptionElement.TryGetProperty("skillName", out JsonElement skillNameElement) ||
            skillOptionElement.TryGetProperty("SkillName", out skillNameElement))
        {
            skillNameValue = skillNameElement.GetString();
            skillOption.SkillName = skillNameValue;
        }
        if (!string.IsNullOrWhiteSpace(skillNameValue))
        {
            AppendUniqueValueToFile(_skillNameFile, skillNameValue);
        }

        // Parse the difficulty
        if (skillOptionElement.TryGetProperty("difficulty", out JsonElement difficultyElement) ||
            skillOptionElement.TryGetProperty("Difficulty", out difficultyElement))
        {
            skillOption.Difficulty = difficultyElement.GetString();
        }

        return skillOption;
    }


    private List<ConversationChoice> ExtractChoicesFromText(string text)
    {
        List<ConversationChoice> choices = new List<ConversationChoice>();

        // Try to find JSON blocks in the text
        Regex jsonRegex = new Regex(@"\{(?:[^{}]|(?<Open>\{)|(?<-Open>\}))+(?(Open)(?!))\}");
        MatchCollection matches = jsonRegex.Matches(text);

        foreach (Match match in matches)
        {
            JsonDocument document = JsonDocument.Parse(match.Value);
            ConversationChoice choice = ParseChoiceFromJson(document.RootElement);
            if (choice != null)
            {
                choices.Add(choice);
            }
        }

        return choices;
    }
    private void EnsureFileExists(string filePath)
    {
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "");
        }
    }

    private void AppendUniqueValueToFile(string filePath, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        try
        {
            EnsureFileExists(filePath);

            List<string> lines = File.ReadAllLines(filePath).ToList();
            bool found = false;
            for (int i = 0; i < lines.Count; i++)
            {
                // Match "value" or "value|count"
                string line = lines[i];
                string[] parts = line.Split('|');
                if (parts[0] == value)
                {
                    int count = 1;
                    if (parts.Length > 1 && int.TryParse(parts[1], out int parsed))
                    {
                        count = parsed + 1;
                    }
                    else
                    {
                        count = 2;
                    }
                    lines[i] = $"{value}|{count}";
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                lines.Add($"{value}|1");
            }
            File.WriteAllLines(filePath, lines);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning("Failed to write unique value to file {File}: {Error}", filePath, ex.Message);
        }
    }

    private void AppendUniqueTemplateUsedPurposeToFile(string filePath, string templateUsed, string templatePurpose)
    {
        if (string.IsNullOrWhiteSpace(templateUsed) || string.IsNullOrWhiteSpace(templatePurpose))
            return;

        string entry = $"{templateUsed}|{templatePurpose}";

        try
        {
            EnsureFileExists(filePath);

            string[] lines = File.ReadAllLines(filePath);
            if (!lines.Contains(entry))
            {
                File.AppendAllText(filePath, entry + Environment.NewLine);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning("Failed to write unique templateUsed|templatePurpose to file {File}: {Error}", filePath, ex.Message);
        }
    }
    private bool TryGetJsonProperty(JsonElement element, string[] propertyNames, out JsonElement value)
    {
        foreach (string name in propertyNames)
        {
            if (element.TryGetProperty(name, out value))
                return true;
        }
        value = default;
        return false;
    }

    private string GetStringProperty(JsonElement element, params string[] propertyNames)
    {
        if (TryGetJsonProperty(element, propertyNames, out JsonElement value))
            return value.GetString();
        return null;
    }

    private int? GetIntProperty(JsonElement element, params string[] propertyNames)
    {
        if (TryGetJsonProperty(element, propertyNames, out JsonElement value) && value.ValueKind == JsonValueKind.Number)
            return value.GetInt32();
        return null;
    }
}