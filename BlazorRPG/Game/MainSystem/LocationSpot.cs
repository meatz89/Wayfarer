public class LocationSpot
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string LocationId { get; set; }
    public LocationSpotTypes Type { get; set; }
    public int CurrentLevel { get; set; } = 1;
    public int CurrentSpotXP { get; set; } = 0;
    public int XPToNextLevel { get; set; } = 100;
    public TimeWindows TimeWindows { get; set; } = TimeWindows.None;

    public string InitialState { get; set; }
    public bool PlayerKnowledge { get; set; }

    // Requirements
    public Dictionary<string, int> SkillRequirements { get; set; }
    public Dictionary<string, int> RelationshipRequirements { get; set; }
    public string CharacterName { get; set; }
    public bool IsClosed { get; set; }

    public LocationSpot(string id, string name)
    {
        Id = id;
        Name = name;
    }

    private int CalculateXPToNextLevel(int currentLevel)
    {
        // Simple formula for now, can be adjusted later
        return currentLevel * 100; // Example: 100 XP per level
    }
}
