

public class LocationSpot
{
    public string Name { get; init; }
    public string LocationName { get; init; }
    public string Description { get; set; }
    public string InteractionDescription { get; set; }

    public Population? Population { get; set; } = Population.Quiet;
    public Atmosphere? Atmosphere { get; set; } = Atmosphere.Calm;
    public Physical? Physical { get; set; } = Physical.Confined;
    public Illumination? Illumination { get; set; } = Illumination.Bright;
    public bool PlayerKnowledge { get; set; }

    // Progression
    public int CurrentSpotXP { get; set; } = 0;
    public List<SpotLevel> SpotLevels { get; set; } = new List<SpotLevel>();
    public int CurrentLevel { get; set; } = 1;

    // Requirements
    public Dictionary<string, int> SkillRequirements { get; set; }
    public Dictionary<string, int> RelationshipRequirements { get; set; }
    public int ReputationRequirement { get; set; }

    public LocationSpotTypes LocationSpotType = LocationSpotTypes.Location;
    public string CharacterName { get; set; }

    public bool IsClosed { get; set; } = true;

    public LocationSpot(string name, string locationName)
    {
        Name = name;
        LocationName = locationName;
    }


    /// <summary>
    /// Returns available actions for the given spot-level.
    /// If an EncounterActionId exists at that exact level, only that action is returned.
    /// Otherwise, applies all Added/Removed lists for definitions <= currentLevel.
    /// </summary>
    public IEnumerable<string> GetActionsForLevel(int currentLevel)
    {
        // Check for exact encounter at this level
        foreach (SpotLevel def in SpotLevels)
        {
            if (def.Level == currentLevel && !string.IsNullOrEmpty(def.EncounterActionId))
                return new[] { def.EncounterActionId };
        }

        List<string> actions = new List<string>();
        foreach (SpotLevel def in SpotLevels)
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

        if (SpotLevels.FirstOrDefault(x =>
        {
            return x.Level == CurrentLevel;
        }) is SpotLevel level)
        {
            spotLevel = level;
        }
        else
        {
            spotLevel = new SpotLevel() { Level = CurrentLevel };
            SpotLevels.Add(spotLevel);
        }

        return spotLevel;
    }

    internal void RegisterActionDefinitions(List<string> actionIds)
    {
        foreach (string actionId in actionIds)
        {
            RegisterActionDefinition(actionId);
        }
    }
}
