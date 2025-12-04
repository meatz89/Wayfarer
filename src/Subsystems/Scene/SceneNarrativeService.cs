using System.Text;

/// <summary>
/// Service that generates narrative text for Scenes and Situations from entity context.
/// Implements Two-Pass Procedural Generation (arc42 §8.28):
/// Pass 1 (Mechanical) produces complete Situation entities
/// Pass 2 (AI Narrative) generates contextual narrative text
///
/// ARCHITECTURE PRINCIPLE: Called during SceneInstantiator.ActivateScene() AFTER
/// all Situations are mechanically complete with resolved entities.
///
/// AI INTEGRATION: Uses OllamaClient with 5-second timeout, graceful fallback.
/// </summary>
public class SceneNarrativeService
{
    private readonly GameWorld _gameWorld;
    private readonly OllamaClient _ollamaClient;
    private readonly ScenePromptBuilder _promptBuilder;

    public SceneNarrativeService(
        GameWorld gameWorld,
        OllamaClient ollamaClient,
        ScenePromptBuilder promptBuilder)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _ollamaClient = ollamaClient; // Can be null - graceful degradation
        _promptBuilder = promptBuilder ?? throw new ArgumentNullException(nameof(promptBuilder));
    }

    /// <summary>
    /// Generate situation narrative from entity context and narrative hints.
    /// Async method - tries AI generation with timeout, falls back to template-based.
    ///
    /// CRITICAL: This is called during Pass 2 when all entity context is resolved.
    /// Context contains ACTUAL entity objects (NPC/Location/Route with full properties).
    ///
    /// Pattern from AINarrativeProvider: 5-second timeout, catch OperationCanceledException.
    /// </summary>
    /// <param name="context">Complete entity context with NPC/Location/Player objects</param>
    /// <param name="narrativeHints">Tone, theme, context, style from SituationTemplate</param>
    /// <param name="situation">The Situation being enriched (for mechanical context)</param>
    /// <returns>Generated narrative text for Situation.Description</returns>
    public async Task<string> GenerateSituationNarrativeAsync(
        ScenePromptContext context,
        NarrativeHints narrativeHints,
        Situation situation)
    {
        // Validate required context
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        // Try AI generation if client available
        if (_ollamaClient != null)
        {
            string aiNarrative = await TryGenerateAINarrativeAsync(context, narrativeHints, situation);
            if (!string.IsNullOrEmpty(aiNarrative))
            {
                Console.WriteLine($"[SceneNarrativeService] AI generated narrative for '{situation?.Name ?? "unknown"}'");
                return aiNarrative;
            }
        }

        // Fallback to template-based generation
        Console.WriteLine($"[SceneNarrativeService] Using fallback narrative for '{situation?.Name ?? "unknown"}'");
        return GenerateFallbackSituationNarrative(context, narrativeHints);
    }

    /// <summary>
    /// Try AI narrative generation with timeout.
    /// Pattern from AINarrativeProvider (lines 57-66):
    /// - 5-second timeout via CancellationTokenSource
    /// - Catch OperationCanceledException → return empty
    /// - Return generated text or empty on failure
    /// </summary>
    private async Task<string> TryGenerateAINarrativeAsync(
        ScenePromptContext context,
        NarrativeHints narrativeHints,
        Situation situation)
    {
        try
        {
            // Build prompt
            string prompt = _promptBuilder.BuildSituationPrompt(context, narrativeHints, situation);

            // 5-second timeout (matching AINarrativeProvider)
            using CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            // Stream response and collect
            StringBuilder responseBuilder = new StringBuilder();
            await foreach (string token in _ollamaClient.StreamCompletionAsync(prompt, cts.Token))
            {
                responseBuilder.Append(token);
            }

            string result = responseBuilder.ToString().Trim();

            // Clean up common AI artifacts (remove markdown formatting if present)
            result = CleanAIResponse(result);

            return result;
        }
        catch (OperationCanceledException)
        {
            // Timeout - expected, use fallback
            Console.WriteLine("[SceneNarrativeService] AI generation timed out (5s), using fallback");
            return string.Empty;
        }
        catch (HttpRequestException ex)
        {
            // Network error - expected when Ollama not running
            Console.WriteLine($"[SceneNarrativeService] AI unavailable: {ex.Message}");
            return string.Empty;
        }
        catch (Exception ex)
        {
            // Unexpected error - log and fallback
            Console.WriteLine($"[SceneNarrativeService] AI error: {ex.GetType().Name}: {ex.Message}");
            return string.Empty;
        }
    }

    /// <summary>
    /// Clean common AI response artifacts
    /// </summary>
    private string CleanAIResponse(string response)
    {
        if (string.IsNullOrEmpty(response))
            return response;

        // Remove markdown code blocks if present
        response = response.Replace("```", "").Trim();

        // Remove leading/trailing quotes if wrapped
        if (response.StartsWith("\"") && response.EndsWith("\""))
            response = response.Substring(1, response.Length - 2);

        return response.Trim();
    }

    /// <summary>
    /// Generate fallback situation narrative when AI generation not available/fails.
    /// Creates contextually appropriate description from entity properties.
    /// Pattern: [Setting] + [NPC/Location context] + [Tone/Theme hints]
    /// </summary>
    private string GenerateFallbackSituationNarrative(ScenePromptContext context, NarrativeHints narrativeHints)
    {
        List<string> narrativeParts = new List<string>();

        // Time/Weather context
        narrativeParts.Add(GetTimeWeatherNarrative(context.CurrentTimeBlock, context.CurrentWeather));

        // Entity-specific context
        switch (context.Location)
        {
            case not null when context.NPC != null:
                // NPC at Location
                narrativeParts.Add($"You encounter {context.NPC.Name} at {context.Location.Name}.");
                narrativeParts.Add(GetPersonalityNarrative(context.NPC.PersonalityType));
                break;

            case not null:
                // Location only
                narrativeParts.Add($"You find yourself at {context.Location.Name}.");
                narrativeParts.Add(context.Location.Description ?? "The area stretches before you.");
                break;

            default:
                // NPC only (generic location)
                if (context.NPC != null)
                {
                    narrativeParts.Add($"You encounter {context.NPC.Name}.");
                    narrativeParts.Add(GetPersonalityNarrative(context.NPC.PersonalityType));
                }
                break;
        }

        // Theme/Tone hints (if present)
        if (narrativeHints != null)
        {
            string themeNarrative = GetThemeNarrative(narrativeHints.Theme, narrativeHints.Tone);
            if (!string.IsNullOrEmpty(themeNarrative))
                narrativeParts.Add(themeNarrative);
        }

        return string.Join(" ", narrativeParts);
    }

    /// <summary>
    /// Generate time/weather atmospheric narrative
    /// </summary>
    private string GetTimeWeatherNarrative(TimeBlocks timeBlock, string weather)
    {
        string timePhrase = timeBlock switch
        {
            TimeBlocks.Morning => "Morning light filters through the streets.",
            TimeBlocks.Midday => "The midday sun beats down.",
            TimeBlocks.Afternoon => "Afternoon shadows lengthen.",
            TimeBlocks.Evening => "Evening darkness settles over the area.",
            _ => "Time passes."
        };

        if (!string.IsNullOrEmpty(weather) && weather != "clear")
        {
            return $"{timePhrase} {GetWeatherNarrative(weather)}";
        }

        return timePhrase;
    }

    /// <summary>
    /// Generate weather-specific narrative
    /// </summary>
    private string GetWeatherNarrative(string weather)
    {
        return weather.ToLower() switch
        {
            "rain" => "Rain patters against the buildings.",
            "storm" => "Storm clouds gather ominously.",
            "fog" => "Fog obscures distant shapes.",
            "snow" => "Snow falls silently.",
            "wind" => "Wind whistles through the streets.",
            _ => ""
        };
    }

    /// <summary>
    /// Generate personality-specific NPC behavior narrative
    /// </summary>
    private string GetPersonalityNarrative(PersonalityType personality)
    {
        return personality switch
        {
            PersonalityType.DEVOTED => "They greet you with genuine warmth and concern.",
            PersonalityType.MERCANTILE => "They eye you with the practiced assessment of someone who deals in goods and services.",
            PersonalityType.PROUD => "They carry themselves with unmistakable status and formality.",
            PersonalityType.CUNNING => "Their gaze is sharp, calculating.",
            PersonalityType.STEADFAST => "They regard you with calm, dutiful consideration.",
            _ => "They acknowledge your presence."
        };
    }

    /// <summary>
    /// Generate theme-specific narrative hint
    /// </summary>
    private string GetThemeNarrative(string theme, string tone)
    {
        if (string.IsNullOrEmpty(theme))
            return "";

        // Theme-based contextual hints
        string themeHint = theme.ToLower() switch
        {
            "economic_negotiation" => "The air of commerce hangs between you.",
            "authority_confrontation" => "Tension crackles in the air.",
            "information_exchange" => "They seem to hold information you need.",
            "social_maneuvering" => "The dance of influence and reputation begins.",
            "crisis_response" => "Urgency demands decisive action.",
            _ => ""
        };

        return themeHint;
    }

    /// <summary>
    /// Generate contextual location name from PlacementFilter properties
    /// Uses location role to create descriptive names
    /// </summary>
    public string GenerateLocationName(PlacementFilter filter)
    {
        if (filter == null)
            return "Unknown Location";

        List<string> nameParts = new List<string>();

        // Add location role - the functional/narrative role of the location
        if (filter.LocationRole.HasValue)
        {
            string roleWord = filter.LocationRole.Value switch
            {
                LocationRole.Rest => "Private Room",
                LocationRole.Hub => "Hall",
                LocationRole.Connective => "Passage",
                LocationRole.Landmark => "Landmark",
                LocationRole.Hazard => "Danger Zone",
                _ => "Place"
            };
            nameParts.Add(roleWord);
        }

        return nameParts.Count > 0 ? $"The {string.Join(" ", nameParts)}" : "Unknown Location";
    }

    /// <summary>
    /// Generate contextual NPC name from PlacementFilter properties
    /// Uses profession and personality to create descriptive identifiers
    /// </summary>
    public string GenerateNPCName(PlacementFilter filter)
    {
        if (filter == null)
            return "Unknown Person";

        string professionName = "Person";

        if (filter.Profession.HasValue)
        {
            professionName = filter.Profession.Value switch
            {
                Professions.Innkeeper => "Innkeeper",
                Professions.Merchant => "Merchant",
                Professions.Guard => "Guard",
                Professions.Scholar => "Scholar",
                Professions.Artisan => "Artisan",
                Professions.Noble => "Noble",
                Professions.Courier => "Courier",
                Professions.Healer => "Healer",
                _ => "Person"
            };
        }

        string personalityPrefix = "";
        if (filter.PersonalityType.HasValue)
        {
            personalityPrefix = filter.PersonalityType.Value switch
            {
                PersonalityType.DEVOTED => "Devoted",
                PersonalityType.MERCANTILE => "Shrewd",
                PersonalityType.PROUD => "Proud",
                PersonalityType.CUNNING => "Cunning",
                PersonalityType.STEADFAST => "Steadfast",
                _ => ""
            };
        }

        if (!string.IsNullOrEmpty(personalityPrefix))
            return $"The {personalityPrefix} {professionName}";

        return $"The {professionName}";
    }

    /// <summary>
    /// Generate contextual route name from PlacementFilter properties
    /// Uses terrain types to create descriptive route names
    /// </summary>
    public string GenerateRouteName(PlacementFilter filter)
    {
        if (filter == null)
            return "Unknown Route";

        if (filter.Terrain != null)
        {
            string terrainName = filter.Terrain switch
            {
                TerrainType.Forest => "Forest Path",
                TerrainType.Mountains => "Mountain Trail",
                TerrainType.Plains => "Open Road",
                TerrainType.Road => "Paved Road",
                TerrainType.Water => "River Crossing",
                TerrainType.Swamp => "Swamp Trail",
                _ => $"{filter.Terrain} Route"
            };

            return terrainName;
        }

        // Use difficulty to add context
        if (filter.MinDifficulty.HasValue)
        {
            string difficulty = filter.MinDifficulty.Value switch
            {
                >= 5 => "Treacherous Route",
                >= 3 => "Challenging Path",
                _ => "Easy Route"
            };
            return difficulty;
        }

        return "The Route";
    }
}
