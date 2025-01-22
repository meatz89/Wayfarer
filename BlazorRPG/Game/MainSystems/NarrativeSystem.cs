public class NarrativeSystem
{
    private List<LocationNarrative> narrativeContents;
    private string openAiApiKey;
    private const string NewLine = "\r\n";

    public NarrativeSystem(
        GameContentProvider gameContentProvider,
        JournalSystem journalSystem,
        IConfiguration configuration
        )
    {
        narrativeContents = new List<LocationNarrative>();
        narrativeContents = gameContentProvider.GetNarratives();
        JournalSystem = journalSystem;
        openAiApiKey = configuration.GetValue<string>("OpenAiApiKey");
        LargeLanguageAdapter = new LargeLanguageAdapter(openAiApiKey);
    }

    public LargeLanguageAdapter LargeLanguageAdapter { get; }
    public JournalSystem JournalSystem { get; }

    public void NewEncounter(EncounterContext context, ActionImplementation actionImplementation)
    {
        LargeLanguageAdapter.Reset();

        string initialSituation = $"The player is in the {context.LocationName}, a {context.LocationArchetype}, at the {context.LocationSpotName}. It is {context.TimeSlot.ToString()}. The player starts a {context.ActionType.ToString()} action.";
        string actionGoal = $"{actionImplementation.Name}";

        JournalSystem.StartEncounter(initialSituation, actionGoal);
    }

    public ChoicesNarrativeResponse GetChoicesNarrative(EncounterContext context, List<EncounterChoice> choices)
    {
        string encounterState = "Encounter State:" + GetEncounterState(context);
        string initialGoal = JournalSystem.GetCurrentEncounterGoal();

        string prompt = $"" + NewLine;
        prompt += $"Do NOT stray away to far from the initial goal of the encounter: '{initialGoal}' {NewLine}{NewLine}";

        prompt += $"{encounterState}{NewLine}{NewLine}";
        prompt = AddChoicesToPrompt(choices, prompt);

        List<CompletionMessage4o> previousPrompts = GetPreviousPrompts();

        CompletionMessage4o choicesPrompt = OpenAiHelper.CreateCompletionMessage(Roles.user, prompt);
        ChoicesNarrativeResponse choicesNarrativeResponse = LargeLanguageAdapter.NextEncounterChoices(previousPrompts, choicesPrompt);

        JournalSystem.NoteNewEncounterAssistantNarrative(choicesNarrativeResponse.introductory_narrative);
        return choicesNarrativeResponse;
    }

    public string GetEncounterSuccessNarrative(EncounterContext context)
    {
        string encounterState = "Encounter State:" + GetEncounterState(context);

        string prompt = $"The last player decision ended the encounter successfully. " +
            $"Wrap up the encounter narrative.{NewLine}{NewLine}";
        prompt += $"{encounterState}{NewLine}{NewLine}";

        List<CompletionMessage4o> previousPrompts = GetPreviousPrompts();

        CompletionMessage4o choicesPrompt = OpenAiHelper.CreateCompletionMessage(Roles.user, prompt);
        string encounterEndNarrative = LargeLanguageAdapter.EncounterEndNarrative(previousPrompts, choicesPrompt);

        JournalSystem.EndEncounter(encounterEndNarrative);

        return encounterEndNarrative;
    }

    private List<CompletionMessage4o> GetPreviousPrompts()
    {
        List<CompletionMessage4o> previousPrompts = new();
        List<string> initialDescriptions = JournalSystem.GetInitialEncounterDescriptions();
        List<Narrative> encounterNarratives = JournalSystem.GetDescriptionForCurrentEncounter();
        foreach (string description in initialDescriptions)
        {
            CompletionMessage4o previousPrompt =
                OpenAiHelper.CreateCompletionMessage(Roles.user, description);
            previousPrompts.Add(previousPrompt);
        }
        foreach (Narrative narrative in encounterNarratives)
        {
            CompletionMessage4o previousPrompt =
                OpenAiHelper.CreateCompletionMessage(narrative.Role, narrative.Text);
            previousPrompts.Add(previousPrompt);
        }
        return previousPrompts;
    }


    public void MakeChoice(EncounterContext context, EncounterChoice encounterChoice)
    {
        string choice =
            $"{encounterChoice.Designation} " +
            $"('{encounterChoice.Narrative}'){NewLine}" +
            $"This is a {encounterChoice.Archetype.ToString().ToUpper()} choice{NewLine}" +
            $"This is a {encounterChoice.Approach.ToString().ToUpper()} approach";

        string prompt = $"The player chooses: {choice}";
        JournalSystem.NoteNewEncounterNarrative(prompt);
    }

    private static string AddChoicesToPrompt(List<EncounterChoice> choices, string prompt)
    {
        int i = 1;
        prompt += $"Create the narrative descriptions for the new choice options:{NewLine}{NewLine}";
        foreach (EncounterChoice choice in choices)
        {
            prompt += CreatePromptForChoice(i, choice);
            i++;
        }

        return prompt;
    }

    private static string CreatePromptForChoice(int index, EncounterChoice encounterChoice)
    {
        string prompt = $"{index}. ({encounterChoice.Archetype} - {encounterChoice.Approach})";
        string effects = $": ";

        List<BaseValueChange> baseValueChanges = encounterChoice.CalculationResult.BaseValueChanges;
        List<ValueModification> valueModifications = encounterChoice.CalculationResult.ValueModifications;

        foreach (BaseValueChange baseValueChange in baseValueChanges)
        {
            effects += $"{baseValueChange.Amount} to {baseValueChange.ValueType}; ";
        }

        foreach (ValueModification valueModification in valueModifications)
        {
            if (valueModification is EncounterValueModification encounterValueMod)
                effects += $"{encounterValueMod.Amount} to {encounterValueMod.ValueType}; ";

            if (valueModification is EnergyCostReduction energyCostReduction)
                effects += $"{energyCostReduction.Amount} to {energyCostReduction.EnergyType}; ";
        }

        prompt = prompt + effects + NewLine;
        return prompt;
    }

    public string GetLocationNarrative(LocationNames locationName)
    {
        LocationNarrative locationNarrative = narrativeContents
            .Where(x => x.LocationName == locationName)
            .FirstOrDefault();

        return locationNarrative.Description;
    }

    private static string GetEncounterState(EncounterContext context)
    {
        EncounterValues values = context.CurrentValues;
        string encounterState = $"{NewLine}" +
            $"Outcome ({values.Outcome}/10), " +
            $"Pressure ({values.Pressure}/10), " +
            $"Momentum ({values.Momentum}/10), " +
            $"Insight ({values.Insight}/10), " +
            $"Resonance ({values.Resonance}/10)";

        return encounterState;
    }
}