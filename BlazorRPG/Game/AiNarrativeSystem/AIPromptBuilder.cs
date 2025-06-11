using System.Numerics;
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

    public AIPrompt BuildIntroductionPrompt(
        EncounterContext context,
        EncounterState state,
        string memoryContent)
    {
        string template = promptTemplates[INTRO_MD];

        Player player = context.Player;
        GameWorld gameWorld = context.GameWorld;

        StringBuilder prompt = new StringBuilder();

        // Add base context
        AddBaseContext(prompt, context);

        // Add memory context
        AddMemoryContext(prompt, gameWorld);

        // Add global goal context
        AddGlobalGoalContext(prompt, gameWorld);

        // Add core game state context
        AddEncounterContext(prompt, context, state, player);

        // Add time context
        AddTimeContext(prompt, gameWorld);

        // Add resource context
        AddResourceContext(prompt, gameWorld.GetPlayer());

        // Add travel context
        AddTravelContext(prompt, gameWorld);

        // Replace Prompt Context placeholder
        var content = template.Replace("{PROMPT_CONTEXT}", prompt.ToString());

        AIPrompt aiPrompt = new AIPrompt()
        {
            Content = content
        };

        return aiPrompt;
    }

    public AIPrompt BuildChoicesPrompt(
        EncounterContext context,
        EncounterState state,
        List<ChoiceTemplate> choiceTemplates)
    {
        string template = promptTemplates[CHOICES_MD];

        Player player = context.Player;
        GameWorld gameWorld = context.GameWorld;

        StringBuilder prompt = new StringBuilder();

        // Add core game state context
        AddEncounterContext(prompt, context, state, player);

        // Add choices information
        AddChoiceTemplatesContext(prompt, context, choiceTemplates);

        // Replace Prompt Context placeholder
        var content = template.Replace("{PROMPT_CONTEXT}", prompt.ToString());

        AIPrompt aiPrompt = new AIPrompt()
        {
            Content = content.ToString()
        };

        return aiPrompt;
    }

    public AIPrompt BuildReactionPrompt(
        EncounterContext context,
        EncounterState state,
        EncounterChoice chosenOption)
    {
        string template = promptTemplates[REACTION_MD];

        Player player = context.Player;
        GameWorld gameWorld = context.GameWorld;

        StringBuilder prompt = new StringBuilder();

        // Add core game state context
        AddEncounterContext(prompt, context, state, player);

        // Add selected choice context
        AddSelectedChoiceContext(prompt, chosenOption);

        // Replace Prompt Context placeholder
        var content = template.Replace("{PROMPT_CONTEXT}", prompt.ToString());

        AIPrompt aiPrompt = new AIPrompt()
        {
            Content = content.ToString()
        };

        return aiPrompt;
    }

    public AIPrompt BuildEncounterConclusionPrompt(
        EncounterContext context,
        EncounterState state,
        BeatOutcomes outcome,
        EncounterChoice finalChoice
        )
    {
        string template = promptTemplates[ENDING_MD];

        GameWorld gameWorld = context.GameWorld;
        Player player = context.Player;

        StringBuilder prompt = new StringBuilder();

        // Add core game state context
        AddEncounterContext(prompt, context, state, player);

        // Add goal context
        AddEncounterGoalContext(prompt, context, state, gameWorld);

        // Add time context
        AddTimeContext(prompt, gameWorld);

        // Add resource context
        AddResourceContext(prompt, gameWorld.GetPlayer());

        // Add travel context
        AddTravelContext(prompt, gameWorld);

        // Replace Prompt Context placeholder
        var content = template.Replace("{PROMPT_CONTEXT}", prompt.ToString());

        AIPrompt aiPrompt = new AIPrompt()
        {
            Content = content.ToString()
        };

        return aiPrompt;
    }
    public AIPrompt BuildPostEncounterEvolutionPrompt(PostEncounterEvolutionInput input)
    {
        string template = promptTemplates[WORLD_EVOLUTION_MD];

        string prompt = CreatePromptJson(
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

        AIPrompt aiPrompt = new AIPrompt()
        {
            Content = prompt.ToString()
        };

        return aiPrompt;
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

    private void AddEncounterContext(StringBuilder prompt, EncounterContext context, EncounterState state, Player player)
    {
        prompt.AppendLine();

        prompt.AppendLine("ENCOUNTER CONTEXT:");

        // Add focus points
        prompt.AppendLine($"- Focus Points: {state.FocusPoints}/{state.MaxFocusPoints}");

        // Add active flags
        prompt.AppendLine("- Active State Flags:");
        List<FlagStates> activeFlags = state.FlagManager.GetAllActiveFlags();
        foreach (FlagStates flag in activeFlags)
        {
            prompt.AppendLine($"  * {flag}");
        }

        // Add NPC information if available
        if (context.TargetNPC != null)
        {
            prompt.AppendLine($"- Current NPC: {context.TargetNPC.Name}");
            prompt.AppendLine($"  * Role: {context.TargetNPC.Role}");
            prompt.AppendLine($"  * Attitude: {context.TargetNPC.Attitude}");
        }

        // Get player status
        string playerStatus = $"- Focus Points: {state.FocusPoints}/{state.MaxFocusPoints}\n";

        // Add duration information
        prompt.AppendLine($"- Encounter Type: {context.SkillCategory.ToString()}");
        prompt.AppendLine($"- Encounter Duration: {state.DurationCounter}/{state.MaxDuration}");

        // Add player skills
        prompt.AppendLine("- Player Skills Available:");
        foreach (SkillCard card in player.AvailableCards)
        {
            if (!card.IsExhausted)
            {
                prompt.AppendLine($"  * {card.Name} (Level {card.Level}, {card.Category})");
            }
        }

        prompt.AppendLine();
    }

    private static void AddBaseContext(StringBuilder prompt, EncounterContext context)
    {
        prompt.AppendLine();

        string environmentDetails = $"A {context.LocationSpotName.ToLower()} in a {context.LocationName.ToLower()}";
        string npcList = "";
        if (string.IsNullOrWhiteSpace(npcList))
        {
            npcList = "None";
        }

        // Format player character info
        string gender = context.Player.Gender.ToString();
        string playerArchetype = context.Player.Archetype.ToString();
        string playerInfo = $"{gender} {playerArchetype}";

        // Get action and approach information
        LocationAction locationAction = context.LocationAction;
        string actionGoal = locationAction.ObjectiveDescription;

        // Get chosen approach
        string approachName = context.ActionApproach?.Name ?? "General approach";
        string approachDescription = context.ActionApproach?.Description ?? "Using available skills";
        string approachDetails = $"{approachName}: {approachDescription}";

        // Append lines with easy to understand labels
        prompt.AppendLine("Encounter Type: " + context.SkillCategory);
        prompt.AppendLine("Location Name: " + context.LocationName);
        prompt.AppendLine("Location Spot: " + context.LocationSpotName);
        prompt.AppendLine("Character Goal: " + actionGoal);
        prompt.AppendLine("Environment Details: " + environmentDetails);
        prompt.AppendLine("Player Info: " + playerInfo);
        prompt.AppendLine("NPCs Present: " + npcList);
        prompt.AppendLine("Chosen Approach: " + approachDetails);

        prompt.AppendLine();
    }

    private void AddMemoryContext(StringBuilder prompt, GameWorld gameWorld)
    {
        prompt.AppendLine();

        MemoryFileAccess memoryFileAccess = new MemoryFileAccess(gameWorld.GetGameInstanceId());
        List<string> memories = memoryFileAccess.GetAllMemories().Result;
        
        prompt.AppendLine("Memories:");
        if (memories.Any())
        {
            foreach (var memory in memories)
            {
                prompt.AppendLine($"- {memory}");
            }
        }
        else
        {
            prompt.AppendLine("- No memories available.");
        }
        
        prompt.AppendLine();
    }

    private void AddEncounterGoalContext(StringBuilder prompt, EncounterContext context, EncounterState state, GameWorld gameWorld)
    {
        prompt.AppendLine();

        prompt.AppendLine("ENCOUNTER GOAL CONTEXT:");

        var outcome = state.EncounterOutcome;

        string encounterGoal = context.LocationAction.ObjectiveDescription;
        prompt.AppendLine(encounterGoal);

        string goalAchievementStatus =
            outcome == BeatOutcomes.Success ? $"You have successfully achieved your goal to {encounterGoal}" :
            outcome == BeatOutcomes.Failure ? $"You have failed to {encounterGoal}" : string.Empty;

        prompt.AppendLine(goalAchievementStatus);

        prompt.AppendLine();
    }

    private void AddGlobalGoalContext(StringBuilder prompt, GameWorld gameWorld)
    {
        prompt.AppendLine();

        prompt.AppendLine("GLOBAL GOAL CONTEXT:");

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
        prompt.AppendLine();

        prompt.AppendLine("TIME CONTEXT:");
        prompt.AppendLine($"- Current Day: {GameWorld.CurrentDay}");
        prompt.AppendLine($"- Time of Day: {GameWorld.CurrentTimeOfDay}");

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
        prompt.AppendLine();

        prompt.AppendLine("RESOURCE CONTEXT:");
        prompt.AppendLine($"- Energy: {player.Energy}/{player.MaxEnergy}");
        prompt.AppendLine($"- Money: {player.Money} coins");
        prompt.AppendLine($"- Reputation: {player.Reputation} ({player.GetReputationLevel()})");

        prompt.AppendLine();
    }

    private void AddTravelContext(StringBuilder prompt, GameWorld gameWorld)
    {
        prompt.AppendLine();

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
                    bool canTravel = route.CanTravel(gameWorld.GetPlayer());
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

    private void AddSelectedChoiceContext(StringBuilder prompt, EncounterChoice choice)
    {
        prompt.AppendLine();

        prompt.AppendLine("SELECTED CHOICE CONTEXT:");
        prompt.AppendLine($"- Choice ID: {choice.ChoiceID}");
        prompt.AppendLine($"- Narrative Text: {choice.NarrativeText}");
        prompt.AppendLine($"- Focus Cost: {choice.FocusCost}");
        prompt.AppendLine($"- Is Disabled: {choice.IsDisabled}");
        prompt.AppendLine($"- Is Affordable: {choice.IsAffordable}");
        prompt.AppendLine($"- Template Used: {choice.TemplateUsed}");
        prompt.AppendLine($"- Template Purpose: {choice.TemplatePurpose}");

        // Skill Option details
        if (choice.SkillOption != null)
        {
            prompt.AppendLine($"- Skill Option: {choice.SkillOption}");
        }

        // Skill Check details
        if (choice.SkillCheck != null)
        {
            prompt.AppendLine($"- Skill Check: {choice.SkillCheck}");
        }

        // Success/Failure Narratives
        if (!string.IsNullOrWhiteSpace(choice.SuccessNarrative))
        {
            prompt.AppendLine($"- Success Narrative: {choice.SuccessNarrative}");
        }
        if (!string.IsNullOrWhiteSpace(choice.FailureNarrative))
        {
            prompt.AppendLine($"- Failure Narrative: {choice.FailureNarrative}");
        }

        prompt.AppendLine();
    }

    private void AddChoiceTemplatesContext(StringBuilder prompt, EncounterContext context, List<ChoiceTemplate> choiceTemplates)
    {
        prompt.AppendLine();

        choiceTemplates = new List<ChoiceTemplate>();
        for (int i = 0; i < choiceTemplates.Count; i++)
        {
            ChoiceTemplate template = choiceTemplates[i];
            prompt.AppendLine($"\nCHOICE TEMPLATE {i + 1}: {template.TemplateName}");
            prompt.AppendLine($"Strategic Purpose: {template.StrategicPurpose}");
            prompt.AppendLine($"Weight: {template.Weight}");

            // Input mechanics
            if (template.InputMechanics != null)
            {
                prompt.AppendLine($"Input Mechanics: {template.InputMechanics.ToJsonObject()}");
            }

            // Effects
            if (template.SuccessEffect != null)
            {
                prompt.AppendLine($"Success Effect: {template.SuccessEffect.GetDescriptionForPlayer()}");
            }
            if (template.FailureEffect != null)
            {
                prompt.AppendLine($"Failure Effect: {template.FailureEffect.GetDescriptionForPlayer()}");
            }

            // Narrative guidance
            prompt.AppendLine($"Conceptual Output: {template.ConceptualOutput}");
            prompt.AppendLine($"Success Outcome Narrative Guidance: {template.SuccessOutcomeNarrativeGuidance}");
            prompt.AppendLine($"Failure Outcome Narrative Guidance: {template.FailureOutcomeNarrativeGuidance}");

            // Contextual costs
            if (template.ContextualCosts != null && template.ContextualCosts.Count > 0)
            {
                prompt.AppendLine("Contextual Costs:");
                foreach (var kvp in template.ContextualCosts)
                {
                    prompt.AppendLine($"  * {kvp.Key}: Energy={kvp.Value.EnergyCost}, Money={kvp.Value.MoneyCost}, Reputation={kvp.Value.ReputationImpact}, Time={kvp.Value.TimeCost}");
                }
            }
        }

        prompt.AppendLine("");
        prompt.AppendLine("INSTRUCTIONS:");
        prompt.AppendLine("1. Generate a narrative beat description (2-3 sentences) appropriate to the current encounter context and encounter state.");
        prompt.AppendLine("2. Create 4-6 distinct choices that advance toward the player's goals.");
        prompt.AppendLine("3. For each choice:");
        prompt.AppendLine("   - Set Focus cost (0-2)");
        prompt.AppendLine("   - Define which skill cards can be used");
        prompt.AppendLine("   - Set appropriate difficulty (Easy=2, Standard=3, Hard=4, Exceptional=5)");
        prompt.AppendLine("   - Select appropriate template from the provided options");

        GameWorld gameWorld = context.GameWorld;

        // Add contextual guidance
        if (gameWorld.DeadlineDay > 0)
        {
            int daysRemaining = gameWorld.DeadlineDay - GameWorld.CurrentDay;

            if (daysRemaining <= 3)
            {
                prompt.AppendLine($"4. IMPORTANT: Create a sense of urgency due to the approaching deadline: {daysRemaining} days remaining.");
            }
        }

        // Add memory guidance
        if (gameWorld.GetPlayer().Memories.Any())
        {
            prompt.AppendLine("6. Reference the player's past experiences where relevant.");
        }

        prompt.AppendLine();
    }
}
