public class CharacterProperties
{
    public CharacterNames Character { get; set; }
    public LocationNames Location { get; set; }

    public List<BasicAction> Actions = new();
}