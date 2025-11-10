# Hex-Based Travel System Specification

## Overview

Wayfarer uses a procedurally-generated hex map to define spatial relationships and travel routes. Players never see the hex grid directly - they experience it through visual novel-style route travel choices. The map fills fractally through AI generation as players discover new areas.

---

## ⚠️ PRIME DIRECTIVE: PLAYABILITY OVER IMPLEMENTATION ⚠️

**THE FUNDAMENTAL RULE: A game that compiles but is unplayable is WORSE than a game that crashes.**

Before implementing ANY travel/route/location system:

### Mandatory Playability Validation

1. **Can the player REACH destinations from their starting location?**
   - Trace EXACT route chain from starting location
   - Verify routes exist in JSON and connect locations
   - Confirm player can see routes in Travel UI
   - No broken route chains, no inaccessible location islands

2. **Are routes VISIBLE and EXECUTABLE in UI?**
   - Routes render in TravelManager query results
   - Route cards show costs, danger, and destination
   - Player can click route and initiate travel
   - Travel successfully places player at destination

3. **Do Scenes spawn and appear during travel?**
   - Route travel triggers Scene instantiation
   - Scenes appear as situations during journey
   - Player sees 2-4 choices at each situation
   - Completing situations advances route progress

### Fail-Fast Enforcement

**FORBIDDEN - Silent defaults hiding broken player paths:**
Checking if routes exist before displaying masks problem when route collection empty. Null-coalescing empty list for unavailable routes hides that player trapped at location with no exit routes.

**REQUIRED - Throw exceptions for missing critical connections:**
Validate starting location has at least one outgoing route, throw exception if none exist preventing player soft-lock. Validate route destination location exists in GameWorld.Locations, throw exception if route points to nonexistent location. Fail-fast approach forces fixing broken content rather than silently degrading experience.

### The Playability Test for Travel

For EVERY location/route in the game:

1. **Start game** → Player spawns at starting location
2. **Check routes** → Does location have at least one route visible in UI?
3. **Select route** → Can player click and initiate travel?
4. **During travel** → Do Scenes spawn with 2-4 choices?
5. **Complete travel** → Does player arrive at destination successfully?
6. **From destination** → Does new location have routes or is player trapped?

**If ANY step fails for ANY location, that content is INACCESSIBLE.**

---

## Core Game Loop

### Three-Loop Structure

**SHORT LOOP (10-30 seconds):**
Situation presents narrative context. Player views 2-4 Choices displaying visible costs (energy/hunger/coins/time). Player selects Choice paying cost receiving reward. Resources change and progress advances. Next Situation appears or destination reached.

**MEDIUM LOOP (5-15 minutes):**
Player departs from Tavern hub location. Selects Route to delivery destination from available routes. Travels Route experiencing chain of Situations with choices. Reaches destination Location completing delivery earning coins. Returns to Tavern. Spends coins on survival necessities (sleep/food).

**LONG LOOP (hours):**
Player accumulates coins from multiple deliveries. Purchases upgrades improving capabilities (better equipment, faster travel). Accesses harder or farther delivery routes. Discovers new Locations and Venues through exploration. Unlocks new narrative content through progression.

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

**Spatial Hierarchy:**
GameWorld contains HexMap as procedural scaffolding holding Hex array (grid cells with terrain and danger properties). GameWorld contains Venues as narrative clusters organized by radius containing Locations as specific spots within Venue, Locations contain sub-areas array. GameWorld contains Routes as inter-venue travel paths composed of RouteSegment arrays generated from hex paths.

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
- **Spots**: List<Location> (sub-areas for Scene placement)

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
When Location with IsVenueTravelHub true gets created, system iterates each existing hub Location in different Venue. For each potential connection calculates shortest valid hex path using A-star pathfinding algorithm. If no valid path exists skips that connection creating no Route. If path found calculates Route properties from hex path: DangerRating equals sum of DangerLevel values across all hexes in path, TimeSegments equals path length multiplied by terrain-specific multipliers, TransportType equals most restrictive TerrainType encountered in path. Creates Route entity with calculated properties. Adds Route to GameWorld.Routes collection.

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

### Route Travel as Uniform Experience

**Principle: All Gameplay Uses Scene→Situation→Choice**
Route travel isn't a special system requiring unique mechanics. It uses the exact same Scene→Situation→Choice progression as location visits and NPC interactions. The only difference is PlacementType - scenes spawn on Routes instead of Locations. This architectural uniformity means:
- Content authors learn one pattern that works everywhere
- Player experience remains consistent across all contexts
- No special-case code for "travel sequences" versus "location sequences"
- Route.TimeSegments simply determines how many Situations appear during travel

**Why This Matters:**
Treating travel as a unique system would fragment the codebase, duplicate mechanics, and force players to learn different interaction patterns for different contexts. Uniform architecture means adding travel complexity is just adding more scene templates with PlacementType=Route.

### Placement-Agnostic Content Design

**Principle: SceneTemplates Filter, Don't Specialize**
SceneTemplates specify categorical PlacementFilters (route transport type, danger thresholds, terrain themes) rather than hardcoding behavior for routes versus locations. This means the same situation archetype (negotiation, crisis response, investigation) can spawn anywhere the filters match. A "bandits demand toll" scenario works identically whether spawned at a location checkpoint or mid-route.

**Why This Matters:**
If route scenes required different ChoiceTemplate structure or reward types, content volume explodes. With placement-agnostic design, authors define situations once and let placement filters determine valid spawn contexts. This multiplies content utility without multiplying authoring effort.

---

## Fractal Map Generation

### Sparse-to-Dense Discovery Principle

**The Core Pattern:**
The world begins minimally populated (3-5 starting Venues with 2-3 Locations each, long Routes between them, vast empty hex spaces). Player choices during travel trigger SceneSpawnRewards that generate new Locations at specified hex coordinates. New Locations calculate Venue membership (join existing or create new), auto-generate Routes to existing hub Locations, and receive AI-generated narrative content. Over gameplay hours, the initially sparse network densifies into rich clusters.

**Why Sparse Start:**
Pre-generating complete worlds creates overwhelming initial complexity for players while wasting resources on content players may never reach. Starting sparse and expanding through discovery means:
- New players face manageable early-game navigation
- World complexity scales with player experience
- Generated content reflects player choices (travel routes determine where new locations appear)
- No wasted authoring effort on unreachable areas

### Mechanical-Narrative Separation Principle

**The Division of Labor:**
Procedural systems generate mechanical skeleton (hex coordinates via spatial algorithms, terrain types via noise functions, danger ratings via distance formulas, Route connections via pathfinding). AI systems generate narrative flesh (location names reflecting local themes, descriptions incorporating terrain and history, atmospheric properties matching danger levels, NPC personalities contextually appropriate to location function).

**Why This Separation:**
Procedural systems excel at consistent mechanical generation but produce generic narratives. AI excels at contextual narrative but can't ensure mechanical balance. Separating concerns means:
- Procedural ensures valid spatial topology (no unreachable locations)
- Procedural ensures balanced progression (danger scales with distance)
- AI ensures narrative coherence (names and descriptions feel authored)
- AI ensures atmospheric variety (not all forests feel identical)
- Neither system needs to understand the other's domain

### Lazy Expansion Principle

**The Pattern:**
Hex map starts with small radius (20 hexes from origin). As player approaches edges, new hexes generate using procedural noise for terrain and danger. No pre-generation of full world - only create what player can potentially reach.

**Why Lazy Generation:**
Pre-generating infinite hex grids wastes memory. Most hexes never host Locations and serve only as pathfinding substrate. Generate-on-demand means memory usage scales with actual player exploration, not theoretical world size.

---

## Core Architectural Principles

**Single Source of Truth:** GameWorld owns all spatial entities (Hexes, Venues, Locations, Routes). Entities reference each other by ID, never nested ownership. Location knows its VenueId, Venue doesn't store Location list.

**Strong Typing Enforcement:** TerrainType and TransportType use enums not strings. Route and Location collections use typed Lists not generic Dictionaries. This forces type errors at compile time not runtime.

**Ownership Versus Placement Distinction:** GameWorld OWNS Routes (creates/deletes them). Scenes are PLACED on Routes during travel (temporary association). Routes REFERENCE Locations (stores IDs, doesn't own them). Mixing these concepts creates lifecycle bugs.

**Resource Competition Over Boolean Gates:** Choices during travel compete for shared resources (energy/hunger/coins/time). No "have key to proceed" boolean checks. All gates use arithmetic ("need 15 energy"). This forces meaningful trade-offs - choosing expensive option here means fewer resources later.

**Emergent Complexity From Simple Rules:** Routes emerge from terrain via pathfinding, not manual authoring. Hex danger determines route danger. Terrain types determine transport requirements. Complex travel network emerges from combining simple spatial rules.

**Perfect Information Display:** Route properties (danger, time, transport type) visible before departure. Choice costs visible before selection. Resource states constantly displayed. No hidden requirements discovered mid-journey. Player can make informed decisions, failures come from resource management not hidden information.

## Unifying Architectural Vision

The hex travel system demonstrates core architectural philosophy: **Hidden mechanical complexity supporting visible narrative simplicity**.

Players never see hex coordinates, terrain types, or pathfinding algorithms. They see destinations, danger levels, and travel time. The mechanical substrate (hexes, A-star pathfinding, procedural terrain) exists solely to generate meaningful player decisions (which route to take, which resources to spend, which dangers to risk).

This separation enables:
- Procedural systems to ensure mechanical validity (all locations reachable, danger balanced, routes topologically sound)
- AI systems to ensure narrative quality (names feel authored, descriptions match terrain, atmosphere varies appropriately)
- Players to focus on strategic decisions (resource management, risk assessment) not mechanical minutiae (coordinate navigation, graph traversal)

The three-loop structure (short: choices, medium: routes, long: discovery) maintains engagement across timescales without fragmenting the core Scene→Situation→Choice progression. Travel isn't a separate system - it's scenes placed on routes instead of locations.
