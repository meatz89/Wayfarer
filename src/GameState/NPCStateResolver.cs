using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.GameState;

/// <summary>
/// Emotional states that emerge from letter properties in the queue.
/// These states drive verb costs, available options, and narrative generation.
/// </summary>
public enum NPCEmotionalState
{
    DESPERATE,    // Personal Safety + <6h deadline
    ANXIOUS,      // Personal Safety + >6h, Reputation + <12h, Secrets + <6h 
    CALCULATING,  // Reputation + >12h, Wealth always, Secrets + >6h
    HOSTILE,      // Overdue letters
    WITHDRAWN     // No letters in queue
}

/// <summary>
/// Resolves NPC emotional states using simple Stakes + Time formula.
/// This is the core of the literary UI system - emotional states emerge from game state.
/// </summary>
public class NPCStateResolver
{
    private readonly LetterQueueManager _letterQueueManager;
    private readonly ITimeManager _timeManager;

    public NPCStateResolver(
        LetterQueueManager letterQueueManager,
        ITimeManager timeManager = null)
    {
        _letterQueueManager = letterQueueManager;
        _timeManager = timeManager;
    }

    /// <summary>
    /// Resolve an NPC's emotional state using simple Stakes + Time formula
    /// </summary>
    public NPCEmotionalState CalculateState(NPC npc)
    {
        if (npc == null) return NPCEmotionalState.WITHDRAWN;

        Console.WriteLine($"[NPCStateResolver] Resolving state for {npc.Name} (ID: {npc.ID})");
        
        // Check NPCRelationship.Betrayed - obligation breaking triggers this
        if (npc.PlayerRelationship == NPCRelationship.Betrayed)
        {
            Console.WriteLine($"[NPCStateResolver] NPCRelationship is BETRAYED - returning HOSTILE");
            return NPCEmotionalState.HOSTILE;
        }

        var queue = _letterQueueManager.GetActiveLetters();
        var theirLetters = queue.Where(l => 
            l.SenderId == npc.ID || l.SenderName == npc.Name).ToList();
        
        // No letters = WITHDRAWN
        if (!theirLetters.Any())
        {
            Console.WriteLine($"[NPCStateResolver] No letters - returning WITHDRAWN");
            return NPCEmotionalState.WITHDRAWN;
        }
        

        // Find most urgent letter to determine state
        var mostUrgent = theirLetters.OrderBy(l => l.DeadlineInHours).First();
        var hoursRemaining = mostUrgent.DeadlineInHours;
        
        Console.WriteLine($"[NPCStateResolver] Most urgent letter: {mostUrgent.Stakes} with {hoursRemaining}h remaining");
        
        // Apply simple Stakes + Time formula
        return (mostUrgent.Stakes, hoursRemaining) switch
        {
            // Personal Safety
            (StakeType.SAFETY, <= 6) => NPCEmotionalState.DESPERATE,
            (StakeType.SAFETY, > 6) => NPCEmotionalState.ANXIOUS,
            
            // Reputation  
            (StakeType.REPUTATION, <= 12) => NPCEmotionalState.ANXIOUS,
            (StakeType.REPUTATION, > 12) => NPCEmotionalState.CALCULATING,
            
            // Wealth - always calculating
            (StakeType.WEALTH, _) => NPCEmotionalState.CALCULATING,
            
            // Secrets
            (StakeType.SECRET, <= 6) => NPCEmotionalState.ANXIOUS,
            (StakeType.SECRET, > 6) => NPCEmotionalState.CALCULATING,
            
            // Default fallback
            _ => NPCEmotionalState.CALCULATING
        };
    }

    /// <summary>
    /// Get emoji representation of emotional state
    /// </summary>
    public string GetMoodEmoji(NPCEmotionalState state)
    {
        return state switch
        {
            NPCEmotionalState.DESPERATE => "😰",
            NPCEmotionalState.ANXIOUS => "😟",
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
            
            (NPCEmotionalState.ANXIOUS, StakeType.REPUTATION) => 
                "shifting weight nervously, eyes searching yours",
            (NPCEmotionalState.ANXIOUS, StakeType.WEALTH) => 
                "wringing hands, forced smile not reaching their eyes",
            (NPCEmotionalState.ANXIOUS, StakeType.SAFETY) => 
                "tense shoulders, ready to flee at any moment",
            (NPCEmotionalState.ANXIOUS, StakeType.SECRET) => 
                "hushed voice, constantly checking surroundings",
            
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
            NPCEmotionalState.ANXIOUS => 0,     // Normal cost
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
            .Where(l => l.State == LetterState.Collected)
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
        else if (queue.Letters.Count(l => l.State == LetterState.Collected) > 6)
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
                
            (NPCEmotionalState.ANXIOUS, StakeType.REPUTATION) =>
                $"My {stakesHint} hangs in the balance. {urgency} Please, I need your help.",
                
            (NPCEmotionalState.ANXIOUS, StakeType.SAFETY) =>
                $"The {stakesHint} worries me deeply. {urgency} Can you prioritize this?",
                
            (NPCEmotionalState.ANXIOUS, StakeType.SECRET) =>
                $"That {stakesHint}... {urgency} I'm concerned about who might be watching.",
                
            (NPCEmotionalState.ANXIOUS, StakeType.WEALTH) =>
                $"The {stakesHint} needs attention. {urgency} Time is money, as they say.",
                
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
            NPCEmotionalState.ANXIOUS => $"{npc.Name} seems worried about something.",
            NPCEmotionalState.HOSTILE => $"{npc.Name} glares at you with barely concealed anger.",
            NPCEmotionalState.DESPERATE => $"{npc.Name} looks at you with pleading eyes.",
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