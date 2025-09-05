public class HandDeck
{
    private readonly Random random = new Random();
    public HashSet<CardInstance> Cards { get; } = new();

    public void AddCard(CardInstance card)
    {
        // Ensure card has a pre-rolled value (in case it wasn't added through SessionCardDeck)
        if (card != null)
        {
            if (card.Context == null)
                card.Context = new CardContext();
            
            if (card.Context.PreRolledValue == null)
            {
                card.Context.PreRolledValue = random.Next(1, 101);
                Console.WriteLine($"[HandDeck] Pre-rolled {card.Context.PreRolledValue} for card: {card.Id}");
            }
        }
        
        Cards.Add(card);
    }

    public void AddCards(IEnumerable<CardInstance> cards)
    {
        foreach (CardInstance card in cards)
        {
            AddCard(card); // Use AddCard to ensure pre-rolls
        }
    }

    public void RemoveCard(CardInstance card)
    {
        Cards.Remove(card);
    }

    public void Clear()
    {
        Cards.Clear();
    }

    public bool HasCards()
    {
        return Cards.Any();
    }

    public void RemoveCards(IEnumerable<CardInstance> cardsToRemove)
    {
        foreach (CardInstance card in cardsToRemove)
        {
            Cards.Remove(card);
        }
    }
}
