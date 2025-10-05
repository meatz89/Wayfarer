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
    protected List<ConversationCard> cards = new();
    protected HashSet<string> drawnCardIds = new();

    public int RemainingCards
    {
        get
        {
            int count = 0;
            foreach (ConversationCard c in cards)
            {
                if (!drawnCardIds.Contains(c.Id)) count++;
            }
            return count;
        }
    }
    public int Count => cards.Count;

    public void AddCard(ConversationCard card)
    {
        cards.Add(card);
    }

    public void AddCards(IEnumerable<ConversationCard> newCards)
    {
        cards.AddRange(newCards);
    }

    public ConversationCard DrawCard()
    {
        List<ConversationCard> available = new List<ConversationCard>();
        foreach (ConversationCard c in cards)
        {
            if (!drawnCardIds.Contains(c.Id)) available.Add(c);
        }
        if (!available.Any()) return null;

        ConversationCard card = available[0];
        drawnCardIds.Add(card.Id);
        return card;
    }

    public List<ConversationCard> DrawCards(int count)
    {
        List<ConversationCard> drawn = new List<ConversationCard>();
        for (int i = 0; i < count; i++)
        {
            ConversationCard card = DrawCard();
            if (card != null) drawn.Add(card);
        }
        return drawn;
    }

    public void Reset()
    {
        drawnCardIds.Clear();
    }

    public List<ConversationCard> GetAllCards()
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

    public void RemoveCard(ConversationCard card)
    {
        cards.Remove(card);
        drawnCardIds.Remove(card.Id);
    }

    public static void InitializeGameWorld(GameWorld gameWorld)
    {
        // Card decks are initialized from JSON
    }
}
