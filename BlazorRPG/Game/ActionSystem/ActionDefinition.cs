public class ActionDefinition
{
    // Basic properties
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ActionTypes ActionType { get; set; }

    // Resource costs
    public int CoinCost { get; set; }
    public int EnergyCost { get; set; }
    public int HealthCost { get; set; }
    public int ConfidenceCost { get; set; }
    public int ConcentrationCost { get; set; }
    public string TimeWindowCost { get; set; }

    // Resource yields
    public int CoinGain { get; set; }
    public int RestoresEnergy { get; set; }
    public int RestoresHealth { get; set; }
    public int RestoresConcentration { get; set; }
    public int RestoresConfidence { get; set; }
    public List<RelationshipGain> RelationshipChanges { get; set; } = new();

    // Requirements
    public List<TimeWindow> TimeWindows { get; set; } = new List<TimeWindow>();
    public List<RelationshipRequirement> RelationshipRequirements { get; set; } = new();
    public List<ReputationRequirement> ReputationRequirements { get; internal set; } = new();
    public List<SkillRequirement> SkillRequirements { get; internal set; } = new();

    // Outcome effects
    public int SpotXp { get; set; }

    // Encounter details
    public bool IsOneTimeEncounter { get; set; }
    public string Goal { get; set; }
    public string Complication { get; set; }
    public EncounterTypes EncounterType { get; set; } = EncounterTypes.Social;
    public int Difficulty { get; internal set; }
    public string MoveToLocation { get; internal set; }
    public string MoveToLocationSpot { get; internal set; }

    public ActionDefinition(string id, string name)
    {
        Id = id;
        Name = name;
    }
}