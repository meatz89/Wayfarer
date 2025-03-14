using System.Text;
using System.Text.Json;

public class PromptManager
{
    private readonly TagFormatter _tagFormatter;
    private readonly NarrativeSummaryBuilder _summaryBuilder;
    private readonly EncounterTypeDetector _encounterDetector;
    private readonly Dictionary<string, string> _promptTemplates;
    private readonly string _systemMessage;

    private const string SYSTEM_KEY = "system_message";
    private const string INTRO_KEY = "introduction_prompt";
    private const string ACTION_OUTCOME_KEY = "action_outcome_prompt";
    private const string NEW_SITUATION_KEY = "new_situation_prompt";
    private const string CHOICES_KEY = "choices_prompt";
    private const string ENCOUNTER_SOCIAL_KEY = "encounter_style_social";
    private const string ENCOUNTER_INTELLECTUAL_KEY = "encounter_style_intellectual";
    private const string ENCOUNTER_PHYSICAL_KEY = "encounter_style_physical";
    private const string SITUATION_SOCIAL_KEY = "situation_style_social";
    private const string SITUATION_INTELLECTUAL_KEY = "situation_style_intellectual";
    private const string SITUATION_PHYSICAL_KEY = "situation_style_physical";
    private const string CHOICE_SOCIAL_KEY = "choice_style_social";
    private const string CHOICE_INTELLECTUAL_KEY = "choice_style_intellectual";
    private const string CHOICE_PHYSICAL_KEY = "choice_style_physical";

    public PromptManager(IConfiguration configuration)
    {
        _tagFormatter = new TagFormatter();
        _summaryBuilder = new NarrativeSummaryBuilder();
        _encounterDetector = new EncounterTypeDetector();

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

    public string BuildChoicesPrompt(
        NarrativeContext context,
        List<IChoice> choices,
        List<ChoiceProjection> projections,
        EncounterStatus state)
    {
        if (!_promptTemplates.TryGetValue(CHOICES_KEY, out string template))
        {
            throw new InvalidOperationException($"Choices prompt template not found");
        }

        // Determine encounter type and get appropriate style guidance
        EncounterTypes encounterType = _encounterDetector.DetermineEncounterType(context.LocationName, state);
        string choiceStyleGuidance = GetChoiceStyleGuidance(encounterType);

        // Create a narrative summary and get most recent reaction
        string narrativeSummary = _summaryBuilder.CreateSummary(context);
        string mostRecentReaction = context.GetLastScene() ?? "The situation begins";

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
        }

        // Replace placeholders in template
        string prompt = template
            .Replace("{LOCATION}", context.LocationName)
            .Replace("{NARRATIVE_SUMMARY}", narrativeSummary)
            .Replace("{MOST_RECENT_REACTION}", mostRecentReaction)
            .Replace("{CHOICES_INFO}", choicesInfo.ToString())
            .Replace("{CHOICE_STYLE_GUIDANCE}", choiceStyleGuidance);

        return prompt;
    }

    // New method for action outcome prompt
    public string BuildActionOutcomePrompt(
        NarrativeContext context,
        IChoice chosenOption,
        ChoiceNarrative choiceDescription,
        ChoiceOutcome outcome,
        EncounterStatus newState)
    {
        if (!_promptTemplates.TryGetValue(ACTION_OUTCOME_KEY, out string template))
        {
            throw new InvalidOperationException($"Action outcome prompt template not found");
        }

        // Determine encounter type
        EncounterTypes encounterType = _encounterDetector.DetermineEncounterType(context.LocationName, newState);
        string encounterStyleGuidance = GetEncounterStyleGuidance(encounterType);

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
            .Replace("{CONCENTRATION_CHANGE}", outcome.FocusChange.ToString())
            .Replace("{REPUTATION_CHANGE}", outcome.ConfidenceChange.ToString())
            .Replace("{NARRATIVE_SUMMARY}", narrativeSummary)
            .Replace("{ENCOUNTER_TYPE}", encounterType.ToString())
            .Replace("{ENCOUNTER_STYLE_GUIDANCE}", encounterStyleGuidance);

        return prompt;
    }

    // New method for new situation prompt
    public string BuildNewSituationPrompt(
        NarrativeContext context,
        EncounterStatus state,
        string recentOutcome)
    {
        if (!_promptTemplates.TryGetValue(NEW_SITUATION_KEY, out string template))
        {
            throw new InvalidOperationException($"New situation prompt template not found");
        }

        // Determine encounter type
        EncounterTypes encounterType = _encounterDetector.DetermineEncounterType(context.LocationName, state);
        string situationStyleGuidance = GetSituationStyleGuidance(encounterType);

        // Create a narrative summary
        string narrativeSummary = _summaryBuilder.CreateSummary(context);

        // Replace placeholders in template
        string prompt = template
            .Replace("{LOCATION}", context.LocationName)
            .Replace("{CURRENT_MOMENTUM}", state.Momentum.ToString())
            .Replace("{CURRENT_PRESSURE}", state.Pressure.ToString())
            .Replace("{RECENT_OUTCOME}", recentOutcome)
            .Replace("{SIGNIFICANT_TAGS}", _tagFormatter.GetSignificantTagsFormatted(state))
            .Replace("{NARRATIVE_SUMMARY}", narrativeSummary)
            .Replace("{ENCOUNTER_TYPE}", encounterType.ToString())
            .Replace("{SITUATION_STYLE_GUIDANCE}", situationStyleGuidance);

        return prompt;
    }

    public string BuildIntroductionPrompt(string location, string incitingAction, EncounterStatus state, string encounterGoal = "")
    {
        if (!_promptTemplates.TryGetValue(INTRO_KEY, out string template))
        {
            throw new InvalidOperationException($"Introduction prompt template not found");
        }

        EncounterTypes encounterType = _encounterDetector.DetermineEncounterType(location, state);

        // Get primary and secondary tags for initial emphasis
        (string primaryApproach, string secondaryApproach) = _tagFormatter.GetSignificantApproachTags(state);
        (string primaryFocus, string secondaryFocus) = _tagFormatter.GetSignificantFocusTags(state);

        // Replace placeholders in template
        string prompt = template
            .Replace("{LOCATION}", location)
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

    private string GetSituationStyleGuidance(EncounterTypes type)
    {
        string key = type switch
        {
            EncounterTypes.Social => SITUATION_SOCIAL_KEY,
            EncounterTypes.Intellectual => SITUATION_INTELLECTUAL_KEY,
            EncounterTypes.Physical => SITUATION_PHYSICAL_KEY,
            _ => SITUATION_SOCIAL_KEY
        };

        return _promptTemplates.TryGetValue(key, out string guidance)
            ? guidance
            : "Concrete, observable details in your immediate surroundings";
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