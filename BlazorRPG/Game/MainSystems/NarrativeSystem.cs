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

    internal string GetLocationNarrative(LocationNames locationName)
    {
        LocationNarrative locationNarrative = narrativeContents
            .Where(x => x.LocationName == locationName)
            .FirstOrDefault();

        return locationNarrative.Description;
    }
}