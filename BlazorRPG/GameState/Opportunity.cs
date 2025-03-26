public class Opportunity
{
    // Identity
    public string Name { get; set; }
    public string Type { get; set; }  // "Quest", "Mystery", "Job" - flexible text
    public string Description { get; set; }
    public string Location { get; set; }
    public string LocationSpot { get; set; }  // Specific spot where this opportunity is found
    public string RelatedCharacter { get; set; }

    // Classification
    public string Status { get; set; } = "Available";

    // Connections

    // Narrative details
    public string DetailedDescription { get; set; }
    public string Challenges { get; set; }

    // Rewards (suggested values for engine to implement)
    public Dictionary<string, int> ResourceRewards { get; set; } = new Dictionary<string, int>();
    public List<string> ItemRewards { get; set; } = new List<string>();
    public Dictionary<string, int> RelationshipChanges { get; set; } = new Dictionary<string, int>();
    public List<string> SkillExperience { get; set; } = new List<string>();
}

