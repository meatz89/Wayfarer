# Scene-Situation Architecture: Implementation Status

**Started:** 2025-01-27
**Status:** IN PROGRESS
**Reference:** [IMPLEMENTATION_PLAN.md](./IMPLEMENTATION_PLAN.md) | [SCENE_SITUATION_ARCHITECTURE.md](./SCENE_SITUATION_ARCHITECTURE.md)

---

## Phase 0: Current State Verification COMPLETE

### Verification Results
- Goal.cs does NOT exist (renamed to Situation.cs in prior refactor)
- Situation.cs EXISTS at `src/GameState/Situation.cs`
- SituationCard.cs EXISTS (was GoalCard.cs in prior refactor)
- 05_situations.json EXISTS (was goals.json in prior refactor)
- SituationParser.cs EXISTS
- SituationDTO.cs EXISTS

**Conclusion:** Prior Goal�Situation refactor is COMPLETE. Safe to proceed with EXTENDING Situation with Sir Brante features.

---

## Phase 1: Package Structure & Content Definitions ✅ COMPLETE

**Status:** COMPLETE

### Files Created
- ✅ `src/Content/Core/18_states.json` - 20 state definitions (8 Physical, 5 Mental, 7 Social)
- ✅ `src/Content/Core/19_achievements.json` - 22 achievement definitions across 5 categories

### Progress Notes
- Created 18_states.json with all 20 StateType enum values defined
- Created 19_achievements.json with achievements for Social, Investigation, Physical, Economic, Political categories
- Both files use segment-based duration tracking (no DateTime)
- Ready for Phase 2 (DTOs)

---

## Phase 2: DTOs (Data Transfer Objects) ✅ COMPLETE

**Status:** COMPLETE

### Files Created
- ✅ `src/Content/DTOs/StateDTO.cs`
- ✅ `src/Content/DTOs/AchievementDTO.cs`
- ✅ `src/Content/DTOs/SpawnRuleDTO.cs` (with RequirementOffsetsDTO, ConditionsDTO)
- ✅ `src/Content/DTOs/CompoundRequirementDTO.cs` (with OrPathDTO)
- ✅ `src/Content/DTOs/NumericRequirementDTO.cs`
- ✅ `src/Content/DTOs/PlayerScalesDTO.cs` (strongly-typed nested object with 6 int properties)
- ✅ `src/Content/DTOs/ActiveStateDTO.cs`
- ✅ `src/Content/DTOs/PlayerAchievementDTO.cs`

### Files Modified
- ✅ `src/Content/DTOs/SituationDTO.cs` - Added TemplateId, ParentSituationId, spawn/completion segment tracking
- ✅ `src/Content/DTOs/PlayerInitialConfigDTO.cs` - Added Resolve, Scales, ActiveStates, EarnedAchievements, CompletedSituationIds

### Progress Notes
- All DTOs use segment-based time tracking (Day/TimeBlock/Segment, NO DateTime)
- PlayerScalesDTO is strongly-typed nested object (NOT list or dictionary)
- CompoundRequirementDTO supports OR-based unlocking with multiple paths
- Ready for Phase 3 (Domain Entities)

---

## Phase 3: Domain Entities ✅ COMPLETE

**Status:** COMPLETE

### Files Created
- ✅ Enums (5): StateType, StateCategory, AchievementCategory, InteractionType, PlacementType
- ✅ New Entities (9): PlayerScales, ActiveState, PlayerAchievement, State, Achievement, SpawnRule, CompoundRequirement, NumericRequirement, Scene

### Files Extended
- ✅ Player.cs - Added Resolve, Scales, ActiveStates, EarnedAchievements, CompletedSituationIds
- ✅ Situation.cs - Added TemplateId, ParentSituationId, spawn/completion tracking
- ✅ Location.cs - Already has ActiveSituationIds (no changes needed)
- ✅ NPC.cs - Added BondStrength (already has ActiveSituationIds)

### Progress Notes
- All entities use segment-based time tracking (TimeBlocks enum, NO DateTime)
- Strong typing enforced (PlayerScales is nested object, not Dictionary)
- CompoundRequirement has IsSatisfied logic for runtime evaluation
- Ready for Phase 4 (Parsers) and early integration

---

## Phase 4: Parsers ✅ COMPLETE

**Status:** COMPLETE

### Files Created
- ✅ `src/Content/StateParser.cs` - ConvertDTOToState() and ParseStates() methods
- ✅ `src/Content/AchievementParser.cs` - ConvertDTOToAchievement() with ParseGrantConditions() translating Dictionary to strongly-typed
- ✅ `src/Content/SpawnRuleParser.cs` - ConvertDTOToSpawnRule() for situation spawn cascades
- ✅ `src/Content/RequirementParser.cs` - ConvertDTOToCompoundRequirement() and ConvertDTOToNumericRequirement() for OR-based unlocking

### Files Extended
- ✅ `src/Content/SituationParser.cs` - Added ParseTimeBlock() helper, integrated spawn/completion tracking
- ✅ `src/GameState/PlayerInitialConfig.cs` - Added Resolve, Scales, ActiveStates, EarnedAchievements, CompletedSituationIds
- ✅ `src/Content/PlayerInitialConfigParser.cs` - Parse new Scene-Situation properties from DTO

### Catalogue Analysis Complete
**Decision: NO catalogues needed for Scene-Situation parsers**

After analyzing `SocialCardEffectCatalog.cs`, determined that Scene-Situation parsers don't need catalogues because:
- Properties already concrete in JSON (morality: 0, not "neutral")
- No scaling based on player level or game state
- AchievementParser's inline Dictionary→strongly-typed conversion is appropriate (1:1 mapping, entity-specific)

Catalogues ARE used when JSON has categorical properties requiring translation to concrete values with game state scaling.
Catalogues NOT needed when JSON already has absolute values with no scaling requirements.

**Examples:**
- ✅ SocialCardEffectCatalog: Translates categorical (Remark, Rapport, Depth) → scaled effects
- ❌ StateParser: StateType enum already concrete
- ❌ AchievementParser: Simple 1:1 Dictionary mapping, inline is clearer

### Parse-Time Validation & Architectural Principles

**All parsers MUST validate:**
1. **Required fields present** - Throw InvalidOperationException if missing
2. **Enum values valid** - Throw if invalid enum string (e.g., StateType, AchievementCategory)
3. **Nested objects properly structured** - Validate child objects
4. **ID references exist in GameWorld** - Throw if referenced entity not found
5. **String enum references valid** - When JSON contains strings referencing enums/actions (e.g., "clearConditions": ["Rest"]), validate against action catalogue/enum values

**JSON Authoring Principles (Categorical-First Design):**

1. **NO ICONS IN JSON**
   - Icons are UI concerns, not data
   - JSON stores categorical data, UI derives icon from type/category
   - ❌ WRONG: `"icon": "sword-icon.png"`
   - ✅ CORRECT: No icon property (UI layer determines visual representation)

2. **PREFER CATEGORICAL OVER NUMERICAL**
   - Content authors describe INTENT and RELATIVE MAGNITUDE, not exact mechanics
   - Catalogues translate categorical → concrete with game state scaling
   - ❌ WRONG: `"bondRequirement": 15` (hardcoded number)
   - ✅ CORRECT: `"bondRequirement": "Trusted"` (categorical level)
   - **Exception:** Player state progression uses numerical (XP, Coins, Stats, Scales - runtime accumulation)

3. **VALIDATE ALL REFERENCES AT PARSE TIME**
   - ALL ID references must be validated against GameWorld
   - ALL enum string values must be validated against enum definitions
   - Fail fast at game initialization, not during gameplay
   - Example: `if (!gameWorld.NPCs.ContainsKey(dto.NpcId)) throw new InvalidOperationException(...)`

**Why Scene-Situation Uses Numerical Values:**
- Current design uses numerical for player state (Morality: +7, Resolve: 30, BondStrength: 15)
- Future enhancement may introduce categorical tiers ("Trusted", "Close", "Deep") with catalogue translation
- Principles apply to CONTENT AUTHORING (situations, cards), not PLAYER STATE (runtime progression)

### Progress Notes
- 4 new parsers created, 2 extended
- All use segment-based time tracking (TimeBlocks enum, NO DateTime)
- Strong typing enforced throughout (Dictionary only in DTO, converted to strongly-typed in parser)
- Ready for Phase 5 (GameWorld integration)

---

## Phase 5: GameWorld Updates ✅ COMPLETE

**Status:** COMPLETE

### Files Modified
- ✅ `src/GameState/GameWorld.cs` - Added States and Achievements collections with helper methods
- ✅ `src/Content/PackageContent.cs` - Already had States and Achievements lists (verified in prior phase)

### Architectural Principles Documented

**CRITICAL DISCOVERY:** During Phase 5, identified violation of holistic architecture principle - attempted to implement runtime string matching on List<string> clearConditions.

**Principle Documented (Universal, Not State-Specific):**

**Execution Context Entity Design (Principle 12)**
- Categorical properties from JSON MUST decompose to strongly-typed properties at parse time
- Each property named for execution context (ClearsOnRest → ResourceFacade, not generic "conditions")
- Zero runtime string matching (FORBIDDEN: if (list.Contains("string")))
- Parse-time catalogue translation only (catalogues called during initialization, NEVER at runtime)

**Why This Matters:**
- Tactical thinking: "Check this list" → Wrong architecture
- Holistic thinking: "What contexts check this?" → Correct architecture
- Properties document WHERE used, not WHAT they contain
- Compiler-enforced correctness, no runtime interpretation

**Documentation Updates:**
- ✅ Added Principle 12 to `CLAUDE.md` (holistic entity design)
- ✅ Added principle section to `situation-refactor/SCENE_SITUATION_ARCHITECTURE.md`
- ✅ Updated `situation-refactor/IMPLEMENTATION_PLAN.md` with holistic pattern

**Violations Fixed:**
- ✅ Removed icon properties from achievements JSON (NO ICONS IN JSON principle)
- ✅ Deleted duplicate InteractionType enum (HIGHLANDER principle)
- ✅ Identified but NOT implemented List<string> runtime checking (would violate parse-time translation)

### Progress Notes
- Phase 5 GameWorld extensions complete
- Architectural violations identified and documented
- Universal principles elevated beyond implementation details
- Ready for Phase 6 (Facades)

---

## Phase 6: Facades (State Clearing Implementation) ✅ COMPLETE

**Status:** COMPLETE (Core Infrastructure)

### Architectural Pattern: Catalogue + Resolver (Universal Pattern)

**CRITICAL DISCOVERY:** State clearing implementation must follow proven Catalogue + Resolver pattern from SocialCardEffectCatalog + SocialEffectResolver, not tactical property-setting.

**THE PATTERN (Three-Layer Architecture):**

1. **Catalogue Layer (Parse-Time):**
   - Static class with pure functions
   - Translates categorical strings → strongly-typed behavior objects
   - Called ONCE during initialization by parser
   - Returns concrete behavior object (e.g., StateClearingBehavior)
   - Zero runtime overhead

2. **Behavior Object (Stored on Entity):**
   - Strongly-typed data class with semantic properties
   - Stored on domain entity (State.ClearingBehavior)
   - Contains NO logic, only data
   - Properties named for execution contexts (ClearsOnRest, ClearingItemTypes)

3. **Resolver Layer (Runtime Projection):**
   - Service class with projection methods
   - Uses GameWorld to query current state
   - Checks behavior objects to determine what SHOULD change
   - Returns projections (List<StateType> to clear)
   - Does NOT modify state directly
   - Enables UI preview, testing, single source of truth

4. **Facade Layer (Application):**
   - Calls resolver to get projection
   - Applies changes to GameWorld
   - Triggers cascades (SpawnFacade.EvaluateDormantSituations)

**WHY THIS PATTERN:**

**❌ WRONG - Tactical Property Setting:**
- Catalogue sets properties directly on entity during parsing
- Facades check properties scattered across codebase
- No projection capability (can't preview effects)
- Clearing logic embedded in facades (duplicated, scattered)
- Hard to test (requires full game state)

**✅ CORRECT - Catalogue + Resolver:**
- Catalogue returns behavior object
- Resolver centralizes ALL clearing logic in one service
- Facades call resolver, apply results
- Projection-based (what WOULD happen)
- Single source of truth for clearing decisions
- Testable independently
- UI can preview what would be cleared

**EXISTING EXAMPLES IN WAYFARER:**

**SocialCardEffectCatalog + SocialEffectResolver:**
- Catalogue: `GetEffectFromCategoricalProperties()` returns CardEffectFormula
- Parser: Stores formula on card.EffectFormula
- Resolver: `ProcessSuccessEffect()` projects what WOULD happen
- Facade: Applies projected changes, doesn't call catalogue

**State Clearing (This Implementation):**
- Catalogue: `StateClearConditionsCatalogue.GetClearingBehavior()` returns StateClearingBehavior
- Parser: Stores behavior on state.ClearingBehavior
- Resolver: `StateClearingResolver.GetStatesToClearOnRest()` projects which states to clear
- Facade: Removes states from Player.ActiveStates, triggers cascade

**BENEFITS:**
- ✅ Parse once, execute forever (zero catalogue overhead at runtime)
- ✅ Projection enables UI preview ("this would clear Wounded and Exhausted")
- ✅ Single source of truth (all clearing logic in resolver)
- ✅ Testable (can unit test resolver without full game)
- ✅ Separation of concerns (catalogue=translation, resolver=logic, facade=application)
- ✅ Strong typing enforced (behavior object properties)

### Files Created (State Clearing)

**Supporting Enums:**
- ✅ `src/GameState/Enums/ItemType.cs` - Medical, Food, Remedy, Provisions, Consumable
- ✅ `src/GameState/Enums/PenaltyResolutionType.cs` - PayFine, RepayDebt, ServeTime (Phase 6)
- ✅ `src/GameState/Enums/QuestCompletionType.cs` - ClearName, RestoreReputation, RestoreHonor, AchieveGoal (Phase 6)
- ✅ `src/GameState/Enums/SocialEventType.cs` - ReceiveComfort, BetrayTrust, RemoveDisguise, IdentityRevealed (Phase 6)

**Core Infrastructure:**
- ✅ `src/GameState/StateClearingBehavior.cs` - Data class with execution context properties (ClearsOnRest, RequiresSafeLocation, ClearingItemTypes, ClearsOnChallengeSuccess/Failure, etc.)
- ✅ `src/Content/Catalogues/StateClearConditionsCatalogue.cs` - Parse-time translation of clearConditions strings to StateClearingBehavior
- ✅ `src/Services/StateClearingResolver.cs` - Runtime projection service with methods for each execution context

**Documentation:**
- ✅ `docs/state_clearing_challenge_integration.md` - Integration pattern for challenge facades (SocialFacade, MentalFacade, PhysicalFacade)

### Files Modified (State Clearing)

**Entity and Parser Updates:**
- ✅ `src/GameState/State.cs` - Replaced `List<string> ClearConditions` with `StateClearingBehavior ClearingBehavior`
- ✅ `src/Content/StateParser.cs` - Calls `StateClearConditionsCatalogue.GetClearingBehavior()`, stores on State.ClearingBehavior, removed IsValidClearCondition() method

**Facade Integration (Complete):**
- ✅ `src/Subsystems/Resource/ResourceFacade.cs`
  - Added StateClearingResolver dependency
  - Extended ExecuteRest() with state clearing (rest-based clearing)
  - Added ConsumeItem() stub (requires Item.ItemType property - TODO for future)
- ✅ `src/Subsystems/Time/TimeFacade.cs`
  - Added StateClearingResolver dependency
  - Extended AdvanceSegments() with duration-based state clearing

**Facade Integration (Documented):**
- ⏳ `docs/state_clearing_challenge_integration.md` - Challenge facades (SocialFacade, MentalFacade, PhysicalFacade) integration pattern documented but NOT YET implemented

### StateClearingResolver Methods (Single Source of Truth)

**Implemented Execution Contexts:**
- ✅ `GetStatesToClearOnRest(bool isSafeLocation)` - Used by ResourceFacade.ExecuteRest()
- ✅ `GetStatesToClearOnItemConsumption(ItemType itemType)` - Used by ResourceFacade.ConsumeItem() (stub)
- ✅ `GetStatesToClearOnTimePassage(int currentDay, TimeBlocks currentTimeBlock, int currentSegment)` - Used by TimeFacade.AdvanceSegments()
- ✅ `GetStatesToClearOnChallengeSuccess()` - Ready for challenge facades
- ✅ `GetStatesToClearOnChallengeFailure()` - Ready for challenge facades
- ✅ `GetManuallyClearableStates()` - Ready for player action UI

**Phase 6 Methods (ConsequenceFacade Integration - Future):**
- ✅ `GetStatesToClearOnPenaltyResolution(PenaltyResolutionType penaltyType)` - Placeholder for Phase 6
- ✅ `GetStatesToClearOnQuestCompletion(QuestCompletionType questType)` - Placeholder for Phase 6
- ✅ `GetStatesToClearOnSocialEvent(SocialEventType eventType)` - Placeholder for Phase 6

### Sir Brante Cascade Integration (Phase 6 TODO)

All clearing points include TODO comments for Sir Brante cascade triggering:
```csharp
// TODO Phase 6: Trigger cascade after clearing states
// if (statesToClear.Any())
// {
//     await _spawnFacade.EvaluateDormantSituations();
// }
```

**Locations with cascade TODOs:**
- ResourceFacade.ExecuteRest() - After rest-based clearing
- TimeFacade.AdvanceSegments() - After duration-based clearing
- Challenge facades (documented pattern) - After challenge success/failure clearing

### What Remains for Full Phase 6 Completion

**1. Challenge Facade Implementation:**
- Integrate state clearing into SocialFacade (conversation completion)
- Integrate state clearing into MentalFacade (investigation completion)
- Integrate state clearing into PhysicalFacade (obstacle completion)
- Pattern documented in `docs/state_clearing_challenge_integration.md`

**2. SpawnFacade Cascade Integration:**
- Remove TODO comments, implement cascade triggering
- Requires SpawnFacade.EvaluateDormantSituations() implementation

**3. DI Container Registration:**
- Register StateClearingResolver in dependency injection container
- Ensure all facades receive resolver via constructor injection

**4. Item System Extension:**
- Add ItemType property to Item entity
- Complete ConsumeItem() implementation in ResourceFacade
- Map ItemCategory to ItemType or redesign item categorization

### Progress Notes

- ✅ Catalogue + Resolver pattern documented as universal architecture principle
- ✅ State clearing implemented following proven SocialCardEffectCatalog pattern
- ✅ All clearing logic centralized in StateClearingResolver (single source of truth)
- ✅ Projection-based design enables UI preview and independent testing
- ✅ ResourceFacade and TimeFacade fully integrated with state clearing
- ✅ Challenge facade integration pattern documented for future implementation
- ⏳ SpawnFacade cascade integration deferred (Phase 6 TODO comments in place)
- ⏳ Challenge facades require understanding completion logic before integration
- **Ready for Phase 7 (UI Components) - Core state clearing infrastructure complete**

---

## Phase 7: UI Components

**Status:** NOT STARTED

---

## Phase 8: Save/Load

**Status:** NOT STARTED

---

## Phase 9: Content Creation

**Status:** NOT STARTED

---

## Phase 10: Integration

**Status:** NOT STARTED

---

## Next Steps

**Current Phase:** Phase 7 - UI Components
**Completed Phases:** 0-6 (Verification, Package Structure, DTOs, Domain Entities, Parsers, GameWorld, Facades - State Clearing Infrastructure)
**Next Action:** Implement UI components for Scene-Situation system (state display, achievement tracking, situation cards)
