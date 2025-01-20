
public class NarrativeSystem
{
    private List<LocationNarrative> narrativeContents;

    public NarrativeSystem(GameContentProvider gameContentProvider, LargeLanguageAdapter largeLanguageAdapter)
    {
        narrativeContents = new List<LocationNarrative>();
        narrativeContents = gameContentProvider.GetNarratives();
        LargeLanguageAdapter = largeLanguageAdapter;
    }

    public LargeLanguageAdapter LargeLanguageAdapter { get; }

    public void GenerateStageNarrative(EncounterContext context, List<EncounterChoice> choices)
    {
        var prompt = "";

        var prompt1 = $"{1}. {choices[0].Archetype} - {choices[0].Approach} {Environment.NewLine}";
        var prompt2 = $"{2}. {choices[1].Archetype} - {choices[1].Approach} {Environment.NewLine}";
        var prompt3 = $"{3}. {choices[2].Archetype} - {choices[2].Approach} {Environment.NewLine}";

        var request = new RequestObject()
        {
            Choice1 = prompt1,
            Choice2 = prompt2,
            Choice3 = prompt3,
        };

        LargeLanguageAdapter.Execute(request);
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

        var desig1 = choicesNarrative[0].designation;
        var desig2 = choicesNarrative[1].designation;
        var desig3 = choicesNarrative[2].designation;

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