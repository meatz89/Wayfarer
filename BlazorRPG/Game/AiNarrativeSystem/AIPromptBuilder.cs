using System.Text;

public class AIPromptBuilder
{
    private Dictionary<string, string> promptTemplates;

    private const string SYSTEM_MD1 = "system";
    private const string INTRO_MD = "introduction";
    private const string REACTION_MD = "reaction";
    private const string CHOICES_MD = "choices";
    private const string ENDING_MD = "ending";
    private const string LOCATION_GENERATION_MD = "location-creation";
    private const string ACTION_GENERATION_MD = "action-generation";

    private const string WORLD_EVOLUTION_MD = "post-encounter-evolution";
    private const string MEMORY_CONSOLIDATION_MD = "memory-consolidation";

    public AIPromptBuilder(IConfiguration configuration)
    {
        // Load prompts from JSON files
        string promptsPath = configuration.GetValue<string>("NarrativePromptsPath") ?? "Data/Prompts";

        // Load all prompt templates
        promptTemplates = new Dictionary<string, string>();
        LoadPromptTemplates(promptsPath);
    }

    public AIPrompt BuildInitialPrompt(
        EncounterContext encounterContext,
        EncounterState encounterState,
        string memoryContent)
    {
        string template = promptTemplates[INTRO_MD];

        // Format environment and NPC details
        string environmentDetails = $"A {encounterContext.LocationSpotName.ToLower()} in a {encounterContext.LocationName.ToLower()}";
        string npcList = "";
        if (string.IsNullOrWhiteSpace(npcList))
        {
            npcList = "None";
        }

        // Format player character info
        string characterArchetype = encounterContext.Player.Archetype.ToString();
        string playerStatus = $"Archetype: {characterArchetype}";

        // Get action and approach information
        LocationAction locationAction = encounterContext.LocationAction;
        string actionGoal = locationAction.ObjectiveDescription;

        // Get chosen approach - CRITICAL ADDITION
        string approachName = encounterContext.ActionApproach?.Name ?? "General approach";
        string approachDescription = encounterContext.ActionApproach?.Description ?? "Using available skills";
        string approachDetails = $"{approachName}: {approachDescription}";

        // Replace placeholders in template
        AIPrompt prompt = new AIPrompt()
        {
            Content = CreatePromptJson(
            template
            .Replace("{ENCOUNTER_TYPE}", encounterContext.SkillCategory.ToString())
            .Replace("{LOCATION_NAME}", encounterContext.LocationName)
            .Replace("{LOCATION_SPOT}", encounterContext.LocationSpotName)
            .Replace("{CHARACTER_ARCHETYPE}", characterArchetype)
            .Replace("{CHARACTER_GOAL}", actionGoal)
            .Replace("{ENVIRONMENT_DETAILS}", environmentDetails)
            .Replace("{PLAYER_STATUS}", playerStatus)
            .Replace("{NPC_LIST}", npcList)
            .Replace("{MEMORY_CONTENT}", memoryContent ?? "")
            .Replace("{CHOSEN_APPROACH}", approachDetails)
        )
        };

        return prompt;
    }

    public AIPrompt BuildReactionPrompt(
        EncounterContext context,
        EncounterState encounterState,
        EncounterChoice chosenOption,
        BeatOutcome outcome)
    {
        string template = promptTemplates[REACTION_MD];

        // Format strategic effects
        StringBuilder strategicEffects = new StringBuilder();

        string environmentDetails = $"A {context.LocationSpotName.ToLower()} in a {context.LocationName.ToLower()}";

        // Format player character info
        string characterArchetype = context.Player.Archetype.ToString();
        string playerStatus = $"Archetype: {characterArchetype}";

        string choiceDescription = chosenOption.NarrativeText;

        AIPrompt prompt = new AIPrompt()
        {
            Content = CreatePromptJson(
            template
            .Replace("{ENCOUNTER_TYPE}", context.SkillCategory.ToString())
            .Replace("{LOCATION_NAME}", context.LocationName)
            .Replace("{ENVIRONMENT_DETAILS}", environmentDetails)
            .Replace("{PLAYER_STATUS}", playerStatus)
            .Replace("{SELECTED_CHOICE}", choiceDescription)
            )
        };

        return prompt;
    }


    private string GetCharactersAtLocation(string locationName,
        List<NPC> characters,
        RelationshipList relationshipList)
    {
        // Filter characters by current location
        List<NPC> locationCharacters = characters
            .Where(c =>
            {
                return c.Location.Equals(locationName, StringComparison.OrdinalIgnoreCase);
            })
            .ToList();

        if (locationCharacters.Count == 0)
            return "No known characters are present at this location, but you may include 1-2 named characters if appropriate";

        StringBuilder characterInfo = new StringBuilder();
        characterInfo.AppendLine("Characters known to be at this location:");

        foreach (NPC character in locationCharacters)
        {
            // Get relationship level and description
            int relationshipLevel = 0;
            string relationshipDescription = "Stranger";

            relationshipLevel = relationshipList.GetLevel(character.Name);
            relationshipDescription = GetRelationshipDescription(relationshipLevel);

            characterInfo.AppendLine($"- {character.Name}: {character.Role}. {character.Description}");
            characterInfo.AppendLine($"  Relationship: {relationshipDescription} (Level {relationshipLevel})");
        }

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

    private string GetSkillCategoryDescription(ActionTypes SkillCategory)
    {
        return SkillCategory switch
        {
            ActionTypes.Physical => "Physical",
            ActionTypes.Social => "Social",
            ActionTypes.Intellectual => "Intellectual",
            _ => "Unknown"
        };
    }

    private string BuildPlayerStatusSection(EncounterState encounterState, Player playerState)
    {
        StringBuilder status = new StringBuilder();

        status.AppendLine($"Focus Points: {encounterState.FocusPoints}/6");

        return status.ToString().TrimEnd();
    }

    public AIPrompt BuildChoicesPrompt(
        EncounterContext context,
        EncounterState encounterState,
        WorldStateInput worldStateInput)
    {
        string template = promptTemplates[CHOICES_MD];

        // Get player status
        string playerStatus = $"- Focus Points: {encounterState.FocusPoints}/{encounterState.MaxFocusPoints}\n";

        // Get encounter type and tier
        string encounterType = context.SkillCategory.ToString();
        string encounterTier = GetTierName(encounterState.DurationCounter);
        int successThreshold = 10; // Basic success threshold

        // Replace placeholders in template
        AIPrompt prompt = new AIPrompt()
        {
            Content = CreatePromptJson(
            template
            .Replace("{ENCOUNTER_TYPE}", encounterType)
            .Replace("{CURRENT_STAGE}", encounterState.DurationCounter.ToString())
            .Replace("{ENCOUNTER_TIER}", encounterTier)
            .Replace("{CURRENT_PROGRESS}", encounterState.CurrentProgress.ToString())
            .Replace("{SUCCESS_THRESHOLD}", successThreshold.ToString())
            .Replace("{PLAYER_STATUS}", playerStatus)
            .Replace("{CURRENT_NARRATIVE}", encounterState.CurrentNarrative))
        };

        return prompt;
    }

    private static string BuildChoicesInfo(EncounterContext context, List<EncounterChoice> choices, List<ChoiceProjection> projections)
    {
        string choicesInfo = string.Empty;
        for (int i = 0; i < choices.Count; i++)
        {
            EncounterChoice choice = choices[i];
            ChoiceProjection projection = projections[i];
            string choiceText = $"\nCHOICE {i + 1}: {choice.ChoiceID}\n";
            choiceText += $"Description: {choice.NarrativeText}\n";

            // Focus cost information
            choiceText += $"Focus Cost: {projection.FocusCost}";
            choiceText += "\n";

            // Progress gained
            if (projection.ProgressGained > 0)
                choiceText += $"Progress Gained: {projection.ProgressGained}\n";

            // Focus points gained (if any)
            if (projection.FocusPointsGained > 0)
                choiceText += $"Focus Points Gained: {projection.FocusPointsGained}\n";

            // Skill check information
            if (projection.HasSkillCheck)
            {
                choiceText += $"Skill Check: {projection.SkillUsed} ";
                choiceText += $"Difficulty {projection.SkillCheckDifficulty}\n";
            }

            // Encounter ending information
            if (projection.WillEncounterEnd)
            {
                choiceText += "This choice will end the encounter\n";
                choiceText += $"Projected Final Outcome: {projection.ProjectedOutcome}\n";
                choiceText += $"Goal Achievement: " +
                    $"{(projection.ProjectedOutcome != BeatOutcomes.Failure ?
                    "Will achieve goal to" : "Will fail to")} {context.LocationAction.ObjectiveDescription}\n";
            }

            choicesInfo += choiceText;
        }

        return choicesInfo;
    }

    public AIPrompt BuildEncounterEndPrompt(
        EncounterContext context,
        BeatOutcomes outcome,
        EncounterChoice finalChoice
        )
    {
        string template = promptTemplates[ENDING_MD];

        // Get the last narrative event
        string lastNarrative = "No previous narrative available.";

        // Extract encounter goal from inciting action
        string encounterGoal = context.LocationAction.ObjectiveDescription;

        // Add goal achievement status
        string goalAchievementStatus = outcome != BeatOutcomes.Failure
            ? $"You have successfully achieved your goal to {encounterGoal}"
            : $"You have failed to {encounterGoal}";

        string choiceDescription = finalChoice.NarrativeText;
        AIPrompt prompt = new AIPrompt()
        {
            Content = CreatePromptJson(
            template
            .Replace("{SELECTED_CHOICE}", choiceDescription)
            .Replace("{ENCOUNTER_TYPE}", context.SkillCategory.ToString())
            .Replace("{ENCOUNTER_OUTCOME}", outcome.ToString())
            .Replace("{LOCATION_NAME}", context.LocationName)
            .Replace("{LOCATION_SPOT}", context.LocationSpotName)
            .Replace("{CHARACTER_GOAL}", encounterGoal)
            .Replace("{LAST_NARRATIVE}", lastNarrative)
            .Replace("{GOAL_ACHIEVEMENT_STATUS}", goalAchievementStatus))
        };

        return prompt;
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
            promptTemplates[key] = jsonContent;
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

    public AIPrompt BuildPostEncounterEvolutionPrompt(PostEncounterEvolutionInput input)
    {
        string template = promptTemplates[WORLD_EVOLUTION_MD];

        AIPrompt prompt = new AIPrompt()
        {
            Content = CreatePromptJson(
            template
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
            .Replace("{locationDepth}", input.CurrentDepth.ToString()))
        };
        return prompt;
    }

    public AIPrompt BuildLocationCreationPrompt(LocationCreationInput input)
    {
        string template = promptTemplates[LOCATION_GENERATION_MD];

        AIPrompt prompt = new AIPrompt()
        {
            Content = CreatePromptJson(
            template
            .Replace("{characterArchetype}", input.CharacterArchetype)
            .Replace("{locationName}", input.TravelDestination)
            .Replace("{allKnownLocations}", input.KnownLocations)
            .Replace("{originLocationName}", input.TravelOrigin)
            .Replace("{knownCharacters}", input.KnownCharacters)
            .Replace("{activeOpportunities}", input.ActiveOpportunities))
        };

        return prompt;
    }

    public AIPrompt BuildMemoryPrompt(MemoryConsolidationInput input)
    {
        string template = promptTemplates[MEMORY_CONSOLIDATION_MD];

        AIPrompt prompt = new AIPrompt()
        {
            Content = CreatePromptJson(
            template
            .Replace("{FILE_CONTENT}", input.OldMemory)
            )
        };

        return prompt;
    }

    public string GetSystemMessage(WorldStateInput input)
    {
        string staticSystemPrompt = promptTemplates[SYSTEM_MD1];

        string dynamicSystemPrompt = staticSystemPrompt
            .Replace("{CHARACTER_ARCHETYPE}", input.PlayerArchetype)
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

    public AIPrompt BuildActionGenerationPrompt(ActionGenerationContext context)
    {
        string template = promptTemplates[ACTION_GENERATION_MD];

        AIPrompt prompt = new AIPrompt()
        {
            Content = CreatePromptJson(
            template
            .Replace("{ACTIONNAME}", context.ActionId)
            .Replace("{SPOT_NAME}", context.SpotName)
            .Replace("{LOCATION_NAME}", context.LocationName))
        };

        return prompt;
    }

    private string GetTierName(int durationCounter)
    {
        if (durationCounter <= 2) return "Foundation";
        if (durationCounter <= 4) return "Development";
        return "Execution";
    }
}
