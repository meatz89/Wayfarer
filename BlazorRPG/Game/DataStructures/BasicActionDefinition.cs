public class BasicActionDefinition
{
    public BasicActionTypes ActionType { get; internal set; }
    public List<IRequirement> Requirements { get; set; } = new();
    public List<IOutcome> Outcomes { get; set; } = new();
}