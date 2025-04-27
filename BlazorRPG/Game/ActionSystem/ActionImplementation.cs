public record ActionImplementation
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public string Goal { get; set; }
    public string Complication { get; set; }

    public string DestinationLocation { get; set; }
    public string DestinationLocationSpot { get; set; }

    public string LocationName { get; set; }
    public string LocationSpotName { get; set; }
    public int SpotXp { get; set; }

    public EncounterTemplate EncounterTemplate { get; set; }

    public List<Requirement> Requirements { get; set; } = new();
    public List<Outcome> Costs { get; set; }
    public List<Outcome> Yields { get; set; }

    public int Difficulty { get; set; } = 1;

    public ActionTypes ActionType { get; set; }
    public EncounterTypes EncounterType { get; set; }

    public bool IsCompleted(WorldState worldState)
    {
        if (ActionType == ActionTypes.Basic) return false;
        if (string.IsNullOrEmpty(Id)) return false;

        return worldState.IsEncounterCompleted(Id);
    }

    public bool CanExecute(GameState gameState)
    {
        return Requirements.All(r =>
        {
            return r.IsMet(gameState);
        });
    }

    public ActionGenerationContext GetActionGenerationContext()
    {
        ActionGenerationContext context = new ActionGenerationContext
        {
            ActionId = Id,
            Goal = Goal,
            Complication = Complication,
            BasicActionType = EncounterType.ToString(),
            SpotName = LocationSpotName,
            LocationName = LocationName,
        };

        return context;
    }
}
