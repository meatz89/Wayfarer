public class Character
{
    public CharacterNames CharacterName { get; set; }
    public LocationNames Location { get; set; }

    public List<ActionImplementation> Actions = new();
}