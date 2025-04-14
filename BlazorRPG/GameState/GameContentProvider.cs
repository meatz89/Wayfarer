public class GameContentProvider
{
    private List<Location> locations;
    private List<Character> characters;
    private List<Item> items;

    public string GetBackground => "Rainwater streams from your cloak as you push open the heavy wooden door of the wayside inn. The sudden warmth and golden light from the hearth hits you like a physical force after hours on the dark road. Your muscles ache from fighting the wind, and your boots squelch with every step on the worn floorboards.";
    public string GetInitialSituation => "";
    public GameContentProvider()
    {
        InitializeContent();
    }

    private void InitializeContent()
    {
        locations = new List<Location>
        {
            WorldLocationsContent.Village,
            WorldLocationsContent.DeepForest,
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

}