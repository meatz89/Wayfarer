using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Physical session deck - runtime pile management for Physical engagements
/// Parallel to SessionCardDeck and MentalSessionDeck but for Physical tactical system
/// </summary>
public class PhysicalSessionDeck
{
    // Pile system: Deck → Hand → Played
    private readonly Pile deckPile = new();
    private readonly Pile handPile = new();
    private readonly Pile playedPile = new();
    private readonly Pile requestPile = new();  // GOAL CARDS for this engagement
    private readonly List<CardInstance> lockedCards = new List<CardInstance>();  // Cards locked for combo execution on ASSESS

    public PhysicalSessionDeck() { }

    // Read-only access to pile contents
    public IReadOnlyList<CardInstance> Hand => handPile.Cards;
    public IReadOnlyList<CardInstance> GoalCards => requestPile.Cards;
    public IReadOnlyList<CardInstance> PlayedCards => playedPile.Cards;
    public IReadOnlyList<CardInstance> LockedCards => lockedCards.AsReadOnly();
    public int RemainingDeckCards => deckPile.Count;
    public int HandSize => handPile.Count;

    /// <summary>
    /// Create deck from card instances
    /// </summary>
    public static PhysicalSessionDeck CreateFromInstances(List<CardInstance> deckCards, List<CardInstance> startingHand)
    {
        PhysicalSessionDeck deck = new PhysicalSessionDeck();

        // Add deck cards
        foreach (CardInstance card in deckCards)
        {
            deck.deckPile.Add(card);
        }

        // Shuffle deck
        deck.deckPile.Shuffle();

        // Add starting hand (knowledge cards)
        foreach (CardInstance card in startingHand)
        {
            deck.handPile.Add(card);
        }

        return deck;
    }

    /// <summary>
    /// Add goal card to request pile (unlocks at Breakthrough threshold)
    /// </summary>
    public void AddGoalCard(CardInstance card)
    {
        if (card != null)
            requestPile.Add(card);
    }

    /// <summary>
    /// Draw cards to hand
    /// </summary>
    public void DrawToHand(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (deckPile.Count == 0)
            {
                Console.WriteLine($"[PhysicalSessionDeck] No cards remaining in deck");
                break;
            }

            CardInstance card = deckPile.DrawTop();
            handPile.Add(card);
        }
    }

    /// <summary>
    /// Play card from hand
    /// </summary>
    public void PlayCard(CardInstance card)
    {
        if (card == null || !handPile.Contains(card))
        {
            Console.WriteLine($"[PhysicalSessionDeck] ERROR: Card not in hand");
            return;
        }

        handPile.Remove(card);
        playedPile.Add(card);
    }

    /// <summary>
    /// Check request pile and move goal cards to hand if Breakthrough threshold met
    /// </summary>
    public List<CardInstance> CheckGoalThresholds(int currentBreakthrough)
    {
        List<CardInstance> toMove = requestPile.Cards
            .Where(c => c.Context?.threshold <= currentBreakthrough)
            .ToList();

        List<CardInstance> movedCards = new List<CardInstance>();

        foreach (CardInstance card in toMove)
        {
            requestPile.Remove(card);
            card.IsPlayable = true;
            handPile.Add(card);
            movedCards.Add(card);
            Console.WriteLine($"[PhysicalSessionDeck] Goal card {card.PhysicalCardTemplate?.Id} unlocked (breakthrough {currentBreakthrough})");
        }

        return movedCards;
    }

    /// <summary>
    /// Exhaust all hand cards back to deck pile
    /// Used on ASSESS before drawing fresh Options
    /// </summary>
    public void ExhaustHandToDeck()
    {
        List<CardInstance> cardsToMove = handPile.Cards.ToList();

        foreach (CardInstance card in cardsToMove)
        {
            handPile.Remove(card);
            deckPile.Add(card);
        }

        Console.WriteLine($"[PhysicalSessionDeck] Exhausted {cardsToMove.Count} cards from hand back to deck");
    }

    /// <summary>
    /// Lock a card for combo execution
    /// Used on EXECUTE - card is removed from hand and locked for ASSESS combo trigger
    /// </summary>
    public void LockCard(CardInstance card)
    {
        if (card == null || !handPile.Contains(card))
        {
            Console.WriteLine($"[PhysicalSessionDeck] ERROR: Card not in hand, cannot lock");
            return;
        }

        handPile.Remove(card);
        lockedCards.Add(card);
        Console.WriteLine($"[PhysicalSessionDeck] Locked card {card.PhysicalCardTemplate?.Id} for combo execution");
    }

    /// <summary>
    /// Get currently locked cards
    /// </summary>
    public IReadOnlyList<CardInstance> GetLockedCards()
    {
        return lockedCards.AsReadOnly();
    }

    /// <summary>
    /// Clear locked cards after combo execution
    /// Used after ASSESS triggers all locked cards
    /// </summary>
    public void ClearLockedCards()
    {
        int count = lockedCards.Count;
        lockedCards.Clear();
        Console.WriteLine($"[PhysicalSessionDeck] Cleared {count} locked cards after combo execution");
    }

    /// <summary>
    /// Clear all piles
    /// </summary>
    public void Clear()
    {
        deckPile.Clear();
        handPile.Clear();
        playedPile.Clear();
        requestPile.Clear();
        lockedCards.Clear();
    }
}
