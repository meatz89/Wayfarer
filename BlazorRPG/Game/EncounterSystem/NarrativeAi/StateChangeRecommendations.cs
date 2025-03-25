// General world information for discovery context
// Context for developing specific entities
// Minimal location information from discovery
// Detailed entity information from development
// AI recommendations for state changes after encounters
public class StateChangeRecommendations
{
    public Dictionary<string, int> ResourceChanges { get; set; }
    public Dictionary<string, int> RelationshipChanges { get; set; }
    public List<string> SkillExperienceGained { get; set; }
    public List<string> SuggestedWorldEvents { get; set; }
}
