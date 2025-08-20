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
            Text = "How has your day been?",
            Type = CardType.Trust,
            Persistence = PersistenceType.Persistent,
            Weight = 0,
            BaseComfort = 1
        });

        cards.Add(new ConversationCard
        {
            Id = "listen_actively",
            Text = "*nod and listen attentively*",
            Type = CardType.Trust,
            Persistence = PersistenceType.Persistent,
            Weight = 0,
            BaseComfort = 1
        });

        cards.Add(new ConversationCard
        {
            Id = "share_news",
            Text = "I heard something interesting in the market...",
            Type = CardType.Status,
            Persistence = PersistenceType.Persistent,
            Weight = 1,
            BaseComfort = 2
        });

        cards.Add(new ConversationCard
        {
            Id = "offer_help",
            Text = "Is there anything I can do for you?",
            Type = CardType.Trust,
            Persistence = PersistenceType.Persistent,
            Weight = 1,
            BaseComfort = 3
        });

        cards.Add(new ConversationCard
        {
            Id = "weather_comment",
            Text = "The weather has been unusual lately...",
            Type = CardType.Trust,
            Persistence = PersistenceType.Persistent,
            Weight = 0,
            BaseComfort = 1
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
                    Text = "I've been worried about you...",
                    Type = CardType.Trust,
                    Persistence = PersistenceType.Opportunity,
                    Weight = 2,
                    BaseComfort = 4
                });
                cards.Add(new ConversationCard
                {
                    Id = "family_story",
                    Text = "My family has a saying about times like these...",
                    Type = CardType.Trust,
                    Persistence = PersistenceType.Persistent,
                    Weight = 1,
                    BaseComfort = 3
                });
                break;

            case PersonalityType.MERCANTILE:
                // Commerce-focused cards
                cards.Add(new ConversationCard
                {
                    Id = "trade_opportunity",
                    Text = "I know of a profitable opportunity...",
                    Type = CardType.Commerce,
                    Persistence = PersistenceType.Opportunity,
                    Weight = 2,
                    BaseComfort = 4
                });
                cards.Add(new ConversationCard
                {
                    Id = "negotiate_terms",
                    Text = "Perhaps we can come to an arrangement?",
                    Type = CardType.Commerce,
                    Persistence = PersistenceType.Persistent,
                    Weight = 1,
                    BaseComfort = 3
                });
                break;

            case PersonalityType.PROUD:
                // Status-focused cards
                cards.Add(new ConversationCard
                {
                    Id = "social_position",
                    Text = "Your reputation precedes you...",
                    Type = CardType.Status,
                    Persistence = PersistenceType.Persistent,
                    Weight = 1,
                    BaseComfort = 3
                });
                cards.Add(new ConversationCard
                {
                    Id = "noble_bearing",
                    Text = "One must maintain proper standards...",
                    Type = CardType.Status,
                    Persistence = PersistenceType.Opportunity,
                    Weight = 2,
                    BaseComfort = 4
                });
                break;

            case PersonalityType.CUNNING:
                // Shadow-focused cards
                cards.Add(new ConversationCard
                {
                    Id = "subtle_hint",
                    Text = "Between you and me...",
                    Type = CardType.Shadow,
                    Persistence = PersistenceType.Opportunity,
                    Weight = 1,
                    BaseComfort = 3
                });
                cards.Add(new ConversationCard
                {
                    Id = "hidden_knowledge",
                    Text = "I know something others don't...",
                    Type = CardType.Shadow,
                    Persistence = PersistenceType.Opportunity,
                    Weight = 2,
                    BaseComfort = 5
                });
                break;

            case PersonalityType.STEADFAST:
                // Balanced cards
                cards.Add(new ConversationCard
                {
                    Id = "honest_opinion",
                    Text = "I'll tell you what I really think...",
                    Type = CardType.Trust,
                    Persistence = PersistenceType.Persistent,
                    Weight = 2,
                    BaseComfort = 4
                });
                cards.Add(new ConversationCard
                {
                    Id = "practical_advice",
                    Text = "Here's what I'd do in your position...",
                    Type = CardType.Commerce,
                    Persistence = PersistenceType.Persistent,
                    Weight = 1,
                    BaseComfort = 3
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
                Text = "I need you to deliver something personal for me...",
                Type = CardType.Trust,
                Persistence = PersistenceType.Persistent,
                Weight = 2,
                BaseComfort = 5,
                CanDeliverLetter = true
            });
        }

        if (tokens != null && tokens.TryGetValue(ConnectionType.Commerce, out int commerceTokens) && commerceTokens >= 3)
        {
            cards.Add(new ConversationCard
            {
                Id = "request_commerce_letter",
                Text = "I have a business matter that needs delivery...",
                Type = CardType.Commerce,
                Persistence = PersistenceType.Persistent,
                Weight = 2,
                BaseComfort = 5,
                CanDeliverLetter = true
            });
        }

        // Add obligation manipulation cards if there are active obligations
        cards.Add(new ConversationCard
        {
            Id = "discuss_obligation",
            Text = "About that letter you're carrying for me...",
            Type = CardType.Trust,
            Persistence = PersistenceType.Opportunity,
            Weight = 1,
            BaseComfort = 2,
            ManipulatesObligations = true
        });
    }

    private void AddInitialBurden()
    {
        cards.Add(new ConversationCard
        {
            Id = "awkward_silence",
            Text = "*uncomfortable pause*",
            Type = CardType.Trust,
            Persistence = PersistenceType.Burden,
            Weight = 1,
            BaseComfort = 0
        });
    }

    /// <summary>
    /// Draw cards based on emotional state rules
    /// </summary>
    public List<ConversationCard> Draw(int count)
    {
        var drawn = new List<ConversationCard>();
        
        for (int i = 0; i < count && cards.Any(); i++)
        {
            var card = cards.First();
            cards.RemoveAt(0);
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
            Text = "Please, I'm desperate! You must help me!",
            Type = CardType.Trust,
            Persistence = PersistenceType.Crisis,
            Weight = 5, // Heavy but free in DESPERATE
            BaseComfort = 8,
            IsCrisis = true
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
                Text = "Take a breath. We'll figure this out together.",
                Type = CardType.Trust,
                Persistence = PersistenceType.Persistent,
                Weight = 1,
                BaseComfort = 2,
                IsStateCard = true,
                SuccessState = EmotionalState.TENSE,
                FailureState = currentState
            },
            EmotionalState.GUARDED => new ConversationCard
            {
                Id = "break_ice",
                Text = "I understand your hesitation, but I'm here to help.",
                Type = CardType.Trust,
                Persistence = PersistenceType.Persistent,
                Weight = 1,
                BaseComfort = 2,
                IsStateCard = true,
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
}