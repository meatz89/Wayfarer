# Option A: Full Migration to Dynamic Content Architecture

## Executive Summary

**Approach**: Complete paradigm shift from "Content Unlocking" to "Content Creation"

**Core Transformation**: Replace Goal/Obstacle/Obligation filtering with Scene/Card spawning. Eliminate all boolean gates, replace with resource gates. Transform investigation phases into scene chains.

**Target Architecture**: See `DYNAMIC_CONTENT_PRINCIPLES.md` for complete specification.

**Supporting Analysis**: Five comprehensive architectural analysis documents created by specialized agents (see References section).

---

## Agent Analysis Summary

Five specialized agents performed deep codebase analysis:

### 1. Lead Architect: Entity Architecture Analysis
**Finding**: Scene ≠ Goal semantically. Goals are hierarchical children of Obstacles (multiple approaches to one barrier). Scenes are standalone opportunities. The Obstacle → Goal hierarchy creates graduated difficulty patterns that pure Scene architecture doesn't inherently support.

**Recommendation**: Hybrid architecture - use BOTH systems where each excels. Goals for structured challenges with weakening chains, Scenes for dynamic spawning with expiration.

**Document**: `SCENE-CARD-ARCHITECTURE-ANALYSIS.md`

### 2. Domain Developer: Property Migration Matrix
**Finding**: 100+ properties analyzed across Goal/Obstacle/Obligation/MemoryFlag entities. 60% direct mappings, 25% transformations, 15% intentional deletions. Only 2 weak-typed properties found (MemoryFlag.CreationDay/ExpirationDay as `object`).

**Critical Insight**: MemoryFlag → Card is ONLY complete 1:1 replacement with zero loss. Goal → Scene loses graduated difficulty and weakening chains.

**Document**: Property migration matrix embedded in agent output

### 3. Senior Developer: Service Layer Dependencies
**Finding**: 6 core services, 3 tactical facades, complex dependency graph. ObstacleGoalFilter performs filtering on EVERY location screen refresh. GoalCompletionHandler is single point of goal completion (critical path).

**Refactoring Sequence**: Phase 1 (Scene infrastructure) → Phase 2 (Query layer) → Phase 3 (Tactical systems) → Phase 4 (UI switchover) → Phase 5 (Goal deletion). Phases 2-3 can run in parallel.

**Document**: Service refactoring plan embedded in agent output

### 4. Domain Developer: Content Structure Analysis
**Finding**: Tutorial content is TRIVIAL - only 2 goals, 1 obligation, 0 obstacles. No memory flags, no availability conditions, no boolean logic. All goals `isAvailable: true`. Perfect test case.

**Migration Estimate**: 3 hours to manually convert tutorial. Full content complexity unknown until analyzed.

**Document**: `CONTENT-MIGRATION-ANALYSIS.md`

### 5. Lead Architect: Data Flow Transformation
**Finding**: Current flow has architectural violations - runtime state in JSON (`isAvailable`, `isCompleted`), Goal is both template AND instance (confusion), placement duplication (PlacementNpcId + ActiveGoalIds).

**New Flow**: SceneTemplates (pure definitions) + Scene instances (runtime) + SceneSpawner (creation service). Clean separation eliminates filtering complexity.

**Document**: `SCENE-CARD-DATA-FLOW-ARCHITECTURE.md`

---

## The Core Architectural Decision

### Agent Recommendation vs Pure Scene Architecture

**Lead Architect recommended HYBRID**: Keep Goal/Obstacle for structured challenges, add Scene/Card for dynamic content.

**Why reject hybrid?**
- Maintains dual paradigms (content unlocking AND content creation)
- Perpetuates boolean gate patterns alongside resource gates
- Creates architectural debt (two systems to maintain)
- Defeats purpose of paradigm shift

**Pure Scene architecture requires**:
- New entities to replace Obstacle hierarchy patterns
- Graduated difficulty becomes cost reduction modifiers
- Weakening chains become scene spawning chains
- Property-based gating becomes resource thresholds

**Decision**: Proceed with PURE Scene architecture. Accept that we're recreating Obstacle/Goal patterns under different semantics for better long-term consistency.

---

## The Paradigm Shift Explained

### Current Architecture: Content Unlocking

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

### Target Architecture: Content Creation

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

### Core Entities (Replace Goal/Obstacle/Obligation)

**SceneTemplate** (Replaces: Goal entity as template)
- Pure definition loaded from JSON
- NO runtime state (no IsAvailable, IsCompleted)
- Properties: Id, Title, Description, ChallengeType, DeckId, EntryCosts, VictoryConditions, Outcomes
- Stored in: `GameWorld.SceneTemplates` (reference library)

**Scene** (Replaces: Goal entity as instance)
- Runtime instance spawned by actions
- Properties: Id, TemplateId, LocationId, NpcId, State (Available/Active/Completed/Expired), CreatedSegment, SpawnedByActionId
- Stored in: `GameWorld.ActiveScenes` + `Location.ActiveSceneIds`

**ActionDefinition** (NEW concept)
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

**ActiveObligation** (NEW concept)
- Runtime instance tracking player progress
- Properties: DefinitionId, CurrentPhase, DeadlineSegment, IsFailed
- Stored in: `Player.ActiveObligations`

### What Gets Deleted Entirely

**Obstacle Entity** - COMPLETE DELETION
- Strategic barrier container with Intensity property
- Multiple Goals that reduce Intensity
- Graduated weakening chain pattern
- **Why deleted**: Boolean gate pattern disguised as numbers. Scenes spawn from actions, not contained by barriers.

**Goal Entity** - REPLACED BY TWO
- Current Goal is template + instance (confused)
- Becomes SceneTemplate (definition) + Scene (instance)

**MemoryFlag Entity** - REPLACED
- Weak typing (`object CreationDay`)
- Invisible boolean state
- Becomes Card (strong typing, visible resource)

---

## Entity Property Mappings

### Goal → SceneTemplate + Scene

**Direct Mappings** (40 properties):
- Id, Name → Title, Description, SystemType → ChallengeType, DeckId, PlacementLocationId → LocationId, PlacementNpcId → NpcId, Costs → EntryCosts, DifficultyModifiers, GoalCards → VictoryConditions, Category, ConnectionType

**Renamed** (8 properties):
- PlacementLocationId → LocationId (clearer semantic)
- GoalCards → VictoryConditions (clearer purpose)
- ObligationId → SourceObligationId (audit trail, not ownership)

**Transformed** (6 properties):
- DeleteOnSuccess (bool) → ExpirationRule (enum: OnCompletion/AfterTime/Never)
- Status/IsAvailable/IsCompleted → State (enum: Available/Active/Completed/Expired)
- PhaseDefinitions → SceneChainTemplates

**Deleted** (22 properties):
- Runtime references: PlacementLocation, PlacementNpc, Obligation, ParentObstacle (use GameWorld lookups)
- Obstacle-related: ConsequenceType, ResolutionMethod, TransformDescription, PropertyReduction, SetsRelationshipOutcome
- Instance flags: IsAvailable, IsCompleted (derived from State)

**New Properties** (10):
- Scene.TemplateId (links to SceneTemplate)
- Scene.CreatedSegment (when spawned)
- Scene.SpawnedByActionId (audit trail)
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
1. **Domain Entities** (pure classes, no logic):
   - `SceneTemplate.cs` (21 properties from agent analysis)
   - `Scene.cs` (12 properties for runtime instance)
   - `ActionDefinition.cs` (9 properties for spawning rules)
   - `CardTemplate.cs` (8 properties for card definitions)
   - `Card.cs` (9 properties for player-owned cards)
   - `ObligationDefinition.cs` (refactor existing Obligation)
   - `ActiveObligation.cs` (runtime tracking)

2. **Supporting Types**:
   - `SceneState` enum (Available/Active/Completed/Expired)
   - `EntryCosts` class (Time/Focus/Stamina/Health/Coins)
   - `SceneOutcome` class (Rewards, SpawnedScenes, GrantedCards)
   - `CardCategory` enum
   - `CardEffect` class

3. **GameWorld Extensions**:
   - Add `List<SceneTemplate> SceneTemplates`
   - Add `List<Scene> ActiveScenes`
   - Add `List<ActionDefinition> ActionDefinitions`
   - Add `List<CardTemplate> CardTemplates`
   - Keep existing `List<Goal> Goals` (untouched)

**Testing**: Serialize/deserialize new entities, verify strong typing.

**Risk**: ZERO - No existing code touched.

---

### Phase 2: Data Layer (Week 2)

**Goal**: Parse SceneTemplates/Actions/Cards from JSON.

**Deliverables**:
1. **DTOs**:
   - `SceneTemplateDTO.cs`
   - `ActionDefinitionDTO.cs`
   - `CardTemplateDTO.cs`
   - Verify JSON field names MATCH C# property names (no JsonPropertyName workarounds)

2. **Parsers**:
   - `SceneTemplateParser.cs` (convert DTO → SceneTemplate)
   - `ActionDefinitionParser.cs` (convert DTO → ActionDefinition)
   - `CardTemplateParser.cs` (convert DTO → CardTemplate)

3. **JSON Content** (Manual conversion of tutorial):
   - `scene_templates.json` (2 scenes from tutorial goals)
   - `action_definitions.json` (9 actions: 3 location + 3 player + 2 goals + 1 obligation)
   - `card_templates.json` (memory flags converted)

4. **Parser Integration**:
   - Update `GameWorldParser.cs` to call new parsers
   - Load templates into GameWorld collections

**Testing**: Parse JSON, verify templates loaded correctly, verify NO runtime state in JSON.

**Risk**: LOW - Additive only, doesn't break goal parsing.

---

### Phase 3: Spawning System (Week 3)

**Goal**: Implement scene creation from action completion.

**Deliverables**:
1. **SceneSpawner Service**:
   - `SpawnScene(ActionDefinition action, Location location, NPC npc)` → Creates Scene instance
   - Evaluates spawn conditions (time blocks, prerequisites)
   - Adds Scene to `GameWorld.ActiveScenes`
   - Adds Scene.Id to `Location.ActiveSceneIds` or `NPC.ActiveSceneIds`

2. **Action Execution Integration**:
   - Update `GameFacade.ProcessIntent()` to trigger spawning
   - After action completion: Query ActionDefinitions, evaluate spawn rules, call SpawnScene
   - Log spawning events in `GameWorld.SceneHistory`

3. **Scene Lifecycle**:
   - `Scene.State` transitions: Available → Active → Completed/Expired
   - `SceneExpirationManager` removes expired scenes

**Testing**: Manually trigger actions, verify scenes spawn at correct locations, verify ActiveSceneIds updated.

**Risk**: MEDIUM - First integration with game loop, but isolated to new system.

---

### Phase 4: Query Layer (Week 4)

**Goal**: Replace goal filtering with scene lookup.

**Deliverables**:
1. **Scene Query Service**:
   - `GetActiveScenes(Location location)` → Returns `location.ActiveSceneIds.Select(id => GameWorld.ActiveScenes[id])`
   - `GetActiveScenes(NPC npc)` → Returns `npc.ActiveSceneIds.Select(id => GameWorld.ActiveScenes[id])`
   - NO filtering logic (presence = availability)

2. **UI Building**:
   - `LocationFacade.BuildSceneCards(scenes)` → Parallel to existing `BuildGoalCards(goals)`
   - `SceneCardViewModel` → Parallel to `GoalCardViewModel`
   - Display both Goals AND Scenes temporarily (verify scenes render correctly)

3. **Testing**:
   - Spawn scenes manually
   - Verify UI displays scenes correctly
   - Verify scene costs/rewards visible

**Testing**: Spawn test scenes, verify UI displays correctly, verify no filtering needed.

**Risk**: LOW - Parallel to existing Goal display, doesn't break anything.

---

### Phase 5: Challenge Integration (Week 5)

**Goal**: Tactical systems accept Scene.Id instead of Goal.Id.

**Deliverables**:
1. **Facade Refactoring** (ONE system at a time):
   - **MentalFacade** first (simplest):
     - `StartSession(sceneId, ...)` → Lookup Scene, read Scene.DeckId
     - `CreateContext(Scene scene)` → Use Scene.EntryCosts, Scene.VictoryConditions
   - **PhysicalFacade** second:
     - Same pattern as Mental
   - **SocialFacade** last (most complex):
     - Same pattern, plus NPC context handling

2. **Scene Completion Handler**:
   - `SceneCompletionHandler.CompleteScene(Scene scene)`
   - Apply Scene.Outcomes (rewards, spawn cascading scenes, grant cards)
   - Set Scene.State = Completed
   - Remove from ActiveSceneIds if ExpirationRule = OnCompletion

3. **Challenge Context**:
   - All three systems read from Scene properties (not Goal)
   - VictoryConditions still work identically (same structure as GoalCards)

**Testing**: Start challenges via scenes, complete them, verify outcomes applied.

**Risk**: HIGH - Tactical systems are critical gameplay. Test exhaustively in dev environment.

---

### Phase 6: UI Switchover (Week 6)

**Goal**: UI displays Scenes only, hides Goals.

**Deliverables**:
1. **LocationFacade.GetLocationContentViewModel()**:
   - Call `GetActiveScenes()` instead of `GetAvailableGoals()`
   - Build `SceneCardViewModel` instead of `GoalCardViewModel`
   - Return scene data to UI

2. **UI Components**:
   - `LocationContent.razor` receives scene models
   - `SceneCard.razor` component (or rename GoalCard.razor to CardDisplay.razor)

3. **Intent Handling**:
   - `CommitToSceneIntent` replaces `CommitToGoalIntent`
   - `GameFacade.ProcessIntent()` routes to Scene handling

**Testing**: Full end-to-end flow using scenes (spawn → display → start challenge → complete → cascade).

**Risk**: HIGH - Visible to players. Must work perfectly.

---

### Phase 7: Goal Deletion (Week 7)

**Goal**: Delete ALL Goal/Obstacle/Obligation code. SCORCHED EARTH.

**Deletions** (from service layer analysis):
1. **Entity Files** - COMPLETE DELETION:
   - `Goal.cs` (188 lines)
   - `Obstacle.cs` (95 lines)
   - `MemoryFlag.cs` (13 lines)

2. **Service Files** - COMPLETE DELETION:
   - `ObstacleGoalFilter.cs` (entire file)
   - `GoalCompletionHandler.cs` (entire file)
   - `ObstacleIntensityCalculator.cs` (entire file)

3. **Parser/DTO Files** - COMPLETE DELETION:
   - `GoalDTO.cs`
   - `ObstacleDTO.cs`
   - `GoalParser.cs`
   - Remove goal/obstacle parsing from `VenueParser.cs`

4. **Properties** - DELETION:
   - `GameWorld.Goals` → DELETE
   - `GameWorld.Obstacles` → DELETE
   - `Location.ActiveGoalIds` → DELETE
   - `NPC.ActiveGoalIds` → DELETE
   - `Player.Memories` → DELETE

5. **Methods** - DELETION:
   - `ObstacleGoalFilter.GetVisibleLocationGoals()` → DELETE
   - `GoalCompletionHandler.CompleteGoal()` → DELETE
   - `LocationFacade.BuildGoalCard()` → DELETE
   - ALL goal-related view model building → DELETE

**Verification**:
```bash
grep -r "\.Goals" src/          # Should return ZERO
grep -r "ActiveGoalIds" src/    # Should return ZERO
grep -r "GoalDTO" src/          # Should return ZERO
grep -r "ObstacleGoalFilter" src/  # Should return ZERO
```

**Testing**: Full regression suite, verify nothing broke.

**Risk**: CRITICAL - Complete deletion, cannot rollback easily. Must be CERTAIN Phase 6 works perfectly.

---

### Phase 8: Obligation Refactoring (Week 8)

**Goal**: Adapt Obligation system to use Scene chains.

**Deliverables**:
1. **ObligationDefinition**:
   - Convert `PhaseDefinitions` → `SceneChainTemplates`
   - Each phase spawns initial scene
   - Scene completion spawns next phase's scene

2. **Obligation Discovery**:
   - `ObligationDiscoveryEvaluator` spawns discovery scene (not obstacles)
   - Discovery scene completion spawns Phase 1 scenes

3. **Obligation Progress**:
   - Track via scene completion, not goal completion
   - `ObligationActivity.CompleteScene()` checks if phase complete
   - Phase completion spawns next phase's scenes

**Testing**: Complete full investigation chain, verify phases progress correctly.

**Risk**: MEDIUM - Complex dependency chains, but localized to investigation system.

---

## Critical Path

```
Phase 1 (Entities) ← FOUNDATION
  ↓ REQUIRED BY
Phase 2 (Data Layer) ← PARSING
  ↓ REQUIRED BY
Phase 3 (Spawning) ← CREATION
  ↓ REQUIRED BY
Phase 4 (Query Layer) + Phase 5 (Challenges) ← CAN BE PARALLEL
  ↓ BOTH REQUIRED BY
Phase 6 (UI Switchover) ← USER-VISIBLE
  ↓ REQUIRED BY
Phase 7 (Deletion) ← SCORCHED EARTH
  ↓ REQUIRED BY
Phase 8 (Obligations) ← FINAL POLISH
```

**Timeline**: 8 weeks for complete transformation.

**Parallelization**: Phase 4 (UI building) and Phase 5 (Challenge integration) can happen simultaneously (no dependencies between them).

---

## Testing Strategy

### Unit Tests (Per Phase)

**Phase 1**: Entity serialization/deserialization
**Phase 2**: Parser correctness (DTO → Entity conversions)
**Phase 3**: Scene spawning logic (correct location, correct timing)
**Phase 4**: Scene queries (correct filtering by location/NPC)
**Phase 5**: Challenge context creation (Scene properties → Challenge setup)
**Phase 6**: End-to-end flow (action → spawn → display → challenge → complete)
**Phase 7**: Grep verification (zero references to deleted entities)
**Phase 8**: Obligation chains (multi-phase progression)

### Integration Tests (After Phase 6)

- Complete tutorial flow using scenes
- All challenge types (Social/Mental/Physical)
- Scene expiration
- Card granting
- Obligation discovery and progression
- Save/load with scenes

### Regression Tests (After Phase 7)

- FULL game functionality
- No crashes
- No missing content
- Performance within 10% of baseline

---

## What Gets Lost vs Gained

### Intentionally Eliminated Patterns

**From agent analysis - these are DESIGN IMPROVEMENTS:**

1. **Obstacle Intensity Reduction** (preparation goals reduce difficulty of resolution goals)
   - **Why eliminated**: Boolean gate with numbers. Violates perfect information.
   - **Replacement**: DifficultyModifiers show ALL paths to reduce difficulty BEFORE attempting.

2. **Goal ConsequenceType** (Resolution/Bypass/Transform/Modify)
   - **Why eliminated**: Obstacle modification tied to deleted Obstacle entity.
   - **Replacement**: Scene outcomes spawn new scenes or grant resources.

3. **Runtime State in JSON** (`isAvailable`, `isCompleted`)
   - **Why eliminated**: Architectural violation (mixing template with instance).
   - **Replacement**: SceneTemplate (pure definition) + Scene (runtime instance).

4. **Continuous Filtering** (ObstacleGoalFilter on every UI refresh)
   - **Why eliminated**: Performance overhead, boolean gate pattern.
   - **Replacement**: Simple lookup of ActiveSceneIds (no filtering needed).

### Preserved Core Mechanics

**From agent analysis - these patterns KEPT:**

1. **DifficultyModifiers** - Understanding/Mastery/Familiarity reduce difficulty
   - Direct copy from Goal to Scene
   - TRANSPARENT RESOURCE GATES (player sees exactly what they need)

2. **Resource Costs** - Focus/Stamina/Coins to attempt
   - Direct copy from Goal to Scene
   - RESOURCE COMPETITION creates strategic depth

3. **VictoryConditions** - Momentum thresholds with typed rewards
   - Direct copy from Goal.GoalCards to Scene.VictoryConditions
   - Tactical victory conditions unchanged

4. **Context Matching** - Equipment reduces costs via context tags
   - Obstacle.Contexts → Scene.ContextTags
   - Equipment as economic modifier preserved

---

## Success Criteria

### Technical Metrics

**Code Quality**:
- ✅ Zero references to Goal/Obstacle/MemoryFlag entities
- ✅ Zero boolean condition checking (IsAvailable, CanAccess)
- ✅ Zero weak typing (object types for game data)
- ✅ Build succeeds 0 warnings, 0 errors

**Performance**:
- ✅ Scene queries within 10% of current goal queries
- ✅ Spawning overhead < 5ms per scene
- ✅ Memory usage within 15% of baseline

**Architecture**:
- ✅ All content spawned by actions (no pre-existing libraries)
- ✅ All gates are resource costs (no boolean conditions)
- ✅ All knowledge is cards (no invisible flags)
- ✅ SceneTemplate separate from Scene instance (no confusion)

### Design Metrics

**Paradigm Adherence** (from DYNAMIC_CONTENT_PRINCIPLES.md):
- ✅ Actions create content (spawning demonstrable)
- ✅ Resources gate participation (costs displayed)
- ✅ Equipment reduces costs (economic effect)
- ✅ Scenes expire (urgency creates choices)
- ✅ Failure spawns alternatives (divergent paths)
- ✅ Investigations are chains (no phase gates)

**Strategic Depth**:
- ✅ Resource competition evident (time/focus/stamina shared)
- ✅ Opportunity cost measurable (choosing one prevents another)
- ✅ Multiple valid paths (success and failure both progress)
- ✅ No soft-locks (forward progress always possible)

---

## Risk Assessment

### HIGH RISK: Phase 7 Deletion

**Why**: Complete removal of working system. Cannot easily rollback after this point.

**Mitigation**:
- Git tag before Phase 7: `pre-goal-deletion`
- Phase 6 must be 100% validated (2+ weeks of testing)
- Full regression suite passing
- Manual playthrough of tutorial
- Performance profiling shows no degradation

**Rollback**: If Phase 7 causes catastrophic failure, `git revert` to tag loses 1 week of work. Expensive but feasible.

### MEDIUM RISK: Phase 5 Challenge Integration

**Why**: Tactical systems are critical gameplay. Bugs block progression.

**Mitigation**:
- Refactor ONE system at a time (Mental → Physical → Social)
- Test each system in isolation before proceeding
- Keep dev environment running old system for comparison

### LOW RISK: Phases 1-4

**Why**: Additive only, doesn't modify existing systems.

**Mitigation**: Continuous testing as we build, verify new system in isolation.

---

## Timeline Summary

| Phase | Duration | Key Deliverable | Risk |
|-------|----------|-----------------|------|
| 1: Entities | 1 week | SceneTemplate, Scene, Card entities | ZERO |
| 2: Data Layer | 1 week | Parsers, tutorial JSON conversion | LOW |
| 3: Spawning | 1 week | SceneSpawner, action integration | MEDIUM |
| 4: Query Layer | 1 week | GetActiveScenes, UI building | LOW |
| 5: Challenges | 1 week | Facade refactoring (3 systems) | HIGH |
| 6: UI Switchover | 1 week | Scene-only display | HIGH |
| 7: Deletion | 1 week | Scorched earth removal | CRITICAL |
| 8: Obligations | 1 week | Investigation chain adaptation | MEDIUM |
| **TOTAL** | **8 weeks** | **Full paradigm shift** | - |

---

## References

**Supporting Analysis Documents** (created by agents):

1. **`SCENE-CARD-ARCHITECTURE-ANALYSIS.md`** - Complete entity architecture analysis, semantic gap identification, new entity design (Lead Architect)

2. **Property Migration Matrix** (embedded in agent output) - 100+ property mappings, weak typing elimination, relationship redesign (Domain Developer)

3. **Service Layer Analysis** (embedded in agent output) - Dependency graph, method inventory, refactoring sequence (Senior Developer)

4. **`CONTENT-MIGRATION-ANALYSIS.md`** - Tutorial content structure, migration complexity estimate, JSON schema design (Domain Developer)

5. **`SCENE-CARD-DATA-FLOW-ARCHITECTURE.md`** - Complete data flow transformation, parser triangle analysis, layer-by-layer changes (Lead Architect)

**Core Specification**:
- **`DYNAMIC_CONTENT_PRINCIPLES.md`** - Target architecture principles (10 principles, 560+ lines)

**Current Architecture**:
- **`ARCHITECTURE.md`** - Existing system documentation
- **`CLAUDE.md`** - Development principles and constraints

---

**Document Version**: 2.0 (Agent-Informed)
**Last Updated**: 2025-10-23
**Status**: Architectural plan complete, ready for Phase 1 implementation
