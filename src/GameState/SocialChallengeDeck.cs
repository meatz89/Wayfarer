using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Deck definition for Social tactical engagements
/// Contains card IDs that appear in this specific conversation engagement
/// Same card can appear in multiple engagement decks
/// </summary>
public class ConversationEngagementDeck
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
            CardDefinitionEntry entry = gameWorld.AllCardDefinitions
                .FirstOrDefault(d => d.Card?.Id == cardId);

            if (entry?.Card != null)
            {
                instances.Add(new CardInstance(entry.Card, "deck"));
            }
        }

        return instances;
    }
}
