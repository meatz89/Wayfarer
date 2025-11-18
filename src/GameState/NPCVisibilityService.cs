/// <summary>
/// Service that manages NPC visibility rules without creating circular dependencies.
/// Acts as a bridge between NPCRepository and systems that affect visibility.
/// HIGHLANDER: All methods accept NPC objects, not string IDs
/// </summary>
public class NPCVisibilityService
{
    private readonly List<INPCVisibilityRule> _visibilityRules = new List<INPCVisibilityRule>();

    /// <summary>
    /// Check if an NPC should be visible based on all registered rules
    /// HIGHLANDER: Accepts NPC object, not string ID
    /// </summary>
    public bool IsNPCVisible(NPC npc)
    {
        // If no rules are registered, all NPCs are visible
        if (!_visibilityRules.Any())
            return true;

        // All rules must allow visibility
        return _visibilityRules.All(rule => rule.IsNPCVisible(npc));
    }

    /// <summary>
    /// Filter a list of NPCs based on visibility rules
    /// </summary>
    public List<NPC> FilterVisibleNPCs(List<NPC> npcs)
    {
        if (!_visibilityRules.Any())
            return npcs;

        // HIGHLANDER: Pass NPC object directly, not npc.ID
        return npcs.Where(npc => IsNPCVisible(npc)).ToList();
    }
}

/// <summary>
/// Interface for systems that can affect NPC visibility
/// HIGHLANDER: Methods accept NPC objects, not string IDs
/// </summary>
public interface INPCVisibilityRule
{
    bool IsNPCVisible(NPC npc);
}