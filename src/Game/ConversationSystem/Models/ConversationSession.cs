using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents an active conversation session with an NPC.
/// Tracks all state for the duration of the conversation.
/// </summary>
public class ConversationSession
{
    /// <summary>
    /// The NPC in conversation
    /// </summary>
    public NPC NPC { get; init; }

    /// <summary>
    /// Current emotional state (changes through play)
    /// </summary>
    public EmotionalState CurrentState { get; set; }

    /// <summary>
    /// Cards currently in hand
    /// </summary>
    public List<ConversationCard> HandCards { get; set; }

    /// <summary>
    /// The NPC's conversation deck
    /// </summary>
    public CardDeck Deck { get; init; }

    /// <summary>
    /// Current patience (turns remaining)
    /// </summary>
    public int CurrentPatience { get; set; }

    /// <summary>
    /// Maximum patience at conversation start
    /// </summary>
    public int MaxPatience { get; init; }

    /// <summary>
    /// Comfort built so far
    /// </summary>
    public int CurrentComfort { get; set; }

    /// <summary>
    /// Current conversation depth (0-3)
    /// </summary>
    public int CurrentDepth { get; set; }

    /// <summary>
    /// Turn number
    /// </summary>
    public int TurnNumber { get; set; }

    /// <summary>
    /// Whether a letter has been generated this conversation
    /// </summary>
    public bool LetterGenerated { get; set; }

    /// <summary>
    /// Observation cards added at start
    /// </summary>
    public List<ConversationCard> ObservationCards { get; set; }

    /// <summary>
    /// Initialize a new conversation session
    /// </summary>
    public static ConversationSession StartConversation(
        NPC npc,
        ObligationQueueManager queueManager,
        NPCRelationshipTracker relationshipTracker,
        TokenMechanicsManager tokenManager,
        List<ConversationCard> observationCards,
        ConversationType conversationType)
    {
        // Determine initial state based on NPC condition
        var initialState = ConversationRules.DetermineInitialState(npc, queueManager);
        
        // Calculate starting patience
        var basePatience = GetBasePatience(npc.PersonalityType);
        var relationship = relationshipTracker.GetRelationship(npc.ID);
        var trustBonus = relationship.Trust / 2; // +1 patience per 2 trust
        
        // Apply emotional state penalties
        var statepenalty = initialState switch
        {
            EmotionalState.DESPERATE => -3,
            EmotionalState.TENSE => -1,
            _ => 0
        };
        
        var totalPatience = Math.Max(3, basePatience + trustBonus + statepenalty);

        // Create and initialize deck
        var deck = new CardDeck();
        deck.InitializeForNPC(npc, tokenManager);

        // Draw initial hand
        var handCards = new List<ConversationCard>();
        handCards.AddRange(deck.Draw(3)); // Base 3 cards
        
        // Add observation cards if any
        if (observationCards != null && observationCards.Any())
        {
            handCards.AddRange(observationCards);
        }

        return new ConversationSession
        {
            NPC = npc,
            CurrentState = initialState,
            HandCards = handCards,
            Deck = deck,
            CurrentPatience = totalPatience,
            MaxPatience = totalPatience,
            CurrentComfort = 0,
            CurrentDepth = 0,
            TurnNumber = 0,
            LetterGenerated = false,
            ObservationCards = observationCards ?? new List<ConversationCard>()
        };
    }

    /// <summary>
    /// Start a Quick Exchange conversation (simplified, no emotional states)
    /// </summary>
    public static ConversationSession StartExchange(NPC npc, PlayerResourceState resourceState, TokenMechanicsManager tokenManager)
    {
        // Initialize exchange deck if not already done
        npc.InitializeExchangeDeck();
        
        // Create simplified session for exchanges
        return new ConversationSession
        {
            NPC = npc,
            CurrentState = EmotionalState.NEUTRAL, // No emotional states in exchanges
            HandCards = new List<ConversationCard>(), // Exchange cards are drawn on demand
            Deck = new CardDeck(), // Empty deck - exchanges use ExchangeDeck instead
            CurrentPatience = 1, // Single turn exchange
            MaxPatience = 1,
            CurrentComfort = 0, // No comfort in exchanges
            CurrentDepth = 0,
            TurnNumber = 0,
            LetterGenerated = false,
            ObservationCards = new List<ConversationCard>()
        };
    }

    /// <summary>
    /// Start a Crisis conversation (forced resolution)
    /// </summary>
    public static ConversationSession StartCrisis(NPC npc, ObligationQueueManager queueManager, NPCRelationshipTracker relationshipTracker, TokenMechanicsManager tokenManager, List<ConversationCard> observationCards)
    {
        // Crisis conversations always start in DESPERATE state
        var initialState = EmotionalState.DESPERATE;
        
        // Crisis conversations have limited patience
        var basePatience = 3; // Fixed 3 patience for crisis
        
        // Create crisis deck if needed
        var deck = npc.CrisisDeck ?? new CardDeck();
        
        // Draw initial hand
        var handCards = new List<ConversationCard>();
        handCards.AddRange(deck.Draw(2)); // Start with 2 cards
        
        // Add observation cards if any
        if (observationCards != null && observationCards.Any())
        {
            handCards.AddRange(observationCards);
        }

        return new ConversationSession
        {
            NPC = npc,
            CurrentState = initialState,
            HandCards = handCards,
            Deck = deck,
            CurrentPatience = basePatience,
            MaxPatience = basePatience,
            CurrentComfort = 0,
            CurrentDepth = 0,
            TurnNumber = 0,
            LetterGenerated = false,
            ObservationCards = observationCards ?? new List<ConversationCard>()
        };
    }

    /// <summary>
    /// Execute LISTEN action
    /// </summary>
    public void ExecuteListen()
    {
        TurnNumber++;
        CurrentPatience--;

        // Remove all opportunity cards
        HandCards.RemoveAll(c => c.Persistence == PersistenceType.Opportunity);

        // Get state rules
        var rules = ConversationRules.States[CurrentState];

        // Draw new cards
        var newCards = Deck.Draw(rules.CardsOnListen);
        HandCards.AddRange(newCards);

        // Inject crisis if needed
        if (rules.InjectsCrisis)
        {
            var crisisCard = Deck.GenerateCrisisCard(NPC);
            HandCards.Add(crisisCard);
        }

        // Transition state
        CurrentState = rules.ListenTransition;

        // Check depth advancement
        CheckDepthAdvancement();
    }

    /// <summary>
    /// Execute SPEAK action with selected cards
    /// </summary>
    public CardPlayResult ExecuteSpeak(HashSet<ConversationCard> selectedCards, int statusTokens)
    {
        TurnNumber++;
        CurrentPatience--;

        var manager = new CardSelectionManager(CurrentState);
        foreach (var card in selectedCards)
        {
            manager.ToggleCard(card);
        }

        var result = manager.PlaySelectedCards(statusTokens);

        // Apply comfort
        CurrentComfort += result.TotalComfort;

        // Apply state change if any
        if (result.NewState.HasValue)
        {
            CurrentState = result.NewState.Value;
        }

        // Remove played cards from hand
        foreach (var card in selectedCards)
        {
            HandCards.Remove(card);
            
            // Handle different persistence types
            if (card.Persistence == PersistenceType.OneShot)
            {
                Deck.RemoveCard(card); // Permanently remove
            }
            else
            {
                Deck.Discard(card); // Will return to deck later
            }
        }

        // Check depth advancement
        CheckDepthAdvancement();

        return result;
    }

    /// <summary>
    /// Check if depth should advance
    /// </summary>
    private void CheckDepthAdvancement()
    {
        var rules = ConversationRules.States[CurrentState];
        
        // CONNECTED auto-advances depth
        if (rules.AutoAdvanceDepth)
        {
            CurrentDepth = Math.Min(3, CurrentDepth + 1);
            return;
        }

        // Check for breakthrough advancement
        if (CurrentState == EmotionalState.NEUTRAL || 
            CurrentState == EmotionalState.OPEN)
        {
            // 10+ comfort in single turn advances depth
            // (Would need to track per-turn comfort for this)
        }
    }

    /// <summary>
    /// Check comfort thresholds for rewards
    /// </summary>
    public ConversationOutcome CheckThresholds()
    {
        var outcome = new ConversationOutcome
        {
            TotalComfort = CurrentComfort,
            FinalDepth = CurrentDepth,
            FinalState = CurrentState,
            TurnsUsed = TurnNumber
        };

        if (CurrentComfort >= 20)
        {
            outcome.TokensEarned = 3;
            outcome.LetterUnlocked = true;
            outcome.PerfectConversation = true;
        }
        else if (CurrentComfort >= 15)
        {
            outcome.TokensEarned = 2;
            outcome.LetterUnlocked = true;
        }
        else if (CurrentComfort >= 10)
        {
            outcome.TokensEarned = 1;
            outcome.LetterUnlocked = true;
        }
        else if (CurrentComfort >= 5)
        {
            outcome.TokensEarned = 1;
        }
        else
        {
            outcome.TokensEarned = -1; // Relationship damage
        }

        return outcome;
    }

    /// <summary>
    /// Check if conversation should end
    /// </summary>
    public bool ShouldEnd()
    {
        return CurrentPatience <= 0 || 
               CurrentState == EmotionalState.HOSTILE ||
               Deck.IsEmpty;
    }

    /// <summary>
    /// Check if hand is overflowing (forces SPEAK)
    /// </summary>
    public bool IsHandOverflowing()
    {
        return HandCards.Count > 7;
    }

    private static int GetBasePatience(PersonalityType personality)
    {
        return personality switch
        {
            PersonalityType.DEVOTED => 12,
            PersonalityType.MERCANTILE => 10,
            PersonalityType.PROUD => 8,
            PersonalityType.CUNNING => 10,
            PersonalityType.STEADFAST => 11,
            _ => 10
        };
    }
}