public class Choice
{
    public int Index { get; set; }
    public string Description { get; set; }
    public List<IRequirement> Requirements { get; set; } = new();
    public List<IOutcome> Outcomes { get; set; } = new();
}