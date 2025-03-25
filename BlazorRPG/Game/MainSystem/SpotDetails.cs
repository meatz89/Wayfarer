public class SpotDetails
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string InteractionType { get; set; }
    public string InteractionDescription { get; set; }
    public string Position { get; set; }
    public List<ActionNames> ActionNames { get; set; } = new List<ActionNames>();
}
