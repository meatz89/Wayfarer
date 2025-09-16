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
    private readonly Random random = new Random();

    public SessionCardDeck(string npcId)
    {
        this.npcId = npcId;
    }

    // Direct Pile access - NO compatibility wrappers, NO IReadOnlyList
    public Pile Hand => handPile;
    public Pile RequestCards => requestPile;
    public Pile PlayedHistory => playedPile;

    // Read-only properties for deck state
    public int RemainingDrawCards => drawPile.Count;
    public int DiscardPileCount => discardPile.Count;
    public int TotalDeckCards => drawPile.Count + discardPile.Count;
    public int HandSize => handPile.Count;

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
            // Preserve XP and context
            CardInstance sessionInstance = new CardInstance(instance.Template, instance.SourceContext)
            {
                XP = instance.XP,
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

        var card = drawPile.DrawTop();
        AssignPreRoll(card);
        return card;
    }

    /// <summary>
    /// Draw cards directly to hand
    /// </summary>
    public void DrawToHand(int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Reshuffle if needed
            if (drawPile.Count == 0 && discardPile.Count > 0)
            {
                ReshuffleDiscardPile();
            }

            if (drawPile.Count > 0)
            {
                var card = drawPile.DrawTop();
                if (card != null)
                {
                    AssignPreRoll(card);
                    handPile.Add(card);
                }
            }
        }
    }

    /// <summary>
    /// Play a card - removes from hand, adds to played history and discard pile
    /// </summary>
    public void PlayCard(CardInstance card)
    {
        if (card == null) return;

        handPile.Remove(card);
        playedPile.Add(card);
        discardPile.Add(card);  // For reshuffling later
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

        handPile.Remove(card);
        discardPile.Add(card);
    }

    /// <summary>
    /// Check request pile and move cards to hand if rapport threshold met
    /// </summary>
    public void CheckRequestThresholds(int currentRapport)
    {
        var toMove = requestPile.Cards
            .Where(c => c.Context?.RapportThreshold <= currentRapport)
            .ToList();

        foreach (var card in toMove)
        {
            requestPile.Remove(card);
            handPile.Add(card);
            Console.WriteLine($"[SessionCardDeck] Request card {card.Id} moved to hand (rapport {currentRapport})");
        }
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
        var allDiscards = discardPile.DrawAll();
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

        // Only roll if not already rolled
        if (card.Context.PreRolledValue == null)
        {
            card.Context.PreRolledValue = random.Next(1, 101);
            Console.WriteLine($"[SessionCardDeck] Pre-rolled {card.Context.PreRolledValue} for card: {card.Id}");
        }
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

    // NO COMPATIBILITY METHODS - ALL CALLERS MUST BE UPDATED!
}