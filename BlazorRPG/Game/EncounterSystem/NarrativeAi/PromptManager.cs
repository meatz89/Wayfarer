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
        EncounterStatusModel state,
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

    public string BuildReactionPrompt(
    NarrativeContext context,
    IChoice chosenOption,
    ChoiceNarrative choiceDescription,
    ChoiceOutcome outcome,
    EncounterStatusModel state)
    {
        string template = _promptTemplates[REACTION_MD];

        // Extract primary approach tag
        ApproachTags primaryApproach = GetPrimaryApproach(chosenOption);

        // Calculate encounter stage
        string encounterStage = DetermineEncounterStage(state.CurrentTurn, state.MaxTurns, state.Momentum, state.MaxMomentum, state.Pressure, state.MaxPressure);

        // Get previous momentum and pressure
        int previousMomentum = state.Momentum - outcome.MomentumGain;
        int previousPressure = state.Pressure - outcome.PressureGain;

        // Format strategic effects
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
            strategicEffects.AppendLine($"- Concentration Change: {outcome.ConcentrationChange}");
        }

        if (outcome.ConfidenceChange != 0)
        {
            strategicEffects.AppendLine($"- Confidence Change: {outcome.ConfidenceChange}");
        }

        // Extract encounter goal from inciting action
        string encounterGoal = context.ActionImplementation.Goal;

        // Get character status summary
        string characterStatus = BuildCharacterStatusSummary(state);

        // Replace placeholders in template
        string prompt = template
            .Replace("{ENCOUNTER_TYPE}", context.EncounterType.ToString())
            .Replace("{CURRENT_TURN}", context.Events.Count.ToString())
            .Replace("{MAX_TURNS}", state.MaxTurns.ToString())
            .Replace("{NEW_MOMENTUM}", state.Momentum.ToString())
            .Replace("{MAX_MOMENTUM}", state.MaxMomentum.ToString())
            .Replace("{SUCCESS_THRESHOLD}", state.SuccessThreshold.ToString())
            .Replace("{NEW_PRESSURE}", state.Pressure.ToString())
            .Replace("{MAX_PRESSURE}", state.MaxPressure.ToString())
            .Replace("{CURRENT_HEALTH}", state.Health.ToString())
            .Replace("{MAX_HEALTH}", state.MaxHealth.ToString())
            .Replace("{CURRENT_CONFIDENCE}", state.Confidence.ToString())
            .Replace("{MAX_CONFIDENCE}", state.MaxConfidence.ToString())
            .Replace("{CURRENT_CONCENTRATION}", state.Concentration.ToString())
            .Replace("{MAX_CONCENTRATION}", state.MaxConcentration.ToString())
            .Replace("{ENCOUNTER_STAGE}", encounterStage)
            .Replace("{SELECTED_CHOICE}", choiceDescription.ShorthandName)
            .Replace("{CHOICE_DESCRIPTION}", choiceDescription.FullDescription)
            .Replace("{CHOICE_APPROACH}", primaryApproach.ToString())
            .Replace("{CHOICE_FOCUS}", chosenOption.Focus.ToString())
            .Replace("{MOMENTUM_CHANGE}", outcome.MomentumGain.ToString())
            .Replace("{OLD_MOMENTUM}", previousMomentum.ToString())
            .Replace("{PRESSURE_CHANGE}", outcome.PressureGain.ToString())
            .Replace("{OLD_PRESSURE}", previousPressure.ToString())
            .Replace("{APPROACH_CHANGES}", FormatApproachChanges(outcome.EncounterStateTagChanges))
            .Replace("{FOCUS_CHANGES}", FormatFocusChanges(outcome.FocusTagChanges))
            .Replace("{HEALTH_CHANGE}", outcome.HealthChange.ToString())
            .Replace("{CONFIDENCE_CHANGE}", outcome.ConfidenceChange.ToString())
            .Replace("{CONCENTRATION_CHANGE}", outcome.ConcentrationChange.ToString())
            .Replace("{NEW_TAGS_ACTIVATED}", FormatNewlyActivatedTags(outcome.NewlyActivatedTags))
            .Replace("{STRATEGIC_EFFECTS}", strategicEffects.ToString())
            .Replace("{CHARACTER_GOAL}", encounterGoal)
            .Replace("{INJURIES/STATUS}", characterStatus);

        return prompt;
    }

    public string BuildChoicesPrompt(
        NarrativeContext context,
        List<IChoice> choices,
        List<ChoiceProjection> projections,
        EncounterStatusModel state)
    {
        string template = _promptTemplates[CHOICES_MD];

        // Calculate encounter stage
        string encounterStage = DetermineEncounterStage(state.CurrentTurn, state.MaxTurns, state.Momentum, state.MaxMomentum, state.Pressure, state.MaxPressure);

        // Get the most recent narrative event for current situation
        string currentSituation = "No previous narrative available.";
        if (context.Events.Count > 0)
        {
            NarrativeEvent lastEvent = context.Events[context.Events.Count - 1];
            currentSituation = lastEvent.Summary;
        }

        // Format the active narrative tags
        StringBuilder narrativeTagsInfo = new StringBuilder();
        foreach (IEncounterTag? tag in state.ActiveTags.Where(t => t is NarrativeTag))
        {
            NarrativeTag narrativeTag = (NarrativeTag)tag;
            narrativeTagsInfo.AppendLine($"- {tag.Name}: Blocks {narrativeTag.BlockedFocus} focus choices");
        }

        // Get favorable and dangerous approaches
        string favorableApproaches = FormatFavorableApproaches(state);
        string dangerousApproaches = FormatDangerousApproaches(state);

        // Format choices info
        StringBuilder choicesInfo = new StringBuilder();
        for (int i = 0; i < choices.Count; i++)
        {
            IChoice choice = choices[i];
            ChoiceProjection projection = projections[i];

            // Get the primary approach tag
            ApproachTags primaryApproach = GetPrimaryApproach(choice);

            choicesInfo.AppendLine($@"
CHOICE {i + 1}:
- Approach: {primaryApproach} 
- Focus: {choice.Focus}
- Momentum Change: {projection.MomentumGained}
- Pressure Change: {projection.PressureBuilt}
- Health Change: {projection.HealthChange}
- Concentration Change: {projection.ConcentrationChange}
- Confidence Change: {projection.ConfidenceChange}");

            // Add encounter ending information if applicable
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
            List<ChoiceProjection.ValueComponent> momentumComponents = projection.MomentumComponents
                .Where(c => c.Source != "Momentum Choice Base").ToList();
            List<ChoiceProjection.ValueComponent> pressureComponents = projection.PressureComponents
                .Where(c => c.Source != "Pressure Choice Base").ToList();

            if (momentumComponents.Any() || pressureComponents.Any())
            {
                choicesInfo.AppendLine("- Strategic Effects:");
                foreach (ChoiceProjection.ValueComponent comp in momentumComponents)
                {
                    choicesInfo.AppendLine($"  * {comp.Source}: {comp.Value} momentum");
                }
                foreach (ChoiceProjection.ValueComponent comp in pressureComponents)
                {
                    choicesInfo.AppendLine($"  * {comp.Source}: {comp.Value} pressure");
                }
            }
        }

        // Get character condition/resources
        string characterCondition = BuildCharacterStatusSummary(state);

        // Get the approach and focus for each choice
        Dictionary<int, string> choiceApproaches = ExtractChoiceApproaches(choices);
        Dictionary<int, string> choiceFocuses = ExtractChoiceFocuses(choices);

        // Replace placeholders in template
        string prompt = template
            .Replace("{ENCOUNTER_TYPE}", context.EncounterType.ToString())
            .Replace("{CURRENT_TURN}", context.Events.Count.ToString())
            .Replace("{MAX_TURNS}", state.MaxTurns.ToString())
            .Replace("{CURRENT_MOMENTUM}", state.Momentum.ToString())
            .Replace("{MAX_MOMENTUM}", state.MaxMomentum.ToString())
            .Replace("{SUCCESS_THRESHOLD}", state.SuccessThreshold.ToString())
            .Replace("{CURRENT_PRESSURE}", state.Pressure.ToString())
            .Replace("{MAX_PRESSURE}", state.MaxPressure.ToString())
            .Replace("{CURRENT_HEALTH}", state.Health.ToString())
            .Replace("{MAX_HEALTH}", state.MaxHealth.ToString())
            .Replace("{CURRENT_CONFIDENCE}", state.Confidence.ToString())
            .Replace("{MAX_CONFIDENCE}", state.MaxConfidence.ToString())
            .Replace("{CURRENT_CONCENTRATION}", state.Concentration.ToString())
            .Replace("{MAX_CONCENTRATION}", state.MaxConcentration.ToString())
            .Replace("{ENCOUNTER_STAGE}", encounterStage)
            .Replace("{FAVORABLE_APPROACHES}", favorableApproaches)
            .Replace("{DANGEROUS_APPROACHES}", dangerousApproaches)
            .Replace("{ACTIVE_TAGS}", narrativeTagsInfo.ToString())
            .Replace("{INJURIES/RESOURCES/CONDITION}", characterCondition)
            .Replace("{CHARACTER_GOAL}", context.ActionImplementation.Goal)
            .Replace("{CHOICES_INFO}", choicesInfo.ToString());

        return prompt;
    }

    public string BuildEncounterEndPrompt(
        NarrativeContext context,
        EncounterStatusModel finalState,
        EncounterOutcomes outcome,
        IChoice finalChoice,
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

        // Replace placeholders in template
        string prompt = template
            .Replace("{SELECTED_CHOICE}", choiceDescription.ShorthandName)
            .Replace("{CHOICE_DESCRIPTION}", choiceDescription.FullDescription)
            .Replace("{ENCOUNTER_TYPE}", context.EncounterType.ToString())
            .Replace("{ENCOUNTER_OUTCOME}", outcome.ToString())
            .Replace("{LOCATION_NAME}", context.LocationName)
            .Replace("{LOCATION_SPOT}", context.locationSpotName)
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
        EncounterStatusModel newState,
        string oldMemory)
    {
        string template = _promptTemplates[MEMORY_MD];

        NarrativeSummaryBuilder builder = new NarrativeSummaryBuilder();
        string completeHistory = builder.CreateCompleteHistory(context);
        string summary = builder.CreateSummary(context);

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

    private string FormatApproachValues(EncounterStatusModel state)
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

    private string BuildCharacterStatusSummary(EncounterStatusModel state)
    {
        StringBuilder status = new StringBuilder();

        if (state.Health < state.MaxHealth)
            status.Append($"Health: {state.Health}/{state.MaxHealth}. ");

        if (state.Confidence < state.MaxConfidence)
            status.Append($"Confidence: {state.Confidence}/{state.MaxConfidence}. ");

        if (state.Concentration < state.MaxConcentration)
            status.Append($"Concentration: {state.Concentration}/{state.MaxConcentration}. ");

        return status.Length > 0 ? status.ToString() : "In good condition";
    }

    private string FormatFavorableApproaches(EncounterStatusModel state)
    {
        if (state.EncounterInfo?.MomentumBoostApproaches == null || !state.EncounterInfo.MomentumBoostApproaches.Any())
            return "None specifically";

        return string.Join(", ", state.EncounterInfo.MomentumBoostApproaches);
    }

    private string FormatDangerousApproaches(EncounterStatusModel state)
    {
        if (state.EncounterInfo?.DangerousApproaches == null || !state.EncounterInfo.DangerousApproaches.Any())
            return "None specifically";

        return string.Join(", ", state.EncounterInfo.DangerousApproaches);
    }

    private Dictionary<int, string> ExtractChoiceApproaches(List<IChoice> choices)
    {
        Dictionary<int, string> approaches = new Dictionary<int, string>();
        for (int i = 0; i < choices.Count; i++)
        {
            approaches[i + 1] = GetPrimaryApproach(choices[i]).ToString();
        }
        return approaches;
    }

    private Dictionary<int, string> ExtractChoiceFocuses(List<IChoice> choices)
    {
        Dictionary<int, string> focuses = new Dictionary<int, string>();
        for (int i = 0; i < choices.Count; i++)
        {
            focuses[i + 1] = choices[i].Focus.ToString();
        }
        return focuses;
    }
}
