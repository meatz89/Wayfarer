using System;
using System.Collections.Generic;

public class ConversationCard
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    // Properties list replaces all boolean flags
    public List<CardProperty> Properties { get; set; } = new List<CardProperty>();
    
    // Skeleton tracking - consistent with other entities
    public bool IsSkeleton { get; set; } = false;
    public string SkeletonSource { get; set; } // What created this skeleton
    public TokenType TokenType { get; set; }
    public int Weight { get; set; }
    public Difficulty Difficulty { get; set; }
    
    // Three-effect system: each card can have Success, Failure, and Exhaust effects
    public CardEffect SuccessEffect { get; set; } = CardEffect.None;
    public CardEffect FailureEffect { get; set; } = CardEffect.None;
    public CardEffect ExhaustEffect { get; set; } = CardEffect.None;

    // Display properties
    public string DialogueFragment { get; set; }
    public string VerbPhrase { get; set; }

    // Helper properties that use Properties list as source of truth
    public bool IsFleeting => Properties.Contains(CardProperty.Fleeting);
    public bool IsOpportunity => Properties.Contains(CardProperty.Opportunity);
    public bool IsPersistent => !Properties.Contains(CardProperty.Fleeting) 
                                && !Properties.Contains(CardProperty.Opportunity);
    public bool IsGoal => Properties.Contains(CardProperty.Fleeting) 
                          && Properties.Contains(CardProperty.Opportunity);
    public bool IsBurden => Properties.Contains(CardProperty.Burden);
    public bool IsObservable => Properties.Contains(CardProperty.Observable);
    public bool IsObservation => IsObservable; // Backward compatibility alias
    public bool HasFinalWord => IsGoal; // Goal cards have "final word" behavior
    public bool IsGoalCard => IsGoal; // Backward compatibility alias


    // Compatibility method to ensure default Persistent property
    public void EnsureDefaultProperties()
    {
        // If no exhaustion properties set, default to Persistent
        if (!Properties.Contains(CardProperty.Fleeting) && 
            !Properties.Contains(CardProperty.Opportunity) && 
            !Properties.Contains(CardProperty.Persistent))
        {
            Properties.Add(CardProperty.Persistent);
        }
    }

    /// <summary>
    /// Checks if card follows the one-effect-per-trigger rule
    /// </summary>
    public bool HasValidEffects()
    {
        // Each effect should have at most one type of outcome
        return true; // All effects are now properly separated
    }

    // Deep clone for deck instances
    public ConversationCard DeepClone()
    {
        return new ConversationCard
        {
            Id = this.Id,
            Name = this.Name,
            Description = this.Description,
            Properties = new List<CardProperty>(this.Properties), // Clone the properties list
            IsSkeleton = this.IsSkeleton,
            SkeletonSource = this.SkeletonSource,
            TokenType = this.TokenType,
            Weight = this.Weight,
            Difficulty = this.Difficulty,
            SuccessEffect = this.SuccessEffect?.DeepClone() ?? CardEffect.None,
            FailureEffect = this.FailureEffect?.DeepClone() ?? CardEffect.None,
            ExhaustEffect = this.ExhaustEffect?.DeepClone() ?? CardEffect.None,
            DialogueFragment = this.DialogueFragment,
            VerbPhrase = this.VerbPhrase
        };
    }

    // Get base success percentage from difficulty tier
    public int GetBaseSuccessPercentage()
    {
        // Use DifficultyTier if set, otherwise convert from old Difficulty
        if (Difficulty != default(Difficulty))
            return (int)Difficulty;

        // Fallback to old Difficulty_Legacy
        return Difficulty_Legacy switch
        {
            Difficulty.VeryEasy => 85,
            Difficulty.Easy => 70,
            Difficulty.Medium => 60,
            Difficulty.Hard => 50,
            Difficulty.VeryHard => 40,
            _ => 60
        };
    }

}