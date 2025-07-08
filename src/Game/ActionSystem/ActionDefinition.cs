public class ActionDefinition
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string LocationSpotId { get; set; }
    public int ActionPointCost { get; set; }
    public int SilverCost { get; set; }
    public SkillCategories RefreshCardType { get; set; }
    public int StaminaCost { get; set; }
    public int ConcentrationCost { get; set; }

    // Time windows
    public List<TimeBlocks> TimeWindows { get; set; } = new List<TimeBlocks>();

    // Tag Resonance System
    public List<string> ContextTags { get; set; } = new List<string>();
    public List<string> DomainTags { get; set; } = new List<string>();

    // Movement
    public string MoveToLocation { get; set; }
    public string MoveToLocationSpot { get; set; }

    public ActionDefinition(string id, string name, string spotId)
    {
        Id = id;
        Name = name;
        LocationSpotId = spotId;
    }
}