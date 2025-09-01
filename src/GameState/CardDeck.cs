
// Card decks
public class CardDeck
{
    protected List<ConversationCard> cards = new();
    protected HashSet<string> drawnCardIds = new();
    
    public int RemainingCards 
    { 
        get 
        {
            int count = 0;
            foreach (var c in cards)
            {
                if (!drawnCardIds.Contains(c.Id)) count++;
            }
            return count;
        }
    }
    public int Count { get { return cards.Count; } }
    
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
        var available = new List<ConversationCard>();
        foreach (var c in cards)
        {
            if (!drawnCardIds.Contains(c.Id)) available.Add(c);
        }
        if (!available.Any()) return null;
        
        var card = available[new Random().Next(available.Count)];
        drawnCardIds.Add(card.Id);
        return card;
    }
    
    public List<ConversationCard> DrawCards(int count)
    {
        var drawn = new List<ConversationCard>();
        for (int i = 0; i < count; i++)
        {
            var card = DrawCard();
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
    
    public static void InitializeGameWorld(GameWorld gameWorld)
    {
        // Card decks are initialized from JSON
    }
}
