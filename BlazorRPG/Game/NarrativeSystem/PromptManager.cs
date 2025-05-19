using System.Text;

public class PromptManager
{
    private readonly Dictionary<string, string> _promptTemplates;

    private const string SYSTEM_MD1 = "system";
    private const string INTRO_MD = "introduction";
    private const string REACTION_MD = "reaction";
    private const string CHOICES_MD = "choices";
    private const string ENDING_MD = "ending";
    private const string LOCATION_GENERATION_MD = "location-creation";
    private const string ACTION_GENERATION_MD = "action-generation";

    private const string WORLD_EVOLUTION_MD = "post-encounter-evolution";
    private const string MEMORY_CONSOLIDATION_MD = "memory-consolidation";

    public PromptManager(IConfiguration configuration)
    {
        // Load prompts from JSON files
        string promptsPath = configuration.GetValue<string>("NarrativePromptsPath") ?? "Data/Prompts";

        // Load all prompt templates
        _promptTemplates = new Dictionary<string, string>();
        LoadPromptTemplates(promptsPath);
    }

    public string BuildActionGenerationPrompt(ActionGenerationContext context)
    {
        string template = _promptTemplates[ACTION_GENERATION_MD];

        string prompt = template
            .Replace("{ACTIONNAME}", context.ActionId)
            .Replace("{SPOT_NAME}", context.SpotName)
            .Replace("{LOCATION_NAME}", context.LocationName);

        return CreatePromptJson(prompt);
    }

    public string BuildIntroductionPrompt(
        NarrativeContext context,
        List<Character> characters,
        RelationshipList relationshipList,
        string memoryContent)
    {
        string template = _promptTemplates[INTRO_MD];

        // Format environment and NPC details
        string environmentDetails = $"A {context.LocationSpotName.ToLower()} in a {context.LocationName.ToLower()}";

        string npcList = GetCharactersAtLocation(context.LocationName, characters, relationshipList);

        // Format player character info
        string characterArchetype = context.PlayerState.Archetype.ToString();

        ActionImplementation actionImplementation = context.ActionImplementation;
        string encounterGoal = actionImplementation.Description;

        // Replace placeholders in template
        string prompt = template
            .Replace("{ENCOUNTER_TYPE}", context.EncounterType.ToString())
            .Replace("{LOCATION_NAME}", context.LocationName)
            .Replace("{LOCATION_SPOT}", context.LocationSpotName)
            .Replace("{CHARACTER_ARCHETYPE}", characterArchetype)
            .Replace("{CHARACTER_GOAL}", encounterGoal)
            .Replace("{ENVIRONMENT_DETAILS}", environmentDetails)
            .Replace("{NPC_LIST}", npcList);

        return CreatePromptJson(prompt);
    }

    private string GetCharactersAtLocation(string locationName, 
        List<Character> characters, 
        RelationshipList relationshipList)
    {
        // Filter characters by current location
        List<Character> locationCharacters = characters
            .Where(c =>
            {
                return c.Location.Equals(locationName, StringComparison.OrdinalIgnoreCase);
            })
            .ToList();

        if (locationCharacters.Count == 0)
            return "No known characters are present at this location, but you may include 1-2 named characters if appropriate";

        StringBuilder characterInfo = new StringBuilder();
        characterInfo.AppendLine("Characters known to be at this location:");

        foreach (Character character in locationCharacters)
        {
            // Get relationship level and description
            int relationshipLevel = 0;
            string relationshipDescription = "Stranger";

            relationshipLevel = relationshipList.GetLevel(character.Name);
            relationshipDescription = GetRelationshipDescription(relationshipLevel);

            characterInfo.AppendLine($"- {character.Name}: {character.Role}. {character.Description}");
            characterInfo.AppendLine($"  Relationship: {relationshipDescription} (Level {relationshipLevel})");

            // Add personality traits if available
            if (!string.IsNullOrEmpty(character.Personality))
                characterInfo.AppendLine($"  Personality: {character.Personality}");

            // Add appearance if available
            if (!string.IsNullOrEmpty(character.Appearance))
                characterInfo.AppendLine($"  Appearance: {character.Appearance}");

            // Add recent interaction history if available
            if (character.InteractionHistory != null && character.InteractionHistory.Count > 0)
            {
                string? lastInteraction = character!.InteractionHistory!.LastOrDefault();
                if (!string.IsNullOrEmpty(lastInteraction))
                    characterInfo.AppendLine($"  Last interaction: {lastInteraction}");
            }
        }

        characterInfo.AppendLine("Include these characters in the narrative with appropriate reactions based on relationship level.");
        characterInfo.AppendLine("Characters with higher relationship levels should be more helpful and friendly.");
        characterInfo.AppendLine("Characters with negative relationship levels should be wary, suspicious, or hostile.");

        return characterInfo.ToString();
    }

    private string GetRelationshipDescription(int relationshipLevel)
    {
        string desc = relationshipLevel switch
        {
            < 0 => "Hostile",
            0 => "Stranger",
            1 => "Acquaintance",
            2 => "Familiar face",
            3 => "Friendly",
            4 => "Trusted",
            5 => "Ally",
            6 => "Close ally",
            7 => "Loyal friend",
            8 => "Confidant",
            9 => "Devoted supporter",
            >= 10 => "Unwavering ally",
        };

        if (desc != string.Empty)
            return desc;
        else
            return "Unkown";
    }

    public string BuildReactionPrompt(
        NarrativeContext context,
        EncounterOption chosenOption,
        ChoiceNarrative choiceDescription,
        ChoiceOutcome outcome)
    {
        string template = _promptTemplates[REACTION_MD];

        // Format strategic effects
        StringBuilder strategicEffects = new StringBuilder();
        if (outcome.HealthChange != 0)
        {
            strategicEffects.AppendLine($"- Health Change: {outcome.HealthChange}");
        }

        if (outcome.ConcentrationChange != 0)
        {
            strategicEffects.AppendLine($"- Concentration Change: {outcome.ConcentrationChange}");
        }

        // Replace placeholders in template
        string prompt = template
            .Replace("{SELECTED_CHOICE}", choiceDescription.ShorthandName)
            .Replace("{CHOICE_DESCRIPTION}", choiceDescription.FullDescription)
            .Replace("{STRATEGIC_EFFECTS}", strategicEffects.ToString());

        return CreatePromptJson(prompt);
    }

    public string BuildChoicesPrompt(
        NarrativeContext context,
        List<EncounterOption> choices,
        List<ChoiceProjection> projections)
    {
        string template = _promptTemplates[CHOICES_MD];

        // Get the most recent narrative event for current situation
        string currentSituation = "No previous narrative available.";
        if (context.Events.Count > 0)
        {
            NarrativeEvent lastEvent = context.Events[context.Events.Count - 1];
            currentSituation = lastEvent.Summary;
        }

        // Build choices info without StringBuilder - do it conditionally
        string choicesInfo = "";
        for (int i = 0; i < choices.Count; i++)
        {
            EncounterOption choice = choices[i];
            ChoiceProjection projection = projections[i];

            string choiceText = $"\nCHOICE {i + 1}:\n";

            // Only add resource changes if non-zero
            if (projection.HealthChange != 0)
                choiceText += $"\n- Health Change: {projection.HealthChange}";

            if (projection.ConcentrationChange != 0)
                choiceText += $"\n- Concentration Change: {projection.ConcentrationChange}";

            // Only add encounter ending info if applicable
            if (projection.EncounterWillEnd)
            {
                choiceText += $"\n- Encounter Will End: True";
                choiceText += $"\n- Final Outcome: {projection.ProjectedOutcome}";
                choiceText += $"\n- Goal Achievement: " +
                    $"{(projection.ProjectedOutcome != EncounterOutcomes.Failure ?
                    "Will achieve goal to" : "Will fail to")} {context.ActionImplementation.Description}";
            }

            // Add this choice's text to the overall choices info
            choicesInfo += choiceText;
        }

        // Replace placeholders in template
        string prompt = template
            .Replace("{ENCOUNTER_GOAL}", context.ActionImplementation.Description)
            .Replace("{CHOICES_INFO}", choicesInfo);

        return CreatePromptJson(prompt);
    }

    public string BuildEncounterEndPrompt(
        NarrativeContext context,
        EncounterOutcomes outcome,
        EncounterOption finalChoice,
        ChoiceNarrative choiceDescription
        )
    {
        string template = _promptTemplates[ENDING_MD];

        // Get the last narrative event
        string lastNarrative = "No previous narrative available.";
        if (context.Events.Count > 0)
        {
            NarrativeEvent lastEvent = context.Events[context.Events.Count - 1];
            lastNarrative = lastEvent.Summary;
        }

        // Extract encounter goal from inciting action
        string encounterGoal = context.ActionImplementation.Description;

        // Add goal achievement status
        string goalAchievementStatus = outcome != EncounterOutcomes.Failure
            ? $"You have successfully achieved your goal to {encounterGoal}"
            : $"You have failed to {encounterGoal}";

        // Replace placeholders in template
        string prompt = template
            .Replace("{SELECTED_CHOICE}", choiceDescription.ShorthandName)
            .Replace("{CHOICE_DESCRIPTION}", choiceDescription.FullDescription)
            .Replace("{ENCOUNTER_TYPE}", context.EncounterType.ToString())
            .Replace("{ENCOUNTER_OUTCOME}", outcome.ToString())
            .Replace("{LOCATION_NAME}", context.LocationName)
            .Replace("{LOCATION_SPOT}", context.LocationSpotName)
            .Replace("{CHARACTER_GOAL}", encounterGoal)
            .Replace("{LAST_NARRATIVE}", lastNarrative)
            .Replace("{GOAL_ACHIEVEMENT_STATUS}", goalAchievementStatus);

        return CreatePromptJson(prompt);
    }

    private string FormatTags(List<string> newTags)
    {
        if (newTags == null || !newTags.Any())
            return "No tags";

        StringBuilder builder = new StringBuilder();
        foreach (string tag in newTags)
        {
            builder.AppendLine($"- {tag}");
        }

        return builder.ToString();
    }

    public static string CreatePromptJson(string markdownContent)
    {
        // First normalize all newlines to \n
        string normalized = markdownContent.Replace("\r\n", "\n");

        // Now build the JSON string manually with proper escaping
        StringBuilder jsonBuilder = new StringBuilder();
        jsonBuilder.Append("{\n");
        jsonBuilder.Append("\t\"prompt\": \"");

        // Process each character to ensure proper escaping
        foreach (char c in normalized)
        {
            switch (c)
            {
                case '\\':
                    jsonBuilder.Append("\\\\"); // Escape backslash
                    break;
                case '\"':
                    jsonBuilder.Append("\\\""); // Escape double quote
                    break;
                case '\n':
                    jsonBuilder.Append("\\n"); // Replace newline with \n
                    break;
                default:
                    jsonBuilder.Append(c);
                    break;
            }
        }

        jsonBuilder.Append("\"\n}");

        return jsonBuilder.ToString();
    }

    private void LoadPromptTemplates(string basePath)
    {
        // Load all JSON files in the prompts directory
        foreach (string filePath in Directory.GetFiles(basePath, "*.md"))
        {
            string key = Path.GetFileNameWithoutExtension(filePath);
            string mdContent = LoadPromptFile(filePath);
            string jsonContent = CreatePromptJson(mdContent);
            _promptTemplates[key] = jsonContent;
        }
    }

    private string LoadPromptFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Prompt file not found: {filePath}");
        }

        string mdContent = File.ReadAllText(filePath);
        return mdContent;
    }

    private string DetermineEncounterStage(int currentTurn, int maxTurns, int currentMomentum, int maxMomentum, int currentPressure, int maxPressure)
    {
        double highestProgress = 0;

        double progressMomentum = (double)currentMomentum / maxMomentum;
        double progressPressure = (double)currentPressure / maxPressure;
        double progressTurns = (double)currentTurn / maxTurns;

        highestProgress = double.Max(progressMomentum, progressPressure);
        highestProgress = double.Max(highestProgress, progressTurns);

        if (highestProgress < 0.33) return "Early";
        if (highestProgress < 0.67) return "Middle";
        return "Late";
    }

    public string BuildPostEncounterEvolutionPrompt(PostEncounterEvolutionInput input)
    {
        string template = _promptTemplates[WORLD_EVOLUTION_MD];

        return template
            .Replace("{characterBackground}", input.CharacterBackground)
            .Replace("{currentLocation}", input.CurrentLocation)
            .Replace("{encounterOutcome}", input.EncounterOutcome)
            .Replace("{health}", input.Health.ToString())
            .Replace("{maxHealth}", input.MaxHealth.ToString())
            .Replace("{energy}", input.Energy.ToString())
            .Replace("{maxEnergy}", input.MaxEnergy.ToString())
            .Replace("{allKnownLocations}", input.KnownLocations)
            .Replace("{connectedLocations}", input.ConnectedLocations)
            .Replace("{currentLocationSpots}", input.CurrentLocationSpots)
            .Replace("{allExistingActions}", input.AllExistingActions)
            .Replace("{knownCharacters}", input.KnownCharacters)
            .Replace("{activeOpportunities}", input.ActiveOpportunities)
            .Replace("{currentLocationSpots}", input.CurrentLocationSpots)
            .Replace("{connectedLocations}", input.ConnectedLocations)
            .Replace("{locationDepth}", input.CurrentDepth.ToString());
    }

    public string BuildLocationCreationPrompt(LocationCreationInput input)
    {
        string template = _promptTemplates[LOCATION_GENERATION_MD];

        // Replace placeholders in template
        string prompt = template
            .Replace("{characterArchetype}", input.CharacterArchetype)
            .Replace("{locationName}", input.TravelDestination)
            .Replace("{allKnownLocations}", input.KnownLocations)
            .Replace("{originLocationName}", input.TravelOrigin)
            .Replace("{knownCharacters}", input.KnownCharacters)
            .Replace("{activeOpportunities}", input.ActiveOpportunities)
            .Replace("{locationDepth}", input.CurrentDepth.ToString())
            .Replace("{lastHubDepth}", input.LastHubDepth.ToString());

        return CreatePromptJson(prompt);
    }

    public string BuildMemoryPrompt(MemoryConsolidationInput input)
    {
        string template = _promptTemplates[MEMORY_CONSOLIDATION_MD];

        string prompt = template
            .Replace("{FILE_CONTENT}", input.OldMemory);

        return prompt;
    }

    public string GetSystemMessage(WorldStateInput input)
    {
        string staticSystemPrompt = _promptTemplates[SYSTEM_MD1];

        string dynamicSystemPrompt = staticSystemPrompt
            .Replace("{CHARACTER_ARCHETYPE}", input.CharacterArchetype)
            .Replace("{ENERGY}", input.Energy.ToString())
            .Replace("{MAX_ENERGY}", input.MaxEnergy.ToString())
            .Replace("{COINS}", input.Coins.ToString())
            .Replace("{CURRENT_LOCATION}", input.CurrentLocation)
            .Replace("{LOCATION_DEPTH}", input.LocationDepth.ToString())
            .Replace("{CURRENT_SPOT}", input.CurrentSpot)
            .Replace("{CONNECTED_LOCATIONS}", input.ConnectedLocations)
            .Replace("{LOCATION_SPOTS}", input.LocationSpots)
            .Replace("{INVENTORY}", input.Inventory)
            .Replace("{KNOWN_CHARACTERS}", input.KnownCharacters)
            .Replace("{ACTIVE_OPPORTUNITIES}", input.ActiveOpportunities)
            .Replace("{MEMORY_SUMMARY}", input.MemorySummary);

        return dynamicSystemPrompt;
    }

    public class TagFormatter
    {
        // Simplified tag modifications format
        public string FormatTagModifications<TKey>(Dictionary<TKey, int> tagChanges) where TKey : notnull
        {
            if (tagChanges == null || tagChanges.Count == 0)
                return "None";

            // Only include significant changes (value > 1)
            List<KeyValuePair<TKey, int>> significantChanges = tagChanges.Where(c =>
            {
                return Math.Abs(c.Value) > 1;
            }).ToList();
            if (significantChanges.Count == 0)
                return "Minor changes only";

            List<string> modificationStrings = new List<string>();
            foreach (KeyValuePair<TKey, int> change in significantChanges)
            {
                string direction = change.Value > 0 ? "increased" : "decreased";
                modificationStrings.Add($"{change.Key} {direction} by {Math.Abs(change.Value)}");
            }

            return string.Join(", ", modificationStrings);
        }

        public string FormatTagValues<TKey>(Dictionary<TKey, int> tags) where TKey : notnull
        {
            List<string> tagStrings = new List<string>();
            foreach (KeyValuePair<TKey, int> tag in tags)
            {
                // Include all tags, even those with 0 value
                tagStrings.Add($"{tag.Key}: {tag.Value}");
            }
            return string.Join(", ", tagStrings);
        }

        public string FormatNarrativeTagsExtended(List<NarrativeTag> narrativeTags)
        {
            if (narrativeTags == null || narrativeTags.Count == 0)
                return "None";

            List<string> tagStrings = new List<string>();
            foreach (NarrativeTag tag in narrativeTags)
            {
                tagStrings.Add($"{tag.NarrativeName} ({tag.GetEffectDescription()}");
            }
            return string.Join(", ", tagStrings);
        }

    }
}
