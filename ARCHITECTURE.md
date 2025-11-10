# WAYFARER SYSTEM ARCHITECTURE

**CRITICAL: This document MUST be read and understood before making ANY changes to the Wayfarer codebase.**

**For design philosophy and principles, see [DESIGN_PHILOSOPHY.md](DESIGN_PHILOSOPHY.md)**

## TABLE OF CONTENTS

1. [System Overview](#system-overview)
2. [Content Pipeline Architecture](#content-pipeline-architecture)
3. [Action Execution Pipeline](#action-execution-pipeline)
4. [GameWorld State Management](#gameworld-state-management)
5. [Service & Subsystem Layer](#service--subsystem-layer)
6. [UI Component Architecture](#ui-component-architecture)
7. [Data Flow Patterns](#data-flow-patterns)
8. [Critical Architectural Principles](#critical-architectural-principles)
9. [Component Dependencies](#component-dependencies)

---

## FUNDAMENTAL GAME SYSTEM ARCHITECTURE

**⚠️ READ THIS FIRST - THIS IS THE MOST IMPORTANT SECTION ⚠️**

## TWO-LAYER ARCHITECTURE: STRATEGIC vs TACTICAL

Wayfarer has TWO DISTINCT LAYERS that must NEVER be confused:

1. **STRATEGIC LAYER** - Scene → Situation → Choice (narrative, perfect information, WHAT to attempt)
2. **TACTICAL LAYER** - Mental/Physical/Social Challenges (card gameplay, hidden complexity, HOW to execute)

These layers are connected by the **BRIDGE** (ChoiceTemplate.ActionType) but are fundamentally different systems.

### LAYER 1: STRATEGIC (Perfect Information)

**Purpose**: Narrative progression and player decision-making with complete transparency.

```
Obligation (multi-phase quest)
  ↓ spawns
Scene (persistent narrative container)
  ↓ contains
Situation (narrative moment with 2-4 choices)
  ↓ presents
Choices (player options with visible costs/rewards/requirements)
  ↓ player selects one
Choice Execution (THREE OUTCOMES):
  1. Instant → Apply costs/rewards immediately
  2. Navigate → Move to location/NPC/route
  3. StartChallenge → Spawn tactical subsystem ──┐
                                                  │
                                            BRIDGE CROSSES HERE
```

**Key Strategic Layer Entities:**
- **Scene**: Persistent container in GameWorld.Scenes, owns List<Situation>, tracks CurrentSituation
- **Situation**: Narrative moment embedded in Scene, has Template.ChoiceTemplates (2-4 options)
- **ChoiceTemplate**: Player option with ActionType, costs, requirements, rewards
- **NO victory thresholds** - strategic layer uses state machine progression, not resource accumulation

### LAYER 2: TACTICAL (Hidden Complexity)

**Purpose**: Card-based gameplay execution with emergent tactical depth.

```
                                            BRIDGE CROSSES HERE
                                                  │
Choice with ActionType=StartChallenge ────────────┘
  ↓ spawns
Challenge Session (temporary tactical gameplay)
  ├─ Mental: Progress/Attention/Exposure (investigations)
  ├─ Physical: Breakthrough/Exertion/Danger (obstacles)
  └─ Social: Momentum/Initiative/Doubt (conversations)
  ↓ uses
SituationCards (victory conditions extracted from parent Situation)
  ↓ player builds resource via card play
Threshold Reached (Momentum/Progress/Breakthrough ≥ threshold)
  ↓ grants
SituationCard.Rewards (coins, items, cubes, unlocks)
  ↓ returns to
Strategic Layer (Scene.AdvanceToNextSituation)
```

**Key Tactical Layer Entities:**
- **Challenge Session**: Temporary gameplay (SocialSession, MentalSession, PhysicalSession)
- **SituationCard**: Victory condition with threshold + rewards (stored in Situation.SituationCards)
- **Tactical Cards**: SocialCard, MentalCard, PhysicalCard (playable cards from deck)
- **YES victory thresholds** - tactical layer requires resource accumulation to win

### THE BRIDGE: ChoiceTemplate.ActionType

**How Layers Connect:**

ChoiceTemplate sits at the boundary. Its ActionType property determines execution path:

```csharp
public enum ChoiceActionType
{
    Instant,         // Stay in strategic layer - apply rewards immediately
    Navigate,        // Stay in strategic layer - move player to new context
    StartChallenge   // Cross to tactical layer - spawn challenge session
}
```

**If ActionType = StartChallenge**, additional properties specify the challenge:
- `ChallengeType` (TacticalSystemType): Social/Mental/Physical
- `ChallengeId` (string): Which deck to use for tactical cards
- `OnSuccessReward` / `OnFailureReward`: Applied after challenge outcome

**Bridge Flow:**
1. Player selects Choice with ActionType=StartChallenge
2. GameFacade reads ChallengeType + ChallengeId
3. Appropriate facade (SocialFacade/MentalFacade/PhysicalFacade) creates session
4. Session extracts SituationCards from parent Situation for victory conditions
5. Player plays tactical cards until threshold reached or failure
6. On completion, return to strategic layer with outcome

### CRITICAL: SituationCards Are Tactical, Not Strategic

**WRONG THINKING:**
> "Situation contains SituationCards, so SituationCards are strategic choices."

**CORRECT THINKING:**
> "Situation.SituationCards stores tactical victory conditions. Challenges READ these when spawned. They are NOT strategic choices - they're tactical win conditions."

**Storage vs Usage:**
- **Storage**: SituationCards stored in `Situation.SituationCards` (List<SituationCard>)
- **Usage**: When challenge spawns, it extracts these cards and uses them to determine victory
- **Purpose**: Define "reach 8 Momentum = basic reward" vs "reach 15 Momentum = optimal reward"

**ChoiceTemplates vs SituationCards:**
- **ChoiceTemplate**: Strategic choice presented to player ("Persuade", "Intimidate", "Bribe")
- **SituationCard**: Tactical victory tier ("8 Momentum", "12 Momentum", "15 Momentum")
- Player selects ChoiceTemplate before entering tactical layer
- Player achieves SituationCard thresholds during tactical layer

### ENTITY OWNERSHIP HIERARCHY

```
GameWorld (single source of truth)
 │
 ├─ Scenes (List<Scene>)
 │   ├─ Owns Situations directly (List<Situation>, NOT ID references)
 │   ├─ Tracks CurrentSituation (direct object reference)
 │   ├─ Manages SpawnRules (situation flow)
 │   └─ References placement (PlacementType + PlacementId)
 │
 ├─ Situations (EMBEDDED IN SCENES - NOT separate GameWorld collection)
 │   ├─ Owned by parent Scene
 │   ├─ References Template (SituationTemplate with ChoiceTemplates)
 │   ├─ Has SystemType (Social/Mental/Physical - for bridge routing)
 │   └─ Stores SituationCards (tactical victory conditions - used by challenges)
 │
 ├─ SituationCards (EMBEDDED IN SITUATIONS - tactical victory conditions)
 │   ├─ Stored in Situation.SituationCards list
 │   ├─ Extracted by challenges when spawned (challenges READ them)
 │   ├─ Define threshold (momentum/progress/breakthrough)
 │   └─ Grant rewards on achievement
 │
 ├─ Locations (List<Location>)
 │   └─ PLACEMENT CONTEXT (Scenes appear here, NOT owned by Location)
 │
 └─ NPCs (List<NPC>)
     └─ PLACEMENT CONTEXT (Scenes appear here, NOT owned by NPC)
```

### LAYER SEPARATION EXAMPLES

**Strategic Layer Flow (No Challenge):**
```
Player at Inn Common Room
  → Sees Situation "Pay for Room"
  → Selects Choice "Pay 20 coins"
  → ActionType = Instant
  → Costs applied (20 coins deducted)
  → Rewards applied (room key granted)
  → Scene advances to next Situation
```

**Bridge to Tactical Layer:**
```
Player at Inn Common Room
  → Sees Situation "Negotiate for Better Rate"
  → Selects Choice "Persuade Innkeeper"
  → ActionType = StartChallenge
  → ChallengeType = Social
  → Social challenge spawns
  → Challenge extracts Situation.SituationCards for victory conditions
  → Player enters tactical card gameplay
```

**Tactical Layer Flow:**
```
Social Challenge Active
  → Player has Initiative=8, Momentum=0, Doubt=0
  → Plays SocialCard "Build Rapport" (+2 Momentum, -2 Initiative)
  → Plays SocialCard "Appeal to Greed" (+3 Momentum, +1 Doubt)
  → Momentum=5, Doubt=1
  → Plays SocialCard "Final Push" (+4 Momentum, -3 Initiative)
  → Momentum=9, reaches SituationCard threshold (8 Momentum)
  → SituationCard.Rewards applied (room key + 5 coins)
  → Challenge ends, return to strategic layer
  → Scene advances to next Situation
```

### DATA FLOW EXAMPLE

**Player discovers obligation "Secure Lodging":**

1. **Spawn (Strategic)**: SceneInstantiator spawns Scene with 3 Situations
2. **Placement (Strategic)**: Scene placed at Location "Inn Common Room"
3. **Activation (Strategic)**: Scene.CurrentSituation = Situation 1 "Obtain Room Key"
4. **Choice Presentation (Strategic)**: Player sees 3 ChoiceTemplates:
   - "Pay 20 coins" (Instant)
   - "Negotiate" (StartChallenge → Social)
   - "Steal key" (StartChallenge → Physical)
5. **Player Selects "Negotiate" (Bridge)**
6. **Challenge Spawn (Tactical)**: SocialFacade creates session, extracts SituationCards
7. **Tactical Play (Tactical)**: Player plays SocialCards to build Momentum
8. **Victory (Tactical)**: Reach 15 Momentum threshold, SituationCard.Rewards applied
9. **Return (Bridge)**: Challenge ends, back to strategic layer
10. **Scene Progression (Strategic)**: Scene.AdvanceToNextSituation() → Situation 2
11. **Context Change (Strategic)**: Player navigates to "Inn Upper Floor"
12. **Next Situation (Strategic)**: Situation 2 "Access Locked Room" activates

### FORBIDDEN PATTERNS (DELETE ON SIGHT)

```csharp
// ❌ WRONG - Showing SituationCards in strategic progression flow
Obligation → Scene → Situation → SituationCard  // NO! SituationCard is tactical!

// ❌ WRONG - Treating SituationCards as strategic choices
Situation.ChoiceTemplates = situation.SituationCards  // NO! Different layers!

// ❌ WRONG - GameWorld owning separate Situations collection
public class GameWorld
{
    public List<Situation> Situations { get; set; } // NO! Scenes own Situations!
}

// ❌ WRONG - Situations as tactical layer
"Strategic Layer: Scene/Obligation"
"Tactical Layer: Situation/Challenge"  // NO! Situation is strategic!

// ❌ WRONG - SituationCard as separate reusable Card entity
public class SituationCard : Card { }  // NO! Inline victory condition, not playable card!

// ❌ WRONG - "Obstacle" or "Goal" entities (legacy, deleted)
public class Obstacle { }  // DELETED
public class Goal { }      // DELETED
public class GoalCard { }  // DELETED - replaced by SituationCard
```

### CORRECT PATTERNS

```csharp
// ✅ CORRECT - Scene owns Situations directly (strategic layer)
public class Scene
{
    public List<Situation> Situations { get; set; } = new List<Situation>();
    public Situation CurrentSituation { get; set; }  // Direct object reference
    public SituationSpawnRules SpawnRules { get; set; }
    public PlacementType PlacementType { get; set; }
    public string PlacementId { get; set; }
}

// ✅ CORRECT - Situation stores SituationCards for tactical use
public class Situation
{
    public string Id { get; set; }
    public TacticalSystemType SystemType { get; set; }  // Bridge metadata
    public SituationTemplate Template { get; set; }  // Contains ChoiceTemplates
    public List<SituationCard> SituationCards { get; set; }  // Tactical victory conditions
}

// ✅ CORRECT - ChoiceTemplate bridges layers via ActionType
public class ChoiceTemplate
{
    public ChoiceActionType ActionType { get; set; }  // Instant/Navigate/StartChallenge
    public TacticalSystemType? ChallengeType { get; set; }  // If StartChallenge
    public string ChallengeId { get; set; }  // If StartChallenge
    public ChoiceReward OnSuccessReward { get; set; }  // Applied after challenge
}

// ✅ CORRECT - SituationCard defines tactical victory condition
public class SituationCard
{
    public int threshold { get; set; }  // Universal (Momentum/Progress/Breakthrough)
    public SituationCardRewards Rewards { get; set; }  // On achievement
    public bool IsAchieved { get; set; }  // Runtime tracking
}

// ✅ CORRECT - GameWorld owns Scenes only (strategic layer)
public class GameWorld
{
    public List<Scene> Scenes { get; set; } = new List<Scene>();
    // NO separate Situations - Scenes own them
    // NO Challenges collection - challenges are temporary sessions
}
```

### KEY ARCHITECTURAL RULES

**Strategic Layer Rules:**
1. Scene → Situation → Choice is the COMPLETE strategic flow
2. Situations do NOT progress to SituationCards (different layer)
3. Perfect information: All costs, requirements, rewards visible before selection
4. State machine: Scene.AdvanceToNextSituation() manages progression
5. Persistent: Scenes exist until completed/expired

**Tactical Layer Rules:**
1. Challenge sessions are TEMPORARY (created/destroyed per engagement)
2. SituationCards define victory conditions with thresholds
3. Three parallel systems: Social/Mental/Physical with equivalent depth
4. Hidden complexity: Card draw, exact challenge flow not visible before entry
5. Return to strategic layer on completion with success/failure outcome

**Bridge Rules:**
1. ChoiceTemplate.ActionType determines if bridge crosses
2. Only StartChallenge crosses to tactical layer
3. Instant and Navigate stay in strategic layer
4. One-way: Strategic spawns tactical, tactical returns outcome to strategic
5. SituationCards extracted when challenge spawns, NOT before

---

## PROCEDURAL SCENE GENERATION

### Authoring Flow: JSON → Template → Runtime

**Step 1: JSON Authoring**
Author writes SceneTemplate JSON with minimal specification:
```json
{
  "id": "scene_template_id",
  "sceneArchetypeId": "inn_lodging",  // References archetype catalogue
  "tier": 2,
  "placementFilter": { /* entity selection criteria */ }
}
```

**Step 2: Parse-Time Generation**
Parser calls `SceneGenerationFacade.GenerateSceneFromArchetype()`:
- Resolves placement entities (NPC, Location) from placementFilter
- Builds GenerationContext from entity properties (NPCDemeanor, Quality, etc.)
- Calls SceneArchetypeCatalog with context
- Receives complete SceneArchetypeDefinition (situations + spawn rules + dependent resources)
- Creates SceneTemplate with generated List<SituationTemplate>
- Stores in GameWorld.SceneTemplates

**Step 3: Spawn-Time Instantiation**
When scene spawns, SceneInstantiator:
- Creates Scene from SceneTemplate
- Iterates Template.SituationTemplates
- Instantiates each Situation and embeds in Scene.Situations list
- Resolves markers ("generated:location_id" → actual IDs) via MarkerResolutionMap
- Generates AI narrative if Description null + NarrativeHints present
- Replaces placeholders ({npcName}, {locationName}) with concrete values
- Sets Scene.CurrentSituation to first situation
- **Actions NOT created yet** (deferred until query time)

**Step 4: Query-Time Action Instantiation**
When player enters context, SceneFacade:
- Checks Situation.InstantiationState == Deferred
- Creates LocationActions/NPCActions/PathCards from ChoiceTemplates
- Sets InstantiationState = Instantiated
- Adds to GameWorld collections

### Two-Tier Archetype Composition System

**Tier 1: SituationArchetypeCatalog (Base Generation)**

Generates single-situation mechanical patterns:
```csharp
Input: archetype ID ("service_negotiation"), tier, GenerationContext
Process:
  - Retrieve archetype definition (base costs, stat thresholds)
  - Scale values by context (NPCDemeanor, Quality, PowerDynamic)
  - Generate List<ChoiceTemplate> with PathType assignments
  - RewardTemplate = EMPTY (scene will enrich)
Output: List<ChoiceTemplate> with mechanical structure only
```

**Tier 2: SceneArchetypeCatalog (Composition + Enrichment)**

Composes multiple situations into complete scenes:
```csharp
Input: scene archetype ID ("inn_lodging"), tier, GenerationContext
Process:
  FOR EACH situation in scene:
    - Call SituationArchetypeCatalog.GenerateChoiceTemplates()
    - Receive base choices with PathType
    - Switch on PathType to enrich rewards:
      * InstantSuccess → Add immediate rewards (LocationsToUnlock, ItemIds)
      * Challenge → Add OnSuccessReward (conditional rewards)
      * Fallback → Pass through unchanged
    - Create SituationTemplate with enriched choices
  - Define SituationSpawnRules (Pattern + Transitions)
  - Declare DependentResources (locations/items to generate)
Output: SceneArchetypeDefinition (situations + rules + resources)
```

**Why Two Tiers:**
- **Reusability:** Same situation archetype used by multiple scene archetypes
- **Separation:** Situations define HOW (mechanical paths), Scenes define WHAT (specific resources)
- **Modularity:** Change negotiation mechanics once, affects all services

**Enrichment Pattern:**
Situation catalogue returns choices with NO scene-specific rewards. Scene catalogue adds rewards based on PathType routing. This enables situation reuse across different scene contexts.

### Context-Aware Scaling Mechanism

**GenerationContext Flow:**
```
JSON entities → Parser resolves IDs → Loads NPC/Location from GameWorld
→ Extracts categorical properties (NPCDemeanor, Quality, PowerDynamic, etc.)
→ Builds GenerationContext struct
→ Passes to catalogue methods
```

**Scaling Application:**
Catalogues apply universal formulas to base archetype values:
```csharp
// Base archetype defines: StatThreshold = 5, CoinCost = 5
// Context has: NPCDemeanor.Friendly, Quality.Premium

scaledStatThreshold = context.NpcDemeanor switch {
  Friendly => (int)(5 * 0.6),  // = 3 (easier)
  Neutral => 5,                 // = 5 (baseline)
  Hostile => (int)(5 * 1.4)     // = 7 (harder)
};

scaledCoinCost = context.Quality switch {
  Basic => (int)(5 * 0.6),     // = 3 (cheap)
  Standard => 5,                // = 5 (baseline)
  Premium => (int)(5 * 1.6),   // = 8 (expensive)
  Luxury => (int)(5 * 2.4)      // = 12 (very expensive)
};
```

**Result:** Same archetype + different entity properties = contextually appropriate difficulty. AI authors entities with categorical properties, system handles numeric balance.

### Marker Resolution for Self-Contained Scenes

Scenes with dependent resources use marker syntax in templates:
```
SituationTemplate.RequiredLocationId = "generated:private_room"
ChoiceReward.LocationsToUnlock = ["generated:private_room"]
```

**Resolution Flow:**
1. Parse time: DependentLocationSpec declares id = "private_room", creates actual Location
2. Spawn time: Build MarkerResolutionMap: {"generated:private_room" → "location_actual_guid"}
3. Situation instantiation: Resolve markers via map
4. Store resolved ID in Situation.ResolvedRequiredLocationId
5. Runtime: Use resolved IDs (no marker syntax remains)

**Purpose:** Templates reference resources that don't exist until scene spawns. Markers enable static template definitions with dynamic resource binding.

### AI Narrative Generation Integration

Archetype-generated situations support AI narrative:
```csharp
SituationTemplate {
  NarrativeTemplate = null,  // Signals: generate from context
  NarrativeHints = new NarrativeHints {
    Tone = "transactional",
    Theme = "negotiation",
    Context = "securing_lodging"
  }
}
```

**Generation Flow:**
1. SceneInstantiator detects: Situation.Description == null + NarrativeHints != null
2. Builds ScenePromptContext from entities (NPC personality, location atmosphere)
3. Calls NarrativeService.GenerateSituationNarrative(context, hints)
4. AI generates narrative appropriate to entity context and hints
5. Sets Situation.Description to generated text
6. Replaces placeholders in generated narrative

**Result:** Archetype defines mechanical structure + narrative hints, AI generates concrete narrative from entity context. Same archetype produces contextually appropriate text.

---

## RUNTIME EXECUTION FLOW

### Context Activation & Auto-Activation

Situations activate when player location/NPC context matches situation requirements.

**Activation Check (SceneFacade):**
```csharp
1. Player enters location OR interacts with NPC
2. SceneFacade.GetActionsAtLocation(locationId, npcId) called
3. For each active Scene:
   - Check Scene.ShouldActivateAtContext(locationId, npcId)
   - Scene checks: CurrentSituation matches RequiredLocationId/RequiredNpcId
   - If match: Activate situation
```

**Activation Requirements:**
- **Location + NPC:** Both must match (service negotiation, NPC conversation)
- **Location Only:** Location matches, NPC null or ignored (private room rest, solo investigation)
- **NPC Only:** NPC matches, location optional (traveling merchant, roaming character)

**Auto-Activation Flow:**
```
Scene completes Situation 1
→ Scene.AdvanceToNextSituation() returns ContinueInScene
→ Scene.CurrentSituation = Situation 2
→ Player enters Situation 2's required location
→ SceneFacade detects context match
→ Situation 2 auto-activates
→ Choices appear immediately (no additional player action needed)
```

**Purpose:** Seamless multi-situation progression. Player completes negotiation, enters unlocked room, next situation's choices immediately available. No artificial navigation steps.

### Action Instantiation (Query-Time Creation)

Actions created on-demand when situation activates to reduce memory footprint.

**InstantiationState Transition:**
```csharp
// Initial state at situation creation:
Situation.InstantiationState = InstantiationState.Deferred
// NO actions in GameWorld.LocationActions/NPCActions/PathCards yet

// Query-time instantiation (SceneFacade.GetActionsAtLocation):
if (situation.InstantiationState == InstantiationState.Deferred)
{
    foreach (ChoiceTemplate template in situation.Template.ChoiceTemplates)
    {
        LocationAction action = new LocationAction {
            ChoiceTemplate = template,  // Reference to template
            SituationId = situation.Id
        };
        _gameWorld.LocationActions.Add(action);
    }
    situation.InstantiationState = InstantiationState.Instantiated;
}
```

**Why Query-Time:**
- **Memory Efficiency:** Actions only exist when contextually relevant
- **Lazy Evaluation:** Thousands of template choices, only instantiate what player can access
- **Clean Lifecycle:** Actions deleted when situation completes, recreated fresh if situation re-activates

**Action Properties:**
- `ChoiceTemplate`: Reference to template (requirements, costs, rewards)
- `SituationId`: Parent situation for cleanup
- Ephemeral: Exists only while situation active, deleted on completion

### Choice Execution Routing

ChoiceTemplate.PathType determines execution flow when player selects choice.

**Routing Logic (GameFacade.ExecuteChoice):**
```csharp
switch (choice.PathType)
{
    case ChoicePathType.InstantSuccess:
        // Evaluate requirements immediately
        // Apply costs immediately
        // Apply RewardTemplate immediately
        // Mark situation complete
        → Advance to next situation

    case ChoicePathType.Challenge:
        // Evaluate requirements immediately
        // Apply costs immediately
        // Store OnSuccessReward/OnFailureReward in PendingContext
        // Navigate to tactical screen (Social/Mental/Physical)
        → Wait for challenge completion
        → Apply OnSuccessReward or OnFailureReward based on outcome
        → Advance to next situation

    case ChoicePathType.Fallback:
        // NO requirements (always available)
        // NO costs (free forward progress)
        // Minimal or no rewards
        // Mark situation complete
        → Advance to next situation (may exit scene)
}
```

**ActionType Bridge (within PathType):**
```csharp
if (choice.ActionType == ChoiceActionType.Navigate)
{
    // Apply rewards first
    // Then move player to destination
    // Scene may continue if next situation at new location
}
else if (choice.ActionType == ChoiceActionType.StartChallenge)
{
    // Cross strategic-tactical bridge
    // Store CompletionReward in PendingContext
    // Navigate to challenge screen
}
else // Instant
{
    // Apply rewards, stay in scene
}
```

**Execution Order:**
1. Check requirements → Lock if failed, continue if passed
2. Apply costs → Deduct resources
3. Route by PathType/ActionType → Determine next step
4. Apply rewards → Immediate (InstantSuccess) or Conditional (Challenge)
5. Update situation state → Mark complete
6. Evaluate transitions → Determine next situation

### Scene Progression & Transition Evaluation

Scene.AdvanceToNextSituation() evaluates transition rules to determine next situation.

**Transition Evaluation:**
```csharp
1. Get completed situation
2. Lookup transition from SituationSpawnRules.Transitions
3. Evaluate Transition.Condition:
   - Always: Unconditional progression
   - OnChoice: Check LastChoiceId matches SpecificChoiceId
   - OnSuccess: Check LastChallengeSucceeded == true
   - OnFailure: Check LastChallengeSucceeded == false
4. If match found:
   - Set Scene.CurrentSituation = destination situation
   - Return SceneRoutingDecision (ContinueInScene or ExitToWorld)
5. If no match:
   - Scene complete (no more situations)
   - Return SceneRoutingDecision.SceneComplete
```

**SceneRoutingDecision:**
- **ContinueInScene:** Next situation shares same location/NPC context → Auto-activate immediately
- **ExitToWorld:** Next situation requires different context → Player must navigate
- **SceneComplete:** No more situations → Scene ends

**Context Comparison:**
```csharp
// Scene.CompareContexts() determines routing:
Current context: LocationId, NpcId
Next context: Situation.RequiredLocationId, Situation.RequiredNpcId

if (Same context):
    return ContinueInScene  // Seamless cascade
else:
    return ExitToWorld      // Player must travel/find NPC
```

**Progression State Machine:**
```
Situation 1 Active
→ Player selects choice
→ Choice executes
→ AdvanceToNextSituation() called
→ Evaluate transitions
→ Update CurrentSituation
→ Compare contexts
→ ContinueInScene: Situation 2 auto-activates
→ ExitToWorld: Await player navigation
→ SceneComplete: Scene ends
```

### Dependent Resource Lifecycle

Complete flow from declaration to removal for scene-generated resources.

**Phase 1: Declaration (Parse Time)**
```csharp
// SceneArchetypeCatalog declares resources:
SceneArchetypeDefinition {
    DependentLocations = [
        new DependentLocationSpec {
            Id = "private_room",
            NameTemplate = "{npcName}'s Private Room",
            Properties = [LocationProperty.Safe, LocationProperty.Restful]
        }
    ],
    DependentItems = [
        new DependentItemSpec {
            Id = "room_key",
            NameTemplate = "Key to {locationName}",
            ItemType = ItemType.Key
        }
    ]
}
```

**Phase 2: Creation (Parse Time)**
```csharp
// SceneTemplateParser creates actual entities:
foreach (DependentLocationSpec spec in archetypeDef.DependentLocations)
{
    Location location = new Location {
        Id = GenerateGuid(),  // Actual GUID
        Name = spec.NameTemplate,  // Placeholders remain
        IsLocked = true,  // Starts locked
        Properties = spec.Properties
    };
    _gameWorld.Locations.Add(location);

    // Store mapping: "private_room" → actual GUID
    dependentLocationIds.Add(spec.Id, location.Id);
}
```

**Phase 3: Marker Resolution (Spawn Time)**
```csharp
// Scene spawns, MarkerResolutionMap built:
Scene.MarkerResolutionMap = {
    "generated:private_room" → "location_guid_12345"
}

// Situations resolve markers:
SituationTemplate.RequiredLocationId = "generated:private_room"
→ Situation.ResolvedRequiredLocationId = "location_guid_12345"

ChoiceReward.LocationsToUnlock = ["generated:private_room"]
→ Resolved at execution: Unlock location_guid_12345
```

**Phase 4: Grant (Runtime - Choice Execution)**
```csharp
// Choice rewards grant access:
RewardTemplate.LocationsToUnlock → Set Location.IsLocked = false
RewardTemplate.ItemIds → Add Item to Player.Inventory
```

**Phase 5: Usage (Runtime - Situation Activation)**
```csharp
// Situation requires access:
Situation.RequiredLocationId = "location_guid_12345"
→ Player can only activate if Location.IsLocked = false
→ Auto-activates when player enters
```

**Phase 6: Removal (Runtime - Cleanup)**
```csharp
// Departure choice cleans up:
RewardTemplate.ItemsToRemove → Remove from Player.Inventory
RewardTemplate.LocationsToLock → Set Location.IsLocked = true
```

**Lifecycle Complete:**
```
Declare → Create → Resolve → Grant → Use → Remove
Parse    Parse    Spawn    Choice  Situation  Choice
```

**Purpose:** Resources exist throughout but access controlled. Player can't re-enter locked room after departure. Key removed from inventory. Location persists (for potential future scenes) but inaccessible.

---

## THREE-TIER TIMING MODEL

### Why Three Tiers Exist

The three-tier timing model enables **lazy instantiation**, drastically reducing memory usage and preventing GameWorld from bloating with thousands of inaccessible actions.

### Tier 1: Templates (Parse Time)

**When**: Game startup, JSON parsing
**What**: Immutable archetypes defining reusable patterns

```
SceneTemplate
  └─ List<SituationTemplate>
       └─ List<ChoiceTemplate>
```

**Properties**:
- Created once from JSON at parse time
- Stored in GameWorld.SceneTemplates
- Never modified during gameplay
- Design language for content authors

### Tier 2: Scenes/Situations (Spawn Time)

**When**: Scene spawns from Obligation or SceneSpawnReward
**What**: Runtime instances with lifecycle and state

```
Scene (spawned from SceneTemplate)
  └─ List<Situation> (created and embedded)
       └─ Template reference (ChoiceTemplates NOT instantiated)
```

**Properties**:
- Scene created with embedded Situations
- Situation.Template reference stored (points back to SituationTemplate)
- Situation.InstantiationState = Deferred
- **NO actions created in GameWorld collections yet**

**Why Deferred:**
Situations may require specific context (Location X + NPC Y) that player hasn't reached. Creating actions prematurely bloats GameWorld with thousands of buttons player can't see.

**Example:**
```
Scene "Elena's Favor" spawns with 5 Situations:
  - Situation 1: Requires Location "Market Square" + NPC "Elena"
  - Situation 2: Requires Location "Elena's Workshop"
  - Situation 3: Requires Location "Market Square" (different time)
  - Situation 4: Requires Location "Town Hall" + NPC "Elena"
  - Situation 5: Requires Location "Elena's Home"

At spawn time: 0 actions created in GameWorld
If all actions created immediately: 15+ action buttons in GameWorld (3 choices × 5 situations)
Most are inaccessible until player navigates to correct context
```

### Tier 3: Actions (Query Time)

**When**: Player enters matching context (Location + optional NPC)
**What**: Ephemeral UI projections of ChoiceTemplates

```
Player at Location "Market Square" with NPC "Elena" present
  ↓
SceneFacade queries: Which Situations match this context?
  ↓
Finds Situation 1 (InstantiationState = Deferred)
  ↓
Instantiates 3 NPCActions from Situation.Template.ChoiceTemplates
  ↓
Adds to GameWorld.NPCActions
  ↓
UI displays 3 action buttons
```

**Ephemeral Lifecycle:**
1. **Creation**: Actions instantiated when player enters context
2. **Display**: Actions rendered in UI as clickable cards/buttons
3. **Execution**: Player selects action, GameFacade executes
4. **Deletion**: When Situation completes, actions deleted from GameWorld
5. **Regeneration**: If player returns to same context, actions recreated from Template

**Why Ephemeral:**
- Template is single source of truth (HIGHLANDER)
- Actions are view projections generated on demand
- Prevents duplicate actions accumulating
- No orphaned actions from completed situations

### InstantiationState Tracking

```csharp
public enum InstantiationState
{
    Deferred,      // Situation exists, NO actions in GameWorld
    Instantiated   // Player entered context, actions materialized
}
```

**State Transitions:**
```
Situation spawned → InstantiationState = Deferred
Player enters matching context → SceneFacade instantiates actions → InstantiationState = Instantiated
Situation completes → Actions deleted, Situation deleted or marked complete
```

### Complete Example Flow

**Parse Time (Tier 1):**
```
JSON defines SceneTemplate "secure_lodging_tutorial"
  ├─ SituationTemplate "obtain_key"
  │   └─ 3 ChoiceTemplates (pay coins, negotiate, steal)
  ├─ SituationTemplate "access_room"
  │   └─ 2 ChoiceTemplates (unlock door, break window)
  └─ SituationTemplate "claim_space"
      └─ 2 ChoiceTemplates (rest immediately, inspect first)

Stored in GameWorld.SceneTemplates
```

**Spawn Time (Tier 2):**
```
Obligation spawns Scene from template
  ├─ Scene.Situations created (3 Situations embedded)
  │   ├─ Situation 1: RequiredLocationId = "inn_common_room"
  │   │   └─ InstantiationState = Deferred
  │   ├─ Situation 2: RequiredLocationId = "inn_upper_floor"
  │   │   └─ InstantiationState = Deferred
  │   └─ Situation 3: RequiredLocationId = "generated:private_room"
  │       └─ InstantiationState = Deferred
  └─ Scene.CurrentSituation = Situation 1

GameWorld.Scenes += 1 scene
GameWorld.LocationActions += 0 actions (nothing instantiated yet)
```

**Query Time (Tier 3):**
```
Player navigates to "inn_common_room"
  ↓
LocationContent.razor calls SceneFacade.GetActionsForLocation("inn_common_room")
  ↓
SceneFacade finds Situation 1 (matches location, InstantiationState = Deferred)
  ↓
SceneFacade instantiates 3 LocationActions from Situation.Template.ChoiceTemplates:
  - LocationAction "Pay 20 coins for key"
  - LocationAction "Negotiate for better rate" (StartChallenge)
  - LocationAction "Attempt to steal key" (StartChallenge)
  ↓
Situation 1.InstantiationState = Instantiated
GameWorld.LocationActions += 3 actions
  ↓
UI renders 3 action cards

Player selects "Negotiate for better rate"
  ↓
GameFacade executes (spawns Social challenge if StartChallenge)
  ↓
Challenge succeeds, Situation 1 completes
  ↓
Delete 3 LocationActions from GameWorld.LocationActions
Scene.AdvanceToNextSituation() → CurrentSituation = Situation 2
Situation 1.InstantiationState remains Instantiated (but situation complete)
  ↓
GameWorld.LocationActions -= 3 actions (cleaned up)

Player navigates to "inn_upper_floor"
  ↓
SceneFacade finds Situation 2 (matches location, InstantiationState = Deferred)
  ↓
Instantiates 2 LocationActions from Situation 2.Template.ChoiceTemplates
  ↓
GameWorld.LocationActions += 2 actions
UI renders 2 action cards
```

### Benefits

**Memory Efficiency:**
Only actions for current context exist in GameWorld. Scene with 5 Situations × 3 Choices each = 15 potential actions, but only 3 exist at any time.

**Performance:**
UI queries GameWorld.LocationActions (3 items) instead of scanning all Situations in all Scenes (hundreds).

**Single Source of Truth:**
ChoiceTemplate defined once in SituationTemplate. Runtime actions reference Template, never duplicate data.

**Clean Lifecycle:**
Actions created when needed, deleted when done. No orphaned references, no stale data.

**Easy Debugging:**
Check InstantiationState to see if actions should exist. If Deferred, player hasn't entered context yet.

### Forbidden Patterns

```csharp
// ❌ WRONG - Creating actions at spawn time (Tier 2)
Scene spawned → Immediately instantiate ALL actions for ALL Situations
  → GameWorld bloats with inaccessible actions
  → Memory waste, performance penalty

// ❌ WRONG - Storing actions permanently
Situation.Actions = new List<LocationAction>()  // NO! Actions are ephemeral!
  → Violates lazy instantiation
  → Creates duplicate action collections

// ❌ WRONG - Instantiating without context check
for (Situation in Scene.Situations) {
    Instantiate actions  // NO! Check context first!
}
```

### Correct Patterns

```csharp
// ✅ CORRECT - Query time instantiation with context check
SceneFacade.GetActionsForLocation(locationId):
    For each Scene in GameWorld.Scenes:
        For each Situation in Scene.Situations:
            If Situation.RequiredLocationId == locationId:
                If Situation.InstantiationState == Deferred:
                    Instantiate actions from Template.ChoiceTemplates
                    Set InstantiationState = Instantiated
                Return actions

// ✅ CORRECT - Cleanup after execution
GameFacade.ExecuteLocationAction():
    Execute action
    If Situation completes:
        Delete all actions for that Situation
        Scene.AdvanceToNextSituation()
```

---

## SYSTEM OVERVIEW

Wayfarer is a **low-fantasy tactical RPG** with **three parallel challenge systems** (Social, Mental, Physical) built with **C# ASP.NET Core** and **Blazor Server**. The architecture follows **clean architecture principles** with strict **dependency inversion** and **single responsibility** patterns.

### Core Design Philosophy
- **GameWorld as Single Source of Truth**: All game state flows through GameWorld with zero external dependencies
- **Three Parallel Tactical Systems**: Social (conversations), Mental (investigations), Physical (obstacles) with equivalent depth
- **Strategic-Tactical Bridge**: Scenes spawn Situations with Choices, ChoiceActionType.StartChallenge routes to tactical systems
- **Static Content Loading**: JSON content parsed once at startup without DI dependencies
- **Facade Pattern**: Business logic coordinated through specialized facades
- **Authoritative UI Pattern**: GameScreen owns all UI state, children communicate upward
- **No Shared Mutable State**: Services provide operations, not state storage

---

## CONTENT PIPELINE ARCHITECTURE

### 1. JSON Content Structure

### 2. Static Parser Layer

**Location**: `src/Content/*Parser.cs`

**Parser Responsibilities**:
```csharp
SocialCardParser     → SocialCardDTO → SocialCard
MentalCardParser     → MentalCardDTO → MentalCard
PhysicalCardParser   → PhysicalCardDTO → PhysicalCard
NPCParser            → NPCDTO → NPC
VenueParser          → VenueDTO → Venue
LocationParser       → LocationDTO → Location
SceneParser          → SceneDTO → Scene
SituationParser      → SituationDTO → Situation
```

**CRITICAL PARSER PRINCIPLES**:
- **PARSE AT THE BOUNDARY**: JSON artifacts NEVER pollute domain layer
- **NO JsonElement PASSTHROUGH**: Parsers MUST convert to strongly-typed objects
- **NO Dictionary<string, object>**: Use proper typed properties on domain models
- **JSON FIELD NAMES MUST MATCH C# PROPERTIES**: No JsonPropertyName attributes to hide mismatches
- **STATELESS**: Parsers are static classes with no side effects
- **SINGLE PASS**: Each parser converts DTO to domain entity in one operation
- **CATEGORICAL → MECHANICAL TRANSLATION**: Parsers translate categorical JSON properties to absolute mechanical values through catalogues (see Categorical Properties Pattern below)

### 3. Categorical Properties → Dynamic Scaling Pattern (AI Content Generation)

**CRITICAL ARCHITECTURE DECISION: Why JSON uses categorical properties instead of absolute values**

**The Problem: AI-Generated Runtime Content**

AI-generated content (procedural generation, LLM-created entities, user-generated content) CANNOT specify absolute mechanical values because AI doesn't know:
- Current player progression level (Level 1 vs Level 10)
- Existing game balance (what items/cards/challenges already exist)
- Global difficulty curve (early game vs late game tuning)
- Economy state (coin inflation, resource scarcity)

**The Solution: Relative Categorical Properties + Dynamic Scaling Catalogues**

```
AI generates JSON with categorical properties (relative descriptions)
    ↓
Parser reads current game state (player level, difficulty mode, etc.)
    ↓
Catalogue translates categorical → absolute values WITH SCALING
    ↓
Domain entity receives scaled mechanical values
```

**Example: Equipment Durability**

```csharp
// JSON (AI-generated or hand-authored): Categorical property
{
  "id": "worn_rope",
  "name": "Worn Climbing Rope",
  "durability": "Fragile"    // ← RELATIVE category, not absolute value
}

// Parser translates using catalogue + game state
DurabilityType durability = ParseEnum(dto.Durability);  // Fragile
int playerLevel = gameWorld.Player.Level;                // Current: 3
DifficultyMode difficulty = gameWorld.CurrentDifficulty; // Normal

(int uses, int repairCost) = EquipmentDurabilityCatalog.GetDurabilityValues(
    durability, playerLevel, difficulty);

// Catalogue scales based on game state:
// Level 1:  Fragile → 2 uses, 10 coins
// Level 5:  Fragile → 4 uses, 25 coins  (scaled up for progression)
// Level 10: Fragile → 6 uses, 40 coins  (continues scaling)

// CRITICAL: Fragile ALWAYS weaker than Sturdy (relative consistency maintained)
```

**Example: Card Effects**

```csharp
// JSON: Categorical move type
{
  "conversationalMove": "Remark",
  "boundStat": "Rapport",
  "depth": 2
}

// Parser translates with scaling
CardEffectFormula effect = SocialCardEffectCatalog.GetEffectFromCategoricalProperties(
    ConversationalMove.Remark,
    PlayerStatType.Rapport,
    depth: 2,
    cardId,
    playerLevel);  // ← Scaling factor

// Early game (Level 1): Remark/Rapport/Depth2 → +4 Understanding
// Late game (Level 5): Remark/Rapport/Depth2 → +6 Understanding (scaled)
```

**Why This Architecture Exists:**

1. **AI Content Generation**: AI describes entities relatively ("Fragile rope", "Cunning NPC") without needing to know absolute game values
2. **Dynamic Difficulty Scaling**: Same content scales automatically as player progresses
3. **Consistent Relative Balance**: "Fragile" ALWAYS weaker than "Sturdy" regardless of scaling factors
4. **Future-Proof**: Supports procedural generation, LLM content, user mods, runtime content
5. **Centralized Balance**: Change ONE catalogue formula → ALL entities of that category scale consistently

**Catalogue Implementation Requirements:**

```csharp
// Location: src/Content/Catalogues/*Catalog.cs
public static class EquipmentDurabilityCatalog
{
    // Context-aware scaling function
    public static (int exhaustAfterUses, int repairCost) GetDurabilityValues(
        DurabilityType durability,
        int playerLevel,           // ← Scaling context
        DifficultyMode difficulty) // ← Scaling context
    {
        // Base values for each category
        int baseUses = durability switch
        {
            DurabilityType.Fragile => 2,
            DurabilityType.Sturdy => 5,
            DurabilityType.Durable => 8,
            _ => throw new InvalidOperationException($"Unknown durability: {durability}")
        };

        int baseRepair = durability switch
        {
            DurabilityType.Fragile => 10,
            DurabilityType.Sturdy => 25,
            DurabilityType.Durable => 40,
            _ => throw new InvalidOperationException($"Unknown durability: {durability}")
        };

        // Dynamic scaling based on game state
        float levelScaling = 1.0f + (playerLevel * 0.2f); // +20% per level
        float difficultyScaling = difficulty switch
        {
            DifficultyMode.Easy => 1.2f,
            DifficultyMode.Normal => 1.0f,
            DifficultyMode.Hard => 0.8f,
            _ => 1.0f
        };

        int scaledUses = (int)(baseUses * levelScaling * difficultyScaling);
        int scaledRepair = (int)(baseRepair * levelScaling * difficultyScaling);

        return (scaledUses, scaledRepair);
    }
}
```

**Existing Catalogues:**
- `SocialCardEffectCatalog` (src/Content/Catalogs/SocialCardEffectCatalog.cs)
- `MentalCardEffectCatalog` (src/Content/Catalogs/MentalCardEffectCatalog.cs)
- `PhysicalCardEffectCatalog` (src/Content/Catalogs/PhysicalCardEffectCatalog.cs)
- `EquipmentDurabilityCatalog` (src/Content/Catalogs/EquipmentDurabilityCatalog.cs)

**When to Use Categorical Properties:**

Ask these questions for ANY numeric property in a DTO:
1. "Could AI generate this entity at runtime without knowing global game state?"
2. "Should this value scale with player progression or difficulty?"
3. "Is this RELATIVE (compared to similar entities) rather than ABSOLUTE?"

If YES → Create categorical enum + scaling catalogue
If NO → Consider if it's truly a design-time constant (rare - most values should scale)

**Anti-Pattern: Hardcoded Absolute Values in JSON**

```json
// ❌ WRONG - Absolute values break AI generation and scaling
{
  "exhaustAfterUses": 2,
  "repairCost": 10,
  "understanding": 4,
  "momentum": 2
}

// ✅ CORRECT - Categorical properties enable AI + scaling
{
  "durability": "Fragile",
  "conversationalMove": "Remark",
  "depth": 2
}
```

### 4. Content Loading Orchestration

**Location**: `src/Content/PackageLoader.cs` & `GameWorldInitializer.cs`

**Loading Sequence**:
```
Startup → GameWorldInitializer.CreateGameWorld()
       → PackageLoader.LoadContent()
       → Static Parsers (for each content type)
       → Domain Objects → GameWorld Population
```

**Initialization Architecture**:
- `GameWorldInitializer` is **STATIC** - no DI dependencies
- Creates GameWorld **BEFORE** service registration completes
- Prevents circular dependencies during ServerPrerendered mode
- Content loaded once at startup, never reloaded

---

## ACTION EXECUTION PIPELINE

**Location**: `src/Content/Parsers/*ActionParser.cs`, `src/GameState/*Action.cs`, `src/Services/GameFacade.cs`

### 1. Action System Overview

**⚠️ CRITICAL ARCHITECTURE CHANGE: Actions are NO LONGER defined in JSON**

**Actions are PROCEDURALLY GENERATED from categorical location properties at parse time.**

**Two Action Types**:
- **PlayerActions**: Universal actions available everywhere (e.g., "Check Belongings", "Wait", "Sleep Outside")
- **LocationActions**: Property-driven actions generated from LocationPropertyType enums

**Architecture Goals**:
1. **Catalogue Pattern**: Actions generated from categorical properties (parse-time ONLY)
2. **No JSON Bloat**: Locations define properties, catalogues generate complete actions
3. **Strong Typing**: All action types, costs, rewards strongly-typed
4. **No Dictionary Disease**: Zero string-based matching or dictionary lookups

### 2. Complete System Documentation

**⚠️ FULL DOCUMENTATION**: See [LOCATION_PROPERTY_ACTION_SYSTEM.md](./LOCATION_PROPERTY_ACTION_SYSTEM.md) for comprehensive documentation of:
- Location Property → Action Generation pattern
- Catalogue implementation details
- Parse-time integration
- Runtime querying and filtering
- Property → Action mapping table
- Adding new action types
- Testing and debugging

### 3. Quick Reference

**Location Property → Action Mapping**:

| Property | Generated Action | Costs | Rewards |
|----------|-----------------|-------|---------|
| Crossroads | Travel | None | None |
| Commercial | Work | Time + Stamina | 8 Coins |
| Restful | Rest | Time | +1 Health, +1 Stamina |
| Lodging | Secure Room | 10 Coins | Full Recovery |

**Universal Player Actions**:
- Check Belongings
- Wait
- Sleep Outside

### 5. GameFacade Orchestration (Single Dispatch Point)

**Location**: `src/Services/GameFacade.cs`

**GameFacade Methods** - Single point of entry for action execution:
```csharp
// Execute PlayerAction by enum type
public async Task ExecutePlayerAction(PlayerActionType actionType)
{
    switch (actionType)
    {
        case PlayerActionType.CheckBelongings:
            // Navigate to equipment screen
            break;

        case PlayerActionType.Wait:
            // Delegate to ResourceFacade for time/hunger progression
            await _resourceFacade.ExecuteWait();
            break;

        default:
            throw new InvalidOperationException($"Unknown PlayerActionType: {actionType}");
    }
}

// Execute LocationAction by enum type
public async Task ExecuteLocationAction(LocationActionType actionType, string locationId)
{
    switch (actionType)
    {
        case LocationActionType.Travel:
            // Navigate to travel screen
            break;

        case LocationActionType.Rest:
            // Delegate to ResourceFacade for recovery
            await _resourceFacade.ExecuteRest();
            break;

        case LocationActionType.Work:
            // Delegate to ResourceFacade for work rewards
            await _resourceFacade.PerformWork();
            break;

        case LocationActionType.Investigate:
            // Delegate to LocationFacade for familiarity gain
            await _locationFacade.InvestigateLocation(locationId);
            break;

        default:
            throw new InvalidOperationException($"Unknown LocationActionType: {actionType}");
    }
}
```

**Why GameFacade**: Follows existing patterns like `ExecuteListen()`, `PerformWork()`, `TravelToDestinationAsync()`. GameFacade orchestrates specialized facades while keeping them decoupled.

### 7. Specialized Facade Implementation

**Location**: `src/Subsystems/Resource/ResourceFacade.cs`

**ResourceFacade.ExecuteRest()** - Example action handler:
```csharp
public async Task ExecuteRest()
{
    Player player = _gameWorld.GetPlayer();

    // Advance 1 time segment
    await _timeFacade.AdvanceSegments(1);

    // Hunger increases by +5 per segment (automatic via time progression)
    // No need to manually modify hunger here

    // Resource recovery
    player.Health = Math.Min(player.Health + 1, player.MaxHealth);      // +1 health (16.7% of 6-point max)
    player.Stamina = Math.Min(player.Stamina + 1, player.MaxStamina);   // +1 stamina (16.7% of 6-point max)
}
```

**ResourceFacade.ExecuteWait()** - Example action handler:
```csharp
public async Task ExecuteWait()
{
    // Advance 1 time segment
    await _timeFacade.AdvanceSegments(1);

    // Hunger increases by +5 per segment (automatic via time progression)
    // No resource recovery - just passing time
}
```

### 8. UI Layer Integration

**Location**: `src/Pages/Components/LocationContent.razor.cs`

**BEFORE** (String Matching - Anti-Pattern):
```csharp
// WRONG - scattered string matching
if (action.ActionType == "travel")
{
    // Handle travel inline
}
else if (action.ActionType == "rest")
{
    // Handle rest inline
}
```

**AFTER** (Enum Dispatch through GameFacade):
```csharp
// CORRECT - enum-based dispatch through GameFacade
private async Task HandleLocationAction(LocationAction action)
{
    await GameFacade.ExecuteLocationAction(action.ActionType, currentLocationId);
    await RefreshUI();
}

private async Task HandlePlayerAction(PlayerAction action)
{
    await GameFacade.ExecutePlayerAction(action.ActionType);
    await RefreshUI();
}
```

**Why This Works**: UI layer is dumb display that calls GameFacade with strongly-typed enums. GameFacade handles all dispatch logic. No string matching in UI layer.

### 9. Complete Vertical Slice Example: "Rest" Action

**Complete Flow from JSON to Execution**:

```
1. JSON DEFINITION (01_foundation.json)
   {
     "id": "rest",
     "name": "Rest",
     "actionType": "Rest",  // String in JSON
     "requiredProperties": ["rest", "restful"]
   }

2. PARSER VALIDATION (LocationActionParser.cs)
   - Reads JSON via PackageLoader
   - Validates "Rest" against LocationActionType enum
   - Throws InvalidDataException if "Rest" not in enum
   - Converts string "Rest" to LocationActionType.Rest enum
   - Converts property strings to LocationPropertyType enums

3. GAMEWORLD STORAGE
   - Parsed LocationAction stored in GameWorld.LocationActions
   - ActionType is LocationActionType.Rest (strongly typed)

4. UI DISPLAY (LocationContent.razor.cs)
   - Fetches available LocationActions for current location
   - Filters by property matching (location has "rest" or "restful")
   - Displays "Rest" card in UI

5. USER INTERACTION
   - Player clicks "Rest" card
   - UI calls: await HandleLocationAction(action)

6. GAMEFACADE DISPATCH (GameFacade.cs)
   - UI calls: await GameFacade.ExecuteLocationAction(LocationActionType.Rest, locationId)
   - GameFacade switches on action.ActionType enum
   - Case LocationActionType.Rest: delegates to ResourceFacade.ExecuteRest()

7. SPECIALIZED FACADE EXECUTION (ResourceFacade.cs)
   - ResourceFacade.ExecuteRest() executes game logic:
     - Advances 1 time segment via TimeFacade
     - Recovers +1 health
     - Recovers +1 stamina
     - Hunger increases +5 (automatic via time progression)

8. STATE UPDATE
   - Player resources updated in GameWorld
   - UI refreshes with new state
```

### 10. Critical Principles

**1. Enum Catalogues Prevent Runtime Errors**
- All valid action types defined in enums
- Parsers validate JSON against enums at startup
- Unknown action types crash with descriptive errors BEFORE game starts
- NO runtime string matching errors

**2. Single Dispatch Point in GameFacade**
- ALL action execution goes through GameFacade methods
- NO scattered string matching across multiple files
- GameFacade orchestrates specialized facades
- Follows existing patterns (ExecuteListen, PerformWork, etc.)

**3. Strong Typing Throughout Pipeline**
- JSON strings converted to enums by parsers
- Domain entities use enum types
- GameFacade switches on enums
- UI passes enums to GameFacade
- NO string comparisons at runtime

**4. Property-Based Matching for LocationActions**
- LocationActions use RequiredProperties/OptionalProperties/ExcludedProperties
- Locations tagged with LocationPropertyType enums
- Actions automatically filtered by property matching
- NO manual ID lists or hardcoded availability

**5. Specialized Facades Handle Domain Logic**
- GameFacade delegates to ResourceFacade, TimeFacade, LocationFacade, etc.
- Each facade encapsulates ONE business domain
- Facades remain decoupled (never call each other directly)
- GameFacade orchestrates cross-facade operations

**6. No Dictionary Disease**
- NO string-based actionType matching
- NO Dictionary<string, object> for actions
- NO runtime type checking or casting
- Strong typing enforced from JSON → Parser → Domain → UI

---

## GAMEWORLD STATE MANAGEMENT

**Location**: `src/GameState/GameWorld.cs`

### GameWorld Responsibilities

**State Collections**:
```csharp
// Core Entities
public List<NPC> NPCs { get; set; }
public List<Venue> Venues { get; set; }
public List<LocationEntry> Locations { get; set; }
private Player Player { get; set; }

// Three Parallel Tactical Systems - Card Templates
public List<SocialCard> SocialCards { get; set; }
public List<MentalCard> MentalCards { get; set; }
public List<PhysicalCard> PhysicalCards { get; set; }

// Three Parallel Tactical Systems - Challenge Decks
public List<SocialChallengeDeck> SocialChallengeDecks { get; }
public List<MentalChallengeDeck> MentalChallengeDecks { get; }
public List<PhysicalChallengeDeck> PhysicalChallengeDecks { get; }

// Strategic Layer - Scene System
public List<SceneTemplate> SceneTemplates { get; set; }
public List<Scene> Scenes { get; set; }

// Player Stats System
public List<PlayerStatDefinition> PlayerStatDefinitions { get; set; }
public StatProgression StatProgression { get; set; }
```

**State Operations**:
```csharp
GetPlayer() → Single player instance
GetPlayerResourceState() → Current player resources
GetLocation(string locationId) → Location by ID
GetAvailableStrangers() → NPCs available at venue/time
RefreshStrangersForTimeBlock() → Time-based NPC availability
ApplyInitialPlayerConfiguration() → Apply starting conditions from JSON
```

### CRITICAL GAMEWORLD PRINCIPLES

**1. Zero External Dependencies**
- GameWorld NEVER depends on services, managers, or external components
- All dependencies flow **INWARD** toward GameWorld, never outward
- GameWorld does **NOT** create any managers or services

**2. Single Source of Truth**
- ALL game state lives in GameWorld - no parallel state in services
- Services read/write to GameWorld but don't maintain their own copies
- NO SharedData dictionaries or TempData storage

**3. No Business Logic**
- GameWorld contains **STATE**, not **BEHAVIOR**
- Business logic belongs in services/facades, not GameWorld
- GameWorld provides data access, not game rules

---

## SERVICE & SUBSYSTEM LAYER

**Location**: `src/Services/GameFacade.cs` & `src/Subsystems/*/`

### Service Architecture Hierarchy

```
GameFacade (Pure Orchestrator)
├── THREE PARALLEL TACTICAL SYSTEMS
│   ├── SocialFacade (Social challenges - conversations with NPCs)
│   ├── MentalFacade (Mental challenges - investigations at locations)
│   └── PhysicalFacade (Physical challenges - obstacles at locations)
├── SUPPORTING SYSTEMS
│   ├── LocationFacade (Movement and spot management)
│   ├── ResourceFacade (Health, hunger, coins, stamina)
│   ├── TimeFacade (Time progression and segments)
│   ├── TravelFacade (Route management and travel)
│   ├── TokenFacade (Relationship tokens)
│   ├── NarrativeFacade (Messages and observations)
│   └── ExchangeFacade (NPC trading system)
└── SCENES & CONTENT
    ├── SceneInstantiator (Scene spawning from templates)
    ├── SpawnFacade (Scene spawn condition evaluation)
    ├── ContentGenerationFacade (Dynamic package creation)
    └── RewardApplicationService (Reward application after choices)
```

### Facade Responsibilities

**GameFacade** - Pure orchestrator for UI-Backend communication
- Delegates ALL business logic to specialized facades
- Coordinates cross-facade operations (e.g., completing goals triggers investigations)
- Handles UI-specific orchestration
- NO business logic - only coordination

**Three Parallel Tactical System Facades** - Equivalent depth challenge systems
- `SocialFacade`: Initiative/Momentum/Doubt/Cadence conversation mechanics with NPCs
- `MentalFacade`: Progress/Attention/Exposure/Leads investigation mechanics at locations
- `PhysicalFacade`: Breakthrough/Exertion/Danger/Aggression obstacle mechanics at locations
- Each follows same architectural pattern: Builder/Threshold/Session resources + Binary actions
- All three systems use unified 5-stat progression (Insight/Rapport/Authority/Diplomacy/Cunning)

**Supporting Facades** - Game systems that integrate with tactical challenges
- `ExchangeFacade`: Separate NPC trading system (instant exchanges, not conversations)
- `LocationFacade`: Movement validation, location properties, spot management
- `ResourceFacade`: Permanent resources (Health, Stamina, Hunger, Focus, Coins)
- Each facade encapsulates ONE business domain

### Subsystem Organization

**Location**: `src/Subsystems/[Domain]/`

```
THREE PARALLEL TACTICAL SYSTEMS (Equivalent Depth)
Social/         → Social challenges: Card mechanics, momentum, conversation sessions
Mental/         → Mental challenges: Investigation mechanics, leads, observation system
Physical/       → Physical challenges: Obstacle mechanics, aggression, combo execution

SUPPORTING SYSTEMS
Exchange/       → NPC trading, inventory validation
Location/       → Movement, spot properties, location actions
Resource/       → Health, hunger, coins, stamina, focus management
Time/           → Segment progression, time block transitions
Token/          → Relationship tokens, connection tracking
Travel/         → Route discovery, travel validation
Narrative/      → Message system, observation rewards

SCENE & SPAWN SYSTEMS
Spawn/          → Scene spawn condition evaluation, template instantiation
ProceduralContent/ → Procedural scene generation, AI content integration
Catalogues/     → Parse-time categorical property translation
```

---

## UI COMPONENT ARCHITECTURE

**Location**: `src/Pages/*.razor` & `src/Pages/Components/*.razor`

### Authoritative Page Pattern

**GameScreen.razor** - Single authoritative parent
- Owns ALL screen state and manages child components directly
- Provides outer structure (resources bar, headers, time display)
- Child components rendered INSIDE GameScreen's container
- Children call parent methods directly via CascadingValue

**Child Components** - Screen-specific content only
```
THREE PARALLEL TACTICAL SYSTEMS
ConversationContent.razor  → Social challenges (conversations with NPCs)
MentalContent.razor        → Mental challenges (investigations at locations)
PhysicalContent.razor      → Physical challenges (obstacles at locations)

SUPPORTING SCREENS
LocationContent.razor      → Location exploration UI
ExchangeContent.razor      → NPC trading interface
TravelContent.razor        → Route selection and travel

INVESTIGATION MODALS
InvestigationDiscoveryModal.razor  → Investigation discovery notifications
InvestigationActivationModal.razor → Investigation intro completion
InvestigationProgressModal.razor   → Phase completion notifications
InvestigationCompleteModal.razor   → Investigation completion rewards
```

### Component Communication Pattern

**Direct Parent-Child Communication**:
```csharp
// Child receives parent reference
<CascadingValue Value="@this">
  <LocationContent OnActionExecuted="RefreshUI" />
</CascadingValue>

// Child calls parent methods directly
GameScreen.StartConversation(npcId, requestId)
GameScreen.NavigateToQueue()
GameScreen.HandleTravelRoute(routeId)
```

**Context Objects for Complex State**:
```csharp
THREE PARALLEL TACTICAL SYSTEMS
SocialChallengeContext   → Complete conversation state (Social challenge)
MentalSession            → Investigation state (Mental challenge)
PhysicalSession          → Obstacle state (Physical challenge)

SUPPORTING CONTEXTS
ExchangeContext          → NPC trading session state
TravelDestinationViewModel → Route and destination display state
LocationScreenViewModel  → Location exploration state
```

### CRITICAL UI PRINCIPLES

**1. UI is Dumb Display Only**
- NO game logic in Razor components
- NO attention costs or availability logic in UI
- Backend determines ALL game mechanics through facades

**2. No Shared Mutable State**
- Services provide operations, NOT state storage
- NavigationCoordinator handles navigation ONLY, not data passing
- State lives in components, not services

**3. Screen Component Constraints**
- Screen components NEVER define their own game-container or headers
- GameScreen provides outer structure, children provide content only
- Screen components wrapped in semantic classes like 'conversation-content'

---

## DATA FLOW PATTERNS

### Complete Pipeline Flow

```
JSON Files (Content Definition)
    ↓ [Static Parsers]
Domain Models (Strongly Typed Objects)
    ↓ [GameWorldInitializer]
GameWorld (Single Source of Truth)
    ↓ [Service Facades]
Business Logic Operations
    ↓ [Context Objects]
UI Components (User Interface Display)
    ↓ [User Actions]
Service Facades (State Updates)
    ↓ [GameWorld Updates]
State Persistence
```

### Request/Response Flow

**User Action → UI Response**:
```
1. User clicks UI element
2. Component calls GameScreen method
3. GameScreen calls GameFacade method
4. GameFacade orchestrates subsystem facades
5. Facades execute business logic
6. Facades update GameWorld state
7. GameScreen refreshes UI with new state
```

### Context Creation Pattern

**Complex Operations Use Dedicated Contexts**:
```csharp
// Context created atomically BEFORE navigation
ConversationContext context = await GameFacade.CreateConversationContext(npcId, requestId);

// Context contains ALL data needed for operation
context.NpcInfo = npc data
context.LocationInfo = current location
context.PlayerResources = resource state
context.Session = conversation session

// Context passed as single parameter to child component
<ConversationContent Context="@context" />
```

---

## CRITICAL ARCHITECTURAL PRINCIPLES

### 1. Dependency Inversion
- **All dependencies flow INWARD toward GameWorld**
- GameWorld has zero external dependencies
- Services depend on GameWorld, not vice versa
- UI depends on services, services never depend on UI

### 2. Single Responsibility
- Each facade handles exactly ONE business domain
- GameWorld provides state access, not business logic
- UI components render state, don't contain game rules
- Parsers convert data formats, don't store state

### 3. Immutable Content Pipeline
- JSON content loaded once at startup
- Static parsers create immutable domain objects
- Content never reloaded or modified at runtime
- Domain models are data containers, not active objects

### 4. Clean Architecture Boundaries
```
UI Layer          → GameScreen, Components (Blazor)
Application Layer → GameFacade, Specialized Facades
Domain Layer      → GameWorld, Domain Models
Infrastructure    → Parsers, JSON Files
```

### 5. No Abstraction Over-Engineering
- **NO interfaces unless absolutely necessary**
- **NO inheritance hierarchies** - use composition
- **NO abstract base classes** - keep code direct
- Concrete classes only, straightforward implementations

### 6. State Isolation
- **NO parallel state tracking** across multiple objects
- When duplicate state found, identify single source of truth
- Other objects delegate to canonical source
- **NO caching layers** that can become stale

### 7. Catalogue Pattern: No String Matching, No Dictionaries

**THE FUNDAMENTAL PRINCIPLE:**

**JSON describes entities categorically. Parsers translate categorical descriptions to concrete values via Catalogues. Runtime code uses only concrete, strongly-typed properties. No strings, no dictionaries, no lookups.**

#### Three Phases of Data Flow

**PHASE 1: AUTHORING (JSON - Categorical/Descriptive)**
- Content creators describe entities in human-readable categorical terms
- Properties are descriptive ("Full", "Partial", "Fragile", "Simple")
- Some properties are absolute concrete values (coinCost: 10, time: 1)
- NO runtime semantics embedded in JSON - JSON describes WHAT, not HOW

**PHASE 2: PARSING (Translation - One Time Only)**
- Parser reads JSON via DTO
- Parser calls Catalogue to translate categorical → concrete
- Catalogue returns strongly-typed concrete values (int, bool, object)
- Parser stores concrete values directly on entity properties
- Entity persisted to GameWorld
- Translation happens ONCE at game initialization, NEVER during gameplay

**PHASE 3: RUNTIME (Concrete Values Only)**
- GameFacade/Facades fetch entities from GameWorld
- Use concrete properties directly (action.HealthRecovery, action.CoinCost)
- NO catalogue lookups - values already calculated
- NO string matching - no "if (id == 'something')" ever
- NO dictionary lookups - no Cost["coins"] ever
- PURE strongly-typed property access - compiler-enforced correctness

#### What This Eliminates

**FORBIDDEN FOREVER:**
1. String matching: `if (action.Id == "secure_room")`
2. Dictionary lookups: `Cost["coins"]`, `Cost.ContainsKey("coins")`
3. Dictionary properties: `Dictionary<string, int> Cost`
4. Enum routing at runtime: `switch (recoveryType)` in GameFacade
5. Catalogue calls at runtime: Catalogues live in Parsers folder, never imported by Facades
6. ID-based behavior branching: Entity IDs are for reference only, never control logic

**Why these are forbidden:**
- String matching = runtime typo bugs, no IntelliSense, couples code to JSON IDs
- Dictionaries = no type safety, hidden properties, runtime string keys
- Runtime catalogues = wasted CPU, violates parse-time translation principle
- ID-based logic = magic behavior tied to JSON string values, brittle

#### Benefits of Parse-Time Translation

1. **Fail Fast**: Bad JSON crashes at game init with clear error, not during gameplay
2. **Zero Runtime Overhead**: All calculations done once, not on every action execution
3. **Type Safety**: Compiler catches property access errors, no runtime string bugs
4. **IntelliSense Works**: Developers see real properties (action.HealthRecovery: int)
5. **AI Content Generation**: AI describes effects categorically, parser scales to current game state
6. **No Magic Strings**: Eliminates entire class of typo bugs ("coins" vs "coin")
7. **Testable**: Can unit test catalogues independently from runtime logic
8. **Maintainable**: Change catalogue = affects all entities, change entity property = compiler finds usages

#### Existing Catalogue Examples

- **EquipmentDurabilityCatalog**: JSON "Fragile" → (uses: 2, repairCost: 10) stored on entity
- **SocialCardEffectCatalog**: JSON (stat, depth) → CardEffectFormula with concrete values
- **PhysicalCardEffectCatalog, MentalCardEffectCatalog**: Same pattern

#### Code Review Enforcement

**Parser Code:**
- All categorical JSON properties translated via catalogue?
- All concrete values stored on entity properties (int, bool, object)?
- NO Dictionary<string, X> on entities?
- Catalogue throws on unknown categorical values?

**Runtime Code (GameFacade, Facades, Services):**
- NO catalogue imports?
- NO string comparisons (`action.Id == "something"`)?
- NO dictionary lookups?
- ONLY concrete property access?

**Entity Classes:**
- Properties are concrete types (int, bool, strongly-typed classes)?
- NO Dictionary<string, X> properties?
- Property names describe mechanics, not categories?

**If you see Dictionary or string matching in runtime code, STOP. The architecture is violated.**

#### The Complete Principle

**JSON is descriptive. Parsers translate descriptions to mechanics. Runtime executes mechanics.**

Content creators describe WHAT entities do in human terms.
Parsers translate WHAT to HOW using catalogues.
Runtime executes HOW using strongly-typed properties.

No strings in runtime. No dictionaries in entities. No catalogue lookups after initialization.
**Parse once. Execute forever with pure data access.**

---

## COMPONENT DEPENDENCIES

### Core Dependencies Graph

```
GameScreen
├── Requires: GameFacade
├── Manages: All screen navigation
└── Children: LocationContent, ConversationContent, etc.

GameFacade
├── Requires: GameWorld + All Specialized Facades
├── Provides: Orchestration layer
└── Dependencies: ConversationFacade, LocationFacade, etc.

Specialized Facades
├── Require: GameWorld + Domain-specific managers
├── Provide: Business logic for specific domains
└── Update: GameWorld state through operations

GameWorld
├── Requires: Nothing (zero external dependencies)
├── Provides: Single source of truth for all state
└── Contains: All domain model collections
```

### Service Registration Pattern

```csharp
// GameWorld created BEFORE service registration
GameWorld gameWorld = GameWorldInitializer.CreateGameWorld();

// Services registered with GameWorld dependency
services.AddSingleton(gameWorld);
services.AddScoped<ConversationFacade>();
services.AddScoped<GameFacade>();
```

### Critical Integration Points

**1. Three Parallel Tactical Systems Integration**
- **SocialFacade**: Social challenges (conversations) at NPCs
  - Initiative/Momentum/Doubt/Cadence resource system
  - Integrates with TokenFacade for relationship tokens
  - Integrates with KnowledgeService for knowledge/secrets
  - Personality rules (Proud, Devoted, Mercantile, Cunning, Steadfast)

- **MentalFacade**: Mental challenges (investigations) at Locations
  - Progress/Attention/Exposure/Leads resource system
  - Pauseable session model (can leave and return)
  - Location properties (Delicate, Obscured, Layered, Time-Sensitive, Resistant)

- **PhysicalFacade**: Physical challenges (obstacles) at Locations
  - Breakthrough/Exertion/Danger/Aggression resource system
  - One-shot session model (must complete in single attempt)
  - Challenge types (Combat, Athletics, Finesse, Endurance, Strength)

**2. Scene System Integration**
- Scenes spawn from SceneTemplates based on SpawnConditions
- Situations contain ChoiceTemplates that route to tactical systems
- ChoiceActionType.StartChallenge bridges strategic to tactical layer
- Scene.PlacementType determines where scene appears (NPC/Location/Route)

**3. Unified 5-Stat Progression**
- All cards across all three systems bind to: Insight/Rapport/Authority/Diplomacy/Cunning
- Stat levels determine card depth access (Level 1: depths 1-2, Level 3: depths 1-4, etc.)
- Playing cards grants XP to bound stat
- Stats manifest differently per system (Insight = pattern recognition in Mental, structural analysis in Physical, reading people in Social)

**4. Location System Integration**
- LocationFacade coordinates NPC placement at locations
- Integrates with TravelFacade for route validation
- Coordinates with TimeFacade for time-based availability
- Scenes can appear at Locations based on PlacementFilter

**5. Resource System Integration**
- ResourceFacade manages permanent resources (Health, Stamina, Focus, Hunger, Coins)
- **Mental challenges cost Focus** (concentration depletes)
- **Physical challenges cost Health + Stamina** (injury risk + exertion)
- **Social challenges cost nothing permanent** (but take time)
- Integrates with TimeFacade for time-based resource changes

---

## DEVELOPMENT GUIDELINES

### Before Making Any Changes

1. **Read this entire architecture document**
2. **Identify which layer you're modifying** (Content/Domain/Service/UI)
3. **Trace dependencies** using search tools to find all references
4. **Understand the impact radius** of your changes
5. **Verify architectural principles** are maintained

### Adding New Features

1. **Determine the business domain** - which facade should own it?
2. **Check if GameWorld state** needs new properties
3. **Design the service interface** following existing patterns
4. **Create context objects** for complex UI operations
5. **Follow the UI component hierarchy** - GameScreen → Child Components

### Modifying Existing Systems

1. **Never break the dependency flow** - dependencies always flow inward
2. **Never add shared mutable state** - use GameWorld as single source
3. **Never put business logic in UI** - keep facades responsible for rules
4. **Never bypass the facade layer** - UI must go through GameFacade

---

**This architecture ensures clean separation of concerns, predictable data flow, and maintainable code structure while supporting the complex conversation-based RPG mechanics of Wayfarer.**