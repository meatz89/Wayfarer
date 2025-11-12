# Arc42 Section 8: Crosscutting Concepts

## 8.1 Overview

This section documents patterns, principles, and conventions that span multiple building blocks. These crosscutting concepts ensure architectural consistency and guide decision-making across the entire system.

**Organization:**
- **8.2 Architectural Patterns**: Structural patterns used throughout codebase
- **8.3 Design Principles**: Core design philosophy with priority hierarchy
- **8.4 Domain Concepts**: Gameplay-specific patterns and mechanics
- **8.5 Development Practices**: Coding standards and conventions
- **8.6 Pattern Relationships**: How concepts interact and reinforce each other

---

## 8.2 Architectural Patterns

### 8.2.1 HIGHLANDER Pattern (One Concept, One Representation)

**Core Principle**: "There can be only ONE." One concept gets one representation. No redundant storage, no duplicate paths.

#### Sub-Pattern A: Persistence IDs with Runtime Navigation (BOTH ID + Object)

**Use When**: Property from JSON, frequent runtime navigation

**Pattern**:
- ID comes from JSON (persistence/serialization)
- Object resolved by parser via GameWorld lookup ONCE
- Runtime uses ONLY object reference, never ID lookup
- ID immutable after parsing

**Example**:
```csharp
public class Situation {
    public string TemplateId { get; set; }        // From JSON (for save/load)
    public SituationTemplate Template { get; set; } // Resolved once, cached
}

// Parser resolves ONCE
situation.Template = gameWorld.SituationTemplates[situation.TemplateId];

// Runtime uses cached reference (NEVER looks up by ID again)
var choices = situation.Template.ChoiceTemplates;
```

#### Sub-Pattern B: Runtime-Only Navigation (Object ONLY, NO ID)

**Use When**: Runtime state, not from JSON, changes during gameplay

**Pattern**:
- NO ID property exists
- Object reference everywhere consistently
- NEVER add ID alongside object (DESYNC RISK)

**Example**:
```csharp
public class Scene {
    public Situation CurrentSituation { get; set; }  // Runtime state, no ID
}

// Runtime navigation
scene.CurrentSituation = scene.Situations[0];

// ❌ FORBIDDEN - Don't add CurrentSituationId alongside CurrentSituation
// Object IS the single source of truth
```

#### Sub-Pattern C: Lookup on Demand (ID ONLY, NO Object)

**Use When**: From JSON, but infrequent lookups

**Pattern**:
- ID property only
- GameWorld lookup when needed
- No cached object (saves memory)

**Example**:
```csharp
public class SceneSpawnReward {
    public string SceneTemplateId { get; set; }  // Just ID, no cached object
}

// Lookup on demand (rare operation)
var template = gameWorld.SceneTemplates[reward.SceneTemplateId];
```

#### Decision Tree

- **From JSON + frequent access** → Pattern A (BOTH: ID for persistence, Object for runtime)
- **From JSON + rare access** → Pattern C (ID only, lookup on demand)
- **Runtime state only** → Pattern B (Object ONLY, no ID)

---

### 8.2.2 Catalogue Pattern (Parse-Time Translation)

**Core Principle**: JSON has categorical properties, catalogues translate to concrete types at parse-time, runtime uses concrete types directly.

#### Three Phases

**Phase 1: JSON Authoring (Categorical Properties)**
- Authors/AI write descriptive: "Friendly" not 15, "Premium" not 1.6
- Categorical enums: `NPCDemeanor.Friendly`, `Quality.Premium`
- Why: AI can generate without knowing global game state, self-documenting, enables dynamic scaling

**Phase 2: Parsing (Translation - ONE TIME ONLY)**
- Catalogues translate: `NPCDemeanor.Friendly` → StatThreshold = 3 (scaled by 0.6×)
- Called from Parsers folder ONLY
- Translation deterministic, results stored in GameWorld collections

**Phase 3: Runtime (Concrete Values ONLY)**
- NO catalogue calls at runtime
- NO string matching, NO dictionary lookups
- Direct property access: `if (player.Stat >= choice.RequiredStat)`

#### Catalogue Generation Example

```csharp
// Parse-time ONLY
public static List<ChoiceTemplate> GenerateChoices(
    SituationArchetype archetype,
    GenerationContext context
) {
    // Scale by categorical properties
    int scaledThreshold = archetype.BaseStatThreshold;
    if (context.NpcDemeanor == NPCDemeanor.Friendly) {
        scaledThreshold = (int)(scaledThreshold * 0.6);
    }
    if (context.PowerDynamic == PowerDynamic.Superior) {
        scaledThreshold = (int)(scaledThreshold * 1.4);
    }

    return new List<ChoiceTemplate> {
        new ChoiceTemplate {
            StatThreshold = scaledThreshold,  // Concrete int stored
            CoinCost = scaledCoinCost,        // Concrete int stored
            // ALL properties concrete, ready for runtime
        }
    };
}
```

#### FORBIDDEN Forever

- ❌ Runtime catalogue calls (parse-time ONLY)
- ❌ String matching: `if (action.Id == "secure_room")`
- ❌ Dictionary lookups: `Cost["coins"]`, `Cost.ContainsKey("coins")`
- ❌ ID-based routing: Entity IDs are reference only, never control behavior

---

### 8.2.3 Three-Tier Timing Model

**Core Principle**: Content exists in three timing tiers to enable lazy instantiation and reduce memory.

#### Tier 1: Templates (Parse Time)

**When**: Game startup during JSON parsing

**What**: Immutable archetypes defining reusable patterns

**Characteristics**:
- SceneTemplate contains embedded SituationTemplates
- SituationTemplate contains embedded ChoiceTemplates
- Created ONCE from JSON
- Stored in GameWorld.SceneTemplates
- NEVER modified during gameplay

**Example**:
```csharp
public class SceneTemplate {
    public string Id { get; set; }
    public List<SituationTemplate> SituationTemplates { get; set; }
}
```

#### Tier 2: Scenes/Situations (Spawn Time)

**When**: Scene spawns from Obligation or trigger

**What**: Runtime instances with lifecycle and mutable state

**Characteristics**:
- Scene instance created with embedded Situations
- Situation.Template reference stored (ChoiceTemplates NOT instantiated yet)
- InstantiationState = Deferred
- NO actions created in GameWorld collections yet

**Why Deferred**: Creating actions prematurely bloats GameWorld with thousands of inaccessible actions. Player may never reach all situations.

#### Tier 3: Actions (Query Time)

**When**: Player enters context where situation becomes accessible

**What**: Concrete actions queryable by UI, placed in GameWorld collections

**Process**:
1. Check if situation's context met (player at required location/NPC)
2. If met, instantiate actions from Situation.Template.ChoiceTemplates
3. Create concrete Action entities (LocationAction, NPCAction, PathCard)
4. Add to GameWorld.LocationActions/NPCActions/PathCards
5. Set Situation.InstantiationState = Instantiated

**Cleanup**: When scene completes/expires, actions removed from GameWorld

#### Complete Flow

```
PARSE TIME:
JSON → SceneTemplateParser → SceneTemplate
                            → Stored in GameWorld.SceneTemplates

SPAWN TIME:
Obligation triggers → SceneInstantiator → Scene (with Situations)
                                        → InstantiationState = Deferred
                                        → Stored in GameWorld.Scenes

QUERY TIME:
Player at Location → SceneFacade queries → If Deferred + Context Met
                                         → Instantiate Actions
                                         → Add to GameWorld.LocationActions
                                         → Set InstantiationState = Instantiated
```

---

### 8.2.4 Let It Crash Pattern

**Core Principle**: Fail fast with descriptive errors. No graceful degradation, no defaults, no hiding problems.

#### Philosophy

**WRONG**:
```csharp
// ❌ FORBIDDEN - Hides problem with default
if (data == null) {
    data = new DefaultData();
}
```

**CORRECT**:
```csharp
// ✅ Let it crash - Forces fixing root cause
if (data == null) {
    throw new InvalidOperationException("Data missing: expected XYZ from JSON");
}
```

#### Application Areas

**1. Entity Initialization**
```csharp
public List<Situation> Situations { get; set; } = new List<Situation>();

// Parser trusts initialization, assigns directly
scene.Situations = parsedSituations;  // If null, let it crash

// Runtime trusts initialization, queries directly
var count = scene.Situations.Count;  // If null somehow, let it crash
```

**2. Parse-Time Validation**
```csharp
var template = gameWorld.SceneTemplates.FirstOrDefault(t => t.Id == templateId);
if (template == null) {
    throw new InvalidDataException($"SceneTemplate not found: {templateId}");
}
```

**3. Missing Data**
```csharp
// ❌ FORBIDDEN - Default fallback
return choice.StatThreshold ?? 3;

// ✅ CORRECT - Throw if not set
if (choice.StatThreshold == 0) {
    throw new InvalidOperationException($"StatThreshold not set for {choice.Id}");
}
return choice.StatThreshold;
```

#### When NOT to Let It Crash

- **User input validation**: Graceful error messages appropriate
- **External API failures**: Retry logic appropriate
- **Expected gameplay conditions**: Player not meeting requirements (show UI feedback, don't crash)

**Example**:
```csharp
// ✅ CORRECT - Expected gameplay state
if (player.Coins < cost) {
    return new ValidationResult {
        IsValid = false,
        Message = "Not enough coins"
    };  // Don't crash, this is normal gameplay
}
```

---

### 8.2.5 Sentinel Values Pattern

**Core Principle**: Never use null for domain logic. Create explicit sentinel objects with internal flags.

#### Problem with Null

```csharp
// ❌ PROBLEMATIC
public SpawnConditions Conditions { get; set; } = null;  // null = always eligible?

// Later... null checks everywhere
if (conditions != null && conditions.IsEligible(player)) { /* ... */ }
```

**Issues**: Ambiguous meaning, null checks everywhere, easy to forget checks

#### Sentinel Solution

```csharp
// ✅ CORRECT
public class SpawnConditions {
    private bool _isAlwaysEligible;

    public static SpawnConditions AlwaysEligible =>
        new SpawnConditions { _isAlwaysEligible = true };

    public bool IsEligible(Player player) {
        if (_isAlwaysEligible) return true;
        // ... actual condition checks
    }
}

// Usage
public SpawnConditions Conditions { get; set; } = SpawnConditions.AlwaysEligible;

// Later... no null check needed
if (conditions.IsEligible(player)) { /* ... */ }
```

#### Benefits

1. **Explicit intent**: SpawnConditions.AlwaysEligible is clear
2. **No null checks**: Safe to call methods
3. **Type safety**: Compiler enforces correct usage
4. **Self-documenting**: Code reads naturally

#### When to Use

- **Domain defaults**: "Always eligible", "No requirements", "Unlimited"
- **Optional complex objects**: Better than null for objects with methods
- **State machines**: Explicit "None" or "Initial" states

#### When NOT to Use

- **Simple optionals**: `int?`, `string?` are fine
- **Collections**: Empty `List<T>` is the sentinel (never null)
- **Strings**: Empty string `""` is the sentinel (never null)

---

### 8.2.6 Requirement Inversion Pattern

**Core Principle**: Entities spawn into world immediately, requirements filter visibility/selectability, not spawning.

#### Traditional (Boolean Gate) - WRONG

```csharp
// ❌ WRONG - Content doesn't exist until unlock
if (player.CompletedQuest("phase1")) {
    UnlockQuest("phase2");  // Phase 2 created now
}
```

**Problems**: Hidden content, boolean gates, checklist completion, no strategic planning

#### Requirement Inversion - CORRECT

```csharp
// ✅ CORRECT - Phase 2 exists from game start
Scene phase2 = gameWorld.Scenes.First(s => s.Id == "phase2");

// Spawn conditions filter visibility
if (phase2.SpawnConditions.IsEligible(player)) {
    // Player can see/select phase 2
}

// Perfect information display
UI.ShowScene(phase2, isLocked: !phase2.SpawnConditions.IsEligible(player));
```

#### Benefits

- **Perfect Information**: Player sees what's locked and exact requirements
- **Resource Arithmetic**: Numeric comparisons (`player.Rapport >= 6`) instead of boolean flags
- **State-Based Visibility**: Content visible/locked based on game state, not creation flags
- **Resource Competition**: Shared resources force trade-offs, not checklist completion

#### Resource Layers

1. **Personal Stats**: Capability thresholds (Insight, Rapport, Authority, etc.)
2. **Per-Person Relationships**: Individual capital with each NPC
3. **Permanent Resources**: Health, Stamina, Focus, Resolve, Coins
4. **Time as Competition**: Calendar days, time blocks per day
5. **Ephemeral Context**: Current location, active scenes, NPC availability

**Full Documentation**: See ADR-002 in section 9 for comprehensive explanation.

---

### 8.2.7 Multi-Scene NPC Interaction Pattern

**Core Principle**: A single NPC can have multiple independent active scenes simultaneously. Each scene represents a distinct narrative thread with separate lifecycle, situations, and completion state.

#### Physical Presence vs Interactive Opportunities

**Physical Presence (Always Visible)**:
- NPCs exist in game world as physical entities
- When NPC present at location, player always sees them listed
- Represents fiction: "Elena is standing near the fireplace"

**Interactive Opportunities (Conditional)**:
- Interaction buttons appear only when NPC has active scenes
- Each active scene spawns separate button with descriptive label
- Represents available conversation topics or interaction contexts

**Example**:
```
Elena at Common Room (physical presence shown)
├─ Active Scene 1: "Secure Lodging" → Button: "Secure Lodging"
└─ Active Scene 2: "Inn Trouble Brewing" → Button: "Discuss Inn Trouble"

Thomas at Common Room (physical presence shown)
└─ No active scenes → No buttons
```

#### Scene Independence

Each scene maintains independent lifecycle:
- Scene A: "Secure Lodging" (4 situations cascading sequentially)
- Scene B: "Inn Trouble Brewing" (3 situations cascading sequentially)

Completing Scene A does not affect Scene B. Both remain visible and independently playable until each reaches completion criteria.

#### Sequential Situations Within Scenes

Within single scene, situations flow sequentially without interruption:

1. Player clicks "Secure Lodging" button
2. Scene activates, shows Situation 1
3. Player selects choice → Scene cascades to Situation 2 (no return to location view)
4. Player selects choice → Scene cascades to Situation 3
5. Player selects choice → Scene completes, returns to location view

Scene state machine manages CurrentSituationId and AdvanceToNextSituation() for seamless narrative flow.

#### Perfect Information Requirement

Players must see ALL available interaction options for strategic decisions. Hiding scenes because they lack aesthetic labels violates perfect information principle. Architecture prioritizes functionality over cosmetics:

- Scene exists + active situation → Show button (even with placeholder label)
- No active scene → No button (nothing to engage)

#### Multi-Scene Display Pattern

Architecture shift from single-scene to multi-scene:

**Before (Single Scene)**:
```csharp
// Query first active scene only
var scene = gameWorld.Scenes
    .Where(s => s.PlacedNPCId == npc.Id && s.State == Active)
    .FirstOrDefault();

// ViewModel has single label
string InteractionLabel { get; set; }
```

**After (Multi-Scene)**:
```csharp
// Query ALL active scenes
var scenes = gameWorld.Scenes
    .Where(s => s.PlacedNPCId == npc.Id && s.State == Active)
    .ToList();

// ViewModel has multiple scene descriptors
List<NpcSceneViewModel> AvailableScenes { get; set; }
```

#### Label Derivation Hierarchy

When deriving button labels for scenes, use fallback hierarchy:

1. Scene.DisplayName (explicit authored label)
2. First Situation.Name in scene (derived from situation content)
3. Placeholder "Talk to [NPC Name]" (functional default)

Never hide functional scene because it lacks pretty label. Playability trumps aesthetics.

#### Navigation Routing

When player clicks scene button, navigation must route to SPECIFIC scene:

**Before (Ambiguous)**:
```csharp
// Button click passes npcId only
NavigateToNPC(npcId);

// Navigation searches for any active scene at this NPC
var scene = gameWorld.Scenes
    .Where(s => s.PlacedNPCId == npcId && s.State == Active)
    .FirstOrDefault();  // ❌ Ambiguous when multiple scenes exist
```

**After (Explicit)**:
```csharp
// Button click passes (npcId, sceneId) pair
NavigateToScene(npcId, sceneId);

// Navigation uses sceneId for direct lookup
var scene = gameWorld.Scenes.First(s => s.Id == sceneId);  // ✅ Explicit routing
```

#### Spawn Independence

Scenes spawn independently from different sources:

- Tutorial scenes: Spawn at parse-time (concrete npcId binding)
- Obligation scenes: Spawn at runtime (categorical filters)
- Multiple obligations: Can spawn scenes at same NPC simultaneously
- Each scene: Operates independently until completion

This architectural pattern supports rich narrative branching where NPCs serve as hubs for multiple concurrent story threads.

#### Implementation Requirements

1. **Query Pattern**: Use `.Where()` not `.FirstOrDefault()` when fetching NPC scenes
2. **ViewModel Structure**: Support list of available scenes per NPC
3. **UI Rendering**: Loop through available scenes, render one button per scene
4. **Navigation**: Pass both npcId and sceneId for explicit routing
5. **Label Priority**: Prefer Scene.DisplayName, fallback to Situation.Name, fallback to placeholder

---

### 8.2.6 Dynamic World Building (Lazy Materialization Pattern)

**Core Principle**: World expands in response to narrative need, not pre-emptively. Locations and venues materialize when scenes spawn, validated for playability. **All generated locations persist forever** - no cleanup system exists.

#### Pattern Structure

**Two-Phase Lifecycle**:
1. **Generation**: Scene spawns with DependentLocationSpec → Budget validation (fail-fast) → SceneInstantiator generates LocationDTO → PackageLoader parses → Location entity added to GameWorld
2. **Gameplay**: Location used during scene → Player may visit → Location becomes permanent world feature

**Data Flow**:
```
DependentLocationSpec (Template)
    ↓ Scene Spawn
Capacity Validation (fail-fast if venue full: LocationIds.Count >= MaxLocations)
    ↓ Pass
LocationDTO (JSON generated at runtime)
    ↓ PackageLoader
Location Entity (parsed into GameWorld, indistinguishable from authored)
    ↓ Gameplay
Location Persists Forever (no cleanup)
```

#### Components

**Generation**:
- `DependentLocationSpec`: Template defining location to generate (NamePattern, Properties, HexPlacement)
- `VenueTemplate`: Template for procedural venue generation (Type, Tier, District, MaxLocations)
- `SceneInstantiator.BuildLocationDTO()`: DTO generation with fail-fast capacity validation
- `VenueGeneratorService`: Generate venues with hex allocation and capacity budgets

**Matching**:
- `PlacementFilter`: Categorical property matching (LocationProperties, LocationTags, DistrictId)
- `SceneInstantiator.FindMatchingLocation()`: Query existing locations by categorical properties
- `PlacementSelectionStrategy`: Choose ONE from multiple matches (Closest, LeastRecent, WeightedRandom)

**Validation**:
- `LocationPlayabilityValidator`: Fail-fast validation of playability for ALL locations (hex position, reachability, venue, properties, unlock mechanism)
- Capacity validation: Generated checked BEFORE DTO creation (SceneInstantiator), authored checked AFTER parsing (PackageLoader)

**Synchronization**:
- `HexSynchronizationService`: Maintain HIGHLANDER (Location.HexPosition = source, Hex.LocationId = derived)

**Tracking**:
- `SceneProvenance`: Metadata tracking creation source (for debugging only, not lifecycle decisions)
- `Venue.MaxLocations`: Total capacity budget (counts ALL locations: authored + generated)
- `Venue.LocationIds`: Bidirectional relationship maintained by GameWorld.AddOrUpdateLocation
- Budget derived (LocationIds.Count) not tracked (Catalogue Pattern compliance)
- No locking needed: Blazor Server is single-threaded (07_deployment_view.md line 26)

#### Design Decisions

**Match First, Generate Last**:
- PlacementFilter attempts categorical matching FIRST
- DependentLocationSpec triggers explicit generation (not fallback)
- No silent fallback from matching to generation
- Fail-fast if no match and no explicit generation spec

**Rationale**: Authored content priority. If filter can't find match, either author matching content OR relax filter constraints OR add explicit DependentLocationSpec. Never silently degrade.

**All Locations Persist Forever**:
- No cleanup system exists
- Generated locations become permanent world features
- Provenance tracks creation source (metadata only, not lifecycle)
- Budget enforcement critical since violations cannot be cleaned up

**Rationale**: Simplifies architecture. Locations represent player's narrative journey - deleting them erases history. Budget validation prevents unbounded growth instead of cleanup.

**Bounded Infinity Through Fail-Fast Capacity**:
- Venues have MaxLocations capacity (default 20)
- BuildLocationDTO checks capacity BEFORE DTO creation (LocationIds.Count < MaxLocations)
- Throws InvalidOperationException if venue at capacity
- Small venues (5), medium venues (20), large venues (100), wilderness (unlimited)

**Rationale**: Since locations persist forever, budget violations cannot be cleaned up. Prevention through fail-fast validation is essential. Forces spatial design decisions at authoring time.

**Fail-Fast Validation**:
- LocationPlayabilityValidator throws on unplayable content (ALL locations)
- Validation checks: hex position, reachability, venue, properties, unlock mechanism
- System crashes rather than creating inaccessible content
- Catalogue Pattern: No distinction between authored/generated during validation

**Rationale**: Unplayable content worse than crash. Forces fixing root cause in content authoring. Playability over compilation.

#### Integration with Catalogue Pattern

**Generation Flows Through Standard Pipeline**:
1. SceneInstantiator generates LocationDTO (same structure as authored JSON)
2. LocationDTO serialized to Package JSON
3. PackageLoader loads package (same path as authored content)
4. LocationParser parses LocationDTO → Location entity
5. Location added to GameWorld.Locations

**Rationale**: Generated content indistinguishable from authored content after parsing. Same validation, same resolution, same entity structure. Catalogue Pattern compliance.

#### Example: Self-Contained Scene with Private Room

**Template Specification** (JSON):
```json
{
  "sceneArchetypeId": "service_with_location_access",
  "dependentLocations": [{
    "templateId": "private_room",
    "namePattern": "{NPCName}'s Private Room",
    "venueIdSource": "SameAsBase",
    "hexPlacement": "Adjacent",
    "properties": ["sleepingSpace", "restful", "indoor", "private"],
    "isLockedInitially": true
  }]
}
```

**Generation (Scene Spawn)**:
1. SceneInstantiator reads DependentLocationSpec
2. Checks venue capacity: Can add? → Yes (LocationIds.Count < MaxLocations)
3. Generates LocationDTO with NamePattern resolved ("Elena's Private Room")
4. Finds adjacent hex to base location (venue cluster)
5. Creates Package JSON with generated LocationDTO
6. PackageLoader parses → Location entity created (indistinguishable from authored)
7. Provenance stored: `SceneProvenance { SceneId = "scene_tutorial_001" }` (metadata)

**Gameplay**:
- Player negotiates with Elena → Receives room_key item
- Player unlocks private room → Location used during gameplay
- Player rests → Resource restoration based on room properties
- **Location persists forever** → Never deleted, becomes permanent world feature

#### Bootstrap Gradient

**Early Game (Act 1)**: 95% authored, 5% generated
- Core locations authored (villages, inns, major landmarks)
- Only scene-specific resources generated (private rooms, hideouts)
- Stability priority (authored content tested and validated)

**Mid Game (Act 2-3)**: 60% authored, 40% generated
- Major locations authored, minor locations generated
- Generated venues appear for side quests
- Variety increases while maintaining coherence

**Late Game (Act 4+)**: 20% authored, 80% generated
- Only critical story locations authored
- Procedural expansion dominates
- Infinite world growth enabled

**Rationale**: Authored content establishes baseline quality. Generated content provides infinite variety. Gradient manages transition from stability → variety.

#### Hexagonal Architecture Compliance

**Domain Independence**: Location generation services in `src/Services/` (domain), not `src/Content/` (parsing) or `src/Pages/` (UI)

**Catalogue Pattern**: Generated content flows through same pipeline as authored content (JSON → DTO → Parser → Entity)

**HIGHLANDER**: Location.HexPosition = source of truth, Hex.LocationId = derived lookup (single source, synchronized)

**Fail-Fast**: Validation throws on unplayable content (no silent degradation)

---

## 8.3 Design Principles

### 8.3.1 Principle Priority Hierarchy

When principles conflict, resolve via three-tier priority:

#### TIER 1: Non-Negotiable (Never Compromise)

1. **No Soft-Locks**: Always forward progress. If design creates unwinnable state, redesign completely.
2. **Single Source of Truth**: One owner per entity type. Redundant storage creates desync bugs.

**Rule**: If violating TIER 1, **STOP** - redesign completely.

#### TIER 2: Core Experience (Compromise Only with Clear Justification)

3. **Playability Over Compilation**: Game must be testable and playable. Unplayable code is worthless.
4. **Perfect Information at Strategic Layer**: Player can calculate strategic decisions.
5. **Resource Scarcity Creates Choices**: Shared resources force trade-offs.

**Rule**: TIER 2 beats TIER 3. Creative solutions preferred within same tier.

#### TIER 3: Architectural Quality (Prefer but Negotiable)

6. **HIGHLANDER - One Path**: One instantiation path per entity type.
7. **Elegance Through Minimal Interconnection**: Systems connect at explicit boundaries.
8. **Verisimilitude**: Relationships match conceptual model.

**Rule**: Within same tier, find creative solutions satisfying both.

### 8.3.2 Core Design Principles

#### Principle 1: Single Source of Truth + Explicit Ownership

**Statement**: Every piece of game state has exactly ONE canonical storage location.

**Application**:
- GameWorld owns all entities via flat lists
- Parent references children by ID (never inline at runtime)
- ONE owner per entity type
- Test: Can you name owner in one word?

**Example**:
```csharp
// ✅ CORRECT - GameWorld owns Scenes, Scenes own Situations
GameWorld.Scenes → Scene.Situations → Situation

// ❌ WRONG - Parallel collections
GameWorld.Scenes AND GameWorld.Situations  // Desync risk!
```

#### Principle 2: Strong Typing as Design Enforcement

**Statement**: Strong typing and explicit relationships aren't constraints - they're filters that catch bad design before it propagates.

**Type Restrictions**:
- **ONLY**: `List<T>` where T is entity/enum, strongly-typed objects, `int` for numbers
- **FORBIDDEN**: Dictionary, HashSet, var, object, func, lambda (except LINQ), float/double/decimal, tuples

**Rationale**: Type restrictions enforce clear entity relationships, prevent ambiguity, force proper domain modeling.

**Example**:
```csharp
// ❌ FORBIDDEN
Dictionary<string, int> costs;  // Magic strings, runtime errors
float statValue;                 // Game values are discrete

// ✅ CORRECT
int CoinCost { get; set; }       // Explicit property
int StatValue { get; set; }      // Integer only
```

#### Principle 3: Ownership vs Placement vs Reference

**Statement**: Distinguish between ownership (lifecycle), placement (context), and reference (lookup).

**Definitions**:
- **Ownership**: Parent creates/destroys child. If A destroyed, B destroyed.
- **Placement**: Entity appears at location (lifecycle independent).
- **Reference**: Entity A stores Entity B's ID (no lifecycle dependency).

**Example**:
```csharp
// Ownership: Scene owns Situations
Scene.Situations  // If Scene deleted, Situations deleted

// Placement: Scenes placed at Locations
Scene.PlacementType = Location
Scene.PlacementId = "market_square"
// Location lifecycle independent from Scene

// Reference: Choice references Location
Choice.DestinationLocationId = "inn_room"
// Choice doesn't own Location, just references it
```

#### Principle 4: Inter-Systemic Rules Over Boolean Gates

**Statement**: Systems connect via typed rewards applied at completion, not continuous boolean flag evaluation.

**Architectural Implication**: Use typed reward objects (SceneReward, ChoiceReward) with explicit properties instead of boolean flags. This enforces resource arithmetic and prevents hidden unlocks.

**Example**:
```csharp
// ❌ FORBIDDEN - Boolean gate
if (player.CompletedTutorial) {
    EnableAdvancedFeature();  // Free unlock
}

// ✅ CORRECT - Resource arithmetic with typed rewards
if (player.Understanding >= 5) {  // Numeric comparison
    // Feature accessible based on earned resource
}
```

**For game design rationale and resource economy philosophy**, see [design/05_resource_economy.md](design/05_resource_economy.md).

#### Principle 5: Typed Rewards as System Boundaries

**Statement**: Systems connect via typed rewards applied at completion. One-time effect, not continuous state query.

**Example**:
```csharp
// Scene completion applies typed reward
public class SceneReward {
    public List<string> LocationsToUnlock { get; set; }
    public int CoinsToGrant { get; set; }
    public int UnderstandingToGrant { get; set; }
}

// Applied ONCE at completion
foreach (string locId in reward.LocationsToUnlock) {
    location.IsLocked = false;
}
player.Coins += reward.CoinsToGrant;
```

#### Principle 6: Resource Scarcity Creates Impossible Choices

**Statement**: Shared resources (Time, Focus, Stamina, Health) force player to accept one cost to avoid another.

**Architectural Implication**: Model resources as numeric properties on Player entity, use arithmetic comparison throughout codebase. All choice costs and rewards expressed as integer deltas.

**Resource Types**:
- **Shared**: Time, Focus, Stamina, Health, Coins (compete across systems)
- **System-Specific**: Momentum/Progress/Breakthrough (tactical only)

**Test**: Can player pursue all options without trade-offs? If yes, add scarcity.

**For resource economy design philosophy and impossible choices**, see [design/05_resource_economy.md](design/05_resource_economy.md).

#### Principle 7: One Purpose Per Entity

**Statement**: Each entity type serves exactly one purpose.

**Test**: Describe purpose in one sentence without "and"/"or".

**Example**:
```csharp
// ✅ CORRECT
Scene: "Contains sequential situations progressing toward narrative goal"
Situation: "Presents 2-4 choices with visible costs/rewards"

// ❌ WRONG
Scene: "Contains situations AND tracks player reputation AND manages inventory"
// Multiple purposes = wrong entity design
```

#### Principle 8: Verisimilitude in Entity Relationships

**Statement**: Relationships match conceptual model. If explanation feels backwards, design is wrong.

**Example**:
```csharp
// ✅ CORRECT - Feels natural
Scenes appear at Locations (Scenes placed contextually)

// ❌ WRONG - Feels backwards
Locations own Scenes (Location doesn't create/destroy Scenes)
```

#### Principle 9: Elegance Through Minimal Interconnection

**Statement**: Systems connect at explicit boundaries. One arrow per connection.

**Test**: Can you draw system diagram with one arrow per connection? If spaghetti, refactor.

**Example**:
```csharp
// ✅ CORRECT - Explicit boundary
Scene completion → SceneReward applied → LocationFacade.UnlockLocation()

// ❌ WRONG - Tangled dependencies
Scene checks LocationFacade, LocationFacade updates Scene,
Scene notifies UI, UI calls LocationFacade...
```

#### Principle 10: Perfect Information with Hidden Complexity

**Statement**: Strategic layer visible (costs, rewards, requirements). Tactical layer hidden (card draw, challenge flow).

**Architectural Implication**: All ChoiceTemplate properties (costs, requirements, rewards) must be concrete numeric values displayable in UI. Tactical layer sessions separate entity hierarchy, not exposed until entry.

**Test**: Can player decide WHETHER to attempt before entering? If no, violates principle.

**Example**:
```csharp
// Strategic layer - All visible
Choice: "Negotiate diplomatically"
  Entry cost: Stamina -2
  OnSuccess: Unlock private room
  OnFailure: Pay extra 5 coins

// Tactical layer - Hidden until entry
Challenge session: Card draw order unknown, exact challenge flow hidden
```

**For player experience design rationale**, see [design/01_design_vision.md](design/01_design_vision.md).

#### Principle 11: Execution Context Entity Design

**Statement**: Design properties around WHERE they're checked (execution context), not implementation details.

**Process**:
1. Identify facade method checking property
2. Design property for that context
3. Decompose categorical properties to multiple semantic properties
4. Translate at parse-time (catalogues), use concrete at runtime

**Example**:
```csharp
// JSON: Categorical
{ "npcDemeanor": "Friendly", "quality": "Premium" }

// Parse: Translate
StatThreshold = Base × DemeanorMultiplier × QualityMultiplier

// Runtime: Concrete property checked in ChoiceFacade.EvaluateRequirements()
if (player.Rapport >= choice.StatThreshold) { /* ... */ }
```

#### Principle 12: Categorical Properties → Dynamic Scaling

**Statement**: AI generates categorical properties (AI doesn't know global state), catalogues translate with dynamic scaling.

**Why**: Enables infinite AI-generated content without balance knowledge.

**Example**:
```csharp
// AI generates: "Friendly innkeeper at premium inn"
// Catalogue translates:
StatThreshold = 5 × 0.6 (Friendly) = 3 (easy)
CoinCost = 8 × 1.6 (Premium) = 13 (expensive)
// Contextually appropriate without AI knowing numbers
```

#### Principle 13: Playability Over Compilation

**Statement**: Code that compiles but cannot be played/tested is unacceptable.

**Test**: Can QA tester reach this from game start? If no, incomplete.

**Application**:
- Every scene reachable via gameplay path
- Every challenge deck mathematically solvable
- All dependencies validated at parse-time (fail-fast)

---

## 8.4 Domain Concepts

> **Note**: For game design patterns, content archetypes, and player-facing design concepts, see [design/09_design_patterns.md](design/09_design_patterns.md). This section covers technical domain concepts only.

### 8.4.1 Two-Layer Architecture

**Separation**: Strategic layer (perfect information) vs Tactical layer (hidden complexity)

**Strategic Layer**:
- Flow: Obligation → Scene → Situation → Choice
- Perfect information (all costs/rewards visible)
- State machine progression
- Persistent entities

**Tactical Layer**:
- Flow: Challenge Session → Card Play → Resource Accumulation
- Hidden complexity (card draw order unknown)
- Victory thresholds
- Temporary sessions

**Bridge**: ChoiceTemplate.ActionType (Instant, Navigate, StartChallenge)

**See**: ADR-003 in section 9, section 4.1 in solution strategy

### 8.4.2 Four-Choice Archetype (Guaranteed Progression)

**Pattern**: Every A-story situation has 4 choice types ensuring no soft-locks.

**Choice Types**:
1. **Stat-Gated (Optimal)**: Requires stat threshold, best rewards if qualified
2. **Money-Gated (Reliable)**: Costs coins, good rewards if affordable
3. **Challenge (Risky)**: No requirements, variable outcome (success OR failure both progress)
4. **Guaranteed (Patient)**: Zero requirements, minimal rewards, ALWAYS progresses

**Purpose**: Player chooses HOW to progress (optimal/reliable/risky/patient), not IF.

**See**: ADR-001 in section 9 (Infinite A-Story), QS-001 in section 10

### 8.4.3 Scene Lifecycle States

**States**: Provisional → Active → Completed/Expired

- **Provisional**: Created but player hasn't finalized (preview with rollback)
- **Active**: Player committed, scenes progresses through situations
- **Completed**: All situations finished, rewards applied
- **Expired**: ExpirationDays reached, scene removed

**Transitions**: Spawn → Finalize → Progress → Complete/Expire

**See**: Section 6.2 for detailed flow

### 8.4.4 Marker Resolution (Self-Contained Resources)

**Problem**: Templates can't reference what doesn't exist yet (e.g., scene-generated locations).

**Solution**: Logical markers resolved at spawn.

**Pattern**:
```
Template: "generated:private_room" (marker)
  ↓ Spawn-time
Resolution: "location_guid_12345" (actual GUID)
  ↓ Runtime
Usage: Player navigates to location_guid_12345
```

**Benefits**: Reusable templates, instance isolation, no resource sharing between spawns.

**See**: Section 6.6 for complete lifecycle

---

## 8.5 Development Practices

### 8.5.1 Lambda Restrictions

**ALLOWED**:
- LINQ queries: `.Where()`, `.Select()`, `.FirstOrDefault()`
- Blazor event handlers: `@onclick="() => HandleClick()"`
- Framework configuration: `services.AddHttpClient<T>(client => { /* ... */ })`

**FORBIDDEN**:
- Backend event handlers: Use named methods
- DI registration: Explicit initialization, not lambdas
- Backend logic: Use named methods for debugging/testing

**Example**:
```csharp
// ❌ FORBIDDEN
services.AddSingleton<GameWorld>(_ => GameWorldInitializer.CreateGameWorld());

// ✅ CORRECT
GameWorld gameWorld = GameWorldInitializer.CreateGameWorld();
builder.Services.AddSingleton(gameWorld);
```

### 8.5.2 Antipatterns (Strictly Forbidden)

#### ID Antipattern

**FORBIDDEN**:
- ❌ Encoding data in IDs: `Id = $"move_to_{destinationId}"`
- ❌ Parsing IDs: `Id.StartsWith("move_")`, `Id.Split('_')`
- ❌ ID-based routing: `if (action.Id == "secure_room")`

**CORRECT**:
- ✅ ActionType enum for routing: `switch (action.ActionType)`
- ✅ Strongly-typed properties: `action.DestinationLocationId`
- ✅ IDs for uniqueness only (dictionary keys, debugging)

#### Generic Property Modification Antipattern

**FORBIDDEN**:
```csharp
// ❌ String-based property routing
public class PropertyChange {
    public string PropertyName { get; set; }  // "IsLocked"
    public string NewValue { get; set; }       // "true"
}
if (change.PropertyName == "IsLocked") {
    location.IsLocked = bool.Parse(change.NewValue);
}
```

**CORRECT**:
```csharp
// ✅ Explicit strongly-typed properties
public class SceneReward {
    public List<string> LocationsToUnlock { get; set; }
    public List<string> LocationsToLock { get; set; }
}
foreach (string locId in reward.LocationsToUnlock) {
    location.IsLocked = false;  // Direct property access
}
```

### 8.5.3 Code Quality Standards

**NO Exception Handling** (unless explicitly requested):
```csharp
// ❌ FORBIDDEN (unless requested)
try { /* ... */ } catch (Exception ex) { Log.Error(ex); return null; }

// ✅ CORRECT - Let exceptions bubble
var scene = GetSceneById(id);  // Throws if not found
```

**NO Logging** (unless explicitly requested):
- No Log.Info/Debug/Error unless debugging specific issues
- Don't pollute code with logging

**Avoid Comments**:
- Self-documenting code preferred
- Exception: Complex algorithms, non-obvious business rules (rare)

**No Defaults Unless Strictly Necessary**:
```csharp
// ❌ FORBIDDEN - Default fallback
return scene ?? new Scene();

// ✅ CORRECT - Throw on missing
if (scene == null) throw new InvalidOperationException($"Scene not found: {id}");
return scene;
```

### 8.5.4 Semantic Honesty

**Requirement**: Method names MUST match return types exactly. Parameter types match parameter names. Property names describe actual data.

**Example**:
```csharp
// ❌ FORBIDDEN - Name doesn't match return type
public Location GetVenueById(string id) { return location; }

// ✅ CORRECT - Name matches return type
public Location GetLocationById(string id) { return location; }
```

### 8.5.5 Formatting Standards

**FORBIDDEN**:
- ❌ Regions (#region/#endregion)
- ❌ Inline styles in HTML/Blazor
- ❌ Hungarian notation (strTitle, iCount, lstScenes)
- ❌ Labeling docs as "Revised"/"Refined"/"Updated"

**REQUIRED**:
- PascalCase for entities, properties, methods
- CSS in CSS files (separation of concerns)
- Explicit type names (no var)

### 8.5.6 Async Propagation Pattern

**Core Principle**: Async methods propagate upward through the call stack. If a method calls async code, it MUST be async. Never block on async code with `.Wait()` or `.Result`.

**Always Propagate Async:**
```csharp
// ✅ CORRECT - Async propagates to caller
public async Task ProcessSceneAsync(Scene scene) {
    await _repository.SaveSceneAsync(scene);
    await _notificationService.NotifyAsync();
}

// ❌ FORBIDDEN - Blocking on async
public void ProcessScene(Scene scene) {
    _repository.SaveSceneAsync(scene).Wait();  // Deadlock risk
    _notificationService.NotifyAsync().Result;  // Deadlock risk
}
```

**Propagate to UI Layer:**
```csharp
// ✅ CORRECT - Blazor component method is async
@code {
    private async Task HandleClickAsync() {
        await _sceneService.ProcessSceneAsync(scene);
        StateHasChanged();
    }
}

// ❌ FORBIDDEN - Sync method blocking on async
@code {
    private void HandleClick() {
        _sceneService.ProcessSceneAsync(scene).Wait();
        StateHasChanged();
    }
}
```

**Rationale:**
- **Avoid Deadlocks**: `.Wait()` and `.Result` can deadlock in ASP.NET context
- **Better Performance**: Async allows thread pool to handle other work
- **Consistent Pattern**: All I/O operations are async throughout stack
- **Framework Expectation**: ASP.NET Core and Blazor designed for async

**Enforcement:**
- All database operations async
- All HTTP calls async
- All file I/O async
- UI event handlers async when calling async services
- Never use `.Wait()`, `.Result`, or `.GetAwaiter().GetResult()`

**Exception**: Framework configuration code that runs once at startup may use `.GetAwaiter().GetResult()` if truly required (rare).

---

## 8.6 Pattern Relationships

### 8.6.1 Reinforcing Patterns

**HIGHLANDER + Single Source of Truth**:
- HIGHLANDER enforces one representation
- Single Source of Truth enforces one storage location
- Together: One concept, one representation, one location

**Catalogue + Strong Typing**:
- Catalogue translates categorical → concrete
- Strong Typing requires explicit properties
- Together: Parse-time translation, runtime type safety

**Three-Tier Timing + Let It Crash**:
- Three-Tier requires clear timing boundaries
- Let It Crash validates references at parse-time
- Together: Fail-fast at load, safe at runtime

**Requirement Inversion + Perfect Information**:
- Requirement Inversion: Content exists, requirements filter
- Perfect Information: Show exact requirements
- Together: Player calculates affordability before commitment

### 8.6.2 Conflict Resolution

When patterns conflict, apply principle priority (ADR-006):

**TIER 1 beats all**: If No Soft-Locks or Single Source of Truth violated, redesign.

**TIER 2 beats TIER 3**: Playability, Perfect Information, Resource Scarcity override elegance concerns.

**Within same tier**: Creative solutions satisfying both principles.

**Example**:
- **Conflict**: HIGHLANDER (fail-fast) vs Playability (graceful degradation)
- **Resolution**: TIER 3 vs TIER 2 → Playability wins. Use pattern: authoritative ID + ephemeral cache.

### 8.6.3 Pattern Application Matrix

| Pattern | Parse Time | Spawn Time | Query Time | Runtime |
|---------|------------|------------|------------|---------|
| HIGHLANDER | ID from JSON | Object resolved | - | Object used |
| Catalogue | Translate categorical | - | - | Use concrete |
| Three-Tier Timing | Templates created | Scenes spawned | Actions created | Actions executed |
| Let It Crash | Validate references | Validate context | - | Trust initialization |
| Sentinel Values | Create sentinels | - | - | Check sentinels |
| Requirement Inversion | Content spawned | Conditions evaluated | Visibility filtered | Player selects |

---

## 8.7 Summary

The Wayfarer architecture rests on these crosscutting foundations:

**Architectural Patterns**: HIGHLANDER, Catalogue, Three-Tier Timing, Let It Crash, Sentinel Values, Requirement Inversion

**Design Priorities**: TIER 1 (No Soft-Locks, Single Source of Truth) → TIER 2 (Playability, Perfect Information, Resource Scarcity) → TIER 3 (Elegance, Verisimilitude)

**Core Principles**: Strong typing, explicit ownership, one purpose per entity, resource competition, perfect information at strategic layer

**Development Practices**: Semantic honesty, let it crash, no defaults, no abstraction over-engineering

**Common Theme**: Elegance through clarity. One concept, one representation. Perfect information with hidden complexity. Resource scarcity creates impossible choices.

---

## Related Documentation

- **04_solution_strategy.md** - Strategic decisions implementing these patterns
- **09_architecture_decisions.md** - ADRs documenting pattern adoption
- **02_constraints.md** - Constraints derived from these principles
- **10_quality_requirements.md** - Quality scenarios validating pattern application
