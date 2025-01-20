public class NarrativeSystem
{
    private List<LocationNarrative> narrativeContents;
    private string openAiApiKey;

    private List<CompletionMessage4o> previousPrompts = new();

    public NarrativeSystem(
        GameContentProvider gameContentProvider,
        LargeLanguageAdapter largeLanguageAdapter,
        IConfiguration configuration
        )
    {
        openAiApiKey = configuration.GetValue<string>("OpenAiApiKey");
        narrativeContents = new List<LocationNarrative>();
        narrativeContents = gameContentProvider.GetNarratives();
        LargeLanguageAdapter = largeLanguageAdapter;
    }

    public LargeLanguageAdapter LargeLanguageAdapter { get; }

    public void NewEncounter(EncounterContext context)
    {
        LargeLanguageAdapter.Reset();

        string prompt = "Rainwater streams from your cloak as you push open the heavy wooden door of the wayside inn. The sudden warmth and golden light from the hearth hits you like a physical force after hours on the dark road. Your muscles ache from fighting the wind, and your boots squelch with every step on the worn floorboards.\nThe common room is alive with activity - travelers seeking shelter from the storm have filled most of the tables. Conversations blend with the crackle of the fire and the occasional burst of laughter. A serving girl weaves between patrons with practiced ease, while the innkeeper watches everything from behind a scarred wooden bar.\n\nThe player starts the encounter DISCUSS with the Innkeeper.";
        CompletionMessage4o newPrompt = LargeLanguageAdapter.CreateCompletionMessage(Roles.user, prompt);
        previousPrompts.Add(newPrompt);
    }

    public void NewEncounterStage(EncounterContext context, List<EncounterChoice> choices)
    {
        string prompt1 = $"{1}. {choices[0].Archetype} - {choices[0].Approach} {Environment.NewLine}";
        string prompt2 = $"{2}. {choices[1].Archetype} - {choices[1].Approach} {Environment.NewLine}";
        string prompt3 = $"{3}. {choices[2].Archetype} - {choices[2].Approach} {Environment.NewLine}";

        string prompt = string.Empty;
        prompt += prompt1;
        prompt += prompt2;
        prompt += prompt3;

        CompletionMessage4o newPrompt = LargeLanguageAdapter.CreateCompletionMessage(Roles.user, prompt);

        string response = LargeLanguageAdapter.Execute(previousPrompts, newPrompt, openAiApiKey);
        previousPrompts.Add(newPrompt);

        CompletionMessage4o newResponse = LargeLanguageAdapter.CreateCompletionMessage(Roles.assistant, response);
        previousPrompts.Add(newResponse);

    }

    public string GetStageNarrative()
    {
        SceneNarrative sceneNarrative = LargeLanguageAdapter.GetSceneNarrative();
        return sceneNarrative.Description;
    }

    public List<string> GetStageChoicesNarrative()
    {
        List<ChoicesNarrative> choicesNarrative = LargeLanguageAdapter.GetChoicesNarrative();
        List<string> choices = new List<string>();

        string desig1 = choicesNarrative[0].designation;
        string desig2 = choicesNarrative[1].designation;
        string desig3 = choicesNarrative[2].designation;

        choices.Add(desig1);
        choices.Add(desig2);
        choices.Add(desig3);

        return choices;
    }

    public string GetLocationNarrative(LocationNames locationName)
    {
        LocationNarrative locationNarrative = narrativeContents
            .Where(x => x.LocationName == locationName)
            .FirstOrDefault();

        return locationNarrative.Description;
    }
}