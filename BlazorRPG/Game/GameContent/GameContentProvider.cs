public class GameContentProvider
{
    private List<Narrative> narratives;

    public GameContentProvider()
    {
        InitializeContent();
    }

    private void InitializeContent()
    {
        narratives = new List<Narrative>
        {
            DockNarratives.DockWork,
            DockNarratives.DocksInvestigation,
            MarketNarratives.MarketInvestigation
        };
    }

    public List<Narrative> GetNarratives()
    {
        return narratives;
    }
}