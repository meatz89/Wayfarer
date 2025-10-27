using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Physical session deck - runtime pile management for Physical engagements
/// Parallel to SessionCardDeck and MentalSessionDeck but for Physical tactical system
/// </summary>
public class PhysicalSessionDeck
{
    // THREE-PILE SYSTEM: Deck (draw) → Hand (active) → Locked (exhaust)
    private readonly Pile deckPile = new();
    private readonly Pile handPile = new();
    private readonly Pile requestPile = new();  // SITUATION CARDS for this engagement
    private readonly List<CardInstance> lockedCards = new List<CardInstance>();  // EXHAUST PILE - locked for combo execution on ASSESS

    public PhysicalSessionDeck() { }

    // Read-only access to pile contents
    public IReadOnlyList<CardInstance> Hand => handPile.Cards;
    public IReadOnlyList<CardInstance> SituationCards => requestPile.Cards;
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
    /// Add situation card to request pile (unlocks at Breakthrough threshold)
    /// </summary>
    public void AddSituationCard(CardInstance card)
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
                break;
            }

            CardInstance card = deckPile.DrawTop();
            handPile.Add(card);
        }
    }

    /// <summary>
    /// Check request pile and move situation cards to hand if Breakthrough threshold met
    /// </summary>
    public List<CardInstance> CheckSituationThresholds(int currentBreakthrough)
    {
        List<CardInstance> toMove = requestPile.Cards
            .Where(c => c.Context != null && c.Context.threshold <= currentBreakthrough)
            .ToList();

        List<CardInstance> movedCards = new List<CardInstance>();

        foreach (CardInstance card in toMove)
        {
            requestPile.Remove(card);
            card.IsPlayable = true;
            handPile.Add(card);
            movedCards.Add(card);
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
    }

    /// <summary>
    /// Lock a card for combo execution
    /// Used on EXECUTE - card is removed from hand and locked for ASSESS combo trigger
    /// </summary>
    public void LockCard(CardInstance card)
    {
        if (card == null || !handPile.Contains(card))
        {
            return;
        }

        handPile.Remove(card);
        lockedCards.Add(card);
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
    }

    /// <summary>
    /// Shuffle exhaust pile (locked cards) and hand back to deck
    /// Used on ASSESS to reset card flow: exhaust + hand → deck → shuffle → draw fresh
    /// CORE PHYSICAL MECHANIC: All cards cycle back through Situation deck
    /// </summary>
    public void ShuffleExhaustAndHandBackToDeck()
    {
        int exhaustCount = lockedCards.Count;
        int handCount = handPile.Count;

        // Move all locked cards (exhaust pile) to deck
        foreach (CardInstance card in lockedCards.ToList())
        {
            deckPile.Add(card);
        }
        lockedCards.Clear();

        // Move all hand cards to deck
        foreach (CardInstance card in handPile.Cards.ToList())
        {
            handPile.Remove(card);
            deckPile.Add(card);
        }

        // Shuffle deck
        deckPile.Shuffle();
    }

    /// <summary>
    /// Clear all piles
    /// </summary>
    public void Clear()
    {
        deckPile.Clear();
        handPile.Clear();
        requestPile.Clear();
        lockedCards.Clear();
    }
}
