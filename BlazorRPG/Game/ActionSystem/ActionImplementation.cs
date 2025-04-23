
public record ActionImplementation
{
    public string Id { get; set; }
    public string Description { get; set; }
    public string Goal { get; set; }
    public string Complication { get; set; }
    public EncounterTemplate EncounterTemplate { get; set; }

    public List<TimeWindows> TimeWindows { get; set; } = new();
    public List<Requirement> Requirements { get; set; } = new();
    public List<Outcome> Costs { get; set; }
    public List<Outcome> Yields { get; set; }

    public string CurrentLocation { get; set; }
    public string DestinationLocation { get; set; }
    public string DestinationLocationSpot { get; set; }

    public bool IsRepeatable { get; set; } = false;
    public int Difficulty { get; set; } = 1;

    public ActionTypes ActionType { get; set; }
    public int EncounterChance { get; set; }

    public string CompletionId { get; set; }
    public EncounterTypes EncounterType { get; internal set; }

    public int GetTimeCost()
    {
        TimeOutcome timeCost;
        var hasTimeCost = OutcomeProcessor.GetOfType<TimeOutcome>(Costs, out timeCost);

        if (hasTimeCost)
        {
            return timeCost.hours;
        }
        return 0;
    }

    public int GetEnergyCost()
    {
        EnergyOutcome energyCost;
        var hasEnergyCost = OutcomeProcessor.GetOfType<EnergyOutcome>(Costs, out energyCost);

        if (hasEnergyCost)
        {
            return energyCost.Amount;
        }
        return 0;
    }

    public bool IsCompleted(WorldState worldState)
    {
        if (IsRepeatable) return false;
        if (string.IsNullOrEmpty(CompletionId)) return false;

        return worldState.IsEncounterCompleted(CompletionId);
    }

    public bool CanExecute(GameState gameState)
    {
        return Requirements.All(r => r.IsSatisfied(gameState));
    }

}
