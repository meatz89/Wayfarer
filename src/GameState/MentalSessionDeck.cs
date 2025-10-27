using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Mental session deck - runtime pile management for Mental engagements
/// Parallel to SessionCardDeck but for Mental tactical system
/// </summary>
public class MentalSessionDeck
{
    // Pile system: Deck → Hand → Played
    private readonly Pile deckPile = new();
    private readonly Pile handPile = new();
    private readonly Pile playedPile = new();
    private readonly Pile requestPile = new();  // SITUATION CARDS for this engagement

    public MentalSessionDeck() { }

    // Read-only access to pile contents
    public IReadOnlyList<CardInstance> Hand => handPile.Cards;
    public IReadOnlyList<CardInstance> SituationCards => requestPile.Cards;
    public IReadOnlyList<CardInstance> PlayedCards => playedPile.Cards;
    public int RemainingDeckCards => deckPile.Count;
    public int HandSize => handPile.Count;

    /// <summary>
    /// Create deck from card instances
    /// </summary>
    public static MentalSessionDeck CreateFromInstances(List<CardInstance> deckCards, List<CardInstance> startingHand)
    {
        MentalSessionDeck deck = new MentalSessionDeck();

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
    /// Add situation card to request pile (unlocks at Progress threshold)
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
    /// Play card from hand
    /// </summary>
    public void PlayCard(CardInstance card)
    {
        if (card == null || !handPile.Contains(card))
        {
            return;
        }

        handPile.Remove(card);
        playedPile.Add(card);
    }

    /// <summary>
    /// Check request pile and move situation cards to hand if Progress threshold met
    /// </summary>
    public List<CardInstance> CheckSituationThresholds(int currentProgress)
    {
        List<CardInstance> toMove = requestPile.Cards
            .Where(c => c.Context != null && c.Context.threshold <= currentProgress)
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
    /// Clear all piles
    /// </summary>
    public void Clear()
    {
        deckPile.Clear();
        handPile.Clear();
        playedPile.Clear();
        requestPile.Clear();
    }
}
