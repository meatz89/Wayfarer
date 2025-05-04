public class ActionDefinition
{
    // Basic properties
    public string Id { get; set; }
    public string Name { get; set; }
    public string SpotId { get; set; }
    public string Description { get; set; }
    public ActionTypes ActionType { get; set; }

    // Resource costs
    public int CoinCost { get; set; }
    public int EnergyCost { get; set; }
    public int HealthCost { get; set; }
    public int ConfidenceCost { get; set; }
    public int ConcentrationCost { get; set; }
    public string TimeWindowCost { get; set; } = string.Empty;

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
    public List<SkillRequirement> SkillRequirements { get; set; } = new();

    // Outcome effects
    public int SpotXp { get; set; }

    // Encounter details
    public bool IsOneTimeEncounter { get; set; }
    public string Goal { get; set; }
    public string Complication { get; set; }
    public EncounterTypes EncounterType { get; set; } = EncounterTypes.Rapport;
    public int Difficulty { get; set; }
    public string MoveToLocation { get; set; }
    public string MoveToLocationSpot { get; set; }

    public ActionDefinition(string id, string name, string spotId)
    {
        Id = id;
        Name = name;
        SpotId = spotId;
    }
}