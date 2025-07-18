using System.Collections.Generic;

/// <summary>
/// Data Transfer Object for deserializing action definition data from JSON.
/// Maps to the structure in actions.json.
/// </summary>
public class ActionDefinitionDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string LocationSpotId { get; set; }
    public int SilverCost { get; set; }
    public string RefreshCardType { get; set; }
    public int StaminaCost { get; set; }
    public int ConcentrationCost { get; set; }
    
    // Time windows
    public List<string> CurrentTimeBlocks { get; set; } = new List<string>();
    
    // Tag Resonance System
    public List<string> ContextTags { get; set; } = new List<string>();
    public List<string> DomainTags { get; set; } = new List<string>();
    
    // Movement
    public string MoveToLocation { get; set; }
    public string MoveToLocationSpot { get; set; }
    
    // Categorical System Properties
    public string PhysicalDemand { get; set; }
    public List<string> ItemRequirements { get; set; } = new List<string>();
    public string KnowledgeRequirement { get; set; }
    public string TimeInvestment { get; set; }
    public List<string> EffectCategories { get; set; } = new List<string>();
}