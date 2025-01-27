public record ActionImplementation
{
    public string Name { get; set; }
    public BasicActionTypes ActionType { get; set; }
    public List<Requirement> Requirements { get; set; } = new();
    public List<Outcome> EnergyCosts { get; set; }
    public List<TimeWindows> TimeSlots { get; set; } = new();
    public LocationArchetypes LocationArchetype { get; set; } = new();
    public CrowdDensity CrowdDensity { get; set; } = new();
    public LocationScale LocationScale { get; set; } = new();
    public List<LocationPropertyCondition> SpotAvailabilityConditions { get; set; } = new();
    public List<OutcomeCondition> OutcomeConditions { get; set; }

    public bool CanExecute(GameState gameState)
    {
        return Requirements.All(r => r.IsSatisfied(gameState));
    }

    public List<Outcome> FailureOutcomes
    {
        get
        {
            List<Outcome> outcomes = OutcomeConditions
                        .FirstOrDefault(x => x.EncounterResults == EncounterResults.EncounterFailure).Outcomes;
            return outcomes;
        }
    }

    public List<Outcome> SuccessOutcomes
    {
        get
        {
            List<Outcome> outcomes = OutcomeConditions
                        .FirstOrDefault(x => x.EncounterResults == EncounterResults.EncounterSuccess).Outcomes;
            return outcomes;
        }
    }
}
