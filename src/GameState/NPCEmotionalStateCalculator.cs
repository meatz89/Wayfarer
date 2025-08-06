using System;
using System.Linq;

/// <summary>
/// Emotional states that emerge from letter properties in the queue.
/// These states drive verb costs, available options, and narrative generation.
/// </summary>
public enum NPCEmotionalState
{
    DESPERATE,    // Urgent need (TTL < 2 or SAFETY stakes), easier interaction
    HOSTILE,      // Angry (has overdue letters), harder interaction
    CALCULATING,  // Balanced pressure, normal interaction
    WITHDRAWN     // No engagement (no letters), limited options
}

/// <summary>
/// Calculates NPC emotional states based on their letters in the queue.
/// This is the core of the literary UI system - the queue IS the story.
/// </summary>
public class NPCEmotionalStateCalculator
{
    private readonly LetterQueueManager _letterQueueManager;

    public NPCEmotionalStateCalculator(LetterQueueManager letterQueueManager)
    {
        _letterQueueManager = letterQueueManager;
    }

    /// <summary>
    /// Calculate an NPC's emotional state based on their letters in the queue
    /// </summary>
    public NPCEmotionalState CalculateState(NPC npc)
    {
        if (npc == null) return NPCEmotionalState.WITHDRAWN;

        var queue = _letterQueueManager.GetActiveLetters();
        var theirLetters = queue.Where(l => 
            l.SenderId == npc.ID || l.SenderName == npc.Name).ToList();

        if (!theirLetters.Any())
        {
            return NPCEmotionalState.WITHDRAWN;
        }

        // Check for overdue letters first
        if (theirLetters.Any(l => l.DeadlineInDays <= 0))
        {
            return NPCEmotionalState.HOSTILE;
        }

        // Check for urgent letters or safety stakes
        var mostUrgent = theirLetters.OrderBy(l => l.DeadlineInDays).First();
        if (mostUrgent.DeadlineInDays < 2 || mostUrgent.Stakes == StakeType.SAFETY)
        {
            return NPCEmotionalState.DESPERATE;
        }

        // Default to calculating for normal pressure
        return NPCEmotionalState.CALCULATING;
    }

    /// <summary>
    /// Generate body language based on emotional state and stakes
    /// </summary>
    public string GenerateBodyLanguage(NPCEmotionalState state, StakeType stakes)
    {
        return (state, stakes) switch
        {
            (NPCEmotionalState.DESPERATE, StakeType.REPUTATION) => 
                "fingers worrying their shawl, eyes darting to the door",
            (NPCEmotionalState.DESPERATE, StakeType.WEALTH) => 
                "counting coins nervously, sweat beading on their brow",
            (NPCEmotionalState.DESPERATE, StakeType.SAFETY) => 
                "glancing over their shoulder, voice barely a whisper",
            (NPCEmotionalState.DESPERATE, StakeType.SECRET) => 
                "leaning close, hands trembling slightly",
            
            (NPCEmotionalState.HOSTILE, StakeType.REPUTATION) => 
                "chin raised, eyes cold with disdain",
            (NPCEmotionalState.HOSTILE, StakeType.WEALTH) => 
                "arms crossed tight, tapping their foot impatiently",
            (NPCEmotionalState.HOSTILE, StakeType.SAFETY) => 
                "hand near their weapon, stance aggressive",
            (NPCEmotionalState.HOSTILE, StakeType.SECRET) => 
                "lips pursed, suspicious glare boring into you",
            
            (NPCEmotionalState.CALCULATING, StakeType.REPUTATION) => 
                "measured breathing, each word carefully chosen",
            (NPCEmotionalState.CALCULATING, StakeType.WEALTH) => 
                "fingers steepled, assessing your worth",
            (NPCEmotionalState.CALCULATING, StakeType.SAFETY) => 
                "watchful stance, ready for anything",
            (NPCEmotionalState.CALCULATING, StakeType.SECRET) => 
                "thoughtful pause, weighing what to reveal",
            
            (NPCEmotionalState.WITHDRAWN, _) => 
                "distant gaze, minimal acknowledgment of your presence",
            
            _ => "watching you with guarded interest"
        };
    }

    /// <summary>
    /// Modify attention costs based on NPC emotional state
    /// </summary>
    public int GetAttentionCostModifier(NPCEmotionalState state)
    {
        return state switch
        {
            NPCEmotionalState.DESPERATE => -1,  // Easier to interact with
            NPCEmotionalState.HOSTILE => 1,     // Harder to interact with
            NPCEmotionalState.CALCULATING => 0, // Normal cost
            NPCEmotionalState.WITHDRAWN => 0,   // Normal cost but fewer options
            _ => 0
        };
    }

    /// <summary>
    /// Calculate the narrative pressure level (for peripheral awareness)
    /// </summary>
    public string GetQueuePressureNarrative(LetterQueue queue)
    {
        if (queue == null || !queue.Letters.Any()) return null;

        var mostUrgent = queue.Letters
            .Where(l => l.State == LetterState.Accepted)
            .OrderBy(l => l.DeadlineInDays)
            .FirstOrDefault();

        if (mostUrgent == null) return null;

        if (mostUrgent.DeadlineInDays <= 1)
        {
            return $"⚡ {mostUrgent.SenderName}'s letter burns in your satchel";
        }
        else if (mostUrgent.DeadlineInDays <= 3)
        {
            return $"The weight of {mostUrgent.SenderName}'s letter presses against your ribs";
        }
        else if (queue.Letters.Count(l => l.State == LetterState.Accepted) > 6)
        {
            return "Your satchel strains with accumulated correspondence";
        }

        return null;
    }
}