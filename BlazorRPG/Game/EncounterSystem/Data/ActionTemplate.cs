public class ActionTemplate
{
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

    // If Encounter Action
    public string EncounterTemplateName { get; set; }

    // If Basic Action
    public List<Requirement> Requirements { get; }
    public List<Outcome> Energy { get; }
    public List<Outcome> Costs { get; }
    public List<Outcome> Rewards { get; }
    public int CoinCost { get; set; }

    public ActionTemplate()
    {

    }

}

public enum ActionTypes
{
    Basic, Encounter
}