# Lazy Loading Architecture with Skeleton System

## Overview

The Wayfarer game uses a **lazy loading architecture** with a **skeleton reference system** to handle content that references other content that doesn't exist yet. This is crucial for:

1. **AI-Generated Content**: AI can reference NPCs, locations, or items that don't exist
2. **Modular Content Loading**: Packages can be loaded in any order
3. **Procedural Generation**: Content can be generated on-demand to fill gaps
4. **Graceful Degradation**: Game remains playable even with missing content

## Core Concepts

### Skeletons

A **skeleton** is a mechanically complete but narratively generic placeholder for missing content. Skeletons have:

- All required mechanical properties (stats, types, values)
- Generic narrative content (names, descriptions)
- Tracking metadata (`IsSkeleton`, `SkeletonSource`)
- Deterministic generation based on ID hash
- All 5 persistent decks for NPCs (empty but valid)

### Lazy Resolution

When loading content with missing references:

1. **Detection**: PackageLoader detects missing references
2. **Skeleton Creation**: SkeletonGenerator creates placeholder content
3. **Registration**: Skeleton tracked in GameWorld.SkeletonRegistry
4. **Resolution**: Later packages can replace skeletons with real content
5. **Cleanup**: Resolved skeletons removed from registry

## Architecture Components

### 1. Domain Model Changes

All major domain models have skeleton tracking:

```csharp
public class NPC
{
    // ... existing properties ...
    
    // Skeleton tracking
    public bool IsSkeleton { get; set; } = false;
    public string SkeletonSource { get; set; }
    
    // All 5 persistent decks
    public NPCDecks PersistentDecks { get; set; }
}
```

Similar properties added to:
- `Location` (includes Familiarity property)
- `Location` (includes spot properties for investigation scaling)
- `ConversationCard`

### 2. SkeletonGenerator

Static helper class that generates deterministic skeletons:

```csharp
public static class SkeletonGenerator
{
    public static NPC GenerateSkeletonNPC(string id, string source)
    {
        // Creates NPC with:
        // - Random personality from enum
        // - Generic name like "Unnamed Merchant #3"
        // - Valid mechanical stats
        // - All 5 persistent decks (empty but initialized)
        // - IsSkeleton = true
    }
    
    public static Location GenerateSkeletonLocation(string id, string source)
    {
        // Creates location with:
        // - Random LocationType
        // - Generic name like "Unknown District"
        // - Default travel hub spot
        // - Valid tier and difficulty
        // - Familiarity initialized to 0
        // - MaxFamiliarity set to 3
    }
}
```

### 3. PackageLoader Integration

PackageLoader handles lazy resolution during content loading:

```csharp
private void LoadNPCs(List<NPCDTO> npcDtos)
{
    foreach (var dto in npcDtos)
    {
        // Check if NPC references missing location
        if (!gameWorld.Locations.Any(l => l.Id == dto.LocationId))
        {
            // Create skeleton location
            var skeleton = SkeletonGenerator.GenerateSkeletonLocation(
                dto.LocationId, 
                $"npc_{dto.Id}_reference");
            
            skeleton.Familiarity = 0;
            skeleton.MaxFamiliarity = 3;
            
            gameWorld.Locations.Add(skeleton);
            gameWorld.SkeletonRegistry[dto.LocationId] = "Location";
        }
        
        // Check if this NPC replaces a skeleton
        var existingSkeleton = gameWorld.NPCs
            .FirstOrDefault(n => n.ID == dto.Id && n.IsSkeleton);
        if (existingSkeleton != null)
        {
            // Preserve observation deck cards if any
            var observationCards = existingSkeleton.PersistentDecks.ObservationDeck;
            
            // Replace skeleton with real content
            gameWorld.NPCs.Remove(existingSkeleton);
            gameWorld.SkeletonRegistry.Remove(dto.Id);
            
            // Transfer observation cards to real NPC
            var npc = NPCParser.ConvertDTOToNPC(dto);
            npc.PersistentDecks.ObservationDeck.AddRange(observationCards);
            gameWorld.NPCs.Add(npc);
        }
    }
}
```

### 4. Skeleton Registry

GameWorld tracks all skeletons:

```csharp
public class GameWorld
{
    // Maps skeleton ID to type ("NPC", "Location", "Location", etc.)
    public Dictionary<string, string> SkeletonRegistry { get; set; }
    
    public List<string> GetSkeletonReport()
    {
        // Returns list of all skeletons needing resolution
        return SkeletonRegistry.Select(kvp => $"{kvp.Value}: {kvp.Key}").ToList();
    }
}
```

## NPC Deck Preservation

When replacing skeleton NPCs, special care is taken with persistent decks:

```csharp
public static void ResolveSkeletonNPC(GameWorld gameWorld, NPCDTO dto)
{
    var skeleton = gameWorld.NPCs.FirstOrDefault(n => n.ID == dto.Id && n.IsSkeleton);
    if (skeleton != null)
    {
        // Preserve accumulated cards in all decks
        var preservedDecks = new
        {
            Observation = skeleton.PersistentDecks.ObservationDeck.ToList(),
            Burden = skeleton.PersistentDecks.BurdenDeck.ToList(),
            // Other decks typically empty for skeletons
        };
        
        // Create real NPC
        var realNpc = NPCParser.ConvertDTOToNPC(dto);
        
        // Merge preserved cards into real NPC's decks
        realNpc.PersistentDecks.ObservationDeck.AddRange(preservedDecks.Observation);
        realNpc.PersistentDecks.BurdenDeck.AddRange(preservedDecks.Burden);
        
        // Replace skeleton
        gameWorld.NPCs.Remove(skeleton);
        gameWorld.NPCs.Add(realNpc);
        gameWorld.SkeletonRegistry.Remove(dto.Id);
    }
}
```

## Location Familiarity Preservation

When replacing skeleton locations, familiarity is preserved:

```csharp
public static void ResolveSkeletonLocation(GameWorld gameWorld, LocationDTO dto)
{
    var skeleton = gameWorld.Locations.FirstOrDefault(l => l.Id == dto.Id && l.IsSkeleton);
    if (skeleton != null)
    {
        // Preserve player's familiarity with location
        var preservedFamiliarity = skeleton.Familiarity;
        
        // Create real location
        var realLocation = LocationParser.ConvertDTOToLocation(dto);
        
        // Transfer familiarity
        realLocation.Familiarity = Math.Min(preservedFamiliarity, realLocation.MaxFamiliarity);
        
        // Replace skeleton
        gameWorld.Locations.Remove(skeleton);
        gameWorld.Locations.Add(realLocation);
        gameWorld.SkeletonRegistry.Remove(dto.Id);
    }
}
```

## Use Cases

### 1. AI Content Generation

```
1. AI generates: "NPC 'elena' sends letter to 'mysterious_noble'"
2. System detects 'mysterious_noble' doesn't exist
3. Creates skeleton NPC with ID 'mysterious_noble'
   - Has all 5 persistent decks (empty)
   - Has valid personality and stats
4. Game continues with generic noble
5. Later, AI generates full 'mysterious_noble' content
6. Skeleton replaced with real content
7. Any observation cards accumulated are preserved
```

### 2. Modular Package Loading

```
Package A: NPCs referencing locations that don't exist
  → Skeletons created for missing locations (familiarity = 0)
  
Package B: Locations that resolve some skeletons
  → Skeletons replaced with real locations
  → Player's familiarity preserved
  
Package C: Additional NPCs and remaining locations
  → More skeletons resolved
  → All persistent decks properly initialized
```

### 3. Procedural World Building

```
1. Core game defines main NPCs and locations
2. Player investigates skeleton location
   → Familiarity increases on skeleton
3. Player observes at skeleton location
   → Observation cards go to skeleton NPCs' observation decks
4. System identifies missing references
5. Background process generates full content
6. Skeletons replaced, preserving all progress
```

## Investigation and Observation with Skeletons

Skeleton locations support investigation and observation:

```csharp
public class SkeletonLocation : Location
{
    public SkeletonLocation(string id)
    {
        // ... basic properties ...
        
        // Support investigation
        this.MaxFamiliarity = 3;
        this.Familiarity = 0;
        
        // Create generic spots with properties
        this.Spots.Add(new Location
        {
            Id = $"{id}_main",
            Properties = new Dictionary<string, List<string>>
            {
                ["morning"] = new List<string> { "quiet" },
                ["afternoon"] = new List<string> { "busy" },
                ["evening"] = new List<string> { "closing" }
            }
        });
    }
}
```

This allows the single player of this single-player rpg to:
- Investigate skeleton locations (building familiarity)
- Make observations (though generic until resolved)
- Have consistent spot properties for investigation scaling

## Benefits

### For Development

- **No Load Order Dependencies**: Packages can load in any order
- **Incremental Content**: Add content piece by piece
- **Easy Testing**: Test with partial content
- **Clear Gap Identification**: SkeletonRegistry shows what's missing

### For AI Integration

- **Reference Freedom**: AI can reference anything
- **Graceful Handling**: No crashes from missing content
- **Iterative Generation**: Fill gaps over multiple passes
- **Context Preservation**: Skeletons maintain relationships and progress

### For the single player of this single-player rpg

- **Always Playable**: Game works even with skeletons
- **Progress Preserved**: Familiarity and observation cards carry over
- **Seamless Updates**: Content improves without restarts
- **No Breaking**: New content doesn't break saves
- **Progressive Enhancement**: World gets richer over time

## Testing

The skeleton system includes comprehensive tests:

1. **Skeleton Creation**: Verify skeletons created for missing refs
2. **Skeleton Resolution**: Verify replacement with real content
3. **Deterministic Generation**: Same ID generates same skeleton
4. **Mechanical Completeness**: Skeletons have valid game stats
5. **Deck Preservation**: Observation and burden cards preserved
6. **Familiarity Preservation**: Location familiarity carries over
7. **Playability**: Game works with skeleton content

## Example Package Sequence

### Package 1: NPCs with Missing References
```json
{
  "npcs": [{
    "id": "merchant",
    "locationId": "mysterious_tower",  // Doesn't exist!
    "persistentDecks": {
      "conversationDeck": ["card1", "card2"],
      "requestDeck": ["letter1"],
      "observationDeck": [],
      "burdenDeck": [],
      "exchangeDeck": ["trade1"]
    }
  }]
}
```
Result: Skeleton created for "mysterious_tower" with familiarity system

### Package 2: Locations Resolving Skeletons
```json
{
  "locations": [{
    "id": "mysterious_tower",
    "name": "The Tower of Echoes",
    "maxFamiliarity": 3,
    "spots": [{
      "id": "tower_entrance",
      "properties": {
        "morning": ["quiet"],
        "afternoon": ["mysterious"]
      }
    }]
  }]
}
```
Result: Skeleton replaced with real tower, familiarity preserved

### Package 3: Observation Content
```json
{
  "observations": [{
    "locationId": "mysterious_tower",
    "familiarityRequired": 1,
    "observationCard": {
      "id": "tower_secret",
      "targetNpcId": "merchant",
      "targetDeck": "observation",
      "effect": "UnlockExchange"
    }
  }]
}
```
Result: Observation system fully functional with resolved content

## Future Enhancements

1. **Skeleton Quality Levels**: Basic → Enhanced → Full content
2. **Skeleton Persistence**: Save which skeletons player has seen
3. **Smart Generation**: Use context to generate better skeletons
4. **Skeleton Themes**: Generate skeletons matching game area
5. **Player Feedback**: Let the single player of this single-player rpg request skeleton resolution
6. **Skeleton Statistics**: Track how long skeletons exist
7. **Investigation Patterns**: Skeleton locations adapt spot properties based on nearby real locations

## Conclusion

The lazy loading architecture with skeleton system provides a robust foundation for:
- Dynamic content generation
- AI-driven world building
- Modular content packages
- Graceful content resolution
- Player progress preservation

This ensures the game remains playable at all times while supporting incremental content enhancement. The integration with investigation/familiarity systems and the five-deck NPC structure means even skeleton content provides meaningful gameplay.