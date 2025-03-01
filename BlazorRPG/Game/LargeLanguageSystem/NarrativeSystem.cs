public class NarrativeSystem
{
    private List<LocationNarrative> narrativeContents;
    private string openAiApiKey;
    private const string NewLine = "\r\n";
    public LargeLanguageAdapter LargeLanguageAdapter { get; }
    public JournalSystem JournalSystem { get; }

    public NarrativeSystem(
        GameContentProvider gameContentProvider,
        JournalSystem journalSystem,
        IConfiguration configuration
        )
    {
        narrativeContents = gameContentProvider.GetNarratives();
        JournalSystem = journalSystem;
        openAiApiKey = configuration.GetValue<string>("OpenAiApiKey");
        LargeLanguageAdapter = new LargeLanguageAdapter(openAiApiKey);
    }

    public void NewEncounter(EncounterContext context, ActionImplementation actionImplementation)
    {
        LargeLanguageAdapter.Reset();

        string initialSituation = $"The player is in the {context.Location}, a {context.Location.LocationArchetype}, at the {context.LocationSpot.Name}. The player starts a {context.ActionType.ToString()} action.";
        string actionGoal = $"{actionImplementation.Name}";

        JournalSystem.StartEncounter(initialSituation, actionGoal);
    }

    public ChoicesNarrativeResponse GetChoicesNarrative(EncounterState context, List<Choice> choices)
    {
        string encounterState = "Encounter State:" + GetEncounterState(context);
        string initialGoal = JournalSystem.GetCurrentEncounterGoal();

        string prompt = $"{NewLine}";
        prompt += $"Do NOT stray away to far from the initial goal of the encounter: '{initialGoal}' {NewLine}{NewLine}";

        prompt += $"{encounterState}{NewLine}{NewLine}";
        prompt = AddChoicesToPrompt(choices, prompt);

        List<CompletionMessage4o> previousPrompts = GetPreviousPrompts();

        CompletionMessage4o choicesPrompt = OpenAiHelper.CreateCompletionMessage(Roles.user, prompt);
        ChoicesNarrativeResponse choicesNarrativeResponse = LargeLanguageAdapter.NextEncounterChoices(previousPrompts, choicesPrompt);

        JournalSystem.NoteNewEncounterAssistantNarrative(choicesNarrativeResponse.introductory_narrative);
        return choicesNarrativeResponse;
    }

    public string GetEncounterSuccessNarrative(EncounterState context)
    {
        string encounterState = "Final Encounter State:" + GetEncounterState(context);

        string prompt = $"The last player decision ended the encounter successfully. " +
            $"Wrap up the encounter narrative.{NewLine}{NewLine}";
        prompt += $"{encounterState}{NewLine}{NewLine}";

        List<CompletionMessage4o> previousPrompts = GetPreviousPrompts();

        CompletionMessage4o choicesPrompt = OpenAiHelper.CreateCompletionMessage(Roles.user, prompt);
        string encounterEndNarrative = LargeLanguageAdapter.EncounterEndNarrative(previousPrompts, choicesPrompt);

        JournalSystem.EndEncounter(encounterEndNarrative);

        return encounterEndNarrative;
    }

    public string GetEncounterFailureNarrative(EncounterState encounterStates)
    {
        string encounterState = "Final Encounter State:" + GetEncounterState(encounterStates);

        string prompt = $"The last player decision resulted in failing the encounter. " +
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


    public void MakeChoice(EncounterContext context, Choice encounterChoice)
    {
        string prompt =
            $"{encounterChoice.Narrative} ({encounterChoice.Description}){NewLine}" +
            $"[{encounterChoice.ToString()}]";

        JournalSystem.NoteNewEncounterNarrative(prompt);
    }

    private static string AddChoicesToPrompt(List<Choice> choices, string prompt)
    {
        int i = 1;
        prompt += $"Create the narrative descriptions for the new choice options:{NewLine}{NewLine}";
        foreach (Choice choice in choices)
        {
            prompt += CreatePromptForChoice(i, choice);
            i++;
        }

        return prompt;
    }

    private static string CreatePromptForChoice(int index, Choice encounterChoice)
    {
        string prompt = $"{index}. ({encounterChoice.ToString()})";
        string effects = $": ";

        //List<ValueModification> valueModifications = encounterChoice.CalculationResult.ValueModifications;

        //foreach (ValueModification valueModification in valueModifications)
        //{
        //    if (valueModification is MomentumModification encounterValueMod)
        //        effects += $"{encounterValueMod.Amount} to Momentum; ";

        //    if (valueModification is EnergyCostReduction energyCostReduction)
        //        effects += $"{energyCostReduction.Amount} to {energyCostReduction.EnergyType}; ";
        //}

        prompt = prompt + effects + NewLine;
        return prompt;
    }

    public string GetLocationNarrative(LocationNames locationName)
    {
        LocationNarrative locationNarrative = narrativeContents
            .Where(x => x.LocationName == locationName)
            .FirstOrDefault();

        if (locationNarrative == null)
        {
            return string.Empty;
        }

        return locationNarrative.Description;
    }

    private static string GetEncounterState(EncounterState context)
    {
        string encounterState = $"{NewLine}" +
            $"Outcome ({context.Momentum})";

        return encounterState;
    }
}