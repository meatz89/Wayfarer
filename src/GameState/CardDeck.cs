using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Card deck for conversation system
/// Manages collection of conversation cards with draw mechanics
/// SEPARATE from EngagementTypeDeck (which is for tactical systems)
/// </summary>
public class CardDeck
{
    protected List<SocialCard> cards = new();
    protected HashSet<string> drawnCardIds = new();

    public int RemainingCards
    {
        get
        {
            int count = 0;
            foreach (SocialCard c in cards)
            {
                if (!drawnCardIds.Contains(c.Id)) count++;
            }
            return count;
        }
    }
    public int Count => cards.Count;

    public void AddCard(SocialCard card)
    {
        cards.Add(card);
    }

    public void AddCards(IEnumerable<SocialCard> newCards)
    {
        cards.AddRange(newCards);
    }

    public SocialCard DrawCard()
    {
        List<SocialCard> available = new List<SocialCard>();
        foreach (SocialCard c in cards)
        {
            if (!drawnCardIds.Contains(c.Id)) available.Add(c);
        }
        if (!available.Any()) return null;

        SocialCard card = available[0];
        drawnCardIds.Add(card.Id);
        return card;
    }

    public List<SocialCard> DrawCards(int count)
    {
        List<SocialCard> drawn = new List<SocialCard>();
        for (int i = 0; i < count; i++)
        {
            SocialCard card = DrawCard();
            if (card != null) drawn.Add(card);
        }
        return drawn;
    }

    public void Reset()
    {
        drawnCardIds.Clear();
    }

    public List<SocialCard> GetAllCards()
    {
        return cards.ToList();
    }

    public bool HasCards()
    {
        return RemainingCards > 0;
    }

    public bool Any()
    {
        return cards.Any();
    }

    public bool HasCardsAvailable()
    {
        return HasCards();
    }

    public void RemoveCard(SocialCard card)
    {
        cards.Remove(card);
        drawnCardIds.Remove(card.Id);
    }

    public static void InitializeGameWorld(GameWorld gameWorld)
    {
        // Card decks are initialized from JSON
    }
}
