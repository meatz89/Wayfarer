public class LocationSpot
{
    public LocationSpotTypes Name;
    public LocationNames Location;
    public BasicAction LocationSpotAction;

    public LocationSpot(LocationSpotTypes locationSpotType, LocationNames location, BasicAction action)
    {
        this.Name = locationSpotType;
        this.Location = location;
        this.LocationSpotAction = action;
    }
}