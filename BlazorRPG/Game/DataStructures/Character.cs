public class Character
{
    public CharacterNames CharacterName { get; set; }
    public LocationNames Location { get; set; }

    public List<BasicAction> Actions = new();
}