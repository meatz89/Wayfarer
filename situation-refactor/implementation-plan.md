# WAYFARER SCENE/SITUATION SYSTEM - IMPLEMENTATION PLAN

## EXECUTIVE SUMMARY

This document outlines the complete implementation plan to bring the Wayfarer Scene/Situation system from **85% complete** to **100% complete** while maintaining architectural excellence. The system already has strong foundations with a three-tier timing model, provisional scene architecture, and parse-time catalogue pattern. This plan addresses the remaining 15% of gaps identified through comprehensive codebase analysis.

---

## CURRENT STATE ASSESSMENT

### ✅ IMPLEMENTED (85%)
- Three-Tier Timing Model (Parse → Instantiation → Query)
- SceneTemplate/Scene/Situation domain architecture
- Provisional Scene System (perfect information pattern)
- PlacementFilter with categorical entity selection
- Query-Time Action Instantiation (Tier 3 pattern)
- Modal Scene Takeover (Sir Brante forced moments)
- Cascade/Breathe Progression modes
- Parse-Time Archetype Generation via catalogues
- HIGHLANDER Pattern A (Scene → SituationIds)
- Placement Resolution via enums

### ⚠️ GAPS (15%)
1. **SpawnConditions System** - Eligibility checking not fully implemented
2. **Situation.cs Architecture** - Placement reference inconsistency, 50+ properties
3. **Trigger Event Gaps** - Time advancement, obligation completion, recursive spawning
4. **Selection Strategies** - Priority-based entity selection when multiple match
5. **Route Filtering** - Missing TerrainType, Tier, DangerRating properties
6. **Delayed Spawning** - SceneSpawnReward.DelayDays exists but no scheduler
7. **Scene Expiration** - ExpirationDays exists but no enforcement
8. **AI Narrative Generation** - NarrativeHints exist but no generator integration

---

## ARCHITECTURAL PRINCIPLES (MAINTAINED THROUGHOUT)

### Core Principles
- **Strong Typing** - No dictionaries, no `var`, explicit types everywhere
- **HIGHLANDER Pattern A** - ID references + GameWorld single source of truth
- **Catalogue Pattern** - Parse-time ONLY, NEVER runtime
- **Three-Tier Timing** - Parse → Instantiation → Query (lazy action creation)
- **Semantic Honesty** - Method names match return types and reality
- **Single Granularity** - Consistent tracking at one level (Scene owns placement)
- **"Let It Crash"** - No null-coalescing, fail fast with descriptive errors
- **Parser-JSON-Entity Triangle** - All three layers aligned
- **Perfect Information** - Provisional scenes show consequences before selection

### Refactoring.md Alignment
The plan implements the three-part system:
1. **Spawn Conditions** - When is scene eligible?
2. **Placement Filters** - Where should scene appear?
3. **Trigger Events** - What causes spawn checks?

---

## PHASE 0: ARCHITECTURAL CLEANUP (FOUNDATION)

**Priority:** CRITICAL - Must fix violations BEFORE adding features

**Rationale:** Current Situation.cs has architectural violations that will compound if new features are built on top. Clean foundation = clean features.

### 0.1 Refactor Situation.cs (Single Responsibility)

**Problem:** 50+ properties violate single responsibility principle

**Solution:** Extract logical groups into separate classes:

1. **SituationPlacement** - Where situation appears
   - PlacementLocation (object)
   - PlacementNpc (object)
   - PlacementRouteId (string)
   - ParentScene (object)

2. **SituationSpawnTracking** - Lifecycle timestamps
   - SpawnedDay
   - SpawnedTimeBlock
   - SpawnedSegment
   - CompletedDay
   - CompletedTimeBlock
   - CompletedSegment

3. **SituationRequirements** - Unlock requirements
   - CompoundRequirement
   - ProjectedBondChanges
   - ProjectedScaleShifts
   - ProjectedStates
   - Unmet*Requirement classes (moved to separate file)

4. **SituationConsequence** - Outcome configuration
   - ConsequenceType
   - SetsResolutionMethod
   - SetsRelationshipOutcome
   - TransformDescription

**Result:** Situation.cs reduced from 50+ properties to ~15 core properties (identity, state, template)

### 0.2 Fix Placement Reference Inconsistency (HIGHLANDER)

**Problem:** Scene has PlacementId (string), Situation has PlacementLocation/PlacementNpc (objects). Two sources of truth.

**Solution:** Scene OWNS placement, Situation QUERIES it

**Pattern:**
- Remove duplicate object references from Situation
- Add GetPlacementLocation(GameWorld) method that queries Scene.PlacementId
- Scene is single source of truth for placement
- Situation inherits context from parent Scene

**Result:** ONE representation at runtime (HIGHLANDER Pattern A)

### 0.3 Fix Status Tracking Redundancy

**Problem:** SituationStatus enum + IsAvailable/IsCompleted booleans = dual source of truth

**Solution:**
- Remove boolean flags
- Make them computed properties from Status enum
- Status enum is single source of truth

**Result:** No desync risk, one truth

---

## PHASE 1: SPAWN CONDITIONS SYSTEM

**Implements:** Refactoring.md Part 1 - Eligibility Checking

**Concept:** SceneTemplates have SpawnConditions that determine if template CAN spawn right now. Prevents spawning scenes before player ready.

### Components

**1. SpawnConditions Entity**
Domain entity storing conditions:
- **Player State:** Completed scenes, choices made, stat thresholds, items owned, visit counts
- **World State:** Weather requirements, time blocks, days of week
- **Entity State:** NPC bond thresholds, location reputation, route travel counts
- **Logic:** AND (all must pass) vs OR (any must pass)

**2. SpawnConditionEvaluator Service**
Service that evaluates conditions against player/world/entity state:
- Checks each condition type
- Applies AND/OR logic
- Returns bool (eligible or not)

**3. Integration with SceneInstantiator**
Before creating provisional scene:
- Call SpawnConditionEvaluator
- If not eligible, return null (don't spawn)
- If eligible, proceed with normal flow

### Example Use Cases

**Rain Storm Scene:**
```
SpawnConditions:
  - Weather = Rain OR Storm
  - Route.Terrain contains "Mountain"
  - Player.HasItem "WarmCloak" = false
```
Only spawns in rain on mountain without protection

**NPC Promise Follow-Up:**
```
SpawnConditions:
  - Player.CompletedScenes contains "PromiseScene"
  - Player.ChoiceHistory contains "PromisedToHelp"
  - DaysSince("PromisedToHelp") >= 2
```
Only spawns 2+ days after promise made

---

## PHASE 2: SELECTION STRATEGIES

**Implements:** Refactoring.md Section 9 - Multiple Entity Matching

**Concept:** When PlacementFilter matches multiple entities, SelectionStrategy determines which one to choose. Provides control over placement priority.

### Strategy Types

**1. Priority Scoring**
Score entities by criteria, select highest:
- Bond strength (prefer NPCs with highest bond)
- Last interaction (prefer most/least recent)
- Distance (prefer closest to player)

**2. Contextual Relevance**
Select based on game context:
- Never traveled (prioritize discovery)
- Least recently traveled (avoid repetition)
- Current location connection (prefer adjacent)

**3. Story Coherence**
Select based on narrative continuity:
- Location referenced in spawn conditions (maintains coherence)
- NPC from previous scene (continuation)
- Related entity (same venue, same faction)

**4. Random**
Random selection from matches (when no priority matters)

### Components

**1. SelectionStrategy Entity**
Domain entity defining strategy:
- Strategy type (Priority/Context/Story/Random)
- Criteria list (property, order, priority)
- Applied during placement filter evaluation

**2. EntitySelector Service**
Service that selects entity from matches:
- Scores entities by criteria
- Applies strategy logic
- Returns single entity ID

**3. Integration with SceneInstantiator**
After PlacementFilter finds matches:
- If multiple matches + SelectionStrategy defined, call EntitySelector
- If multiple matches + no strategy, use first match (fallback)
- If single match, use it directly

---

## PHASE 3: TRIGGER EVENT COMPLETION

**Implements:** Refactoring.md Part 3 - Missing Triggers

**Concept:** Trigger events cause system to evaluate templates for spawning. Currently missing 3 of 6 triggers.

### Missing Triggers

**1. Time Advancement Trigger**
Check templates when day/segment advances:
- Find templates with time-dependent SpawnConditions
- Evaluate eligibility
- Spawn if conditions met

**2. Obligation Phase Completion**
Spawn scenes when obligation phase completes:
- ObligationPhaseReward.ScenesToSpawn already exists
- Integration needed with ObligationFacade
- Build context from obligation completion

**3. Recursive Situation Spawning**
Spawn child situations when parent completes:
- SpawnFacade.ExecuteSpawnRules exists but not called
- Integration needed with SituationFacade
- Execute SuccessSpawns/FailureSpawns based on outcome

---

## PHASE 4: ROUTE FILTERING COMPLETION

**Concept:** Route entity missing properties needed for categorical filtering. Routes can't be selected by terrain/tier/danger.

### Missing Properties

**Route Entity Additions:**
- TerrainTypes (List<TerrainType>) - Forest, Mountain, Plains, etc.
- Tier (int 0-4) - Difficulty level
- DangerRating (int 1-10) - Danger level

**TerrainType Enum:**
Forest, Mountain, Plains, Desert, Swamp, Urban, Coastal, River

### Implementation

1. Add properties to Route.cs
2. Create TerrainType enum
3. Add to RouteDTO and RouteParser
4. Update FindMatchingRoute() to apply filters
5. Update JSON content with new properties

---

## PHASE 5: PLACEHOLDER REPLACEMENT ENHANCEMENT

**Concept:** Placeholders in templates replaced with actual entity data at finalization. Current system basic, expand to match refactoring.md vision.

### Additional Placeholders

**Context-Specific:**
- `{NPC_PROFESSION}` - NPC profession display
- `{LOCATION_TYPE}` - Location categorical type
- `{DISPUTED_RESOURCE}` - Context-specific resource
- `{WEATHER}` - Current weather condition
- `{TIME_OF_DAY}` - Current time block

**Custom Placeholders:**
- SceneSpawnContext.CustomPlaceholders dictionary
- Templates can define custom placeholders
- Resolved at spawn time from context

**Verisimilitude:** Placeholders ensure narrative matches mechanical reality

---

## PHASE 6: DELAYED SPAWNING AND EXPIRATION

**Concept:** Time-based spawn control - scenes spawn after delay, expire after time limit.

### Delayed Spawning

**SceneSpawnScheduler Service:**
- PendingSpawn entity (template, reward, context, spawn day)
- ScheduleDelayedSpawn() adds to queue
- CheckPendingSpawns() executes ready spawns
- Called from TimeAdvancementFacade after day advance

**Use Case:** "Marcus's business fails 3 days after you refused to help"

### Scene Expiration

**SceneExpirationChecker Service:**
- CheckExpiredScenes() finds scenes past expiration
- Changes state to Expired
- Removes associated actions
- Called from TimeAdvancementFacade after day advance

**Use Case:** "Limited-time opportunities that vanish if ignored"

---

## PHASE 7: AI NARRATIVE GENERATION INTEGRATION

**Concept:** Templates have NarrativeHints, AI generates actual narrative at spawn time. Procedural content generation.

### Components

**NarrativeGenerator Service:**
- BuildPrompt() from NarrativeHints + context
- Call Ollama API for generation
- Replace placeholders in result
- Cache generated narrative on Situation

**Integration:**
- If SituationTemplate.NarrativeTemplate empty + NarrativeHints present
- Call NarrativeGenerator.GenerateNarrative()
- Store in Situation.GeneratedNarrative
- Use as Description for display

**Benefits:**
- Procedural narrative variety
- Context-aware storytelling
- Reduces JSON authoring burden

---

## PHASE 8: TESTING AND VALIDATION

**Concept:** Comprehensive testing ensures all features work correctly and architecture remains sound.

### Test Content

**Create test scenes demonstrating:**
- All spawn condition types (player/world/entity)
- All selection strategies (priority/context/story/random)
- All trigger events (location/route/NPC/action/time/obligation)
- Delayed spawning with various delays
- Scene expiration with various durations
- Placeholder replacement with all types
- AI narrative generation

### Integration Testing

**Test flows:**
1. Start game → Verify starter scenes spawn with conditions
2. Complete choice → Verify provisional → active flow
3. Fail spawn conditions → Verify scene doesn't spawn
4. Multiple matches → Verify selection strategy chooses correct entity
5. Day advance → Verify time trigger fires
6. Delayed spawn → Verify executes at correct time
7. Scene expiration → Verify removes at correct time

### Architectural Validation

**Run validation checks:**
- No HIGHLANDER violations (placement consistent)
- No catalogue calls at runtime (grep for catalogue imports in facades)
- Parser-JSON-Entity triangle aligned (all three match)
- Strong typing maintained (no dictionaries added)
- "Let It Crash" preserved (no null-coalescing)

---

## IMPLEMENTATION SEQUENCE

### Week 1: Foundation
**Phase 0 - Architectural Cleanup**
- Refactor Situation.cs (single responsibility)
- Fix placement inconsistency (HIGHLANDER)
- Fix status redundancy
- Build and verify no regressions

### Week 2: Core Feature
**Phase 1 - Spawn Conditions**
- Create SpawnConditions entity/DTO/parser
- Implement SpawnConditionEvaluator
- Integrate with SceneInstantiator
- Create test content
- Verify eligibility checking works

### Week 3: Polish
**Phase 2 - Selection Strategies**
- Create SelectionStrategy entities
- Implement EntitySelector service
- Integrate with placement evaluation
- Test priority/context/story selection

### Week 4: Missing Pieces
**Phase 3 - Trigger Completion**
- TimeAdvancementTrigger implementation
- Obligation phase spawn integration
- Recursive situation spawning integration

**Phase 4 - Route Filtering**
- Add TerrainType/Tier/DangerRating to Route
- Update DTO/parser/filter logic
- Update JSON content

### Week 5: Time Systems
**Phase 5 - Placeholders**
- Expand PlaceholderReplacer with new types
- Add custom placeholder support
- Update JSON templates

**Phase 6 - Delayed/Expiration**
- SceneSpawnScheduler implementation
- SceneExpirationChecker implementation
- Integrate with TimeAdvancementFacade

### Week 6: Content Generation + Validation
**Phase 7 - AI Generation**
- NarrativeGenerator service
- Ollama API integration
- Integrate with SceneInstantiator

**Phase 8 - Testing**
- Create comprehensive test content
- Run integration tests
- Architectural validation
- Performance testing

---

## SUCCESS CRITERIA

### Functional Requirements
✅ SpawnConditions prevent ineligible spawns
✅ Selection strategies choose appropriate entities
✅ All 6 trigger events fire correctly
✅ Route filtering works with terrain/tier/danger
✅ Delayed spawning executes at correct time
✅ Scene expiration removes stale scenes
✅ AI narrative generation produces coherent text

### Architectural Requirements
✅ Situation.cs violations fixed (placement consistent, <20 properties)
✅ No new HIGHLANDER violations
✅ Parser-JSON-Entity triangle maintained
✅ All catalogues remain parse-time only
✅ Perfect information pattern preserved
✅ Three-tier timing model intact
✅ Strong typing maintained (no dictionaries)

### Player Experience
✅ Perfect information (player sees consequences before selection)
✅ Dynamic content (AI-generated narratives feel fresh)
✅ Fair gameplay (spawn conditions respect player state)
✅ Verisimilitude (placeholders ensure narrative matches mechanics)
✅ Strategic depth (selection strategies create interesting choices)
✅ No soft-locks (time systems prevent stuck states)

---

## ARCHITECTURAL PATTERNS REFERENCE

### HIGHLANDER Pattern A (Persistence + Navigation)
- Entity has BOTH ID property and object reference
- ID populated from JSON during parsing
- Object populated by parser during parsing (GameWorld lookup)
- Runtime code uses ONLY object reference (never ID lookup)
- ID immutable (never reassigned after parsing)

**Example:** Scene.PlacementId (string) + Scene queries GameWorld.Locations

### Catalogue Pattern (Parse-Time Only)
- Static class with pure scaling functions
- Called ONLY by parsers during initialization
- NEVER imported by facades/managers/services
- Translates categorical properties → concrete values
- Fails fast on unknown categorical values

**Example:** SituationArchetypeCatalog.GetArchetype() called by SceneTemplateParser

### Three-Tier Timing Model
- **Tier 1 (Parse Time):** JSON → DTO → Parser → Template → GameWorld.Templates
- **Tier 2 (Instantiation Time):** Template → Scene/Situations (Dormant) → GameWorld
- **Tier 3 (Query Time):** Situations (Dormant → Active) + Actions instantiated

**Benefits:** Fail fast, lazy instantiation, perfect information

### Provisional Scene Architecture
- Create provisional scenes for ALL choices EAGERLY
- Player sees exact placement before selecting
- Finalize SELECTED scene (Provisional → Active)
- Delete UNSELECTED provisional scenes

**Benefits:** Perfect information, no wasted resources

---

## RISK MITIGATION

### Risk: Breaking Existing Functionality
**Mitigation:** Phase 0 cleanup BEFORE adding features, comprehensive testing after each phase

### Risk: Performance Degradation
**Mitigation:** Maintain lazy instantiation (Tier 3 actions), no eager computation, profile after Phase 8

### Risk: Architectural Drift
**Mitigation:** Agent validation after each phase, strict adherence to patterns, peer review

### Risk: Scope Creep
**Mitigation:** Fixed 8-phase plan, no new features beyond refactoring.md spec, time-boxed weeks

---

## CONCLUSION

This implementation plan brings the Wayfarer Scene/Situation system from 85% to 100% complete while maintaining architectural excellence. The system already has strong foundations - this plan fills the remaining gaps identified through comprehensive analysis. Each phase builds on previous work, with Phase 0 providing critical architectural cleanup before feature additions. Success criteria ensure both functional correctness and architectural integrity.

The result will be a complete, production-ready procedural narrative system that respects the player's time, maintains perfect information, and generates dynamic content through AI integration - all while upholding the strict architectural principles that make Wayfarer maintainable and extensible.
