public class ActionDefinition
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Goal { get; set; }
    public string Complication { get; set; }
    public int SpotXp { get; set; }

    public int Difficulty { get; set; } = 1;
    public string MoveToLocation { get; set; }
    public string MoveToLocationSpot { get; set; }

    public EncounterTypes EncounterType { get; set; }
    public bool IsOneTimeEncounter { get; set; }

    public List<TimeWindow> TimeWindows { get; set; }
    public int EnergyCost { get; set; }
    public int CoinCost { get; set; }
    public int ConfidenceCost { get; set; }
    public int ConcentrationCost { get; set; }
    public int TimeCost { get; set; }

    public int CoinGain { get; set; }
    public int RestoresEnergy { get; set; }
    public List<RelationshipGain> RelationshipGains { get; set; } = new();
    public int RestoresHealth { get; set; }
    public int RestoresConcentration { get; set; }
    public int RestoresConfidence { get; set; }

    public ActionDefinition(string id, string name)
    {
        Id = id;
        Name = name;
    }
}
