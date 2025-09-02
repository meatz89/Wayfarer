using System;
using System.Collections.Generic;

public class ConversationCard
{
    // Core identity
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    // Properties list replaces all boolean flags
    public List<CardProperty> Properties { get; set; } = new List<CardProperty>();
    
    // Skeleton tracking - consistent with other entities
    public bool IsSkeleton { get; set; } = false;
    public string SkeletonSource { get; set; } // What created this skeleton
    
    // Core mechanics
    public TokenType TokenType { get; set; }
    public int Weight { get; set; }
    public Difficulty Difficulty { get; set; }
    
    // Personality targeting - which NPCs can use this card
    public List<string> PersonalityTypes { get; set; } = new List<string>();
    
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

    // Get base success percentage from difficulty tier
    public int GetBaseSuccessPercentage()
    {
        return Difficulty switch
        {
            Difficulty.VeryEasy => 85,
            Difficulty.Easy => 70,
            Difficulty.Medium => 60,
            Difficulty.Hard => 50,
            Difficulty.VeryHard => 40,
            _ => 60
        };
    }
    
    // Helper to get category name for compatibility
    public string Category
    {
        get
        {
            if (Properties.Contains(CardProperty.Exchange)) return nameof(CardCategory.Exchange);
            if (IsBurden) return nameof(CardCategory.Burden);
            if (IsGoal) return nameof(CardCategory.Promise);
            if (IsObservable) return nameof(CardCategory.Observation);
            // Default to Comfort for backwards compatibility
            return nameof(CardCategory.Comfort);
        }
    }
    
    // Additional compatibility properties for UI
    public CardType Type => Properties.Contains(CardProperty.Exchange) ? CardType.Exchange :
                           IsGoal ? CardType.Goal : 
                           IsObservable ? CardType.Observation : 
                           CardType.Normal;
    public CardContext Context => null;
    public string GoalCardType => IsGoal ? "Goal" : null;

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
            PersonalityTypes = new List<string>(this.PersonalityTypes), // Clone personality types
            SuccessEffect = this.SuccessEffect?.DeepClone() ?? CardEffect.None,
            FailureEffect = this.FailureEffect?.DeepClone() ?? CardEffect.None,
            ExhaustEffect = this.ExhaustEffect?.DeepClone() ?? CardEffect.None,
            DialogueFragment = this.DialogueFragment,
            VerbPhrase = this.VerbPhrase
        };
    }


}