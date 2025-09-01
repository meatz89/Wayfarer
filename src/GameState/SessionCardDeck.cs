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
        var deck = new SessionCardDeck(npcId);
        foreach (var template in templates)
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
        var available = new List<CardInstance>();
        foreach (var c in allCards)
        {
            if (!drawnCardIds.Contains(c.InstanceId) && !discardedCardIds.Contains(c.InstanceId))
                available.Add(c);
        }
        if (!available.Any()) return null;
        
        var card = available[new Random().Next(available.Count)];
        drawnCardIds.Add(card.InstanceId);
        return card;
    }
    
    public List<CardInstance> DrawCards(int count)
    {
        var drawn = new List<CardInstance>();
        for (int i = 0; i < count; i++)
        {
            var card = DrawCard();
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
    
    public List<CardInstance> DrawFilteredByCategory(string category, int count)
    {
        var available = new List<CardInstance>();
        foreach (var c in allCards)
        {
            if (!drawnCardIds.Contains(c.InstanceId) && 
                !discardedCardIds.Contains(c.InstanceId) &&
                c.Category == category)
            {
                available.Add(c);
            }
        }
        
        var drawn = new List<CardInstance>();
        for (int i = 0; i < Math.Min(count, available.Count); i++)
        {
            var card = available[new Random().Next(available.Count)];
            drawnCardIds.Add(card.InstanceId);
            drawn.Add(card);
        }
        return drawn;
    }
    
    public int RemainingCards 
    { 
        get 
        {
            int count = 0;
            foreach (var c in allCards)
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
