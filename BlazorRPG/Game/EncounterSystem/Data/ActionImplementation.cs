public record ActionImplementation
{
    public string ActionId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Goal { get; set; }
    public string Complication { get; set; }

    public List<TimeWindows> TimeWindows { get; set; } = new();
    public List<Requirement> Requirements { get; set; } = new();
    public List<Outcome> EnergyCosts { get; set; }
    public List<Outcome> Costs { get; set; }
    public List<Outcome> Rewards { get; set; }

    public string CurrentLocation { get; set; }
    public string DestinationLocation { get; set; }
    public string DestinationLocationSpot { get; set; }

    public BasicActionTypes BasicActionType { get; set; }
    public bool IsRepeatable { get; set; } = false;

    public ActionTypes ActionType { get; set; }
    public int EncounterChance { get; set; }
    public EncounterTemplate EncounterTemplate { get; set; }

    public string CompletionId { get; set; } 
    public int TimeCostHours { get; set; } = 1;


    public void ApplyTimeCost(GameState gameState)
    {
        // Currently just delegates to the existing method
        gameState.WorldState.CurrentTimeInHours =
            (gameState.WorldState.CurrentTimeInHours + TimeCostHours) % 24;

        // Update time window
        const int timeWindowsPerDay = 4;
        const int hoursPerTimeWindow = 6;
        int timeWindow = (gameState.WorldState.CurrentTimeInHours / hoursPerTimeWindow) % timeWindowsPerDay;

        gameState.WorldState.DetermineCurrentTimeWindow(timeWindow);
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
