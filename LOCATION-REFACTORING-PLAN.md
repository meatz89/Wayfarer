# Location Refactoring Plan

## Core Principle
**Player is ALWAYS at a LocationSpot, never just at a Location**
- Every LocationSpot belongs to exactly one Location
- Player's current Location is derived from their CurrentLocationSpot
- This eliminates dual state tracking and potential inconsistencies

## Architecture Changes

### 1. Player Model Changes
```csharp
// REMOVE:
public Location CurrentLocation { get; set; }

// KEEP:
public LocationSpot CurrentLocationSpot { get; set; }

// ADD:
public Location GetCurrentLocation() 
{
    return CurrentLocationSpot?.Location;
}
```

### 2. Travel System Redesign

#### Intra-Location Travel (Same Location)
- **Command**: `MoveToSpotCommand` 
- **Cost**: 1 stamina, 0 coins
- **Time**: 0 hours (instant)
- **Example**: Moving from "abandoned_warehouse" to "lower_ward_square"

#### Inter-Location Travel (Different Locations)
- **Command**: `TravelCommand` (refactored)
- **Uses**: Routes between locations
- **Cost**: Based on route (stamina + coins)
- **Time**: Based on route
- **Target**: Always a specific spot in the destination location
- **Example**: Travel from "lower_ward_square" to "wharf" (via route)

### 3. Key Refactoring Tasks

#### Phase 1: Add Helper Methods
1. Add `GetCurrentLocation()` method to Player
2. Add location reference to LocationSpot if not already present
3. Create temporary compatibility layer

#### Phase 2: Update Core Systems
1. **LocationRepository**
   - Remove `SetCurrentLocation(Location, LocationSpot)`
   - Add `SetCurrentSpot(LocationSpot)`
   - Update `GetCurrentLocation()` to derive from spot

2. **TravelManager**
   - Update to always target spots, not locations
   - Routes connect spots, not just locations

3. **CommandDiscoveryService**
   - Discover intra-location moves (spot to spot, same location)
   - Discover inter-location travels (via routes to spots)

#### Phase 3: Update All References
Replace all instances of:
- `player.CurrentLocation` → `player.GetCurrentLocation()`
- `gameWorld.GetPlayer().CurrentLocation` → `gameWorld.GetPlayer().GetCurrentLocation()`

#### Phase 4: Remove Deprecated Code
1. Remove `CurrentLocation` property from Player
2. Remove any methods that set location without spot
3. Update serialization/deserialization

## Implementation Order

1. **Create GetCurrentLocation() method** - Add without removing property yet
2. **Update LocationSpot to ensure Location reference** - Critical for deriving location
3. **Create new travel commands**:
   - `MoveToSpotCommand` for same location
   - Update `TravelCommand` to target spots
4. **Update CommandDiscoveryService** - Discover both travel types
5. **Refactor all usages** - Systematic replacement
6. **Remove CurrentLocation property** - Final cleanup

## Benefits
1. **Simpler State Management**: Single source of truth for player position
2. **Clearer Travel Logic**: Always traveling between spots
3. **No Sync Issues**: Can't have location/spot mismatch
4. **Better for Tutorial**: Clear movement between specific spots

## Risks & Mitigations
1. **Risk**: Breaking existing saves
   - **Mitigation**: Add migration logic in deserialization

2. **Risk**: Missing some CurrentLocation references
   - **Mitigation**: Compile errors will catch most; grep for runtime strings

3. **Risk**: UI expects separate location/spot
   - **Mitigation**: Update UI to use GetCurrentLocation()

## Testing Strategy
1. Unit tests for new GetCurrentLocation() method
2. Test both travel types in isolation
3. Full E2E test of tutorial flow
4. Save/load compatibility test