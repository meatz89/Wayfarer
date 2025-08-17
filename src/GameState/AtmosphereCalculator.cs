using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.GameState;

/// <summary>
/// Calculates atmospheric effects based on NPC presence at locations.
/// Enhanced to include recent events for environmental storytelling.
/// </summary>
public class AtmosphereCalculator
{
    private readonly NPCRepository _npcRepository;
    private readonly ITimeManager _timeManager;
    private readonly WorldMemorySystem _worldMemory;

    public AtmosphereCalculator(
        NPCRepository npcRepository,
        ITimeManager timeManager,
        WorldMemorySystem worldMemory = null)
    {
        _npcRepository = npcRepository;
        _timeManager = timeManager;
        _worldMemory = worldMemory; // Optional for backward compatibility
    }

    /// <summary>
    /// Calculate atmosphere effect from current NPCs at a location
    /// </summary>
    public AtmosphereEffect CalculateForLocation(string locationSpotId)
    {
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        List<NPC> npcsPresent = _npcRepository.GetNPCsForLocationSpotAndTime(locationSpotId, currentTime);

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

        // Check for recent events to color the atmosphere
        if (_worldMemory != null)
        {
            WorldEvent recentEvent = _worldMemory.GetMostRecentEvent();
            if (recentEvent != null &&
                (DateTime.Now - recentEvent.Timestamp).TotalMinutes < 30)
            {
                return GenerateEventColoredDescription(npcs, recentEvent);
            }
        }

        // Default descriptions
        if (count == 0)
            return "The space is quiet and empty";

        if (count == 1)
        {
            NPC npc = npcs.First();
            return $"{npc.Name} is here, the space feels calm";
        }

        if (count == 2)
            return "A few people are present, creating a gentle murmur";

        // 3 or more
        return "The space bustles with activity and conversation";
    }

    private string GenerateEventColoredDescription(List<NPC> npcs, WorldEvent recentEvent)
    {
        int count = npcs.Count;

        // Color the atmosphere based on recent events
        if (recentEvent.Type == WorldEventType.DeadlineMissed ||
            recentEvent.Type == WorldEventType.ConfrontationOccurred)
        {
            if (count == 0)
                return "The space feels heavy with unspoken disappointment";
            if (count == 1)
                return $"{npcs.First().Name} is here, the air thick with tension";
            return "People exchange knowing glances, the atmosphere tense";
        }

        if (recentEvent.Type == WorldEventType.LetterDelivered ||
            recentEvent.Type == WorldEventType.ObligationFulfilled)
        {
            if (count == 0)
                return "The space holds a lingering warmth of satisfaction";
            if (count == 1)
                return $"{npcs.First().Name} is here, a sense of quiet respect in the air";
            return "There's a subtle warmth in how people acknowledge you";
        }

        // Default to normal description if event doesn't affect atmosphere
        return GenerateDescription(npcs);
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