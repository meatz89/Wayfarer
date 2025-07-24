

public class LocationSpot
{
    public string SpotID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string LocationId { get; set; }
    public LocationSpotTypes Type { get; set; }
    public int CurrentLevel { get; set; } = 1;
    public int CurrentSpotXP { get; set; } = 0;
    public int XPToNextLevel { get; set; } = 100;
    public List<TimeBlocks> CurrentTimeBlocks { get; set; } = new List<TimeBlocks>();
    public string InitialState { get; set; }
    public bool PlayerKnowledge { get; set; }

    // Tag Resonance System
    public List<string> DomainTags { get; set; } = new List<string>();
    public string PreferredApproach { get; set; }
    public string DislikedApproach { get; set; }
    public string DomainExpertise { get; set; }

    // Requirements
    public Dictionary<SkillTypes, int> SkillRequirements { get; set; } = new Dictionary<SkillTypes, int>();
    public Dictionary<string, int> RelationshipRequirements { get; set; } = new Dictionary<string, int>();
    public NPC PrimaryNPC { get; set; }
    public bool IsClosed { get; set; }

    // Access Requirements for this spot
    public AccessRequirement AccessRequirement { get; set; }

    public LocationSpot(string id, string name)
    {
        SpotID = id;
        Name = name;
    }

    public string GetCurrentDescription()
    {
        return "location description"; // Placeholder for actual description logic  
    }

    public List<string> GetCurrentProperties()
    {
        return new List<string>();
    }
}