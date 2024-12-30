public class LocationProperties
{
    public LocationNames Location { get; internal set; }

    public LocationTypes LocationType;

    public List<ActivityTypes> ActivityTypes;

    public List<BasicAction> Actions = new();
}