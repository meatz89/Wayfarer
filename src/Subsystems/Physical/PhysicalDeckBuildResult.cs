/// <summary>
/// Result of building a Physical challenge deck
/// Replaces value tuple (List&lt;CardInstance&gt; deck, List&lt;CardInstance&gt; startingHand)
/// </summary>
public class PhysicalDeckBuildResult
{
    public List<CardInstance> Deck { get; init; }
    public List<CardInstance> StartingHand { get; init; }

    public PhysicalDeckBuildResult(List<CardInstance> deck, List<CardInstance> startingHand)
    {
        Deck = deck;
        StartingHand = startingHand;
    }
}
