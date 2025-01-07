public class GameContentProvider
{
    private List<Location> locations;
    private List<Character> characters;
    private List<Item> items;

    private List<Encounter> encounters;
    private List<Quest> quests;

    public GameContentProvider()
    {
        InitializeContent();
    }

    private void InitializeContent()
    {
        locations = new List<Location>
        {
            LocationContent.LionsHeadTavern,
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

        encounters = new List<Encounter>
        {
            EncounterContent.TavernServeDrinks
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

    public List<Encounter> GetEncounters()
    {
        return encounters;
    }

    public List<Quest> GetQuests()
    {
        return quests;
    }
}