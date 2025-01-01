public class LocationSpot
{
    public LocationSpotTypes Name;
    public BasicAction LocationSpotAction;

    public LocationSpot(LocationSpotTypes locationSpotType, BasicAction action)
    {
        this.Name = locationSpotType;
        this.LocationSpotAction = action;
    }
}