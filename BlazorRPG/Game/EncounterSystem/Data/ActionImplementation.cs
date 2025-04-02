public record ActionImplementation
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Goal { get; internal set; }
    public string Complication { get; internal set; }
    public BasicActionTypes BasicActionType { get; set; }
    public ActionTypes ActionType { get; set; }
    public List<Requirement> Requirements { get; set; } = new();
    public List<TimeWindows> TimeWindows { get; set; } = new();
    public List<Outcome> EnergyCosts { get; set; }
    public List<Outcome> Costs { get; set; }
    public List<Outcome> Rewards { get; set; }
    public EncounterTemplate EncounterTemplate { get; set; }

    public bool CanExecute(GameState gameState)
    {
        return Requirements.All(r => r.IsSatisfied(gameState));
    }

}
