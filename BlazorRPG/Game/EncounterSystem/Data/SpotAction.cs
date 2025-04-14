public class SpotAction
{
    public string ActionId { get; set; }
    public string EncounterId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Goal { get; set; }
    public string Complication { get; set; }
    public BasicActionTypes BasicActionType { get; set; }
    public ActionTypes ActionType { get; set; }
    public string LocationName { get; set; }
    public string LocationSpotName { get; set; }
    public int Difficulty { get; set; }
    public bool IsEncounterAction => ActionType == ActionTypes.Encounter;
    public List<Requirement> Requirements { get; set; }
    public List<Outcome> Energy { get; set; }
    public List<Outcome> Costs { get; set; }
    public List<Outcome> Rewards { get; set; }
    public bool IsRepeatable { get; set; }
    public string LocationSpotTarget { get; set; }
}

public enum ActionTypes
{
    Basic, Encounter
}