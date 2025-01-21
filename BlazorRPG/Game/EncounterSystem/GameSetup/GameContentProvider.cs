public class GameContentProvider
{
    private List<Location> locations;
    private List<Character> characters;
    private List<Item> items;

    private List<Quest> quests;
    private List<ChoiceSetTemplate> choiceSetTemplates;
    private List<LocationNarrative> narrativeContents;

    private List<LocationPropertyChoiceEffect> locationArchetypeEffects = new();

    public string GetBackground => "Rainwater streams from your cloak as you push open the heavy wooden door of the wayside inn. The sudden warmth and golden light from the hearth hits you like a physical force after hours on the dark road. Your muscles ache from fighting the wind, and your boots squelch with every step on the worn floorboards.";
    public string GetInitialSituation => "";
        //"The common room is alive with activity - travelers seeking shelter from the storm have filled most of the tables. Conversations blend with the crackle of the fire and the occasional burst of laughter. A serving girl weaves between patrons with practiced ease, while the innkeeper watches everything from behind a scarred wooden bar.";

    public GameContentProvider()
    {
        InitializeContent();
    }

    private void InitializeContent()
    {
        locations = new List<Location>
        {
            LocationContent.WaysideInn,
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
        choiceSetTemplates = ChoiceSetTemplateContent.TutorialSequence;
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