using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.GameState;

/// <summary>
/// Represents an NPC's conversation deck - their unique set of possible interactions
/// Decks evolve through letter delivery and conversation outcomes
/// </summary>
public class NPCDeck
{
    public string NpcId { get; set; }
    public List<ConversationChoice> Cards { get; set; } = new List<ConversationChoice>();

    // Maximum deck size to prevent bloat
    public const int MaxDeckSize = 20;

    private readonly TokenMechanicsManager _tokenManager;
    private readonly GameWorld _gameWorld;

    public NPCDeck(string npcId, PersonalityType personalityType = PersonalityType.STEADFAST, TokenMechanicsManager tokenManager = null, GameWorld gameWorld = null)
    {
        NpcId = npcId;
        _tokenManager = tokenManager;
        _gameWorld = gameWorld;
        InitializeStartingDeck(personalityType);
    }

    /// <summary>
    /// Initialize with universal starting cards + personality-specific cards
    /// </summary>
    private void InitializeStartingDeck(PersonalityType personalityType)
    {
        // Universal starting cards (from docs)
        Cards.AddRange(new[]
        {
            new ConversationChoice
            {
                ChoiceID = "small_talk",
                Name = "Small Talk",
                Description = "Simple social courtesy",
                NarrativeText = "Simple social courtesy",
                Difficulty = 2,
                PatienceCost = 1,
                ComfortGain = 2,
                Category = RelationshipCardCategory.Basic,
                ChoiceType = ConversationChoiceType.Introduction
            },
            new ConversationChoice
            {
                ChoiceID = "listen",
                Name = "Listen",
                Description = "Give them your full attention",
                NarrativeText = "Give them your full attention",
                Difficulty = 3,
                PatienceCost = 1,
                ComfortGain = 3,
                Category = RelationshipCardCategory.Basic,
                ChoiceType = ConversationChoiceType.Introduction
            },
            new ConversationChoice
            {
                ChoiceID = "how_are_things",
                Name = "How Are Things?",
                Description = "Check on their current situation",
                NarrativeText = "Check on their current situation",
                Difficulty = 2,
                PatienceCost = 1,
                ComfortGain = 1,
                Category = RelationshipCardCategory.Basic,
                ChoiceType = ConversationChoiceType.Introduction
            },
            new ConversationChoice
            {
                ChoiceID = "offer_help",
                Name = "Offer Help",
                Description = "Show willingness to assist",
                NarrativeText = "Show willingness to assist",
                Difficulty = 4,
                PatienceCost = 2,
                ComfortGain = 4,
                Category = RelationshipCardCategory.Basic,
                ChoiceType = ConversationChoiceType.Introduction
            },
            new ConversationChoice
            {
                ChoiceID = "quick_exit",
                Name = "Quick Exit",
                Description = "Politely end the conversation",
                NarrativeText = "Politely end the conversation",
                Difficulty = 0,
                PatienceCost = 0,
                ComfortGain = 0,
                Category = RelationshipCardCategory.Basic,
                ChoiceType = ConversationChoiceType.Default
            },
            // Negative cards (deck pollution)
            new ConversationChoice
            {
                ChoiceID = "awkward_silence",
                Name = "Awkward Silence",
                Description = "Uncomfortable pause in conversation",
                NarrativeText = "Uncomfortable pause in conversation",
                Difficulty = 8,
                PatienceCost = 0,
                ComfortGain = -2,
                Category = RelationshipCardCategory.Basic,
                ChoiceType = ConversationChoiceType.DeclineLetterOffer  // NEGATIVE card
            },
            new ConversationChoice
            {
                ChoiceID = "bad_memory",
                Name = "Bad Memory",
                Description = "Brings up an unfortunate past event",
                NarrativeText = "Brings up an unfortunate past event",
                Difficulty = 6,
                PatienceCost = 1,
                ComfortGain = -1,
                Category = RelationshipCardCategory.Basic,
                ChoiceType = ConversationChoiceType.DeclineLetterOffer  // NEGATIVE card
            },
            // Crisis cards - only available during DESPERATE/ANXIOUS states
            new ConversationChoice
            {
                ChoiceID = "promise_personal_help",
                Name = "Promise Personal Help",
                Description = "Swear to prioritize their needs above all others",
                NarrativeText = "Swear to prioritize their needs above all others",
                Difficulty = 3,
                PatienceCost = 3,
                ComfortGain = 6,
                Category = RelationshipCardCategory.Crisis,
                ChoiceType = ConversationChoiceType.AcceptLetterOffer,  // POSITIVE crisis card
                MechanicalEffects = GetPromisePersonalHelpEffects(NpcId)
            },
            new ConversationChoice
            {
                ChoiceID = "emergency_arrangement",
                Name = "Emergency Arrangement",
                Description = "Offer immediate assistance for their urgent situation",
                NarrativeText = "Offer immediate assistance for their urgent situation",
                Difficulty = 4,
                PatienceCost = 2,
                ComfortGain = 5,
                Category = RelationshipCardCategory.Crisis,
                ChoiceType = ConversationChoiceType.AcceptLetterOffer,  // POSITIVE crisis card
                MechanicalEffects = GetEmergencyArrangementEffects(NpcId)
            }
        });

        // Add betrayal cards - only available during HOSTILE states
        Cards.AddRange(new List<ConversationChoice>
        {
            new ConversationChoice
            {
                ChoiceID = "desperate_apology",
                Name = "Desperate Apology",
                Description = "Acknowledge your failures and beg for forgiveness",
                NarrativeText = "Acknowledge your failures and beg for forgiveness",
                Difficulty = 8,
                PatienceCost = 2,
                ComfortGain = 3,
                Category = RelationshipCardCategory.Betrayal,
                ChoiceType = ConversationChoiceType.AcceptLetterOffer,  // Positive betrayal recovery
                MechanicalEffects = GetDesperateApologyEffects(NpcId)
            },
            new ConversationChoice
            {
                ChoiceID = "compensation_offer",
                Name = "Offer Compensation",
                Description = "Promise payment or services to make amends",
                NarrativeText = "Promise payment or services to make amends",
                Difficulty = 6,
                PatienceCost = 1,
                ComfortGain = 2,
                Category = RelationshipCardCategory.Betrayal,
                ChoiceType = ConversationChoiceType.AcceptLetterOffer,  // Positive betrayal recovery
                MechanicalEffects = GetCompensationOfferEffects(NpcId)
            },
            new ConversationChoice
            {
                ChoiceID = "blame_circumstances",
                Name = "Blame Circumstances",
                Description = "Deflect responsibility to external factors",
                NarrativeText = "Deflect responsibility to external factors",
                Difficulty = 5,
                PatienceCost = 1,
                ComfortGain = -1,
                Category = RelationshipCardCategory.Betrayal,
                ChoiceType = ConversationChoiceType.PurgeLetter,  // NEGATIVE - makes things worse
                MechanicalEffects = GetBlameCircumstancesEffects(NpcId)
            }
        });

        // Add personality-specific cards based on NPC type
        Cards.AddRange(GetPersonalityCards(personalityType));
    }

    /// <summary>
    /// Create personality-specific conversation cards that reflect authentic human interaction patterns.
    /// These cards emerge from the NPC's natural way of communicating, not mechanical abilities.
    /// </summary>
    private List<ConversationChoice> GetPersonalityCards(PersonalityType personalityType)
    {
        return personalityType switch
        {
            PersonalityType.DEVOTED => new List<ConversationChoice>
            {
                new ConversationChoice
                {
                    ChoiceID = "offer_comfort",
                    Name = "Offer Comfort",
                    Description = "Show genuine care for their emotional wellbeing",
                    Difficulty = 3,
                    PatienceCost = 2,
                    ComfortGain = 4,
                    Category = RelationshipCardCategory.Basic,
                    NarrativeText = "Show genuine care for their emotional wellbeing",
                    ChoiceType = ConversationChoiceType.Introduction
                },
                new ConversationChoice
                {
                    ChoiceID = "listen_deeply",
                    Name = "Listen Deeply",
                    Description = "Give your complete, patient attention to their concerns",
                    Difficulty = 4,
                    PatienceCost = 2,
                    ComfortGain = 3,
                    Category = RelationshipCardCategory.Basic,
                    NarrativeText = "Give your complete, patient attention to their concerns",
                    ChoiceType = ConversationChoiceType.Introduction
                },
                new ConversationChoice
                {
                    ChoiceID = "share_personal_story",
                    Name = "Share Personal Story",
                    Description = "Open up about your own experiences to build connection",
                    Difficulty = 5,
                    PatienceCost = 3,
                    ComfortGain = 5,
                    Category = RelationshipCardCategory.Basic,
                    NarrativeText = "Open up about your own experiences to build connection",
                    ChoiceType = ConversationChoiceType.Introduction
                }
            },

            PersonalityType.MERCANTILE => new List<ConversationChoice>
            {
                new ConversationChoice
                {
                    ChoiceID = "discuss_business",
                    Name = "Discuss Business",
                    Description = "Talk about practical matters and mutual opportunities",
                    Difficulty = 3,
                    PatienceCost = 1,
                    ComfortGain = 3,
                    Category = RelationshipCardCategory.Basic,
                    NarrativeText = "Talk about practical matters and mutual opportunities",
                    ChoiceType = ConversationChoiceType.Introduction
                },
                new ConversationChoice
                {
                    ChoiceID = "negotiate_terms",
                    Name = "Negotiate Terms",
                    Description = "Work out the details of a mutually beneficial arrangement",
                    Difficulty = 4,
                    PatienceCost = 2,
                    ComfortGain = 4,
                    Category = RelationshipCardCategory.Basic,
                    NarrativeText = "Work out the details of a mutually beneficial arrangement",
                    ChoiceType = ConversationChoiceType.Introduction
                },
                new ConversationChoice
                {
                    ChoiceID = "assess_worth",
                    Name = "Assess Worth",
                    Description = "Evaluate the value of goods, services, or opportunities",
                    Difficulty = 2,
                    PatienceCost = 1,
                    ComfortGain = 2,
                    Category = RelationshipCardCategory.Basic,
                    NarrativeText = "Evaluate the value of goods, services, or opportunities",
                    ChoiceType = ConversationChoiceType.Introduction
                }
            },

            PersonalityType.PROUD => new List<ConversationChoice>
            {
                new ConversationChoice
                {
                    ChoiceID = "show_respect",
                    Name = "Show Respect",
                    Description = "Acknowledge their position and importance with proper deference",
                    Difficulty = 2,
                    PatienceCost = 1,
                    ComfortGain = 3,
                    Category = RelationshipCardCategory.Basic,
                    NarrativeText = "Acknowledge their position and importance with proper deference",
                    ChoiceType = ConversationChoiceType.Introduction
                },
                new ConversationChoice
                {
                    ChoiceID = "acknowledge_status",
                    Name = "Acknowledge Status",
                    Description = "Recognize their social standing and the authority it brings",
                    Difficulty = 3,
                    PatienceCost = 2,
                    ComfortGain = 4,
                    Category = RelationshipCardCategory.Basic,
                    NarrativeText = "Recognize their social standing and the authority it brings",
                    ChoiceType = ConversationChoiceType.Introduction
                },
                new ConversationChoice
                {
                    ChoiceID = "formal_address",
                    Name = "Formal Address",
                    Description = "Use proper titles and courteous language befitting their rank",
                    Difficulty = 2,
                    PatienceCost = 1,
                    ComfortGain = 2,
                    Category = RelationshipCardCategory.Basic,
                    NarrativeText = "Use proper titles and courteous language befitting their rank",
                    ChoiceType = ConversationChoiceType.Introduction
                }
            },

            PersonalityType.CUNNING => new List<ConversationChoice>
            {
                new ConversationChoice
                {
                    ChoiceID = "read_between_lines",
                    Name = "Read Between Lines",
                    Description = "Pick up on subtle hints and unspoken meanings",
                    Difficulty = 5,
                    PatienceCost = 2,
                    ComfortGain = 4,
                    Category = RelationshipCardCategory.Basic,
                    NarrativeText = "Pick up on subtle hints and unspoken meanings",
                    ChoiceType = ConversationChoiceType.Introduction
                },
                new ConversationChoice
                {
                    ChoiceID = "share_information",
                    Name = "Share Information",
                    Description = "Offer a useful piece of knowledge or gossip",
                    Difficulty = 4,
                    PatienceCost = 2,
                    ComfortGain = 3,
                    Category = RelationshipCardCategory.Basic,
                    NarrativeText = "Offer a useful piece of knowledge or gossip",
                    ChoiceType = ConversationChoiceType.Introduction
                },
                new ConversationChoice
                {
                    ChoiceID = "test_loyalty",
                    Name = "Test Loyalty",
                    Description = "Carefully probe to see where their true allegiances lie",
                    Difficulty = 6,
                    PatienceCost = 3,
                    ComfortGain = 5,
                    Category = RelationshipCardCategory.Basic,
                    NarrativeText = "Carefully probe to see where their true allegiances lie",
                    ChoiceType = ConversationChoiceType.Introduction
                }
            },

            PersonalityType.STEADFAST => new List<ConversationChoice>
            {
                new ConversationChoice
                {
                    ChoiceID = "show_honor",
                    Name = "Show Honor",
                    Description = "Demonstrate your commitment to doing what's right",
                    Difficulty = 3,
                    PatienceCost = 2,
                    ComfortGain = 4,
                    Category = RelationshipCardCategory.Basic,
                    NarrativeText = "Demonstrate your commitment to doing what's right",
                    ChoiceType = ConversationChoiceType.Introduction
                },
                new ConversationChoice
                {
                    ChoiceID = "speak_plainly",
                    Name = "Speak Plainly",
                    Description = "Be direct and honest without fancy words or deception",
                    Difficulty = 2,
                    PatienceCost = 1,
                    ComfortGain = 3,
                    Category = RelationshipCardCategory.Basic,
                    NarrativeText = "Be direct and honest without fancy words or deception",
                    ChoiceType = ConversationChoiceType.Introduction
                },
                new ConversationChoice
                {
                    ChoiceID = "respect_duty",
                    Name = "Respect Duty",
                    Description = "Acknowledge the importance of their responsibilities",
                    Difficulty = 2,
                    PatienceCost = 1,
                    ComfortGain = 2,
                    Category = RelationshipCardCategory.Basic,
                    NarrativeText = "Acknowledge the importance of their responsibilities",
                    ChoiceType = ConversationChoiceType.Introduction
                }
            },

            _ => new List<ConversationChoice>() // Fallback to no personality cards
        };
    }

    /// <summary>
    /// Draw 5 cards from deck for this conversation round
    /// </summary>
    public List<ConversationChoice> DrawCards(Dictionary<ConnectionType, int> currentTokens, int currentComfort = 0, NPCEmotionalState emotionalState = NPCEmotionalState.CALCULATING)
    {
        Console.WriteLine($"[NPCDeck] DrawCards called for {NpcId}: Total cards={Cards.Count}, EmotionalState={emotionalState}");
        
        List<ConversationChoice> availableCards = Cards
            .Where(card => card.CanPlay(currentTokens, currentComfort) && card.IsAvailableInState(emotionalState))
            .ToList();
            
        Console.WriteLine($"[NPCDeck] Available cards after filtering: {availableCards.Count}");

        // Shuffle available cards
        Random random = new Random();
        availableCards = availableCards.OrderBy(x => random.Next()).ToList();

        // Draw up to 5 cards, always include Quick Exit if available
        List<ConversationChoice> drawnCards = new List<ConversationChoice>();
        ConversationChoice? exitCard = availableCards.FirstOrDefault(c => c.ChoiceID == "quick_exit");
        if (exitCard != null)
        {
            drawnCards.Add(exitCard);
            availableCards.Remove(exitCard);
        }

        // Fill remaining slots with other cards
        drawnCards.AddRange(availableCards.Take(4));

        Console.WriteLine($"[NPCDeck] DrawCards returning {drawnCards.Count} choices for {NpcId}");
        foreach (var choice in drawnCards)
        {
            Console.WriteLine($"[NPCDeck]   - {choice.ChoiceID}: {choice.ChoiceType}");
        }
        
        return drawnCards;
    }

    /// <summary>
    /// Add a card to the deck (from letter delivery rewards)
    /// </summary>
    public void AddCard(ConversationChoice card)
    {
        if (Cards.Count >= MaxDeckSize) return;

        // Don't add duplicates
        if (Cards.Any(c => c.ChoiceID == card.ChoiceID)) return;

        Cards.Add(card);
    }

    /// <summary>
    /// Remove a card from the deck (clearing negatives)
    /// </summary>
    public void RemoveCard(string cardId)
    {
        Cards.RemoveAll(c => c.ChoiceID == cardId);
    }

    /// <summary>
    /// Calculate starting patience for conversations
    /// Negative cards reduce starting patience by 0.5 each
    /// </summary>
    public double GetPatiencePenalty()
    {
        int negativeCards = Cards.Where(c => c.ComfortGain < 0).Count();
        return negativeCards * 0.5;
    }

    /// <summary>
    /// Get mechanical effects for Promise Personal Help crisis card
    /// </summary>
    private List<IMechanicalEffect> GetPromisePersonalHelpEffects(string npcId)
    {
        List<IMechanicalEffect> effects = new List<IMechanicalEffect>
        {
            new CreateBindingObligationEffect(npcId, "Priority delivery promise")
        };

        if (_tokenManager != null)
        {
            effects.Add(new GainTokensEffect(ConnectionType.Trust, GameRules.CRISIS_CARD_TOKEN_REWARD, npcId, _tokenManager));
        }

        return effects;
    }

    /// <summary>
    /// Get mechanical effects for Emergency Arrangement crisis card
    /// </summary>
    private List<IMechanicalEffect> GetEmergencyArrangementEffects(string npcId)
    {
        List<IMechanicalEffect> effects = new List<IMechanicalEffect>
        {
            new CreateBindingObligationEffect(npcId, "Emergency assistance obligation")
        };

        if (_tokenManager != null)
        {
            effects.Add(new GainTokensEffect(ConnectionType.Trust, GameRules.CRISIS_CARD_TOKEN_REWARD, npcId, _tokenManager));
        }

        return effects;
    }

    /// <summary>
    /// Get mechanical effects for Desperate Apology betrayal card
    /// </summary>
    private List<IMechanicalEffect> GetDesperateApologyEffects(string npcId)
    {
        List<IMechanicalEffect> effects = new List<IMechanicalEffect>();

        if (_tokenManager != null)
        {
            // Desperate apology restores relationship but at great personal cost
            effects.Add(new RestoreRelationshipEffect(npcId, NPCRelationship.Unfriendly, _gameWorld));
            effects.Add(new BurnTokensEffect(ConnectionType.Status, 3, npcId, _tokenManager)); // Lose dignity
        }

        return effects;
    }

    /// <summary>
    /// Get mechanical effects for Compensation Offer betrayal card
    /// </summary>
    private List<IMechanicalEffect> GetCompensationOfferEffects(string npcId)
    {
        List<IMechanicalEffect> effects = new List<IMechanicalEffect>();

        if (_tokenManager != null && _gameWorld != null)
        {
            // Compensation offer - restore relationship but at financial cost
            effects.Add(new RestoreRelationshipEffect(npcId, NPCRelationship.Wary, _gameWorld));
            effects.Add(new BurnTokensEffect(ConnectionType.Commerce, 3, npcId, _tokenManager)); // Financial cost
        }

        return effects;
    }

    /// <summary>
    /// Get mechanical effects for Blame Circumstances betrayal card
    /// </summary>
    private List<IMechanicalEffect> GetBlameCircumstancesEffects(string npcId)
    {
        List<IMechanicalEffect> effects = new List<IMechanicalEffect>();

        if (_tokenManager != null && _gameWorld != null)
        {
            // Blame circumstances - partial restoration but damages trust (poor strategy)
            effects.Add(new RestoreRelationshipEffect(npcId, NPCRelationship.Hostile, _gameWorld)); // Still hostile
            effects.Add(new BurnTokensEffect(ConnectionType.Trust, 2, npcId, _tokenManager)); // Deflection backfires
        }

        return effects;
    }
}