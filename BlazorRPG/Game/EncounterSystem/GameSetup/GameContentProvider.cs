
public class GameContentProvider
{
    private List<Location> locations;
    private List<Character> characters;
    private List<Item> items;

    private List<Quest> quests;
    private List<ChoiceSetTemplate> choiceSetTemplates;
    private List<LocationNarrative> narrativeContents;

    private List<LocationPropertyChoiceEffect> locationArchetypeEffects = new();

    public GameContentProvider()
    {
        InitializeContent();
    }

    private void InitializeContent()
    {
        locations = new List<Location>
        {
            LocationContent.ForestRoad,
            LocationContent.AncientCrossroads,
            LocationContent.WanderersWelcome
        };

        characters = new List<Character>
        {
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
            //QuestContent.MerchantApprentice,
        };


        narrativeContents = NarrativeContent.LocationNarratives;
        choiceSetTemplates = ChoiceSetContent.TutorialSequence;
        locationArchetypeEffects = new List<LocationPropertyChoiceEffect>();
        // LocationPropertyChoiceEffects.AllEffects;
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

    internal List<LocationNarrative> GetNarratives()
    {
        return narrativeContents;
    }
}