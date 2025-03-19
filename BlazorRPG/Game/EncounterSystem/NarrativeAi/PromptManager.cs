using System.Text;

public class PromptManager
{
    private readonly Dictionary<string, string> _promptTemplates;

    private const string SYSTEM_MD = "system";
    private const string INTRO_MD = "introduction";
    private const string REACTION_MD = "reaction";
    private const string CHOICES_MD = "choices";
    private const string ENDING_MD = "ending";
    private const string MEMORY_MD = "memory";

    public PromptManager(IConfiguration configuration)
    {
        // Load prompts from JSON files
        string promptsPath = configuration.GetValue<string>("NarrativePromptsPath") ?? "Data/Prompts";

        // Load all prompt templates
        _promptTemplates = new Dictionary<string, string>();
        LoadPromptTemplates(promptsPath);
    }

    public string BuildIntroductionPrompt(
        NarrativeContext context,
        EncounterStatus state,
        string memoryContent)
    {
        string template = _promptTemplates[INTRO_MD];

        // Get primary approach values
        string primaryApproach = state.ApproachTags.OrderByDescending(t => t.Value).First().Key.ToString();
        string secondaryApproach = state.ApproachTags.OrderByDescending(t => t.Value).Skip(1).First().Key.ToString();

        // Get significant focus tags
        string primaryFocus = state.FocusTags.OrderByDescending(t => t.Value).First().Key.ToString();
        string secondaryFocus = state.FocusTags.OrderByDescending(t => t.Value).Skip(1).First().Key.ToString();

        // Format environment and NPC details
        string environmentDetails = $"A {context.LocationName.ToLower()} with difficulty level {state.EncounterInfo?.Difficulty ?? 1}";
        string npcList = "Local individuals relevant to the encounter";
        string timeConstraints = $"Maximum {state.MaxTurns} turns";
        string additionalChallenges = state.EncounterInfo != null
            ? $"Difficulty level {state.EncounterInfo.Difficulty} (adds +{state.EncounterInfo.Difficulty} pressure per turn)"
            : "Standard difficulty";

        // Format character archetype based on primary approach
        string characterArchetype = GetCharacterArchetype(primaryApproach);

        // Format approach stats
        string approachStats = FormatApproachValues(state);

        ActionImplementation actionImplementation = context.ActionImplementation;
        string encounterGoal = actionImplementation.Goal;
        string encounterComplication = actionImplementation.Complication;

        // Replace placeholders in template
        string prompt = template
            .Replace("{ENCOUNTER_TYPE}", context.EncounterType.ToString())
            .Replace("{LOCATION_NAME}", context.LocationName)
            .Replace("{LOCATION_SPOT}", context.locationSpotName)
            .Replace("{CHARACTER_ARCHETYPE}", characterArchetype)
            .Replace("{APPROACH_STATS}", approachStats)
            .Replace("{CHARACTER_GOAL}", encounterGoal)
            .Replace("{ENVIRONMENT_DETAILS}", environmentDetails)
            .Replace("{NPC_LIST}", npcList)
            .Replace("{TIME_CONSTRAINTS}", timeConstraints)
            .Replace("{ADDITIONAL_CHALLENGES}", additionalChallenges)
            .Replace("{ENCOUNTER_COMPLICATION}", encounterComplication)
            .Replace("{MEMORY_CONTENT}", memoryContent);

        return prompt;
    }

    public string BuildChoicesPrompt(
        NarrativeContext context,
        List<IChoice> choices,
        List<ChoiceProjection> projections,
        EncounterStatus state)
    {
        string template = _promptTemplates[CHOICES_MD];

        // Create a complete encounter history for context
        NarrativeSummaryBuilder builder = new NarrativeSummaryBuilder();
        string completeHistory = builder.CreateCompleteHistory(context);

        // Get the most recent narrative event
        string currentSituation = "No previous narrative available.";
        if (context.Events.Count > 0)
        {
            NarrativeEvent lastEvent = context.Events[context.Events.Count - 1];
            currentSituation = lastEvent.SceneDescription;
        }

        // Get encounter type from presentation style
        string choiceStyleGuidance = GetChoiceStyleGuidance(context.EncounterType);

        // Format the active narrative tags for choice blocking awareness
        StringBuilder narrativeTagsInfo = new StringBuilder();
        narrativeTagsInfo.AppendLine("## Active Narrative Tags:");
        foreach (IEncounterTag? tag in state.ActiveTags.Where(t => t is NarrativeTag))
        {
            NarrativeTag narrativeTag = (NarrativeTag)tag;
            narrativeTagsInfo.AppendLine($"- {tag.Name}: Blocks {narrativeTag.BlockedFocus} focus choices");
        }

        // Add location strategic preferences
        StringBuilder locationPreferences = new StringBuilder();
        locationPreferences.AppendLine("## Location Strategic Information:");

        if (state.EncounterInfo?.MomentumBoostApproaches?.Any() == true)
        {
            locationPreferences.AppendLine($"- Favored Approaches: {string.Join(", ", state.EncounterInfo.MomentumBoostApproaches)}");
            locationPreferences.AppendLine("  These approaches work particularly well in this location.");
        }

        if (state.EncounterInfo?.DangerousApproaches?.Any() == true)
        {
            locationPreferences.AppendLine($"- Disfavored Approaches: {string.Join(", ", state.EncounterInfo.DangerousApproaches)}");
            locationPreferences.AppendLine("  These approaches are challenging or risky here.");
        }

        if (state.EncounterInfo?.PressureReducingFocuses?.Any() == true)
        {
            locationPreferences.AppendLine($"- Favored Focuses: {string.Join(", ", state.EncounterInfo.PressureReducingFocuses)}");
            locationPreferences.AppendLine("  These focuses are particularly effective here.");
        }

        if (state.EncounterInfo?.MomentumReducingFocuses?.Any() == true)
        {
            locationPreferences.AppendLine($"- Disfavored Focuses: {string.Join(", ", state.EncounterInfo.MomentumReducingFocuses)}");
            locationPreferences.AppendLine("  These focuses may lead to resource loss or complications.");
        }

        // Format choices info
        StringBuilder choicesInfo = new StringBuilder();
        for (int i = 0; i < choices.Count; i++)
        {
            IChoice choice = choices[i];
            ChoiceProjection projection = projections[i];

            // Get the primary approach tag
            ApproachTags primaryApproach = GetPrimaryApproach(choice);

            // Determine if this choice is blocked by a narrative tag
            bool isBlocked = state.ActiveTags.Any(t => t is NarrativeTag nt && nt.BlockedFocus == choice.Focus);

            choicesInfo.AppendLine($@"
Choice {i + 1}: {choice.Name}
- Approach: {primaryApproach} 
- Focus: {choice.Focus}
- Effect Type: {choice.EffectType}
- Momentum Change: {projection.MomentumGained}
- Pressure Change: {projection.PressureBuilt}
- Health Change: {projection.HealthChange}
- Concentration Change: {projection.ConcentrationChange}
- Confidence Change: {projection.ConfidenceChange}");

            // Add encounter ending information if this choice will end the encounter
            if (projection.EncounterWillEnd)
            {
                choicesInfo.AppendLine($"- Encounter Will End: True");
                choicesInfo.AppendLine($"- Final Outcome: {projection.ProjectedOutcome}");
                choicesInfo.AppendLine($"- Goal Achievement: " +
                    $"{(projection.ProjectedOutcome != EncounterOutcomes.Failure ? 
                    "Will achieve goal to" : "Will fail to")} {context.ActionImplementation.Goal}");
            }

            // Add any new narrative tags that would activate
            if (projection.NewlyActivatedTags.Any())
            {
                choicesInfo.AppendLine("- Would Activate Tags: " + string.Join(", ", projection.NewlyActivatedTags));
            }

            // Add any strategic tag effects
            List<ChoiceProjection.ValueComponent> momentumComponents = projection.MomentumComponents.Where(c => c.Source != "Momentum Choice Base").ToList();
            List<ChoiceProjection.ValueComponent> pressureComponents = projection.PressureComponents.Where(c => c.Source != "Pressure Choice Base").ToList();

            if (momentumComponents.Any() || pressureComponents.Any())
            {
                choicesInfo.AppendLine("- Strategic Effects:");
                foreach (ChoiceProjection.ValueComponent? comp in momentumComponents)
                {
                    choicesInfo.AppendLine($"  * {comp.Source}: {comp.Value} momentum");
                }
                foreach (ChoiceProjection.ValueComponent? comp in pressureComponents)
                {
                    choicesInfo.AppendLine($"  * {comp.Source}: {comp.Value} pressure");
                }
            }
        }

        // Format approach values
        string approachValues = FormatApproachValues(state);

        // Format active tag names
        string activeTags = string.Join(", ", state.ActiveTagNames);

        string encounterGoal = context.ActionImplementation.Goal;

        // Replace placeholders in template
        string prompt = template
            .Replace("{ENCOUNTER_TYPE}", context.EncounterType.ToString())
            .Replace("{LOCATION_NAME}", context.LocationName)
            .Replace("{LOCATION_SPOT}", context.locationSpotName)
            .Replace("{CHARACTER_GOAL}", encounterGoal)
            .Replace("{CURRENT_SITUATION}", currentSituation)
            .Replace("{ACTIVE_TAGS}", narrativeTagsInfo.ToString())
            .Replace("{LOCATION_PREFERENCES}", locationPreferences.ToString())
            .Replace("{MOMENTUM}", state.Momentum.ToString())
            .Replace("{PRESSURE}", state.Pressure.ToString())
            .Replace("{APPROACH_VALUES}", approachValues)
            .Replace("{LIST_TAGS}", activeTags)
            .Replace("{NPC_LIST}", "relevant individuals in the scene")
            .Replace("{CHOICES_INFO}", choicesInfo.ToString())
            .Replace("{CHOICE_STYLE_GUIDANCE}", choiceStyleGuidance);

        return prompt;
    }

    public string BuildReactionPrompt(
        NarrativeContext context,
        IChoice chosenOption,
        ChoiceNarrative choiceDescription,
        ChoiceOutcome outcome,
        EncounterStatus newState)
    {
        string template = _promptTemplates[REACTION_MD];

        // Extract primary approach tag
        ApproachTags primaryApproach = GetPrimaryApproach(chosenOption);

        // Format tag changes for narrative representation
        string tagChangesGuidance = FormatTagChangesForNarrative(outcome);

        // Format activated/deactivated tags
        string tagActivationGuidance = FormatTagActivationForNarrative(outcome);

        // Create a complete encounter history for context
        NarrativeSummaryBuilder builder = new NarrativeSummaryBuilder();
        string completeHistory = builder.CreateCompleteHistory(context);

        // Get encounter type from presentation style
        string encounterType = GetEncounterStyleGuidance(context.EncounterType);

        // Determine turn information from events
        int currentTurn = context.Events.Count > 0 ? context.Events.Count : 1;
        // Default to 6 turns if we can't determine it
        int maxTurns = 6;

        // Format outcome components to represent momentum and pressure changes
        StringBuilder strategicEffects = new StringBuilder();
        strategicEffects.AppendLine("## Momentum and Pressure Changes:");
        strategicEffects.AppendLine($"- Momentum Gained: {outcome.MomentumGain}");
        strategicEffects.AppendLine($"- Pressure Gained: {outcome.PressureGain}");

        if (outcome.HealthChange != 0)
        {
            strategicEffects.AppendLine($"- Health Change: {outcome.HealthChange}");
        }

        if (outcome.ConcentrationChange != 0)
        {
            strategicEffects.AppendLine($"- Focus Change: {outcome.ConcentrationChange}");
        }

        if (outcome.ConfidenceChange != 0)
        {
            strategicEffects.AppendLine($"- Confidence Change: {outcome.ConfidenceChange}");
        }

        // Get previous momentum and pressure from the context's last state
        int previousMomentum = newState.Momentum - outcome.MomentumGain;
        int previousPressure = newState.Pressure - outcome.PressureGain;

        // Extract encounter goal from inciting action
        string encounterGoal = context.ActionImplementation.Goal;

        // Get the choice narrative description
        string choiceNarrativeDesc = choiceDescription?.FullDescription ?? chosenOption.Name;

        // Replace placeholders in template
        string prompt = template
            .Replace("{ENCOUNTER_TYPE}", context.EncounterType.ToString())
            .Replace("{LOCATION_NAME}", context.LocationName)
            .Replace("{LOCATION_SPOT}", context.locationSpotName)
            .Replace("{CHARACTER_GOAL}", encounterGoal)
            .Replace("{SELECTED_CHOICE}", chosenOption.Name)
            .Replace("{CHOICE_DESCRIPTION}", choiceNarrativeDesc)
            .Replace("{APPROACH}", primaryApproach.ToString())
            .Replace("{FOCUS}", chosenOption.Focus.ToString())
            .Replace("{EFFECT_TYPE}", chosenOption.EffectType.ToString())
            .Replace("{M_OLD}", previousMomentum.ToString())
            .Replace("{P_OLD}", previousPressure.ToString())
            .Replace("{M_NEW}", newState.Momentum.ToString())
            .Replace("{P_NEW}", newState.Pressure.ToString())
            .Replace("{APPROACH_CHANGES}", FormatApproachChanges(outcome.EncounterStateTagChanges))
            .Replace("{FOCUS_CHANGES}", FormatFocusChanges(outcome.FocusTagChanges))
            .Replace("{NEW_NARRATIVE_TAGS}", FormatNewlyActivatedTags(outcome.NewlyActivatedTags))
            .Replace("{STRATEGIC_EFFECTS}", strategicEffects.ToString())
            .Replace("{CURRENT_TURN}", currentTurn.ToString())
            .Replace("{MAX_TURNS}", maxTurns.ToString())
            .Replace("{ENCOUNTER_STYLE_GUIDANCE}", encounterType);

        return prompt;
    }

    public string BuildEncounterEndPrompt(
        NarrativeContext context,
        EncounterStatus finalState,
        EncounterOutcomes outcome,
        IChoice finalChoice)
    {
        string template = _promptTemplates[ENDING_MD];

        // Get the last narrative event
        string lastNarrative = "No previous narrative available.";
        if (context.Events.Count > 0)
        {
            NarrativeEvent lastEvent = context.Events[context.Events.Count - 1];
            lastNarrative = lastEvent.SceneDescription;
        }

        // Format approach values
        string approachValues = FormatApproachValues(finalState);

        // Format focus values
        StringBuilder focusValues = new StringBuilder();
        foreach (KeyValuePair<FocusTags, int> focus in finalState.FocusTags)
        {
            focusValues.Append($"{focus.Key} {focus.Value}, ");
        }

        // Remove trailing comma
        string formattedFocusValues = focusValues.ToString().TrimEnd(',', ' ');

        // Get encounter type from presentation style
        string encounterType = GetEncounterStyleGuidance(context.EncounterType);

        // Extract encounter goal from inciting action
        string encounterGoal = context.ActionImplementation.Goal;

        // Add goal achievement status
        string goalAchievementStatus = outcome != EncounterOutcomes.Failure
            ? $"You have successfully achieved your goal to {encounterGoal}"
            : $"You have failed to {encounterGoal}";

        string choiceNarrativeDesc = finalChoice.Description ?? finalChoice.Name;

        // Replace placeholders in template
        string prompt = template
            .Replace("{ENCOUNTER_TYPE}", context.EncounterType.ToString())
            .Replace("{ENCOUNTER_OUTCOME}", outcome.ToString())
            .Replace("{LOCATION_NAME}", context.LocationName)
            .Replace("{LOCATION_SPOT}", context.locationSpotName)
            .Replace("{SELECTED_CHOICE}", finalChoice.Name)
            .Replace("{CHOICE_DESCRIPTION}", choiceNarrativeDesc)
            .Replace("{CHARACTER_GOAL}", encounterGoal)
            .Replace("{FINAL_MOMENTUM}", finalState.Momentum.ToString())
            .Replace("{FINAL_PRESSURE}", finalState.Pressure.ToString())
            .Replace("{APPROACH_VALUES}", approachValues)
            .Replace("{FOCUS_VALUES}", formattedFocusValues)
            .Replace("{LAST_NARRATIVE}", lastNarrative)
            .Replace("{GOAL_ACHIEVEMENT_STATUS}", goalAchievementStatus);

        return prompt;
    }

    public string BuildMemoryPrompt(
        NarrativeContext context,
        ChoiceOutcome outcome,
        EncounterStatus newState,
        string oldMemory)
    {
        string template = _promptTemplates[MEMORY_MD];

        NarrativeSummaryBuilder builder = new NarrativeSummaryBuilder();
        string completeHistory = builder.CreateCompleteHistory(context);
        var summary = builder.CreateSummary(context);

        // Replace placeholders in template
        string prompt = template
            .Replace("{FILE_CONTENT}", oldMemory);

        return prompt;
    }


    // Helper methods for formatting ChoiceOutcome data
    private string FormatTagChangesForNarrative(ChoiceOutcome outcome)
    {
        StringBuilder changes = new StringBuilder();

        // Format approach tag changes
        if (outcome.EncounterStateTagChanges.Any())
        {
            changes.AppendLine("Tag changes that should be reflected in narrative:");
            foreach (KeyValuePair<ApproachTags, int> change in outcome.EncounterStateTagChanges)
            {
                if (Math.Abs(change.Value) >= 2) // Only emphasize significant changes
                {
                    string direction = change.Value > 0 ? "increased" : "decreased";
                    changes.AppendLine($"- {change.Key} {direction} by {Math.Abs(change.Value)}: Show this as {GetApproachChangeDescription(change.Key, change.Value > 0)}");
                }
            }
        }

        // Format focus tag changes
        if (outcome.FocusTagChanges.Any())
        {
            if (!changes.ToString().Contains("Tag changes")) // Add header if not already added
            {
                changes.AppendLine("Tag changes that should be reflected in narrative:");
            }

            foreach (KeyValuePair<FocusTags, int> change in outcome.FocusTagChanges)
            {
                if (Math.Abs(change.Value) >= 2) // Only emphasize significant changes
                {
                    string direction = change.Value > 0 ? "increased" : "decreased";
                    changes.AppendLine($"- {change.Key} {direction} by {Math.Abs(change.Value)}: Show this as {GetFocusChangeDescription(change.Key, change.Value > 0)}");
                }
            }
        }

        // Add resource changes if significant
        if (outcome.HealthChange <= -2)
        {
            changes.AppendLine($"- Health {outcome.HealthChange}: Show physical injury or pain");
        }

        if (outcome.ConcentrationChange <= -2)
        {
            changes.AppendLine($"- Focus {outcome.ConcentrationChange}: Show mental fatigue or distraction");
        }

        if (outcome.ConfidenceChange <= -2)
        {
            changes.AppendLine($"- Confidence {outcome.ConfidenceChange}: Show social embarrassment or loss of status");
        }

        return changes.ToString();
    }

    private string FormatApproachChanges(Dictionary<ApproachTags, int> approachChanges)
    {
        if (approachChanges == null || !approachChanges.Any())
            return "No significant approach changes";

        StringBuilder builder = new StringBuilder();
        foreach (KeyValuePair<ApproachTags, int> change in approachChanges)
        {
            builder.AppendLine($"- {change.Key}: {(change.Value > 0 ? "+" : "")}{change.Value}");
        }

        return builder.ToString();
    }

    private string FormatFocusChanges(Dictionary<FocusTags, int> focusChanges)
    {
        if (focusChanges == null || !focusChanges.Any())
            return "No significant focus changes";

        StringBuilder builder = new StringBuilder();
        foreach (KeyValuePair<FocusTags, int> change in focusChanges)
        {
            builder.AppendLine($"- {change.Key}: {(change.Value > 0 ? "+" : "")}{change.Value}");
        }

        return builder.ToString();
    }

    private string FormatNewlyActivatedTags(List<string> newTags)
    {
        if (newTags == null || !newTags.Any())
            return "No newly activated tags";

        StringBuilder builder = new StringBuilder();
        foreach (string tag in newTags)
        {
            builder.AppendLine($"- {tag}");
        }

        return builder.ToString();
    }

    private string FormatTagActivationForNarrative(ChoiceOutcome outcome)
    {
        StringBuilder result = new StringBuilder();

        if (outcome.NewlyActivatedTags.Count > 0)
        {
            result.AppendLine("Newly activated tags to incorporate in narrative:");
            foreach (string tag in outcome.NewlyActivatedTags)
            {
                result.AppendLine($"- {tag}: Introduce narrative elements that reflect this tag's activation");
            }
        }

        if (outcome.DeactivatedTags.Count > 0)
        {
            result.AppendLine("Deactivated tags to acknowledge in narrative:");
            foreach (string tag in outcome.DeactivatedTags)
            {
                result.AppendLine($"- {tag}: Show how this aspect has diminished or is no longer relevant");
            }
        }

        return result.ToString();
    }

    private string FormatApproachValues(EncounterStatus state)
    {
        List<string> approaches = new List<string>();
        foreach (KeyValuePair<ApproachTags, int> approach in state.ApproachTags)
        {
            approaches.Add($"{approach.Key} {approach.Value}");
        }
        return string.Join(", ", approaches);
    }

    private string GetCharacterArchetype(string primaryApproach)
    {
        return primaryApproach switch
        {
            "Dominance" => "Warrior",
            "Rapport" => "Bard",
            "Analysis" => "Scholar",
            "Precision" => "Ranger",
            "Evasion" => "Thief",
            _ => "Traveler"
        };
    }

    private string GetEncounterStyleGuidance(EncounterTypes type)
    {
        return type switch
        {
            EncounterTypes.Social => "Direct dialogue with simple, practical words that reflect medieval speech patterns. Focus on social dynamics, status differences, and the traveler's attempt to navigate social hierarchies. Include some direct speech with quotation marks, showing the exact words exchanged between the player character and NPC.",
            EncounterTypes.Intellectual => "Brief thought process using common language appropriate to a medieval traveler. Express observations, deductions, and problem-solving through inner monologue. Focus on practical knowledge and survival-oriented thinking rather than academic or scholarly reasoning.",
            EncounterTypes.Physical => "Clear description of physical actions and immediate results. Emphasize bodily sensations, physical effort, fatigue, and the mechanical realities of movement and exertion. Include details about weight, texture, temperature, and other tactile elements that ground the narrative in physical reality.",
            _ => "Practical description focusing on immediate situation"
        };
    }

    private string GetChoiceStyleGuidance(EncounterTypes type)
    {
        return type switch
        {
            EncounterTypes.Social => "Direct speech with dialogue in quotes, focus on status and relationships",
            EncounterTypes.Intellectual => "Observations and problem-solving as inner thoughts, practical knowledge",
            EncounterTypes.Physical => "Detailed physical actions, bodily sensations, and environment interactions",
            _ => "Practical description focusing on immediate situation"
        };
    }

    private ApproachTags GetPrimaryApproach(IChoice choice)
    {
        // Find the approach tag with the largest modification
        List<TagModification> approachMods = choice.TagModifications
            .Where(m => m.Type == TagModification.TagTypes.EncounterState)
            .Where(m => IsApproachTag((ApproachTags)m.Tag))
            .OrderByDescending(m => m.Delta)
            .ToList();

        if (approachMods.Any())
        {
            return (ApproachTags)approachMods.First().Tag;
        }

        // Default to Analysis if no approach is found (fallback)
        return ApproachTags.Analysis;
    }

    private bool IsApproachTag(ApproachTags tag)
    {
        return tag == ApproachTags.Dominance ||
               tag == ApproachTags.Rapport ||
               tag == ApproachTags.Analysis ||
               tag == ApproachTags.Precision ||
               tag == ApproachTags.Evasion;
    }

    private string GetApproachChangeDescription(ApproachTags approach, bool isPositive)
    {
        return (approach, isPositive) switch
        {
            (ApproachTags.Dominance, true) => "increased authority, commanding presence, or intimidation factor",
            (ApproachTags.Dominance, false) => "diminished authority, lessened presence, or reduced intimidation factor",
            (ApproachTags.Rapport, true) => "improved charm, likeability, or social influence",
            (ApproachTags.Rapport, false) => "reduced charm, awkwardness, or social disconnection",
            (ApproachTags.Analysis, true) => "sharper observation, clearer thinking, or deeper understanding",
            (ApproachTags.Analysis, false) => "confusion, overlooking details, or failing to make connections",
            (ApproachTags.Precision, true) => "improved accuracy, careful execution, or greater control",
            (ApproachTags.Precision, false) => "clumsiness, carelessness, or lack of control",
            (ApproachTags.Evasion, true) => "better stealth, secrecy, or ability to hide intentions",
            (ApproachTags.Evasion, false) => "exposure, visibility, or inability to hide",
            _ => "significant change in approach"
        };
    }

    private string GetFocusChangeDescription(FocusTags focus, bool isPositive)
    {
        return (focus, isPositive) switch
        {
            (FocusTags.Relationship, true) => "better social connections, trust, or understanding of others",
            (FocusTags.Relationship, false) => "damaged relationships, broken trust, or social isolation",
            (FocusTags.Information, true) => "greater knowledge, awareness, or understanding of facts",
            (FocusTags.Information, false) => "confusion, misinformation, or lack of awareness",
            (FocusTags.Physical, true) => "improved physical capability, bodily awareness, or strength",
            (FocusTags.Physical, false) => "physical limitation, discomfort, or weakness",
            (FocusTags.Environment, true) => "better awareness of surroundings, control of space, or environmental advantage",
            (FocusTags.Environment, false) => "environmental disadvantage, disorientation, or lack of spatial awareness",
            (FocusTags.Resource, true) => "better resource management, acquisition of valuable items, or material advantage",
            (FocusTags.Resource, false) => "resource depletion, loss of valuable items, or material disadvantage",
            _ => "significant change in focus"
        };
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



    public string GetSystemMessage()
    {
        string template = _promptTemplates[SYSTEM_MD];
        return template;
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

}
