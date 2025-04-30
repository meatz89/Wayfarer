public class Character
{
    // Identity
    public string Name { get; set; }
    public string Role { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }

    // Narrative elements
    public string Personality { get; set; }
    public string Background { get; set; }
    public string Appearance { get; set; }

    public List<string> InteractionHistory { get; set; } = new List<string>();

    public List<ActionImplementation> Actions = new();
}
