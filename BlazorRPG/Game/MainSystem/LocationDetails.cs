public class LocationDetails
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string DetailedDescription { get; set; } = "";
    public string History { get; set; } = "";
    public string PointsOfInterest { get; set; } = "";
    public int TravelTimeMinutes { get; set; }
    public string TravelDescription { get; set; } = "";
    public List<string> ConnectedLocationIds { get; set; } = new List<string>();
    public List<SpotDetails> NewLocationSpots { get; set; } = new List<SpotDetails>();
    public List<StrategicTag> StrategicTags { get; set; } = new List<StrategicTag>();
    public List<NarrativeTag> NarrativeTags { get; set; } = new List<NarrativeTag>();
    public List<NewAction> NewActions { get; set; } = new List<NewAction>();
    public PlayerLocationUpdate LocationUpdate { get; set; } = new PlayerLocationUpdate();
}
