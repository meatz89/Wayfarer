using System.Text.Json;
using System.Text.RegularExpressions;

public class EncounterChoiceResponseParser
{
    private ILogger<EncounterChoiceResponseParser> _logger;

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
        if (choiceElement.TryGetProperty("template", out JsonElement templateElement))
        {
            choice.TemplateUsed = templateElement.GetString();
        }
        else if (choiceElement.TryGetProperty("templateUsed", out JsonElement templateUsedElement))
        {
            choice.TemplateUsed = templateUsedElement.GetString();
        }

        if (choiceElement.TryGetProperty("templatePurpose", out JsonElement templatePurposeElement))
        {
            choice.TemplatePurpose = templatePurposeElement.GetString();
        }

        // Parse skill options
        if (choiceElement.TryGetProperty("skillOption", out JsonElement skillOptionElement))
        {
            choice.SkillOption = ParseSkillOptionFromJson(skillOptionElement);
        }

        return choice;
    }

    private SkillOption ParseSkillOptionFromJson(JsonElement skillOptionElement)
    {
        SkillOption skillOption = new SkillOption();

        // Parse the skill name
        if (skillOptionElement.TryGetProperty("skillName", out JsonElement skillNameElement) ||
            skillOptionElement.TryGetProperty("SkillName", out skillNameElement))
        {
            skillOption.SkillName = skillNameElement.GetString();
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
            skillOption.SuccessEffectEntry = ParseEffectFromJson(successEffectElement);
        }

        // Parse the failure effect
        if (skillOptionElement.TryGetProperty("failureEffect", out JsonElement failureEffectElement) ||
            skillOptionElement.TryGetProperty("FailureEffect", out failureEffectElement))
        {
            skillOption.FailureEffectEntry = ParseEffectFromJson(failureEffectElement);
        }

        return skillOption;
    }

    private EffectEntry ParseEffectFromJson(JsonElement effectElement)
    {
        EffectEntry effect = new EffectEntry();

        // Parse the narrative effect
        if (effectElement.TryGetProperty("narrativeEffect", out JsonElement narrativeEffectElement) ||
            effectElement.TryGetProperty("NarrativeEffect", out narrativeEffectElement))
        {
            effect.Effect = narrativeEffectElement.GetString();
        }

        // Parse the mechanical effect ID
        if (effectElement.TryGetProperty("mechanicalEffectID", out JsonElement mechanicalEffectIDElement) ||
            effectElement.TryGetProperty("MechanicalEffectID", out mechanicalEffectIDElement))
        {
            effect.ID = mechanicalEffectIDElement.GetString();
        }

        return effect;
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