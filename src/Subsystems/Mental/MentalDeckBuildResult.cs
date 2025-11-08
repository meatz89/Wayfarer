/// <summary>
/// Result of building a Mental challenge deck
/// Replaces value tuple (List&lt;CardInstance&gt; deck, List&lt;CardInstance&gt; startingHand)
/// </summary>
public class MentalDeckBuildResult
{
    public List<CardInstance> Deck { get; init; }
    public List<CardInstance> StartingHand { get; init; }

    public MentalDeckBuildResult(List<CardInstance> deck, List<CardInstance> startingHand)
    {
        Deck = deck;
        StartingHand = startingHand;
    }
}
