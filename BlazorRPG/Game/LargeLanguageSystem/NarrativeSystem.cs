using BlazorRPG.Game.EncounterManager;

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
        string EncounterState = "Encounter State:" + GetEncounterState(context);
        string initialGoal = JournalSystem.GetCurrentEncounterGoal();

        string prompt = $"{NewLine}";
        prompt += $"Do NOT stray away to far from the initial goal of the Encounter: '{initialGoal}' {NewLine}{NewLine}";

        prompt += $"{EncounterState}{NewLine}{NewLine}";
        prompt = AddChoicesToPrompt(choices, prompt);

        List<CompletionMessage4o> previousPrompts = GetPreviousPrompts();

        CompletionMessage4o choicesPrompt = OpenAiHelper.CreateCompletionMessage(Roles.user, prompt);
        ChoicesNarrativeResponse choicesNarrativeResponse = LargeLanguageAdapter.NextEncounterChoices(previousPrompts, choicesPrompt);

        JournalSystem.NoteNewEncounterAssistantNarrative(choicesNarrativeResponse.introductory_narrative);
        return choicesNarrativeResponse;
    }

    public string GetEncounterSuccessNarrative(EncounterState context)
    {
        string EncounterState = "Final Encounter State:" + GetEncounterState(context);

        string prompt = $"The last player decision ended the Encounter successfully. " +
            $"Wrap up the Encounter narrative.{NewLine}{NewLine}";
        prompt += $"{EncounterState}{NewLine}{NewLine}";

        List<CompletionMessage4o> previousPrompts = GetPreviousPrompts();

        CompletionMessage4o choicesPrompt = OpenAiHelper.CreateCompletionMessage(Roles.user, prompt);
        string EncounterEndNarrative = LargeLanguageAdapter.EncounterEndNarrative(previousPrompts, choicesPrompt);

        JournalSystem.EndEncounter(EncounterEndNarrative);

        return EncounterEndNarrative;
    }

    public string GetEncounterFailureNarrative(EncounterState EncounterStates)
    {
        string EncounterState = "Final Encounter State:" + GetEncounterState(EncounterStates);

        string prompt = $"The last player decision resulted in failing the Encounter. " +
            $"Wrap up the Encounter narrative.{NewLine}{NewLine}";
        prompt += $"{EncounterState}{NewLine}{NewLine}";

        List<CompletionMessage4o> previousPrompts = GetPreviousPrompts();

        CompletionMessage4o choicesPrompt = OpenAiHelper.CreateCompletionMessage(Roles.user, prompt);
        string EncounterEndNarrative = LargeLanguageAdapter.EncounterEndNarrative(previousPrompts, choicesPrompt);

        JournalSystem.EndEncounter(EncounterEndNarrative);

        return EncounterEndNarrative;
    }

    private List<CompletionMessage4o> GetPreviousPrompts()
    {
        List<CompletionMessage4o> previousPrompts = new();
        List<string> initialDescriptions = JournalSystem.GetInitialEncounterDescriptions();
        List<Narrative> EncounterNarratives = JournalSystem.GetDescriptionForCurrentEncounter();
        foreach (string description in initialDescriptions)
        {
            CompletionMessage4o previousPrompt =
                OpenAiHelper.CreateCompletionMessage(Roles.user, description);
            previousPrompts.Add(previousPrompt);
        }
        foreach (Narrative narrative in EncounterNarratives)
        {
            CompletionMessage4o previousPrompt =
                OpenAiHelper.CreateCompletionMessage(narrative.Role, narrative.Text);
            previousPrompts.Add(previousPrompt);
        }
        return previousPrompts;
    }


    public void MakeChoicePrompt(Choice EncounterChoice)
    {
        string prompt =
            $"{EncounterChoice.ToString()}";

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

    private static string CreatePromptForChoice(int index, Choice EncounterChoice)
    {
        string prompt = $"{index}. ({EncounterChoice.ToString()})";
        string effects = $": ";

        //List<ValueModification> valueModifications = EncounterChoice.CalculationResult.ValueModifications;

        //foreach (ValueModification valueModification in valueModifications)
        //{
        //    if (valueModification is MomentumModification EncounterValueMod)
        //        effects += $"{EncounterValueMod.Amount} to Momentum; ";

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
        string EncounterState = $"{NewLine}" +
            $"Outcome ({context.Momentum})";

        return EncounterState;
    }
}