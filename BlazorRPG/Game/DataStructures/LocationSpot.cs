public class LocationSpot
{
    public LocationSpotTypes Name;
    public LocationNames Location;
    public BasicAction LocationSpotAction;
    public List<BasicAction> CharacterActions;

    public LocationSpot(
        LocationSpotTypes locationSpotType, 
        LocationNames location, 
        BasicAction spotAction, 
        List<BasicAction> characterActions)
    {
        this.Name = locationSpotType;
        this.Location = location;
        this.LocationSpotAction = spotAction;
        this.CharacterActions = characterActions;
    }
}