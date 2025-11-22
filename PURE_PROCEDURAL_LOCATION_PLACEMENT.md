# PURE PROCEDURAL LOCATION PLACEMENT

## ARCHITECTURAL PRINCIPLE

**Zero Hardcoded Data. Pure Categorical Generation.**

This document defines the CORRECT spatial architecture for location placement in Wayfarer. The system is built on three absolute rules:

1. **NO hardcoded hex coordinates** (Q, R) in JSON
2. **NO entity instance IDs** (venueId, locationId) in JSON
3. **ALL placement emerges from categorical properties** and procedural algorithms

Spatial relationships are NOT authored—they are GENERATED at runtime from semantic properties.

---

## THE PROBLEM WITH CURRENT SYSTEM

**Current Violations:**

1. Locations have `"venueId": "brass_bell_inn"` in JSON
   - This is an entity instance ID cross-reference (FORBIDDEN)
   - Hardcodes spatial relationships (location belongs to specific venue)
   - Prevents procedural generation and AI content creation

2. Locations DO NOT have hex coordinates in JSON
   - Missing spatial source of truth
   - Forces venues to be assigned BEFORE placement (architectural inversion)

3. PlaceAllLocations() relies on pre-assigned venue
   - Groups locations by venue.Name before placement
   - Requires venue to be known at parse time
   - Cannot place locations without hardcoded venue reference

**Architectural Inversion:**
Current system assigns venue BEFORE hex placement, but venue should be determined AFTER placement via spatial containment. The system has cause and effect backwards.

---

## THE CORRECT SYSTEM: CATEGORICAL PROPERTIES

**JSON Input (Authored Locations):**

Locations are defined ONLY by categorical semantic properties:

- `name`: Human-readable identifier (natural key)
- `purpose`: LocationPurpose enum (Dwelling, Commerce, Transit, etc.)
- `safety`: LocationSafety enum (Safe, Neutral, Dangerous)
- `privacy`: LocationPrivacy enum (Public, SemiPublic, Private)
- `activity`: LocationActivity enum (Busy, Moderate, Quiet)
- `properties`: Array of categorical tags (["restful", "indoor", "commercial"])
- `distanceFromPlayer`: Categorical distance hint ("start", "near", "medium", "far", "distant")

**What's ABSENT from JSON:**
- NO `venueId` field (entity instance ID)
- NO `Q`, `R` fields (hardcoded coordinates)
- NO spatial positioning data

**Why This Works:**
Categorical properties provide ENOUGH information for procedural placement without hardcoding spatial relationships. The algorithm matches properties to venues, calculates distance from player, and generates hex positions deterministically.

---

## CATEGORICAL DISTANCE SYSTEM

**The Problem:** How does a content author specify "this location should be near the player" without hardcoding coordinates?

**The Solution:** Categorical distance hints.

**Distance Hint Values:**

- `"start"`: Player origin area (0-1 hex radius)
  - Tutorial locations, always immediately accessible
  - Example: Starting inn, town square

- `"near"`: Early game (2-5 hex radius)
  - Short travel distance, early content
  - Example: Nearby market, local shrine

- `"medium"`: Mid game (6-12 hex radius)
  - Moderate travel, establishes journey rhythm
  - Example: Neighboring village, distant farm

- `"far"`: Late game (13-25 hex radius)
  - Long travel, significant journey
  - Example: Remote fortress, mountain monastery

- `"distant"`: End game (26-50 hex radius)
  - Epic travel, major expedition
  - Example: Ancient ruins, edge of known world

**Key Properties:**

1. **Categorical, not numeric**: "near" is a concept, not a coordinate
2. **Player-relative**: Distance measured FROM player's current position
3. **Deterministic**: Same hint + same player position = same radius range every time
4. **Content-author friendly**: Designers think "I want this near the start" not "I want this at hex (5, 3)"

---

## CATEGORICAL MATCHING: PURPOSE TO TYPE

**The Problem:** How does a location find the right venue without a venueId?

**The Solution:** Semantic property matching.

**Matching Algorithm:**

Locations have a `Purpose` enum. Venues have a `Type` enum. The algorithm maps purpose to type:

- LocationPurpose.**Dwelling** → VenueType.**Inn** or VenueType.**Residential**
- LocationPurpose.**Commerce** → VenueType.**Market** or VenueType.**Commercial**
- LocationPurpose.**Transit** → VenueType.**Crossroads** or VenueType.**Gateway**
- LocationPurpose.**Wilderness** → VenueType.**Wilderness**
- LocationPurpose.**Defense** → VenueType.**Fortress** or VenueType.**Guard**
- LocationPurpose.**Governance** → VenueType.**Administrative**
- LocationPurpose.**Worship** → VenueType.**Temple**
- LocationPurpose.**Learning** → VenueType.**Academy**
- LocationPurpose.**Entertainment** → VenueType.**Theater** or VenueType.**Arena**
- LocationPurpose.**Generic** → ANY VenueType (fallback)

**Semantic Coherence:**
A "Common Room" with Purpose=Dwelling naturally belongs in an Inn venue. A "Market Stall" with Purpose=Commerce naturally belongs in a Market venue. The categorical properties MEAN something—they're not arbitrary tags.

**Zero Hardcoding:**
No entity instance IDs anywhere. All relationships via categorical enum matching. AI can generate locations with Purpose=Dwelling and the system automatically finds Inn venues.

---

## THE SEVEN-PHASE PLACEMENT ALGORITHM

**Overview:** LocationPlacementService.PlaceLocation() takes a Location with categorical properties and procedurally generates its HexPosition.

### **Phase 1: Categorical Distance Translation**

Input: `"distanceFromPlayer": "near"`

Algorithm: Translate categorical hint to hex radius range

Output: `(minRadius: 2, maxRadius: 5)`

This is a deterministic lookup table. No randomness.

---

### **Phase 2: Venue Matching via Categorical Properties**

Input: `location.Purpose = Dwelling`

Algorithm: Find all venues where VenueMatchesLocationPurpose(venue, location) returns true

Output: List of candidate venues (all Inns and Residential venues)

This filters the venue list to only semantically appropriate containers.

---

### **Phase 3: Distance Filtering**

Input:
- Candidate venues from Phase 2
- Distance range from Phase 1: (2, 5)
- Player current position: (0, 0)

Algorithm: Calculate hex distance from venue.CenterHex to player.CurrentPosition using DistanceTo() formula. Keep only venues within radius range.

Output: Venues that are 2-5 hexes from player

This enforces the distance hint spatially.

---

### **Phase 4: Capacity Budget Check**

Input: Distance-filtered venues from Phase 3

Algorithm: Filter venues where venue.CanAddLocation() returns true (LocationIds.Count < MaxLocations)

Output: Venues with available capacity

This prevents unlimited expansion of a single venue. Bounded infinity principle.

---

### **Phase 5: Density Check**

Input: Capacity-filtered venues from Phase 4

Algorithm: Prefer venues with FEWER existing locations

Output: Venues sorted by location count (ascending)

This ensures even distribution across venues. Prevents clumping.

---

### **Phase 6: Venue Selection Strategy**

Input: Filtered and sorted venue list

Algorithm: Select ONE venue using strategy (default: Closest to player)

Output: Single selected venue

If multiple venues equally match all criteria, pick the one CLOSEST to player. Deterministic tiebreaker.

---

### **Phase 7: Hex Position Assignment Within Venue**

Input:
- Selected venue
- Location to place

Algorithm:
1. Get venue's allocated hexes (venue.GetAllocatedHexes())
   - SingleHex venue: 1 hex (center)
   - ClusterOf7 venue: 7 hexes (center + 6 neighbors)
2. Find FIRST unoccupied hex in cluster
3. Assign location.HexPosition = that hex
4. Assign location.Venue = venue object reference

Output: Location with HexPosition and Venue assigned

This is deterministic (first unoccupied hex) and validates spatial containment.

---

## SPATIAL SCAFFOLDING ORDER

**The Correct Sequence:**

1. **Venues Placed FIRST** (VenueGeneratorService.PlaceAuthoredVenues)
   - Venues receive CenterHex procedurally
   - Venue hex territories defined (ClusterOf7 = 7 hexes each)
   - Venues exist in space but contain zero locations

2. **Locations Placed SECOND** (LocationPlacementService.PlaceLocation)
   - For each location with categorical properties:
     - Find matching venues (Purpose → Type)
     - Filter by distance from player
     - Select best venue
     - Assign hex within venue cluster
     - location.HexPosition now set
     - location.Venue now set (object reference)

3. **Spatial Containment Validated THIRD** (AssignVenuesSpatially - optional)
   - For each location:
     - Check venue.ContainsHex(location.HexPosition)
     - Verify location.HexPosition is within venue.GetAllocatedHexes()
   - This is a validation step, not primary assignment
   - Catches bugs where placement violated spatial constraints

**Why This Order:**
Venues define spatial territories. Locations fill those territories. Containment validation ensures correctness. This is venue-first spatial architecture.

---

## CAPACITY BUDGET ENFORCEMENT

**Problem:** Without limits, first venue could accumulate all locations.

**Solution:** MaxLocations per venue (capacity budget).

**How It Works:**

Each venue has `MaxLocations` property (default: 20).

Before placing a location in a venue, check: `venue.LocationIds.Count < venue.MaxLocations`

If capacity reached, venue is excluded from candidate list. Location placed in different venue or new venue generated.

**Benefits:**

1. **Bounded infinity**: Prevents runaway expansion
2. **Geographic distribution**: Forces content across multiple venues
3. **Performance**: Caps complexity per venue
4. **Player experience**: Variety through distribution

---

## DETERMINISTIC GUARANTEES

**Determinism = Same Input Always Produces Same Output**

**Guaranteed Deterministic:**

1. **Distance translation**: "near" → (2, 5) every time
2. **Categorical matching**: Dwelling → Inn every time
3. **Distance calculation**: Hex distance formula is pure math
4. **Capacity check**: LocationIds.Count < MaxLocations is deterministic
5. **Selection strategy**: Closest to player is deterministic
6. **Hex assignment**: First unoccupied hex is deterministic

**NO Randomness:**
- No Random.Shared.Next()
- No GUID generation
- No shuffle operations
- No probabilistic selection

**Result:** Same JSON + same venue layout + same player position = same location placement every game load.

**Why Determinism Matters:**
- Predictable for testing
- Fair for players (no random advantages/disadvantages)
- AI-friendly (deterministic formulas are explainable)
- Debugging-friendly (reproducible bugs)

---

## FALLBACK STRATEGIES

**What if algorithm can't place a location?**

### **Tier 1: No Matching Venues**

Problem: Location has Purpose=Dwelling, but no Inn or Residential venues exist.

Fallback: **THROW ERROR**

Rationale: This is a content creation problem. Either:
- Generate a new Inn venue dynamically
- Change location Purpose to match available venues
- Add Inn venue to content package

Cannot silently fail. Content author must resolve.

---

### **Tier 2: No Venues Within Distance Range**

Problem: "near" hint specifies 2-5 hexes, but all Inn venues are 10+ hexes away.

Fallback: **IGNORE DISTANCE CONSTRAINT**

Rationale: Distance is a preference, not a requirement. If player is at edge of world and all Inns are far, place in nearest Inn anyway. Better to violate distance hint than fail placement.

Console log: "No venues within distance 'near', using all matching venues"

---

### **Tier 3: All Matching Venues At Capacity**

Problem: All Inn venues have reached MaxLocations capacity.

Fallback: **THROW ERROR**

Rationale: This is a world expansion problem. Either:
- Generate a new Inn venue dynamically (VenueGeneratorService)
- Increase MaxLocations on existing venues
- Reduce number of Dwelling locations in content

Cannot silently fail. System must expand venue inventory.

---

## WHAT CHANGES IN JSON

**DELETE These Fields:**

```
"venueId": "brass_bell_inn"     ← Entity instance ID (FORBIDDEN)
"Q": 5                           ← Hardcoded coordinate (FORBIDDEN)
"R": 3                           ← Hardcoded coordinate (FORBIDDEN)
```

**ADD This Field:**

```
"distanceFromPlayer": "start"    ← Categorical distance hint (REQUIRED)
```

**KEEP These Fields:**

```
"name": "Common Room"            ← Natural key (REQUIRED)
"purpose": "Dwelling"            ← Categorical property (REQUIRED)
"safety": "Safe"                 ← Categorical property
"privacy": "SemiPublic"          ← Categorical property
"activity": "Moderate"           ← Categorical property
"properties": ["restful", ...]   ← Categorical tags
```

**Example BEFORE (WRONG):**

```json
{
  "id": "common_room",
  "name": "Common Room",
  "venueId": "The Brass Bell Inn",
  "purpose": "Dwelling",
  "safety": "Safe"
}
```

**Example AFTER (CORRECT):**

```json
{
  "name": "Common Room",
  "purpose": "Dwelling",
  "safety": "Safe",
  "privacy": "SemiPublic",
  "activity": "Moderate",
  "properties": ["restful", "indoor", "commercial"],
  "distanceFromPlayer": "start"
}
```

---

## WHAT CHANGES IN CODE

### **LocationDTO Changes**

DELETE:
- `public string VenueId { get; set; }` property

KEEP:
- All categorical property fields
- ADD: `public string DistanceFromPlayer { get; set; }`

---

### **LocationParser Changes**

DELETE:
- Venue assignment logic (lines that do `location.Venue = gameWorld.Venues.FirstOrDefault(...)`)
- VenueId lookup entirely

MODIFY:
- Store distanceFromPlayer hint temporarily (pass to placement service)

---

### **LocationPlacementService Changes**

DELETE:
- PlaceLocationsInVenue(venue, locations) method signature (assumes venue known)

ADD:
- PlaceLocation(location, distanceHint, player) method
- TranslateDistanceHint(string hint) method
- FindMatchingVenues(location) method
- FilterVenuesByDistance(venues, hint, playerPosition) method
- FilterVenuesByCapacity(venues) method
- SelectVenue(venues, playerPosition) method
- AssignHexPositionInVenue(location, venue) method

---

### **PackageLoader Changes**

MODIFY PlaceAllLocations():
- Change from grouping by pre-assigned venue
- Change to calling PlaceLocation() for each location individually
- Pass location, distanceFromPlayer hint, and current player

---

## IMPLEMENTATION SEQUENCE

**Step 1: Update JSON Files**
- Open 01_foundation.json
- For each location: DELETE venueId, ADD distanceFromPlayer
- Commit: "Remove venueId from location JSON, add distanceFromPlayer hints"

**Step 2: Update LocationDTO**
- Delete VenueId property
- Add DistanceFromPlayer property
- Commit: "Delete VenueId from LocationDTO, add DistanceFromPlayer"

**Step 3: Update LocationParser**
- Remove venue assignment block
- Store distanceFromPlayer for later use
- Commit: "Remove venue assignment from LocationParser"

**Step 4: Implement Categorical Placement**
- Add all seven-phase methods to LocationPlacementService
- Implement venue matching algorithm
- Implement distance translation
- Commit: "Implement pure procedural location placement algorithm"

**Step 5: Update PackageLoader**
- Refactor PlaceAllLocations() to call new algorithm
- Pass player reference for distance calculations
- Commit: "Integrate categorical placement into PackageLoader"

**Step 6: Fix Async Propagation**
- Make GameFacade.StartPhysicalSession() async
- Await all async calls in chain
- Commit: "Fix async propagation through PhysicalFacade"

**Step 7: Test and Verify**
- Build with zero warnings
- Run tests
- Launch game and verify console logs show procedural placement
- Verify venues assigned via spatial containment
- Commit: "Verify pure procedural location placement working"

---

## SUCCESS CRITERIA

**Zero Violations:**
- ✅ No venueId in any location JSON
- ✅ No Q, R coordinates in any location JSON
- ✅ No entity instance IDs in location JSON

**Pure Categorical:**
- ✅ All locations have distanceFromPlayer hint
- ✅ All locations have categorical properties (Purpose, Safety, etc.)
- ✅ Placement derived entirely from categorical matching

**Correct Spatial Flow:**
- ✅ Venues placed first with CenterHex
- ✅ Locations placed second via categorical algorithm
- ✅ Venue assigned via spatial containment (location in venue cluster)

**Deterministic:**
- ✅ Same JSON produces same spatial layout every load
- ✅ No randomness in placement algorithm
- ✅ All formulas are deterministic and visible

**Buildable:**
- ✅ Zero compilation errors
- ✅ Zero warnings
- ✅ All tests pass

**Playable:**
- ✅ Game loads successfully
- ✅ Locations accessible and functional
- ✅ Console logs show placement working correctly

---

## ARCHITECTURAL CORRECTNESS

This system embodies the HIGHLANDER principle: **One concept, one implementation.**

**Spatial Relationships:**
- ONE source of truth: HexPosition
- ONE assignment method: Procedural categorical matching
- ONE validation: Spatial containment

**Zero Duplication:**
- No venueId AND venue object reference (was duplication)
- No Q,R in JSON AND HexPosition in domain (would be duplication)
- Categorical properties in JSON → HexPosition in domain (single transformation)

**Perfect Information:**
- distanceFromPlayer hint visible to content authors
- Distance ranges visible in formula (logged to console)
- Venue matching logic based on documented enums
- All variables observable and verifiable

This is PURE ARCHITECTURE. Zero compromises. Zero half measures. Exactly as it should be.

---

**END OF DOCUMENT**

Next session: Implement this system exactly as specified.
