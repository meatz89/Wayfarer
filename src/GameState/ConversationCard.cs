using System;
using System.Collections.Generic;

public class ConversationCard
{
    // Core identity
    public string Id { get; set; }
    public string Description { get; set; }

    // Single source of truth for card type
    public CardType CardType { get; set; } = CardType.Conversation;

    // Properties list replaces all boolean flags
    public List<CardProperty> Properties { get; set; } = new List<CardProperty>();

    // Skeleton tracking - consistent with other entities
    public bool IsSkeleton { get; set; } = false;
    public string SkeletonSource { get; set; } // What created this skeleton

    // Core mechanics
    public ConnectionType TokenType { get; set; }
    public int Focus { get; set; }
    public Difficulty Difficulty { get; set; }

    // Token requirements for gated exchanges
    public int MinimumTokensRequired { get; set; } = 0;
    public ConnectionType? RequiredTokenType { get; set; }

    // Personality targeting - which NPCs can use this card
    public List<string> PersonalityTypes { get; set; } = new List<string>();

    // Rapport threshold for goal cards (Letter, Promise, BurdenGoal)
    public int RapportThreshold { get; set; } = 0;

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
    public bool IsBurden => Properties.Contains(CardProperty.Burden);
    public bool IsObservable => CardType == CardType.Observation;

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


    // Additional compatibility properties for UI
    public CardContext Context => null;
    public string RequestCardType => (CardType == CardType.Letter || CardType == CardType.Promise || CardType == CardType.BurdenGoal) ? "Request" : null;

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
            MinimumTokensRequired = this.MinimumTokensRequired,
            RequiredTokenType = this.RequiredTokenType,
            PersonalityTypes = new List<string>(this.PersonalityTypes), // Clone personality types
            RapportThreshold = this.RapportThreshold,
            SuccessEffect = this.SuccessEffect?.DeepClone() ?? CardEffect.None,
            FailureEffect = this.FailureEffect?.DeepClone() ?? CardEffect.None,
            ExhaustEffect = this.ExhaustEffect?.DeepClone() ?? CardEffect.None,
            DialogueFragment = this.DialogueFragment,
            VerbPhrase = this.VerbPhrase,
            CardType = this.CardType
        };
    }


}