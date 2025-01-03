

public class GameContentProvider
{
    private List<Location> locations;
    private List<Character> characters;
    private List<Item> items;

    private List<Narrative> narratives;
    private List<Quest> quests;

    public GameContentProvider()
    {
        InitializeContent();
    }

    private void InitializeContent()
    {
        locations = new List<Location>
        {
            LocationContent.HarborStreets,
            LocationContent.MarketSquare,
            LocationContent.Tavern,
        };

        characters = new List<Character>
        {
            CharacterContent.Bartender,
            CharacterContent.WealthyMerchant
        };

        items = new List<Item>
        {
            ItemContent.WoodcuttersAxe,
            ItemContent.TorchLight,
            ItemContent.CraftingApron,
            ItemContent.CharmingPendant
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
        return locations;
    }

    public List<Character>? GetCharacters()
    {
        return characters;
    }

    public List<Item> GetItems()
    {
        return items;
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