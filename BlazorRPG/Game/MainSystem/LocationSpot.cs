public class LocationSpot
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string LocationName { get; set; }
    public List<string> BaseActionIds { get; set; } = new();
    public List<string> UnlockableActionIds { get; set; } = new();
    public List<string> ResidentCharacterIds { get; set; } = new List<string>();
    public List<string> AssociatedOpportunityIds { get; set; } = new List<string>();

    public string InteractionDescription { get; set; }

    public Population? Population { get; set; } = Population.Quiet;
    public Atmosphere? Atmosphere { get; set; } = Atmosphere.Calm;
    public Physical? Physical { get; set; } = Physical.Confined;
    public Illumination? Illumination { get; set; } = Illumination.Bright;
    public bool PlayerKnowledge { get; set; }

    // Progression
    public int CurrentSpotLevel { get; set; }
    public int CurrentSpotXP { get; set; }
    public int[] LevelThresholds { get; set; }

    // Requirements
    public Dictionary<string, int> SkillRequirements { get; set; }
    public Dictionary<string, int> RelationshipRequirements { get; set; }
    public int ReputationRequirement { get; set; }

    public bool IsClosed { get; set; } = true;
    public LocationSpotTypes LocationSpotType = LocationSpotTypes.Location;
    public string CharacterName { get; set; }

    // Update based on time
    public void OnTimeChanged(TimeWindows newTimeWindow)
    {
    }

}
