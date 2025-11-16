/// <summary>
/// Deck definition for Social tactical challenges
/// Contains card IDs that appear in this specific social challenge
/// Same card can appear in multiple challenge decks
/// </summary>
public class SocialChallengeDeck
{
    // HIGHLANDER: NO Id property - SocialChallengeDeck identified by object reference
    public string Name { get; set; }
    public string Description { get; set; }
    public int DangerThreshold { get; set; }
    public int InitialHandSize { get; set; }
    public int MaxHandSize { get; set; }
    public List<string> CardIds { get; set; } = new List<string>();

    /// <summary>
    /// Build card instances from this deck's card IDs
    /// Returns all conversation cards matching this deck's IDs
    /// </summary>
    public List<CardInstance> BuildCardInstances(GameWorld gameWorld)
    {
        List<CardInstance> instances = new List<CardInstance>();

        foreach (string cardId in CardIds)
        {
            SocialCard card = gameWorld.SocialCards.FirstOrDefault(d => d != null && d.Id == cardId);

            if (card == null)
                throw new System.InvalidOperationException($"Social card not found: {cardId}. Ensure card is loaded in GameWorld.SocialCards");
            instances.Add(new CardInstance(card));
        }

        return instances;
    }
}
