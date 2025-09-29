using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// HIGHLANDER PRINCIPLE: This is the ONE AND ONLY card deck system for conversations.
/// ALL card collections live here. NO EXCEPTIONS.
/// Uses Pile abstraction consistently - NEVER expose List<CardInstance> directly.
/// NO compatibility layers, NO legacy code, NO fallbacks.
///
/// This class manages the correct pile system:
/// - Deck (cards to draw from)
/// - Mind (hand - cards currently available to play)
/// - Spoken (conversation memory - where played cards go)
/// - Request pile (cards waiting for momentum threshold)
/// </summary>
public class SessionCardDeck
{
    // CORRECT PILE SYSTEM: Deck → Mind → Spoken
    private readonly Pile deckPile = new();      // Cards to draw from
    private readonly Pile spokenPile = new();    // Spoken pile - conversation memory
    private readonly Pile mindPile = new();      // Mind pile - cards in hand
    private readonly Pile requestPile = new();   // Cards waiting for momentum threshold

    private readonly string npcId;
    // DETERMINISTIC SYSTEM: No random number generation needed

    public SessionCardDeck(string npcId)
    {
        this.npcId = npcId;
    }

    // ENCAPSULATED: Read-only access to pile contents through Pile's IReadOnlyList property
    public IReadOnlyList<CardInstance> HandCards => mindPile.Cards;
    public IReadOnlyList<CardInstance> RequestCards => requestPile.Cards;
    public IReadOnlyList<CardInstance> SpokenCards => spokenPile.Cards;

    // Read-only properties for deck state
    public int RemainingDeckCards => deckPile.Count;
    public int SpokenPileCount => spokenPile.Count;
    public int TotalDeckCards => deckPile.Count + spokenPile.Count;
    public int HandSize => mindPile.Count;
    public int RequestPileSize => requestPile.Count;

    /// <summary>
    /// Create a deck from card templates
    /// </summary>
    public static SessionCardDeck CreateFromTemplates(List<ConversationCard> templates, string npcId)
    {
        SessionCardDeck deck = new SessionCardDeck(npcId);
        foreach (ConversationCard template in templates)
        {
            CardInstance cardInstance = new CardInstance(template);
            deck.deckPile.Add(cardInstance);
        }
        return deck;
    }

    /// <summary>
    /// Create a deck from existing card instances (preserves XP)
    /// </summary>
    public static SessionCardDeck CreateFromInstances(List<CardInstance> instances, string npcId)
    {
        SessionCardDeck deck = new SessionCardDeck(npcId);
        foreach (CardInstance instance in instances)
        {
            // Preserve context
            CardInstance sessionInstance = new CardInstance(instance.ConversationCardTemplate, instance.SourceContext)
            {
                InstanceId = instance.InstanceId,
                Context = instance.Context,
                IsPlayable = instance.IsPlayable
            };
            deck.deckPile.Add(sessionInstance);
        }
        return deck;
    }

    /// <summary>
    /// Add a card directly to the Deck pile
    /// </summary>
    public void AddCard(CardInstance card)
    {
        if (card != null)
            deckPile.Add(card);
    }

    /// <summary>
    /// Add a request/goal card that requires rapport threshold
    /// </summary>
    public void AddRequestCard(CardInstance card)
    {
        if (card != null)
            requestPile.Add(card);
    }

    /// <summary>
    /// Draw a single card (does NOT add to hand automatically)
    /// </summary>
    public CardInstance DrawCard()
    {
        // Reshuffle if needed - ONLY Standard cards reshuffle
        if (deckPile.Count == 0 && spokenPile.Count > 0)
        {
            ReshuffleSpokenPile();
        }

        if (deckPile.Count == 0)
            return null;

        CardInstance card = deckPile.DrawTop();
        AssignPreRoll(card);
        return card;
    }

    /// <summary>
    /// Draw cards directly to hand with momentum-based filtering
    /// </summary>
    public void DrawToHand(int count, int currentMomentum = int.MaxValue, PlayerStats playerStats = null)
    {
        int cardsDrawn = 0;

        for (int i = 0; i < count; i++)
        {
            // Reshuffle if needed - ONLY Standard cards reshuffle
            if (deckPile.Count == 0 && spokenPile.Count > 0)
            {
                ReshuffleSpokenPile();
            }

            // If still no cards after reshuffling, we're out of cards entirely
            if (deckPile.Count == 0)
            {
                Console.WriteLine($"[SessionCardDeck] WARNING: Requested {count} cards but only {cardsDrawn} available (Deck pile and reshuffleable Spoken cards empty)");
                break;
            }

            // Find next accessible card based on momentum and stat bonuses
            CardInstance card = DrawNextAccessibleCard(currentMomentum, playerStats);
            if (card != null)
            {
                AssignPreRoll(card);
                mindPile.Add(card);
                cardsDrawn++;
            }
            else
            {
                Console.WriteLine($"[SessionCardDeck] No more accessible cards at momentum {currentMomentum}");
                break;
            }
        }

        // Log the actual draw result
        if (cardsDrawn < count)
        {
            Console.WriteLine($"[SessionCardDeck] Drew {cardsDrawn}/{count} cards (momentum filter limited available cards)");
        }
        else
        {
            Console.WriteLine($"[SessionCardDeck] Successfully drew {cardsDrawn} cards with momentum filtering");
        }
    }

    /// <summary>
    /// Play a card - ALL cards persist on SPEAK and LISTEN
    /// Cards go to Spoken pile (conversation memory)
    /// Standard cards can reshuffle, Banish cards cannot
    /// </summary>
    public void PlayCard(CardInstance card)
    {
        if (card == null)
        {
            Console.WriteLine("[SessionCardDeck] ERROR: PlayCard called with null card!");
            return;
        }

        Console.WriteLine($"[SessionCardDeck] Playing card {card.ConversationCardTemplate.Id} from Mind to Spoken pile");

        // Track total cards before operation
        int totalCardsBefore = mindPile.Count + deckPile.Count + spokenPile.Count + requestPile.Count;
        Console.WriteLine($"[SessionCardDeck] Before play - Mind: {mindPile.Count}, Deck: {deckPile.Count}, Spoken: {spokenPile.Count}, Request: {requestPile.Count}, Total: {totalCardsBefore}");

        // Check if card exists in mind (hand) before removing
        if (!mindPile.Contains(card))
        {
            Console.WriteLine($"[SessionCardDeck] ERROR: Card {card.ConversationCardTemplate.Id} not found in mind!");
            return;
        }

        mindPile.Remove(card);

        // ALL cards go to Spoken pile (conversation memory)
        spokenPile.Add(card);

        Console.WriteLine($"[SessionCardDeck] Card {card.ConversationCardTemplate.Id} moved to Spoken pile. Spoken count: {spokenPile.Count}");

        // Validate total card count remains constant
        int totalCardsAfter = mindPile.Count + deckPile.Count + spokenPile.Count + requestPile.Count;
        Console.WriteLine($"[SessionCardDeck] After play - Mind: {mindPile.Count}, Deck: {deckPile.Count}, Spoken: {spokenPile.Count}, Request: {requestPile.Count}, Total: {totalCardsAfter}");

        if (totalCardsBefore != totalCardsAfter)
        {
            Console.WriteLine($"[SessionCardDeck] CRITICAL ERROR: Card count mismatch! Lost {totalCardsBefore - totalCardsAfter} cards!");
            Console.WriteLine($"[SessionCardDeck] Card that disappeared: {card.ConversationCardTemplate.Id} (Persistence: {card?.ConversationCardTemplate?.Persistence})");
        }
    }

    /// <summary>
    /// Move a card to Spoken pile without playing it (for special effects)
    /// </summary>
    public void MoveToSpoken(CardInstance card)
    {
        if (card != null && !spokenPile.Contains(card))
        {
            spokenPile.Add(card);
        }
    }

    /// <summary>
    /// Remove a card from Mind and move to Spoken pile
    /// </summary>
    public void MoveFromMindToSpoken(CardInstance card)
    {
        if (card == null) return;

        int totalBefore = mindPile.Count + deckPile.Count + spokenPile.Count + requestPile.Count;
        Console.WriteLine($"[SessionCardDeck] Moving card {card.ConversationCardTemplate.Id} from Mind to Spoken pile");

        mindPile.Remove(card);
        spokenPile.Add(card);

        int totalAfter = mindPile.Count + deckPile.Count + spokenPile.Count + requestPile.Count;
        if (totalBefore != totalAfter)
        {
            Console.WriteLine($"[SessionCardDeck] ERROR: Card lost during move to spoken! {card.ConversationCardTemplate.Id} disappeared");
        }
    }

    /// <summary>
    /// Check request pile and move cards to mind if momentum threshold met
    /// Returns the list of cards that were moved
    /// </summary>
    public List<CardInstance> CheckRequestThresholds(int currentMomentum)
    {
        List<CardInstance> toMove = requestPile.Cards
            .Where(c => c.Context?.MomentumThreshold <= currentMomentum)
            .ToList();

        List<CardInstance> movedCards = new List<CardInstance>();

        foreach (CardInstance? card in toMove)
        {
            int totalBefore = mindPile.Count + deckPile.Count + spokenPile.Count + requestPile.Count;

            requestPile.Remove(card);
            mindPile.Add(card);
            Console.WriteLine($"[SessionCardDeck] Request card {card.ConversationCardTemplate.Id} moved to mind (momentum {currentMomentum})");
            movedCards.Add(card);

            int totalAfter = mindPile.Count + deckPile.Count + spokenPile.Count + requestPile.Count;
            if (totalBefore != totalAfter)
            {
                Console.WriteLine($"[SessionCardDeck] ERROR: Card count changed during request move! Expected {totalBefore}, got {totalAfter}");
            }
        }

        return movedCards;
    }

    /// <summary>
    /// Shuffle the Deck pile
    /// </summary>
    public void ShuffleDeckPile()
    {
        deckPile.Shuffle();
    }

    /// <summary>
    /// Reshuffle ONLY Standard cards from Spoken pile back into Deck pile
    /// Banish cards stay in Spoken pile permanently
    /// </summary>
    private void ReshuffleSpokenPile()
    {
        List<CardInstance> standardCards = spokenPile.Cards
            .Where(card => card?.ConversationCardTemplate?.Persistence == PersistenceType.Statement)
            .ToList();

        Console.WriteLine($"[SessionCardDeck] Reshuffling {standardCards.Count} Standard cards from spoken into deck (leaving {spokenPile.Count - standardCards.Count} Banish cards in spoken)");

        // Remove ONLY Standard cards from Spoken pile
        foreach (CardInstance card in standardCards)
        {
            spokenPile.Remove(card);
        }

        // Move Standard cards to Deck pile
        deckPile.AddRange(standardCards);
        deckPile.Shuffle();
    }

    /// <summary>
    /// Assign pre-rolled dice value to a card
    /// </summary>
    private void AssignPreRoll(CardInstance card)
    {
        if (card == null) return;

        if (card.Context == null)
            card.Context = new CardContext();

        // DETERMINISTIC SYSTEM: No pre-rolled values needed - removed dice logic
    }

    /// <summary>
    /// Check if cards are available (in deck or can reshuffle Standard cards from spoken)
    /// </summary>
    public bool HasCardsAvailable()
    {
        bool hasInDeck = deckPile.Count > 0;
        bool hasReshufflableInSpoken = spokenPile.Cards.Any(card => card?.ConversationCardTemplate?.Persistence == PersistenceType.Statement);

        return hasInDeck || hasReshufflableInSpoken;
    }

    /// <summary>
    /// Clear all piles (for cleanup)
    /// </summary>
    public void Clear()
    {
        deckPile.Clear();
        spokenPile.Clear();
        mindPile.Clear();
        requestPile.Clear();
    }

    /// <summary>
    /// Reset for a new conversation (moves all cards back to Deck pile)
    /// </summary>
    public void ResetForNewConversation()
    {
        // Move all cards from all piles back to deck
        deckPile.AddRange(spokenPile.DrawAll());
        deckPile.AddRange(mindPile.DrawAll());
        deckPile.AddRange(requestPile.DrawAll());

        // Shuffle for fresh start
        deckPile.Shuffle();
    }

    /// <summary>
    /// Check if a card is in the mind (hand)
    /// </summary>
    public bool IsCardInMind(CardInstance card)
    {
        return mindPile.Contains(card);
    }

    /// <summary>
    /// Check if a card is in the request pile
    /// </summary>
    public bool IsCardInRequestPile(CardInstance card)
    {
        return requestPile.Contains(card);
    }

    /// <summary>
    /// Add a card directly to mind (use carefully - prefer DrawToHand)
    /// </summary>
    public void AddCardToMind(CardInstance card)
    {
        if (card != null)
        {
            int totalBefore = mindPile.Count + deckPile.Count + spokenPile.Count + requestPile.Count;
            mindPile.Add(card);
            int totalAfter = mindPile.Count + deckPile.Count + spokenPile.Count + requestPile.Count;

            if (totalBefore + 1 != totalAfter)
            {
                Console.WriteLine($"[SessionCardDeck] ERROR: Card count mismatch when adding to mind! Expected {totalBefore + 1}, got {totalAfter}");
            }
        }
    }

    /// <summary>
    /// Add multiple cards directly to mind (for Threading effect)
    /// </summary>
    public void AddCardsToMind(List<CardInstance> cards)
    {
        if (cards == null || cards.Count == 0) return;

        int totalBefore = mindPile.Count + deckPile.Count + spokenPile.Count + requestPile.Count;
        Console.WriteLine($"[SessionCardDeck] Adding {cards.Count} cards to mind");

        foreach (CardInstance card in cards)
        {
            if (card != null)
            {
                mindPile.Add(card);
            }
        }

        int totalAfter = mindPile.Count + deckPile.Count + spokenPile.Count + requestPile.Count;
        if (totalBefore + cards.Count != totalAfter)
        {
            Console.WriteLine($"[SessionCardDeck] ERROR: Card count mismatch when adding cards to mind! Expected {totalBefore + cards.Count}, got {totalAfter}");
        }
    }

    /// <summary>
    /// Process card persistence - ALL cards persist in refined system
    /// Standard and Banish cards both stay in Spoken pile after playing
    /// NO cards are lost during LISTEN actions
    /// </summary>
    public void ProcessCardPersistence()
    {
        // In the refined system, ALL cards persist on SPEAK and LISTEN
        // Cards only go to Spoken pile when explicitly played
        // No cards are removed during LISTEN actions
        // The difference between Standard and Banish is only in reshuffling behavior

        Console.WriteLine($"[SessionCardDeck] Processing persistence - ALL cards persist in refined system");
        Console.WriteLine($"[SessionCardDeck] Current state - Mind: {mindPile.Count}, Spoken: {spokenPile.Count}, Deck: {deckPile.Count}");

        // Nothing to process - all cards stay where they are during LISTEN
    }

    /// <summary>
    /// Check if a card can be accessed based on momentum and stat specialization bonuses
    /// Cards accessible if depth <= momentum OR (has matching stat AND depth <= momentum + stat bonus)
    /// Foundation cards (depth 1-2) are always accessible
    /// </summary>
    private bool CanAccessCard(ConversationCard card, int currentMomentum, PlayerStats playerStats)
    {
        if (card == null) return false;

        int cardDepth = (int)card.Depth;

        // Foundation cards (depth 1-2) are always accessible
        if (cardDepth <= 2) return true;

        // Basic momentum gate: card accessible if depth <= momentum
        if (cardDepth <= currentMomentum) return true;

        // Stat specialization bonus: if card has bound stat, check for bonus access
        if (card.BoundStat.HasValue && playerStats != null)
        {
            int statBonus = playerStats.GetDepthBonus(card.BoundStat.Value);
            return cardDepth <= currentMomentum + statBonus;
        }

        return false;
    }

    /// <summary>
    /// Draw the next accessible card from the deck pile based on momentum filtering
    /// Returns null if no accessible cards are available
    /// </summary>
    private CardInstance DrawNextAccessibleCard(int currentMomentum, PlayerStats playerStats)
    {
        // Find accessible cards in the deck pile
        List<CardInstance> accessibleCards = deckPile.Cards
            .Where(card => CanAccessCard(card.ConversationCardTemplate, currentMomentum, playerStats))
            .ToList();

        if (accessibleCards.Count == 0)
        {
            Console.WriteLine($"[SessionCardDeck] No accessible cards in deck at momentum {currentMomentum}");
            return null;
        }

        // Take the first accessible card (maintains deck order but applies filter)
        CardInstance selectedCard = accessibleCards.First();
        deckPile.Remove(selectedCard);

        Console.WriteLine($"[SessionCardDeck] Drew accessible card: {selectedCard.ConversationCardTemplate.Title} (depth {(int)selectedCard.ConversationCardTemplate.Depth})");
        return selectedCard;
    }

    // NO DIRECT PILE ACCESS - ALL OPERATIONS GO THROUGH METHODS!
}