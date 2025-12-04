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
/// AI INTEGRATION: Uses IAICompletionProvider with retry + exponential backoff, graceful fallback.
/// TESTABILITY: Accepts interface for mock injection in tests.
///
/// TEST/PRODUCTION PARITY: GetPrompt* methods expose exact prompts that production will use.
/// Tests MUST use these methods to ensure prompt parity - no duplicate prompt building allowed.
/// </summary>
public class SceneNarrativeService
{
    private readonly GameWorld _gameWorld;
    private readonly IAICompletionProvider _aiProvider;
    private readonly ScenePromptBuilder _promptBuilder;

    // Retry configuration
    private const int MaxRetries = 3;
    private const int InitialDelayMs = 1000; // 1 second, doubles each retry
    private const int SituationTimeoutSeconds = 30; // Increased from 20 for slow PCs
    private const int ChoiceLabelTimeoutSeconds = 10; // Increased from 5 for slow PCs

    public SceneNarrativeService(
        GameWorld gameWorld,
        IAICompletionProvider aiProvider,
        ScenePromptBuilder promptBuilder)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _aiProvider = aiProvider; // Can be null - graceful degradation
        _promptBuilder = promptBuilder ?? throw new ArgumentNullException(nameof(promptBuilder));
    }

    // ==================== PROMPT VISIBILITY FOR TEST PARITY ====================

    /// <summary>
    /// Get the exact prompt that would be sent to AI for situation narrative generation.
    /// USE THIS IN TESTS to ensure test prompts match production prompts exactly.
    /// HIGHLANDER: This is the ONLY way to see the prompt - prevents test/production divergence.
    /// </summary>
    public string GetSituationPrompt(ScenePromptContext context, NarrativeHints hints, Situation situation)
    {
        return _promptBuilder.BuildSituationPrompt(context, hints, situation);
    }

    /// <summary>
    /// Get the exact prompt that would be sent to AI for choice label generation.
    /// USE THIS IN TESTS to ensure test prompts match production prompts exactly.
    /// </summary>
    public string GetChoiceLabelPrompt(
        ScenePromptContext context,
        Situation situation,
        ChoiceTemplate choiceTemplate,
        CompoundRequirement scaledRequirement,
        Consequence scaledConsequence)
    {
        return _promptBuilder.BuildChoiceLabelPrompt(context, situation, choiceTemplate, scaledRequirement, scaledConsequence);
    }

    // ==================== EXTENDED GENERATION WITH METADATA ====================

    /// <summary>
    /// Generate situation narrative with full result metadata.
    /// Returns both the narrative AND information about how it was generated.
    /// USE THIS IN TESTS to capture whether fallback was used and why.
    /// </summary>
    public async Task<NarrativeGenerationResult> GenerateSituationNarrativeWithMetadataAsync(
        ScenePromptContext context,
        NarrativeHints narrativeHints,
        Situation situation)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        NarrativeGenerationResult result = new NarrativeGenerationResult
        {
            Prompt = GetSituationPrompt(context, narrativeHints, situation)
        };

        if (_aiProvider != null)
        {
            (string aiNarrative, string failureReason) = await TryGenerateAINarrativeWithRetryAsync(context, narrativeHints, situation);
            if (!string.IsNullOrEmpty(aiNarrative))
            {
                result.Narrative = aiNarrative;
                result.UsedFallback = false;
                return result;
            }
            result.FallbackReason = failureReason;
        }
        else
        {
            result.FallbackReason = "AI provider not configured";
        }

        result.Narrative = GenerateFallbackSituationNarrative(context, narrativeHints);
        result.UsedFallback = true;
        return result;
    }

    /// <summary>
    /// Warm up the AI model by sending a simple prompt.
    /// Call this before running tests to ensure model is loaded into memory.
    /// Returns true if model responded, false if unavailable.
    /// </summary>
    public async Task<bool> WarmupModelAsync(int timeoutSeconds = 60)
    {
        if (_aiProvider == null)
            return false;

        Console.WriteLine("[SceneNarrativeService] Starting model warmup...");

        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                using CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

                StringBuilder responseBuilder = new StringBuilder();
                await foreach (string token in _aiProvider.StreamCompletionAsync("Say 'ready' in one word.", cts.Token))
                {
                    responseBuilder.Append(token);
                    // Got any response = model is warm
                    if (responseBuilder.Length > 0)
                    {
                        Console.WriteLine($"[SceneNarrativeService] Model warmup successful on attempt {attempt}");
                        return true;
                    }
                }

                if (responseBuilder.Length > 0)
                {
                    Console.WriteLine($"[SceneNarrativeService] Model warmup successful on attempt {attempt}");
                    return true;
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"[SceneNarrativeService] Warmup attempt {attempt} timed out ({timeoutSeconds}s)");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[SceneNarrativeService] Warmup attempt {attempt} failed: {ex.Message}");
            }

            if (attempt < MaxRetries)
            {
                int delayMs = InitialDelayMs * (int)Math.Pow(2, attempt - 1);
                Console.WriteLine($"[SceneNarrativeService] Retrying warmup in {delayMs}ms...");
                await Task.Delay(delayMs);
            }
        }

        Console.WriteLine("[SceneNarrativeService] Model warmup failed after all retries");
        return false;
    }

    /// <summary>
    /// Generate situation narrative from entity context and narrative hints.
    /// Async method - tries AI generation with timeout, falls back to template-based.
    ///
    /// CRITICAL: This is called during Pass 2 when all entity context is resolved.
    /// Context contains ACTUAL entity objects (NPC/Location/Route with full properties).
    ///
    /// Pattern from AINarrativeProvider: 20-second timeout (cold-start), catch OperationCanceledException.
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

        // Try AI generation if provider available
        if (_aiProvider != null)
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
    /// Synchronous fallback method for backwards compatibility.
    /// Used when async context not available.
    /// </summary>
    public string GenerateSituationNarrative(ScenePromptContext context, NarrativeHints narrativeHints)
    {
        // Validate required context
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        // Generate contextual narrative from entity properties
        return GenerateFallbackSituationNarrative(context, narrativeHints);
    }

    /// <summary>
    /// Try AI narrative generation with retry and exponential backoff.
    /// Returns tuple of (result, failureReason) for metadata tracking.
    /// </summary>
    private async Task<(string result, string failureReason)> TryGenerateAINarrativeWithRetryAsync(
        ScenePromptContext context,
        NarrativeHints narrativeHints,
        Situation situation)
    {
        string prompt = _promptBuilder.BuildSituationPrompt(context, narrativeHints, situation);
        string lastError = "";

        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                using CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(SituationTimeoutSeconds));

                StringBuilder responseBuilder = new StringBuilder();
                await foreach (string token in _aiProvider.StreamCompletionAsync(prompt, cts.Token))
                {
                    responseBuilder.Append(token);
                }

                string result = responseBuilder.ToString().Trim();
                result = CleanAIResponse(result);

                if (!string.IsNullOrEmpty(result))
                {
                    return (result, "");
                }

                lastError = "Empty response from AI";
            }
            catch (OperationCanceledException)
            {
                lastError = $"Timeout ({SituationTimeoutSeconds}s) on attempt {attempt}";
                Console.WriteLine($"[SceneNarrativeService] {lastError}");
            }
            catch (HttpRequestException ex)
            {
                lastError = $"HTTP error on attempt {attempt}: {ex.Message}";
                Console.WriteLine($"[SceneNarrativeService] {lastError}");
            }
            catch (Exception ex)
            {
                lastError = $"Error on attempt {attempt}: {ex.GetType().Name}: {ex.Message}";
                Console.WriteLine($"[SceneNarrativeService] {lastError}");
            }

            // Exponential backoff before retry
            if (attempt < MaxRetries)
            {
                int delayMs = InitialDelayMs * (int)Math.Pow(2, attempt - 1);
                Console.WriteLine($"[SceneNarrativeService] Retrying in {delayMs}ms...");
                await Task.Delay(delayMs);
            }
        }

        return ("", $"All {MaxRetries} attempts failed. Last error: {lastError}");
    }

    /// <summary>
    /// Try AI narrative generation with timeout (legacy single-attempt version).
    /// Kept for backwards compatibility with existing callers.
    /// </summary>
    private async Task<string> TryGenerateAINarrativeAsync(
        ScenePromptContext context,
        NarrativeHints narrativeHints,
        Situation situation)
    {
        (string result, string _) = await TryGenerateAINarrativeWithRetryAsync(context, narrativeHints, situation);
        return result;
    }

    /// <summary>
    /// Generate AI choice label from entity context and mechanical properties.
    /// Async method - tries AI generation with timeout, falls back to template-based.
    ///
    /// CRITICAL: This is called during Pass 2B AFTER Situation.Description is generated.
    /// Choice labels use situation context for narrative coherence.
    ///
    /// Pattern: 5-second timeout (shorter than situations - labels are simpler)
    /// </summary>
    /// <param name="context">Complete entity context with NPC/Location/Player objects</param>
    /// <param name="situation">The parent Situation (for narrative context)</param>
    /// <param name="choiceTemplate">The ChoiceTemplate being instantiated</param>
    /// <param name="scaledRequirement">Pre-scaled requirement</param>
    /// <param name="scaledConsequence">Pre-scaled consequence</param>
    /// <returns>Generated action label for Choice.Label</returns>
    public async Task<string> GenerateChoiceLabelAsync(
        ScenePromptContext context,
        Situation situation,
        ChoiceTemplate choiceTemplate,
        CompoundRequirement scaledRequirement,
        Consequence scaledConsequence)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));
        if (situation == null)
            throw new ArgumentNullException(nameof(situation));
        if (choiceTemplate == null)
            throw new ArgumentNullException(nameof(choiceTemplate));

        if (_aiProvider != null)
        {
            string aiLabel = await TryGenerateAIChoiceLabelAsync(
                context, situation, choiceTemplate, scaledRequirement, scaledConsequence);
            if (!string.IsNullOrEmpty(aiLabel))
            {
                Console.WriteLine($"[SceneNarrativeService] AI generated label for choice '{choiceTemplate.Id}'");
                return aiLabel;
            }
        }

        Console.WriteLine($"[SceneNarrativeService] Using fallback label for choice '{choiceTemplate.Id}'");
        return GenerateFallbackChoiceLabel(context, choiceTemplate);
    }

    /// <summary>
    /// Try AI choice label generation with retry and exponential backoff.
    /// </summary>
    private async Task<string> TryGenerateAIChoiceLabelAsync(
        ScenePromptContext context,
        Situation situation,
        ChoiceTemplate choiceTemplate,
        CompoundRequirement scaledRequirement,
        Consequence scaledConsequence)
    {
        string prompt = _promptBuilder.BuildChoiceLabelPrompt(
            context, situation, choiceTemplate, scaledRequirement, scaledConsequence);

        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                using CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(ChoiceLabelTimeoutSeconds));

                StringBuilder responseBuilder = new StringBuilder();
                await foreach (string token in _aiProvider.StreamCompletionAsync(prompt, cts.Token))
                {
                    responseBuilder.Append(token);
                }

                string result = responseBuilder.ToString().Trim();
                result = CleanAIResponse(result);

                if (!string.IsNullOrEmpty(result))
                {
                    return result;
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"[SceneNarrativeService] Choice label timeout ({ChoiceLabelTimeoutSeconds}s) on attempt {attempt}");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[SceneNarrativeService] Choice label HTTP error on attempt {attempt}: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SceneNarrativeService] Choice label error on attempt {attempt}: {ex.GetType().Name}: {ex.Message}");
            }

            // Exponential backoff before retry
            if (attempt < MaxRetries)
            {
                int delayMs = InitialDelayMs * (int)Math.Pow(2, attempt - 1);
                await Task.Delay(delayMs);
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// Generate fallback choice label from template with placeholder substitution.
    /// Uses ActionTextTemplate with {NPCName}, {LocationName} replaced.
    /// </summary>
    private string GenerateFallbackChoiceLabel(ScenePromptContext context, ChoiceTemplate choiceTemplate)
    {
        string label = choiceTemplate.ActionTextTemplate ?? "Take action";

        if (context.NPC != null)
            label = label.Replace("{NPCName}", context.NPC.Name);
        else
            label = label.Replace("{NPCName}", "them");

        if (context.Location != null)
            label = label.Replace("{LocationName}", context.Location.Name);
        else
            label = label.Replace("{LocationName}", "here");

        return label;
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

        // Remove model-specific artifacts (Gemma3 end tokens, etc.)
        response = response.Replace("</end_of_turn>", "").Trim();
        response = response.Replace("<end_of_turn>", "").Trim();

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
