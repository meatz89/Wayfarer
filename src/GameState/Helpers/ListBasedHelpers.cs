using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.GameState.Enums;

/// <summary>
/// Helper classes to replace Dictionary and HashSet with List-based implementations
/// following deterministic principles and avoiding non-deterministic data structures
/// </summary>

/// <summary>
/// Helper class for resource entries (replaces Dictionary<string, int>)
/// </summary>
public class ResourceEntry
{
    public string ResourceType { get; set; }
    public int Amount { get; set; }
}

/// <summary>
/// Helper class for NPC token entries (replaces nested Dictionary)
/// </summary>
public class NPCTokenEntry
{
    public string NpcId { get; set; }
    public int Trust { get; set; }
    public int Commerce { get; set; }
    public int Status { get; set; }
    public int Shadow { get; set; }

    public int GetTokenCount(ConnectionType type)
    {
        return type switch
        {
            ConnectionType.Trust => Trust,
            ConnectionType.Commerce => Commerce,
            ConnectionType.Status => Status,
            ConnectionType.Shadow => Shadow,
            _ => 0
        };
    }

    public void SetTokenCount(ConnectionType type, int value)
    {
        switch (type)
        {
            case ConnectionType.Trust:
                Trust = value;
                break;
            case ConnectionType.Commerce:
                Commerce = value;
                break;
            case ConnectionType.Status:
                Status = value;
                break;
            case ConnectionType.Shadow:
                Shadow = value;
                break;
        }
    }
}

/// <summary>
/// Helper class for route entries (replaces Dictionary<string, List<RouteOption>>)
/// </summary>
public class KnownRouteEntry
{
    public string OriginSpotId { get; set; }
    public List<RouteOption> Routes { get; set; } = new List<RouteOption>();
}

/// <summary>
/// Helper class for letter history entries (replaces Dictionary<string, LetterHistory>)
/// </summary>
public class LetterHistoryEntry
{
    public string NpcId { get; set; }
    public LetterHistory History { get; set; }
}

/// <summary>
/// Helper class for familiarity entries (replaces Dictionary<string, int>)
/// </summary>
public class FamiliarityEntry
{
    public string EntityId { get; set; }
    public int Level { get; set; }
}

/// <summary>
/// Helper class for card deck entries (replaces Dictionary<string, int>)
/// </summary>
public class CardDeckEntry
{
    public string CardId { get; set; }
    public int Count { get; set; }
}

/// <summary>
/// Helper class for NPC exchange card entries (replaces Dictionary<string, List<ExchangeCard>>)
/// </summary>
public class NPCExchangeCardEntry
{
    public string NpcId { get; set; }
    public List<ExchangeCard> ExchangeCards { get; set; } = new List<ExchangeCard>();
}

/// <summary>
/// Helper class for conversation type definition entries (replaces Dictionary<string, ConversationTypeDefinition>)
/// </summary>
public class ConversationTypeEntry
{
    public string TypeId { get; set; }
    public ConversationTypeDefinition Definition { get; set; }
}

/// <summary>
/// Helper class for card deck definition entries (replaces Dictionary<string, CardDeckDefinition>)
/// </summary>
public class CardDeckDefinitionEntry
{
    public string DeckId { get; set; }
    public CardDeckDefinition Definition { get; set; }
}

/// <summary>
/// Helper class for card definition entries (replaces Dictionary<string, ConversationCard>)
/// </summary>
public class CardDefinitionEntry
{
    public string CardId { get; set; }
    public ConversationCard Card { get; set; }
}

/// <summary>
/// Helper class for location spot entries (replaces Dictionary<string, LocationSpot>)
/// </summary>
public class LocationSpotEntry
{
    public string SpotId { get; set; }
    public LocationSpot Spot { get; set; }
}

/// <summary>
/// Helper class for skeleton registry entries (replaces Dictionary<string, string>)
/// </summary>
public class SkeletonRegistryEntry
{
    public string SkeletonKey { get; set; }
    public string ContentType { get; set; }
}

/// <summary>
/// Helper class for path card discovery entries (replaces Dictionary<string, bool>)
/// </summary>
public class PathCardDiscoveryEntry
{
    public string CardId { get; set; }
    public bool IsDiscovered { get; set; }
}

/// <summary>
/// Helper class for event deck position entries (replaces Dictionary<string, int>)
/// </summary>
public class EventDeckPositionEntry
{
    public string DeckId { get; set; }
    public int Position { get; set; }
}

/// <summary>
/// Helper class for path collection entries (replaces Dictionary<string, PathCardCollectionDTO>)
/// </summary>
public class PathCollectionEntry
{
    public string CollectionId { get; set; }
    public PathCardCollectionDTO Collection { get; set; }
}

/// <summary>
/// Helper class for travel event entries (replaces Dictionary<string, TravelEventDTO>)
/// </summary>
public class TravelEventEntry
{
    public string EventId { get; set; }
    public TravelEventDTO TravelEvent { get; set; }
}

/// <summary>
/// Helper class for personality mapping entries (replaces Dictionary<PersonalityType, PersonalityCardMapping>)
/// </summary>
public class PersonalityMappingEntry
{
    public PersonalityType PersonalityType { get; set; }
    public PersonalityCardMapping Mapping { get; set; }
}

/// <summary>
/// Helper class for token unlock entries (replaces Dictionary<int, List<string>>)
/// </summary>
public class TokenUnlockEntry
{
    public int TokenCount { get; set; }
    public List<string> UnlockedIds { get; set; } = new List<string>();
}

/// <summary>
/// Extension methods to make List-based lookups as easy as Dictionary lookups
/// </summary>
public static class ListBasedHelperExtensions
{
    // Resource entry helpers
    public static int GetResourceAmount(this List<ResourceEntry> resources, string resourceType)
    {
        return resources.FirstOrDefault(r => r.ResourceType == resourceType)?.Amount ?? 0;
    }

    public static void SetResourceAmount(this List<ResourceEntry> resources, string resourceType, int amount)
    {
        ResourceEntry? existing = resources.FirstOrDefault(r => r.ResourceType == resourceType);
        if (existing != null)
        {
            existing.Amount = amount;
        }
        else
        {
            resources.Add(new ResourceEntry { ResourceType = resourceType, Amount = amount });
        }
    }

    // NPC token helpers
    public static NPCTokenEntry GetNPCTokenEntry(this List<NPCTokenEntry> tokens, string npcId)
    {
        NPCTokenEntry? entry = tokens.FirstOrDefault(t => t.NpcId == npcId);
        if (entry == null)
        {
            entry = new NPCTokenEntry { NpcId = npcId };
            tokens.Add(entry);
        }
        return entry;
    }

    public static int GetTokenCount(this List<NPCTokenEntry> tokens, string npcId, ConnectionType type)
    {
        return tokens.FirstOrDefault(t => t.NpcId == npcId)?.GetTokenCount(type) ?? 0;
    }

    public static void SetTokenCount(this List<NPCTokenEntry> tokens, string npcId, ConnectionType type, int count)
    {
        NPCTokenEntry entry = tokens.GetNPCTokenEntry(npcId);
        entry.SetTokenCount(type, count);
    }

    public static bool ContainsKey(this List<NPCTokenEntry> tokens, string npcId)
    {
        return tokens.Any(t => t.NpcId == npcId);
    }

    public static Dictionary<ConnectionType, int> GetItem(this List<NPCTokenEntry> tokens, string npcId)
    {
        NPCTokenEntry entry = tokens.FirstOrDefault(t => t.NpcId == npcId);
        if (entry == null) return new Dictionary<ConnectionType, int>();

        return new Dictionary<ConnectionType, int>
        {
            [ConnectionType.Trust] = entry.Trust,
            [ConnectionType.Commerce] = entry.Commerce,
            [ConnectionType.Status] = entry.Status,
            [ConnectionType.Shadow] = entry.Shadow
        };
    }

    public static void SetItem(this List<NPCTokenEntry> tokens, string npcId, Dictionary<ConnectionType, int> tokenCounts)
    {
        NPCTokenEntry entry = tokens.GetNPCTokenEntry(npcId);
        if (tokenCounts.ContainsKey(ConnectionType.Trust))
            entry.Trust = tokenCounts[ConnectionType.Trust];
        if (tokenCounts.ContainsKey(ConnectionType.Commerce))
            entry.Commerce = tokenCounts[ConnectionType.Commerce];
        if (tokenCounts.ContainsKey(ConnectionType.Status))
            entry.Status = tokenCounts[ConnectionType.Status];
        if (tokenCounts.ContainsKey(ConnectionType.Shadow))
            entry.Shadow = tokenCounts[ConnectionType.Shadow];
    }

    // Card deck helpers
    public static int GetCardCount(this List<CardDeckEntry> deck, string cardId)
    {
        return deck.FirstOrDefault(c => c.CardId == cardId)?.Count ?? 0;
    }

    public static void SetCardCount(this List<CardDeckEntry> deck, string cardId, int count)
    {
        CardDeckEntry? existing = deck.FirstOrDefault(c => c.CardId == cardId);
        if (existing != null)
        {
            existing.Count = count;
        }
        else
        {
            deck.Add(new CardDeckEntry { CardId = cardId, Count = count });
        }
    }

    // Familiarity helpers
    public static int GetFamiliarity(this List<FamiliarityEntry> familiarityList, string entityId)
    {
        return familiarityList.FirstOrDefault(f => f.EntityId == entityId)?.Level ?? 0;
    }

    public static void SetFamiliarity(this List<FamiliarityEntry> familiarityList, string entityId, int level)
    {
        FamiliarityEntry? existing = familiarityList.FirstOrDefault(f => f.EntityId == entityId);
        if (existing != null)
        {
            existing.Level = level;
        }
        else
        {
            familiarityList.Add(new FamiliarityEntry { EntityId = entityId, Level = level });
        }
    }

    // Generic ID-based lookups
    public static T FindById<T>(this List<T> list, string id) where T : class
    {
        return list.FirstOrDefault(item =>
        {
            System.Reflection.PropertyInfo? prop = typeof(T).GetProperty("Id") ??
                      typeof(T).GetProperty("CardId") ??
                      typeof(T).GetProperty("SpotId") ??
                      typeof(T).GetProperty("NpcId") ??
                      typeof(T).GetProperty("TypeId") ??
                      typeof(T).GetProperty("DeckId") ??
                      typeof(T).GetProperty("EventId") ??
                      typeof(T).GetProperty("CollectionId");
            return prop?.GetValue(item)?.ToString() == id;
        });
    }

    // LocationSpot helpers
    public static IEnumerable<LocationSpot> Values(this List<LocationSpotEntry> spots)
    {
        return spots.Select(s => s.Spot);
    }

    public static bool TryGetValue(this List<LocationSpotEntry> spots, string spotId, out LocationSpot spot)
    {
        LocationSpotEntry? entry = spots.FindById(spotId);
        spot = entry?.Spot;
        return entry != null;
    }

    public static bool ContainsKey(this List<LocationSpotEntry> spots, string spotId)
    {
        return spots.Any(s => s.SpotId == spotId);
    }

    // PathCollection helpers
    public static IEnumerable<PathCardCollectionDTO> Values(this List<PathCollectionEntry> collections)
    {
        return collections.Select(c => c.Collection);
    }

    public static bool ContainsKey(this List<PathCollectionEntry> collections, string collectionId)
    {
        return collections.Any(c => c.CollectionId == collectionId);
    }

    public static PathCardCollectionDTO GetValue(this List<PathCollectionEntry> collections, string collectionId)
    {
        return collections.FindById(collectionId)?.Collection;
    }

    // NPC Exchange Card helpers
    public static bool TryGetValue(this List<NPCExchangeCardEntry> exchanges, string npcId, out List<ExchangeCard> cards)
    {
        NPCExchangeCardEntry? entry = exchanges.FindById(npcId);
        cards = entry?.ExchangeCards;
        return entry != null;
    }

    // Conversation Type helpers
    public static bool TryGetValue(this List<ConversationTypeEntry> types, string typeId, out ConversationTypeDefinition definition)
    {
        ConversationTypeEntry? entry = types.FindById(typeId);
        definition = entry?.Definition;
        return entry != null;
    }

    // Card Definition helpers
    public static bool TryGetValue(this List<CardDefinitionEntry> cards, string cardId, out ConversationCard card)
    {
        CardDefinitionEntry? entry = cards.FindById(cardId);
        card = entry?.Card;
        return entry != null;
    }

    public static bool ContainsKey(this List<CardDefinitionEntry> cards, string cardId)
    {
        return cards.Any(c => c.CardId == cardId);
    }

    // Skeleton Registry helpers
    public static bool ContainsKey(this List<SkeletonRegistryEntry> registry, string key)
    {
        return registry.Any(r => r.SkeletonKey == key);
    }

    public static void Add(this List<SkeletonRegistryEntry> registry, string key, string contentType)
    {
        if (!registry.Any(r => r.SkeletonKey == key))
        {
            registry.Add(new SkeletonRegistryEntry { SkeletonKey = key, ContentType = contentType });
        }
    }

    public static void Remove(this List<SkeletonRegistryEntry> registry, string key)
    {
        SkeletonRegistryEntry entry = registry.FirstOrDefault(r => r.SkeletonKey == key);
        if (entry != null)
        {
            registry.Remove(entry);
        }
    }

    // Event deck position helpers
    public static bool ContainsKey(this List<EventDeckPositionEntry> positions, string deckId)
    {
        return positions.Any(p => p.DeckId == deckId);
    }

    public static int GetValue(this List<EventDeckPositionEntry> positions, string deckId)
    {
        return positions.FindById(deckId)?.Position ?? 0;
    }

    public static void SetValue(this List<EventDeckPositionEntry> positions, string deckId, int position)
    {
        EventDeckPositionEntry existing = positions.FindById(deckId);
        if (existing != null)
        {
            existing.Position = position;
        }
        else
        {
            positions.Add(new EventDeckPositionEntry { DeckId = deckId, Position = position });
        }
    }

    // Boolean discovery helpers
    public static bool IsDiscovered(this List<PathCardDiscoveryEntry> discoveries, string cardId)
    {
        return discoveries.FirstOrDefault(d => d.CardId == cardId)?.IsDiscovered ?? false;
    }

    public static void SetDiscovered(this List<PathCardDiscoveryEntry> discoveries, string cardId, bool discovered)
    {
        PathCardDiscoveryEntry? existing = discoveries.FirstOrDefault(d => d.CardId == cardId);
        if (existing != null)
        {
            existing.IsDiscovered = discovered;
        }
        else
        {
            discoveries.Add(new PathCardDiscoveryEntry { CardId = cardId, IsDiscovered = discovered });
        }
    }

    // Dictionary-style indexer for LocationSpotEntry
    public static LocationSpot GetItem(this List<LocationSpotEntry> spots, string spotId)
    {
        return spots.FindById(spotId)?.Spot;
    }

    public static void SetItem(this List<LocationSpotEntry> spots, string spotId, LocationSpot spot)
    {
        LocationSpotEntry existing = spots.FindById(spotId);
        if (existing != null)
        {
            existing.Spot = spot;
        }
        else
        {
            spots.Add(new LocationSpotEntry { SpotId = spotId, Spot = spot });
        }
    }

    public static void Remove(this List<LocationSpotEntry> spots, string spotId)
    {
        LocationSpotEntry entry = spots.FindById(spotId);
        if (entry != null)
        {
            spots.Remove(entry);
        }
    }

    // Dictionary-style indexer for PathCollectionEntry
    public static PathCardCollectionDTO GetItem(this List<PathCollectionEntry> collections, string collectionId)
    {
        return collections.FindById(collectionId)?.Collection;
    }

    public static void SetItem(this List<PathCollectionEntry> collections, string collectionId, PathCardCollectionDTO collection)
    {
        PathCollectionEntry existing = collections.FindById(collectionId);
        if (existing != null)
        {
            existing.Collection = collection;
        }
        else
        {
            collections.Add(new PathCollectionEntry { CollectionId = collectionId, Collection = collection });
        }
    }

    // Dictionary-style indexer for TravelEventEntry
    public static TravelEventDTO GetItem(this List<TravelEventEntry> events, string eventId)
    {
        return events.FindById(eventId)?.TravelEvent;
    }

    public static void SetItem(this List<TravelEventEntry> events, string eventId, TravelEventDTO travelEvent)
    {
        TravelEventEntry existing = events.FindById(eventId);
        if (existing != null)
        {
            existing.TravelEvent = travelEvent;
        }
        else
        {
            events.Add(new TravelEventEntry { EventId = eventId, TravelEvent = travelEvent });
        }
    }

    // Dictionary-style indexer for CardDefinitionEntry
    public static ConversationCard GetItem(this List<CardDefinitionEntry> cards, string cardId)
    {
        return cards.FindById(cardId)?.Card;
    }

    public static void SetItem(this List<CardDefinitionEntry> cards, string cardId, ConversationCard card)
    {
        CardDefinitionEntry existing = cards.FindById(cardId);
        if (existing != null)
        {
            existing.Card = card;
        }
        else
        {
            cards.Add(new CardDefinitionEntry { CardId = cardId, Card = card });
        }
    }

    // Dictionary-style indexer for ConversationTypeEntry
    public static ConversationTypeDefinition GetItem(this List<ConversationTypeEntry> types, string typeId)
    {
        return types.FindById(typeId)?.Definition;
    }

    public static void SetItem(this List<ConversationTypeEntry> types, string typeId, ConversationTypeDefinition definition)
    {
        ConversationTypeEntry existing = types.FindById(typeId);
        if (existing != null)
        {
            existing.Definition = definition;
        }
        else
        {
            types.Add(new ConversationTypeEntry { TypeId = typeId, Definition = definition });
        }
    }

    // PathCardDiscoveryEntry helpers
    public static bool ContainsKey(this List<PathCardDiscoveryEntry> discoveries, string cardId)
    {
        return discoveries.Any(d => d.CardId == cardId);
    }

    public static bool GetItem(this List<PathCardDiscoveryEntry> discoveries, string cardId)
    {
        return discoveries.FirstOrDefault(d => d.CardId == cardId)?.IsDiscovered ?? false;
    }

    public static void SetItem(this List<PathCardDiscoveryEntry> discoveries, string cardId, bool isDiscovered)
    {
        discoveries.SetDiscovered(cardId, isDiscovered);
    }

    // TravelEventEntry helpers
    public static bool ContainsKey(this List<TravelEventEntry> events, string eventId)
    {
        return events.Any(e => e.EventId == eventId);
    }

    // LetterHistoryEntry helpers
    public static bool ContainsKey(this List<LetterHistoryEntry> history, string npcId)
    {
        return history.Any(h => h.NpcId == npcId);
    }

    public static LetterHistory GetItem(this List<LetterHistoryEntry> history, string npcId)
    {
        return history.FindById(npcId)?.History;
    }

    public static void SetItem(this List<LetterHistoryEntry> history, string npcId, LetterHistory letterHistory)
    {
        LetterHistoryEntry existing = history.FindById(npcId);
        if (existing != null)
        {
            existing.History = letterHistory;
        }
        else
        {
            history.Add(new LetterHistoryEntry { NpcId = npcId, History = letterHistory });
        }
    }

    // Values() method for LetterHistoryEntry
    public static IEnumerable<LetterHistory> Values(this List<LetterHistoryEntry> history)
    {
        return history.Select(h => h.History);
    }

    // CardDeckDefinitionEntry helpers
    public static CardDeckDefinition GetItem(this List<CardDeckDefinitionEntry> decks, string deckId)
    {
        return decks.FindById(deckId)?.Definition;
    }

    public static void SetItem(this List<CardDeckDefinitionEntry> decks, string deckId, CardDeckDefinition definition)
    {
        CardDeckDefinitionEntry existing = decks.FindById(deckId);
        if (existing != null)
        {
            existing.Definition = definition;
        }
        else
        {
            decks.Add(new CardDeckDefinitionEntry { DeckId = deckId, Definition = definition });
        }
    }

    public static bool ContainsKey(this List<CardDeckDefinitionEntry> decks, string deckId)
    {
        return decks.Any(d => d.DeckId == deckId);
    }

    public static bool TryGetValue(this List<CardDeckDefinitionEntry> decks, string deckId, out CardDeckDefinition definition)
    {
        CardDeckDefinitionEntry? entry = decks.FindById(deckId);
        definition = entry?.Definition;
        return entry != null;
    }
}