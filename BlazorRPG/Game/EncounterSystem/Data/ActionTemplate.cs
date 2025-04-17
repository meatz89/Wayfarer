public class ActionTemplate
{
    public string ActionId { get; private set; }
    public string Name { get; private set; }
    public int Difficulty { get; private set; }
    public int EncounterChance { get; private set; }
    public BasicActionTypes BasicActionType { get; private set; }
    public bool IsRepeatable { get; private set; }

    public int TimeCostHours { get; set; } = 1;
    public List<TimeWindows> TimeWindows { get; set; }
    public string Description { get; set; }
    public string LocationName { get; set; }
    public string LocationSpotName { get; set; }
    public string MoveToLocation { get; set; }
    public string MoveToLocationSpot { get; set; }

    public List<Requirement> Requirements { get; set; }
    public List<Outcome> Energy { get; set; }
    public List<Outcome> Costs { get; set; }
    public List<Outcome> Rewards { get; set; }
    
    public string Goal { get; set; }
    public string Complication { get; set; }

    public ActionTemplate(string actionId, string name, int difficulty, int encounterChance, BasicActionTypes basicActionType, bool isRepeatable)
    {
        ActionId = actionId;
        Name = name;
        Difficulty = difficulty;
        EncounterChance = encounterChance;
        BasicActionType = basicActionType;
        IsRepeatable = isRepeatable;
    }
}
