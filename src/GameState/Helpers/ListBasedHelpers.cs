using System;
using System.Collections.Generic;
using System.Linq;


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
    public int Diplomacy { get; set; }
    public int Status { get; set; }
    public int Shadow { get; set; }

    public int GetTokenCount(ConnectionType type)
    {
        return type switch
        {
            ConnectionType.Trust => Trust,
            ConnectionType.Diplomacy => Diplomacy,
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
            case ConnectionType.Diplomacy:
                Diplomacy = value;
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

    public static Dictionary<ConnectionType, int> GetTokens(this List<NPCTokenEntry> tokens, string npcId)
    {
        NPCTokenEntry entry = tokens.FirstOrDefault(t => t.NpcId == npcId);
        if (entry == null) return new Dictionary<ConnectionType, int>();

        return new Dictionary<ConnectionType, int>
        {
            [ConnectionType.Trust] = entry.Trust,
            [ConnectionType.Diplomacy] = entry.Diplomacy,
            [ConnectionType.Status] = entry.Status,
            [ConnectionType.Shadow] = entry.Shadow
        };
    }


    // Card deck helpers

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
    public static IEnumerable<LocationSpot> GetAllSpots(this List<LocationSpotEntry> spots)
    {
        return spots.Select(s => s.Spot);
    }

    // LocationSpotEntry lookups are handled by FindById

    // PathCollection helpers
    public static IEnumerable<PathCardCollectionDTO> GetAllCollections(this List<PathCollectionEntry> collections)
    {
        return collections.Select(c => c.Collection);
    }

    // PathCollectionEntry lookups are handled by FindById and Any()

    // NPC Exchange Card helpers are handled by FindById

    // Card Definition helpers

    // Skeleton Registry helpers
    public static void AddSkeleton(this List<SkeletonRegistryEntry> registry, string key, string contentType)
    {
        if (!registry.Any(r => r.SkeletonKey == key))
        {
            registry.Add(new SkeletonRegistryEntry { SkeletonKey = key, ContentType = contentType });
        }
    }


    // Event deck position helpers
    public static int GetPosition(this List<EventDeckPositionEntry> positions, string deckId)
    {
        return positions.FindById(deckId)?.Position ?? 0;
    }

    public static void SetPosition(this List<EventDeckPositionEntry> positions, string deckId, int position)
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

    // LocationSpotEntry helpers
    public static LocationSpot GetSpot(this List<LocationSpotEntry> spots, string spotId)
    {
        return spots.FindById(spotId)?.Spot;
    }

    public static void AddOrUpdateSpot(this List<LocationSpotEntry> spots, string spotId, LocationSpot spot)
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

    public static void RemoveSpot(this List<LocationSpotEntry> spots, string spotId)
    {
        LocationSpotEntry entry = spots.FindById(spotId);
        if (entry != null)
        {
            spots.Remove(entry);
        }
    }


    // PathCollectionEntry helpers
    public static PathCardCollectionDTO GetCollection(this List<PathCollectionEntry> collections, string collectionId)
    {
        return collections.FindById(collectionId)?.Collection;
    }

    public static void AddOrUpdateCollection(this List<PathCollectionEntry> collections, string collectionId, PathCardCollectionDTO collection)
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


    // TravelEventEntry helpers
    public static TravelEventDTO GetEvent(this List<TravelEventEntry> events, string eventId)
    {
        return events.FindById(eventId)?.TravelEvent;
    }


    // CardDefinitionEntry helpers
    public static ConversationCard GetCard(this List<CardDefinitionEntry> cards, string cardId)
    {
        return cards.FindById(cardId)?.Card;
    }

    public static void AddOrUpdateCard(this List<CardDefinitionEntry> cards, string cardId, ConversationCard card)
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


    // ConversationTypeEntry helpers
    public static ConversationTypeDefinition GetConversationType(this List<ConversationTypeEntry> types, string typeId)
    {
        return types.FindById(typeId)?.Definition;
    }

    public static void AddOrUpdateConversationType(this List<ConversationTypeEntry> types, string typeId, ConversationTypeDefinition definition)
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

    // PathCardDiscoveryEntry helpers already exist above as IsDiscovered/SetDiscovered

    // TravelEventEntry helpers

    // LetterHistoryEntry helpers
    public static LetterHistory GetHistory(this List<LetterHistoryEntry> history, string npcId)
    {
        return history.FindById(npcId)?.History;
    }

    public static void AddOrUpdateHistory(this List<LetterHistoryEntry> history, string npcId, LetterHistory letterHistory)
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

    // Get all histories
    public static IEnumerable<LetterHistory> GetAllHistories(this List<LetterHistoryEntry> history)
    {
        return history.Select(h => h.History);
    }


    // CardDeckDefinitionEntry helpers
    public static CardDeckDefinition GetDeck(this List<CardDeckDefinitionEntry> decks, string deckId)
    {
        return decks.FindById(deckId)?.Definition;
    }

    public static void AddOrUpdateDeck(this List<CardDeckDefinitionEntry> decks, string deckId, CardDeckDefinition definition)
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
}