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
    DESPERATE,    // Urgent need (high pressure from multiple factors)
    HOSTILE,      // Angry (overdue letters, betrayed trust, broken obligations)
    CALCULATING,  // Balanced pressure, normal interaction
    WITHDRAWN     // No engagement (no letters, no relationship), limited options
}

/// <summary>
/// Calculates NPC emotional states based on multiple weighted factors.
/// This is the core of the literary UI system - emotional states emerge from game state.
/// </summary>
public class NPCEmotionalStateCalculator
{
    private readonly LetterQueueManager _letterQueueManager;
    private readonly ConnectionTokenManager _tokenManager;
    private readonly ITimeManager _timeManager;
    private readonly StandingObligationManager _obligationManager;
    private readonly ConsequenceEngine _consequenceEngine;

    // Multi-factor weights for emotional state calculation
    private const float LETTER_PRESSURE_WEIGHT = 0.30f;  // 30% - urgency and stakes of letters
    private const float TOKEN_RELATIONSHIP_WEIGHT = 0.25f; // 25% - current token relationships
    private const float HISTORY_WEIGHT = 0.20f;          // 20% - past failures/successes
    private const float TIME_CONSTRAINT_WEIGHT = 0.15f;  // 15% - current time of day vs deadlines
    private const float OBLIGATION_WEIGHT = 0.10f;       // 10% - standing obligations

    public NPCEmotionalStateCalculator(
        LetterQueueManager letterQueueManager,
        ConnectionTokenManager tokenManager = null,
        ITimeManager timeManager = null,
        StandingObligationManager obligationManager = null,
        ConsequenceEngine consequenceEngine = null)
    {
        _letterQueueManager = letterQueueManager;
        _tokenManager = tokenManager;
        _timeManager = timeManager;
        _obligationManager = obligationManager;
        _consequenceEngine = consequenceEngine;
    }

    /// <summary>
    /// Calculate an NPC's emotional state based on multiple weighted factors
    /// </summary>
    public NPCEmotionalState CalculateState(NPC npc)
    {
        if (npc == null) return NPCEmotionalState.WITHDRAWN;

        var queue = _letterQueueManager.GetActiveLetters();
        var theirLetters = queue.Where(l => 
            l.SenderId == npc.ID || l.SenderName == npc.Name).ToList();

        Console.WriteLine($"[NPCEmotionalStateCalculator] Calculating state for {npc.Name} (ID: {npc.ID})");
        
        // Quick check for immediate triggers
        if (theirLetters.Any(l => l.DeadlineInHours <= 0))
        {
            Console.WriteLine($"[NPCEmotionalStateCalculator] Found overdue letter - returning HOSTILE");
            return NPCEmotionalState.HOSTILE;
        }

        // Calculate multi-factor emotional pressure
        float totalPressure = 0f;
        
        // Factor 1: Letter Pressure (30%)
        float letterPressure = CalculateLetterPressure(theirLetters);
        totalPressure += letterPressure * LETTER_PRESSURE_WEIGHT;
        
        // Factor 2: Token Relationships (25%)
        if (_tokenManager != null)
        {
            float tokenPressure = CalculateTokenPressure(npc);
            totalPressure += tokenPressure * TOKEN_RELATIONSHIP_WEIGHT;
        }
        
        // Factor 3: History (20%)
        if (_consequenceEngine != null)
        {
            float historyPressure = CalculateHistoryPressure(npc);
            totalPressure += historyPressure * HISTORY_WEIGHT;
        }
        
        // Factor 4: Time Constraints (15%)
        if (_timeManager != null)
        {
            float timePressure = CalculateTimePressure(theirLetters);
            totalPressure += timePressure * TIME_CONSTRAINT_WEIGHT;
        }
        
        // Factor 5: Obligations (10%)
        if (_obligationManager != null)
        {
            float obligationPressure = CalculateObligationPressure(npc);
            totalPressure += obligationPressure * OBLIGATION_WEIGHT;
        }

        Console.WriteLine($"[NPCEmotionalStateCalculator] Total pressure: {totalPressure:F2}");
        Console.WriteLine($"  - Letter: {letterPressure:F2}, Token: {(_tokenManager != null ? CalculateTokenPressure(npc) : 0):F2}");
        
        // Map total pressure to emotional state
        if (totalPressure >= 0.75f)
        {
            return NPCEmotionalState.DESPERATE;
        }
        else if (totalPressure >= 0.5f)
        {
            return NPCEmotionalState.HOSTILE;
        }
        else if (theirLetters.Any() || (_tokenManager != null && HasAnyTokens(npc)))
        {
            return NPCEmotionalState.CALCULATING;
        }
        else
        {
            return NPCEmotionalState.WITHDRAWN;
        }
    }
    
    private float CalculateLetterPressure(List<Letter> letters)
    {
        if (!letters.Any()) return 0f;
        
        float pressure = 0f;
        foreach (var letter in letters)
        {
            // Urgency component
            if (letter.DeadlineInHours <= 1) pressure += 0.4f;
            else if (letter.DeadlineInHours <= 3) pressure += 0.2f;
            else if (letter.DeadlineInHours <= 6) pressure += 0.1f;
            
            // Stakes component
            if (letter.Stakes == StakeType.SAFETY) pressure += 0.3f;
            else if (letter.Stakes == StakeType.SECRET) pressure += 0.2f;
            else if (letter.Stakes == StakeType.REPUTATION) pressure += 0.15f;
            else if (letter.Stakes == StakeType.WEALTH) pressure += 0.1f;
            
            // Position component (letters stuck in bad positions)
            var position = _letterQueueManager.GetLetterPosition(letter.Id);
            if (position.HasValue && position.Value > 4) pressure += 0.1f;
        }
        
        return Math.Min(1f, pressure / Math.Max(1, letters.Count));
    }
    
    private float CalculateTokenPressure(NPC npc)
    {
        var tokens = _tokenManager.GetTokensWithNPC(npc.ID);
        
        // Negative tokens create pressure
        float pressure = 0f;
        foreach (var kvp in tokens)
        {
            if (kvp.Value < 0)
            {
                pressure += Math.Abs(kvp.Value) * 0.1f;
            }
        }
        
        // Lack of any positive relationship also creates pressure
        if (!tokens.Any(kvp => kvp.Value > 0))
        {
            pressure += 0.2f;
        }
        
        return Math.Min(1f, pressure);
    }
    
    private float CalculateHistoryPressure(NPC npc)
    {
        // This would check consequence history for this NPC
        // For now, return moderate pressure
        return 0.3f;
    }
    
    private float CalculateTimePressure(List<Letter> letters)
    {
        if (!letters.Any()) return 0f;
        
        var currentHour = _timeManager.GetCurrentTimeHours();
        var remainingHours = 22 - currentHour; // Hours until late night
        
        // Check if we have enough time to deliver urgent letters
        var urgentCount = letters.Count(l => l.DeadlineInHours <= remainingHours);
        
        if (urgentCount > 2) return 0.8f;
        if (urgentCount > 1) return 0.5f;
        if (urgentCount > 0) return 0.3f;
        
        return 0f;
    }
    
    private float CalculateObligationPressure(NPC npc)
    {
        var obligations = _obligationManager.GetActiveObligations()
            .Where(o => o.RelatedNPCId == npc.ID);
        
        return obligations.Any() ? 0.5f : 0f;
    }
    
    private bool HasAnyTokens(NPC npc)
    {
        var tokens = _tokenManager.GetTokensWithNPC(npc.ID);
        return tokens.Any(kvp => kvp.Value != 0);
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