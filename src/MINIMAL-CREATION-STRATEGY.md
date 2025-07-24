# Minimal Creation Strategy for Wayfarer Content

## Overview

This strategy ensures the game can ALWAYS run, regardless of content validation failures or missing references. Every factory provides a minimal creation method that requires only an ID.

## Core Principle

**"The game must never fail to start due to content issues"**

When content loading fails or references are missing, the system automatically creates minimal placeholder entities that satisfy all requirements.

## Implementation Pattern

### 1. Factory Minimal Methods

Every factory implements a `CreateMinimal[Entity]` method:

```csharp
public class LocationFactory
{
    /// <summary>
    /// Create a minimal location with just an ID.
    /// Used for dummy/placeholder creation when references are missing.
    /// </summary>
    public Location CreateMinimalLocation(string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Location ID cannot be empty", nameof(id));
            
        var name = FormatIdAsName(id);
        
        return new Location(id, name)
        {
            Description = $"A location called {name}",
            DomainTags = new List<string> { "GENERIC" },
            ConnectedLocationIds = new List<string>(),
            LocationSpotIds = new List<string>()
        };
    }
}
```

### 2. ID-to-Name Formatting

All factories use consistent ID formatting:

```csharp
private string FormatIdAsName(string id)
{
    // Convert snake_case or kebab-case to Title Case
    // "merchant_quarter" → "Merchant Quarter"
    // "old-town-gate" → "Old Town Gate"
    return string.Join(" ", 
        id.Replace('_', ' ').Replace('-', ' ')
          .Split(' ')
          .Select(word => string.IsNullOrEmpty(word) ? "" : 
              char.ToUpper(word[0]) + word.Substring(1).ToLower()));
}
```

### 3. Minimal Entity Defaults

Each entity type has sensible defaults:

#### Locations
- Name: Generated from ID
- Description: Generic placeholder
- Domain Tags: ["GENERIC"]
- No connections initially

#### NPCs
- Name: Generated from ID
- Profession: Merchant (most flexible)
- Location: Provided or "unknown_location"
- Letter Token Types: ["Common"] (basic interaction)

#### Location Spots
- Name: Generated from ID
- Type: FEATURE (generic type)
- Time Blocks: Morning, Afternoon, Evening (standard availability)
- Domain Tags: ["GENERIC"]

#### Routes
- Method: Walking (always available)
- Cost: 0 coins, 2 stamina (minimal)
- Time: 8 hours (standard day travel)
- Always discovered (no unlock requirements)

#### Letter Templates
- Token Type: Common (universally available)
- Payment: 2-5 coins (low but reasonable)
- Deadline: 24-48 hours (achievable)
- Size: Small (easy to carry)

## Usage in Content Pipeline

### Phase 6: Final Validation

The final phase of content loading creates dummy entities for any missing references:

```csharp
public class Phase6_FinalValidation : IInitializationPhase
{
    public void Execute(InitializationContext context)
    {
        // 1. Ensure player has location
        if (player.CurrentLocation == null)
        {
            var locationFactory = new LocationFactory();
            var dummyLocation = locationFactory.CreateMinimalLocation("dummy_start_location");
            gameWorld.WorldState.locations.Add(dummyLocation);
            player.CurrentLocation = dummyLocation;
        }
        
        // 2. Check all NPC references
        foreach (var missingNPCId in missingNPCs)
        {
            var npcFactory = new NPCFactory();
            var dummyNPC = npcFactory.CreateMinimalNPC(missingNPCId, someLocation.Id);
            gameWorld.WorldState.NPCs.Add(dummyNPC);
        }
        
        // Log what was created
        Console.WriteLine($"Created {createdDummies.Count} dummy entities");
    }
}
```

## Benefits

1. **Guaranteed Game Start**: Content errors never prevent playing
2. **Graceful Degradation**: Missing content replaced with playable defaults
3. **Easy Debugging**: Clear logs show what was auto-created
4. **Progressive Loading**: Can improve content without breaking existing saves
5. **Developer Friendly**: Can test mechanics without complete content

## Best Practices

### 1. Always Log Dummy Creation
```csharp
createdDummies.Add($"NPC: {missingId} at {location.Id}");
Console.WriteLine($"  Created dummy NPC: {missingId} at {location.Id}");
```

### 2. Use Meaningful Defaults
- NPCs should have at least one token type
- Locations should have at least one spot
- Routes should be bidirectional
- All entities should have human-readable names

### 3. Validate After Creation
Even dummy entities must satisfy core requirements:
```csharp
if (player.CurrentLocation == null || player.CurrentLocationSpot == null)
{
    throw new InvalidOperationException(
        "CRITICAL: Failed to initialize player location even with minimal data");
}
```

## Example: Complete Minimal World

```csharp
public class MinimalGameWorldInitializer
{
    public GameWorld Initialize()
    {
        var gameWorld = new GameWorld();
        
        // Two locations for a route
        var millbrook = locationFactory.CreateMinimalLocation("millbrook");
        var thornwood = locationFactory.CreateMinimalLocation("thornwood");
        
        // One spot per location
        var millbrookSquare = spotFactory.CreateMinimalSpot("millbrook_square", "millbrook");
        var thornwoodSquare = spotFactory.CreateMinimalSpot("thornwood_square", "thornwood");
        
        // One NPC per location
        var millbrookElder = npcFactory.CreateMinimalNPC("millbrook_elder", "millbrook");
        var thornwoodMerchant = npcFactory.CreateMinimalNPC("thornwood_merchant", "thornwood");
        
        // Bidirectional route
        var routeToThornwood = new RouteOption
        {
            Id = "walk_millbrook_thornwood",
            Name = "Walk to Thornwood",
            Origin = "millbrook",
            Destination = "thornwood",
            Method = TravelMethods.Walking,
            BaseCoinCost = 0,
            BaseStaminaCost = 2,
            TravelTimeHours = 8
        };
        
        // One letter template
        var basicLetter = new LetterTemplate
        {
            Id = "basic_delivery",
            Name = "Basic Delivery",
            TokenType = "Common",
            PaymentMin = 2,
            PaymentMax = 5,
            DeadlineHoursMin = 24,
            DeadlineHoursMax = 48
        };
        
        // Initialize player
        player.CurrentLocation = millbrook;
        player.CurrentLocationSpot = millbrookSquare;
        
        return gameWorld;
    }
}
```

## Migration Path

1. **Phase 1**: Implement minimal methods in all factories
2. **Phase 2**: Update content pipeline to use minimal creation
3. **Phase 3**: Add validation reporting to identify what needs proper content
4. **Phase 4**: Gradually replace dummy content with real content
5. **Phase 5**: Keep minimal creation as fallback for missing DLC/mods

## Conclusion

This strategy transforms content errors from game-breaking bugs into minor inconveniences. Players can always start playing, while developers get clear feedback about missing content. The game becomes more robust and easier to develop iteratively.