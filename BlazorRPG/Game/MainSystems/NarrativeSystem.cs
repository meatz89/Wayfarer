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

        string taskToSolve = $"You are in the {context.LocationName}, a {context.LocationArchetype}, at the {context.LocationSpotName}. It is {context.TimeSlot.ToString()}. You start your {context.ActionType.ToString()} task: { actionImplementation.Name }.";
        string initialSituation = string.Empty;

        JournalSystem.StartEncounter(taskToSolve, initialSituation);
    }

    public void MakeChoice(EncounterContext context, EncounterChoice encounterChoice)
    {
        string choice = $"[{encounterChoice.Archetype} - {encounterChoice.Approach}] {encounterChoice.Description}";
        string prompt = $"You made a choice: {choice}";

        JournalSystem.NoteNewEncounterNarrative(prompt);
    }

    public ChoicesNarrativeResponse GetNewStageChoicesNarrative(EncounterContext context, List<EncounterChoice> choices)
    {
        string prompt = $"Analyze the narrative consequences of the last choice and create the new Situation Description. " + NewLine +
            $"Create the narrative descriptions for the new choice options: {NewLine}";

        int i = 1;
        foreach(var choice in choices)
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

    private static string CreatePromptForChoice(int index, EncounterChoice encounterChoice)
    {
        return $"{index}. ({encounterChoice.Archetype} - {encounterChoice.Approach}) {NewLine}";
    }

    public string GetEncounterSuccessNarrative(EncounterContext context)
    {
        string prompt = $"The encounter is over. The player has won. Wrap up the encounter narrative.";

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

    public string GetLocationNarrative(LocationNames locationName)
    {
        LocationNarrative locationNarrative = narrativeContents
            .Where(x => x.LocationName == locationName)
            .FirstOrDefault();

        return locationNarrative.Description;
    }

}