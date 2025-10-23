# Option A: Full Migration to Dynamic Content Architecture

## Executive Summary

**Approach**: Complete paradigm shift from "Content Unlocking" to "Content Creation"

**Core Transformation**: Replace Goal/Obstacle/Obligation filtering with Scene/Card spawning. Eliminate all boolean gates, replace with resource gates. Transform investigation phases into scene chains.

**Target Architecture**: See `DYNAMIC_CONTENT_PRINCIPLES.md` for complete specification.

**Supporting Analysis**: Five comprehensive architectural analysis documents created by specialized agents (see References section).

---

## Agent Analysis: Problems Identified in Current System

Five specialized agents performed deep codebase analysis and identified critical architectural violations:

### 1. Lead Architect: Architectural Violations
**Problems Found**:
- Goal entity conflates template AND instance (architectural confusion)
- No clear separation between "what can exist" vs "what currently exists"
- Runtime references violate single source of truth (PlacementLocation, ParentObstacle cached objects)

**Impact**: Cannot distinguish between "all possible goals" and "currently active goals" without filtering logic.

**Document**: `SCENE-CARD-ARCHITECTURE-ANALYSIS.md`

### 2. Domain Developer: Data Layer Violations
**Problems Found**:
- Runtime state stored in JSON (`isAvailable`, `isCompleted`) - wrong layer
- Weak typing (`MemoryFlag.CreationDay` as `object` instead of `int`)
- 100+ properties analyzed, found template/instance mixing throughout

**Impact**: JSON files contain both definition AND state, making templates non-reusable.

**Document**: Property migration matrix embedded in agent output

### 3. Senior Developer: Performance Violations
**Problems Found**:
- `ObstacleGoalFilter` performs filtering on EVERY location screen refresh
- Continuous boolean checking (`IsAvailable && !IsCompleted`) creates overhead
- 6 core services with complex dependency graph centered on filtering

**Impact**: Performance degrades as content grows (O(n) filtering on every UI update).

**Document**: Service layer analysis embedded in agent output

### 4. Domain Developer: Content Complexity
**Problems Found**:
- Tutorial content is simple (2 goals, 1 obligation, 0 obstacles) but architecture supports massive complexity
- No memory flags, no availability conditions in tutorial, yet system built for them
- Boolean gate infrastructure unused but pervasive

**Impact**: Architecture overbuilt for simple needs, adds unnecessary complexity.

**Document**: `CONTENT-MIGRATION-ANALYSIS.md`

### 5. Lead Architect: Data Flow Violations
**Problems Found**:
- Placement duplication (`Goal.PlacementNpcId` + `NPC.ActiveGoalIds` both track same relationship)
- Filtering tries to separate template from instance via runtime boolean checks
- Parser-JSON-Entity triangle has runtime state leaking into JSON layer

**Impact**: Dual sources of truth for placement, filtering complexity to maintain consistency.

**Document**: `SCENE-CARD-DATA-FLOW-ARCHITECTURE.md`

---

## Scene Architecture: Intentional Design Patterns

Scene system is a COMPLETE, COHERENT architecture with these FIRST-CLASS native patterns:

### Pattern 1: Cost Reduction Modifiers

**What it is**: Equipment and Cards dynamically reduce Scene entry costs.

**How it works**:
- Scene has base entry costs (Focus: 5, Stamina: 2, Coins: 10)
- Player owns Equipment "Climbing Harness"
- Harness matches Scene.ContextTags ("Climbing")
- Entry cost recalculated: Stamina 2 → 1 (50% reduction)

**Why intentional**: Makes equipment ECONOMIC MODIFIERS, not boolean unlocks. Player sees EXACT cost before and after equipment. Creates strategic equipment choices.

**Design principle**: Transparent resource gates (DYNAMIC_CONTENT_PRINCIPLES.md Principle 4)

### Pattern 2: Spawn Chain Cascades

**What it is**: Scene completion spawns follow-up Scenes, creating branching narratives.

**How it works**:
- Scene "Question Suspect" completes successfully
- Success outcome defines: `SpawnScenes: ["scene_suspect_confesses", "scene_search_evidence"]`
- Two new Scenes appear at different locations
- Player chooses which to pursue first

**Why intentional**: Creates branching investigations without pre-authored phase gates. Content emerges from player actions, not unlocked by conditions.

**Design principle**: Actions create content (DYNAMIC_CONTENT_PRINCIPLES.md Principle 1)

### Pattern 3: Expiration Lifecycle

**What it is**: Scenes disappear after time/events, creating urgency.

**How it works**:
- Scene "Elena's Request" spawned Monday segment 8
- Expiration rule: `ExpiresAfterSegments: 12`
- Thursday segment 20 arrives
- Scene automatically removed from GameWorld.ActiveScenes

**Why intentional**: Creates impossible choices (cannot do everything). Missed content is LOST CONTENT. No completionism.

**Design principle**: Time creates and expires scenes (DYNAMIC_CONTENT_PRINCIPLES.md Principle 8)

### Pattern 4: Resource Gating

**What it is**: ALL scene access controlled by resources (Time, Focus, Stamina, Health, Coins), ZERO boolean conditions.

**How it works**:
- Scene "Guild Meeting" requires: Coins 5, Focus 2, Time 1 segment
- Player has: Coins 3, Focus 5, Time available
- Player SEES scene but CANNOT AFFORD entry
- No "you haven't done X yet" message - just "need 2 more coins"

**Why intentional**: Perfect information. Player always knows EXACTLY what's needed. No hidden gates. Strategic resource management.

**Design principle**: Resources gate participation (DYNAMIC_CONTENT_PRINCIPLES.md Principle 1)

### Pattern 5: Transparent Difficulty

**What it is**: ALL paths to reduce difficulty visible upfront, no hidden conditions.

**How it works**:
- Scene shows: Base difficulty 8
- DifficultyModifiers visible: "Understanding 5: -3 difficulty", "Equipment 'Scholar's Glasses': -2 difficulty"
- Player sees: "With Understanding 5 and Scholar's Glasses, difficulty becomes 3"
- No surprises, no hidden unlocks

**Why intentional**: Board game transparency. Player can CALCULATE optimal path before committing. No trial-and-error required.

**Design principle**: Perfect information (DYNAMIC_CONTENT_PRINCIPLES.md Principle 11)

---

## The Paradigm Shift

### Current Architecture: Content Unlocking (Filtering)

**Lifecycle**:
```
Parse JSON → Store ALL goals → Filter by conditions → Display subset
```

**Problems** (from agent analysis):
- Runtime state stored in JSON (`isAvailable`, `isCompleted`)
- Goal entity is both template AND instance
- Continuous filtering on every UI refresh (performance)
- Boolean gates everywhere (IsAvailable checks)
- Placement duplication (PlacementNpcId + ActiveGoalIds)

**Entity Hierarchy**:
```
Obligation (mystery structure)
  ↓ spawns
Obstacle (strategic barrier with Intensity property)
  ↓ contains
Goal (approach to overcome, can reduce Obstacle.Intensity)
  ↓ appears at
Location/NPC (placement)
  ↓ player filters
Challenge (if IsAvailable)
```

### Target Architecture: Content Creation (Spawning)

**Lifecycle**:
```
Parse templates → Store definitions → Action spawns Scene instance → Display all active
```

**Solutions**:
- No runtime state in JSON (only templates)
- SceneTemplate (definition) separate from Scene (instance)
- No filtering (presence = availability)
- Resource gates replace boolean checks
- Single source of placement (ActiveSceneIds only)

**Entity Hierarchy**:
```
Player Action
  ↓ creates
Scene (ephemeral opportunity at location)
  ↓ has
Entry Costs (resource gate)
  ↓ player affords
Challenge
  ↓ completion produces
Outcomes (spawn new scenes, grant cards)
  ↓ cascade
Scene Chains (investigations, narratives)
```

---

## New Entity Architecture

### Core Entities

**SceneTemplate** (Replaces: Goal entity as template)
- Pure definition loaded from JSON
- NO runtime state (no IsAvailable, IsCompleted)
- Properties: Id, Title, Description, ChallengeType, DeckId, EntryCosts, VictoryConditions, Outcomes, ContextTags, DifficultyModifiers
- Stored in: `GameWorld.SceneTemplates` (reference library)

**Scene** (Replaces: Goal entity as instance)
- Runtime instance spawned by actions
- Properties: Id, TemplateId, LocationId, NpcId, State (Available/Active/Completed/Expired), CreatedSegment, SpawnedByActionId
- Stored in: `GameWorld.ActiveScenes` + `Location.ActiveSceneIds`

**ActionDefinition** (NEW concept - no equivalent in old system)
- Defines WHEN/WHERE scenes spawn
- Properties: Id, Title, SpawnSceneTemplateId, SpawnLocation (NPC/Location/Route/Global), SpawnConditions, DespawnConditions
- Stored in: `GameWorld.ActionDefinitions` (reference library)

**CardTemplate** (Replaces: MemoryFlag)
- Defines type of knowledge/memory player can gain
- Properties: Id, Title, Description, Category (Knowledge/Experience/Relationship/Discovery/Quest), Effects
- Stored in: `GameWorld.CardTemplates` (reference library)

**Card** (Replaces: MemoryFlag instance)
- Runtime instance owned by player
- Properties: Id, TemplateId, CreationSegment, SourceSceneId, Effects
- Stored in: `Player.OwnedCards`

**ObligationDefinition** (Replaces: Obligation entity)
- Multi-phase investigation structure
- Properties: Id, Title, Description, DiscoverySceneTemplateId, SceneChainTemplates (phases), CompletionRewards
- Stored in: `GameWorld.ObligationDefinitions`

**ActiveObligation** (NEW concept - runtime tracking)
- Runtime instance tracking player progress
- Properties: DefinitionId, CurrentPhase, DeadlineSegment, IsFailed
- Stored in: `Player.ActiveObligations`

### Complete Deletions

**Obstacle Entity** - DELETED ENTIRELY
- Reason: Strategic barrier container with boolean gate patterns (Intensity reduction gates Goal visibility)
- Replacement: Scenes spawn from actions, no container needed

**Goal Entity** - REPLACED BY TWO
- Reason: Conflates template and instance (architectural violation)
- Replacement: SceneTemplate (definition) + Scene (instance)

**MemoryFlag Entity** - REPLACED
- Reason: Weak typing (`object CreationDay`), invisible boolean state
- Replacement: Card (strong typing, visible resource)

---

## Entity Property Mappings

### Goal → SceneTemplate + Scene

**Direct Mappings** (40 properties):
- Id, Name → Title, Description, SystemType → ChallengeType, DeckId, PlacementLocationId → LocationId, PlacementNpcId → NpcId, Costs → EntryCosts, DifficultyModifiers, GoalCards → VictoryConditions, Category, ConnectionType, ContextTags

**Renamed** (8 properties):
- PlacementLocationId → LocationId (clearer semantic)
- GoalCards → VictoryConditions (clearer purpose)
- ObligationId → SourceObligationId (audit trail, not ownership)

**Transformed** (6 properties):
- DeleteOnSuccess (bool) → ExpirationRule (enum: OnCompletion/AfterTime/Never)
- Status/IsAvailable/IsCompleted → State (enum: Available/Active/Completed/Expired)
- PhaseDefinitions → SceneChainTemplates

**Deleted** (22 properties):
- Runtime references: PlacementLocation, PlacementNpc, Obligation, ParentObstacle (violate single source of truth)
- Obstacle-related: ConsequenceType, ResolutionMethod, TransformDescription, PropertyReduction, SetsRelationshipOutcome (Obstacle deleted)
- Instance flags: IsAvailable, IsCompleted (derived from State enum)

**New Properties** (10):
- Scene.TemplateId (links to SceneTemplate)
- Scene.CreatedSegment (when spawned)
- Scene.SpawnedByActionId (audit trail)
- Scene.ExpirationRule (when removed)
- Card.Category, Card.Template, Card.SourceScene, Card.Effects

### MemoryFlag → Card

**Strong Typing Fixes**:
- CreationDay: `object` → `int` (CreationSegment)
- ExpirationDay: `object` → `int?` (null = permanent)

**New Properties**:
- Category: CardCategory enum (Knowledge/Experience/Relationship/Discovery/Quest)
- Template: CardTemplate reference
- SourceScene: string (which scene granted this)
- Effects: List<CardEffect> (cost reductions, passive bonuses)

---

## Implementation Phases

### Phase 1: Entity Layer (Week 1)

**Goal**: Create all new entities, no old entity modification.

**Deliverables**:
1. **Domain Entities**:
   - `SceneTemplate.cs` (21 properties)
   - `Scene.cs` (12 properties)
   - `ActionDefinition.cs` (9 properties)
   - `CardTemplate.cs` (8 properties)
   - `Card.cs` (9 properties)
   - `ObligationDefinition.cs` (refactor Obligation)
   - `ActiveObligation.cs` (runtime tracking)

2. **Supporting Types**:
   - `SceneState` enum
   - `EntryCosts` class
   - `SceneOutcome` class
   - `CardCategory` enum
   - `CardEffect` class

3. **GameWorld Extensions**:
   - Add `List<SceneTemplate> SceneTemplates`
   - Add `List<Scene> ActiveScenes`
   - Add `List<ActionDefinition> ActionDefinitions`
   - Add `List<CardTemplate> CardTemplates`
   - Keep existing `List<Goal> Goals` (untouched)

**Testing**: Serialize/deserialize, verify strong typing.

**Risk**: ZERO - No existing code touched.

---

### Phase 2: Data Layer (Week 2)

**Goal**: Parse SceneTemplates/Actions/Cards from JSON.

**Deliverables**:
1. **DTOs**:
   - `SceneTemplateDTO.cs`
   - `ActionDefinitionDTO.cs`
   - `CardTemplateDTO.cs`
   - Verify JSON field names MATCH C# property names (no JsonPropertyName)

2. **Parsers**:
   - `SceneTemplateParser.cs`
   - `ActionDefinitionParser.cs`
   - `CardTemplateParser.cs`

3. **JSON Content** (Manual conversion of tutorial):
   - `scene_templates.json` (2 scenes from tutorial)
   - `action_definitions.json` (9 actions)
   - `card_templates.json` (memory flags converted)

4. **Parser Integration**:
   - Update `GameWorldParser.cs` to call new parsers

**Testing**: Parse JSON, verify templates loaded, verify NO runtime state in JSON.

**Risk**: LOW - Additive only.

---

### Phase 3: Spawning System (Week 3)

**Goal**: Implement scene creation from action completion.

**Deliverables**:
1. **SceneSpawner Service**:
   - `SpawnScene(ActionDefinition, Location, NPC)` → Creates Scene instance
   - Evaluates spawn conditions
   - Adds to `GameWorld.ActiveScenes`
   - Adds to `Location.ActiveSceneIds`

2. **Action Execution Integration**:
   - Update `GameFacade.ProcessIntent()` to trigger spawning
   - Log spawning events in `GameWorld.SceneHistory`

3. **Scene Lifecycle**:
   - State transitions: Available → Active → Completed/Expired
   - `SceneExpirationManager` removes expired

**Testing**: Trigger actions, verify scenes spawn at correct locations.

**Risk**: MEDIUM - First integration with game loop.

---

### Phase 4: Query Layer (Week 4)

**Goal**: Replace goal filtering with scene lookup.

**Deliverables**:
1. **Scene Query Service**:
   - `GetActiveScenes(Location)` → Simple lookup, NO filtering
   - `GetActiveScenes(NPC)` → Simple lookup

2. **UI Building**:
   - `LocationFacade.BuildSceneCards(scenes)`
   - `SceneCardViewModel`
   - Display both Goals AND Scenes temporarily

**Testing**: Verify scenes display correctly.

**Risk**: LOW - Parallel to existing Goal display.

---

### Phase 5: Challenge Integration (Week 5)

**Goal**: Tactical systems accept Scene.Id.

**Deliverables**:
1. **Facade Refactoring** (ONE at a time):
   - **MentalFacade** first
   - **PhysicalFacade** second
   - **SocialFacade** last

2. **Scene Completion Handler**:
   - `SceneCompletionHandler.CompleteScene(Scene)`
   - Apply outcomes (rewards, spawn cascading scenes, grant cards)
   - Set State = Completed
   - Remove from ActiveSceneIds if ExpirationRule

3. **Challenge Context**:
   - Read from Scene properties

**Testing**: Start/complete challenges via scenes.

**Risk**: HIGH - Critical gameplay systems.

---

### Phase 6: UI Switchover (Week 6)

**Goal**: UI displays Scenes only.

**Deliverables**:
1. **LocationFacade**:
   - Call `GetActiveScenes()` instead of `GetAvailableGoals()`
   - Build `SceneCardViewModel`

2. **UI Components**:
   - `LocationContent.razor` receives scene models

3. **Intent Handling**:
   - `CommitToSceneIntent` replaces `CommitToGoalIntent`

**Testing**: Full end-to-end using scenes.

**Risk**: HIGH - User-visible.

---

### Phase 7: Goal Deletion (Week 7)

**Goal**: Delete ALL Goal/Obstacle/MemoryFlag code. SCORCHED EARTH.

**Deletions**:
1. **Entity Files**: `Goal.cs`, `Obstacle.cs`, `MemoryFlag.cs`
2. **Service Files**: `ObstacleGoalFilter.cs`, `GoalCompletionHandler.cs`, `ObstacleIntensityCalculator.cs`
3. **Parser/DTO Files**: `GoalDTO.cs`, `ObstacleDTO.cs`, `GoalParser.cs`
4. **Properties**: `GameWorld.Goals`, `Location.ActiveGoalIds`, `NPC.ActiveGoalIds`, `Player.Memories`
5. **Methods**: All goal-related view model building

**Verification**:
```bash
grep -r "\.Goals" src/          # Should return ZERO
grep -r "ActiveGoalIds" src/    # Should return ZERO
grep -r "GoalDTO" src/          # Should return ZERO
grep -r "ObstacleGoalFilter" src/  # Should return ZERO
```

**Risk**: CRITICAL - Complete deletion. Git tag before: `pre-goal-deletion`

---

### Phase 8: Obligation Refactoring (Week 8)

**Goal**: Adapt Obligation system to Scene chains.

**Deliverables**:
1. **ObligationDefinition**:
   - Convert `PhaseDefinitions` → `SceneChainTemplates`

2. **Obligation Discovery**:
   - Spawns discovery scene (not obstacles)

3. **Obligation Progress**:
   - Track via scene completion

**Testing**: Complete full investigation chain.

**Risk**: MEDIUM - Complex dependency chains.

---

## Critical Path

```
Phase 1 (Entities) ← FOUNDATION
  ↓
Phase 2 (Data Layer) ← PARSING
  ↓
Phase 3 (Spawning) ← CREATION
  ↓
Phase 4 (Query) + Phase 5 (Challenges) ← PARALLEL
  ↓
Phase 6 (UI Switchover) ← USER-VISIBLE
  ↓
Phase 7 (Deletion) ← SCORCHED EARTH
  ↓
Phase 8 (Obligations) ← FINAL
```

**Timeline**: 8 weeks total

**Parallelization**: Phase 4-5 can run simultaneously

---

## Success Criteria

### Technical Metrics

**Code Quality**:
- ✅ Zero references to Goal/Obstacle/MemoryFlag entities
- ✅ Zero boolean condition checking
- ✅ Zero weak typing
- ✅ Build succeeds 0 warnings, 0 errors

**Performance**:
- ✅ Scene queries within 10% of current
- ✅ Spawning overhead < 5ms per scene
- ✅ Memory usage within 15%

**Architecture**:
- ✅ SceneTemplate separate from Scene instance
- ✅ All content spawned by actions
- ✅ All gates are resource costs
- ✅ All knowledge is cards

### Design Metrics

**Scene Architecture Completeness**:
- ✅ Cost reduction modifiers working (Pattern 1)
- ✅ Spawn chain cascades working (Pattern 2)
- ✅ Expiration lifecycle working (Pattern 3)
- ✅ Resource gating working (Pattern 4)
- ✅ Transparent difficulty working (Pattern 5)

**Strategic Depth**:
- ✅ Resource competition evident
- ✅ Opportunity cost measurable
- ✅ Multiple valid paths
- ✅ No soft-locks

---

## Risk Assessment

### HIGH RISK: Phase 7 Deletion

**Why**: Complete removal of working system.

**Mitigation**:
- Git tag before Phase 7: `pre-goal-deletion`
- Phase 6 must be 100% validated (2+ weeks testing)
- Full regression suite passing

**Rollback**: `git revert` to tag, loses 1 week of work.

### MEDIUM RISK: Phase 5 Challenge Integration

**Why**: Tactical systems critical.

**Mitigation**:
- Refactor ONE system at a time
- Test each in isolation

### LOW RISK: Phases 1-4

**Why**: Additive only.

---

## Timeline Summary

| Phase | Duration | Key Deliverable | Risk |
|-------|----------|-----------------|------|
| 1: Entities | 1 week | SceneTemplate, Scene, Card entities | ZERO |
| 2: Data Layer | 1 week | Parsers, tutorial JSON | LOW |
| 3: Spawning | 1 week | SceneSpawner, action integration | MEDIUM |
| 4: Query Layer | 1 week | GetActiveScenes, UI building | LOW |
| 5: Challenges | 1 week | Facade refactoring | HIGH |
| 6: UI Switchover | 1 week | Scene-only display | HIGH |
| 7: Deletion | 1 week | Scorched earth removal | CRITICAL |
| 8: Obligations | 1 week | Investigation chains | MEDIUM |
| **TOTAL** | **8 weeks** | **Complete transformation** | - |

---

## References

**Supporting Analysis Documents** (created by agents):

1. **`SCENE-CARD-ARCHITECTURE-ANALYSIS.md`** - Entity architecture violations identified (Lead Architect)

2. **Property Migration Matrix** (embedded in agent output) - 100+ property mappings, weak typing fixes (Domain Developer)

3. **Service Layer Analysis** (embedded in agent output) - Dependency graph, refactoring sequence (Senior Developer)

4. **`CONTENT-MIGRATION-ANALYSIS.md`** - Tutorial content analysis, migration estimate (Domain Developer)

5. **`SCENE-CARD-DATA-FLOW-ARCHITECTURE.md`** - Data flow violations, parser triangle analysis (Lead Architect)

**Core Specification**:
- **`DYNAMIC_CONTENT_PRINCIPLES.md`** - Target architecture principles (10 principles)

**Current Architecture**:
- **`ARCHITECTURE.md`** - Existing system documentation
- **`CLAUDE.md`** - Development principles and constraints

---

**Document Version**: 3.0 (Clean Intentional Design)
**Last Updated**: 2025-10-23
**Status**: Architectural plan complete, ready for Phase 1 implementation
