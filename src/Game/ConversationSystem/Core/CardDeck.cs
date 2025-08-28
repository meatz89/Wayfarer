using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents an NPC's conversation deck.
/// Each NPC has a unique deck based on personality and relationship.
/// </summary>
public class CardDeck
{
    private readonly List<ConversationCard> cards;
    private readonly List<ConversationCard> discardPile;
    private readonly Random random;
    private Dictionary<ConnectionType, int> currentTokens;

    public CardDeck()
    {
        cards = new List<ConversationCard>();
        discardPile = new List<ConversationCard>();
        random = new Random();
    }

    /// <summary>
    /// Initialize deck for an NPC based on personality
    /// </summary>
    public void InitializeForNPC(NPC npc, TokenMechanicsManager tokenManager)
    {
        cards.Clear();
        discardPile.Clear();

        // Store tokens for filtering during draw
        currentTokens = tokenManager?.GetTokensWithNPC(npc.ID) ?? new Dictionary<ConnectionType, int>();

        // Add universal basic cards (always Persistent)
        AddUniversalCards();

        // Add personality-specific cards
        AddPersonalityCards(npc.PersonalityType);

        // Add state transition cards for emotional dynamics
        AddStateTransitionCards(npc.PersonalityType);

        // Add relationship cards based on current tokens
        AddRelationshipCards(currentTokens);

        // Add one mild burden for narrative tension
        AddInitialBurden();

        Shuffle();
    }

    private void AddUniversalCards()
    {
        // Basic persistent cards everyone has - depth 0-5 accessible at start
        cards.Add(new ConversationCard
        {
            Id = "small_talk",
            Template = CardTemplateType.CasualInquiry,
            Context = new CardContext { },
            Type = CardType.Trust,
            Persistence = PersistenceType.Persistent,
            Weight = 0,
            BaseComfort = 1,
            Depth = 2  // Basic level, accessible at start (comfort 5)
        });

        cards.Add(new ConversationCard
        {
            Id = "listen_actively",
            Template = CardTemplateType.ActiveListening,
            Context = new CardContext { },
            Type = CardType.Trust,
            Persistence = PersistenceType.Persistent,
            Weight = 0,
            BaseComfort = 1,
            Depth = 3  // Basic level, accessible at start
        });

        cards.Add(new ConversationCard
        {
            Id = "share_news",
            Template = CardTemplateType.ShareInformation,
            Context = new CardContext { },
            Type = CardType.Status,
            Persistence = PersistenceType.Persistent,
            Weight = 1,
            BaseComfort = 2,
            Depth = 7  // Decent level (6-10 range)
        });

        cards.Add(new ConversationCard
        {
            Id = "offer_help",
            Template = CardTemplateType.OfferHelp,
            Context = new CardContext { },
            Type = CardType.Trust,
            Persistence = PersistenceType.Persistent,
            Weight = 1,
            BaseComfort = 3,
            Depth = 12  // Good level (11-15 range)
        });

        cards.Add(new ConversationCard
        {
            Id = "weather_comment",
            Template = CardTemplateType.CasualInquiry,
            Context = new CardContext { ObservationType = ObservationType.Normal },
            Type = CardType.Trust,
            Persistence = PersistenceType.Persistent,
            Weight = 0,
            BaseComfort = 1,
            Depth = 1  // Very basic level
        });
    }

    private void AddPersonalityCards(PersonalityType personality)
    {
        switch (personality)
        {
            case PersonalityType.DEVOTED:
                // Trust-focused cards across depth ranges
                cards.Add(new ConversationCard
                {
                    Id = "share_concern",
                    Template = CardTemplateType.ExpressEmpathy,
                    Context = new CardContext { Personality = PersonalityType.DEVOTED },
                    Type = CardType.Trust,
                    Persistence = PersistenceType.Opportunity,
                    Weight = 2,
                    BaseComfort = 4,
                    Depth = 8  // Decent level (6-10 range)
                });
                cards.Add(new ConversationCard
                {
                    Id = "family_story",
                    Template = CardTemplateType.SharePersonal,
                    Context = new CardContext { Personality = PersonalityType.DEVOTED },
                    Type = CardType.Trust,
                    Persistence = PersistenceType.Persistent,
                    Weight = 1,
                    BaseComfort = 3,
                    Depth = 14  // Good level (11-15 range)
                });
                // Add more devoted cards to fill depth ranges
                cards.Add(new ConversationCard
                {
                    Id = "gentle_understanding",
                    Template = CardTemplateType.ExpressEmpathy,
                    Context = new CardContext { Personality = PersonalityType.DEVOTED },
                    Type = CardType.Trust,
                    Persistence = PersistenceType.Persistent,
                    Weight = 1,
                    BaseComfort = 2,
                    Depth = 4  // Basic level, accessible at start
                });
                cards.Add(new ConversationCard
                {
                    Id = "deep_empathy",
                    Template = CardTemplateType.ExpressEmpathy,
                    Context = new CardContext { Personality = PersonalityType.DEVOTED },
                    Type = CardType.Trust,
                    Persistence = PersistenceType.Persistent,
                    Weight = 3,
                    BaseComfort = 8,
                    Depth = 18  // Excellent level (16-20 range)
                });
                break;

            case PersonalityType.MERCANTILE:
                // Commerce-focused cards
                cards.Add(new ConversationCard
                {
                    Id = "trade_opportunity",
                    Template = CardTemplateType.ProposeDeal,
                    Context = new CardContext { Personality = PersonalityType.MERCANTILE },
                    Type = CardType.Commerce,
                    Persistence = PersistenceType.Opportunity,
                    Weight = 2,
                    BaseComfort = 4,
                    Depth = 8  // Decent level
                });
                cards.Add(new ConversationCard
                {
                    Id = "negotiate_terms",
                    Template = CardTemplateType.NegotiateTerms,
                    Context = new CardContext { Personality = PersonalityType.MERCANTILE },
                    Type = CardType.Commerce,
                    Persistence = PersistenceType.Persistent,
                    Weight = 1,
                    BaseComfort = 3,
                    Depth = 14  // Good level
                });
                break;

            case PersonalityType.PROUD:
                // Status-focused cards
                cards.Add(new ConversationCard
                {
                    Id = "social_position",
                    Template = CardTemplateType.AcknowledgePosition,
                    Context = new CardContext { Personality = PersonalityType.PROUD },
                    Type = CardType.Status,
                    Persistence = PersistenceType.Persistent,
                    Weight = 1,
                    BaseComfort = 3,
                    Depth = 8,  // Decent level
                });
                cards.Add(new ConversationCard
                {
                    Id = "noble_bearing",
                    Template = CardTemplateType.ShowRespect,
                    Context = new CardContext { Personality = PersonalityType.PROUD },
                    Type = CardType.Status,
                    Persistence = PersistenceType.Opportunity,
                    Weight = 2,
                    BaseComfort = 4,
                    Depth = 14  // Good level
                });
                break;

            case PersonalityType.CUNNING:
                // Shadow-focused cards
                cards.Add(new ConversationCard
                {
                    Id = "subtle_hint",
                    Template = CardTemplateType.ShareSecret,
                    Context = new CardContext { Personality = PersonalityType.CUNNING },
                    Type = CardType.Shadow,
                    Persistence = PersistenceType.Opportunity,
                    Weight = 1,
                    BaseComfort = 3,
                    Depth = 8,  // Decent level
                });
                cards.Add(new ConversationCard
                {
                    Id = "hidden_knowledge",
                    Template = CardTemplateType.ImplyKnowledge,
                    Context = new CardContext { Personality = PersonalityType.CUNNING },
                    Type = CardType.Shadow,
                    Persistence = PersistenceType.Opportunity,
                    Weight = 2,
                    BaseComfort = 5,
                    Depth = 17  // Excellent level - hidden knowledge
                });
                break;

            case PersonalityType.STEADFAST:
                // Balanced cards
                cards.Add(new ConversationCard
                {
                    Id = "honest_opinion",
                    Template = CardTemplateType.SharePersonal,
                    Context = new CardContext { Personality = PersonalityType.STEADFAST },
                    Type = CardType.Trust,
                    Persistence = PersistenceType.Persistent,
                    Weight = 2,
                    BaseComfort = 4,
                    Depth = 14  // Good level
                });
                cards.Add(new ConversationCard
                {
                    Id = "practical_advice",
                    Template = CardTemplateType.OfferHelp,
                    Context = new CardContext { Personality = PersonalityType.STEADFAST },
                    Type = CardType.Commerce,
                    Persistence = PersistenceType.Persistent,
                    Weight = 1,
                    BaseComfort = 3,
                    Depth = 8,  // Decent level
                });
                break;
        }
    }

    private void AddRelationshipCards(Dictionary<ConnectionType, int> tokens)
    {
        // Get token counts
        int trustTokens = tokens?.GetValueOrDefault(ConnectionType.Trust, 0) ?? 0;
        int commerceTokens = tokens?.GetValueOrDefault(ConnectionType.Commerce, 0) ?? 0;
        
        // Add token cards (cards that grant tokens on success)
        // These are important for OPEN, EAGER, and CONNECTED states
        cards.Add(new ConversationCard
        {
            Id = "build_trust_token",
            Template = CardTemplateType.ExpressEmpathy,
            Context = new CardContext { GrantsToken = true },
            Type = CardType.Trust,
            Persistence = PersistenceType.Persistent,
            Weight = 2,
            BaseComfort = 0,  // Tokens don't give comfort, they give tokens
            Depth = 10,  // Mid-level depth
            Category = CardCategory.COMFORT  // Use comfort category for now
        });
        
        cards.Add(new ConversationCard
        {
            Id = "build_commerce_token",
            Template = CardTemplateType.ProposeDeal,
            Context = new CardContext { GrantsToken = true },
            Type = CardType.Commerce,
            Persistence = PersistenceType.Persistent,
            Weight = 2,
            BaseComfort = 0,  // Tokens don't give comfort
            Depth = 10,
            Category = CardCategory.COMFORT
        });
        
        // Add cards based on relationship depth
        if (trustTokens >= 3)
        {
            cards.Add(new ConversationCard
            {
                Id = "request_trust_letter",
                Template = CardTemplateType.MentionLetter,
                Context = new CardContext { },
                Type = CardType.Trust,
                Persistence = PersistenceType.Persistent,
                Weight = 2,
                BaseComfort = 5,
                CanDeliverLetter = true,
                Depth = 17,  // Excellent level - letter delivery
            });
        }
        
        // Add advanced cards (5+ tokens)
        if (trustTokens >= 5)
        {
            cards.Add(new ConversationCard
            {
                Id = "deep_trust",
                Template = CardTemplateType.ExpressVulnerability,
                Context = new CardContext { },
                Type = CardType.Trust,
                Persistence = PersistenceType.Opportunity,
                Weight = 2,
                BaseComfort = 6,
                Depth = 15,
            });
        }
        
        // Add master cards (10+ tokens)
        if (trustTokens >= 10)
        {
            cards.Add(new ConversationCard
            {
                Id = "unbreakable_bond",
                Template = CardTemplateType.ExpressVulnerability,
                Context = new CardContext { },
                Type = CardType.Trust,
                Persistence = PersistenceType.OneShot,
                Weight = 3,
                BaseComfort = 10,
                Depth = 20,
            });
        }

        if (commerceTokens >= 3)
        {
            cards.Add(new ConversationCard
            {
                Id = "request_commerce_letter",
                Template = CardTemplateType.MentionLetter,
                Context = new CardContext { },
                Type = CardType.Commerce,
                Persistence = PersistenceType.Persistent,
                Weight = 2,
                BaseComfort = 5,
                CanDeliverLetter = true,
                Depth = 17,  // Excellent level - letter delivery
            });
        }
        
        // Add advanced Commerce cards (5+ tokens)
        if (commerceTokens >= 5)
        {
            cards.Add(new ConversationCard
            {
                Id = "lucrative_deal",
                Template = CardTemplateType.ProposeDeal,
                Context = new CardContext { },
                Type = CardType.Commerce,
                Persistence = PersistenceType.Opportunity,
                Weight = 2,
                BaseComfort = 7,
                Depth = 16,
            });
        }

        // Add obligation manipulation cards if there are active obligations
        cards.Add(new ConversationCard
        {
            Id = "discuss_obligation",
            Template = CardTemplateType.DiscussObligation,
            Context = new CardContext { },
            Type = CardType.Trust,
            Persistence = PersistenceType.Opportunity,
            Weight = 1,
            BaseComfort = 2,
            ManipulatesObligations = true,
            Depth = 13  // Good level - discussing obligations
        });
    }

    private void AddInitialBurden()
    {
        cards.Add(new ConversationCard
        {
            Id = "awkward_silence",
            Template = CardTemplateType.ShowingTension,
            Context = new CardContext { },
            Type = CardType.Trust,
            Persistence = PersistenceType.Burden,
            Weight = 1,
            BaseComfort = 0,
            Category = CardCategory.COMFORT,  // Burden cards are comfort cards with negative effects
            Depth = 0  // Always accessible
        });
    }

    private void AddStateTransitionCards(PersonalityType personality)
    {
        // Add state cards based on personality tendencies
        
        // Calming card - for moving from negative states
        cards.Add(new ConversationCard
        {
            Id = "calm_reassurance",
            Template = CardTemplateType.CalmnessAttempt,
            Context = new CardContext { Personality = personality },
            Type = CardType.Trust,
            Persistence = PersistenceType.Persistent,
            Weight = 1,
            BaseComfort = 0,
            Category = CardCategory.STATE,
            SuccessState = EmotionalState.NEUTRAL,
            FailureState = null,  // Stay in current state
            Depth = 0  // Always available
        });

        // DEPTH 0 STATE CARDS - Essential for state variety from the start
        
        // Guarded approach - available immediately for cautious interactions
        cards.Add(new ConversationCard
        {
            Id = "guarded_approach",
            Template = CardTemplateType.GuardedApproach,
            Context = new CardContext { Personality = personality },
            Type = CardType.Trust,
            Persistence = PersistenceType.Persistent,
            Weight = 1,
            BaseComfort = 0,
            Category = CardCategory.STATE,
            SuccessState = EmotionalState.GUARDED,
            FailureState = null,  // Stay in current state
            Depth = 0  // Always available
        });
        
        // Warm greeting - for moving to OPEN state early
        cards.Add(new ConversationCard
        {
            Id = "warm_greeting",
            Template = CardTemplateType.WarmGreeting,
            Context = new CardContext { Personality = personality },
            Type = CardType.Trust,
            Persistence = PersistenceType.Persistent,
            Weight = 1,
            BaseComfort = 0,
            Category = CardCategory.STATE,
            SuccessState = EmotionalState.OPEN,
            FailureState = EmotionalState.NEUTRAL,
            Depth = 0  // Always available
        });
        
        // Tense comment - can create tension immediately
        cards.Add(new ConversationCard
        {
            Id = "tense_comment",
            Template = CardTemplateType.TenseComment,
            Context = new CardContext { Personality = personality },
            Type = CardType.Trust,
            Persistence = PersistenceType.Persistent,
            Weight = 1,
            BaseComfort = 0,
            Category = CardCategory.STATE,
            SuccessState = EmotionalState.TENSE,
            FailureState = null,
            Depth = 0  // Always available
        });
        
        // Eager engagement - available early for enthusiastic starts
        cards.Add(new ConversationCard
        {
            Id = "eager_engagement",
            Template = CardTemplateType.EagerEngagement,
            Context = new CardContext { Personality = personality },
            Type = CardType.Trust,
            Persistence = PersistenceType.Opportunity,
            Weight = 1,
            BaseComfort = 0,
            Category = CardCategory.STATE,
            SuccessState = EmotionalState.EAGER,
            FailureState = EmotionalState.NEUTRAL,
            Depth = 0  // Always available
        });
        
        // Overwhelmed response - when things get too much
        cards.Add(new ConversationCard
        {
            Id = "overwhelmed_response",
            Template = CardTemplateType.OverwhelmedResponse,
            Context = new CardContext { Personality = personality },
            Type = CardType.Trust,
            Persistence = PersistenceType.Persistent,
            Weight = 1,
            BaseComfort = 0,
            Category = CardCategory.STATE,
            SuccessState = EmotionalState.OVERWHELMED,
            FailureState = null,
            Depth = 0  // Can happen at any comfort level
        });
        
        // Desperate plea - for crisis situations
        if (personality == PersonalityType.DEVOTED || personality == PersonalityType.CUNNING)
        {
            cards.Add(new ConversationCard
            {
                Id = "desperate_plea",
                Template = CardTemplateType.DesperatePlea,
                Context = new CardContext { Personality = personality },
                Type = CardType.Trust,
                Persistence = PersistenceType.Opportunity,
                Weight = 2,
                BaseComfort = 0,
                Category = CardCategory.STATE,
                SuccessState = EmotionalState.DESPERATE,
                FailureState = EmotionalState.TENSE,
                Depth = 0  // Crisis can happen anytime
            });
        }

        // Opening up card - for building rapport (DEPTH 1)
        cards.Add(new ConversationCard
        {
            Id = "opening_up",
            Template = CardTemplateType.OpeningUp,
            Context = new CardContext { Personality = personality },
            Type = CardType.Trust,
            Persistence = PersistenceType.Persistent,
            Weight = 2,
            BaseComfort = 0,
            Category = CardCategory.STATE,
            SuccessState = EmotionalState.OPEN,
            FailureState = EmotionalState.GUARDED,
            Depth = 6  // Requires some comfort
        });

        // Eagerness card - for high energy engagement
        cards.Add(new ConversationCard
        {
            Id = "becoming_eager",
            Template = CardTemplateType.BecomingEager,
            Context = new CardContext { Personality = personality },
            Type = CardType.Trust,
            Persistence = PersistenceType.Opportunity,
            Weight = 2,
            BaseComfort = 0,
            Category = CardCategory.STATE,
            SuccessState = EmotionalState.EAGER,
            FailureState = EmotionalState.NEUTRAL,
            Depth = 7  // Requires engagement
        });

        // Connection card - for deep rapport (rare)
        if (personality == PersonalityType.DEVOTED || personality == PersonalityType.STEADFAST)
        {
            cards.Add(new ConversationCard
            {
                Id = "deep_connection",
                Template = CardTemplateType.ExpressVulnerability,
                Context = new CardContext { Personality = personality },
                Type = CardType.Trust,
                Persistence = PersistenceType.OneShot,
                Weight = 3,
                BaseComfort = 0,
                Category = CardCategory.STATE,
                SuccessState = EmotionalState.CONNECTED,
                FailureState = EmotionalState.OPEN,
                Depth = 19  // Only at highest comfort level
            });
        }
    }

    /// <summary>
    /// Draw cards based on emotional state rules, filtered by comfort level only
    /// </summary>
    public List<ConversationCard> Draw(int count, int currentComfort = 5)
    {
        var drawn = new List<ConversationCard>();
        
        // Filter cards by comfort level only - tokens affect success rates, not card availability
        // This ensures linear progression without artificial breakpoints
        var availableCards = cards
            .Where(c => c.Depth <= currentComfort)
            .ToList();
        
        Console.WriteLine($"[CardDeck.Draw] Comfort: {currentComfort}, Available: {availableCards.Count}");
        
        for (int i = 0; i < count && availableCards.Any(); i++)
        {
            var card = availableCards.First();
            availableCards.RemoveAt(0);
            cards.Remove(card);
            drawn.Add(card);
        }

        return drawn;
    }
    
    /// <summary>
    /// Draw cards filtered by specific card types (for emotional state filtering)
    /// </summary>
    public List<ConversationCard> DrawFilteredByTypes(int count, int currentComfort, List<CardType> allowedTypes, bool includeTokenCards)
    {
        var drawn = new List<ConversationCard>();
        
        // Build the filter
        var availableCards = cards.Where(c => c.Depth <= currentComfort);
        
        if (allowedTypes != null)
        {
            // Filter by allowed types
            availableCards = availableCards.Where(c => allowedTypes.Contains(c.Type));
        }
        
        // Special handling for token cards (cards that grant tokens)
        if (includeTokenCards)
        {
            // Include cards that have "Token" in their template or grant tokens
            availableCards = availableCards.Where(c => 
                allowedTypes == null || allowedTypes.Contains(c.Type) ||
                c.Template.ToString().Contains("Token") ||
                c.Context?.GrantsToken == true);
        }
        
        var cardList = availableCards.ToList();
        
        // Shuffle for randomness
        for (int i = cardList.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            var temp = cardList[i];
            cardList[i] = cardList[j];
            cardList[j] = temp;
        }
        
        // Draw up to count cards
        for (int i = 0; i < count && cardList.Any(); i++)
        {
            var card = cardList.First();
            cardList.RemoveAt(0);
            cards.Remove(card);
            drawn.Add(card);
        }
        
        return drawn;
    }
    
    /// <summary>
    /// Draw cards filtered by category (for guaranteed state cards)
    /// </summary>
    public List<ConversationCard> DrawFilteredByCategory(int count, int currentComfort, CardCategory category)
    {
        var drawn = new List<ConversationCard>();
        
        var availableCards = cards
            .Where(c => c.Depth <= currentComfort && c.Category == category)
            .ToList();
        
        // Shuffle for randomness
        for (int i = availableCards.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            var temp = availableCards[i];
            availableCards[i] = availableCards[j];
            availableCards[j] = temp;
        }
        
        // Draw up to count cards
        for (int i = 0; i < count && availableCards.Any(); i++)
        {
            var card = availableCards.First();
            availableCards.RemoveAt(0);
            cards.Remove(card);
            drawn.Add(card);
        }
        
        return drawn;
    }

    /// <summary>
    /// Add a card to the deck (from letter delivery reward, etc.)
    /// </summary>
    public void AddCard(ConversationCard card)
    {
        cards.Add(card);
        Shuffle();
    }

    /// <summary>
    /// Remove a card permanently (one-shot cards)
    /// </summary>
    public void RemoveCard(ConversationCard card)
    {
        cards.Remove(card);
        discardPile.Remove(card);
    }

    /// <summary>
    /// Discard played cards (will return to deck later)
    /// </summary>
    public void Discard(ConversationCard card)
    {
        if (card.Persistence != PersistenceType.OneShot)
        {
            discardPile.Add(card);
        }
    }

    /// <summary>
    /// Reset deck for new conversation
    /// </summary>
    public void ResetForNewConversation()
    {
        // Return persistent cards from discard
        var persistentCards = discardPile
            .Where(c => c.Persistence == PersistenceType.Persistent || 
                       c.Persistence == PersistenceType.Burden)
            .ToList();
        
        cards.AddRange(persistentCards);
        discardPile.Clear();
        Shuffle();
    }

    /// <summary>
    /// Generate a Crisis letter for desperate states
    /// </summary>
    public ConversationCard GenerateCrisisCard(NPC npc)
    {
        return new ConversationCard
        {
            Id = $"crisis_{Guid.NewGuid()}",
            Template = CardTemplateType.DesperateRequest,
            Context = new CardContext 
            { 
                Personality = npc.PersonalityType,
                EmotionalState = EmotionalState.DESPERATE,
                UrgencyLevel = 3,
                HasDeadline = true
            },
            Type = CardType.Trust,
            Persistence = PersistenceType.Crisis,
            Weight = 0, // Free to play in DESPERATE/HOSTILE states
            BaseComfort = 8,
            Category = CardCategory.CRISIS
        };
    }

    /// <summary>
    /// Generate a state changer card
    /// </summary>
    public ConversationCard GenerateStateCard(EmotionalState currentState)
    {
        return currentState switch
        {
            EmotionalState.DESPERATE => new ConversationCard
            {
                Id = "calm_reassurance",
                Template = CardTemplateType.CalmnessAttempt,
                Context = new CardContext { EmotionalState = currentState },
                Type = CardType.Trust,
                Persistence = PersistenceType.Persistent,
                Weight = 1,
                BaseComfort = 2,
                Category = CardCategory.STATE,
                SuccessState = EmotionalState.TENSE,
                FailureState = currentState
            },
            EmotionalState.GUARDED => new ConversationCard
            {
                Id = "break_ice",
                Template = CardTemplateType.OpeningUp,
                Context = new CardContext { EmotionalState = currentState },
                Type = CardType.Trust,
                Persistence = PersistenceType.Persistent,
                Weight = 1,
                BaseComfort = 2,
                Category = CardCategory.STATE,
                SuccessState = EmotionalState.NEUTRAL,
                FailureState = currentState
            },
            _ => null
        };
    }

    /// <summary>
    /// Add a single goal card to the deck and shuffle it in
    /// </summary>
    public void ShuffleInGoalCard(ConversationCard goalCard)
    {
        if (goalCard == null || !goalCard.IsGoalCard)
            throw new ArgumentException("Invalid goal card provided");
            
        // Remove any existing goal cards first (there should only be ONE)
        cards.RemoveAll(c => c.IsGoalCard);
        discardPile.RemoveAll(c => c.IsGoalCard);
        
        // Add the new goal card
        cards.Add(goalCard);
        
        // Shuffle to randomize position
        Shuffle();
    }
    
    /// <summary>
    /// Check if deck contains a goal card
    /// </summary>
    public bool HasGoalCard()
    {
        return cards.Any(c => c.IsGoalCard) || discardPile.Any(c => c.IsGoalCard);
    }
    
    private void Shuffle()
    {
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            var temp = cards[i];
            cards[i] = cards[j];
            cards[j] = temp;
        }
    }

    public int RemainingCards => cards.Count;
    public bool IsEmpty => !cards.Any();
    
    /// <summary>
    /// Get all cards in the deck (for filtering purposes)
    /// </summary>
    public IEnumerable<ConversationCard> GetCards()
    {
        return cards.Concat(discardPile);
    }
    
    /// <summary>
    /// Check if this is a crisis deck (contains only Crisis letters)
    /// </summary>
    public bool IsCrisis()
    {
        // A crisis deck contains cards with Crisis category or Crisis persistence
        return cards.Any() && cards.All(c => c.Category == CardCategory.CRISIS || c.Persistence == PersistenceType.Crisis);
    }
    
    /// <summary>
    /// Get all cards in the deck (for dev mode viewing)
    /// </summary>
    public List<ConversationCard> GetAllCards()
    {
        return new List<ConversationCard>(cards);
    }
}