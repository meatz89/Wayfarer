public record LocationAction
{
    public string ActionId { get; set; }
    public string Name { get; set; }
    public SkillCategories RequiredCardType { get; set; }
    public ActionExecutionTypes ActionExecutionType { get; set; } = ActionExecutionTypes.Encounter;

    public string ObjectiveDescription { get; set; }
    public string DestinationLocation { get; set; }
    public string DestinationLocationSpot { get; set; }

    public string LocationId { get; set; }
    public string LocationSpotId { get; set; }

    public List<IRequirement> Requirements { get; set; } = new();
    public int ActionPointCost { get; set; }

    public int Difficulty { get; set; } = 1;
    public List<ApproachDefinition> Approaches { get; set; } = new List<ApproachDefinition>();
    public Contract Opportunity { get; set; }
    public int Complexity { get; set; }

}