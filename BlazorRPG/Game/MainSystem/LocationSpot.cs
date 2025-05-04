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
    public List<SpotLevel> LevelData { get; set; } = new List<SpotLevel>();
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


    /// <summary>
    /// Returns available actions for the given spot-level.
    /// If an EncounterActionId exists at that exact level, only that action is returned.
    /// Otherwise, applies all Added/Removed lists for definitions <= currentLevel.
    /// </summary>
    public List<string> GetActionsForLevel(int currentLevel)
    {
        // Check for exact encounter at this level
        foreach (SpotLevel def in LevelData)
        {
            if (def.Level == currentLevel && !string.IsNullOrEmpty(def.EncounterActionId))
                return new() { def.EncounterActionId };
        }

        List<string> actions = new List<string>();
        foreach (SpotLevel def in LevelData)
        {
            if (def.Level <= currentLevel)
            {
                foreach (string rem in def.RemovedActionIds)
                    actions.Remove(rem);
                foreach (string add in def.AddedActionIds)
                    if (!actions.Contains(add)) actions.Add(add);
            }
        }
        return actions;
    }

    public void RegisterActionDefinition(string actionId)
    {
        SpotLevel spotLevel = GetCurrentSpotLevel();
        spotLevel.AddedActionIds.Add(actionId);
    }

    private SpotLevel GetCurrentSpotLevel()
    {
        SpotLevel spotLevel = null;

        if (LevelData.FirstOrDefault(x =>
        {
            return x.Level == CurrentLevel;
        }) is SpotLevel level)
        {
            spotLevel = level;
        }
        else
        {
            spotLevel = new SpotLevel() { Level = CurrentLevel };
            LevelData.Add(spotLevel);
        }

        return spotLevel;
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
