public class GameContentProvider
{
    private List<LocationActions> locationActions;
    private List<Narrative> narratives;

    public GameContentProvider()
    {
        InitializeContent();
    }

    private void InitializeContent()
    {
        locationActions = new List<LocationActions>
        {
            LocationActionsContent.Docks,
            LocationActionsContent.Market
        };

        narratives = new List<Narrative>
        {
            DockNarrativesContent.DockWork,
            DockNarrativesContent.DocksInvestigation,
            MarketNarrativesContent.MarketInvestigation
        };
    }

    public List<LocationActions> GetLocationActions()
    {
        return locationActions;
    }

    public List<Narrative> GetNarratives()
    {
        return narratives;
    }
}