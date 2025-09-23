using System.Collections.Generic;
using System.Linq;

public class Pile
{
    private readonly List<CardInstance> cards = new();
    // DETERMINISTIC SYSTEM: No random number generation needed

    public IReadOnlyList<CardInstance> Cards => cards;
    public int Count => cards.Count;

    public void Add(CardInstance card)
    {
        if (card == null) return;

        // DETERMINISTIC SYSTEM: No pre-rolled values needed
        if (card.Context == null)
            card.Context = new CardContext();

        cards.Add(card);
    }

    public void AddRange(IEnumerable<CardInstance> cardsToAdd)
    {
        foreach (CardInstance card in cardsToAdd)
        {
            Add(card);
        }
    }

    public CardInstance DrawTop()
    {
        if (cards.Count == 0) return null;

        CardInstance card = cards[0];
        cards.RemoveAt(0);
        return card;
    }

    public List<CardInstance> DrawMultiple(int count)
    {
        List<CardInstance> drawn = new List<CardInstance>();
        for (int i = 0; i < count && cards.Count > 0; i++)
        {
            drawn.Add(DrawTop());
        }
        return drawn;
    }

    public List<CardInstance> DrawAll()
    {
        List<CardInstance> all = new List<CardInstance>(cards);
        cards.Clear();
        return all;
    }

    public void Remove(CardInstance card)
    {
        cards.Remove(card);
    }

    public void Clear()
    {
        cards.Clear();
    }

    public void Shuffle()
    {
        // DETERMINISTIC SYSTEM: No shuffling - cards maintain their original order
        // This ensures consistent, predictable card draw order
    }

    public bool Contains(CardInstance card)
    {
        return cards.Contains(card);
    }

    public bool Any()
    {
        return cards.Any();
    }
}