using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// HIGHLANDER PRINCIPLE: This is the ONE AND ONLY card deck system for conversations.
/// ALL card collections live here. NO EXCEPTIONS.
/// Uses Pile abstraction consistently - NEVER expose List<CardInstance> directly.
/// NO compatibility layers, NO legacy code, NO fallbacks.
///
/// This class manages:
/// - Draw pile (cards to draw from)
/// - Discard pile (cards that were used, will reshuffle)
/// - Hand pile (cards currently available to play)
/// - Request pile (cards waiting for rapport threshold)
/// - Played pile (history of cards played this conversation)
/// </summary>
public class SessionCardDeck
{
    // HIGHLANDER: ALL card piles use Pile abstraction - NO List<CardInstance>!
    private readonly Pile drawPile = new();
    private readonly Pile discardPile = new();
    private readonly Pile handPile = new();      // Was ConversationSession.ActiveCards
    private readonly Pile requestPile = new();   // Was ConversationSession.RequestPile
    private readonly Pile playedPile = new();    // Was ConversationSession.PlayedCards

    private readonly string npcId;
    // DETERMINISTIC SYSTEM: No random number generation needed

    public SessionCardDeck(string npcId)
    {
        this.npcId = npcId;
    }

    // ENCAPSULATED: Read-only access to pile contents through Pile's IReadOnlyList property
    public IReadOnlyList<CardInstance> HandCards => handPile.Cards;
    public IReadOnlyList<CardInstance> RequestCards => requestPile.Cards;
    public IReadOnlyList<CardInstance> PlayedHistoryCards => playedPile.Cards;

    // Read-only properties for deck state
    public int RemainingDrawCards => drawPile.Count;
    public int DiscardPileCount => discardPile.Count;
    public int TotalDeckCards => drawPile.Count + discardPile.Count;
    public int HandSize => handPile.Count;
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
            deck.drawPile.Add(cardInstance);
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
            CardInstance sessionInstance = new CardInstance(instance.Template, instance.SourceContext)
            {
                InstanceId = instance.InstanceId,
                Context = instance.Context,
                IsPlayable = instance.IsPlayable
            };
            deck.drawPile.Add(sessionInstance);
        }
        return deck;
    }

    /// <summary>
    /// Add a card directly to the draw pile
    /// </summary>
    public void AddCard(CardInstance card)
    {
        if (card != null)
            drawPile.Add(card);
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
        // Reshuffle if needed
        if (drawPile.Count == 0 && discardPile.Count > 0)
        {
            ReshuffleDiscardPile();
        }

        if (drawPile.Count == 0)
            return null;

        CardInstance card = drawPile.DrawTop();
        AssignPreRoll(card);
        return card;
    }

    /// <summary>
    /// Draw cards directly to hand
    /// </summary>
    public void DrawToHand(int count)
    {
        int cardsDrawn = 0;

        for (int i = 0; i < count; i++)
        {
            // Reshuffle if needed
            if (drawPile.Count == 0 && discardPile.Count > 0)
            {
                ReshuffleDiscardPile();
            }

            // If still no cards after reshuffling, we're out of cards entirely
            if (drawPile.Count == 0)
            {
                Console.WriteLine($"[SessionCardDeck] WARNING: Requested {count} cards but only {cardsDrawn} available (draw pile and discard both empty)");
                break;
            }

            CardInstance card = drawPile.DrawTop();
            if (card != null)
            {
                AssignPreRoll(card);
                handPile.Add(card);
                cardsDrawn++;
            }
        }

        // Log the actual draw result
        if (cardsDrawn < count)
        {
            Console.WriteLine($"[SessionCardDeck] Drew {cardsDrawn}/{count} cards (not enough cards in circulation)");
        }
        else
        {
            Console.WriteLine($"[SessionCardDeck] Successfully drew {cardsDrawn} cards");
        }
    }

    /// <summary>
    /// Play a card - removes from hand, adds to discard pile
    /// CRITICAL: This method MUST ensure cards go to discard pile for reshuffling
    /// </summary>
    public void PlayCard(CardInstance card)
    {
        if (card == null)
        {
            Console.WriteLine("[SessionCardDeck] ERROR: PlayCard called with null card!");
            return;
        }

        Console.WriteLine($"[SessionCardDeck] Playing card {card.Id} from hand");

        // Track total cards before operation (excluding playedPile which is never used)
        int totalCardsBefore = handPile.Count + drawPile.Count + discardPile.Count + requestPile.Count;
        Console.WriteLine($"[SessionCardDeck] Before play - Hand: {handPile.Count}, Draw: {drawPile.Count}, Discard: {discardPile.Count}, Request: {requestPile.Count}, Total: {totalCardsBefore}");

        // Check if card exists in hand before removing
        if (!handPile.Contains(card))
        {
            Console.WriteLine($"[SessionCardDeck] ERROR: Card {card.Id} not found in hand!");
            return;
        }

        handPile.Remove(card);

        // Card goes to discard pile for reshuffling
        discardPile.Add(card);

        Console.WriteLine($"[SessionCardDeck] Card {card.Id} moved to discard. Discard count: {discardPile.Count}");

        // Validate total card count remains constant (excluding unused playedPile)
        int totalCardsAfter = handPile.Count + drawPile.Count + discardPile.Count + requestPile.Count;
        Console.WriteLine($"[SessionCardDeck] After play - Hand: {handPile.Count}, Draw: {drawPile.Count}, Discard: {discardPile.Count}, Request: {requestPile.Count}, Total: {totalCardsAfter}");

        if (totalCardsBefore != totalCardsAfter)
        {
            Console.WriteLine($"[SessionCardDeck] CRITICAL ERROR: Card count mismatch! Lost {totalCardsBefore - totalCardsAfter} cards!");
            Console.WriteLine($"[SessionCardDeck] Card that disappeared: {card.Id} (Type: {card.CardType}, Persistence: {card.Persistence})");
        }
    }

    /// <summary>
    /// Discard a card without playing it (e.g., exhausted cards)
    /// </summary>
    public void DiscardCard(CardInstance card)
    {
        if (card != null && !discardPile.Contains(card))
        {
            discardPile.Add(card);
        }
    }

    /// <summary>
    /// Remove a card from hand and discard it (for exhaust effects)
    /// </summary>
    public void ExhaustFromHand(CardInstance card)
    {
        if (card == null) return;

        int totalBefore = handPile.Count + drawPile.Count + discardPile.Count + requestPile.Count;
        Console.WriteLine($"[SessionCardDeck] Exhausting card {card.Id} from hand");

        handPile.Remove(card);
        discardPile.Add(card);

        int totalAfter = handPile.Count + drawPile.Count + discardPile.Count + requestPile.Count;
        if (totalBefore != totalAfter)
        {
            Console.WriteLine($"[SessionCardDeck] ERROR: Card lost during exhaust! {card.Id} disappeared");
        }
    }

    /// <summary>
    /// Check request pile and move cards to hand if momentum threshold met
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
            int totalBefore = handPile.Count + drawPile.Count + discardPile.Count + requestPile.Count;

            requestPile.Remove(card);
            handPile.Add(card);
            Console.WriteLine($"[SessionCardDeck] Request card {card.Id} moved to hand (momentum {currentMomentum})");
            movedCards.Add(card);

            int totalAfter = handPile.Count + drawPile.Count + discardPile.Count + requestPile.Count;
            if (totalBefore != totalAfter)
            {
                Console.WriteLine($"[SessionCardDeck] ERROR: Card count changed during request move! Expected {totalBefore}, got {totalAfter}");
            }
        }

        return movedCards;
    }

    /// <summary>
    /// Shuffle the draw pile
    /// </summary>
    public void ShuffleDrawPile()
    {
        drawPile.Shuffle();
    }

    /// <summary>
    /// Reshuffle discard pile back into draw pile
    /// </summary>
    private void ReshuffleDiscardPile()
    {
        Console.WriteLine($"[SessionCardDeck] Reshuffling {discardPile.Count} cards from discard into draw");

        // Move all discard to draw using Pile methods
        List<CardInstance> allDiscards = discardPile.DrawAll();
        drawPile.AddRange(allDiscards);
        drawPile.Shuffle();
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
    /// Check if cards are available (in draw or can reshuffle from discard)
    /// </summary>
    public bool HasCardsAvailable()
    {
        return drawPile.Count > 0 || discardPile.Count > 0;
    }

    /// <summary>
    /// Clear all piles (for cleanup)
    /// </summary>
    public void Clear()
    {
        drawPile.Clear();
        discardPile.Clear();
        handPile.Clear();
        requestPile.Clear();
        playedPile.Clear();
    }

    /// <summary>
    /// Reset for a new conversation (moves all cards back to draw pile)
    /// </summary>
    public void ResetForNewConversation()
    {
        // Move all cards from all piles back to draw
        drawPile.AddRange(discardPile.DrawAll());
        drawPile.AddRange(handPile.DrawAll());
        drawPile.AddRange(requestPile.DrawAll());
        drawPile.AddRange(playedPile.DrawAll());

        // Shuffle for fresh start
        drawPile.Shuffle();
    }

    /// <summary>
    /// Check if a card is in the hand
    /// </summary>
    public bool IsCardInHand(CardInstance card)
    {
        return handPile.Contains(card);
    }

    /// <summary>
    /// Check if a card is in the request pile
    /// </summary>
    public bool IsCardInRequestPile(CardInstance card)
    {
        return requestPile.Contains(card);
    }

    /// <summary>
    /// Add a card directly to hand (use carefully - prefer DrawToHand)
    /// </summary>
    public void AddCardToHand(CardInstance card)
    {
        if (card != null)
        {
            int totalBefore = handPile.Count + drawPile.Count + discardPile.Count + requestPile.Count;
            handPile.Add(card);
            int totalAfter = handPile.Count + drawPile.Count + discardPile.Count + requestPile.Count;

            if (totalBefore + 1 != totalAfter)
            {
                Console.WriteLine($"[SessionCardDeck] ERROR: Card count mismatch when adding to hand! Expected {totalBefore + 1}, got {totalAfter}");
            }
        }
    }

    /// <summary>
    /// Add multiple cards directly to hand (for Threading effect)
    /// </summary>
    public void AddCardsToHand(List<CardInstance> cards)
    {
        if (cards == null || cards.Count == 0) return;

        int totalBefore = handPile.Count + drawPile.Count + discardPile.Count + requestPile.Count;
        Console.WriteLine($"[SessionCardDeck] Adding {cards.Count} cards to hand");

        foreach (CardInstance card in cards)
        {
            if (card != null)
            {
                handPile.Add(card);
            }
        }

        int totalAfter = handPile.Count + drawPile.Count + discardPile.Count + requestPile.Count;
        if (totalBefore + cards.Count != totalAfter)
        {
            Console.WriteLine($"[SessionCardDeck] ERROR: Card count mismatch when adding cards to hand! Expected {totalBefore + cards.Count}, got {totalAfter}");
        }
    }

    // NO DIRECT PILE ACCESS - ALL OPERATIONS GO THROUGH METHODS!
}