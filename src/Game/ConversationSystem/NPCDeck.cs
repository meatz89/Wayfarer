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

        // Add personality-specific betrayal recovery cards - only available during HOSTILE states
        Cards.AddRange(GetPersonalityBetrayalCards(personalityType));

        // Add personality-specific cards based on NPC type
        Cards.AddRange(GetPersonalityCards(personalityType));
    }

    /// <summary>
    /// Create personality-specific betrayal recovery cards that reflect how each personality type
    /// would realistically respond to broken promises and trust violations.
    /// </summary>
    private List<ConversationChoice> GetPersonalityBetrayalCards(PersonalityType personalityType)
    {
        return personalityType switch
        {
            PersonalityType.DEVOTED => new List<ConversationChoice>
            {
                new ConversationChoice
                {
                    ChoiceID = "show_genuine_remorse",
                    Name = "Show Genuine Remorse",
                    Description = "Express heartfelt regret and demonstrate you understand the hurt caused",
                    NarrativeText = "Express heartfelt regret and demonstrate you understand the hurt caused",
                    Difficulty = 8,
                    PatienceCost = 3,
                    ComfortGain = 4,
                    Category = RelationshipCardCategory.Betrayal,
                    ChoiceType = ConversationChoiceType.AcceptLetterOffer,
                    MechanicalEffects = GetDevotedBetrayalRecoveryEffects(NpcId)
                }
            },
            
            PersonalityType.MERCANTILE => new List<ConversationChoice>
            {
                new ConversationChoice
                {
                    ChoiceID = "offer_business_arrangement",
                    Name = "Offer Business Arrangement",
                    Description = "Propose a formal agreement that benefits both parties",
                    NarrativeText = "Propose a formal agreement that benefits both parties",
                    Difficulty = 6,
                    PatienceCost = 1,
                    ComfortGain = 2,
                    Category = RelationshipCardCategory.Betrayal,
                    ChoiceType = ConversationChoiceType.AcceptLetterOffer,
                    MechanicalEffects = GetMercantileBetrayalRecoveryEffects(NpcId)
                }
            },
            
            PersonalityType.PROUD => new List<ConversationChoice>
            {
                new ConversationChoice
                {
                    ChoiceID = "formal_apology_ritual",
                    Name = "Formal Apology Ritual",
                    Description = "Make a proper, ceremonial acknowledgment of wrongdoing",
                    NarrativeText = "Make a proper, ceremonial acknowledgment of wrongdoing",
                    Difficulty = 7,
                    PatienceCost = 2,
                    ComfortGain = 3,
                    Category = RelationshipCardCategory.Betrayal,
                    ChoiceType = ConversationChoiceType.AcceptLetterOffer,
                    MechanicalEffects = GetProudBetrayalRecoveryEffects(NpcId)
                }
            },
            
            PersonalityType.CUNNING => new List<ConversationChoice>
            {
                new ConversationChoice
                {
                    ChoiceID = "provide_valuable_information",
                    Name = "Provide Valuable Information",
                    Description = "Share secrets or knowledge as currency for forgiveness",
                    NarrativeText = "Share secrets or knowledge as currency for forgiveness",
                    Difficulty = 5,
                    PatienceCost = 1,
                    ComfortGain = 2,
                    Category = RelationshipCardCategory.Betrayal,
                    ChoiceType = ConversationChoiceType.AcceptLetterOffer,
                    MechanicalEffects = GetCunningBetrayalRecoveryEffects(NpcId)
                }
            },
            
            PersonalityType.STEADFAST => new List<ConversationChoice>
            {
                new ConversationChoice
                {
                    ChoiceID = "demonstrate_renewed_commitment",
                    Name = "Demonstrate Renewed Commitment",
                    Description = "Prove your reliability through immediate action and future dedication",
                    NarrativeText = "Prove your reliability through immediate action and future dedication",
                    Difficulty = 6,
                    PatienceCost = 2,
                    ComfortGain = 3,
                    Category = RelationshipCardCategory.Betrayal,
                    ChoiceType = ConversationChoiceType.AcceptLetterOffer,
                    MechanicalEffects = GetSteadfastBetrayalRecoveryEffects(NpcId)
                }
            },
            
            _ => new List<ConversationChoice>() // Fallback to empty list
        };
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
    /// Draw 5 cards from deck for this conversation round, excluding cards already played
    /// Prioritizes letter request cards to ensure they appear when available
    /// </summary>
    public List<ConversationChoice> DrawCards(Dictionary<ConnectionType, int> currentTokens, int currentComfort = 0, NPCEmotionalState emotionalState = NPCEmotionalState.CALCULATING, ConversationState conversationState = null)
    {
        Console.WriteLine($"[NPCDeck] DrawCards called for {NpcId}: Total cards={Cards.Count}, EmotionalState={emotionalState}");
        
        // CARD GAME MECHANICS: Filter out cards already played this conversation
        List<ConversationChoice> availableCards = Cards
            .Where(card => card.CanPlay(currentTokens, currentComfort) && 
                          card.IsAvailableInState(emotionalState) &&
                          (conversationState == null || !conversationState.IsCardPlayed(card.ChoiceID))) // Exclude played cards
            .ToList();
            
        Console.WriteLine($"[NPCDeck] Available cards after filtering: {availableCards.Count}");

        // Separate letter request cards and other cards
        List<ConversationChoice> letterRequestCards = availableCards
            .Where(c => c.Category == RelationshipCardCategory.LetterRequest)
            .ToList();
        List<ConversationChoice> otherCards = availableCards
            .Where(c => c.Category != RelationshipCardCategory.LetterRequest)
            .ToList();

        // Shuffle non-priority cards
        Random random = new Random();
        otherCards = otherCards.OrderBy(x => random.Next()).ToList();

        // Build final draw: Always include Quick Exit + DeliveryObligation Request cards + fill with others
        List<ConversationChoice> drawnCards = new List<ConversationChoice>();
        
        // Always include Quick Exit if available
        ConversationChoice? exitCard = otherCards.FirstOrDefault(c => c.ChoiceID == "quick_exit");
        if (exitCard != null)
        {
            drawnCards.Add(exitCard);
            otherCards.Remove(exitCard);
        }

        // Always include available letter request cards (these are the earned rewards)
        foreach (ConversationChoice letterCard in letterRequestCards)
        {
            if (drawnCards.Count < 5)
            {
                drawnCards.Add(letterCard);
                Console.WriteLine($"[NPCDeck] Prioritizing letter request card: {letterCard.ChoiceID}");
            }
        }

        // Fill remaining slots with other cards
        int remainingSlots = 5 - drawnCards.Count;
        drawnCards.AddRange(otherCards.Take(remainingSlots));

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
    /// Check if deck has a letter request card of specified type
    /// </summary>
    public bool HasLetterRequestCard(ConnectionType tokenType)
    {
        string cardId = GetLetterRequestCardId(tokenType);
        return Cards.Any(c => c.ChoiceID == cardId);
    }

    /// <summary>
    /// Add letter request card when comfort threshold is reached
    /// Cards persist in deck until successfully played or removed
    /// </summary>
    public void AddLetterRequestCard(ConnectionType tokenType, int currentRelationshipLevel)
    {
        // Don't add duplicates
        if (HasLetterRequestCard(tokenType)) return;
        
        // Create letter request card based on relationship type
        ConversationChoice letterCard = CreateLetterRequestCard(tokenType, currentRelationshipLevel);
        AddCard(letterCard);
    }

    /// <summary>
    /// Create a letter request card for the specified token type
    /// </summary>
    private ConversationChoice CreateLetterRequestCard(ConnectionType tokenType, int relationshipLevel)
    {
        ConversationChoiceType choiceType = tokenType switch
        {
            ConnectionType.Trust => ConversationChoiceType.RequestTrustLetter,
            ConnectionType.Commerce => ConversationChoiceType.RequestCommerceLetter,
            ConnectionType.Status => ConversationChoiceType.RequestStatusLetter,
            ConnectionType.Shadow => ConversationChoiceType.RequestShadowLetter,
            _ => ConversationChoiceType.RequestTrustLetter
        };

        string narrative = tokenType switch
        {
            ConnectionType.Trust => "Ask if they have any personal letters that need delivering",
            ConnectionType.Commerce => "Inquire about business correspondence requiring a courier",
            ConnectionType.Status => "Offer to carry formal correspondence to proper recipients",
            ConnectionType.Shadow => "Hint that you can be trusted with sensitive matters",
            _ => "Ask if they have any letters that need delivering"
        };

        // Higher relationship levels make letter requests easier
        int difficulty = Math.Max(2, 6 - relationshipLevel / 2);
        
        return new ConversationChoice
        {
            ChoiceID = GetLetterRequestCardId(tokenType),
            Name = $"Request {tokenType} Letter",
            Description = $"Ask for a {tokenType.ToString().ToLower()} letter to deliver",
            NarrativeText = narrative,
            Category = RelationshipCardCategory.LetterRequest,
            ChoiceType = choiceType,
            Difficulty = difficulty,
            PatienceCost = 2,
            ComfortGain = 0, // No comfort gain from asking
            Requirements = new Dictionary<ConnectionType, int>(), // No token requirements - comfort threshold is sufficient
            SuccessOutcome = "They consider your request carefully",
            NeutralOutcome = "They seem uncertain about entrusting you",
            FailureOutcome = "They politely decline your offer"
        };
    }

    private string GetLetterRequestCardId(ConnectionType tokenType)
    {
        return $"letter_request_{tokenType.ToString().ToLower()}";
    }

    /// <summary>
    /// Check if deck has a special letter request card of specified type
    /// </summary>
    public bool HasSpecialLetterRequestCard(ConnectionType tokenType)
    {
        string cardId = GetSpecialLetterRequestCardId(tokenType);
        return Cards.Any(c => c.ChoiceID == cardId);
    }

    /// <summary>
    /// Add special letter request card when token threshold (5+ tokens) is reached
    /// Special letters require higher relationship investment
    /// Only supports IntroductionDeliveryObligation (Trust) and AccessPermit (Commerce)
    /// </summary>
    public void AddSpecialLetterRequestCard(ConnectionType tokenType, int currentTokenCount)
    {
        // Only support Trust (Introduction) and Commerce (AccessPermit) special letters
        if (tokenType != ConnectionType.Trust && tokenType != ConnectionType.Commerce) return;
        
        // Don't add duplicates
        if (HasSpecialLetterRequestCard(tokenType)) return;
        
        // Only add if meets threshold (5+ tokens)
        if (currentTokenCount < 5) return;
        
        // Create special letter request card
        ConversationChoice specialCard = CreateSpecialLetterRequestCard(tokenType, currentTokenCount);
        AddCard(specialCard);
    }

    /// <summary>
    /// Create a special letter request card for the specified token type
    /// Only supports IntroductionDeliveryObligation (Trust) and AccessPermit (Commerce)
    /// </summary>
    private ConversationChoice CreateSpecialLetterRequestCard(ConnectionType tokenType, int tokenCount)
    {
        ConversationChoiceType choiceType = tokenType switch
        {
            ConnectionType.Trust => ConversationChoiceType.IntroductionLetter,
            ConnectionType.Commerce => ConversationChoiceType.AccessPermit,
            _ => throw new ArgumentException($"Special letters only support Trust (Introduction) and Commerce (AccessPermit), not {tokenType}")
        };

        string specialType = tokenType switch
        {
            ConnectionType.Trust => "Introduction",
            ConnectionType.Commerce => "Access Permit",
            _ => throw new ArgumentException($"Unsupported special letter type: {tokenType}")
        };

        string narrative = tokenType switch
        {
            ConnectionType.Trust => "Ask if they could introduce you to someone in their network",
            ConnectionType.Commerce => "Request access to restricted business areas or routes",
            _ => throw new ArgumentException($"Unsupported special letter type: {tokenType}")
        };

        string benefit = tokenType switch
        {
            ConnectionType.Trust => "Unlocks new contact",
            ConnectionType.Commerce => "Unlocks new routes",
            _ => throw new ArgumentException($"Unsupported special letter type: {tokenType}")
        };

        // Special letters are more challenging requests
        int difficulty = Math.Max(4, 8 - tokenCount / 3);
        
        return new ConversationChoice
        {
            ChoiceID = GetSpecialLetterRequestCardId(tokenType),
            Name = $"Request {specialType} Letter",
            Description = $"Ask for a special {specialType.ToLower()} letter ({benefit})",
            NarrativeText = narrative,
            Category = RelationshipCardCategory.LetterRequest,
            ChoiceType = choiceType,
            Difficulty = difficulty,
            PatienceCost = 3, // Special requests cost more patience
            ComfortGain = 0, // No comfort gain from requesting favors
            Requirements = new Dictionary<ConnectionType, int> 
            { 
                { tokenType, 5 } // Requires 5+ tokens of the type
            },
            SuccessOutcome = $"They agree to provide the {specialType.ToLower()} letter",
            NeutralOutcome = $"They consider your request for the {specialType.ToLower()} letter",
            FailureOutcome = $"They decline to provide the {specialType.ToLower()} letter"
        };
    }

    private string GetSpecialLetterRequestCardId(ConnectionType tokenType)
    {
        return $"special_letter_request_{tokenType.ToString().ToLower()}";
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

    /// <summary>
    /// DEVOTED personality betrayal recovery: Requires demonstrated emotional understanding
    /// </summary>
    private List<IMechanicalEffect> GetDevotedBetrayalRecoveryEffects(string npcId)
    {
        List<IMechanicalEffect> effects = new List<IMechanicalEffect>();
        if (_tokenManager != null && _gameWorld != null)
        {
            // Genuine remorse moves from HOSTILE to WARY (requires ongoing trust building)
            effects.Add(new RestoreRelationshipEffect(npcId, NPCRelationship.Wary, _gameWorld));
            effects.Add(new BurnTokensEffect(ConnectionType.Status, 2, npcId, _tokenManager)); // Humble yourself
            // Creates ongoing obligation to rebuild trust through actions
            effects.Add(new CreateBindingObligationEffect(npcId, "Prove trustworthiness through consistent actions"));
        }
        return effects;
    }

    /// <summary>
    /// MERCANTILE personality betrayal recovery: Business arrangement approach
    /// </summary>
    private List<IMechanicalEffect> GetMercantileBetrayalRecoveryEffects(string npcId)
    {
        List<IMechanicalEffect> effects = new List<IMechanicalEffect>();
        if (_tokenManager != null && _gameWorld != null)
        {
            // Business arrangement quickly restores working relationship
            effects.Add(new RestoreRelationshipEffect(npcId, NPCRelationship.Unfriendly, _gameWorld));
            effects.Add(new BurnTokensEffect(ConnectionType.Commerce, 3, npcId, _tokenManager)); // Financial compensation
            // Creates commercial obligation to maintain business relationship
            effects.Add(new CreateBindingObligationEffect(npcId, "Maintain professional obligations"));
        }
        return effects;
    }

    /// <summary>
    /// PROUD personality betrayal recovery: Formal acknowledgment of status and wrong
    /// </summary>
    private List<IMechanicalEffect> GetProudBetrayalRecoveryEffects(string npcId)
    {
        List<IMechanicalEffect> effects = new List<IMechanicalEffect>();
        if (_tokenManager != null && _gameWorld != null)
        {
            // Formal apology restores dignity and relationship
            effects.Add(new RestoreRelationshipEffect(npcId, NPCRelationship.Unfriendly, _gameWorld));
            effects.Add(new BurnTokensEffect(ConnectionType.Status, 4, npcId, _tokenManager)); // Major loss of personal dignity
            effects.Add(new GainTokensEffect(ConnectionType.Status, 2, npcId, _tokenManager)); // But acknowledges their status
        }
        return effects;
    }

    /// <summary>
    /// CUNNING personality betrayal recovery: Information exchange for forgiveness
    /// </summary>
    private List<IMechanicalEffect> GetCunningBetrayalRecoveryEffects(string npcId)
    {
        List<IMechanicalEffect> effects = new List<IMechanicalEffect>();
        if (_tokenManager != null && _gameWorld != null)
        {
            // Information exchange creates new dynamic - wary but interested
            effects.Add(new RestoreRelationshipEffect(npcId, NPCRelationship.Wary, _gameWorld));
            effects.Add(new GainTokensEffect(ConnectionType.Shadow, 2, npcId, _tokenManager)); // Information creates shadow bond
            effects.Add(new BurnTokensEffect(ConnectionType.Trust, 1, npcId, _tokenManager)); // But trust remains damaged
        }
        return effects;
    }

    /// <summary>
    /// STEADFAST personality betrayal recovery: Demonstrated commitment through action
    /// </summary>
    private List<IMechanicalEffect> GetSteadfastBetrayalRecoveryEffects(string npcId)
    {
        List<IMechanicalEffect> effects = new List<IMechanicalEffect>();
        if (_tokenManager != null && _gameWorld != null)
        {
            // Commitment demonstration restores basic relationship
            effects.Add(new RestoreRelationshipEffect(npcId, NPCRelationship.Unfriendly, _gameWorld));
            effects.Add(new BurnTokensEffect(ConnectionType.Trust, 2, npcId, _tokenManager)); // Trust still damaged
            // Creates strong obligation to prove reliability
            effects.Add(new CreateBindingObligationEffect(npcId, "Demonstrate unwavering reliability"));
        }
        return effects;
    }
}