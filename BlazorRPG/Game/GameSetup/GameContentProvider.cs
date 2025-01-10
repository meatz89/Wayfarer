public class GameContentProvider
{
    private List<Location> locations;
    private List<Character> characters;
    private List<Item> items;

    private List<Quest> quests;
    private List<ChoiceSetTemplate> choiceSetTemplates;

    private List<LocationPropertyChoiceEffect> locationArchetypeEffects = new();

    public GameContentProvider()
    {
        InitializeContent();
    }

    private void InitializeContent()
    {
        locations = new List<Location>
        {
            LocationContent.LionsHeadTavern,
            LocationContent.QuietBookshop,
            LocationContent.CraftsmanWorkshop,
            LocationContent.TendedGarden,
            LocationContent.BusyDockyard,
            LocationContent.BusyMarketplace,
            LocationContent.WildForest
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

        quests = new List<Quest>
        {
            QuestContent.MerchantApprentice,
        };


        choiceSetTemplates = new List<ChoiceSetTemplate>()
        {
            ChoiceSetContent.ServingDrinks,
        };

        locationArchetypeEffects = LocationPropertyChoiceEffects.Effects;
    }

    public List<LocationPropertyChoiceEffect> GetLocationArchetypeEffects()
    {
        return locationArchetypeEffects;
    }

    public List<ChoiceSetTemplate> GetChoiceSetTemplates()
    {
        return choiceSetTemplates;
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

    public List<Quest> GetQuests()
    {
        return quests;
    }
}