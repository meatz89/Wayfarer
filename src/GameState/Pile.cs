using System.Collections.Generic;
using System.Linq;

public class Pile
{
    private readonly List<CardInstance> cards = new();
    private readonly Random random = new Random();

    public IReadOnlyList<CardInstance> Cards => cards;
    public int Count => cards.Count;

    public void Add(CardInstance card)
    {
        if (card == null) return;

        // Ensure card has a pre-rolled value
        if (card.Context == null)
            card.Context = new CardContext();

        if (card.Context.PreRolledValue == null)
        {
            card.Context.PreRolledValue = random.Next(1, 101);
            Console.WriteLine($"[Pile] Pre-rolled {card.Context.PreRolledValue} for card: {card.Id}");
        }

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
        // Fisher-Yates shuffle
        int n = cards.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            CardInstance temp = cards[k];
            cards[k] = cards[n];
            cards[n] = temp;
        }
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