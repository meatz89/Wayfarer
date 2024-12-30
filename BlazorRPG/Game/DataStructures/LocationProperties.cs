public class LocationProperties
{
    public LocationNames Location { get; internal set; }

    public LocationTypes LocationType;
    public ActivityTypes ActivityType;

    public List<BasicAction> Actions = new();
}