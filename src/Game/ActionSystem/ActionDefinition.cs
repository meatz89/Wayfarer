using Wayfarer.Game.ActionSystem;

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

    // Tool and equipment requirements
    public List<ToolCategory> ToolRequirements { get; set; } = new List<ToolCategory>();
    public List<EquipmentCategory> EquipmentRequirements { get; set; } = new List<EquipmentCategory>();

    // Knowledge and skill requirements
    public KnowledgeRequirement KnowledgeRequirement { get; set; } = KnowledgeRequirement.None;

    // Information requirements
    public List<InformationRequirementData> InformationRequirements { get; set; } = new List<InformationRequirementData>();

    // Time investment category
    public TimeInvestment TimeInvestment { get; set; } = TimeInvestment.Standard;

    // Effects produced by this action
    public List<EffectCategory> EffectCategories { get; set; } = new List<EffectCategory>();

    // Information effects provided by this action
    public List<InformationEffectData> InformationEffects { get; set; } = new List<InformationEffectData>();

    // Contract discovery effects provided by this action
    public List<ContractDiscoveryEffectData> ContractDiscoveryEffects { get; set; } = new List<ContractDiscoveryEffectData>();

    public ActionDefinition(string id, string name, string spotId)
    {
        Id = id;
        Name = name;
        LocationSpotId = spotId;
    }
}