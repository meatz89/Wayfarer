// Main service class - slim coordinator
using BlazorRPG.Game.EncounterManager;
using BlazorRPG.Game.EncounterManager.NarrativeAi;
using System.Text;
using System.Text.Json;

// Loads and manages prompts from JSON files
public class PromptManager
{
    private readonly TagFormatter _tagFormatter;
    private readonly NarrativeSummaryBuilder _summaryBuilder;
    private readonly EncounterTypeDetector _encounterDetector;
    private readonly Dictionary<string, string> _promptTemplates;
    private readonly string _systemMessage;

    private const string SYSTEM_KEY = "system_message";
    private const string INTRO_KEY = "introduction_prompt";
    private const string REACTION_KEY = "reaction_prompt";
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
    private const string CHOICE_REQUIREMENTS_KEY = "choice_requirements";

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

    // Simplified BuildChoicesPrompt method
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

        if (!_promptTemplates.TryGetValue(CHOICE_REQUIREMENTS_KEY, out string choiceRequirements))
        {
            throw new InvalidOperationException($"Choice requirements template not found");
        }

        EncounterTypes encounterType = _encounterDetector.DetermineEncounterType(context.LocationName, state);
        string choiceStyleGuidance = GetChoiceStyleGuidance(encounterType);

        // Create a concise narrative summary
        string narrativeSummary = _summaryBuilder.CreateSummary(context);

        // Get only significant tags and active narrative tags with effects
        string significantTags = _tagFormatter.GetSignificantTagsFormatted(state);
        string activeNarrativeTags = _tagFormatter.FormatActiveNarrativeTags(state);

        // Create encounter goal based on location
        string encounterGoal = "";

        StringBuilder choicesInfo = new StringBuilder();

        // Add each choice with simplified mechanical properties
        for (int i = 0; i < choices.Count; i++)
        {
            IChoice choice = choices[i];
            ChoiceProjection projection = projections[i];

            choicesInfo.AppendLine($@"
Choice {i + 1}: {choice.Name}
- Approach: {choice.Approach} ({TagCharacteristicsProvider.GetApproachCharacteristics(choice.Approach.ToString())})
- Focus: {choice.Focus} ({TagCharacteristicsProvider.GetFocusCharacteristics(choice.Focus.ToString())})
- Effect: {(choice.EffectType == EffectTypes.Momentum ? $"Progress +{projection.MomentumGained}" : $"Risk +{projection.PressureBuilt}")}"
            );
        }

        // Replace placeholders in template
        string prompt = template
            .Replace("{LOCATION}", context.LocationName)
            .Replace("{ENCOUNTER_GOAL}", encounterGoal)
            .Replace("{NARRATIVE_SUMMARY}", narrativeSummary)
            .Replace("{SIGNIFICANT_TAGS}", significantTags)
            .Replace("{ACTIVE_NARRATIVE_TAGS}", activeNarrativeTags)
            .Replace("{CHOICE_STYLE_GUIDANCE}", choiceStyleGuidance)
            .Replace("{CHOICES_INFO}", choicesInfo.ToString())
            .Replace("{CHOICE_REQUIREMENTS}", choiceRequirements);

        // Always include system message
        return _systemMessage + "\n\n" + prompt;
    }

    public string BuildReactionPrompt(
    NarrativeContext context,
    IChoice chosenOption,
    ChoiceNarrative choiceDescription,
    ChoiceOutcome outcome,
    EncounterStatus newState)
    {
        if (!_promptTemplates.TryGetValue(REACTION_KEY, out string template))
        {
            throw new InvalidOperationException($"Reaction prompt template not found");
        }

        EncounterTypes encounterType = _encounterDetector.DetermineEncounterType(context.LocationName, newState);
        string encounterStyleGuidance = GetEncounterStyleGuidance(encounterType);
        string situationStyleGuidance = GetSituationStyleGuidance(encounterType);

        // Create a concise narrative summary
        string narrativeSummary = _summaryBuilder.CreateSummary(context);

        // Format tag modifications
        string approachTagsModified = _tagFormatter.FormatTagModifications(outcome.ApproachTagChanges);
        string focusTagsModified = _tagFormatter.FormatTagModifications(outcome.FocusTagChanges);
        string encounterStateTagsModified = _tagFormatter.FormatTagModifications(outcome.EncounterStateTagChanges);

        // Format newly activated tags
        string newlyActivatedTags = string.Join(", ", outcome.NewlyActivatedTags);
        string deactivatedTags = string.Join(", ", outcome.DeactivatedTags);

        // Create encounter goal
        string encounterGoal = "";

        // Get resource changes directly from the outcome
        string healthChange = outcome.HealthChange.ToString();
        string concentrationChange = outcome.ConcentrationChange.ToString();
        string reputationChange = outcome.ReputationChange.ToString();

        // Replace placeholders in template
        string prompt = template
            .Replace("{LOCATION}", context.LocationName)
            .Replace("{ENCOUNTER_GOAL}", encounterGoal)
            .Replace("{CHOICE_NAME}", chosenOption.Name)
            .Replace("{CHOICE_APPROACH}", chosenOption.Approach.ToString())
            .Replace("{CHOICE_FOCUS}", chosenOption.Focus.ToString())
            .Replace("{CHOICE_DESCRIPTION}", choiceDescription.FullDescription)
            .Replace("{MOMENTUM_GAIN}", outcome.MomentumGain.ToString())
            .Replace("{PRESSURE_GAIN}", outcome.PressureGain.ToString())
            .Replace("{CURRENT_MOMENTUM}", newState.Momentum.ToString())
            .Replace("{CURRENT_PRESSURE}", newState.Pressure.ToString())
            .Replace("{HEALTH_CHANGE}", healthChange)
            .Replace("{CONCENTRATION_CHANGE}", concentrationChange)
            .Replace("{REPUTATION_CHANGE}", reputationChange)
            .Replace("{APPROACH_TAGS_MODIFIED}", approachTagsModified)
            .Replace("{FOCUS_TAGS_MODIFIED}", focusTagsModified)
            .Replace("{ENCOUNTER_STATE_TAGS_MODIFIED}", encounterStateTagsModified)
            .Replace("{NEWLY_ACTIVATED_TAGS}", newlyActivatedTags)
            .Replace("{DEACTIVATED_TAGS}", deactivatedTags)
            .Replace("{SIGNIFICANT_TAGS}", _tagFormatter.GetSignificantTagsFormatted(newState))
            .Replace("{NARRATIVE_SUMMARY}", narrativeSummary)
            .Replace("{ENCOUNTER_STYLE_GUIDANCE}", encounterStyleGuidance)
            .Replace("{SITUATION_STYLE_GUIDANCE}", situationStyleGuidance);

        // Always include system message
        return _systemMessage + "\n\n" + prompt;
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

        // Always include system message
        return _systemMessage + "\n\n" + prompt;
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