using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Provides fallback content when referenced entities are missing.
/// This ensures the game can run even with content validation errors,
/// allowing developers to test functionality while fixing content issues.
/// </summary>
public class ContentFallbackService
{
private readonly Dictionary<string, NPC> _fallbackNPCs = new();
private readonly Dictionary<string, Location> _fallbackLocations = new();
private readonly Dictionary<string, RouteOption> _fallbackRoutes = new();
private readonly List<ContentFallbackEntry> _fallbackLog = new();
private readonly GameWorld _gameWorld;

public ContentFallbackService(GameWorld gameWorld)
{
    _gameWorld = gameWorld;
}

/// <summary>
/// Gets or creates a fallback NPC for a missing reference.
/// Returns the real NPC if it exists, otherwise creates a clearly marked fallback.
/// </summary>
public NPC GetOrCreateFallbackNPC(string npcId)
{
    // First check if real NPC exists in GameWorld
    NPC realNpc = _gameWorld.WorldState.GetCharacters()?.FirstOrDefault(n => n.ID == npcId);
    if (realNpc != null)
    {
        return realNpc;
    }

    // Check if we already created a fallback for this ID
    if (_fallbackNPCs.TryGetValue(npcId, out NPC fallbackNpc))
    {
        return fallbackNpc;
    }

    // Create new fallback NPC with clear identification
    fallbackNpc = new NPC
    {
        ID = npcId,
        Name = $"[MISSING: {npcId}]",
        Profession = Professions.Beggar, // Using Beggar as a generic fallback profession
        Location = "market_square", // Default to common location
        SpotId = "central_fountain", // Default to common spot
        Description = $"FALLBACK NPC: Referenced as '{npcId}' but not found in content files",
        ProvidedServices = new List<ServiceTypes>(),
        LetterTokenTypes = new List<ConnectionType> { ConnectionType.Trust }
    };

    // Register the fallback
    _fallbackNPCs[npcId] = fallbackNpc;
    // Note: We can't add to GameWorld NPCs directly during runtime

    // Log the fallback creation
    LogFallback(ContentType.NPC, npcId, "NPC referenced but not defined in npcs.json");

    return fallbackNpc;
}

/// <summary>
/// Gets or creates a fallback location for a missing reference.
/// </summary>
public Location GetOrCreateFallbackLocation(string locationId)
{
    // First check if real location exists in GameWorld
    Location realLocation = _gameWorld.WorldState.locations?.FirstOrDefault(l => l.Id == locationId);
    if (realLocation != null)
    {
        return realLocation;
    }

    // Check if we already created a fallback
    if (_fallbackLocations.TryGetValue(locationId, out Location fallbackLocation))
    {
        return fallbackLocation;
    }

    // Create new fallback location (using constructor)
    fallbackLocation = new Location(locationId, $"[MISSING: {locationId}]")
    {
        Description = $"FALLBACK LOCATION: Referenced as '{locationId}' but not found in content files",
        LocationType = LocationTypes.Settlement
    };

    // Register the fallback
    _fallbackLocations[locationId] = fallbackLocation;
    // Note: LocationRepository doesn't have AddLocation, locations are managed by GameWorld

    // Log the fallback creation
    LogFallback(ContentType.Location, locationId, "Location referenced but not defined in locations.json");

    return fallbackLocation;
}

/// <summary>
/// Validates and patches all content references, creating fallbacks where needed.
/// Call this after initial content loading to ensure all references are resolvable.
/// </summary>
public ContentFallbackReport ValidateAndPatchContent(
    List<DeliveryObligation> letters,
    List<RouteOption> routes)
{
    _fallbackLog.Clear();

    // Conversations now handled by new card-based system
    // No validation needed for old conversation definitions

    // Validate letter sender/recipient references
    foreach (DeliveryObligation letter in letters)
    {
        if (!string.IsNullOrEmpty(letter.SenderId))
        {
            GetOrCreateFallbackNPC(letter.SenderId);
        }
        if (!string.IsNullOrEmpty(letter.RecipientId))
        {
            GetOrCreateFallbackNPC(letter.RecipientId);
        }
    }

    // Validate route location references
    foreach (RouteOption route in routes)
    {
        if (!string.IsNullOrEmpty(route.Origin))
        {
            GetOrCreateFallbackLocation(route.Origin);
        }
        if (!string.IsNullOrEmpty(route.Destination))
        {
            GetOrCreateFallbackLocation(route.Destination);
        }
    }

    // Generate report
    return new ContentFallbackReport
    {
        TotalFallbacksCreated = _fallbackLog.Count,
        FallbacksByType = _fallbackLog.GroupBy(f => f.Type)
            .ToDictionary(g => g.Key, g => g.Count()),
        Details = new List<ContentFallbackEntry>(_fallbackLog),
        HasFallbacks = _fallbackLog.Any()
    };
}

/// <summary>
/// Gets a human-readable summary of all fallbacks created.
/// </summary>
public string GetFallbackSummary()
{
    if (!_fallbackLog.Any())
    {
        return "No content fallbacks needed - all references valid.";
    }

    string summary = $"⚠️ CONTENT FALLBACKS ACTIVE ⚠️\n";
    summary += $"Created {_fallbackLog.Count} fallback entities due to missing content:\n\n";

    foreach (IGrouping<ContentType, ContentFallbackEntry> group in _fallbackLog.GroupBy(f => f.Type))
    {
        summary += $"{group.Key}s ({group.Count()}):\n";
        foreach (ContentFallbackEntry? entry in group)
        {
            summary += $"  - {entry.MissingId}: {entry.Reason}\n";
        }
    }

    summary += "\nThese fallbacks allow the game to run but should be fixed in content files.";
    return summary;
}

/// <summary>
/// Checks if a given entity ID is a fallback (not real content).
/// </summary>
public bool IsFallback(string entityId)
{
    return _fallbackNPCs.ContainsKey(entityId) ||
           _fallbackLocations.ContainsKey(entityId) ||
           _fallbackRoutes.ContainsKey(entityId);
}

private void LogFallback(ContentType type, string missingId, string reason)
{
    _fallbackLog.Add(new ContentFallbackEntry
    {
        Type = type,
        MissingId = missingId,
        Reason = reason,
        CreatedAt = DateTime.Now
    });

    Console.WriteLine($"[CONTENT FALLBACK] Created fallback {type} for '{missingId}': {reason}");
}
}

public enum ContentType
{
NPC,
Location,
Route,
Item,
Letter
}

public class ContentFallbackEntry
{
public ContentType Type { get; set; }
public string MissingId { get; set; }
public string Reason { get; set; }
public DateTime CreatedAt { get; set; }
}

public class ContentFallbackReport
{
public int TotalFallbacksCreated { get; set; }
public Dictionary<ContentType, int> FallbacksByType { get; set; }
public List<ContentFallbackEntry> Details { get; set; }
public bool HasFallbacks { get; set; }
}
