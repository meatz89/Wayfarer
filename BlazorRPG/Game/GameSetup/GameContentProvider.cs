public class GameContentProvider
{
    private List<LocationProperties> locationProperties;
    private List<Narrative> narratives;

    public GameContentProvider()
    {
        InitializeContent();
    }

    private void InitializeContent()
    {
        locationProperties = new List<LocationProperties>
        {
            LocationPropertiesContent.HarborStreets,
            LocationPropertiesContent.Docks,
            LocationPropertiesContent.Market,
            LocationPropertiesContent.Tavern
        };

        narratives = new List<Narrative>
        {
            DockNarrativesContent.DockWork,
            DockNarrativesContent.DocksInvestigation,
            MarketNarrativesContent.MarketInvestigation
        };
    }

    public List<LocationProperties> GetLocationProperties()
    {
        return locationProperties;
    }

    public List<Narrative> GetNarratives()
    {
        return narratives;
    }
}