public class BasicAction
{
    public BasicActionTypes ActionType { get; internal set; }
    public string Description { get; internal set; }

    public List<TimeWindows> TimeSlots = new();
    public List<IRequirement> Requirements { get; set; } = new();
    public List<IOutcome> Outcomes { get; set; } = new();
}