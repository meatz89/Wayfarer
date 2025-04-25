
public record ActionImplementation
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public string Goal { get; set; }
    public string Complication { get; set; }

    public string DestinationLocation { get; set; }
    public string DestinationLocationSpot { get; set; }

    public string LocationName { get; internal set; }
    public string LocationSpotName { get; internal set; }
    public int SpotXp { get; internal set; }

    public EncounterTemplate EncounterTemplate { get; set; }

    public List<Requirement> Requirements { get; set; } = new();
    public List<Outcome> Costs { get; set; }
    public List<Outcome> Yields { get; set; }

    public int Difficulty { get; set; } = 1;

    public ActionTypes ActionType { get; set; }
    public EncounterTypes EncounterType { get; set; }

    public int GetTimeCost()
    {
        TimeOutcome timeCost;
        bool hasTimeCost = OutcomeProcessor.GetOfType<TimeOutcome>(Costs, out timeCost);

        if (hasTimeCost)
        {
            return timeCost.hours;
        }
        return 0;
    }

    public int GetEnergyCost()
    {
        EnergyOutcome energyCost;
        bool hasEnergyCost = OutcomeProcessor.GetOfType<EnergyOutcome>(Costs, out energyCost);

        if (hasEnergyCost)
        {
            return energyCost.Amount;
        }
        return 0;
    }

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

    public EncounterTemplate GetEncounterTemplate()
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

        EncounterTemplate encounterTemplate = WorldEncounterContent.GetDefaultTemplate();

        return encounterTemplate;
    }
}
