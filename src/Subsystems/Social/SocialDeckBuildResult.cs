/// <summary>
/// Result of building a Social conversation deck
/// Replaces value tuple (SocialSessionCardDeck deck, List&lt;CardInstance&gt; SituationCards)
/// </summary>
public class SocialDeckBuildResult
{
public SocialSessionCardDeck Deck { get; init; }
public List<CardInstance> SituationCards { get; init; }

public SocialDeckBuildResult(SocialSessionCardDeck deck, List<CardInstance> situationCards)
{
    Deck = deck;
    SituationCards = situationCards;
}
}
