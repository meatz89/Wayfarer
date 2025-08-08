using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Calculates atmospheric effects based on NPC presence at locations.
/// No tags, no stored state - pure functional calculation.
/// </summary>
public class AtmosphereCalculator
{
    private readonly NPCRepository _npcRepository;
    private readonly ITimeManager _timeManager;

    public AtmosphereCalculator(NPCRepository npcRepository, ITimeManager timeManager)
    {
        _npcRepository = npcRepository;
        _timeManager = timeManager;
    }

    /// <summary>
    /// Calculate atmosphere effect from current NPCs at a location
    /// </summary>
    public AtmosphereEffect CalculateForLocation(string locationSpotId)
    {
        var currentTime = _timeManager.GetCurrentTimeBlock();
        var npcsPresent = _npcRepository.GetNPCsForLocationSpotAndTime(locationSpotId, currentTime);
        
        return CalculateFromNPCPresence(npcsPresent);
    }

    /// <summary>
    /// Calculate atmosphere effect from a specific set of NPCs
    /// </summary>
    public AtmosphereEffect CalculateFromNPCPresence(List<NPC> npcs)
    {
        if (npcs == null || !npcs.Any())
        {
            return new AtmosphereEffect
            {
                AttentionModifier = 0,
                ConversationTimeModifier = 0,
                Description = "The space is quiet and empty"
            };
        }

        int npcCount = npcs.Count;
        
        // Simple, predictable rules based on NPC count
        int attentionModifier = npcCount switch
        {
            0 => 0,     // Empty: neutral
            1 => 0,     // One person: focused conversation
            2 => -1,    // Two people: some distraction
            _ => -2     // Three or more: very distracting
        };

        // Time modifier based on crowd
        int timeModifier = npcCount switch
        {
            0 => 0,      // Empty: normal time
            1 => 5,      // One person: can take time
            2 => 0,      // Two people: normal pace
            _ => -5      // Crowded: rushed conversations
        };

        // Generate natural description
        string description = GenerateDescription(npcs);

        return new AtmosphereEffect
        {
            AttentionModifier = attentionModifier,
            ConversationTimeModifier = timeModifier,
            Description = description
        };
    }

    private string GenerateDescription(List<NPC> npcs)
    {
        int count = npcs.Count;
        
        if (count == 0)
            return "The space is quiet and empty";
        
        if (count == 1)
        {
            var npc = npcs.First();
            return $"{npc.Name} is here, the space feels calm";
        }
        
        if (count == 2)
            return "A few people are present, creating a gentle murmur";
        
        // 3 or more
        return "The space bustles with activity and conversation";
    }
}

/// <summary>
/// Represents the atmospheric effect at a location
/// </summary>
public struct AtmosphereEffect
{
    /// <summary>
    /// Modifier to attention points (-2 to +2)
    /// </summary>
    public int AttentionModifier { get; set; }
    
    /// <summary>
    /// Modifier to conversation time in minutes
    /// </summary>
    public int ConversationTimeModifier { get; set; }
    
    /// <summary>
    /// Natural language description for UI
    /// </summary>
    public string Description { get; set; }
}