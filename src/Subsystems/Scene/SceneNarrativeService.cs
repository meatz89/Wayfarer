
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
    // TODO: Add OllamaClient and PromptBuilder when implementing full AI generation
    // private readonly OllamaClient _ollamaClient;
    // private readonly PromptBuilder _promptBuilder;

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
    /// SYNCHRONOUS FOR NOW: Made synchronous to avoid breaking SceneInstantiator.FinalizeScene()
    /// TODO: Make async when implementing full AI generation (requires making full call chain async)
    ///
    /// AI Generation Flow (future async implementation):
    /// 1. Build prompt from context.ToDictionary() + narrative hints
    /// 2. Call OllamaClient with 5 second timeout
    /// 3. Parse AI response into situation description
    /// 4. Fallback on timeout/failure
    /// </summary>
    /// <param name="context">Complete entity context with NPC/Location/Player objects</param>
    /// <param name="narrativeHints">Tone, theme, context, style from SituationTemplate</param>
    /// <returns>Generated narrative text for Situation.Description</returns>
    public string GenerateSituationNarrative(ScenePromptContext context, NarrativeHints narrativeHints)
    {
        // Validate required context
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        // TODO: Implement full async AI generation
        // For now, return fallback narrative based on context

        // FALLBACK: Generate contextual placeholder narrative
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
    /// TODO: Implement full AI generation using OllamaClient
    ///
    /// Future implementation pattern (following AINarrativeProvider):
    /// 1. Build prompt from context.ToDictionary() + narrativeHints
    /// 2. string prompt = promptBuilder.BuildSituationPrompt(contextDict, hints)
    /// 3. using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5))
    /// 4. string aiResponse = await ollamaClient.GenerateAsync(prompt, cts.Token)
    /// 5. Parse AI response (extract narrative text, handle formatting)
    /// 6. Return generated narrative or fallback on timeout/error
    /// </summary>
    private async Task<string> GenerateAISituationNarrativeAsync(ScenePromptContext context, NarrativeHints hints)
    {
        // TODO: Implement AI generation
        // For now, this method is a placeholder for future implementation
        await Task.CompletedTask;
        return null;
    }
}
