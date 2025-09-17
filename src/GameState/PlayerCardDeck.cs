using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Player's persistent conversation deck that maintains CardInstance objects with their XP
/// </summary>
public class PlayerCardDeck
{
    private List<CardInstance> cardInstances = new();

    public int Count => cardInstances.Count;

    /// <summary>
    /// Add a new card instance to the player's deck
    /// </summary>
    public void AddCardInstance(CardInstance instance)
    {
        cardInstances.Add(instance);
    }

    /// <summary>
    /// Add multiple card instances
    /// </summary>
    public void AddCardInstances(IEnumerable<CardInstance> instances)
    {
        cardInstances.AddRange(instances);
    }

    /// <summary>
    /// Create a card instance from a template and add it to the deck
    /// </summary>
    public void AddCardFromTemplate(ConversationCard template)
    {
        CardInstance instance = new CardInstance(template, "player_deck");
        cardInstances.Add(instance);
    }

    /// <summary>
    /// Get all card instances for use in a conversation
    /// </summary>
    public List<CardInstance> GetAllInstances()
    {
        return cardInstances.ToList();
    }

    /// <summary>
    /// Find a specific instance by its InstanceId
    /// </summary>
    public CardInstance FindInstanceById(string instanceId)
    {
        return cardInstances.FirstOrDefault(c => c.InstanceId == instanceId);
    }


    /// <summary>
    /// Clear all cards (used for reinitialization)
    /// </summary>
    public void Clear()
    {
        cardInstances.Clear();
    }

    /// <summary>
    /// Check if deck has any cards
    /// </summary>
    public bool Any()
    {
        return cardInstances.Any();
    }
}