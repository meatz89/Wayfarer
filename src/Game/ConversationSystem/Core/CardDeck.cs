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

        // Add universal basic cards (always Persistent)
        AddUniversalCards();

        // Add personality-specific cards
        AddPersonalityCards(npc.PersonalityType);

        // Add state transition cards for emotional dynamics
        AddStateTransitionCards(npc.PersonalityType);

        // Add relationship cards based on current tokens
        var tokens = tokenManager.GetTokensWithNPC(npc.ID);
        AddRelationshipCards(tokens);

        // Add one mild burden for narrative tension
        AddInitialBurden();

        Shuffle();
    }

    private void AddUniversalCards()
    {
        // Basic persistent cards everyone has
        cards.Add(new ConversationCard
        {
            Id = "small_talk",
            Template = CardTemplateType.CasualInquiry,
            Context = new CardContext { },
            Type = CardType.Trust,
            Persistence = PersistenceType.Persistent,
            Weight = 0,
            BaseComfort = 1,
            MinDepth = 0  // Surface level
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
            MinDepth = 0  // Surface level
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
            MinDepth = 1  // Personal level
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
            MinDepth = 2  // Intimate level
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
            MinDepth = 0  // Surface level
        });
    }

    private void AddPersonalityCards(PersonalityType personality)
    {
        switch (personality)
        {
            case PersonalityType.DEVOTED:
                // Trust-focused cards
                cards.Add(new ConversationCard
                {
                    Id = "share_concern",
                    Template = CardTemplateType.ExpressEmpathy,
                    Context = new CardContext { Personality = PersonalityType.DEVOTED },
                    Type = CardType.Trust,
                    Persistence = PersistenceType.Opportunity,
                    Weight = 2,
                    BaseComfort = 4,
                    MinDepth = 1  // Personal level
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
                    MinDepth = 2  // Intimate level
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
                    MinDepth = 1  // Personal level
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
                    MinDepth = 2  // Intimate level
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
                    MinDepth = 1  // Personal level
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
                    MinDepth = 2  // Intimate level
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
                    MinDepth = 1  // Personal level
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
                    MinDepth = 3  // Deep level - hidden knowledge
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
                    MinDepth = 2  // Intimate level
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
                    MinDepth = 1  // Personal level
                });
                break;
        }
    }

    private void AddRelationshipCards(Dictionary<ConnectionType, int> tokens)
    {
        // Add cards based on relationship depth
        if (tokens != null && tokens.TryGetValue(ConnectionType.Trust, out int trustTokens) && trustTokens >= 3)
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
                MinDepth = 3  // Deep level - letter delivery
            });
        }

        if (tokens != null && tokens.TryGetValue(ConnectionType.Commerce, out int commerceTokens) && commerceTokens >= 3)
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
                MinDepth = 3  // Deep level - letter delivery
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
            MinDepth = 2  // Intimate level - discussing obligations
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
            MinDepth = 0  // Surface level - awkward silence
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
            MinDepth = 0  // Available at surface
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
            MinDepth = 0  // Available at surface
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
            MinDepth = 0  // Available at surface
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
            MinDepth = 0  // Available at surface
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
            MinDepth = 0  // Available at surface
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
            MinDepth = 0  // Can happen at any depth
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
                MinDepth = 0  // Crisis can happen anytime
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
            MinDepth = 1  // Requires some depth
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
            MinDepth = 1  // Requires engagement
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
                MinDepth = 3  // Only at deepest level
            });
        }
    }

    /// <summary>
    /// Draw cards based on emotional state rules, filtered by depth level
    /// </summary>
    public List<ConversationCard> Draw(int count, int currentDepth = 0)
    {
        var drawn = new List<ConversationCard>();
        
        // Filter cards by depth level - only cards at or below current depth
        var availableCards = cards.Where(c => c.MinDepth <= currentDepth).ToList();
        
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
    /// Generate a crisis card for desperate states
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
    /// Check if this is a crisis deck (contains only crisis cards)
    /// </summary>
    public bool IsCrisis()
    {
        // A crisis deck contains cards with Crisis category or Crisis persistence
        return cards.Any() && cards.All(c => c.Category == CardCategory.CRISIS || c.Persistence == PersistenceType.Crisis);
    }
}