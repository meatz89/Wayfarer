
/// <summary>
/// Service that generates narrative text for Scenes and Situations from entity context.
/// Implements AI generation pattern: Entity context → AI prompt → Generated narrative.
///
/// ARCHITECTURE PRINCIPLE: Called at Scene finalization (SceneInstantiator.FinalizeScene)
/// when complete entity context is available (NPC/Location/Player objects with all properties).
///
/// CURRENT IMPLEMENTATION: Fallback narrative generation
/// FUTURE ENHANCEMENT: Full AI integration via OllamaClient (following AINarrativeProvider pattern)
/// </summary>
public class SceneNarrativeService
{
    private readonly GameWorld _gameWorld;

    public SceneNarrativeService(GameWorld gameWorld)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
    }

    /// <summary>
    /// Generate situation narrative from entity context and narrative hints.
    /// Returns AI-generated text if available, fallback text otherwise.
    ///
    /// CRITICAL: This is called at finalization when all entity context is resolved.
    /// Context contains ACTUAL entity objects (NPC/Location/Route with full properties).
    ///
    /// SYNCHRONOUS: Uses fallback text generation from entity properties.
    /// </summary>
    /// <param name="context">Complete entity context with NPC/Location/Player objects</param>
    /// <param name="narrativeHints">Tone, theme, context, style from SituationTemplate</param>
    /// <returns>Generated narrative text for Situation.Description</returns>
    public string GenerateSituationNarrative(ScenePromptContext context, NarrativeHints narrativeHints)
    {
        // Validate required context
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        // Generate contextual narrative from entity properties
        return GenerateFallbackSituationNarrative(context, narrativeHints);
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
    /// Uses multiple properties to create descriptive names
    /// </summary>
    public string GenerateLocationName(PlacementFilter filter)
    {
        if (filter == null)
            return "Unknown Location";

        List<string> nameParts = new List<string>();

        // Capabilities are flags enum, no ordered list - skip prefix generation

        // Add location type
        if (filter.LocationType.HasValue)
        {
            string typeWord = filter.LocationType.Value switch
            {
                LocationTypes.Inn => "Inn",
                LocationTypes.Tavern => "Tavern",
                LocationTypes.Market => "Market",
                LocationTypes.Shop => "Shop",
                LocationTypes.Temple => "Temple",
                LocationTypes.Palace => "Palace",
                LocationTypes.Guild => "Guild",
                LocationTypes.Crossroads => "Crossroads",
                _ => "Place"
            };
            nameParts.Add(typeWord);
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

        if (filter.TerrainType != null)
        {
            string terrainName = filter.TerrainType.ToLower() switch
            {
                "forest" => "Forest Path",
                "mountain" => "Mountain Trail",
                "plains" => "Open Road",
                "river" => "River Crossing",
                "urban" => "City Street",
                "wilderness" => "Wilderness Track",
                _ => $"{filter.TerrainType} Route"
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
