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
        AddLocationContext(prompt, context);

        // Add base context
        AddPlayerContext(prompt, context);

        // Add memory context
        AddMemoryContext(prompt, gameWorld);

        // Add core game state context
        AddEncounterContext(prompt, context, state, player);

        // Add time context
        AddTimeContext(prompt, gameWorld);

        // Add resource context
        AddResourceContext(prompt, gameWorld.GetPlayer());

        // Add travel context
        AddTravelContext(prompt, gameWorld);

        // Replace Prompt Context placeholder
        string content = BuildFinalContent(template, prompt);

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

        // Add player context
        AddPlayerContext(prompt, context);

        // Add core game state context
        AddEncounterContext(prompt, context, state, player);

        // Add choices information
        AddChoiceTemplatesContext(prompt, context, choiceTemplates);

        // Replace Prompt Context placeholder
        string content = BuildFinalContent(template, prompt);

        AIPrompt aiPrompt = new AIPrompt()
        {
            Content = content
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

        // Add base context
        AddLocationContext(prompt, context);

        // Add base context
        AddPlayerContext(prompt, context);

        // Add core game state context
        AddEncounterContext(prompt, context, state, player);

        // Add selected choice context
        AddSelectedChoiceContext(prompt, state, chosenOption);

        // Replace Prompt Context placeholder
        string content = BuildFinalContent(template, prompt);

        AIPrompt aiPrompt = new AIPrompt()
        {
            Content = content
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
        string content = BuildFinalContent(template, prompt);

        AIPrompt aiPrompt = new AIPrompt()
        {
            Content = content
        };

        return aiPrompt;
    }
    public AIPrompt BuildPostEncounterEvolutionPrompt(PostEncounterEvolutionInput input)
    {
        string template = promptTemplates[WORLD_EVOLUTION_MD];

        string prompt =
            template
            .Replace("{characterBackground}", input.CharacterBackground)
            .Replace("{currentLocation}", input.CurrentLocation)
            .Replace("{encounterOutcome}", input.EncounterOutcome)
            .Replace("{health}", input.Health.ToString())
            .Replace("{maxHealth}", input.MaxHealth.ToString())
            .Replace("{stamina}", input.Stamina.ToString())
            .Replace("{maxStamina}", input.MaxStamina.ToString())
            .Replace("{allKnownLocations}", input.KnownLocations)
            .Replace("{connectedLocations}", input.ConnectedLocations)
            .Replace("{currentLocationSpots}", input.CurrentLocationSpots)
            .Replace("{allExistingActions}", input.AllExistingActions)
            .Replace("{knownCharacters}", input.KnownCharacters)
            .Replace("{activeContracts}", input.ActiveContracts)
            .Replace("{currentLocationSpots}", input.CurrentLocationSpots)
            .Replace("{connectedLocations}", input.ConnectedLocations);

        AIPrompt aiPrompt = new AIPrompt()
        {
            Content = prompt.ToString()
        };

        return aiPrompt;
    }

    public AIPrompt BuildLocationCreationPrompt(LocationCreationInput input)
    {
        string template = promptTemplates[LOCATION_GENERATION_MD];

        string content =
            template
            .Replace("{characterArchetype}", input.CharacterArchetype)
            .Replace("{locationName}", input.TravelDestination)
            .Replace("{allKnownLocations}", input.KnownLocations)
            .Replace("{originLocationName}", input.TravelOrigin)
            .Replace("{knownCharacters}", input.KnownCharacters)
            .Replace("{activeContracts}", input.ActiveContracts);
        AIPrompt prompt = new AIPrompt()
        {
            Content = content
        };

        return prompt;
    }

    public AIPrompt BuildActionGenerationPrompt(ActionGenerationContext context)
    {
        string template = promptTemplates[ACTION_GENERATION_MD];

        string content =
            template
            .Replace("{ACTIONNAME}", context.ActionId)
            .Replace("{SPOT_NAME}", context.SpotName)
            .Replace("{LOCATION_NAME}", context.LocationName);
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
            Content = template.Replace("{FILE_CONTENT}", input.OldMemory)
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
            string jsonContent = mdContent;
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

    public string GetSystemMessage(WorldStateInput input)
    {
        string staticSystemPrompt = promptTemplates[SYSTEM_MD1];

        string dynamicSystemPrompt = staticSystemPrompt
            .Replace("{CHARACTER_ARCHETYPE}", input.PlayerArchetype)
            .Replace("{ENERGY}", input.Stamina.ToString())
            .Replace("{MAX_ENERGY}", input.MaxStamina.ToString())
            .Replace("{COINS}", input.Coins.ToString())
            .Replace("{CURRENT_LOCATION}", input.CurrentLocation)
            .Replace("{LOCATION_DEPTH}", input.LocationDepth.ToString())
            .Replace("{CURRENT_SPOT}", input.CurrentSpot)
            .Replace("{CONNECTED_LOCATIONS}", input.ConnectedLocations)
            .Replace("{LOCATION_SPOTS}", input.LocationSpots)
            .Replace("{INVENTORY}", input.Inventory)
            .Replace("{KNOWN_CHARACTERS}", input.KnownCharacters)
            .Replace("{ACTIVE_Contracts}", input.ActiveContracts)
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
            prompt.AppendLine($"  * Relationship: {context.TargetNPC.PlayerRelationship}");
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

    private static void AddPlayerContext(StringBuilder prompt, EncounterContext context)
    {
        prompt.AppendLine();
        prompt.AppendLine("PLAYER CHARACTER CONTEXT:");

        string name = context.Player.Name.ToString();
        string gender = context.Player.Gender.ToString();
        string playerArchetype = context.Player.Archetype.ToString();

        prompt.AppendLine("Player name: " + name);
        prompt.AppendLine("Player gender: " + gender);
        prompt.AppendLine("Player archetype: " + playerArchetype);
        prompt.AppendLine();
    }

    private static void AddLocationContext(StringBuilder prompt, EncounterContext context)
    {
        prompt.AppendLine();
        prompt.AppendLine("LOCATION CONTEXT:");

        string environmentDetails = $"A {context.LocationSpotName.ToLower()} in a {context.LocationName.ToLower()}";
        string npcList = "";
        if (string.IsNullOrWhiteSpace(npcList))
        {
            npcList = "None";
        }
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
        prompt.AppendLine("Player character Goal: " + actionGoal);
        prompt.AppendLine("Environment Details: " + environmentDetails);
        prompt.AppendLine("NPCs Present: " + npcList);
        prompt.AppendLine("Chosen Approach: " + approachDetails);

        prompt.AppendLine();
    }

    private void AddMemoryContext(StringBuilder prompt, GameWorld gameWorld)
    {
        prompt.AppendLine();
        prompt.AppendLine("PLAYER CHARACTER MEMORIES");

        MemoryFileAccess memoryFileAccess = new MemoryFileAccess(gameWorld.GetGameInstanceId());
        List<string> memories = memoryFileAccess.GetAllMemories().Result;

        if (memories.Any())
        {
            foreach (string memory in memories)
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

        BeatOutcomes outcome = state.EncounterOutcome;

        string encounterGoal = context.LocationAction.ObjectiveDescription;
        prompt.AppendLine(encounterGoal);

        string goalAchievementStatus =
            outcome == BeatOutcomes.Success ? $"You have successfully achieved your goal to {encounterGoal}" :
            outcome == BeatOutcomes.Failure ? $"You have failed to {encounterGoal}" : string.Empty;

        prompt.AppendLine(goalAchievementStatus);

        prompt.AppendLine();
    }

    private void AddTimeContext(StringBuilder prompt, GameWorld gameWorld)
    {
        prompt.AppendLine();
        prompt.AppendLine("TIME CONTEXT:");

        prompt.AppendLine($"- Current Day: {gameWorld.CurrentDay}");
        prompt.AppendLine($"- Time of Day: {gameWorld.CurrentTimeBlock}");

        if (gameWorld.DeadlineDay > 0)
        {
            int daysRemaining = gameWorld.DeadlineDay - gameWorld.CurrentDay;
            prompt.AppendLine($"- Deadline: {daysRemaining} days remaining");
            prompt.AppendLine($"- Deadline Reason: {gameWorld.DeadlineReason}");
        }

        prompt.AppendLine();
    }

    private void AddResourceContext(StringBuilder prompt, Player player)
    {
        prompt.AppendLine();
        prompt.AppendLine("RESOURCE CONTEXT:");

        prompt.AppendLine($"- Stamina: {player.Stamina}/{player.MaxStamina}");
        prompt.AppendLine($"- Money: {player.Coins} coins");
        prompt.AppendLine($"- Reputation: {player.Reputation} ({player.GetReputationLevel()})");

        prompt.AppendLine();
    }

    private void AddTravelContext(StringBuilder prompt, GameWorld gameWorld)
    {
        // TODO: Implement when encounters are needed
        // Currently not used as encounters are not implemented
        return;
    }

    private void AddSelectedChoiceContext(StringBuilder prompt, EncounterState state, EncounterChoice choice)
    {
        prompt.AppendLine();

        prompt.AppendLine("SELECTED CHOICE:");
        prompt.AppendLine($"{choice.NarrativeText}");

        bool success = state.LastBeatOutcome == BeatOutcomes.Success;
        if (success)
        {
            prompt.AppendLine("RESULT: Success");
            //prompt.AppendLine($"{choice.SuccessNarrative}");
        }
        else
        {
            prompt.AppendLine("RESULT: Failure");
            //prompt.AppendLine($"{choice.FailureNarrative}");
        }

        prompt.AppendLine();
    }

    private void AddChoiceTemplatesContext(StringBuilder prompt, EncounterContext context, List<ChoiceTemplate> choiceTemplates)
    {
        prompt.AppendLine();

        choiceTemplates.Clear(); // clear list

        for (int i = 0; i < choiceTemplates.Count; i++)
        {
            ChoiceTemplate template = choiceTemplates[i];
            prompt.AppendLine($"\nCHOICE TEMPLATE {i + 1}: {template.TemplateName}");
            prompt.AppendLine($"Template Purpose: {template.TemplatePurpose}");
            prompt.AppendLine($"Weight (How often it should be picked relative to other templates): {template.Weight}");

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
            int daysRemaining = gameWorld.DeadlineDay - gameWorld.CurrentDay;

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

    private static string BuildFinalContent(string template, StringBuilder prompt)
    {
        string content = template.Replace("{PROMPT_CONTEXT}", prompt.ToString());

        StringBuilder finalContent = new StringBuilder();
        foreach (string line in template.Split(@"\n"))
        {
            if (line.Contains("{PROMPT_CONTEXT}"))
            {
                finalContent.AppendLine(line.Replace("{PROMPT_CONTEXT}", prompt.ToString()));
            }
            else
            {
                finalContent.AppendLine(line);
            }
        }

        // Normalize to ensure only one empty line (i.e. two consecutive newlines) between paragraphs.
        string normalizedContent = System.Text.RegularExpressions.Regex.Replace(finalContent.ToString(), @"(\n\s*){3,}", "\n\n");
        return normalizedContent;
    }
}
