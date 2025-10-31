# Location Property → Action Generation System

**CRITICAL**: This document describes the CURRENT implementation of action generation. Actions are NOT defined in JSON.

## System Overview

### Core Principle

**Locations are defined by categorical properties → Properties procedurally generate actions at parse time → Runtime filters actions by property matching**

### Data Flow

```
JSON: Location has properties: ["crossroads", "commercial", "restful", "lodging"]
  ↓ (Parse Time - Tier 1)
LocationParser converts property strings → LocationPropertyType enums
  ↓ (Parse Time - Tier 1)
PackageLoader calls LocationActionCatalog.GenerateActionsForLocation()
  ↓ (Parse Time - Tier 1)
Catalogue sees properties → Generates 4 complete LocationAction entities:
  - Travel (from Crossroads)
  - Work (from Commercial)
  - Rest (from Restful)
  - Secure Room (from Lodging)
  ↓ (Parse Time - Tier 1)
Actions stored in GameWorld.LocationActions
  ↓ (Runtime - Tier 3)
LocationActionManager queries GameWorld, filters by location properties
  ↓ (Runtime - Tier 3)
UI displays filtered actions to player
```

## Architecture Components

### 1. Location Property Type Enum

**Location**: `src/GameState/LocationPropertyType.cs`

```csharp
public enum LocationPropertyType
{
    // Movement properties
    Crossroads,     // Hub location, enables travel action

    // Work properties
    Commercial,     // Enables work action (earn coins)

    // Rest properties
    Restful,        // Enables rest action (recover resources)

    // Service properties
    Lodging,        // Enables secure room action (full recovery for coins)

    // ... many more properties for conversation mechanics, atmosphere, etc.
}
```

**Purpose**: Categorical properties that describe WHAT a location IS, not WHAT actions it HAS.

### 2. Action Catalogues (Parse-Time Entity Generators)

**Location**: `src/Content/Catalogs/LocationActionCatalog.cs`, `src/Content/Catalogs/PlayerActionCatalog.cs`

**LocationActionCatalog** - Generates actions from location properties:

```csharp
public static class LocationActionCatalog
{
    /// <summary>
    /// Generate ALL LocationActions for a location based on its categorical properties.
    /// Called by Parser ONLY - runtime never touches this.
    /// </summary>
    public static List<LocationAction> GenerateActionsForLocation(
        Location location,
        List<Location> allLocations)
    {
        List<LocationAction> actions = new List<LocationAction>();

        // Property-based action generation
        actions.AddRange(GeneratePropertyBasedActions(location));

        // Intra-venue movement actions
        actions.AddRange(GenerateIntraVenueMovementActions(location, allLocations));

        return actions;
    }

    private static List<LocationAction> GeneratePropertyBasedActions(Location location)
    {
        List<LocationAction> actions = new List<LocationAction>();

        // Crossroads → Travel action
        if (location.LocationProperties.Contains(LocationPropertyType.Crossroads))
        {
            actions.Add(new LocationAction
            {
                Id = $"travel_{location.Id}",
                Name = "Travel to Another Location",
                Description = "Select a route to travel to another location",
                ActionType = LocationActionType.Travel,
                Costs = ActionCosts.None(),
                Rewards = ActionRewards.None(),
                RequiredProperties = new List<LocationPropertyType> { LocationPropertyType.Crossroads },
                Availability = new List<TimeBlocks>(),  // Always available
                Priority = 100
            });
        }

        // Commercial → Work action
        if (location.LocationProperties.Contains(LocationPropertyType.Commercial))
        {
            actions.Add(new LocationAction
            {
                Id = $"work_{location.Id}",
                Name = "Work",
                Description = "Earn coins through labor. Base pay 8 coins, reduced by hunger penalty.",
                ActionType = LocationActionType.Work,
                Costs = new ActionCosts { /* Time + Stamina handled by intent */ },
                Rewards = new ActionRewards { CoinReward = 8 },
                RequiredProperties = new List<LocationPropertyType> { LocationPropertyType.Commercial },
                Availability = new List<TimeBlocks> { TimeBlocks.Morning, TimeBlocks.Midday, TimeBlocks.Afternoon },
                Priority = 150
            });
        }

        // Restful → Rest action
        if (location.LocationProperties.Contains(LocationPropertyType.Restful))
        {
            actions.Add(new LocationAction
            {
                Id = $"rest_{location.Id}",
                Name = "Rest",
                Description = "Take time to rest and recover. Restores +1 Health and +1 Stamina.",
                ActionType = LocationActionType.Rest,
                Costs = new ActionCosts { /* Time handled by intent */ },
                Rewards = new ActionRewards { HealthRecovery = 1, StaminaRecovery = 1 },
                RequiredProperties = new List<LocationPropertyType> { LocationPropertyType.Restful },
                Availability = new List<TimeBlocks> { TimeBlocks.Morning, TimeBlocks.Midday, TimeBlocks.Afternoon, TimeBlocks.Evening },
                Priority = 50
            });
        }

        // Lodging → Secure Room action
        if (location.LocationProperties.Contains(LocationPropertyType.Lodging))
        {
            actions.Add(new LocationAction
            {
                Id = $"secure_room_{location.Id}",
                Name = "Secure a Room (10 coins)",
                Description = "Purchase a room for the night. Full recovery of all resources.",
                ActionType = LocationActionType.SecureRoom,
                Costs = new ActionCosts { Coins = 10 },
                Rewards = new ActionRewards { FullRecovery = true },
                RequiredProperties = new List<LocationPropertyType> { LocationPropertyType.Lodging },
                Availability = new List<TimeBlocks> { TimeBlocks.Evening },
                Priority = 100
            });
        }

        return actions;
    }

    private static List<LocationAction> GenerateIntraVenueMovementActions(
        Location location,
        List<Location> allLocations)
    {
        List<LocationAction> actions = new List<LocationAction>();

        // Find other locations in same venue
        List<Location> sameVenueLocations = allLocations
            .Where(l => l.VenueId == location.VenueId && l.Id != location.Id)
            .ToList();

        // Generate "Move to X" action for each other location in venue
        foreach (Location destination in sameVenueLocations)
        {
            actions.Add(new LocationAction
            {
                Id = $"move_to_{destination.Id}",
                Name = $"Move to {destination.Name}",
                Description = $"Walk to {destination.Name} within the same venue (instant, free)",
                ActionType = LocationActionType.Travel,
                Costs = ActionCosts.None(),  // FREE
                Rewards = ActionRewards.None(),
                RequiredProperties = new List<LocationPropertyType>(),  // No requirements
                Availability = new List<TimeBlocks>(),  // Always available
                Priority = 90
            });
        }

        return actions;
    }
}
```

**PlayerActionCatalog** - Generates universal actions:

```csharp
public static class PlayerActionCatalog
{
    public static List<PlayerAction> GenerateUniversalActions()
    {
        return new List<PlayerAction>
        {
            new PlayerAction
            {
                Id = "check_belongings",
                Name = "Check Belongings",
                Description = "Review your current equipment and inventory",
                ActionType = PlayerActionType.CheckBelongings,
                Costs = ActionCosts.None(),
                Rewards = ActionRewards.None()
            },
            new PlayerAction
            {
                Id = "wait",
                Name = "Wait",
                Description = "Pass time without activity. Advances 1 time segment.",
                ActionType = PlayerActionType.Wait,
                Costs = new ActionCosts { /* Time handled by intent */ },
                Rewards = ActionRewards.None()
            },
            new PlayerAction
            {
                Id = "sleep_outside",
                Name = "Sleep Outside",
                Description = "Sleep rough without shelter. Costs 2 Health, no recovery.",
                ActionType = PlayerActionType.SleepOutside,
                Costs = new ActionCosts { Health = 2 },
                Rewards = ActionRewards.None()
            }
        };
    }
}
```

**CRITICAL RULES**:
- Catalogues called ONLY at parse time by PackageLoader
- Catalogues generate COMPLETE entities (Id, Name, Description, Costs, Rewards, ALL fields)
- Runtime code NEVER calls catalogues
- `using Wayfarer.Content.Catalogues;` ONLY allowed in Parser/PackageLoader files

### 3. Parse-Time Integration

**Location**: `src/Content/PackageLoader.cs`

```csharp
public void LoadPackage(GamePackage package, bool allowSkeletons)
{
    // ... load locations first ...

    LoadLocations(package.Content.Venues, allowSkeletons);
    LoadLocationSpots(package.Content.Locations, allowSkeletons);

    // CATALOGUE PATTERN: Generate actions from categorical properties (PARSE TIME ONLY)
    // Must happen AFTER all locations loaded
    GeneratePlayerActionsFromCatalogue();
    GenerateLocationActionsFromCatalogue();

    // ... continue with rest of loading ...
}

private void GeneratePlayerActionsFromCatalogue()
{
    List<PlayerAction> playerActions = PlayerActionCatalog.GenerateUniversalActions();

    foreach (PlayerAction action in playerActions)
    {
        if (!_gameWorld.PlayerActions.Any(a => a.Id == action.Id))
        {
            _gameWorld.PlayerActions.Add(action);
        }
    }
}

private void GenerateLocationActionsFromCatalogue()
{
    List<Location> allLocations = _gameWorld.Locations.ToList();

    foreach (Location location in allLocations)
    {
        List<LocationAction> generatedActions = LocationActionCatalog.GenerateActionsForLocation(
            location,
            allLocations
        );

        foreach (LocationAction action in generatedActions)
        {
            if (!_gameWorld.LocationActions.Any(a => a.Id == action.Id))
            {
                _gameWorld.LocationActions.Add(action);
            }
        }
    }
}
```

**KEY POINTS**:
- Called AFTER all locations loaded (needs complete location list for intra-venue movement)
- Actions generated ONCE at initialization
- Actions stored in GameWorld.LocationActions / GameWorld.PlayerActions
- No duplicate checking (same action ID won't be generated twice)

### 4. JSON Content (Properties Only, NO Actions)

**Location**: `src/Content/Core/01_foundation.json`

```json
{
  "locations": [
    {
      "id": "common_room",
      "venueId": "brass_bell_inn",
      "name": "Common Room",
      "description": "Warm inn with lamplight and evening conversations.",
      "properties": {
        "base": [
          "crossroads",
          "commercial"
        ],
        "all": [
          "public",
          "busy",
          "restful",
          "lodging"
        ]
      }
    }
  ]
}
```

**What Parser Does**:
1. Reads `"crossroads"` string → Converts to `LocationPropertyType.Crossroads` enum
2. Reads `"commercial"` string → Converts to `LocationPropertyType.Commercial` enum
3. Reads `"restful"` string → Converts to `LocationPropertyType.Restful` enum
4. Reads `"lodging"` string → Converts to `LocationPropertyType.Lodging` enum
5. Calls `LocationActionCatalog.GenerateActionsForLocation(location, allLocations)`
6. Catalogue sees 4 properties → Generates 4 actions (Travel, Work, Rest, Secure Room)
7. Actions stored in `GameWorld.LocationActions`

**FORBIDDEN**:
```json
// ❌ WRONG - Actions defined in JSON
{
  "locationActions": [
    { "id": "rest", "name": "Rest", "type": "rest" }
  ]
}
```

### 5. Runtime Querying and Filtering

**Location**: `src/Services/LocationActionManager.cs`

```csharp
public List<LocationAction> GetLocationActions(Location location, TimeBlocks currentTime)
{
    // Query GameWorld.LocationActions (NO catalogue calls!)
    return _gameWorld.LocationActions
        .Where(action => action.MatchesLocation(location, currentTime))
        .OrderBy(action => action.Priority)
        .ToList();
}
```

**LocationAction.MatchesLocation()** - Property-based filtering:

```csharp
public class LocationAction
{
    public bool MatchesLocation(Location location, TimeBlocks currentTime)
    {
        // Check required properties
        if (RequiredProperties.Any() &&
            !RequiredProperties.All(p => location.LocationProperties.Contains(p)))
        {
            return false;
        }

        // Check excluded properties
        if (ExcludedProperties.Any(p => location.LocationProperties.Contains(p)))
        {
            return false;
        }

        // Check time availability
        if (Availability.Any() && !Availability.Contains(currentTime))
        {
            return false;
        }

        return true;
    }
}
```

**KEY POINTS**:
- Runtime queries GameWorld (single source of truth)
- NO catalogue calls at runtime
- Property matching filters actions dynamically
- Same action can appear at multiple locations (if properties match)

## Property → Action Mapping Table

| LocationPropertyType | Generated Action | Costs | Rewards | Time Availability |
|---------------------|------------------|-------|---------|-------------------|
| **Crossroads** | Travel to Another Location | None | None | Always |
| **Commercial** | Work | Time + Stamina | 8 Coins | Morning/Midday/Afternoon |
| **Restful** | Rest | Time | +1 Health, +1 Stamina | Morning/Midday/Afternoon/Evening |
| **Lodging** | Secure Room for Night | 10 Coins + Time | Full Recovery | Evening only |
| **[Same Venue]** | Move to [Location Name] | None (FREE) | None | Always |

**Universal Player Actions** (no property requirements):
- Check Belongings (no cost, no time)
- Wait (time cost only)
- Sleep Outside (2 Health cost, no recovery)

## Design Benefits

### 1. Content Creation Flexibility
- Authors assign properties, not actions
- Same property generates consistent actions everywhere
- Easy to understand: "crossroads location" → Travel available

### 2. Balance Changes
- Change Work reward in catalogue → ALL Commercial locations update
- No JSON editing needed
- Single source of truth for action mechanics

### 3. Content Expansion
- Add new `LocationPropertyType.Training` enum value
- Add catalogue rule: `Training → Generate "Train Skills" action`
- ALL locations with Training property get action automatically

### 4. AI Content Generation
- AI assigns categorical properties to new locations
- AI doesn't need to know exact action mechanics
- Actions generate automatically from properties

### 5. No JSON Bloat
- Location definition: 4 property strings
- Generates: 4 complete actions (Id, Name, Description, Costs, Rewards, Availability)
- Huge reduction in JSON file size

## Adding New Property → Action Mappings

To add a new action type:

1. **Add enum value** to `LocationPropertyType.cs`:
```csharp
public enum LocationPropertyType
{
    // ...existing...
    Training,  // NEW: Enables training action
}
```

2. **Add generation rule** to `LocationActionCatalog.GeneratePropertyBasedActions()`:
```csharp
if (location.LocationProperties.Contains(LocationPropertyType.Training))
{
    actions.Add(new LocationAction
    {
        Id = $"train_{location.Id}",
        Name = "Train Skills",
        Description = "Practice and improve your abilities",
        ActionType = LocationActionType.Train,
        Costs = new ActionCosts { Coins = 5 },
        Rewards = new ActionRewards { /* skill XP */ },
        RequiredProperties = new List<LocationPropertyType> { LocationPropertyType.Training },
        Priority = 75
    });
}
```

3. **Use property in JSON**:
```json
{
  "id": "training_yard",
  "properties": {
    "base": ["training"]
  }
}
```

4. **Done** - Action automatically appears at all Training locations

## Testing the System

**Verification Steps**:
1. Build project → Parser calls catalogues at initialization
2. Start game → Actions should be in GameWorld.LocationActions
3. Navigate to location → UI should show actions matching properties
4. Check browser console → No errors about missing actions

**Debug Checklist**:
- ✅ Location has property in JSON → Property in Location.LocationProperties list
- ✅ Property in list → Catalogue sees property → Generates action
- ✅ Action generated → Action in GameWorld.LocationActions
- ✅ Action in GameWorld → LocationActionManager queries → Filters by MatchesLocation()
- ✅ Filtered actions → UI displays → Player can click

## Common Mistakes

### ❌ Calling Catalogues at Runtime
```csharp
// WRONG - Catalogue called at runtime
public List<LocationAction> GetActions(Location location)
{
    return LocationActionCatalog.GenerateActionsForLocation(location); // NO!
}
```

### ❌ Actions Defined in JSON
```json
// WRONG - Actions in JSON
{
  "locationActions": [{ "id": "rest", "name": "Rest" }]
}
```

### ❌ String Matching in Runtime Code
```csharp
// WRONG - String matching
if (action.Id == "secure_room")
{
    // Special handling
}
```

### ✅ CORRECT - Property-Based Filtering
```csharp
// CORRECT - Property-based logic
if (action.RequiredProperties.Contains(LocationPropertyType.Lodging))
{
    // Lodging-specific logic
}
```

## Summary

**THE GOLDEN RULE:**

JSON defines location PROPERTIES → Parser calls CATALOGUE → Catalogue generates ACTIONS → GameWorld stores actions → Runtime QUERIES GameWorld

**NO JSON actions. NO runtime catalogue calls. Property-driven action generation ONLY.**
