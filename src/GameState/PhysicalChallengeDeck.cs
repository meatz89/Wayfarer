using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Deck definition for Physical tactical challenges
/// Contains card IDs that appear in this specific physical challenge
/// Same card can appear in multiple challenge decks
/// </summary>
public class PhysicalChallengeDeck
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<string> CardIds { get; set; } = new List<string>();

    /// <summary>
    /// Build card instances from this deck's card IDs
    /// Returns all physical cards matching this deck's IDs
    /// </summary>
    public List<CardInstance> BuildCardInstances(GameWorld gameWorld)
    {
        List<CardInstance> instances = new List<CardInstance>();

        foreach (string cardId in CardIds)
        {
            PhysicalCardEntry entry = gameWorld.PhysicalCards.FirstOrDefault(e => e.CardId == cardId);
            if (entry?.Card != null)
            {
                instances.Add(new CardInstance(entry.Card, "deck"));
            }
        }

        return instances;
    }
}
