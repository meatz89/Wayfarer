

public class GameContentProvider
{
    private List<Location> locationProperties;
    private List<Character> characterProperties;
    private List<Narrative> narratives;
    private List<Quest> quests;

    public GameContentProvider()
    {
        InitializeContent();
    }

    private void InitializeContent()
    {
        locationProperties = new List<Location>
        {
            LocationContent.HarborStreets,
            LocationContent.MarketSquare,
            LocationContent.Tavern,
        };

        characterProperties = new List<Character>
        {
            CharacterContent.Bartender,
            CharacterContent.WealthyMerchant
        };

        narratives = new List<Narrative>
        {
            DockNarrativesContent.DockWork,
            DockNarrativesContent.DocksInvestigation,
            MarketNarrativesContent.MarketInvestigation
        };

        quests = new List<Quest>
        {
            QuestContent.MerchantApprentice,
        };
    }

    public List<Location> GetLocations()
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

    public List<Quest> GetQuests()
    {
        return quests;
    }
}