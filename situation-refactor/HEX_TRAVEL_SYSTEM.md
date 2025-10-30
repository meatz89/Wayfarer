# Hex-Based Travel System Specification

## Overview

Wayfarer uses a procedurally-generated hex map to define spatial relationships and travel routes. Players never see the hex grid directly - they experience it through visual novel-style route travel choices. The map fills fractally through AI generation as players discover new areas.

---

## Core Game Loop

### Three-Loop Structure

**SHORT LOOP (10-30 seconds):**
```
Situation presents narrative context
↓
View 2-4 Choices with visible costs (energy/hunger/coins/time)
↓
Select Choice → Pay cost → Receive reward
↓
Resources change, progress advances
↓
Next Situation or destination reached
```

**MEDIUM LOOP (5-15 minutes):**
```
Depart from Tavern (hub)
↓
Select Route to delivery destination
↓
Travel Route (chain of Situations with choices)
↓
Reach destination Location
↓
Complete delivery, earn coins
↓
Return to Tavern
↓
Spend coins on survival (sleep/food)
```

**LONG LOOP (hours):**
```
Accumulate coins from deliveries
↓
Purchase upgrades (better equipment, faster travel)
↓
Access harder/farther delivery routes
↓
Discover new Locations/Venues through exploration
↓
Unlock new narrative content
```

### Resource Management

**Primary Resources:**
- **Coins**: Universal currency (earn from deliveries, spend on survival/upgrades)
- **Energy**: Depletes during travel/choices (restore via sleep at Tavern)
- **Hunger**: Depletes during travel (restore via food/drink at Tavern)
- **Time**: Each action costs time segments (limited per delivery)

**Strategic Pressure:**
All resources compete. Choices offer multiple valid approaches with different costs. No binary gates - all costs are arithmetic comparisons creating impossible choices.

---

## Spatial Architecture

### Entity Hierarchy

```
GameWorld
├── HexMap (procedural scaffolding)
│   └── Hex[] (grid cells with terrain/danger properties)
├── Venues (narrative clusters, radius-based)
│   └── Locations (specific spots within Venue)
│       └── LocationSpot[] (sub-areas within Location)
└── Routes (inter-venue travel paths)
    └── RouteSegment[] (generated from hex path)
```

### Hex

**Properties:**
- **Coordinates**: AxialCoordinates (Q, R)
- **TerrainType**: Plains | Forest | Mountains | Swamp | Road | Impassable
- **DangerLevel**: int (0-10)

**Purpose:**
- Defines world topology
- Determines Route properties through pathfinding
- Never directly visible to player
- Generated procedurally, enriched by AI

### Venue

**Definition:**
Cluster of Locations within hex radius (typically 1-2 hexes). Maximum 7 Locations per Venue.

**Properties:**
- **Id**: string (unique identifier)
- **Name**: string (AI-generated narrative)
- **HexCoordinates**: AxialCoordinates (center point)
- **TravelHubLocationId**: string (EXACTLY one hub Location for inter-venue travel)

**Lifecycle:**
- Created when first Location in area generated
- Subsequent nearby Locations auto-assigned to existing Venue
- Venue boundaries calculated procedurally from hex distance

**Travel Rules:**
- Movement between Locations within same Venue: instant, no cost, no Scenes
- Movement between Venues: requires Route travel with Scenes

### Location

**Properties:**
- **Id**: string
- **Name**: string (AI-generated)
- **VenueId**: string (reference)
- **HexCoordinates**: AxialCoordinates
- **IsVenueTravelHub**: bool (exactly one per Venue)
- **LocationType**: enum (Shop | Residence | Landmark | Hub)
- **Spots**: List<LocationSpot> (sub-areas for Scene placement)

**Ownership:**
- GameWorld.Locations (flat list)
- Location references Venue by ID
- Venue does NOT store Location list (query via Location.VenueId)

**Generation:**
1. Procedural system generates hex coordinates
2. Calculate Venue membership (radius check)
3. Assign mechanical properties (type, coordinates)
4. AI assigns narrative flesh (name, description, atmospheric properties)

### Route

**Definition:**
Bidirectional travel path connecting two venue hub Locations. Generated automatically when new Location created.

**Properties:**
- **Id**: string
- **SourceLocationId**: string (hub Location)
- **DestinationLocationId**: string (hub Location)
- **DangerRating**: int (0-100, sum of hex danger along path)
- **TimeSegments**: int (number of Situations during travel)
- **TransportType**: Walking | Cart (NOT Boat - keep simple)
- **HexPath**: List<AxialCoordinates> (underlying hex sequence)

**Bidirectionality:**
Single Route entity represents both directions. UI presents as two separate travel options but references same Route.

**Generation Algorithm:**
```
When Location with IsVenueTravelHub = true created:
  For each existing hub Location in different Venue:
    Calculate shortest valid hex path (A* pathfinding)
    
    If no valid path exists:
      Skip (no Route created)
    
    Calculate Route properties from hex path:
      DangerRating = Sum(hex.DangerLevel for hex in path)
      TimeSegments = PathLength × TerrainMultipliers
      TransportType = MostRestrictive(hex.TerrainType for hex in path)
    
    Create Route with calculated properties
    Add to GameWorld.Routes
```

**Transport Type Rules:**
- **Walking**: Traverses all passable terrain
- **Cart**: Requires mostly Plains/Road, limited Forest tolerance
- **Impassable**: Blocks all transport types

Route inherits most restrictive transport requirement from its hex path.

**Terrain Multipliers:**
- Plains: 1.0
- Road: 0.8
- Forest: 1.5
- Mountains: 2.0
- Swamp: 2.5
- Impassable: ∞ (blocks path)

---

## Travel Mechanics

### Route Selection

**Player at hub Location:**
1. UI queries GameWorld.Routes where SourceLocationId = currentLocation.Id
2. UI displays available destinations (Route.DestinationLocationId)
3. UI shows Route properties (danger, time, transport type)
4. Player selects destination

**Player at non-hub Location:**
Must travel to Venue's hub Location first (instant movement within Venue).

### Route Travel Execution

**Initialization:**
```
Player selects Route
↓
GameFacade.InitiateRouteTravel(routeId, direction)
↓
SceneInstantiator spawns travel Scenes on Route
↓
Scenes contain Situations with choices
↓
Number of Situations = Route.TimeSegments
```

**Progression:**
```
For each Situation during travel:
  Player views narrative context
  Player evaluates 2-4 Choices (costs visible)
  Player selects Choice
  Resources consumed (energy/hunger/coins/time)
  Progress advances toward destination
  
  If resources depleted:
    Travel fails (forced detour or negative consequence)
  Else:
    Continue to next Situation

Reach destination Location (hub)
Travel complete
```

### Scene Spawning on Routes

**Template Filtering:**
SceneTemplates have PlacementFilters that specify:
- PlacementType = Route
- TransportType compatibility
- DangerRating thresholds
- Narrative themes matching terrain

**Spawning:**
When Route travel initiated, system:
1. Queries SceneTemplates with matching PlacementFilters
2. Selects N templates (N = Route.TimeSegments)
3. Instantiates Scenes at sequential positions along Route
4. Scenes activate as player progresses

**Example Scene Filter:**
```json
{
  "placementType": "Route",
  "transportType": "Walking",
  "minDanger": 30,
  "maxDanger": 70,
  "terrainThemes": ["Forest", "Mountains"]
}
```

---

## Fractal Map Generation

### Initial World State

**Sparse Configuration:**
- 3-5 starting Venues widely separated
- Each Venue has 2-3 Locations
- Routes connect starting Venues (long distance)
- Large empty hex spaces between Venues

### Progressive Expansion

**Discovery Mechanism:**
Player choices during travel can spawn SceneSpawnRewards that:
1. Generate new Location at specified hex coordinates
2. Calculate Venue membership (existing or new)
3. Generate Routes from new hub to existing hubs
4. AI generates narrative content for new Location

**Densification:**
Over time, empty hex spaces fill with new Locations. Initial sparse network becomes dense clusters of Venues connected by Routes.

**AI Integration:**
- Procedural system generates mechanical skeleton (coordinates, terrain, connections)
- AI assigned narrative flesh (names, descriptions, themes, atmospheric properties)
- No code changes needed for new content
- Templates ensure AI-generated content mechanically valid

### Hex Map Storage

**Structure:**
```csharp
public class HexMap
{
    public Dictionary<AxialCoordinates, Hex> Hexes { get; set; }
    public int Radius { get; set; } // Max distance from origin
}

public class Hex
{
    public AxialCoordinates Coordinates { get; set; }
    public TerrainType Terrain { get; set; }
    public int DangerLevel { get; set; }
}
```

**Generation:**
- Initial radius: 20 hexes from origin
- Expands as player discovers edges
- New hexes generated with procedural noise (terrain/danger)
- AI enriches with narrative context when Locations placed

---

## Design Principles Applied

### Principle 1: Single Source of Truth
- GameWorld owns all Hexes, Venues, Locations, Routes
- Locations reference Venues by ID
- Routes reference Locations by ID
- No nested ownership

### Principle 2: Strong Typing
- No Dictionary<string, object>
- TerrainType enum, not string
- TransportType enum, not string
- List<Route>, List<Location>, List<Hex>

### Principle 3: Ownership vs Placement vs Reference
- **Ownership**: GameWorld owns Routes
- **Placement**: Scenes placed on Routes during travel
- **Reference**: Route references source/destination Locations

### Principle 4: Inter-Systemic Rules
- Choices compete for energy/hunger/coins/time
- No boolean gates ("have item X to unlock")
- Arithmetic comparisons ("need 15 energy")
- Multiple valid paths with different costs

### Principle 8: Verisimilitude
- Hex topology defines spatial relationships
- Routes emerge from terrain, not manual authoring
- Travel difficulty matches terrain reality
- Venue clustering matches geographic logic

### Principle 10: Perfect Information
- Route properties visible before travel
- Choice costs visible before selection
- Resource states always displayed
- No hidden gates or surprise costs

---

## Implementation Notes

### Route Generation Performance

**Optimization:**
Routes generated lazily when hub Location created, not all-at-once. For N venues, generates N-1 new Routes per hub, not N² routes total.

**Pathfinding:**
Use A* algorithm with hex distance heuristic. Cache paths to avoid recalculation.

### Save System

**Persisted:**
- Complete HexMap state (terrain, danger)
- All Venues (IDs, coordinates, hub references)
- All Locations (properties, venue membership)
- All Routes (properties, hex paths)
- Player position (current Location)

**Not Persisted:**
- Active travel Scenes (recreated from Route properties)
- Scene templates (loaded from content packages)
- Pathfinding cache (recalculated on load)

### UI Presentation

**Route Selection Screen:**
```
Available Destinations:
- [Location Name] (Venue Name)
  Danger: ██████░░░░ (60/100)
  Time: 4 segments
  Transport: Walking
  [Depart]
```

**During Travel:**
```
Progress: ███░░░░ (3/4 segments complete)
Destination: [Location Name]

[Situation narrative text]

Choices:
1. [Choice 1] - Energy: 10, Time: 1
2. [Choice 2] - Coins: 5, Hunger: 5
3. [Choice 3] - Energy: 5, Time: 2
```

### AI Content Generation

**Mechanical Skeleton (Procedural):**
```json
{
  "locationId": "loc_7f3a9b",
  "hexCoordinates": {"q": 12, "r": -5},
  "venueId": "venue_3d8f1c",
  "isVenueTravelHub": false,
  "locationType": "Landmark"
}
```

**Narrative Flesh (AI):**
```json
{
  "name": "The Crooked Tower",
  "description": "An ancient watchtower, listing dangerously...",
  "atmosphericProperties": ["eerie", "abandoned", "mysterious"],
  "spots": [
    {
      "id": "tower_base",
      "name": "Tower Entrance",
      "properties": {
        "morning": ["quiet", "foggy"],
        "afternoon": ["bright", "exposed"]
      }
    }
  ]
}
```

## Conclusion

The hex-based travel system provides elegant spatial structure without exposing complexity to players. Routes emerge naturally from terrain topology, AI enriches mechanical skeletons with narrative content, and the three-loop structure maintains engagement from moment-to-moment choices through long-term world discovery.

By keeping the hex grid transparent and presenting only meaningful travel choices, the system supports both tight resource management gameplay and expansive world exploration without overwhelming players with minutiae.
