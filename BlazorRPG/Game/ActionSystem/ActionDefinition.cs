public class ActionDefinition
{
    public string Id { get; set; }
    public int Difficulty { get; set; }
    public int EncounterChance { get; set; }
    public bool IsRepeatable { get; set; }

    public List<TimeWindows> AvailableWindows { get; set; }
    public string Description { get; set; }
    public string LocationName { get; set; }
    public string LocationSpotName { get; set; }
    public string MoveToLocation { get; set; }
    public string MoveToLocationSpot { get; set; }

    public List<Requirement> Requirements { get; set; }
    public List<Outcome> Costs { get; set; }
    public List<Outcome> Yields { get; set; }

    public string Goal { get; set; }
    public string Complication { get; set; }
    public EncounterTypes EncounterType { get; set; }
    public int TimeCost { get; internal set; }
    public int EnergyCost { get; internal set; }

    public ActionDefinition()
    {
    }

    public ActionDefinition(string actionId, string name, int difficulty, int encounterChance, EncounterTypes encounterType, bool isRepeatable)
    {
        Id = name;
        Difficulty = difficulty;
        EncounterChance = encounterChance;
        EncounterType = encounterType;
        IsRepeatable = isRepeatable;
    }
}
