using System;
using System.Collections.Generic;
using Wayfarer.GameState.Enums;

public class ConversationCard
{
    // Core identity
    public string Id { get; set; }
    public string Description { get; set; }

    // Single source of truth for card type
    public CardType CardType { get; set; } = CardType.Conversation;

    // Categorical properties that define behavior through context
    public PersistenceType Persistence { get; set; } = PersistenceType.Thought;
    public SuccessEffectType SuccessType { get; set; } = SuccessEffectType.None;
    public FailureEffectType FailureType { get; set; } = FailureEffectType.None;
    public ExhaustEffectType ExhaustType { get; set; } = ExhaustEffectType.None;

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

    // Promise card specific properties
    public int QueuePosition { get; set; } = 0; // Position to force in queue (usually 1)
    public int InstantRapport { get; set; } = 0; // Rapport gained from burning tokens
    public string RequestId { get; set; } // Links card to its parent NPCRequest

    // Display properties
    public string DialogueFragment { get; set; }
    public string VerbPhrase { get; set; }

    // Level bonuses that apply at specific levels
    public List<CardLevelBonus> LevelBonuses { get; set; } = new List<CardLevelBonus>();

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


    // Deep clone for deck instances
    public ConversationCard DeepClone()
    {
        return new ConversationCard
        {
            Id = this.Id,
            Description = this.Description,
            Persistence = this.Persistence,
            SuccessType = this.SuccessType,
            FailureType = this.FailureType,
            ExhaustType = this.ExhaustType,
            IsSkeleton = this.IsSkeleton,
            SkeletonSource = this.SkeletonSource,
            TokenType = this.TokenType,
            Focus = this.Focus,
            Difficulty = this.Difficulty,
            MinimumTokensRequired = this.MinimumTokensRequired,
            RequiredTokenType = this.RequiredTokenType,
            PersonalityTypes = new List<string>(this.PersonalityTypes),
            RapportThreshold = this.RapportThreshold,
            QueuePosition = this.QueuePosition,
            InstantRapport = this.InstantRapport,
            RequestId = this.RequestId,
            DialogueFragment = this.DialogueFragment,
            VerbPhrase = this.VerbPhrase,
            CardType = this.CardType,
            LevelBonuses = new List<CardLevelBonus>(this.LevelBonuses)
        };
    }


}