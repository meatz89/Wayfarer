public class HandDeck
{
    public HashSet<CardInstance> Cards { get; } = new();

    public void AddCard(CardInstance card)
    {
        Cards.Add(card);
    }

    public void AddCards(IEnumerable<CardInstance> cards)
    {
        foreach (CardInstance card in cards)
        {
            Cards.Add(card);
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
