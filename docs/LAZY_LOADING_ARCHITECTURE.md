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
}
```

Similar properties added to:
- `Location`
- `LocationSpot`
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
        // - IsSkeleton = true
    }
    
    public static Location GenerateSkeletonLocation(string id, string source)
    {
        // Creates location with:
        // - Random LocationType
        // - Generic name like "Unknown District"
        // - Default travel hub spot
        // - Valid tier and difficulty
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
            
            gameWorld.Locations.Add(skeleton);
            gameWorld.SkeletonRegistry[dto.LocationId] = "Location";
        }
        
        // Check if this NPC replaces a skeleton
        var existingSkeleton = gameWorld.NPCs
            .FirstOrDefault(n => n.ID == dto.Id && n.IsSkeleton);
        if (existingSkeleton != null)
        {
            // Replace skeleton with real content
            gameWorld.NPCs.Remove(existingSkeleton);
            gameWorld.SkeletonRegistry.Remove(dto.Id);
        }
        
        // Load the NPC
        var npc = NPCParser.ConvertDTOToNPC(dto);
        gameWorld.NPCs.Add(npc);
    }
}
```

### 4. Skeleton Registry

GameWorld tracks all skeletons:

```csharp
public class GameWorld
{
    // Maps skeleton ID to type ("NPC", "Location", "LocationSpot", etc.)
    public Dictionary<string, string> SkeletonRegistry { get; set; }
    
    public List<string> GetSkeletonReport()
    {
        // Returns list of all skeletons needing resolution
    }
}
```

## Use Cases

### 1. AI Content Generation

```
1. AI generates: "NPC 'elena' sends letter to 'mysterious_noble'"
2. System detects 'mysterious_noble' doesn't exist
3. Creates skeleton NPC with ID 'mysterious_noble'
4. Game continues with generic noble
5. Later, AI generates full 'mysterious_noble' content
6. Skeleton replaced with real content
```

### 2. Modular Package Loading

```
Package A: NPCs referencing locations that don't exist
  → Skeletons created for missing locations
  
Package B: Locations that resolve some skeletons
  → Skeletons replaced with real locations
  
Package C: Additional NPCs and remaining locations
  → More skeletons resolved
```

### 3. Procedural World Building

```
1. Core game defines main NPCs and locations
2. Player actions trigger new content needs
3. System identifies missing references
4. Creates skeletons immediately (game playable)
5. Background process generates full content
6. Skeletons gradually replaced with rich content
```

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
- **Context Preservation**: Skeletons maintain relationships

### For Players

- **Always Playable**: Game works even with skeletons
- **Seamless Updates**: Content improves without restarts
- **No Breaking**: New content doesn't break saves
- **Progressive Enhancement**: World gets richer over time

## Testing

The skeleton system includes comprehensive tests:

1. **Skeleton Creation**: Verify skeletons created for missing refs
2. **Skeleton Resolution**: Verify replacement with real content
3. **Deterministic Generation**: Same ID generates same skeleton
4. **Mechanical Completeness**: Skeletons have valid game stats
5. **Playability**: Game works with skeleton content
6. **Multi-Package**: Accumulation across multiple packages

## Example Package Sequence

### Package 1: NPCs with Missing References
```json
{
  "npcs": [{
    "id": "merchant",
    "locationId": "mysterious_tower"  // Doesn't exist!
  }]
}
```
Result: Skeleton created for "mysterious_tower"

### Package 2: Locations Resolving Skeletons
```json
{
  "locations": [{
    "id": "mysterious_tower",
    "name": "The Tower of Echoes"
  }]
}
```
Result: Skeleton replaced with real tower

### Package 3: Additional Content
```json
{
  "npcs": [{
    "id": "tower_guardian",
    "locationId": "mysterious_tower"  // Now exists!
  }]
}
```
Result: No skeleton needed, reference resolved

## Future Enhancements

1. **Skeleton Quality Levels**: Basic → Enhanced → Full content
2. **Skeleton Persistence**: Save which skeletons player has seen
3. **Smart Generation**: Use context to generate better skeletons
4. **Skeleton Themes**: Generate skeletons matching game area
5. **Player Feedback**: Let players request skeleton resolution
6. **Skeleton Statistics**: Track how long skeletons exist

## Conclusion

The lazy loading architecture with skeleton system provides a robust foundation for:
- Dynamic content generation
- AI-driven world building
- Modular content packages
- Graceful content resolution

This ensures the game remains playable at all times while supporting incremental content enhancement.