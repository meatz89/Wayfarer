public class Character
{
    // Identity
    public string Name { get; set; }
    public string Description { get; set; }

    // Narrative elements
    public string Role { get; set; }  // Not an enum - flexible text
    public string Personality { get; set; }
    public string Background { get; set; }
    public string Appearance { get; set; }

    // Location
    public string HomeLocationId { get; set; }

    // Relationships
    public Dictionary<string, string> RelationshipsWithOthers { get; set; } = new Dictionary<string, string>();

    // Encounter preferences
    public Dictionary<string, int> ApproachPreferences { get; set; } = new Dictionary<string, int>();

    // Player interaction history
    public List<string> InteractionHistory { get; set; } = new List<string>();


    public List<ActionImplementation> Actions = new();
}