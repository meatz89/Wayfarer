public class BasicAction
{
    public BasicActionTypes Id { get; set; }
    public string Name { get; set; }
    public bool IsAvailable = true;
    public int Cost = 1;

    public List<TimeWindows> TimeSlots = new();
    public List<IRequirement> Requirements { get; set; } = new();
    public List<IOutcome> Outcomes { get; set; } = new();
}