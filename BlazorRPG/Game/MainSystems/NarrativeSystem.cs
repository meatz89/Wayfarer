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

        string initialSituation = $"The player is in the {context.LocationName}, a {context.LocationArchetype}, at the {context.LocationSpotName}. It is {context.TimeSlot.ToString()}. The player starts a {context.ActionType.ToString()} action.";
        string actionGoal = $"{actionImplementation.Name}";

        JournalSystem.StartEncounter(initialSituation, actionGoal);
    }

    public ChoicesNarrativeResponse GetChoicesNarrative(EncounterContext context, List<EncounterChoice> choices)
    {
        var values = context.CurrentValues;
        var encounterState = $"Current States: " +
            $"Outcome ({values.Outcome}/10), " +
            $"Pressure ({values.Pressure}/10), " +
            $"Momentum ({values.Momentum}/10), " +
            $"Insight ({values.Insight}/10), " +
            $"Resonance ({values.Resonance}/10)";

        string initialGoal = JournalSystem.GetCurrentEncounterGoal();

        string prompt = $"Analyze the narrative consequences of the last choice and create the new Situation Description. " + NewLine;
        prompt += $"Do NOT stray away to far from the initial goal of the encounter: '{initialGoal}' {NewLine}{NewLine}";
        prompt += $"{encounterState}{NewLine}{NewLine}";
        prompt = AddChoicesToPrompt(choices, prompt);

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

    public void MakeChoice(EncounterContext context, EncounterChoice encounterChoice)
    {
        string choice = 
            $"{encounterChoice.Designation} " +
            $"('{encounterChoice.Narrative}'){NewLine}" +
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
        string effects = $": ";

        List<BaseValueChange> baseValueChanges = encounterChoice.CalculationResult.BaseValueChanges;
        List<ValueModification> valueModifications = encounterChoice.CalculationResult.ValueModifications;

        foreach (BaseValueChange baseValueChange in baseValueChanges)
        {
            effects += $"{baseValueChange.Amount} to {baseValueChange.ValueType}, ";
        }

        foreach (ValueModification valueModification in valueModifications)
        {
            if (valueModification is EncounterValueModification encounterValueMod)
                effects += $"{encounterValueMod.Amount} to {encounterValueMod.ValueType}, ";
        }

        prompt = prompt + effects + NewLine + NewLine;

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