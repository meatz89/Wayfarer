public class GameContentProvider
{
    private List<Location> locations;
    private List<Character> characters;

    public string GetBackground => "Rainwater streams from your cloak as you push open the heavy wooden door of the wayside inn. The sudden warmth and golden light from the hearth hits you like a physical force after hours on the dark road. Your muscles ache from fighting the wind, and your boots squelch with every step on the worn floorboards.";
    public string GetInitialSituation => "";
    public GameContentProvider()
    {
        InitializeContent();
    }

    private void InitializeContent()
    {
        characters = new List<Character>
        {
        };
    }


    public List<Character>? GetCharacters()
    {
        return characters;
    }


}