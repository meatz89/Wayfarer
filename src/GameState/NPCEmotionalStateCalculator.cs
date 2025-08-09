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

        Console.WriteLine($"[NPCEmotionalStateCalculator] Calculating state for {npc.Name} (ID: {npc.ID})");
        Console.WriteLine($"[NPCEmotionalStateCalculator] Found {theirLetters.Count} letters for this NPC");
        
        foreach (var letter in theirLetters)
        {
            Console.WriteLine($"  - Letter: {letter.Description}, Deadline: {letter.DeadlineInHours}h, Stakes: {letter.Stakes}");
        }

        if (!theirLetters.Any())
        {
            Console.WriteLine($"[NPCEmotionalStateCalculator] No letters found - returning WITHDRAWN");
            return NPCEmotionalState.WITHDRAWN;
        }

        // Check for overdue letters first
        if (theirLetters.Any(l => l.DeadlineInHours <= 0))
        {
            Console.WriteLine($"[NPCEmotionalStateCalculator] Found overdue letter - returning HOSTILE");
            return NPCEmotionalState.HOSTILE;
        }

        // Check for urgent letters or safety stakes
        var mostUrgent = theirLetters.OrderBy(l => l.DeadlineInHours).First();
        Console.WriteLine($"[NPCEmotionalStateCalculator] Most urgent letter: {mostUrgent.Description}, Deadline: {mostUrgent.DeadlineInHours}h, Stakes: {mostUrgent.Stakes}");
        
        if (mostUrgent.DeadlineInHours < 2 || mostUrgent.Stakes == StakeType.SAFETY)
        {
            Console.WriteLine($"[NPCEmotionalStateCalculator] Urgent or safety stakes - returning DESPERATE");
            return NPCEmotionalState.DESPERATE;
        }

        // Default to calculating for normal pressure
        Console.WriteLine($"[NPCEmotionalStateCalculator] Normal pressure - returning CALCULATING");
        return NPCEmotionalState.CALCULATING;
    }

    /// <summary>
    /// Get emoji representation of emotional state
    /// </summary>
    public string GetMoodEmoji(NPCEmotionalState state)
    {
        return state switch
        {
            NPCEmotionalState.DESPERATE => "😰",
            NPCEmotionalState.HOSTILE => "😠",
            NPCEmotionalState.CALCULATING => "🤔",
            NPCEmotionalState.WITHDRAWN => "😐",
            _ => "😊"
        };
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
            .OrderBy(l => l.DeadlineInHours)
            .FirstOrDefault();

        if (mostUrgent == null) return null;

        if (mostUrgent.DeadlineInHours <= 1)
        {
            return $"⚡ {mostUrgent.SenderName}'s letter burns in your satchel";
        }
        else if (mostUrgent.DeadlineInHours <= 3)
        {
            return $"The weight of {mostUrgent.SenderName}'s letter presses against your ribs";
        }
        else if (queue.Letters.Count(l => l.State == LetterState.Accepted) > 6)
        {
            return "Your satchel strains with accumulated correspondence";
        }

        return null;
    }

    /// <summary>
    /// Generate NPC dialogue from their letter properties and emotional state
    /// </summary>
    public string GenerateNPCDialogue(
        NPC npc,
        NPCEmotionalState state,
        Letter mostUrgentLetter,
        SceneContext context)
    {
        if (mostUrgentLetter == null)
        {
            return GenerateNoLetterDialogue(npc, state);
        }
        
        // Build dialogue from letter properties
        var stakesHint = mostUrgentLetter.GetStakesHint();
        var urgency = GetUrgencyDescription(mostUrgentLetter.DeadlineInHours);
        
        return (state, mostUrgentLetter.Stakes) switch
        {
            (NPCEmotionalState.DESPERATE, StakeType.REPUTATION) =>
                $"The letter contains {stakesHint}. {urgency} If this isn't delivered, my standing will be ruined.",
                
            (NPCEmotionalState.DESPERATE, StakeType.SAFETY) =>
                $"This is {stakesHint}! {urgency} Lives depend on this reaching {mostUrgentLetter.RecipientName}!",
                
            (NPCEmotionalState.DESPERATE, StakeType.WEALTH) =>
                $"It's {stakesHint}. {urgency} Everything I've worked for depends on this delivery.",
                
            (NPCEmotionalState.DESPERATE, StakeType.SECRET) =>
                $"The letter... it contains {stakesHint}. {urgency} If the wrong people learn of this...",
                
            (NPCEmotionalState.HOSTILE, StakeType.REPUTATION) =>
                $"You still haven't delivered my letter about {stakesHint}. {urgency} This negligence is unacceptable.",
                
            (NPCEmotionalState.HOSTILE, StakeType.SAFETY) =>
                $"People are in danger because of your delays! The {stakesHint} I sent - {urgency}",
                
            (NPCEmotionalState.HOSTILE, StakeType.WEALTH) =>
                $"My {stakesHint} sits undelivered while you waste time. {urgency} The losses mount daily.",
                
            (NPCEmotionalState.HOSTILE, StakeType.SECRET) =>
                $"That {stakesHint} in your satchel - {urgency} Do you understand what you're risking?",
                
            (NPCEmotionalState.CALCULATING, StakeType.WEALTH) =>
                $"My letter concerns {stakesHint}. {urgency} There may be profit in swift delivery.",
                
            (NPCEmotionalState.CALCULATING, StakeType.REPUTATION) =>
                $"I've entrusted you with {stakesHint}. {urgency} Handle it wisely.",
                
            (NPCEmotionalState.CALCULATING, StakeType.SAFETY) =>
                $"The {stakesHint} needs careful handling. {urgency} Discretion is paramount.",
                
            (NPCEmotionalState.CALCULATING, StakeType.SECRET) =>
                $"About that {stakesHint} you're carrying... {urgency} Some things are worth more than coin.",
                
            _ => $"I need this letter delivered. It's about {stakesHint}. {urgency}"
        };
    }
    
    private string GenerateNoLetterDialogue(NPC npc, NPCEmotionalState state)
    {
        return state switch
        {
            NPCEmotionalState.WITHDRAWN => $"{npc.Name} barely acknowledges your presence.",
            NPCEmotionalState.CALCULATING => $"{npc.Name} watches you with measured interest.",
            NPCEmotionalState.HOSTILE => $"{npc.Name} glares at you with barely concealed anger.",
            _ => $"{npc.Name} seems preoccupied."
        };
    }
    
    private string GetUrgencyDescription(int deadlineDays)
    {
        return deadlineDays switch
        {
            <= 0 => "It's already overdue!",
            1 => "It must arrive TODAY.",
            2 => "Tomorrow is the absolute latest.",
            <= 3 => "Time is running short.",
            <= 5 => "There's still time, but don't delay.",
            _ => "The deadline approaches."
        };
    }
}