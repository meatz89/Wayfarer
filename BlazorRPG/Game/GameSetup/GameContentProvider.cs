
public class GameContentProvider
{
    private List<LocationProperties> locationProperties;
    private List<CharacterProperties> characterProperties;
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
            LocationPropertiesContent.Tavern,
            LocationPropertiesContent.Forest
        };

        characterProperties = new List<CharacterProperties>
        {
            CharacterPropertiesContent.MarketVendor,
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

    public List<CharacterProperties>? GetCharacterProperties()
    {
        return characterProperties;
    }

    public List<Narrative> GetNarratives()
    {
        return narratives;
    }
}