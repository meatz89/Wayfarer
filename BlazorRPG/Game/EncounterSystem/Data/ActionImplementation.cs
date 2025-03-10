public record ActionImplementation
{
    public string Name { get; set; }
    public string Description { get; internal set; }
    public BasicActionTypes ActionType { get; set; }
    public bool IsEncounterAction { get; internal set; }
    public List<Requirement> Requirements { get; set; } = new();
    public List<TimeWindows> TimeWindows { get; set; } = new();
    public LocationArchetypes LocationArchetype { get; set; } = new();
    public CrowdDensity CrowdDensity { get; set; } = new();
    public OpportunityTypes Opportunity { get; set; } = new();
    public List<Outcome> EnergyCosts { get; set; }
    public List<Outcome> Costs { get; set; }
    public List<Outcome> Rewards { get; set; }

    public bool CanExecute(GameState gameState)
    {
        return Requirements.All(r => r.IsSatisfied(gameState));
    }

}
