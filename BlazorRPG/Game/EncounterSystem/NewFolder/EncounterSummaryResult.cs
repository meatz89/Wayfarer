public class EncounterSummaryResult
{
    public EncounterOutcome OutcomeResult { get; set; } = new EncounterOutcome();
    public List<InventoryItem> InventoryChanges { get; set; } = new List<InventoryItem>();
    public ResourceChange ResourceChanges { get; set; } = new ResourceChange();
    public List<DiscoveredLocation> DiscoveredLocations { get; set; } = new List<DiscoveredLocation>();
    public List<NPC> NewNPCs { get; set; } = new List<NPC>();
    public List<RelationshipChange> RelationshipChanges { get; set; } = new List<RelationshipChange>();
    public List<Quest> Quests { get; set; } = new List<Quest>();
    public List<Job> Jobs { get; set; } = new List<Job>();
    public List<Rumor> Rumors { get; set; } = new List<Rumor>();
    public TimePassage TimePassage { get; set; } = new TimePassage();

    // Track parsing errors by section
    public Dictionary<string, string> ParseErrors { get; set; } = new Dictionary<string, string>();

    // Track which sections were successfully parsed
    public HashSet<string> SuccessfullyParsedSections { get; set; } = new HashSet<string>();
}
