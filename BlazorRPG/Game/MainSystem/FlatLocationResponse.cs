public class FlatLocationResponse
{
    public PlayerLocationUpdate PlayerLocationUpdate { get; set; } = new PlayerLocationUpdate();
    public List<FlatLocationSpot> LocationSpots { get; set; } = new List<FlatLocationSpot>();
    public List<FlatActionDefinition> ActionDefinitions { get; set; } = new List<FlatActionDefinition>();
}
