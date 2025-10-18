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
        NPCTokenEntry entry = tokens.FirstOrDefault(t => t.NpcId == npcId);
        if (entry == null)
            throw new System.InvalidOperationException($"No token entry found for NPC '{npcId}' - ensure NPC exists before accessing tokens");
        return entry.GetTokenCount(type);
    }

    public static void SetTokenCount(this List<NPCTokenEntry> tokens, string npcId, ConnectionType type, int count)
    {
        NPCTokenEntry entry = tokens.GetNPCTokenEntry(npcId);
        entry.SetTokenCount(type, count);
    }

    public static Dictionary<ConnectionType, int> GetTokens(this List<NPCTokenEntry> tokens, string npcId)
    {
        NPCTokenEntry entry = tokens.FirstOrDefault(t => t.NpcId == npcId);
        if (entry == null)
            throw new System.InvalidOperationException($"No token entry found for NPC '{npcId}' - ensure NPC exists before accessing tokens");

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
        FamiliarityEntry entry = familiarityList.FirstOrDefault(f => f.EntityId == entityId);
        // Lazy initialization: Return 0 if entry doesn't exist yet
        // Entries are created on first SetFamiliarity call
        return entry?.Level ?? 0;
    }

    public static void SetFamiliarity(this List<FamiliarityEntry> familiarityList, string entityId, int level)
    {
        FamiliarityEntry existing = familiarityList.FirstOrDefault(f => f.EntityId == entityId);
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
            System.Reflection.PropertyInfo prop = typeof(T).GetProperty("Id") ??
                      typeof(T).GetProperty("CardId") ??
                      typeof(T).GetProperty("LocationId") ??
                      typeof(T).GetProperty("NpcId") ??
                      typeof(T).GetProperty("TypeId") ??
                      typeof(T).GetProperty("DeckId") ??
                      typeof(T).GetProperty("EventId") ??
                      typeof(T).GetProperty("CollectionId");
            if (prop == null)
                throw new System.InvalidOperationException($"Type {typeof(T).Name} does not have a recognized ID property");
            object value = prop.GetValue(item);
            if (value == null)
                return false;
            return value.ToString() == id;
        });
    }

    // Location helpers - no longer needed, use List<Location> directly

    // Locations are stored directly as List<Location> in GameWorld

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
        EventDeckPositionEntry entry = positions.FindById(deckId);
        if (entry == null)
            throw new System.InvalidOperationException($"No event deck position entry found for deck '{deckId}' - ensure deck exists before accessing position");
        return entry.Position;
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
        PathCardDiscoveryEntry entry = discoveries.FirstOrDefault(d => d.CardId == cardId);
        if (entry == null)
            throw new System.InvalidOperationException($"No discovery entry found for card '{cardId}' - ensure card exists before checking discovery status");
        return entry.IsDiscovered;
    }

    public static void SetDiscovered(this List<PathCardDiscoveryEntry> discoveries, string cardId, bool discovered)
    {
        PathCardDiscoveryEntry existing = discoveries.FirstOrDefault(d => d.CardId == cardId);
        if (existing != null)
        {
            existing.IsDiscovered = discovered;
        }
        else
        {
            discoveries.Add(new PathCardDiscoveryEntry { CardId = cardId, IsDiscovered = discovered });
        }
    }

    // Location helpers - work directly with List<Location>
    public static void AddOrUpdateSpot(this List<Location> locations, string locationId, Location location)
    {
        Location existing = locations.FirstOrDefault(l => l.Id == locationId);
        if (existing != null)
        {
            locations.Remove(existing);
        }
        locations.Add(location);
    }

    public static void RemoveSpot(this List<Location> locations, string locationId)
    {
        Location existing = locations.FirstOrDefault(l => l.Id == locationId);
        if (existing != null)
        {
            locations.Remove(existing);
        }
    }

    // PathCollectionEntry helpers
    public static PathCardCollectionDTO GetCollection(this List<PathCollectionEntry> collections, string collectionId)
    {
        PathCollectionEntry entry = collections.FindById(collectionId);
        if (entry == null)
            throw new System.InvalidOperationException($"No collection entry found for collection '{collectionId}' - ensure collection exists before accessing");
        return entry.Collection;
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
        TravelEventEntry entry = events.FindById(eventId);
        if (entry == null)
            throw new System.InvalidOperationException($"No event entry found for event '{eventId}' - ensure event exists before accessing");
        return entry.TravelEvent;
    }

}
