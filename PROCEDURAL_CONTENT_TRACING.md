# Procedural Content Tracing System

## Executive Summary

A **strongly-typed, synchronous, single-component system** that captures the complete spawn graph of procedurally generated content (scenes, situations, choices) with their properties, relationships, and context. Enables debugging visualization as an interactive node graph showing what spawned what, when, why, and with what properties.

**Core Principle:** Capture spawn events at creation time, store relationships as direct object references, visualize as explorable tree/graph.

---

## Status
🚧 HIGH-LEVEL DESIGN COMPLETE - READY FOR IMPLEMENTATION

---

## Goals (FROM USER REQUEST)

✅ Track all procedurally generated scenes, situations, and choices
✅ Record what spawned what (parent-child relationships)
✅ Capture properties and context at spawn time
✅ Record which entities (locations/NPCs/routes) were involved
✅ Record which other scenes/situations were spawned as consequences
✅ Enable debugging screen with node graph visualization
✅ Strongly typed (explicit classes, no dictionaries)
✅ Synchronous (no async complexity)
✅ Single system/component (centralized tracking)

---

## Research Findings Summary

### Current Procedural Generation Architecture

**Five-System Flow:**
1. **Scene Selection** - SpawnConditions evaluation
2. **Scene Specification** - SceneSpawnReward with SceneTemplateId
3. **Package Generator** - SceneInstantiator generates JSON with PlacementFilters
4. **Entity Resolver** - EntityResolver.FindOrCreate resolves categorical filters
5. **Scene Instantiation** - SceneParser creates domain Scene entity

**Spawn Mechanisms:**
- **Scene Spawning:** Via `ChoiceReward.ScenesToSpawn` (List of SceneSpawnReward)
- **Situation Spawning:** Via `Situation.SuccessSpawns/FailureSpawns` (List of SpawnRule)
- **Choice Execution:** RewardApplicationService.ApplyChoiceReward → FinalizeSceneSpawns

**Entity Relationships:**
- Direct object references (NO IDs per HIGHLANDER principle)
- GameWorld.Scenes contains List<Scene>
- Scene.Situations contains List<Situation>
- Situation has Location/NPC/Route object references

**Key Properties:**
- Scene: TemplateId, Category, MainStorySequence, State, DisplayName, EstimatedDifficulty
- Situation: Name, Type, SystemType, Location, NPC, Route, SuccessSpawns, FailureSpawns
- ChoiceTemplate: ActionTextTemplate, PathType, RequirementFormula, CostTemplate, RewardTemplate

### Existing Visualization Infrastructure

**Available Patterns:**
- **Hex Grid:** CSS-based with coordinate positioning (HexMapContent.razor)
- **Debug Panel:** Collapsible fixed panel with sections (DebugPanel.razor)
- **Card System:** Grid layouts for item display (SituationCard.razor, etc.)
- **List Components:** Vertical item lists (StrangerList.razor)
- **Section Pattern:** Stacked sections with headers

**What's Missing:**
- No tree/graph visualization components
- No node-link diagram rendering
- No interactive graph libraries (no D3, no Chart.js)
- Conversation trees render linearly, not as diagrams

**Implication:** Need to build graph visualization from scratch using CSS/SVG.

---

## PART 1: TRACE DATA STRUCTURE

### Core Architecture Principle

**Capture spawn events at creation time, NOT query domain entities later.**

**Why:** Domain entities (Scene, Situation) don't have IDs. Object references are ephemeral (memory addresses change). Must capture relationship data at moment of spawning with stable identifiers.

**Pattern:** Observer/Event pattern where spawning systems notify tracing system of creation events.

### Trace Node Types

**Three node types reflecting spawn hierarchy:**

1. **SceneSpawnNode** - Tracks scene creation
2. **SituationSpawnNode** - Tracks situation creation
3. **ChoiceExecutionNode** - Tracks player choice execution

**Hierarchy:** SceneSpawnNode → contains List<SituationSpawnNode> → contains List<ChoiceExecutionNode>

### SceneSpawnNode (Root Level)

**Purpose:** Capture everything about a procedurally spawned scene.

**Properties:**

```
Core Identity:
- NodeId : string (GUID for stable reference)
- SceneTemplateId : string (which template spawned this)
- DisplayName : string (scene name)
- SpawnTimestamp : DateTime (when created)
- GameDay : int (which day)
- GameTimeBlock : TimeBlock (which time period)

Spawn Context:
- SpawnTrigger : SpawnTriggerType enum (ChoiceReward, SituationSuccess, SituationFailure, Tutorial)
- ParentNodeId : string (NodeId of parent scene, null if root)
- ParentSituationNodeId : string (NodeId of situation that spawned this)
- ParentChoiceNodeId : string (NodeId of choice that spawned this)

Scene Properties:
- Category : StoryCategory (MainStory, SideStory, Service)
- MainStorySequence : int? (A-story number if applicable)
- EstimatedDifficulty : string
- State : SceneState (Provisional, Active, Completed, Expired)
- ProgressionMode : ProgressionMode (Breathe, Cascade)
- SituationCount : int

Placement Properties:
- PlacedLocation : LocationSnapshot (location where scene became available)
- PlacementFilter : PlacementFilterSnapshot (categorical requirements used)

Children:
- Situations : List<SituationSpawnNode> (embedded situations)
- SpawnedScenes : List<string> (NodeIds of scenes spawned by this scene's choices)

Lifecycle:
- CompletedTimestamp : DateTime? (when scene completed)
- CompletedGameDay : int?
```

**Design Decisions:**
- Store SNAPSHOTS of properties (not object references - those don't serialize)
- ParentNodeId for traversal (can walk up/down tree)
- SpawnedScenes list for horizontal relationships (sibling scenes)
- State tracks lifecycle (can see which scenes are still active vs completed)

### SituationSpawnNode (Middle Level)

**Purpose:** Capture situation-level spawning and execution details.

**Properties:**

```
Core Identity:
- NodeId : string (GUID)
- SituationTemplateId : string (which template)
- Name : string
- Description : string
- SpawnTimestamp : DateTime

Spawn Context:
- ParentSceneNodeId : string (which scene contains this)
- SpawnTrigger : SituationSpawnTriggerType (InitialScene, SuccessSpawn, FailureSpawn)
- ParentSituationNodeId : string? (if spawned by another situation)

Situation Properties:
- Type : SituationType (Normal, Crisis)
- SystemType : TacticalSystemType (Social, Mental, Physical)
- InteractionType : SituationInteractionType (Instant, Challenge, Navigation)

Placement Properties:
- Location : LocationSnapshot (where situation takes place)
- NPC : NPCSnapshot? (which NPC if applicable)
- Route : RouteSnapshot? (which route if applicable)
- SegmentIndex : int? (for route situations)

Children:
- Choices : List<ChoiceExecutionNode> (choices player made)
- SpawnedSituations : List<string> (NodeIds of child situations)

Lifecycle:
- LifecycleStatus : LifecycleStatus (Selectable, Completed, Failed)
- CompletedTimestamp : DateTime?
- LastChallengeSucceeded : bool?
```

**Design Decisions:**
- LocationSnapshot instead of Location object (serialize properties needed for debugging)
- NPCSnapshot captures name, demeanor, profession (enough to identify)
- RouteSnapshot captures origin/destination names (not full objects)
- Tracks both choices executed AND situations spawned (complete trace)

### ChoiceExecutionNode (Leaf Level)

**Purpose:** Capture player decisions and their consequences.

**Properties:**

```
Core Identity:
- NodeId : string (GUID)
- ChoiceId : string (template ID)
- ActionText : string (what player saw)
- ExecutionTimestamp : DateTime

Execution Context:
- ParentSituationNodeId : string
- PathType : ChoicePathType (InstantSuccess, Challenge, Fallback)
- ActionType : ChoiceActionType (Instant, StartChallenge, Navigate)

Requirements & Costs:
- RequirementSnapshot : RequirementSnapshot (what was required)
- CostSnapshot : CostSnapshot (what player paid)
- PlayerMetRequirements : bool (did player qualify)

Challenge Details (if applicable):
- ChallengeId : string?
- DeckId : string?
- ChallengeSucceeded : bool?
- ChallengeOutcome : string? (summary)

Rewards:
- RewardSnapshot : RewardSnapshot (base rewards)
- OnSuccessRewardSnapshot : RewardSnapshot? (challenge success)
- OnFailureRewardSnapshot : RewardSnapshot? (challenge failure)

Spawn Consequences:
- SpawnedSceneNodeIds : List<string> (scenes spawned by this choice)
- SpawnedSituationNodeIds : List<string> (situations spawned)

Navigation (if applicable):
- DestinationLocation : LocationSnapshot?
```

**Design Decisions:**
- Captures BOTH requirements and whether player met them (debug why choice was/wasn't available)
- Challenge outcome tracked (success/failure affects spawns)
- Spawn consequences in BOTH directions (choice knows what it spawned, spawned nodes know parent)
- Reward snapshots show what player actually got

### Snapshot Classes

**Why Snapshots:** Domain entities use object references. Cannot store Scene/Location/NPC objects in trace (circular references, serialization issues, memory bloat). Instead capture PROPERTIES needed for debugging.

**LocationSnapshot:**
```
- Name : string
- HexPosition : AxialCoordinates (Q, R)
- Purpose : LocationPurpose
- Privacy : LocationPrivacy
- Safety : LocationSafety
- Activity : LocationActivity
- Quality : Quality
```

**NPCSnapshot:**
```
- Name : string
- Profession : string
- Demeanor : NPCDemeanor
- SocialStanding : NPCSocialStanding
- StoryRole : NPCStoryRole
```

**RouteSnapshot:**
```
- OriginLocationName : string
- DestinationLocationName : string
- TerrainType : TerrainType
- Distance : int
```

**PlacementFilterSnapshot:**
```
- PersonalityTypes : List<NPCDemeanor>
- LocationCapabilities : List<LocationCapability>
- LocationTags : List<string>
- TerrainTypes : List<TerrainType>
- SelectionStrategy : SelectionStrategy
```

**RequirementSnapshot:**
```
- RequiredRapport : int?
- RequiredInsight : int?
- RequiredAuthority : int?
- RequiredDiplomacy : int?
- RequiredCunning : int?
- RequiredCoins : int?
- RequiredStates : List<string>
```

**CostSnapshot:**
```
- CoinsSpent : int
- StaminaSpent : int
- FocusSpent : int
- HealthSpent : int
- ResolveSpent : int
```

**RewardSnapshot:**
```
- CoinsGained : int
- ResolveGained : int
- HealthGained : int
- StaminaGained : int
- FocusGained : int
- StatGains : Dictionary<string, int> (stat name → amount)
- BondChanges : List<string> (NPC name + delta summary)
- ItemsGranted : List<string> (item names)
- StatesApplied : List<string> (state names)
```

**Design Decisions:**
- Snapshots are IMMUTABLE (set once at creation)
- Contain ONLY what's needed for debugging (not complete entity state)
- Primitive types and enums (easy serialization if needed later)
- Human-readable (developer can understand without lookup)

---

## PART 2: TRACE COLLECTION SYSTEM

### Single Central Component

**Class:** `ProceduralContentTracer`

**Location:** `/home/user/Wayfarer/src/Services/ProceduralContentTracer.cs`

**Purpose:** Single authoritative system tracking all spawn events.

**Responsibilities:**
- Capture spawn events from spawning systems
- Build trace node graph
- Maintain root-level index
- Provide query methods for visualization
- Persist trace data (optional future: serialize to JSON for post-game analysis)

**NOT Responsible For:**
- Modifying game state
- Validating spawns
- Making spawn decisions
- UI rendering (separate component handles visualization)

### ProceduralContentTracer Class Structure

```
Properties:
- RootScenes : List<SceneSpawnNode> (top-level scenes, no parent)
- AllSceneNodes : List<SceneSpawnNode> (flat index for ID lookup)
- AllSituationNodes : List<SituationSpawnNode> (flat index)
- AllChoiceNodes : List<ChoiceExecutionNode> (flat index)
- IsEnabled : bool (can disable for performance if needed)

Methods:
- RecordSceneSpawn(sceneData) : SceneSpawnNode
- RecordSituationSpawn(situationData) : SituationSpawnNode
- RecordChoiceExecution(choiceData) : ChoiceExecutionNode
- GetNodeById(nodeId) : object (returns scene/situation/choice node)
- GetRootScenes() : List<SceneSpawnNode>
- GetChildrenOfScene(nodeId) : List<SceneSpawnNode>
- GetSpawnChain(nodeId) : List<object> (walk up to root)
- Clear() (reset for new game)
```

**Design Decisions:**
- List collections (per codebase standards, not Dictionary)
- LINQ queries for lookups (FirstOrDefault by NodeId)
- Synchronous methods (no async complexity)
- Optional enabling/disabling (production might disable, debug enables)

### Integration Points (Where to Hook)

**Three critical integration points where spawning happens:**

#### 1. Scene Spawning Hook

**Location:** `SceneInstanceFacade.SpawnScene()` method

**Hook Point:** AFTER `PackageLoaderFacade.LoadDynamicPackage()` returns Scene entity

**Capture:**
```
Scene domain entity → Extract properties → Create SceneSpawnNode
- Read Scene.TemplateId, DisplayName, Category, etc.
- Read SpawnContext (passed as parameter to SpawnScene)
- Create snapshots for Location, PlacementFilter
- Call Tracer.RecordSceneSpawn(sceneData)
- Receive SceneSpawnNode with NodeId
- Continue normal execution
```

**No Impact on Game Logic:** Tracer is observer only, doesn't modify Scene.

#### 2. Situation Spawning Hook

**Location:** `SpawnFacade.ExecuteSpawnRules()` method

**Hook Point:** AFTER new Situation created and added to Scene.Situations

**Capture:**
```
Situation domain entity → Extract properties → Create SituationSpawnNode
- Read Situation.Name, Type, SystemType, etc.
- Create LocationSnapshot from Situation.Location
- Create NPCSnapshot from Situation.NPC (if applicable)
- Call Tracer.RecordSituationSpawn(situationData)
- Link to parent SceneSpawnNode
```

**Also Hook:** Initial situations created during scene parsing (not just cascaded spawns).

#### 3. Choice Execution Hook

**Location:** `RewardApplicationService.ApplyChoiceReward()` method

**Hook Point:** BEFORE applying rewards (capture choice, AFTER execution capture spawns)

**Capture:**
```
ChoiceTemplate + Situation context → Create ChoiceExecutionNode
- Read ChoiceTemplate properties
- Create RequirementSnapshot, CostSnapshot
- Record player qualification status
- Call Tracer.RecordChoiceExecution(choiceData)
- AFTER FinalizeSceneSpawns: Link spawned scene NodeIds to ChoiceExecutionNode
```

**Two-Phase Recording:**
1. Initial: Capture choice execution details
2. Post-Spawn: Update with spawned scene/situation NodeIds

### Linking Strategy

**Bidirectional Links:**
- Parent → Children (SceneSpawnNode.SpawnedScenes list)
- Child → Parent (SceneSpawnNode.ParentNodeId)

**Why Both Directions:**
- Top-down traversal (explore all children)
- Bottom-up traversal (trace choice back to root scene)
- Visualization needs both (render tree, highlight path)

**Link Establishment:**

```
When spawning scene S2 from choice C1 in situation T1 in scene S1:

1. RecordChoiceExecution creates ChoiceExecutionNode
   - ParentSituationNodeId = T1.NodeId

2. RecordSceneSpawn creates SceneSpawnNode for S2
   - ParentNodeId = S1.NodeId
   - ParentSituationNodeId = T1.NodeId
   - ParentChoiceNodeId = C1.NodeId

3. Update backwards links:
   - C1.SpawnedSceneNodeIds.Add(S2.NodeId)
   - S1.SpawnedScenes.Add(S2.NodeId)

Result: Can traverse S2 → C1 → T1 → S1 OR S1 → T1 → C1 → S2
```

### Snapshot Creation Helpers

**Static helper class:** `SnapshotFactory`

**Methods:**
```
- CreateLocationSnapshot(Location) : LocationSnapshot
- CreateNPCSnapshot(NPC) : NPCSnapshot
- CreateRouteSnapshot(RouteOption) : RouteSnapshot
- CreatePlacementFilterSnapshot(PlacementFilter) : PlacementFilterSnapshot
- CreateRequirementSnapshot(CompoundRequirement) : RequirementSnapshot
- CreateCostSnapshot(ChoiceCost, executed) : CostSnapshot
- CreateRewardSnapshot(ChoiceReward) : RewardSnapshot
```

**Purpose:** Centralize snapshot creation logic, ensure consistency.

**Example:**
```csharp
LocationSnapshot CreateLocationSnapshot(Location location)
{
    return new LocationSnapshot
    {
        Name = location.Name,
        HexPosition = location.HexPosition,
        Purpose = location.Purpose,
        Privacy = location.Privacy,
        Safety = location.Safety,
        Activity = location.Activity,
        Quality = location.Quality
    };
}
```

---

## PART 3: VISUALIZATION STRATEGY

### Design Goals

**Functional over beautiful:** Developer debugging tool, not player-facing content.

**Key Requirements:**
- Show spawn hierarchy clearly (tree structure)
- Display node properties inline (no excessive clicking)
- Navigate large trees (100+ nodes)
- Identify patterns (repeated templates, cascading failures)
- Trace individual spawn chains (highlight path from root to selected node)
- Filter/search (find specific scenes, templates, locations)

**NOT Requirements:**
- Pixel-perfect layout
- Animated transitions
- Touch gestures
- Mobile optimization
- Accessibility (developer tool)

### Visualization Approach: Vertical Tree Layout

**Pattern:** CSS-based vertical tree with indentation hierarchy.

**Why Vertical (not horizontal):**
- Scales better for deep trees (scrolling down natural)
- More readable text (horizontal names, not rotated)
- Simpler CSS (no complex SVG path drawing)
- Familiar pattern (file explorers, code outlines)

**Layout Structure:**

```
Root Scene: A3 Journey to Merchant Quarter
├─ Situation: Negotiate with Guard (choice: bribe) → spawned Scene A4
│  ├─ Choice: Bribe Guard (8 coins) [EXECUTED]
│  │  └─ Spawned: Scene A4 "Investigation Begins"
│  └─ Choice: Persuade Guard (Rapport 5) [NOT CHOSEN]
├─ Situation: Enter Marketplace
│  └─ Choice: Navigate to Market Square [EXECUTED]

Root Scene: A4 Investigation Begins
├─ Situation: Question Witness (challenge: SUCCEEDED)
│  ├─ Choice: Interrogate (challenge) [EXECUTED]
│  │  ├─ Spawned: Situation "Follow-up Questions"
│  │  └─ Challenge: SUCCESS (3/3 cards succeeded)
│  └─ Spawned Situation: Follow-up Questions
│     └─ Choice: Press for Details [EXECUTED]
```

**Visual Hierarchy:**
- Level 0: Root scenes (bold, large, colored by category)
- Level 1: Situations within scene (indented, medium size)
- Level 2: Choices within situation (indented further, small)
- Level 3: Spawned consequences (indented, gray, italics)

### Node Card Design

**Each node renders as a card with:**

**SceneSpawnNode Card:**
```
┌─────────────────────────────────────────┐
│ [SCENE] A3: Journey to Merchant Quarter │ ← Header with category badge
│ Main Story #3 | Provisional             │ ← Metadata line
│ Spawned: Day 2, Morning                 │ ← Timing
│ Location: City Gates (Public, Safe)     │ ← Placement
│ Template: journey_to_location           │ ← Template ID
│ Difficulty: Moderate | 3 situations     │ ← Properties
│ ↳ Spawned by: Choice "Accept Mission"  │ ← Parent link (clickable)
│ ↳ Spawned: 1 scene (A4)                │ ← Child summary (expandable)
└─────────────────────────────────────────┘
```

**SituationSpawnNode Card:**
```
┌─────────────────────────────────────────┐
│ [SITUATION] Negotiate with Guard        │ ← Header
│ Social Challenge | Crisis               │ ← Type metadata
│ Location: Checkpoint Alpha (Hostile)    │ ← Context
│ NPC: Guard Captain Marcus (Hostile)     │ ← NPC
│ Completed: Day 2, Morning               │ ← Outcome
│ ↳ 2 choices (1 executed)                │ ← Choice summary
│ ↳ Spawned: 1 scene                      │ ← Spawn consequences
└─────────────────────────────────────────┘
```

**ChoiceExecutionNode Card:**
```
┌─────────────────────────────────────────┐
│ [CHOICE] Bribe Guard (8 coins)          │ ← Action text
│ Money Path | EXECUTED                   │ ← Path type + status
│ Cost: 8 coins (paid)                    │ ← Cost
│ Requirements: None                       │ ← Requirements
│ Rewards: +1 Authority, Access Granted   │ ← Rewards
│ ↳ Spawned: Scene A4 "Investigation..."  │ ← Consequences
└─────────────────────────────────────────┘
```

**Design Decisions:**
- Compact (most info visible without expand)
- Color-coded headers (scene type, situation type, choice outcome)
- Clickable parent links (jump to parent node)
- Expandable child sections (click to show/hide children)
- Icons for node types (scene/situation/choice badges)

### CSS Structure

**Base Classes:**

```css
.trace-tree {
  /* Container for entire tree */
  padding: 20px;
  font-family: monospace; /* developer tool aesthetic */
  background: #1e1e1e; /* dark theme */
  color: #d4d4d4;
}

.trace-node {
  /* Base node card */
  margin: 8px 0;
  padding: 12px;
  border-left: 3px solid #444;
  background: #2d2d2d;
  border-radius: 4px;
}

.trace-node-scene {
  border-left-color: #4ec9b0; /* teal for scenes */
  padding-left: 20px; /* root level */
}

.trace-node-situation {
  border-left-color: #dcdcaa; /* yellow for situations */
  padding-left: 40px; /* indented */
}

.trace-node-choice {
  border-left-color: #569cd6; /* blue for choices */
  padding-left: 60px; /* further indented */
}

.trace-node-expanded {
  /* When node is expanded showing children */
}

.trace-node-collapsed {
  /* When node is collapsed hiding children */
  opacity: 0.7;
}

.trace-node-header {
  font-weight: bold;
  font-size: 14px;
  margin-bottom: 6px;
  cursor: pointer; /* clickable to expand/collapse */
}

.trace-node-metadata {
  font-size: 12px;
  color: #888;
  line-height: 1.6;
}

.trace-node-children {
  margin-top: 8px;
  /* Recursive container for child nodes */
}

.trace-badge {
  /* Node type badge (SCENE, SITUATION, CHOICE) */
  display: inline-block;
  padding: 2px 6px;
  background: #444;
  border-radius: 3px;
  font-size: 10px;
  font-weight: bold;
  margin-right: 8px;
}

.trace-link {
  /* Clickable parent/child links */
  color: #4fc1ff;
  cursor: pointer;
  text-decoration: underline;
}

.trace-link:hover {
  color: #6fc3ff;
}
```

**Category Colors:**
```css
.scene-main-story { border-left-color: #f14c4c; } /* Red */
.scene-side-story { border-left-color: #3794ff; } /* Blue */
.scene-service { border-left-color: #89d185; } /* Green */

.situation-completed { background: #1e3a1e; } /* Dark green tint */
.situation-failed { background: #3a1e1e; } /* Dark red tint */
.situation-active { background: #3a3a1e; } /* Dark yellow tint */

.choice-executed { border-left-color: #89d185; } /* Green */
.choice-not-chosen { border-left-color: #666; opacity: 0.5; } /* Gray */
```

**Highlighting:**
```css
.trace-node-highlighted {
  /* When tracing path to selected node */
  background: #3a3a00;
  box-shadow: 0 0 8px #ffff00;
}

.trace-node-selected {
  /* Currently selected node */
  background: #003a3a;
  box-shadow: 0 0 12px #00ffff;
  border-left-width: 5px;
}
```

### Component Structure

**Main Component:** `SpawnGraphViewer.razor`

**Location:** `/home/user/Wayfarer/src/Pages/Components/SpawnGraphViewer.razor`

**Component Hierarchy:**

```
SpawnGraphViewer (top-level container)
├─ Filter/Search Panel (top section)
│  ├─ Search input (filter by name/template)
│  ├─ Category filter (Main/Side/Service)
│  ├─ Day range filter (show scenes from days X-Y)
│  └─ Template filter (specific archetype)
├─ Tree Container (scrollable main area)
│  └─ SceneNode (recursive component)
│     ├─ Scene card header (expandable)
│     ├─ Scene metadata
│     └─ Children (if expanded)
│        └─ SituationNode (recursive component)
│           ├─ Situation card header
│           ├─ Situation metadata
│           └─ Children (if expanded)
│              └─ ChoiceNode (leaf component)
│                 ├─ Choice card header
│                 └─ Choice metadata
└─ Detail Panel (right sidebar, optional)
   └─ Selected node full details (when node clicked)
```

**Recursive Rendering:**

Each node type renders itself + recursively renders children. Standard Blazor recursive component pattern.

**Example (SceneNode.razor):**
```razor
<div class="trace-node trace-node-scene @GetCategoryClass() @(IsExpanded ? "trace-node-expanded" : "trace-node-collapsed")"
     @onclick="ToggleExpand">

    <div class="trace-node-header">
        <span class="trace-badge">SCENE</span>
        @Node.DisplayName
    </div>

    <div class="trace-node-metadata">
        <div>@Node.Category | @Node.State</div>
        <div>Spawned: Day @Node.GameDay, @Node.GameTimeBlock</div>
        <div>Location: @Node.PlacedLocation.Name (@Node.PlacedLocation.Privacy, @Node.PlacedLocation.Safety)</div>
        <div>Template: @Node.SceneTemplateId</div>

        @if (Node.ParentNodeId != null)
        {
            <div>↳ Spawned by: <span class="trace-link" @onclick="() => JumpToParent()">@GetParentName()</span></div>
        }

        @if (Node.SpawnedScenes.Count > 0)
        {
            <div>↳ Spawned: @Node.SpawnedScenes.Count scene(s)</div>
        }
    </div>

    @if (IsExpanded && Node.Situations.Count > 0)
    {
        <div class="trace-node-children">
            @foreach (var situation in Node.Situations)
            {
                <SituationNode Node="@situation" Tracer="@Tracer" OnNodeSelected="@OnNodeSelected" />
            }
        </div>
    }
</div>
```

**Key Features:**
- Click header to expand/collapse
- Click parent link to jump to parent (scrolls into view, highlights)
- Click child link to expand children
- Recursive nesting preserves hierarchy

### Interaction Patterns

**1. Expand/Collapse:**
- Click node header → toggle IsExpanded state
- Renders/hides children section
- Persists expand state during session (don't collapse on re-render)

**2. Jump to Node:**
- Click parent/child link → scroll to target node
- Highlight target node (yellow glow for 2 seconds)
- Auto-expand path to target if collapsed

**3. Trace Path:**
- Click "Trace to Root" button on node
- Highlights all ancestors up to root scene
- Dims non-highlighted nodes (opacity 0.3)
- Shows spawn chain clearly

**4. Filter/Search:**
- Type in search box → filter nodes by name/template
- Select category → show only Main Story scenes
- Set day range → show only scenes from days 1-5
- Filters stack (AND logic)
- Maintains expand state for matching nodes

**5. Detail Panel:**
- Click node → select it
- Detail panel shows FULL properties (all snapshots expanded)
- Shows complete requirement/cost/reward breakdowns
- Copy button to copy NodeId for logging

### Data Flow

**Component Parameters:**

```csharp
[Parameter] public ProceduralContentTracer Tracer { get; set; }
[Parameter] public string InitialSelectedNodeId { get; set; } // Optional: jump to specific node on load
```

**Component State:**

```csharp
private List<SceneSpawnNode> FilteredRootScenes { get; set; } // After applying filters
private string SearchText { get; set; } = "";
private StoryCategory? CategoryFilter { get; set; } = null;
private int? MinDay { get; set; } = null;
private int? MaxDay { get; set; } = null;
private string SelectedNodeId { get; set; } // Currently selected for detail panel
private HashSet<string> ExpandedNodeIds { get; set; } = new(); // Track expanded state
private HashSet<string> HighlightedNodeIds { get; set; } = new(); // For path tracing
```

**Render Logic:**

```csharp
protected override void OnParametersSet()
{
    // Apply filters to Tracer.RootScenes
    FilteredRootScenes = Tracer.RootScenes
        .Where(scene => MatchesFilters(scene))
        .ToList();
}

private bool MatchesFilters(SceneSpawnNode scene)
{
    if (!string.IsNullOrEmpty(SearchText) && !scene.DisplayName.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
        return false;

    if (CategoryFilter.HasValue && scene.Category != CategoryFilter.Value)
        return false;

    if (MinDay.HasValue && scene.GameDay < MinDay.Value)
        return false;

    if (MaxDay.HasValue && scene.GameDay > MaxDay.Value)
        return false;

    return true;
}

private void ToggleExpand(string nodeId)
{
    if (ExpandedNodeIds.Contains(nodeId))
        ExpandedNodeIds.Remove(nodeId);
    else
        ExpandedNodeIds.Add(nodeId);

    StateHasChanged();
}

private void SelectNode(string nodeId)
{
    SelectedNodeId = nodeId;
    StateHasChanged();
}

private void TracePath(string nodeId)
{
    HighlightedNodeIds.Clear();

    // Walk up parent chain
    string currentId = nodeId;
    while (currentId != null)
    {
        HighlightedNodeIds.Add(currentId);

        // Get parent of current node (scene/situation/choice)
        var node = Tracer.GetNodeById(currentId);
        currentId = GetParentId(node);
    }

    StateHasChanged();
}
```

### Performance Considerations

**Potential Issue:** Large trees (500+ nodes) could slow rendering.

**Optimizations:**

**1. Virtualization (if needed):**
- Only render visible nodes (viewport-based culling)
- Use Blazor Virtualize component for root scene list
- Lazy-load children on expand (already handled by expand/collapse)

**2. Filtering:**
- Filter at root level, hide non-matching subtrees entirely
- Don't render filtered-out nodes (early return in MatchesFilters)

**3. Memoization:**
- Cache filter results (don't re-filter on every render)
- Only recompute when SearchText/Filters change

**4. Collapse All:**
- Button to collapse all nodes (reset ExpandedNodeIds)
- Start with all collapsed (opt-in expansion)

**5. Pagination:**
- Show 20 root scenes at a time
- "Load More" button for additional scenes
- Default: show only most recent scenes first

**Implementation Priority:**
1. Start simple (render all, no virtualization)
2. Test with large dataset (100 scenes, 300 situations)
3. Add optimizations if slow (likely not needed given Blazor performance)

### Alternative View: Compact Timeline

**Secondary visualization mode:** Timeline view showing scenes chronologically.

**Layout:**
```
Day 1  Morning: [A1 Tutorial] [Service: Inn Check-in]
       Afternoon: [A2 Meet Contact]
       Evening: [Service: Evening Meal]

Day 2  Morning: [A3 Journey] → spawned [A4 Investigation]
       Afternoon: [A4 Investigation] → spawned [A5 Confrontation]
```

**Benefits:**
- See progression over time
- Identify pacing issues (too many scenes one day, none another)
- Spot service scene clustering

**Implementation:**
- Group scenes by GameDay + GameTimeBlock
- Render as horizontal cards in vertical timeline
- Click scene → switch to tree view with that scene selected

**Lower Priority:** Implement tree view first, timeline later if needed.

---

## PART 4: USER EXPERIENCE & WORKFLOW

### Access Pattern

**How developers access the trace viewer:**

**Option 1: Debug Panel Integration (Recommended)**

Add button to existing DebugPanel component:
```
[Debug Panel]
├─ Player Stats controls
├─ Resource controls
├─ Quick Actions
└─ [NEW] "View Spawn Graph" button → opens modal/page
```

**Why:** Existing debug infrastructure, familiar location, consistent UX.

**Option 2: Standalone Debug Route**

Create dedicated route `/debug/spawn-graph`:
- Navigate via URL directly
- Bookmark for quick access
- Can open in separate tab/window

**Why:** Doesn't clutter main UI, can view alongside game.

**Recommended Hybrid:**
- Add button to DebugPanel (primary access)
- Also create `/debug/spawn-graph` route (secondary access)
- Button opens route in modal overlay (can toggle to full-screen)

### Typical Debugging Workflows

**Workflow 1: "Why didn't this scene spawn?"**

Developer expects scene to spawn from choice but it doesn't appear.

**Steps:**
1. Open Spawn Graph Viewer
2. Find the scene with choice that should have spawned (search by scene name)
3. Expand scene → situations → find choice
4. Examine choice node:
   - Check SpawnedSceneNodeIds list (empty = nothing spawned)
   - Check RewardSnapshot → ScenesToSpawn (was spawn configured?)
   - Check SpawnConditions in parent scene (were conditions met?)
5. Trace to root to see full context
6. Identify: Missing SpawnReward config OR failed SpawnConditions OR bug in spawning code

**What visualization provides:**
- Clear view of what choice actually spawned
- Reward configuration visible inline
- Spawn conditions captured at creation time
- Can compare expected vs actual

**Workflow 2: "Where did this scene come from?"**

Player encounters unexpected scene, developer needs to trace origin.

**Steps:**
1. Open Spawn Graph Viewer
2. Search for scene by name
3. Click "Trace to Root" button
4. Path highlights: Root scene → situation → choice → spawned scene → ...
5. Examine each step in chain:
   - What template spawned parent?
   - What choice triggered spawn?
   - What placement filters were used?
   - When was it spawned (day/time)?
6. Identify: Archetype generating unexpected spawns OR placement filter too broad OR cascading spawn chain unintended

**What visualization provides:**
- Complete spawn chain visible
- Parent-child relationships clear
- Placement context captured
- Timeline of spawning events

**Workflow 3: "Is this archetype working correctly?"**

Developer modifies scene archetype, wants to verify behavior.

**Steps:**
1. Play game until archetype spawns (multiple instances)
2. Open Spawn Graph Viewer
3. Filter by template ID (e.g., "investigation_archetype")
4. View all instances of that archetype
5. Examine variations:
   - Different NPCs/locations used (placement working?)
   - Different choice outcomes (all paths functional?)
   - Spawned consequences consistent (cascading working?)
6. Spot patterns: Always spawns at same location = placement filter wrong, Never spawns follow-up = reward config missing, Always same NPC = selection strategy too narrow

**What visualization provides:**
- See all instances of archetype together
- Compare placement across instances
- Verify cascading behavior
- Identify degenerate patterns

**Workflow 4: "Is pacing balanced?"**

Developer wants to verify scene distribution over time.

**Steps:**
1. Open Spawn Graph Viewer
2. Switch to Timeline View (alternative visualization)
3. Examine day-by-day distribution:
   - Day 1: 5 scenes (good)
   - Day 2: 12 scenes (too many? clustered in morning?)
   - Day 3: 2 scenes (too few? content drought?)
4. Identify: Service scenes clustering, A-story scenes too frequent, B/C content not spawning
5. Adjust spawn rates / spacing logic

**What visualization provides:**
- Chronological view of all spawns
- Pacing visible at glance
- Category distribution clear
- Time-of-day patterns visible

**Workflow 5: "Why did choice requirements seem wrong?"**

Player reports choice required wrong stat value.

**Steps:**
1. Open Spawn Graph Viewer
2. Find scene with choice in question
3. Expand to choice node
4. Examine RequirementSnapshot:
   - RequiredRapport: 8 (what was shown to player)
   - Base formula from template (where did 8 come from?)
5. Examine parent situation:
   - NPC: Hostile Guard (Demeanor: Hostile)
   - NPCSnapshot shows Hostile = 1.4× multiplier
6. Trace formula: Base Rapport 6 × 1.4 = 8.4 → rounds to 8
7. Identify: Working as designed OR base value wrong in archetype

**What visualization provides:**
- Actual requirement values captured
- NPC properties visible (explain scaling)
- Formula derivation traceable
- Context for requirement calculation

### UI State Management

**Persistent State (Across Sessions):**

Use browser localStorage to persist:
- ExpandedNodeIds (remember what user expanded)
- SelectedNodeId (remember what was selected)
- Filter preferences (category, day range)
- View mode (tree vs timeline)

**Why:** Developer debugging same issue across multiple test runs wants to return to same state.

**Session State (Reset on Page Reload):**
- HighlightedNodeIds (traced paths)
- ScrollPosition (can scroll to same node)

**Reset Button:**
- Clear all filters
- Collapse all nodes
- Deselect node
- Clear highlights
- Return to default state

### Export/Logging Features

**Export Trace Data (Future Enhancement):**

Button to export entire trace as JSON file:
```json
{
  "exportTimestamp": "2025-01-15T14:30:00Z",
  "gameDay": 5,
  "rootScenes": [
    {
      "nodeId": "guid-here",
      "sceneTemplateId": "a3_journey",
      "displayName": "Journey to Merchant Quarter",
      "situations": [...],
      "spawnedScenes": [...]
    }
  ]
}
```

**Use Cases:**
- Share trace with other developers
- Archive trace for bug reports
- Diff traces between game versions
- Analyze patterns programmatically

**Console Logging Hook:**

Add console log when node clicked:
```javascript
console.log(`[SpawnTrace] Scene: ${node.DisplayName}, NodeId: ${node.NodeId}, Template: ${node.SceneTemplateId}`);
```

**Why:** Can copy NodeId for searching in code/logs.

---

## PART 5: IMPLEMENTATION ROADMAP

### Phase 1: Core Data Structure (Highest Priority)

**Deliverables:**
1. Node classes (SceneSpawnNode, SituationSpawnNode, ChoiceExecutionNode)
2. Snapshot classes (LocationSnapshot, NPCSnapshot, etc.)
3. ProceduralContentTracer service class
4. SnapshotFactory helper class

**Verification:**
- Nodes created successfully
- Snapshots capture correct properties
- Tracer maintains collections correctly
- No performance impact on game

**Estimated Complexity:** Medium
- ~10 classes total
- Straightforward property classes
- No complex logic yet

### Phase 2: Integration Hooks (Critical Path)

**Deliverables:**
1. Hook in SceneInstanceFacade.SpawnScene
2. Hook in SpawnFacade.ExecuteSpawnRules
3. Hook in RewardApplicationService.ApplyChoiceReward
4. Hook in SceneParser for initial situations
5. Bidirectional linking logic

**Verification:**
- Spawn events captured correctly
- Parent-child links established
- No missed spawn events
- No impact on game logic flow

**Estimated Complexity:** Medium-High
- Must understand spawn flow completely
- Must not break existing functionality
- Requires careful testing

### Phase 3: Basic Visualization (MVP)

**Deliverables:**
1. SpawnGraphViewer.razor component
2. SceneNode.razor recursive component
3. SituationNode.razor recursive component
4. ChoiceNode.razor component
5. Basic CSS styling
6. Expand/collapse functionality

**Verification:**
- Tree renders correctly
- Hierarchy visible
- Can expand/collapse nodes
- Properties displayed inline

**Estimated Complexity:** Medium
- Standard Blazor component patterns
- CSS styling straightforward
- Recursive rendering well-understood

### Phase 4: Interactive Features (Enhancement)

**Deliverables:**
1. Search/filter panel
2. Jump-to-node navigation
3. Trace-to-root path highlighting
4. Detail panel
5. NodeId copy functionality

**Verification:**
- Search filters correctly
- Jump scrolls to node
- Path highlights correctly
- Detail panel shows all properties

**Estimated Complexity:** Low-Medium
- Standard UI interaction patterns
- StateHasChanged() management
- Scrolling logic (element.scrollIntoView)

### Phase 5: Polish & Optimization (Nice-to-Have)

**Deliverables:**
1. Timeline view (alternative visualization)
2. Export to JSON
3. Virtualization (if needed)
4. Persistent state (localStorage)
5. Console logging hooks

**Verification:**
- Timeline groups correctly
- Export produces valid JSON
- Performance acceptable with large datasets
- State persists across refreshes

**Estimated Complexity:** Low-Medium
- Each feature independent
- Can be added incrementally
- Not blocking for core functionality

### Testing Strategy

**Unit Tests (If Applicable):**
- SnapshotFactory methods return correct snapshots
- ProceduralContentTracer manages collections correctly
- Linking logic creates bidirectional references

**Integration Tests:**
- Spawn entire scene, verify trace captured
- Execute choice with spawn reward, verify scene linked
- Situation cascading spawn, verify parent-child relationship

**Manual Testing:**
- Play game for 5 days, open viewer, verify complete tree
- Search for specific scene, verify found
- Trace path to root, verify highlights correct
- Filter by category, verify only matching scenes shown

**Performance Testing:**
- Generate 100 scenes with 300 situations, verify render performance
- Measure memory usage of trace data (acceptable overhead?)
- Profile Blazor render time for large tree

### Rollout Strategy

**Development Environment:**
- Enable tracer by default (always capture spawns)
- Viewer accessible via debug panel
- Full features enabled

**Production Environment (If Shipped):**
- Disable tracer by default (set IsEnabled = false)
- Viewer hidden (no debug panel in production)
- Can enable via config file for bug reports

**Why:** Tracing has small overhead (memory + CPU for snapshot creation). Acceptable in development, should be optional in production.

### Maintenance Considerations

**When Spawning Changes:**
- Add new spawn mechanism → add integration hook
- Modify spawn logic → update snapshots to capture new properties
- Change entity properties → update snapshot classes

**When Visualization Needs Change:**
- Add new filter → extend filter panel
- Change layout → modify CSS
- Add export format → extend export logic

**Documentation:**
- Document hook locations in codebase
- Document snapshot property meanings
- Document filter logic
- Document node ID generation scheme

---

## PART 6: TECHNICAL SPECIFICATIONS

### Node ID Generation

**Pattern:** GUID for stable, unique, collision-free identifiers.

```csharp
SceneSpawnNode node = new SceneSpawnNode
{
    NodeId = Guid.NewGuid().ToString(),
    // ...
};
```

**Why GUID:**
- Guaranteed unique (no collision risk)
- Can be generated independently (no central counter)
- Immutable (doesn't change when game reloads)
- Serializable (export to JSON works)
- Copyable (developer can copy-paste into logs)

**NOT Sequential IDs:**
- Sequential IDs require central counter (state management complexity)
- Sequential IDs break when game reloads (NodeId "5" might be different scene)
- GUIDs solve both problems

### Memory Management

**Potential Concern:** Storing complete spawn history for long games.

**Analysis:**
- Typical 5-day game: ~50 scenes, ~150 situations, ~300 choices
- Per node: ~500 bytes (properties + snapshots)
- Total: ~250KB for complete trace

**Acceptable:** 250KB is negligible for modern systems.

**Mitigation (If Needed):**
- Sliding window: Keep only last N days (purge old scenes)
- On-demand loading: Serialize to disk, load on viewer open
- Compression: GZip trace data if storing long-term

**Recommendation:** Start without limits, add mitigation only if memory becomes issue.

### Thread Safety

**Concern:** Spawning happens synchronously on main thread. Tracer also synchronous. No concurrency.

**Conclusion:** Thread safety NOT required. All operations on main thread.

**If Async Later:**
- Use ConcurrentBag for AllSceneNodes/AllSituationNodes/AllChoiceNodes
- Lock on modifications to node collections
- Immutable snapshots already thread-safe

### Serialization Support

**Current Requirement:** None. Trace lives in memory during game session.

**Future Enhancement:** Export to JSON.

**Serialization-Friendly Design:**
- All node properties are primitives, enums, strings, lists (JSON-serializable)
- No circular references (parent links use NodeId strings, not object references)
- No delegates, no functions, no runtime types

**Example JSON Output:**
```json
{
  "nodeId": "abc-123-guid",
  "sceneTemplateId": "investigation_archetype",
  "displayName": "Question the Witness",
  "category": "MainStory",
  "gameDay": 2,
  "placedLocation": {
    "name": "Guard Barracks",
    "purpose": "Civic",
    "privacy": "Public"
  },
  "situations": [
    {
      "nodeId": "def-456-guid",
      "name": "Interrogate Guard",
      "location": {
        "name": "Guard Barracks",
        "purpose": "Civic"
      }
    }
  ]
}
```

**Standard System.Text.Json serialization works with no custom converters.**

### Compatibility with HIGHLANDER Principle

**Principle:** No entity instance IDs in domain entities.

**Trace System Compliance:**
- Trace nodes have NodeIds (ALLOWED - trace nodes are NOT domain entities)
- Trace nodes reference domain entities via snapshots (no object references stored)
- Snapshots capture properties only (name, enums, primitives)
- No Scene.Id, Location.Id, NPC.Id stored anywhere

**Why Compliant:**
- Trace system is observational metadata, not domain model
- NodeIds identify trace nodes (spawn events), not domain entities
- Domain entities remain ID-free

**Analogy:** Logging system can assign log entry IDs without violating HIGHLANDER. Trace system assigns spawn event IDs without violating HIGHLANDER.

---

## PART 7: DEBUGGING USE CASES (DETAILED EXAMPLES)

### Use Case 1: Cascading Spawn Chain Investigation

**Scenario:** Developer notices 5 scenes spawned in rapid succession, wants to understand chain.

**Investigation:**
1. Open Spawn Graph Viewer
2. Filter to day where spawning occurred
3. Observe tree structure:
   ```
   Scene A4: Investigation Begins
   ├─ Situation: Question Witness
   │  └─ Choice: Interrogate (challenge: SUCCESS)
   │     └─ Spawned: Situation "Follow-up" (from SuccessSpawns)
   │        └─ Choice: Press Further (challenge: SUCCESS)
   │           └─ Spawned: Situation "Revelation" (from SuccessSpawns)
   │              └─ Choice: Confront with Evidence (instant)
   │                 └─ Spawned: Scene A5 "Showdown" (from ScenesToSpawn)
   ```

4. Examine each spawn:
   - Follow-up spawned from SuccessSpawns (cascading within scene)
   - Revelation spawned from SuccessSpawns (cascading again)
   - Scene A5 spawned from ChoiceReward.ScenesToSpawn (scene-level spawn)

5. Conclusion: Cascading success spawns created chain. Working as designed.

**What visualization reveals:**
- Spawn trigger type (success vs failure vs choice reward)
- Depth of cascading (3 levels deep)
- Endpoint of chain (final scene)

### Use Case 2: Placement Filter Debugging

**Scenario:** Scene always spawns at same location despite placement filter requesting variety.

**Investigation:**
1. Open Spawn Graph Viewer
2. Search for scene template (e.g., "service_negotiation")
3. Expand all instances (5 instances found)
4. Examine PlacementFilterSnapshot for each:
   ```
   Instance 1: LocationTags: [Inn, Public], SelectionStrategy: Random
   Instance 2: LocationTags: [Inn, Public], SelectionStrategy: Random
   Instance 3: LocationTags: [Inn, Public], SelectionStrategy: Random
   ```
5. Examine PlacedLocation for each:
   ```
   Instance 1: Rusty Flagon Inn
   Instance 2: Rusty Flagon Inn
   Instance 3: Rusty Flagon Inn
   ```

6. Conclusion: Only one location matches filter (only one inn in world).

**Fix:** Either broaden placement filter (allow Taverns too) OR ensure world generation creates multiple inns.

**What visualization reveals:**
- Placement filter configuration visible
- Actual selected locations visible
- Pattern of degenerate selection clear

### Use Case 3: Requirement Scaling Verification

**Scenario:** Player reports choice requirement "felt wrong", developer investigates formula.

**Investigation:**
1. Open Spawn Graph Viewer
2. Find scene with reported choice
3. Expand to ChoiceExecutionNode
4. Examine RequirementSnapshot:
   ```
   RequiredRapport: 12
   RequiredInsight: 0
   RequiredAuthority: 0
   ```
5. Examine parent SituationSpawnNode:
   ```
   NPC: Merchant (Demeanor: Neutral, SocialStanding: Notable)
   ```
6. Calculate expected requirement:
   - Base Rapport from archetype: 8
   - Neutral demeanor: 1.0× multiplier (no change)
   - Notable standing: 1.5× multiplier (higher threshold for influential NPCs)
   - Expected: 8 × 1.5 = 12
   - Actual: 12 ✓

7. Conclusion: Formula working correctly. "Felt wrong" is subjective, but math checks out.

**What visualization reveals:**
- Actual requirement value player saw
- NPC properties that affect scaling
- Can manually verify formula
- Can compare to similar situations to validate consistency

### Use Case 4: Missing Spawn Investigation

**Scenario:** Choice should spawn follow-up scene but doesn't appear.

**Investigation:**
1. Open Spawn Graph Viewer
2. Find scene with choice
3. Expand to ChoiceExecutionNode
4. Examine SpawnedSceneNodeIds:
   ```
   SpawnedSceneNodeIds: [] (EMPTY - nothing spawned)
   ```
5. Examine RewardSnapshot:
   ```
   ScenesToSpawn: [
     { SceneTemplateId: "follow_up_investigation", SpawnConditions: { RequiredTags: ["evidence_gathered"] } }
   ]
   ```
6. Examine PlayerState at execution time (need additional logging):
   ```
   Player tags: ["witness_questioned", "guard_bribed"]
   Missing: "evidence_gathered"
   ```

7. Conclusion: SpawnConditions failed (player didn't have required tag). Choice should have granted "evidence_gathered" tag but didn't.

**Fix:** Add StateApplication to choice reward granting "evidence_gathered" tag.

**What visualization reveals:**
- Spawn was CONFIGURED (ScenesToSpawn not empty)
- Spawn DIDN'T EXECUTE (SpawnedSceneNodeIds empty)
- SpawnConditions visible (required tag shown)
- Gap between expected and actual

---

## PART 8: ARCHITECTURAL ALIGNMENT

### Compliance with Codebase Principles

**✅ HIGHLANDER Principle:**
- Trace nodes use NodeIds (allowed - not domain entities)
- Domain entities remain ID-free
- Snapshots use properties, not object references

**✅ Domain Collection Principle:**
- List<SceneSpawnNode> collections (not Dictionary)
- LINQ queries for lookups (FirstOrDefault by NodeId)
- Explicit strongly-typed classes

**✅ Global Namespace Principle:**
- All trace classes in global namespace
- No custom namespaces (except Blazor components if required)

**✅ Explicit Property Principle:**
- Strongly-typed snapshot properties
- No string-based dynamic properties
- Explicit classes for each snapshot type

**✅ Synchronous Architecture:**
- All methods synchronous (no async/await)
- Hooks execute on main thread
- No concurrency concerns

**✅ Single Source of Truth:**
- ProceduralContentTracer is authoritative
- All hooks record to single tracer instance
- No distributed trace storage

**✅ Strongly Typed:**
- Explicit node classes (not generic nodes)
- Enum properties (not string flags)
- Compile-time type safety

### Integration with Existing Systems

**Dependency Injection:**
```csharp
// Startup.cs or equivalent
services.AddSingleton<ProceduralContentTracer>();
```

**Why Singleton:** One trace per game session, shared across all facades/services.

**Facade/Service Usage:**
```csharp
class SceneInstanceFacade
{
    private readonly ProceduralContentTracer tracer;

    public SceneInstanceFacade(ProceduralContentTracer tracer, /* other dependencies */)
    {
        this.tracer = tracer;
    }

    public Scene SpawnScene(/* parameters */)
    {
        // Existing spawn logic
        Scene scene = PackageLoaderFacade.LoadDynamicPackage(/* ... */);

        // NEW: Record spawn
        if (tracer.IsEnabled)
        {
            SceneSpawnNode node = tracer.RecordSceneSpawn(new SceneSpawnData
            {
                Scene = scene,
                SpawnContext = context,
                ParentChoiceNodeId = parentChoiceId
            });
        }

        return scene;
    }
}
```

**Zero Impact When Disabled:**
- Check `IsEnabled` before recording
- If disabled, tracer does nothing (no performance cost)
- Existing game logic unchanged

---

## CONCLUSION

This procedural content tracing system provides **comprehensive debugging visibility** into the spawn graph of scenes, situations, and choices in Wayfarer's procedurally-generated content system.

**What It Achieves:**

1. **Complete Spawn History** - Every scene, situation, and choice captured
2. **Relationship Tracking** - Parent-child links in both directions
3. **Property Snapshots** - Full context captured at spawn time
4. **Interactive Visualization** - Tree view with expand/collapse navigation
5. **Developer-Friendly** - Search, filter, trace, and export capabilities
6. **Architectural Compliance** - Follows all codebase principles strictly
7. **Zero Impact** - Optional system, no effect on game logic when disabled
8. **Extensible** - Easy to add properties, filters, views as needs evolve

**Implementation Priority:**

- **Phase 1:** Data structure (foundational, no visibility yet)
- **Phase 2:** Integration hooks (capture spawns, still no visibility)
- **Phase 3:** Basic visualization (MVP - functional debugging)
- **Phase 4:** Interactive features (enhanced usability)
- **Phase 5:** Polish (nice-to-have, not blocking)

**Next Steps:**

1. Review this design document for completeness
2. Validate integration points are correct (verify spawn flow in codebase)
3. Begin Phase 1 implementation (node classes, tracer service)
4. Test Phase 1 in isolation (unit tests, no UI yet)
5. Proceed to Phase 2 (hooks), verify spawns captured
6. Build Phase 3 (visualization), validate against real gameplay
7. Iterate on Phase 4-5 as time permits

This design is **ready for full implementation** with clear architectural decisions, detailed specifications, and comprehensive examples.

---

## APPENDIX: File Structure

**Domain/Services Layer:**
```
/src/Services/
├─ ProceduralContentTracer.cs
├─ SnapshotFactory.cs
/src/GameState/SpawnTrace/
├─ SceneSpawnNode.cs
├─ SituationSpawnNode.cs
├─ ChoiceExecutionNode.cs
├─ LocationSnapshot.cs
├─ NPCSnapshot.cs
├─ RouteSnapshot.cs
├─ PlacementFilterSnapshot.cs
├─ RequirementSnapshot.cs
├─ CostSnapshot.cs
├─ RewardSnapshot.cs
├─ SpawnTriggerType.cs (enum)
├─ SituationSpawnTriggerType.cs (enum)
```

**Frontend/Components Layer:**
```
/src/Pages/Components/SpawnTrace/
├─ SpawnGraphViewer.razor
├─ SpawnGraphViewer.razor.cs
├─ SceneNode.razor
├─ SceneNode.razor.cs
├─ SituationNode.razor
├─ SituationNode.razor.cs
├─ ChoiceNode.razor
├─ ChoiceNode.razor.cs
/src/wwwroot/css/
├─ spawn-trace.css
```

**Total Files:** ~20 files (10 backend classes, 8 Razor components, 1 CSS, 1 enum file)

---

END OF DESIGN DOCUMENT
