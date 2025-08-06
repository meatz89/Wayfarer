using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Service that manages NPC visibility rules without creating circular dependencies.
/// Acts as a bridge between NPCRepository and systems that affect visibility.
/// </summary>
public class NPCVisibilityService
{
    private readonly List<INPCVisibilityRule> _visibilityRules = new List<INPCVisibilityRule>();
    
    /// <summary>
    /// Register a visibility rule provider
    /// </summary>
    public void RegisterVisibilityRule(INPCVisibilityRule rule)
    {
        _visibilityRules.Add(rule);
    }
    
    /// <summary>
    /// Unregister a visibility rule provider
    /// </summary>
    public void UnregisterVisibilityRule(INPCVisibilityRule rule)
    {
        _visibilityRules.Remove(rule);
    }
    
    /// <summary>
    /// Check if an NPC should be visible based on all registered rules
    /// </summary>
    public bool IsNPCVisible(string npcId)
    {
        // If no rules are registered, all NPCs are visible
        if (!_visibilityRules.Any())
            return true;
            
        // All rules must allow visibility
        return _visibilityRules.All(rule => rule.IsNPCVisible(npcId));
    }
    
    /// <summary>
    /// Filter a list of NPCs based on visibility rules
    /// </summary>
    public List<NPC> FilterVisibleNPCs(List<NPC> npcs)
    {
        if (!_visibilityRules.Any())
            return npcs;
            
        return npcs.Where(npc => IsNPCVisible(npc.ID)).ToList();
    }
}

/// <summary>
/// Interface for systems that can affect NPC visibility
/// </summary>
public interface INPCVisibilityRule
{
    bool IsNPCVisible(string npcId);
}