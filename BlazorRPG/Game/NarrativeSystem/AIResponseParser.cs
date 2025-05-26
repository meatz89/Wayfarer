using System.Text.Json;

public class AIResponseParser
{

    public AIResponseParser()
    {
    }

    public AiChoice ParseChoiceResponse(string response)
    {
        // Extract JSON from potential text
        string jsonContent = ExtractJsonFromText(response);

        // Attempt to parse as full response format
        AIResponseFormat fullResponse = JsonSerializer.Deserialize<AIResponseFormat>(jsonContent);

        if (fullResponse != null && fullResponse.AvailableChoices != null && fullResponse.AvailableChoices.Count > 0)
        {
            // Return the first choice from the full response
            return fullResponse.AvailableChoices[0];
        }

        // If full response parsing fails, try parsing as direct AIChoice
        AiChoice directChoice = JsonSerializer.Deserialize<AiChoice>(jsonContent);
        if (directChoice != null)
        {
            return directChoice;
        }

        return null; // Return null if no valid choice found
    }

    public List<AiChoice> ParseMultipleChoicesResponse(string response)
    {
        // Extract JSON from potential text
        string jsonContent = ExtractJsonFromText(response);

        // First try to parse as our expected format with a "choices" array
        ChoicesResponse choicesResponse = JsonSerializer.Deserialize<ChoicesResponse>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (choicesResponse != null && choicesResponse.Choices != null && choicesResponse.Choices.Count > 0)
        {
            return TransformChoices(choicesResponse.Choices);
        }

        // Try to parse as a direct list of AiChoice objects
        List<AiChoice> directChoices = JsonSerializer.Deserialize<List<AiChoice>>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (directChoices != null && directChoices.Count > 0)
        {
            return directChoices;
        }

        // Try to parse as a format with "availableChoices" property
        var availableChoicesResponse = JsonSerializer.Deserialize<AvailableChoicesResponse>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (availableChoicesResponse != null && availableChoicesResponse.AvailableChoices != null && availableChoicesResponse.AvailableChoices.Count > 0)
        {
            return availableChoicesResponse.AvailableChoices;
        }

        // Last attempt to extract a choices array from any JSON object
        var rawJson = JsonDocument.Parse(jsonContent);
        JsonElement rootElement = rawJson.RootElement;

        // Try to find a property that might contain choices
        foreach (JsonProperty property in rootElement.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.Array)
            {
                try
                {
                    List<AiChoice> extractedChoices = JsonSerializer.Deserialize<List<AiChoice>>(
                        property.Value.GetRawText(),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (extractedChoices != null && extractedChoices.Count > 0)
                    {
                        return extractedChoices;
                    }
                }
                catch
                {
                    // Continue to next property if this one doesn't contain valid AiChoice objects
                    continue;
                }
            }
        }

        // If we reach here, no valid choices were found
        return new List<AiChoice> { CreateFallbackChoice() };
    }

    private List<AiChoice> TransformChoices(List<Choice> choices)
    {
        List<AiChoice> result = new List<AiChoice>();

        foreach (Choice choice in choices)
        {
            // Create a new AiChoice with data from the Choice object
            AiChoice aiChoice = new AiChoice
            {
                ChoiceID = choice.ChoiceID ?? $"choice_{choice.Index}",
                NarrativeText = choice.Name ?? choice.NarrativeText,
                FocusCost = choice.FocusCost
            };

            // Handle skill options
            if (choice.SkillOptions != null && choice.SkillOptions.Count > 0)
            {
                aiChoice.SkillOptions = choice.SkillOptions;
            }
            else
            {
                // Create a default skill option if none provided
                aiChoice.SkillOptions = new List<SkillOption>
            {
                new SkillOption
                {
                    SkillName = DetermineSkillFromName(choice.Name ?? choice.NarrativeText),
                    Difficulty = "Standard",
                    SCD = 3,
                    SuccessPayload = new Payload
                    {
                        NarrativeEffect = "You succeed in your attempt.",
                        MechanicalEffectID = choice.FocusCost == 0 ? "GAIN_FOCUS_1" : "SET_FLAG_INSIGHT_GAINED"
                    },
                    FailurePayload = new Payload
                    {
                        NarrativeEffect = "You encounter a setback.",
                        MechanicalEffectID = "ADVANCE_DURATION_1"
                    }
                }
            };
            }

            result.Add(aiChoice);
        }

        return result;
    }

    private string DetermineSkillFromName(string name)
    {
        // Determine skill based on name content
        if (name.Contains("force") || name.Contains("strength") || name.Contains("power"))
            return "Brute Force";
        if (name.Contains("jump") || name.Contains("leap") || name.Contains("climb"))
            return "Acrobatics";
        if (name.Contains("pick") || name.Contains("unlock") || name.Contains("open"))
            return "Lockpicking";
        if (name.Contains("investigate") || name.Contains("examine") || name.Contains("analyze"))
            return "Investigation";
        if (name.Contains("watch") || name.Contains("observe") || name.Contains("notice"))
            return "Perception";
        if (name.Contains("plan") || name.Contains("strategize") || name.Contains("prepare"))
            return "Strategy";
        if (name.Contains("manner") || name.Contains("polite") || name.Contains("proper"))
            return "Etiquette";
        if (name.Contains("convince") || name.Contains("persuade") || name.Contains("bargain"))
            return "Negotiation";
        if (name.Contains("pretend") || name.Contains("deceive") || name.Contains("perform"))
            return "Acting";
        if (name.Contains("threaten") || name.Contains("intimidate") || name.Contains("frighten"))
            return "Threatening";

        // Default to Perception if no match
        return "Perception";
    }

    // Classes for deserialization
    private class ChoicesResponse
    {
        public List<Choice> Choices { get; set; }
    }

    private class AvailableChoicesResponse
    {
        public List<AiChoice> AvailableChoices { get; set; }
    }

    private class Choice
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string ChoiceID { get; set; }
        public string NarrativeText { get; set; }
        public string Description { get; set; }
        public int FocusCost { get; set; }
        public List<SkillOption> SkillOptions { get; set; }
    }

    private class SimpleChoice
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    private string ExtractJsonFromText(string text)
    {
        // Find JSON content between curly braces
        int startIndex = text.IndexOf('{');
        int endIndex = text.LastIndexOf('}');

        if (startIndex >= 0 && endIndex > startIndex)
        {
            return text.Substring(startIndex, endIndex - startIndex + 1);
        }

        return text; // Return original if no JSON found
    }

    private AiChoice CreateFallbackChoice()
    {
        // Create a safe fallback choice when parsing fails
        AiChoice fallback = new AiChoice
        {
            ChoiceID = "fallback_choice",
            NarrativeText = "Continue carefully",
            FocusCost = 1,
            SkillOptions = new List<SkillOption>
            {
                new SkillOption
                {
                    SkillName = "Observation",
                    Difficulty = "Standard",
                    SCD = 3,
                    SuccessPayload = new Payload
                    {
                        NarrativeEffect = "You proceed with caution and avoid complications.",
                        MechanicalEffectID = "GAIN_FOCUS_1"
                    },
                    FailurePayload = new Payload
                    {
                        NarrativeEffect = "You proceed but encounter a minor setback.",
                        MechanicalEffectID = "ADVANCE_DURATION_1"
                    }
                }
            }
        };

        return fallback;
    }

    private class AIResponseFormat
    {
        public string BeatNarration { get; set; }
        public List<AiChoice> AvailableChoices { get; set; }
    }
}