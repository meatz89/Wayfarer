using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Deck definition for Social tactical challenges
/// Contains card IDs that appear in this specific social challenge
/// Same card can appear in multiple challenge decks
/// </summary>
public class SocialChallengeDeck
{
    public string Id { get; set; }
    public string Name { get; set; }
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
            SocialCard entry = gameWorld.SocialCards
                .FirstOrDefault(d => d.Card?.Id == cardId);

            if (entry?.Card != null)
            {
                instances.Add(new CardInstance(entry.Card, "deck"));
            }
        }

        return instances;
    }
}
