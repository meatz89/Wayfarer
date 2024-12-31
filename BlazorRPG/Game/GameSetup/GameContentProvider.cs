
public class GameContentProvider
{
    private List<Location> locationProperties;
    private List<Character> characterProperties;
    private List<Narrative> narratives;

    public GameContentProvider()
    {
        InitializeContent();
    }

    private void InitializeContent()
    {
        locationProperties = new List<Location>
        {
            LocationContent.HarborStreets,
            LocationContent.Docks,
            LocationContent.MarketSquare,
            LocationContent.ArtisanRow,
            LocationContent.Tavern,
            LocationContent.ForestEdge,
            LocationContent.CarpentersWorkshop
        };

        characterProperties = new List<Character>
        {
            CharacterContent.MarketVendor,
        };

        narratives = new List<Narrative>
        {
            DockNarrativesContent.DockWork,
            DockNarrativesContent.DocksInvestigation,
            MarketNarrativesContent.MarketInvestigation
        };
    }

    public List<Location> GetLocationProperties()
    {
        return locationProperties;
    }

    public List<Character>? GetCharacterProperties()
    {
        return characterProperties;
    }

    public List<Narrative> GetNarratives()
    {
        return narratives;
    }
}