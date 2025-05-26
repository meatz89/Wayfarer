using System.Text.Json;
using System.Text.RegularExpressions;

public class AIResponseParser
{
    private readonly ILogger<AIResponseParser> _logger;

    public AIResponseParser(ILogger<AIResponseParser> logger = null)
    {
        _logger = logger;
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

    public List<AiChoice> ParseMultipleChoicesResponse(string response)
    {
        List<AiChoice> choices = new List<AiChoice>();

        try
        {
            // Clean up the response to remove markdown code block formatting
            string cleanedResponse = CleanMarkdownCodeBlocks(response);

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
                        AiChoice choice = ParseChoiceFromJson(choiceElement);
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
                        AiChoice choice = ParseChoiceFromJson(choiceElement);
                        if (choice != null)
                        {
                            choices.Add(choice);
                        }
                    }
                }
                else if (document.RootElement.ValueKind == JsonValueKind.Object)
                {
                    // Response is a single choice object
                    AiChoice choice = ParseChoiceFromJson(document.RootElement);
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
                choices = ExtractChoicesFromText(response);
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
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error parsing AI response");

            // Create fallback choices
            choices = CreateFallbackChoices();
        }

        return choices;
    }

    private AiChoice ParseChoiceFromJson(JsonElement choiceElement)
    {
        try
        {
            AiChoice choice = new AiChoice();

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

            // Parse skill options
            if (choiceElement.TryGetProperty("skillOptions", out JsonElement skillOptionsElement) ||
                choiceElement.TryGetProperty("SkillOptions", out skillOptionsElement))
            {
                if (skillOptionsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement skillOptionElement in skillOptionsElement.EnumerateArray())
                    {
                        SkillOption skillOption = ParseSkillOptionFromJson(skillOptionElement);
                        if (skillOption != null)
                        {
                            choice.SkillOption = skillOption;
                        }
                    }
                }
            }

            return choice;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error parsing choice from JSON");
            return null;
        }
    }

    private SkillOption ParseSkillOptionFromJson(JsonElement skillOptionElement)
    {
        try
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

            // Parse the success payload
            if (skillOptionElement.TryGetProperty("successPayload", out JsonElement successPayloadElement) ||
                skillOptionElement.TryGetProperty("SuccessPayload", out successPayloadElement))
            {
                skillOption.SuccessPayload = ParsePayloadFromJson(successPayloadElement);
            }

            // Parse the failure payload
            if (skillOptionElement.TryGetProperty("failurePayload", out JsonElement failurePayloadElement) ||
                skillOptionElement.TryGetProperty("FailurePayload", out failurePayloadElement))
            {
                skillOption.FailurePayload = ParsePayloadFromJson(failurePayloadElement);
            }

            return skillOption;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error parsing skill option from JSON");
            return null;
        }
    }

    private Payload ParsePayloadFromJson(JsonElement payloadElement)
    {
        try
        {
            Payload payload = new Payload();

            // Parse the narrative effect
            if (payloadElement.TryGetProperty("narrativeEffect", out JsonElement narrativeEffectElement) ||
                payloadElement.TryGetProperty("NarrativeEffect", out narrativeEffectElement))
            {
                payload.NarrativeEffect = narrativeEffectElement.GetString();
            }

            // Parse the mechanical effect ID
            if (payloadElement.TryGetProperty("mechanicalEffectID", out JsonElement mechanicalEffectIDElement) ||
                payloadElement.TryGetProperty("MechanicalEffectID", out mechanicalEffectIDElement))
            {
                payload.MechanicalEffectID = mechanicalEffectIDElement.GetString();
            }

            return payload;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error parsing payload from JSON");
            return new Payload
            {
                NarrativeEffect = "Error parsing payload",
                MechanicalEffectID = "ADVANCE_DURATION_1" // Safe default
            };
        }
    }

    private List<AiChoice> ExtractChoicesFromText(string text)
    {
        List<AiChoice> choices = new List<AiChoice>();

        // Try to find JSON blocks in the text
        Regex jsonRegex = new Regex(@"\{(?:[^{}]|(?<Open>\{)|(?<-Open>\}))+(?(Open)(?!))\}");
        MatchCollection matches = jsonRegex.Matches(text);

        foreach (Match match in matches)
        {
            try
            {
                JsonDocument document = JsonDocument.Parse(match.Value);
                AiChoice choice = ParseChoiceFromJson(document.RootElement);
                if (choice != null)
                {
                    choices.Add(choice);
                }
            }
            catch (JsonException)
            {
                // Not valid JSON, skip
            }
        }

        // If we still couldn't find any choices, return fallbacks
        if (choices.Count == 0)
        {
            choices = CreateFallbackChoices();
        }

        return choices;
    }

    private List<AiChoice> CreateFallbackChoices()
    {
        _logger?.LogWarning("Creating fallback choices");

        // Create basic fallback choices
        return new List<AiChoice>
        {
            CreateFallbackChoice(0, "Proceed carefully", 1, "Observation"),
            CreateFallbackChoice(1, "Take aggressive action", 2, "BruteForce"),
            CreateFallbackChoice(2, "Try diplomatic approach", 1, "Negotiation"),
            CreateFallbackChoice(3, "Gather more information", 1, "Investigation"),
            CreateFallbackChoice(4, "Take a moment to recover", 0, "Perception")
        };
    }

    private AiChoice CreateFallbackChoice(int index, string narrativeText, int focusCost, string skillName)
    {
        return new AiChoice
        {
            ChoiceID = $"fallback_choice_{index}",
            NarrativeText = narrativeText,
            FocusCost = focusCost,
            SkillOption =
            new SkillOption
            {
                SkillName = skillName,
                Difficulty = "Standard",
                SCD = 3,
                SuccessPayload = new Payload
                {
                    NarrativeEffect = "You succeed in your attempt.",
                    MechanicalEffectID = focusCost == 0 ? "GAIN_FOCUS_1" : "SET_FLAG_INSIGHT_GAINED"
                },
                FailurePayload = new Payload
                {
                    NarrativeEffect = "You encounter a setback.",
                    MechanicalEffectID = "ADVANCE_DURATION_1"
                }
            }
        };
    }
}