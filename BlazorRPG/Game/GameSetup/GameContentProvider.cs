public class GameContentProvider
{
    private List<BasicActionDefinition> basicActionDefinitions;
    private List<LocationProperties> locationProperties;
    private List<Narrative> narratives;

    public GameContentProvider()
    {
        InitializeContent();
    }

    private void InitializeContent()
    {
        basicActionDefinitions = new List<BasicActionDefinition>
        {
            BasicActionDefinitionContent.LaborAction,
            BasicActionDefinitionContent.FoodBuyAction,
            BasicActionDefinitionContent.FoodSellAction,
            BasicActionDefinitionContent.DiscussAction,
            BasicActionDefinitionContent.RestAction,
        };

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

    public List<BasicActionDefinition> GetBasicActionDefinitions()
    {
        return basicActionDefinitions;
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