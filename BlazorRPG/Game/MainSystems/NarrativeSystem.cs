public class NarrativeSystem
{
    private List<LocationNarrative> narrativeContents;
    private string openAiApiKey;
    private const string NewLine = "\r\n";

    public NarrativeSystem(
        GameContentProvider gameContentProvider,
        LargeLanguageAdapter largeLanguageAdapter,
        JournalSystem journalSystem,
        IConfiguration configuration
        )
    {
        openAiApiKey = configuration.GetValue<string>("OpenAiApiKey");
        narrativeContents = new List<LocationNarrative>();
        narrativeContents = gameContentProvider.GetNarratives();
        LargeLanguageAdapter = largeLanguageAdapter;
        JournalSystem = journalSystem;
    }

    public LargeLanguageAdapter LargeLanguageAdapter { get; }
    public JournalSystem JournalSystem { get; }

    public void NewEncounter(EncounterContext context, ActionImplementation actionImplementation)
    {
        LargeLanguageAdapter.Reset();

        string taskToSolve = $"The player is in the {context.LocationName}, a {context.LocationArchetype}, at the {context.LocationSpotName}. It is {context.TimeSlot.ToString()}. The player starts a {context.ActionType.ToString()} action: {actionImplementation.Name}.";
        string initialSituation = string.Empty;

        JournalSystem.StartEncounter(taskToSolve, initialSituation);
    }

    public ChoicesNarrativeResponse GetChoicesNarrative(EncounterContext context, List<EncounterChoice> choices)
    {
        string prompt = $"Analyze the narrative consequences of the last choice and create the new Situation Description. " + NewLine +
            $"Create the narrative descriptions for the new choice options: {NewLine}";

        int i = 1;
        foreach (EncounterChoice choice in choices)
        {
            prompt += CreatePromptForChoice(i, choice);
            i++;
        }

        List<CompletionMessage4o> previousPrompts = new();
        List<string> list = JournalSystem.GetDescriptionForCurrentEncounter();
        foreach (string choice in list)
        {
            CompletionMessage4o previousPrompt = OpenAiHelpers.CreateCompletionMessage(Roles.user, choice);
            previousPrompts.Add(previousPrompt);
        }

        CompletionMessage4o choicesPrompt = OpenAiHelpers.CreateCompletionMessage(Roles.user, prompt);
        ChoicesNarrativeResponse choicesNarrativeResponse = LargeLanguageAdapter.NextEncounterChoices(previousPrompts, choicesPrompt, openAiApiKey);

        JournalSystem.NoteNewEncounterAssistantNarrative(choicesNarrativeResponse.introductory_narrative);
        return choicesNarrativeResponse;
    }

    public void MakeChoice(EncounterContext context, EncounterChoice encounterChoice)
    {
        string choice = $"{encounterChoice.Designation} {NewLine}" +
            $"This is a {encounterChoice.Archetype.ToString().ToUpper()} choice{NewLine}" +
            $"This is a {encounterChoice.Approach.ToString().ToUpper()} approach";

        string prompt = $"The player chose: {choice}";
        JournalSystem.NoteNewEncounterNarrative(prompt);
    }

    public string GetEncounterSuccessNarrative(EncounterContext context)
    {
        string prompt = $"The last player decision successfully ended the encounter. " +
            $"Wrap up the encounter narrative.";

        List<CompletionMessage4o> previousPrompts = new();
        List<string> list = JournalSystem.GetDescriptionForCurrentEncounter();
        foreach (string choice in list)
        {
            CompletionMessage4o previousPrompt = OpenAiHelpers.CreateCompletionMessage(Roles.user, choice);
            previousPrompts.Add(previousPrompt);
        }

        CompletionMessage4o choicesPrompt = OpenAiHelpers.CreateCompletionMessage(Roles.user, prompt);
        string encounterEndNarrative = LargeLanguageAdapter.EncounterEndNarrative(previousPrompts, choicesPrompt, openAiApiKey);

        JournalSystem.EndEncounter(encounterEndNarrative);

        return encounterEndNarrative;
    }

    private static string CreatePromptForChoice(int index, EncounterChoice encounterChoice)
    {
        string prompt = $"{index}. ({encounterChoice.Archetype} - {encounterChoice.Approach})";
        string effects = $" effects {NewLine}";

        List<BaseValueChange> baseValueChanges = encounterChoice.CalculationResult.BaseValueChanges;
        List<ValueModification> valueModifications = encounterChoice.CalculationResult.ValueModifications;

        foreach (BaseValueChange baseValueChange in baseValueChanges)
        {
            effects += $"{baseValueChange.Amount} to {baseValueChange.ValueType}" + NewLine;
        }

        foreach (ValueModification valueModification in valueModifications)
        {
            if (valueModification is EncounterValueModification encounterValueMod)
                effects += $"{encounterValueMod.Amount} to {encounterValueMod.ValueType}" + NewLine;
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

}