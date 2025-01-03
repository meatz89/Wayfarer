public class LocationSpot
{
    public LocationSpotNames Name;
    public LocationNames Location;
    public CharacterNames Character;

    public BasicAction LocationSpotAction;
    public List<BasicAction> CharacterActions;

    public LocationSpot(
        LocationSpotNames locationSpotType,
        LocationNames location,
        CharacterNames character,
        BasicAction spotAction,
        List<BasicAction> characterActions)
    {
        this.Name = locationSpotType;
        this.Location = location;
        this.LocationSpotAction = spotAction;
        this.Character = character;
        this.CharacterActions = characterActions;
    }
}