public class LocationSpot
{
    public string Id { get; init; }
    public string Name { get; init; }
    public string Description { get; set; }
    public string InteractionDescription { get; set; }

    public Population? Population { get; set; } = Population.Quiet;
    public Atmosphere? Atmosphere { get; set; } = Atmosphere.Calm;
    public Physical? Physical { get; set; } = Physical.Confined;
    public Illumination? Illumination { get; set; } = Illumination.Bright;
    public bool PlayerKnowledge { get; set; }

    // Progression
    public int CurrentLevel { get; set; } = 1;
    public int CurrentSpotXP { get; set; } = 0;
    public int XPToNextLevel { get; set; } = 0;

    // Requirements
    public Dictionary<string, int> SkillRequirements { get; set; }
    public Dictionary<string, int> RelationshipRequirements { get; set; }
    public int ReputationRequirement { get; set; }

    public LocationSpotTypes LocationSpotType = LocationSpotTypes.Location;
    public string CharacterName { get; set; }

    public List<TimeWindow> TimeWindows { get; set; } = new() { TimeWindow.Morning, TimeWindow.Afternoon, TimeWindow.Evening, TimeWindow.Night };
    public bool IsClosed { get; set; }
    public string LocationId { get; internal set; }

    public LocationSpot(string id, string name)
    {
        Id = id;
        Name = name;
    }

    public void IncreaseSpotXP(int spotXp)
    {
        CurrentSpotXP += spotXp;
        if (CurrentSpotXP >= XPToNextLevel)
        {
            CurrentLevel++;
            CurrentSpotXP = 0;
            XPToNextLevel = CalculateXPToNextLevel(CurrentLevel);
        }
    }

    private int CalculateXPToNextLevel(int currentLevel)
    {
        // Simple formula for now, can be adjusted later
        return currentLevel * 100; // Example: 100 XP per level
    }
}
