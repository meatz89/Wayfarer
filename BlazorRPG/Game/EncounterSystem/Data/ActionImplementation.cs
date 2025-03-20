public record ActionImplementation
{
    public ActionNames Name { get; set; }
    public string Goal { get; internal set; }
    public string Complication { get; internal set; }
    public BasicActionTypes ActionType { get; set; }
    public bool IsEncounterAction { get; internal set; }
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
