public class GameContentProvider
{
    private List<Location> locations;
    private List<Character> characters;
    private List<Item> items;

    private List<Quest> quests;
    private List<ChoiceSetTemplate> choiceSetTemplates;

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

        quests = new List<Quest>
        {
            QuestContent.MerchantApprentice,
        };


        choiceSetTemplates = new List<ChoiceSetTemplate>()
        {
            ChoiceSetContent.ExampleTemplate,
        };
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