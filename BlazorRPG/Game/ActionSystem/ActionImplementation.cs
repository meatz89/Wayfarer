public record ActionImplementation
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public string Goal { get; set; }
    public string Complication { get; set; }

    public string DestinationLocation { get; set; }
    public string DestinationLocationSpot { get; set; }

    public string LocationId { get; set; }
    public string LocationSpotId { get; set; }
    public int SpotXp { get; set; }

    public EncounterTemplate EncounterTemplate { get; set; }

    public List<IRequirement> Requirements { get; set; } = new();
    public List<Outcome> Costs { get; set; }
    public List<Outcome> Yields { get; set; }

    public ActionTypes ActionType { get; set; }
    public EncounterTypes EncounterType { get; set; }
    public int ActionPointCost { get; internal set; }

    public int Difficulty { get; set; } = 1;

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
            SpotName = LocationSpotId,
            LocationName = LocationId,
        };

        return context;
    }

    internal string GetExertionType()
    {
        return "None";
    }

    internal string GetMentalLoadType()
    {
        return "None";
    }

    internal string GetSocialImpactType()
    {
        return "None";
    }

    internal string GetRecoveryType()
    {
        return "None";
    }
}
