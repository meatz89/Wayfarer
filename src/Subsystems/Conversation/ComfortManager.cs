using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages the comfort battery system for conversations.
/// Handles comfort calculations, thresholds, and resets.
/// </summary>
public class ComfortManager
{
    private const int COMFORT_THRESHOLD = 3;
    private const int MIN_COMFORT = -10;
    private const int MAX_COMFORT = 10;

    /// <summary>
    /// Update comfort value with change
    /// </summary>
    public int UpdateComfort(int currentComfort, int change)
    {
        var newComfort = currentComfort + change;
        return Math.Clamp(newComfort, MIN_COMFORT, MAX_COMFORT);
    }

    /// <summary>
    /// Check if comfort has reached threshold for state change
    /// </summary>
    public bool CheckThreshold(int comfort)
    {
        return Math.Abs(comfort) >= COMFORT_THRESHOLD;
    }

    /// <summary>
    /// Check if should trigger state change
    /// </summary>
    public bool ShouldTriggerStateChange(int comfort)
    {
        return comfort >= COMFORT_THRESHOLD || comfort <= -COMFORT_THRESHOLD;
    }

    /// <summary>
    /// Reset comfort after state change
    /// </summary>
    public int ResetComfort()
    {
        return 0;
    }

    /// <summary>
    /// Calculate comfort change from played cards
    /// </summary>
    public int CalculateComfortChange(HashSet<CardInstance> cards, EmotionalState currentState)
    {
        int totalComfort = 0;
        
        foreach (var card in cards)
        {
            var comfort = CalculateSingleCardComfort(card, currentState);
            totalComfort += comfort;
        }
        
        // Check for set bonus (3+ cards of same type)
        if (CheckSetBonus(cards))
        {
            totalComfort += 2;
        }
        
        // State-specific bonuses
        if (currentState == EmotionalState.CONNECTED && totalComfort > 5)
        {
            totalComfort += 1; // Connected state bonus
        }
        else if (currentState == EmotionalState.EAGER && cards.Count >= 3)
        {
            totalComfort += 1; // Eager state bonus for multiple cards
        }
        
        return totalComfort;
    }

    /// <summary>
    /// Calculate comfort from single card
    /// </summary>
    private int CalculateSingleCardComfort(CardInstance card, EmotionalState currentState)
    {
        // Burden cards always negative (burden is a CardCategory)
        if (card.Category == CardCategory.Burden.ToString())
        {
            return -Math.Max(1, card.Weight);
        }
        
        // Goal cards provide bonus
        if (card.IsGoalCard)
        {
            return card.Weight + 2;
        }
        
        // Observation cards have base comfort from template
        if (card.IsObservation)
        {
            return card.BaseComfort;
        }
        
        // State modifiers
        float stateModifier = GetStateComfortModifier(currentState);
        
        // Base comfort is card weight
        int baseComfort = card.Weight;
        
        // Apply state modifier
        int modifiedComfort = (int)Math.Round(baseComfort * stateModifier);
        
        return Math.Max(1, modifiedComfort); // Minimum 1 comfort for non-burden cards
    }

    /// <summary>
    /// Get comfort modifier for emotional state
    /// </summary>
    private float GetStateComfortModifier(EmotionalState state)
    {
        return state switch
        {
            EmotionalState.DESPERATE => 0.5f,  // Reduced effectiveness
            EmotionalState.HOSTILE => 0.25f,   // Severely reduced
            EmotionalState.TENSE => 0.75f,     // Slightly reduced
            EmotionalState.GUARDED => 0.8f,    // Slightly reduced
            EmotionalState.NEUTRAL => 1.0f,    // Normal
            EmotionalState.OPEN => 1.1f,       // Slightly enhanced
            EmotionalState.EAGER => 1.2f,      // Enhanced
            EmotionalState.CONNECTED => 1.3f,  // Significantly enhanced
            _ => 1.0f
        };
    }

    /// <summary>
    /// Check for set bonus (3+ cards of same type)
    /// </summary>
    private bool CheckSetBonus(HashSet<CardInstance> cards)
    {
        if (cards.Count < 3)
            return false;
        
        var typeGroups = cards.GroupBy(c => c.Type);
        return typeGroups.Any(g => g.Count() >= 3);
    }

    /// <summary>
    /// Get comfort description for narrative
    /// </summary>
    public string GetComfortDescription(int comfort)
    {
        return comfort switch
        {
            0 => "neutral",
            <= -7 => "extremely uncomfortable",
            <= -5 => "very uncomfortable",
            <= -3 => "uncomfortable",
            < 0 => "slightly uncomfortable",
            >= 7 => "very comfortable",
            >= 5 => "quite comfortable",
            >= 3 => "comfortable",
            _ => "slightly comfortable" // covers all positive values > 0
        };
    }

    /// <summary>
    /// Get narrative text for comfort change
    /// </summary>
    public string GetComfortChangeNarrative(int oldComfort, int newComfort, string npcName)
    {
        var change = newComfort - oldComfort;
        
        if (change == 0)
            return $"{npcName} maintains their composure.";
        
        if (change > 0)
        {
            return change switch
            {
                >= 5 => $"{npcName} visibly relaxes and smiles warmly.",
                >= 3 => $"{npcName} seems more at ease.",
                >= 2 => $"{npcName} nods appreciatively.",
                _ => $"{npcName} shows subtle signs of approval."
            };
        }
        else
        {
            return change switch
            {
                <= -5 => $"{npcName} becomes visibly distressed.",
                <= -3 => $"{npcName} tenses up noticeably.",
                <= -2 => $"{npcName} frowns and shifts uncomfortably.",
                _ => $"{npcName} seems slightly put off."
            };
        }
    }

    /// <summary>
    /// Calculate comfort threshold progress
    /// </summary>
    public ComfortProgress GetComfortProgress(int comfort)
    {
        return new ComfortProgress
        {
            CurrentComfort = comfort,
            PositiveProgress = Math.Max(0, comfort) / (float)COMFORT_THRESHOLD,
            NegativeProgress = Math.Max(0, -comfort) / (float)COMFORT_THRESHOLD,
            IsAtPositiveThreshold = comfort >= COMFORT_THRESHOLD,
            IsAtNegativeThreshold = comfort <= -COMFORT_THRESHOLD,
            DistanceToPositive = Math.Max(0, COMFORT_THRESHOLD - comfort),
            DistanceToNegative = Math.Max(0, comfort - (-COMFORT_THRESHOLD))
        };
    }

    /// <summary>
    /// Check if comfort level enables letter generation
    /// </summary>
    public bool EnablesLetterGeneration(int comfort, EmotionalState state)
    {
        // Letters emerge from positive states with good comfort
        if (state == EmotionalState.OPEN || state == EmotionalState.EAGER || state == EmotionalState.CONNECTED)
        {
            return comfort >= 5;
        }
        
        return false;
    }

    /// <summary>
    /// Apply special comfort bonuses
    /// </summary>
    public int ApplySpecialBonuses(int baseComfort, ComfortContext context)
    {
        int bonusComfort = baseComfort;
        
        // Letter delivery bonus
        if (context.DeliveredLetter)
        {
            bonusComfort += 5;
        }
        
        // Goal achievement bonus
        if (context.GoalAchieved)
        {
            bonusComfort += 3;
        }
        
        // Crisis resolution bonus
        if (context.CrisisResolved)
        {
            bonusComfort += 7;
        }
        
        return bonusComfort;
    }
}

/// <summary>
/// Progress towards comfort thresholds
/// </summary>
public class ComfortProgress
{
    public int CurrentComfort { get; set; }
    public float PositiveProgress { get; set; }
    public float NegativeProgress { get; set; }
    public bool IsAtPositiveThreshold { get; set; }
    public bool IsAtNegativeThreshold { get; set; }
    public int DistanceToPositive { get; set; }
    public int DistanceToNegative { get; set; }
}

/// <summary>
/// Context for comfort calculations
/// </summary>
public class ComfortContext
{
    public bool DeliveredLetter { get; set; }
    public bool GoalAchieved { get; set; }
    public bool CrisisResolved { get; set; }
}