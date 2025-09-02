public class SessionCardDeck
{
    private readonly List<CardInstance> allCards = new();
    private readonly HashSet<string> drawnCardIds = new();
    private readonly HashSet<string> discardedCardIds = new();
    private readonly string npcId;

    public SessionCardDeck(string npcId)
    {
        this.npcId = npcId;
    }

    public static SessionCardDeck CreateFromTemplates(List<ConversationCard> templates, string npcId)
    {
        SessionCardDeck deck = new SessionCardDeck(npcId);
        foreach (ConversationCard template in templates)
        {
            deck.AddCard(new CardInstance(template));
        }
        return deck;
    }

    public void AddCard(CardInstance card)
    {
        allCards.Add(card);
    }

    public CardInstance DrawCard()
    {
        List<CardInstance> available = new List<CardInstance>();
        foreach (CardInstance c in allCards)
        {
            if (!drawnCardIds.Contains(c.InstanceId) && !discardedCardIds.Contains(c.InstanceId))
                available.Add(c);
        }
        if (!available.Any()) return null;

        CardInstance card = available[new Random().Next(available.Count)];
        drawnCardIds.Add(card.InstanceId);
        return card;
    }

    public List<CardInstance> DrawCards(int count)
    {
        List<CardInstance> drawn = new List<CardInstance>();
        for (int i = 0; i < count; i++)
        {
            CardInstance card = DrawCard();
            if (card != null) drawn.Add(card);
        }
        return drawn;
    }

    public void DiscardCard(string instanceId)
    {
        discardedCardIds.Add(instanceId);
    }

    public void Discard(CardInstance card)
    {
        discardedCardIds.Add(card.InstanceId);
    }

    public void ResetForNewConversation()
    {
        drawnCardIds.Clear();
        // Keep discarded cards discarded (single-use cards)
    }

    public List<CardInstance> GetAllCards()
    {
        return allCards.ToList();
    }

    public void Shuffle()
    {
        // Shuffle is handled by randomized drawing
    }

    public List<CardInstance> DrawFilteredByProperties(List<CardProperty> requiredProperties, int count)
    {
        List<CardInstance> available = new List<CardInstance>();
        foreach (CardInstance c in allCards)
        {
            if (!drawnCardIds.Contains(c.InstanceId) &&
                !discardedCardIds.Contains(c.InstanceId))
            {
                // Check if card has all required properties
                bool hasAllProperties = true;
                foreach (CardProperty prop in requiredProperties)
                {
                    if (!c.Properties.Contains(prop))
                    {
                        hasAllProperties = false;
                        break;
                    }
                }
                if (hasAllProperties)
                {
                    available.Add(c);
                }
            }
        }

        List<CardInstance> drawn = new List<CardInstance>();
        Random random = new Random();
        for (int i = 0; i < Math.Min(count, available.Count); i++)
        {
            CardInstance card = available[random.Next(available.Count)];
            drawnCardIds.Add(card.InstanceId);
            drawn.Add(card);
            available.Remove(card); // Prevent drawing same card twice
        }
        return drawn;
    }
    
    // Helper methods for common property-based filtering
    public List<CardInstance> DrawGoalCards(int count)
    {
        // Goal cards have both Fleeting AND Opportunity properties
        return DrawFilteredByProperties(new List<CardProperty> { CardProperty.Fleeting, CardProperty.Opportunity }, count);
    }
    
    public List<CardInstance> DrawBurdenCards(int count)
    {
        return DrawFilteredByProperties(new List<CardProperty> { CardProperty.Burden }, count);
    }
    
    public List<CardInstance> DrawObservableCards(int count)
    {
        return DrawFilteredByProperties(new List<CardProperty> { CardProperty.Observable }, count);
    }
    
    public List<CardInstance> DrawExchangeCards(int count)
    {
        return DrawFilteredByProperties(new List<CardProperty> { CardProperty.Exchange }, count);
    }

    public int RemainingCards
    {
        get
        {
            int count = 0;
            foreach (CardInstance c in allCards)
            {
                if (!drawnCardIds.Contains(c.InstanceId) && !discardedCardIds.Contains(c.InstanceId))
                    count++;
            }
            return count;
        }
    }

    public bool HasCardsAvailable()
    {
        return RemainingCards > 0;
    }

    public void Clear()
    {
        allCards.Clear();
        drawnCardIds.Clear();
        discardedCardIds.Clear();
    }
}
