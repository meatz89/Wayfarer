
public class ActionDefinition
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string LocationSpotId { get; set; }
    public int SilverCost { get; set; }
    public SkillCategories RefreshCardType { get; set; }
    public int StaminaCost { get; set; }
    public int ConcentrationCost { get; set; }

    // Time windows
    public List<TimeBlocks> CurrentTimeBlocks { get; set; } = new List<TimeBlocks>();

    // Tag Resonance System
    public List<string> ContextTags { get; set; } = new List<string>();
    public List<string> DomainTags { get; set; } = new List<string>();

    // Movement
    public string MoveToLocation { get; set; }
    public string MoveToLocationSpot { get; set; }

    // === CATEGORICAL SYSTEM PROPERTIES ===
    // Physical requirements
    public PhysicalDemand PhysicalDemand { get; set; } = PhysicalDemand.None;

    // Item requirements
    public List<ItemCategory> ItemRequirements { get; set; } = new List<ItemCategory>();

    // Knowledge and skill requirements
    public KnowledgeRequirement KnowledgeRequirement { get; set; } = KnowledgeRequirement.None;


    // Time investment category
    public TimeInvestment TimeInvestment { get; set; } = TimeInvestment.Standard;

    // Effects produced by this action
    public List<EffectCategory> EffectCategories { get; set; } = new List<EffectCategory>();



    public ActionDefinition(string id, string name, string spotId)
    {
        Id = id;
        Name = name;
        LocationSpotId = spotId;
    }
}