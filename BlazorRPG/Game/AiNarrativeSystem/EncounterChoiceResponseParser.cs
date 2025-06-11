using System.Text.Json;
using System.Text.RegularExpressions;

public class EncounterChoiceResponseParser
{
    private ILogger<EncounterChoiceResponseParser> _logger;

    // Add these fields to EncounterChoiceResponseParser
    private readonly string _templateFile = "UniqueTemplates.txt";
    private readonly string _skillNameFile = "UniqueSkillNames.txt";
    private readonly string _effectFile = "UniqueEffects.txt";

    public EncounterChoiceResponseParser(ILogger<EncounterChoiceResponseParser> logger = null)
    {
        _logger = logger;
    }

    public List<EncounterChoice> ParseMultipleChoicesResponse(string responseString)
    {
        List<EncounterChoice> choices = new List<EncounterChoice>();

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
                    EncounterChoice choice = ParseChoiceFromJson(choiceElement);
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
                    EncounterChoice choice = ParseChoiceFromJson(choiceElement);
                    if (choice != null)
                    {
                        choices.Add(choice);
                    }
                }
            }
            else if (document.RootElement.ValueKind == JsonValueKind.Object)
            {
                // Response is a single choice object
                EncounterChoice choice = ParseChoiceFromJson(document.RootElement);
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

    private EncounterChoice ParseChoiceFromJson(JsonElement choiceElement)
    {
        EncounterChoice choice = new EncounterChoice();

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

        // Parse template information
        string templateValue = null;
        if (choiceElement.TryGetProperty("template", out JsonElement templateElement))
        {
            templateValue = templateElement.GetString();
            choice.TemplateUsed = templateValue;
        }
        else if (choiceElement.TryGetProperty("templateUsed", out JsonElement templateUsedElement))
        {
            templateValue = templateUsedElement.GetString();
            choice.TemplateUsed = templateValue;
        }
        if (!string.IsNullOrWhiteSpace(templateValue))
        {
            AppendUniqueValueToFile(_templateFile, templateValue);
        }

        if (choiceElement.TryGetProperty("templatePurpose", out JsonElement templatePurposeElement))
        {
            choice.TemplatePurpose = templatePurposeElement.GetString();
        }

        // Parse skill options
        if (choiceElement.TryGetProperty("skillOption", out JsonElement skillOptionElement))
        {
            choice.SkillOption = ParseSkillOptionFromJson(choice, skillOptionElement);
        }

        return choice;
    }

    private SkillOption ParseSkillOptionFromJson(EncounterChoice choice, JsonElement skillOptionElement)
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

        // Parse the SCD (Skill Check Difficulty)
        if (skillOptionElement.TryGetProperty("sCD", out JsonElement scdElement) ||
            skillOptionElement.TryGetProperty("SCD", out scdElement))
        {
            skillOption.SCD = scdElement.GetInt32();
        }

        // Parse the success effect
        if (skillOptionElement.TryGetProperty("successEffect", out JsonElement successEffectElement) ||
            skillOptionElement.TryGetProperty("SuccessEffect", out successEffectElement))
        {
            choice.SuccessNarrative = successEffectElement.ToString();
            if (choice.SuccessNarrative != null && !string.IsNullOrWhiteSpace(choice.SuccessNarrative))
            {
                AppendUniqueValueToFile(_effectFile, choice.SuccessNarrative);
            }
        }

        // Parse the failure effect
        if (skillOptionElement.TryGetProperty("failureEffect", out JsonElement failureEffectElement) ||
            skillOptionElement.TryGetProperty("FailureEffect", out failureEffectElement))
        {
            choice.FailureNarrative = failureEffectElement.ToString();
            if (choice.FailureNarrative != null && !string.IsNullOrWhiteSpace(choice.FailureNarrative))
            {
                AppendUniqueValueToFile(_effectFile, choice.FailureNarrative);
            }
        }

        // Parse the success effect
        skillOption.SuccessEffect = choice.ChoiceTemplate?.SuccessEffect;

        // Parse the failure effect
        skillOption.FailureEffect = choice.ChoiceTemplate?.FailureEffect;
        return skillOption;
    }


    // Utility method to append unique value to file or increment a number next to entry
    private void AppendUniqueValueToFile(string filePath, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        try
        {
            // Ensure file exists
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "");
            }

            var lines = File.ReadAllLines(filePath).ToList();
            bool found = false;
            for (int i = 0; i < lines.Count; i++)
            {
                // Match "value" or "value|count"
                var line = lines[i];
                var parts = line.Split('|');
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
    
    private List<EncounterChoice> ExtractChoicesFromText(string text)
    {
        List<EncounterChoice> choices = new List<EncounterChoice>();

        // Try to find JSON blocks in the text
        Regex jsonRegex = new Regex(@"\{(?:[^{}]|(?<Open>\{)|(?<-Open>\}))+(?(Open)(?!))\}");
        MatchCollection matches = jsonRegex.Matches(text);

        foreach (Match match in matches)
        {
            JsonDocument document = JsonDocument.Parse(match.Value);
            EncounterChoice choice = ParseChoiceFromJson(document.RootElement);
            if (choice != null)
            {
                choices.Add(choice);
            }
        }

        return choices;
    }
}