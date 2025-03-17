using System.Text;
using System.Text.Json;

public class PromptManager
{
    private readonly TagFormatter _tagFormatter;
    private readonly NarrativeSummaryBuilder _summaryBuilder;
    private readonly Dictionary<string, string> _promptTemplates;
    private readonly string _systemMessage;

    private const string SYSTEM_KEY = "system_message";
    private const string INTRO_KEY = "introduction_prompt";
    private const string JSON_NARRATIVE_KEY = "json_narrative_prompt";
    private const string JSON_CHOICES_KEY = "json_choices_prompt";
    private const string ENCOUNTER_SOCIAL_KEY = "encounter_style_social";
    private const string ENCOUNTER_INTELLECTUAL_KEY = "encounter_style_intellectual";
    private const string ENCOUNTER_PHYSICAL_KEY = "encounter_style_physical";
    private const string CHOICE_SOCIAL_KEY = "choice_style_social";
    private const string CHOICE_INTELLECTUAL_KEY = "choice_style_intellectual";
    private const string CHOICE_PHYSICAL_KEY = "choice_style_physical";

    public PromptManager(IConfiguration configuration)
    {
        _tagFormatter = new TagFormatter();
        _summaryBuilder = new NarrativeSummaryBuilder();

        // Load prompts from JSON files
        string promptsPath = configuration.GetValue<string>("NarrativePromptsPath") ?? "Data/Prompts";

        // Load system message
        _systemMessage = LoadPromptFile(Path.Combine(promptsPath, "system_message.json"));

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
            if (key != "system_message") // System message already loaded separately
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

    private string FormatTagChangesForNarrative(ChoiceOutcome outcome)
    {
        StringBuilder guidance = new StringBuilder();

        // Add significant focus tag changes
        foreach (KeyValuePair<FocusTags, int> change in outcome.FocusTagChanges)
        {
            if (Math.Abs(change.Value) >= 2)
            {
                string direction = change.Value > 0 ? "+" : "";
                guidance.AppendLine($"- {change.Key} {direction}{change.Value}: Show this as {GetFocusTagChangeDescription(change.Key, change.Value > 0)}");
            }
        }

        // Add significant encounter state tag changes
        foreach (KeyValuePair<ApproachTags, int> change in outcome.EncounterStateTagChanges)
        {
            if (Math.Abs(change.Value) >= 2)
            {
                string direction = change.Value > 0 ? "+" : "";
                guidance.AppendLine($"- {change.Key} {direction}{change.Value}: Show this as {GetEncounterStateTagChangeDescription(change.Key, change.Value > 0)}");
            }
        }

        // Add resource changes that need to be represented
        if (outcome.HealthChange <= -3)
        {
            guidance.AppendLine($"- Health {outcome.HealthChange}: Describe specific injuries, pain, or physical limitations");
        }

        if (outcome.ConfidenceChange <= -3)
        {
            guidance.AppendLine($"- Confidence {outcome.ConfidenceChange}: Show how people's attitudes have visibly changed");
        }

        if (outcome.ConcentrationChange <= -3)
        {
            guidance.AppendLine($"- Concentration {outcome.ConcentrationChange}: Indicate mental fatigue, confusion, or distraction");
        }

        return guidance.ToString();
    }

    private string FormatTagActivationForNarrative(ChoiceOutcome outcome)
    {
        StringBuilder guidance = new StringBuilder();

        // Add newly activated tags
        if (outcome.NewlyActivatedTags.Count > 0)
        {
            guidance.AppendLine("## NEWLY ACTIVATED TAGS");
            foreach (string tag in outcome.NewlyActivatedTags)
            {
                guidance.AppendLine($"- {tag}: Introduce narrative elements that reflect this new tag's activation");
            }
        }

        // Add deactivated tags
        if (outcome.DeactivatedTags.Count > 0)
        {
            guidance.AppendLine("## DEACTIVATED TAGS");
            foreach (string tag in outcome.DeactivatedTags)
            {
                guidance.AppendLine($"- {tag}: Show how this aspect has diminished or is no longer relevant");
            }
        }

        return guidance.ToString();
    }

    private string GetFocusTagChangeDescription(FocusTags tag, bool isPositive)
    {
        return (tag, isPositive) switch
        {
            (FocusTags.Relationship, true) => "improved social connections, trust building, or mutual understanding",
            (FocusTags.Relationship, false) => "damaged relationships, distrust, or social isolation",
            (FocusTags.Information, true) => "gained knowledge, clearer understanding, or valuable insights",
            (FocusTags.Information, false) => "misinformation, confusion, or loss of critical details",
            (FocusTags.Physical, true) => "better bodily awareness, physical control, or direct interaction",
            (FocusTags.Physical, false) => "reduced physical capabilities, awkwardness, or loss of control",
            (FocusTags.Environment, true) => "better awareness of surroundings, tactical advantage, or control of space",
            (FocusTags.Environment, false) => "environmental disadvantage, poor positioning, or spatial disorientation",
            (FocusTags.Resource, true) => "acquired valuables, better equipment, or resource advantage",
            (FocusTags.Resource, false) => "depleted supplies, lost items, or resource disadvantage",
            _ => "notable change in focus"
        };
    }

    private string GetEncounterStateTagChangeDescription(ApproachTags tag, bool isPositive)
    {
        return (tag, isPositive) switch
        {
            (ApproachTags.Dominance, true) => "increased authority, command, or intimidation factor",
            (ApproachTags.Dominance, false) => "diminished influence, weakened position, or reduced credibility",
            (ApproachTags.Rapport, true) => "improved social connection, trust, or relationship building",
            (ApproachTags.Rapport, false) => "damaged relationships, distrust, or social distance",
            (ApproachTags.Analysis, true) => "enhanced understanding, insights gained, or clarity of thought",
            (ApproachTags.Analysis, false) => "confusion, misunderstanding, or incomplete information",
            (ApproachTags.Precision, true) => "refined control, careful movement, or improved accuracy",
            (ApproachTags.Precision, false) => "clumsiness, imprecision, or reduced physical control",
            (ApproachTags.Concealment, true) => "improved stealth, deeper shadows, or reduced visibility",
            (ApproachTags.Concealment, false) => "increased exposure, visibility, or attention drawn",
            _ => "notable change in encounter state"
        };
    }

    public string BuildNarrativePrompt(
        NarrativeContext context,
        IChoice chosenOption,
        ChoiceNarrative choiceDescription,
        ChoiceOutcome outcome,
        EncounterStatus newState)
    {
        string template = _promptTemplates[JSON_NARRATIVE_KEY];

        EncounterTypes encounterType = context.EncounterType;
        string encounterStyleGuidance = GetEncounterStyleGuidance(encounterType);

        // Format tag changes to ensure they're represented in the narrative
        string tagChangesGuidance = FormatTagChangesForNarrative(outcome);

        // Format newly activated and deactivated tags
        string tagActivationGuidance = FormatTagActivationForNarrative(outcome);

        // Create a narrative summary
        string narrativeSummary = _summaryBuilder.CreateSummary(context);

        // Replace placeholders in template
        string prompt = template
            .Replace("{LOCATION}", context.LocationName)
            .Replace("{CHOICE_NAME}", chosenOption.Name)
            .Replace("{CHOICE_APPROACH}", chosenOption.Approach.ToString())
            .Replace("{CHOICE_FOCUS}", chosenOption.Focus.ToString())
            .Replace("{CHOICE_DESCRIPTION}", choiceDescription.FullDescription)
            .Replace("{MOMENTUM_GAIN}", outcome.MomentumGain.ToString())
            .Replace("{PRESSURE_GAIN}", outcome.PressureGain.ToString())
            .Replace("{HEALTH_CHANGE}", outcome.HealthChange.ToString())
            .Replace("{CONCENTRATION_CHANGE}", outcome.ConcentrationChange.ToString())
            .Replace("{REPUTATION_CHANGE}", outcome.ConfidenceChange.ToString())
            .Replace("{NARRATIVE_SUMMARY}", narrativeSummary)
            .Replace("{TAG_CHANGES_GUIDANCE}", tagChangesGuidance)
            .Replace("{TAG_ACTIVATION_GUIDANCE}", tagActivationGuidance)
            .Replace("{ENCOUNTER_TYPE}", encounterType.ToString())
            .Replace("{ENCOUNTER_STYLE_GUIDANCE}", encounterStyleGuidance);

        return prompt;
    }

    public string BuildChoicesPrompt(
        NarrativeContext context,
        List<IChoice> choices,
        List<ChoiceProjection> projections,
        EncounterStatus state)
    {
        string template = _promptTemplates[JSON_CHOICES_KEY];

        EncounterTypes encounterType = context.EncounterType;
        string choiceStyleGuidance = GetChoiceStyleGuidance(encounterType);

        // Create a narrative summary 
        string narrativeSummary = _summaryBuilder.CreateSummary(context);

        // Get the last narrative event to ensure continuity
        string mostRecentNarrative = "No previous narrative available.";

        // Extract the most recent narrative from context
        if (context.Events.Count > 0)
        {
            NarrativeEvent lastEvent = context.Events[context.Events.Count - 1];
            mostRecentNarrative = lastEvent.SceneDescription;
        }

        // Format choices info
        StringBuilder choicesInfo = new StringBuilder();
        for (int i = 0; i < choices.Count; i++)
        {
            IChoice choice = choices[i];
            ChoiceProjection projection = projections[i];

            string approachDesc = TagCharacteristicsProvider.GetApproachCharacteristics(choice.Approach.ToString());
            string focusDesc = TagCharacteristicsProvider.GetFocusCharacteristics(choice.Focus.ToString());
            string effectDesc = choice.EffectType == EffectTypes.Momentum ?
                $"MOMENTUM +{projection.MomentumGained}" :
                $"PRESSURE +{projection.PressureBuilt}";

            choicesInfo.AppendLine($@"
Choice {i + 1}: 
- Approach: {choice.Approach} ({approachDesc})
- Focus: {choice.Focus} ({focusDesc})
- Effect: {effectDesc}
- Key Changes: {_tagFormatter.FormatKeyTagChanges(projection)}");

            // Add resource changes if present
            if (projection.HealthChange != 0)
            {
                choicesInfo.AppendLine($"- Health Change: {projection.HealthChange}");
            }
            if (projection.ConcentrationChange != 0)
            {
                choicesInfo.AppendLine($"- Concnetration Change: {projection.ConcentrationChange}");
            }
            if (projection.ConfidenceChange != 0)
            {
                choicesInfo.AppendLine($"- Confidence Change: {projection.ConfidenceChange}");
            }
        }

        // Replace placeholders in template
        string prompt = template
            .Replace("{LOCATION}", context.LocationName)
            .Replace("{NARRATIVE_SUMMARY}", narrativeSummary)
            .Replace("{MOST_RECENT_REACTION}", mostRecentNarrative)
            .Replace("{CHOICES_INFO}", choicesInfo.ToString())
            .Replace("{CHOICE_STYLE_GUIDANCE}", choiceStyleGuidance);

        return prompt;
    }

    public string BuildIntroductionPrompt(NarrativeContext context, string incitingAction, EncounterStatus state, string encounterGoal = "")
    {
        string template = _promptTemplates[INTRO_KEY];

        EncounterTypes encounterType = context.EncounterType;

        // Get primary and secondary tags for initial emphasis
        (string primaryApproach, string secondaryApproach) = _tagFormatter.GetSignificantApproachTags(state);
        (string primaryFocus, string secondaryFocus) = _tagFormatter.GetSignificantFocusTags(state);

        // Replace placeholders in template
        string prompt = template
            .Replace("{LOCATION}", context.LocationName)
            .Replace("{INCITING_ACTION}", incitingAction)
            .Replace("{ENCOUNTER_TYPE}", encounterType.ToString())
            .Replace("{PRIMARY_APPROACH}", primaryApproach)
            .Replace("{SECONDARY_APPROACH}", secondaryApproach)
            .Replace("{PRIMARY_FOCUS}", primaryFocus)
            .Replace("{SECONDARY_FOCUS}", secondaryFocus)
            .Replace("{CHARACTER_OR_ENVIRONMENT_FOCUS}", (encounterType == EncounterTypes.Social ? "CHARACTER" : "ENVIRONMENT"))
            .Replace("{OPPOSITE_FOCUS}", (encounterType == EncounterTypes.Social ? "the environment" : "social interaction"))
            .Replace("{ENCOUNTER_GOAL}", encounterGoal);

        return prompt;
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
            : "Practical description focusing on immediate situation";
    }

    private string GetChoiceStyleGuidance(EncounterTypes type)
    {
        string key = type switch
        {
            EncounterTypes.Social => CHOICE_SOCIAL_KEY,
            EncounterTypes.Intellectual => CHOICE_INTELLECTUAL_KEY,
            EncounterTypes.Physical => CHOICE_PHYSICAL_KEY,
            _ => CHOICE_SOCIAL_KEY
        };

        return _promptTemplates.TryGetValue(key, out string guidance)
            ? guidance
            : "Specific action I would take in this situation";
    }
}