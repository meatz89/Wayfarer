public class LocationProperties
{
    public LocationNames Location { get; internal set; }
    public LocationTypes LocationType;
    public ActivityTypes ActivityType;
    public TradeResourceTypes TradeResourceType;
    public TradeDirections TradeDirection;
    public List<TimeSlots> TimeSlots = new();
    public BasicActionTypes PrimaryAction;
}