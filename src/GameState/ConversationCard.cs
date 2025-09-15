using System;
using System.Collections.Generic;
using Wayfarer.GameState.Enums;

public class ConversationCard
{
    // Core identity
    public string Id { get; init; }
    public string Description { get; init; }

    // Single source of truth for card type
    public CardType CardType { get; init; } = CardType.Conversation;

    // Categorical properties that define behavior through context
    public PersistenceType Persistence { get; init; } = PersistenceType.Thought;
    public SuccessEffectType SuccessType { get; init; } = SuccessEffectType.None;
    public FailureEffectType FailureType { get; init; } = FailureEffectType.None;
    public ExhaustEffectType ExhaustType { get; init; } = ExhaustEffectType.None;

    // Skeleton tracking - consistent with other entities
    public bool IsSkeleton { get; init; } = false;
    public string SkeletonSource { get; init; } // What created this skeleton

    // Core mechanics
    public ConnectionType TokenType { get; init; }
    public int Focus { get; init; }
    public Difficulty Difficulty { get; init; }

    // Token requirements for gated exchanges
    public int MinimumTokensRequired { get; init; } = 0;
    public ConnectionType? RequiredTokenType { get; init; }

    // Personality targeting - which NPCs can use this card
    public IReadOnlyList<string> PersonalityTypes { get; init; } = new List<string>();

    // Rapport threshold for goal cards (Letter, Promise, BurdenGoal)
    public int RapportThreshold { get; init; } = 0;

    // Promise card specific properties
    public int QueuePosition { get; init; } = 0; // Position to force in queue (usually 1)
    public int InstantRapport { get; init; } = 0; // Rapport gained from burning tokens
    public string RequestId { get; init; } // Links card to its parent NPCRequest

    // Display properties
    public string DialogueFragment { get; init; }
    public string VerbPhrase { get; init; }

    // Level bonuses that apply at specific levels
    public IReadOnlyList<CardLevelBonus> LevelBonuses { get; init; } = new List<CardLevelBonus>();

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




}