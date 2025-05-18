public record ActionImplementation
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public string DestinationLocation { get; set; }
    public string DestinationLocationSpot { get; set; }

    public string LocationId { get; set; }
    public string LocationSpotId { get; set; }

    public List<IRequirement> Requirements { get; set; } = new();
    public List<Outcome> Costs { get; set; }
    public List<Outcome> Yields { get; set; }

    public ActionExecutionTypes ActionType { get; set; }
    public int ActionPointCost { get; set; }

    public int Difficulty { get; set; } = 1;
    public List<ApproachDefinition> Approaches { get; set; } = new List<ApproachDefinition>();
    public CommissionDefinition Commission { get; internal set; }
}
