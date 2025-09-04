using System;
using System.Collections.Generic;

public class ConversationCard
{
    // Core identity
    public string Id { get; set; }
    public string Description { get; set; }
    
    // Properties list replaces all boolean flags
    public List<CardProperty> Properties { get; set; } = new List<CardProperty>();
    
    // Skeleton tracking - consistent with other entities
    public bool IsSkeleton { get; set; } = false;
    public string SkeletonSource { get; set; } // What created this skeleton
    
    // Core mechanics
    public TokenType TokenType { get; set; }
    public int Focus { get; set; }
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
    public bool IsImpulse => Properties.Contains(CardProperty.Impulse);
    public bool IsOpening => Properties.Contains(CardProperty.Opening);
    public bool IsPersistent => !Properties.Contains(CardProperty.Impulse) 
                                && !Properties.Contains(CardProperty.Opening);
    public bool IsRequest => Properties.Contains(CardProperty.Impulse) 
                          && Properties.Contains(CardProperty.Opening);
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
            if (IsRequest) return nameof(CardCategory.Promise);
            if (IsObservable) return nameof(CardCategory.Observation);
            // Default to Flow for backwards compatibility
            return nameof(CardCategory.Flow);
        }
    }
    
    // Additional compatibility properties for UI
    public CardType Type => Properties.Contains(CardProperty.Exchange) ? CardType.Exchange :
                           IsRequest ? CardType.Request : 
                           IsObservable ? CardType.Observation : 
                           CardType.Normal;
    public CardContext Context => null;
    public string RequestCardType => IsRequest ? "Request" : null;

    // Deep clone for deck instances
    public ConversationCard DeepClone()
    {
        return new ConversationCard
        {
            Id = this.Id,
            Description = this.Description,
            Properties = new List<CardProperty>(this.Properties), // Clone the properties list
            IsSkeleton = this.IsSkeleton,
            SkeletonSource = this.SkeletonSource,
            TokenType = this.TokenType,
            Focus = this.Focus,
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