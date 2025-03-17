using System.Text;
using System.Text.Json;

public class PromptManager
{
    private readonly Dictionary<string, string> _promptTemplates;
    private readonly string _systemMessage;

    private const string SYSTEM_KEY = "system_message";
    private const string INTRO_KEY = "introduction_prompt";
    private const string NARRATIVE_KEY = "narrative_prompt";
    private const string CHOICES_KEY = "choices_prompt";
    private const string ENCOUNTER_SOCIAL_KEY = "encounter_style_social";
    private const string ENCOUNTER_INTELLECTUAL_KEY = "encounter_style_intellectual";
    private const string ENCOUNTER_PHYSICAL_KEY = "encounter_style_physical";

    public PromptManager(IConfiguration configuration)
    {
        // Load prompts from JSON files
        string promptsPath = configuration.GetValue<string>("NarrativePromptsPath") ?? "Data/Prompts";

        // Load system message
        _systemMessage = LoadPromptFile(Path.Combine(promptsPath, SYSTEM_KEY + ".json"));

        // Load all prompt templates
        _promptTemplates = new Dictionary<string, string>();
        LoadPromptTemplates(promptsPath);
    }

    private void LoadPromptTemplates(string basePath)
    {
        // Load all JSON files in the prompts directory
        foreach (string filePath in Directory.GetFiles(basePath, "*.json"))
        {
            string key = Path.GetFileNameWithoutExtension(filePath);
            if (key != SYSTEM_KEY) // System message already loaded separately
            {
                _promptTemplates[key] = LoadPromptFile(filePath);
            }
        }
    }

    private string LoadPromptFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Prompt file not found: {filePath}");
        }

        string jsonContent = File.ReadAllText(filePath);
        using (JsonDocument document = JsonDocument.Parse(jsonContent))
        {
            JsonElement root = document.RootElement;
            return root.GetProperty("prompt").GetString() ?? string.Empty;
        }
    }

    public string GetSystemMessage()
    {
        return _systemMessage;
    }

    public string BuildNarrativePrompt(
        NarrativeContext context,
        IChoice chosenOption,
        ChoiceNarrative choiceDescription,
        ChoiceOutcome outcome,
        EncounterStatus newState)
    {
        string template = _promptTemplates[NARRATIVE_KEY];

        // Extract primary approach tag
        EncounterStateTags primaryApproach = GetPrimaryApproach(chosenOption);

        // Format tag changes for narrative representation
        string tagChangesGuidance = FormatTagChangesForNarrative(outcome);

        // Format activated/deactivated tags
        string tagActivationGuidance = FormatTagActivationForNarrative(outcome);

        // Create a narrative summary for context
        string narrativeSummary = CreateSummary(context);

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
        string encounterGoal = context.IncitingAction;

        // Replace placeholders in template
        string prompt = template
            .Replace("{ENCOUNTER_TYPE}", context.EncounterType.ToString())
            .Replace("{LOCATION}", context.LocationName)
            .Replace("{CHARACTER_GOAL}", encounterGoal)
            .Replace("{SELECTED_CHOICE}", chosenOption.Name)
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

    public string BuildChoicesPrompt(
        NarrativeContext context,
        List<IChoice> choices,
        List<ChoiceProjection> projections,
        EncounterStatus state)
    {
        string template = _promptTemplates[CHOICES_KEY];

        // Create a narrative summary for context
        string narrativeSummary = CreateSummary(context);

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

        // Format choices info
        StringBuilder choicesInfo = new StringBuilder();
        for (int i = 0; i < choices.Count; i++)
        {
            IChoice choice = choices[i];
            ChoiceProjection projection = projections[i];

            // Get the primary approach tag
            EncounterStateTags primaryApproach = GetPrimaryApproach(choice);

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

        // Extract encounter goal from inciting action
        string encounterGoal = context.IncitingAction;

        // Replace placeholders in template
        string prompt = template
            .Replace("{LOCATION}", context.LocationName)
            .Replace("{CHARACTER_GOAL}", encounterGoal)
            .Replace("{NARRATIVE_SUMMARY}", narrativeSummary)
            .Replace("{CURRENT_SITUATION}", currentSituation)
            .Replace("{ACTIVE_TAGS}", narrativeTagsInfo.ToString())
            .Replace("{MOMENTUM}", state.Momentum.ToString())
            .Replace("{PRESSURE}", state.Pressure.ToString())
            .Replace("{APPROACH_VALUES}", approachValues)
            .Replace("{LIST_TAGS}", activeTags)
            .Replace("{NPC_LIST}", "relevant individuals in the scene")
            .Replace("{CHOICES_INFO}", choicesInfo.ToString())
            .Replace("{CHOICE_STYLE_GUIDANCE}", choiceStyleGuidance);

        return prompt;
    }

    public string BuildIntroductionPrompt(
        NarrativeContext context,
        string incitingAction,
        EncounterStatus state,
        string encounterGoal = "")
    {
        string template = _promptTemplates[INTRO_KEY];

        // Get primary approach values
        string primaryApproach = state.ApproachTags.OrderByDescending(t => t.Value).First().Key.ToString();
        string secondaryApproach = state.ApproachTags.OrderByDescending(t => t.Value).Skip(1).First().Key.ToString();

        // Get significant focus tags
        string primaryFocus = state.FocusTags.OrderByDescending(t => t.Value).First().Key.ToString();
        string secondaryFocus = state.FocusTags.OrderByDescending(t => t.Value).Skip(1).First().Key.ToString();

        // Format environment and NPC details
        string environmentDetails = $"A {context.LocationName.ToLower()} with difficulty level {state.Location?.Difficulty ?? 1}";
        string npcList = "Local individuals relevant to the encounter";
        string timeConstraints = $"Maximum {state.MaxTurns} turns";
        string additionalChallenges = state.Location != null
            ? $"Difficulty level {state.Location.Difficulty} (adds +{state.Location.Difficulty} pressure per turn)"
            : "Standard difficulty";

        // Format character archetype based on primary approach
        string characterArchetype = GetCharacterArchetype(primaryApproach);

        // Format approach stats
        string approachStats = FormatApproachValues(state);

        // If encounterGoal is empty, use incitingAction
        if (string.IsNullOrEmpty(encounterGoal))
        {
            encounterGoal = incitingAction;
        }

        // Replace placeholders in template
        string prompt = template
            .Replace("{ENCOUNTER_TYPE}", context.EncounterType.ToString())
            .Replace("{LOCATION_NAME}", context.LocationName)
            .Replace("{CHARACTER_ARCHETYPE}", characterArchetype)
            .Replace("{APPROACH_STATS}", approachStats)
            .Replace("{CHARACTER_GOAL}", encounterGoal)
            .Replace("{ENVIRONMENT_DETAILS}", environmentDetails)
            .Replace("{NPC_LIST}", npcList)
            .Replace("{TIME_CONSTRAINTS}", timeConstraints)
            .Replace("{ADDITIONAL_CHALLENGES}", additionalChallenges);

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
            foreach (KeyValuePair<EncounterStateTags, int> change in outcome.EncounterStateTagChanges)
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

    private string FormatApproachChanges(Dictionary<EncounterStateTags, int> approachChanges)
    {
        if (approachChanges == null || !approachChanges.Any())
            return "No significant approach changes";

        StringBuilder builder = new StringBuilder();
        foreach (KeyValuePair<EncounterStateTags, int> change in approachChanges)
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
        foreach (KeyValuePair<EncounterStateTags, int> approach in state.ApproachTags)
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
            "Concealment" => "Thief",
            _ => "Traveler"
        };
    }

    private string GetEncounterStyleGuidance(EncounterTypes type)
    {
        string key = type switch
        {
            EncounterTypes.Social => ENCOUNTER_SOCIAL_KEY,
            EncounterTypes.Intellectual => ENCOUNTER_INTELLECTUAL_KEY,
            EncounterTypes.Physical => ENCOUNTER_PHYSICAL_KEY,
            _ => ENCOUNTER_SOCIAL_KEY
        };

        return _promptTemplates.TryGetValue(key, out string guidance)
            ? guidance
            : "Focus on immediate practical situation using sensory details appropriate to encounter type";
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

    private EncounterStateTags GetPrimaryApproach(IChoice choice)
    {
        // Find the approach tag with the largest modification
        List<TagModification> approachMods = choice.TagModifications
            .Where(m => m.Type == TagModification.TagTypes.EncounterState)
            .Where(m => IsApproachTag((EncounterStateTags)m.Tag))
            .OrderByDescending(m => m.Delta)
            .ToList();

        if (approachMods.Any())
        {
            return (EncounterStateTags)approachMods.First().Tag;
        }

        // Default to Analysis if no approach is found (fallback)
        return EncounterStateTags.Analysis;
    }

    private bool IsApproachTag(EncounterStateTags tag)
    {
        return tag == EncounterStateTags.Dominance ||
               tag == EncounterStateTags.Rapport ||
               tag == EncounterStateTags.Analysis ||
               tag == EncounterStateTags.Precision ||
               tag == EncounterStateTags.Concealment;
    }

    private string CreateSummary(NarrativeContext context)
    {
        // Basic summary from the context's events
        if (context.Events.Count == 0)
        {
            return $"Beginning a new encounter at {context.LocationName} after {context.IncitingAction}.";
        }

        // Get the most recent events (up to 3)
        int eventCount = Math.Min(3, context.Events.Count);
        List<string> recentEvents = new List<string>();

        for (int i = context.Events.Count - eventCount; i < context.Events.Count; i++)
        {
            NarrativeEvent evt = context.Events[i];
            recentEvents.Add($"Turn {evt.TurnNumber}: {SummarizeEvent(evt)}");
        }

        return string.Join("\n", recentEvents);
    }

    private string SummarizeEvent(NarrativeEvent evt)
    {
        // Extract key points from the event
        if (evt.ChosenOption != null)
        {
            return $"Used {evt.ChosenOption.Name}. {evt.Outcome}";
        }
        else
        {
            // For events without a chosen option (like the intro)
            return evt.SceneDescription.Length > 100
                ? evt.SceneDescription.Substring(0, 100) + "..."
                : evt.SceneDescription;
        }
    }

    private string GetApproachChangeDescription(EncounterStateTags approach, bool isPositive)
    {
        return (approach, isPositive) switch
        {
            (EncounterStateTags.Dominance, true) => "increased authority, commanding presence, or intimidation factor",
            (EncounterStateTags.Dominance, false) => "diminished authority, lessened presence, or reduced intimidation factor",
            (EncounterStateTags.Rapport, true) => "improved charm, likeability, or social influence",
            (EncounterStateTags.Rapport, false) => "reduced charm, awkwardness, or social disconnection",
            (EncounterStateTags.Analysis, true) => "sharper observation, clearer thinking, or deeper understanding",
            (EncounterStateTags.Analysis, false) => "confusion, overlooking details, or failing to make connections",
            (EncounterStateTags.Precision, true) => "improved accuracy, careful execution, or greater control",
            (EncounterStateTags.Precision, false) => "clumsiness, carelessness, or lack of control",
            (EncounterStateTags.Concealment, true) => "better stealth, secrecy, or ability to hide intentions",
            (EncounterStateTags.Concealment, false) => "exposure, visibility, or inability to hide",
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
}