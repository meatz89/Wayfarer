public record ActionImplementation
{
    public string ActionId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Goal { get; set; }
    public string Complication { get; set; }
    public BasicActionTypes BasicActionType { get; set; }
    public ActionTypes ActionType { get; set; }
    public List<Requirement> Requirements { get; set; } = new();
    public List<TimeWindows> TimeWindows { get; set; } = new();
    public List<Outcome> EnergyCosts { get; set; }
    public List<Outcome> Costs { get; set; }
    public List<Outcome> Rewards { get; set; }
    public EncounterTemplate EncounterTemplate { get; set; }

    public bool IsRepeatable { get; set; } = false;
    public string CompletionId { get; set; } 
    public ActionExperienceTypes ExperienceType { get; set; } = ActionExperienceTypes.Normal;

    public string CurrentLocation { get; set; }
    public int TimeCostHours { get; set; } = 1;

    public string DestinationLocation { get; set; }
    public string DestinationLocationSpot { get; set; }

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

public enum ActionExperienceTypes
{
    Normal,           // Standard action
    StoryProgress,    // Advances main story
    Discovery,        // Reveals new information/locations
    ResourceManagement // Basic resource action (rest, trade, etc)
}