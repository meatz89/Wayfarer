public class Pile
{
private readonly List<CardInstance> cards = new();
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
    // Fisher-Yates shuffle for proper randomization
    // Card EFFECTS are deterministic (no dice rolls), but draw ORDER is randomized for variety
    Random rng = new Random();
    int n = cards.Count;
    while (n > 1)
    {
        n--;
        int k = rng.Next(n + 1);
        CardInstance value = cards[k];
        cards[k] = cards[n];
        cards[n] = value;
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