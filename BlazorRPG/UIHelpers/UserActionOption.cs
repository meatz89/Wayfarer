
public class UserActionOption
{
    public int Index { get; set; }
    public string Description { get; set; }
    public bool IsDisabled { get; set; }
    public BasicAction BasicAction { get; set; }
    public LocationNames Location { get; internal set; }
    public LocationSpotTypes LocationSpot { get; internal set; }
    public CharacterNames Character { get; internal set; }

    public string Display()
    {
        return $"{Index}. {Description}";
    }
}
