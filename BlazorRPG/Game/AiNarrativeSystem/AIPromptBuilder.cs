using System.Text;

public partial class AIPromptBuilder
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
        string content = CreatePromptJson(
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
        );

        GameWorld gameWorld = encounterContext.gameWorld;

        StringBuilder prompt = new StringBuilder(content);

        // Add core game state context
        AddGameStateContext(prompt, gameWorld);

        // Add time context
        AddGoalContext(prompt, gameWorld);

        // Add time context
        AddTimeContext(prompt, gameWorld);

        // Add resource context
        AddResourceContext(prompt, gameWorld.Player);

        // Add travel context
        AddTravelContext(prompt, gameWorld);

        // Add instructions with context-awareness
        AddEnhancedInstructions(prompt, gameWorld);

        AIPrompt aiPrompt = new AIPrompt()
        {
            Content = prompt.ToString()
        };

        return aiPrompt;
    }

    private void AddGameStateContext(StringBuilder prompt, GameWorld gameWorld)
    {
        prompt.AppendLine("ENCOUNTER CONTEXT:");

        if (gameWorld.CurrentEncounter != null)
        {
            // Add focus points
            EncounterState state = gameWorld.CurrentEncounter.state;
            prompt.AppendLine($"- Focus Points: {state.FocusPoints}/{state.MaxFocusPoints}");

            // Add active flags
            prompt.AppendLine("- Active State Flags:");
            List<FlagStates> activeFlags = state.FlagManager.GetAllActiveFlags();
            foreach (FlagStates flag in activeFlags)
            {
                prompt.AppendLine($"  * {flag}");
            }

            // Add NPC information if available
            EncounterContext encounterContext = gameWorld.CurrentEncounter.GetEncounterContext();
            if (encounterContext.CurrentNPC != null)
            {
                prompt.AppendLine($"- Current NPC: {encounterContext.CurrentNPC.Name}");
                prompt.AppendLine($"  * Role: {encounterContext.CurrentNPC.Role}");
                prompt.AppendLine($"  * Attitude: {encounterContext.CurrentNPC.Attitude}");
            }

            // Add duration information
            prompt.AppendLine($"- Encounter Duration: {state.DurationCounter}/{gameWorld.CurrentEncounter.state.MaxDuration}");
        }

        // Add player skills
        prompt.AppendLine("- Player Skills Available:");
        foreach (SkillCard card in gameWorld.Player.AvailableCards)
        {
            if (!card.IsExhausted)
            {
                prompt.AppendLine($"  * {card.Name} (Level {card.Level}, {card.Category})");
            }
        }

        prompt.AppendLine();
    }


    private void AddGoalContext(StringBuilder prompt, GameWorld gameWorld)
    {
        prompt.AppendLine("GOAL CONTEXT:");

        // Core goal
        List<Goal> coreGoals = gameWorld.GetGoalsByType(GoalType.Core);
        if (coreGoals.Any())
        {
            Goal coreGoal = coreGoals.First(); // Typically only one core goal
            prompt.AppendLine($"- Main Goal: {coreGoal.Name}");
            prompt.AppendLine($"  * {coreGoal.Description}");
            prompt.AppendLine($"  * Progress: {coreGoal.Progress * 100:0}%");

            if (coreGoal.Deadline != -1)
            {
                int daysRemaining = coreGoal.Deadline - GameWorld.CurrentDay;
                prompt.AppendLine($"  * Deadline: {daysRemaining} days remaining");
            }
        }

        // Supporting goals
        List<Goal> supportingGoals = gameWorld.GetGoalsByType(GoalType.Supporting);
        if (supportingGoals.Any())
        {
            prompt.AppendLine("- Supporting Goals:");
            foreach (Goal goal in supportingGoals)
            {
                prompt.AppendLine($"  * {goal.Name}: {goal.Progress * 100:0}% complete");
            }
        }

        // Opportunity goals (limit to 3 most recent)
        List<Goal> opportunityGoals = gameWorld.GetGoalsByType(GoalType.Opportunity)
            .OrderByDescending(g => g.CreationDay)
            .Take(3)
            .ToList();

        if (opportunityGoals.Any())
        {
            prompt.AppendLine("- Recent Opportunities:");
            foreach (Goal goal in opportunityGoals)
            {
                prompt.AppendLine($"  * {goal.Name}: {goal.Progress * 100:0}% complete");
            }
        }

        prompt.AppendLine();
    }

    private void AddTimeContext(StringBuilder prompt, GameWorld gameWorld)
    {
        prompt.AppendLine("TIME CONTEXT:");
        prompt.AppendLine($"- Current Day: {GameWorld.CurrentDay}");
        prompt.AppendLine($"- Time of Day: {gameWorld.CurrentTimeOfDay}");

        if (gameWorld.DeadlineDay > 0)
        {
            int daysRemaining = gameWorld.DeadlineDay - GameWorld.CurrentDay;
            prompt.AppendLine($"- Deadline: {daysRemaining} days remaining");
            prompt.AppendLine($"- Deadline Reason: {gameWorld.DeadlineReason}");
        }

        prompt.AppendLine();
    }

    private void AddResourceContext(StringBuilder prompt, Player player)
    {
        prompt.AppendLine("RESOURCE CONTEXT:");
        prompt.AppendLine($"- Energy: {player.Energy}/{player.MaxEnergy}");
        prompt.AppendLine($"- Money: {player.Money} coins");
        prompt.AppendLine($"- Reputation: {player.Reputation} ({player.GetReputationLevel()})");

        prompt.AppendLine();
    }

    private void AddTravelContext(StringBuilder prompt, GameWorld gameWorld)
    {
        prompt.AppendLine("TRAVEL CONTEXT:");
        prompt.AppendLine($"- Current Location: {gameWorld.CurrentLocation.Name}");

        List<TravelRoute> availableRoutes = gameWorld.GetRoutesFromCurrentLocation();

        if (availableRoutes.Any())
        {
            prompt.AppendLine("- Available Routes:");

            foreach (TravelRoute route in availableRoutes)
            {
                prompt.AppendLine($"  * To {route.Destination.Name}:");
                prompt.AppendLine($"    - Time: {route.GetActualTimeCost()} hours");
                prompt.AppendLine($"    - Energy: {route.GetActualEnergyCost()} points");
                prompt.AppendLine($"    - Danger: {route.DangerLevel}/10");
                prompt.AppendLine($"    - Knowledge: Level {route.KnowledgeLevel}/3");

                if (route.RequiredEquipment.Any())
                {
                    bool canTravel = route.CanTravel(gameWorld.Player);
                    string status = canTravel ? "Requirements met" : "Missing requirements";
                    prompt.AppendLine($"    - Requirements: {string.Join(", ", route.RequiredEquipment)} ({status})");
                }
            }
        }
        else
        {
            prompt.AppendLine("- No known routes from this location");
        }

        prompt.AppendLine();
    }

    private void AddEnhancedInstructions(StringBuilder prompt, GameWorld gameWorld)
    {
        prompt.AppendLine("INSTRUCTIONS:");
        prompt.AppendLine("1. Generate a narrative beat description (2-3 sentences) appropriate to the current time of day and cultural context.");
        prompt.AppendLine("2. Create 3-4 distinct choices that advance toward the player's goals and are culturally appropriate.");
        prompt.AppendLine("3. For each choice:");
        prompt.AppendLine("   - Set Focus cost (0-2)");
        prompt.AppendLine("   - Define which skill cards can be used");
        prompt.AppendLine("   - Set appropriate difficulty (Easy=2, Standard=3, Hard=4, Exceptional=5)");
        prompt.AppendLine("   - Include any special bonuses from preparations");
        prompt.AppendLine("   - Select appropriate template from the provided options");
        prompt.AppendLine("   - Ensure narrative descriptions match cultural context");

        // Add contextual guidance
        if (gameWorld.DeadlineDay > 0)
        {
            int daysRemaining = gameWorld.DeadlineDay - gameWorld.CurrentDay;

            if (daysRemaining <= 3)
            {
                prompt.AppendLine("4. IMPORTANT: Create a sense of urgency due to the approaching deadline.");
            }
        }

        // Add memory guidance
        if (gameWorld.Player.Memories.Any())
        {
            prompt.AppendLine("6. Reference the player's past experiences where relevant.");
        }

        prompt.AppendLine();
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

        string content = CreatePromptJson(
            template
            .Replace("{ENCOUNTER_TYPE}", context.SkillCategory.ToString())
            .Replace("{LOCATION_NAME}", context.LocationName)
            .Replace("{ENVIRONMENT_DETAILS}", environmentDetails)
            .Replace("{PLAYER_STATUS}", playerStatus)
            .Replace("{SELECTED_CHOICE}", choiceDescription)
            );
        AIPrompt prompt = new AIPrompt()
        {
            Content = content
        };

        return prompt;
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
        string content = CreatePromptJson(
            template
            .Replace("{ENCOUNTER_TYPE}", encounterType)
            .Replace("{CURRENT_STAGE}", encounterState.DurationCounter.ToString())
            .Replace("{ENCOUNTER_TIER}", encounterTier)
            .Replace("{CURRENT_PROGRESS}", encounterState.CurrentProgress.ToString())
            .Replace("{SUCCESS_THRESHOLD}", successThreshold.ToString())
            .Replace("{PLAYER_STATUS}", playerStatus)
            .Replace("{CURRENT_NARRATIVE}", encounterState.CurrentNarrative));
        AIPrompt prompt = new AIPrompt()
        {
            Content = content
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

        string lastNarrative = "No previous narrative available.";

        string encounterGoal = context.LocationAction.ObjectiveDescription;

        string goalAchievementStatus = outcome != BeatOutcomes.Failure
            ? $"You have successfully achieved your goal to {encounterGoal}"
            : $"You have failed to {encounterGoal}";

        string choiceDescription = finalChoice.NarrativeText;
        string content = CreatePromptJson(
            template
            .Replace("{SELECTED_CHOICE}", choiceDescription)
            .Replace("{ENCOUNTER_TYPE}", context.SkillCategory.ToString())
            .Replace("{ENCOUNTER_OUTCOME}", outcome.ToString())
            .Replace("{LOCATION_NAME}", context.LocationName)
            .Replace("{LOCATION_SPOT}", context.LocationSpotName)
            .Replace("{CHARACTER_GOAL}", encounterGoal)
            .Replace("{LAST_NARRATIVE}", lastNarrative)
            .Replace("{GOAL_ACHIEVEMENT_STATUS}", goalAchievementStatus));
        AIPrompt prompt = new AIPrompt()
        {
            Content = content
        };

        return prompt;
    }
    public AIPrompt BuildPostEncounterEvolutionPrompt(PostEncounterEvolutionInput input)
    {
        string template = promptTemplates[WORLD_EVOLUTION_MD];

        string content = CreatePromptJson(
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
            .Replace("{locationDepth}", input.CurrentDepth.ToString()));
        AIPrompt prompt = new AIPrompt()
        {
            Content = content
        };
        return prompt;
    }

    public AIPrompt BuildLocationCreationPrompt(LocationCreationInput input)
    {
        string template = promptTemplates[LOCATION_GENERATION_MD];

        string content = CreatePromptJson(
            template
            .Replace("{characterArchetype}", input.CharacterArchetype)
            .Replace("{locationName}", input.TravelDestination)
            .Replace("{allKnownLocations}", input.KnownLocations)
            .Replace("{originLocationName}", input.TravelOrigin)
            .Replace("{knownCharacters}", input.KnownCharacters)
            .Replace("{activeOpportunities}", input.ActiveOpportunities));
        AIPrompt prompt = new AIPrompt()
        {
            Content = content
        };

        return prompt;
    }

    public AIPrompt BuildActionGenerationPrompt(ActionGenerationContext context)
    {
        string template = promptTemplates[ACTION_GENERATION_MD];

        string content = CreatePromptJson(
            template
            .Replace("{ACTIONNAME}", context.ActionId)
            .Replace("{SPOT_NAME}", context.SpotName)
            .Replace("{LOCATION_NAME}", context.LocationName));
        AIPrompt prompt = new AIPrompt()
        {
            Content = content
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
    private string GetTierName(int durationCounter)
    {
        if (durationCounter <= 2) return "Foundation";
        if (durationCounter <= 4) return "Development";
        return "Execution";
    }
}
