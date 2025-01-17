public class NarrativeSystem
{
    private List<LocationNarrative> narrativeContents;

    public NarrativeSystem(GameContentProvider gameContentProvider)
    {
        narrativeContents = new List<LocationNarrative>();
        narrativeContents = gameContentProvider.GetNarratives();
    }

    internal string GetLocationNarrative(LocationNames locationName)
    {
        LocationNarrative locationNarrative = narrativeContents
            .Where(x => x.LocationName == locationName)
            .FirstOrDefault();

        return locationNarrative.Description;
    }
}