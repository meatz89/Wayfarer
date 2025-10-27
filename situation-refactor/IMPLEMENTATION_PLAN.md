# Scene-Situation Architecture: Complete Implementation Plan

## Reference Documents

- **Architecture Specification:** [SCENE_SITUATION_ARCHITECTURE.md](./SCENE_SITUATION_ARCHITECTURE.md)
- **Design Principles:** [game-design-principles.md](../game-design-principles.md)

## Implementation Philosophy

### Critical Rules (NO EXCEPTIONS)

**1. SEARCH FIRST, ASSUME NEVER**
- Before touching ANY file, search the codebase exhaustively
- Use Glob to find ALL related files
- Use Grep to find ALL references to classes/methods/properties
- Read COMPLETE files (no limit/offset unless genuinely too large)
- Verify assumptions with actual code inspection
- NEVER create entities/classes/properties that already exist
- NEVER delete entities/classes/properties that are still referenced

**2. REFACTOR OVER CREATE**
- Prefer renaming/modifying existing classes over creating new ones
- If Goal serves the same purpose as Situation, RENAME Goal → Situation
- If Location already has goal tracking, MODIFY it to track situations
- Only create NEW classes when functionality is genuinely new
- Check inheritance hierarchies before creating parallel structures

**3. DELETE LEGACY IMMEDIATELY**
- NO compatibility layers
- NO gradual migration paths
- NO "old and new" systems coexisting
- When refactoring Goal → Situation, DELETE all Goal code immediately after migration
- Remove all obsolete properties from entities the moment they're replaced
- Delete all TODO comments and legacy code blocks on sight
- If a class becomes empty after refactoring, DELETE the entire file

**4. ARCHITECTURE COMPLIANCE**
- Follow game-design-principles.md strictly:
  - Single source of truth (GameWorld owns everything)
  - Strong typing (NO dictionaries, NO HashSets, ONLY List<T>)
  - Ownership vs Placement vs Reference (know which is which)
  - No boolean gates (accumulative numerical resources only)
  - Typed rewards as system boundaries
  - One purpose per entity
- Violating these principles = WRONG DESIGN, not implementation problem

**5. COMPLETENESS OVER SAFETY**
- Implementation speed is priority
- Breaking things temporarily is acceptable
- Non-playable intermediate states are acceptable
- Focus on complete end-to-end implementation
- No incremental "let's test this first" stops
- Implement entire vertical slices in one pass

---

## Phase 0: Current State Verification (CRITICAL FIRST STEP)

### Purpose
Verify that the Goal→Situation rename from the prior refactor is complete. Confirm current state matches architecture document assumptions. Fail fast if assumptions are wrong.

### Verification Checklist

**Before Starting ANY implementation:**

- [ ] Search for `Goal.cs` in src/GameState/
  - [ ] Expected result: NOT FOUND (renamed to Situation.cs in prior refactor)
  - [ ] If found: STOP - Situation.cs and Goal.cs should not coexist

- [ ] Verify `Situation.cs` exists in src/GameState/
  - [ ] Expected result: EXISTS
  - [ ] Verify it contains: PlacementLocationId, PlacementNpcId, SituationCards, ConsequenceType
  - [ ] If not found: STOP - Prior refactor incomplete

- [ ] Search for "GoalCard" references
  - [ ] Expected result: RENAMED to SituationCard
  - [ ] Check: src/GameState/Cards/SituationCard.cs exists
  - [ ] If GoalCard.cs found: STOP - Rename incomplete

- [ ] Search for references to "Goal" in codebase
  - [ ] Expected: Only in comments/documentation as "was Goal, now Situation"
  - [ ] No class names, property names, or variable names with "Goal"
  - [ ] If active Goal references found: STOP - Rename incomplete

- [ ] Verify 05_situations.json exists (was goals.json)
  - [ ] Expected result: File exists with "situations" array
  - [ ] Contains elena_evening_service, thomas_warehouse_loading
  - [ ] If goals.json found instead: STOP - Content rename incomplete

- [ ] Verify SituationParser.cs exists (was GoalParser)
  - [ ] Expected result: src/Content/SituationParser.cs exists
  - [ ] Has ConvertDTOToSituation method
  - [ ] If GoalParser.cs found: STOP - Parser rename incomplete

- [ ] Verify time system is segment-based
  - [ ] Search for "DateTime" in GameState/
  - [ ] Expected: NOT FOUND (except in comments about external APIs)
  - [ ] Verify Player has: CurrentDay (int), CurrentTimeBlock (enum), CurrentSegment (int)
  - [ ] If DateTime found in domain logic: VIOLATION - Wrong time system

- [ ] Verify strong typing enforcement
  - [ ] Search for "Dictionary<string," in GameState/
  - [ ] Expected: Only for entity lookups (Dictionary<string, Situation>, etc.)
  - [ ] No Dictionary<string, int>, Dictionary<string, object>, etc.
  - [ ] If generic dictionaries found: VIOLATION - Architecture violation

### If ANY verification fails:

**STOP IMPLEMENTATION**
- Document what's different from assumptions
- Determine if prior refactor needs completion
- Update architecture documents to match reality
- Only proceed once current state is clear

### If ALL verifications pass:

**Proceed to Phase 1**
- Situation.cs exists and ready to extend
- SituationCards exist (was GoalCards)
- Time system is segment-based
- Strong typing enforced
- Safe to begin Sir Brante integration

---

## Phase 1: Content Structure (JSON Layer)

### 1.1: Create New JSON Content Files

**Before Starting:**
- [ ] Search existing Content/Core directory structure
- [ ] Identify all existing .json files and their purposes
- [ ] Verify naming conventions currently used
- [ ] Check if any "situations" or "scales" or "states" files already exist

**Implementation:**

- [ ] Create `src/Content/Core/15_situations.json`
  - [ ] Define complete Situation template structure
  - [ ] Include: id, name, interactionType, placementType, targetPlacementId
  - [ ] Include: requirements (compound OR paths with categorical properties)
  - [ ] Include: costs (resolve, systemCost, coins, time - all tier-based/categorical)
  - [ ] Include: projectedConsequences (bondChanges, scaleShifts, resourceGains, stateApplications)
  - [ ] Include: resolutionSpawns (onSuccess, onFailure lists with spawn rules)
  - [ ] Include: challengeDefinition (if Challenge type: type, targetObstacleId, goalCards)
  - [ ] Include: navigationDefinition (if Navigation type: destinationId, autoTriggerScene)
  - [ ] Include: narrativeHints (tone, theme, context for AI)
  - [ ] Create 10-15 example templates covering all interaction types
  - [ ] Create spawn chain examples (parent → child with requirement offsets)

- [ ] Create `src/Content/Core/16_scales.json`
  - [ ] Define all 6 scale types: Morality, Lawfulness, Method, Caution, Transparency, Fame
  - [ ] For each: type, min, max, negativeLabel, positiveLabel, zeroLabel
  - [ ] Document mechanical meaning of each extreme

- [ ] Create `src/Content/Core/17_states.json`
  - [ ] Define all state types (20-25 total)
  - [ ] Categories: Physical, Mental, Social
  - [ ] For each: type, category, blockedActions, enabledActions, clearConditions, duration, description
  - [ ] Examples: Wounded, Exhausted, Sick, Confused, Inspired, Wanted, Celebrated, Armed, Provisioned

- [ ] Create `src/Content/Core/18_achievements.json`
  - [ ] Define achievement templates
  - [ ] For each: id, category (Combat/Social/Investigation/Economic/Political), name, description, icon
  - [ ] Define grant conditions (which Situations grant which achievements)

- [ ] Create `src/Content/Core/19_scene_templates.json` (optional, for authored scenes)
  - [ ] Define 3-5 critical authored scene moments
  - [ ] For each: id, triggerConditions, introNarrative, situationGroup, resolutionNarrative, exclusive

### 1.2: Modify Existing JSON Content Files

**Before Starting:**
- [ ] Read `player.json` completely
- [ ] Read `locations.json` completely
- [ ] Read `npcs.json` completely
- [ ] Read `investigations.json` completely
- [ ] Identify all Goal references in existing JSON
- [ ] Map which entities currently reference Goals

**Implementation:**

- [ ] Modify `src/Content/Core/01_foundation.json` or `06_gameplay.json` (player config)
  - [ ] Add `resolve: 30` to player starting state
  - [ ] Add `scales` object (STRONG TYPING - nested object with 6 properties):
    ```json
    "scales": {
      "morality": 0,
      "lawfulness": 0,
      "method": 0,
      "caution": 0,
      "transparency": 0,
      "fame": 0
    }
    ```
  - [ ] Add `activeStates: []` (empty array - will hold StateType enum values)
  - [ ] Add `earnedAchievements: []` (empty array - will hold achievement IDs with earned time)
  - [ ] Add `completedSituationIds: []` (empty array - situation ID strings)

- [ ] Modify `src/Content/Core/06_gameplay.json` (or wherever locations defined)
  - [ ] For each location:
    - [ ] Add `initialSituations: []` (list of Situation template IDs to spawn on first visit)
    - [ ] REMOVE `goals: []` or `goalIds: []` (static Goal lists NO LONGER EXIST)
  - [ ] Search for any "goals" property in location definitions
  - [ ] Delete all static goal assignments to locations

- [ ] Modify `src/Content/Core/03_npcs.json`
  - [ ] For each NPC:
    - [ ] Add `bondStrength: 0` (starting relationship)
    - [ ] Add `initialSituations: []` (Situation template IDs available from first meeting)
    - [ ] Add `statuses: []` (empty list of NpcStatus enums)
  - [ ] Search for any static goal assignments to NPCs
  - [ ] Delete all static goal lists from NPCs

- [ ] Modify `src/Content/Core/13_investigations.json`
  - [ ] For each Investigation:
    - [ ] For each phase:
      - [ ] Change `spawnedGoals: []` → `spawnedSituations: []`
      - [ ] Replace Goal IDs with Situation template IDs
      - [ ] Update structure to reference Situation templates instead of Goals
  - [ ] Search entire file for "goal" (case insensitive)
  - [ ] Rename all goal references to situation references

### 1.3: Delete Obsolete JSON Files

**Before Starting:**
- [ ] Search for `goals.json` or `05_goals.json` or similar
- [ ] Verify it's no longer needed (replaced by situations.json)
- [ ] Grep entire codebase for references to this file
- [ ] Verify parsers no longer attempt to load it

**Implementation:**

- [ ] DELETE `src/Content/Core/05_goals.json` (or equivalent)
  - [ ] File no longer needed (replaced by situations.json)
  - [ ] Goals are now spawned dynamically from Situation templates

---

## Phase 2: DTO Layer (Data Transfer Objects)

### 2.1: Create New DTOs

**Before Starting:**
- [ ] Search `src/Content` for existing DTO files
- [ ] Identify DTO naming conventions (suffix? folder structure?)
- [ ] Verify no SituationDTO already exists
- [ ] Check if DTOs are in separate files or grouped

**Implementation:**

- [ ] Create `src/Content/DTOs/SituationDTO.cs` (or add to existing DTO file)
  - [ ] All properties matching situations.json structure
  - [ ] Compound requirement structure (OrPaths with typed sub-requirements)
  - [ ] Cost structure (resolve, systemCost, coins, time - all categorical strings)
  - [ ] ProjectedConsequences structure (bondChanges, scaleShifts, etc.)
  - [ ] SpawnRule structure (templateId, targetPlacement, requirementOffsets, conditions)
  - [ ] ChallengeDefinition structure (type, targetObstacleId, goalCards)
  - [ ] NavigationDefinition structure (destinationId, autoTriggerScene)
  - [ ] NO JsonPropertyName attributes (JSON names MUST match C# property names)

- [ ] Create `src/Content/DTOs/ScaleDTO.cs`
  - [ ] Properties: type, min, max, negativeLabel, positiveLabel, zeroLabel

- [ ] Create `src/Content/DTOs/StateDTO.cs`
  - [ ] Properties: type, category, blockedActions, enabledActions, clearConditions, duration, description

- [ ] Create `src/Content/DTOs/AchievementDTO.cs`
  - [ ] Properties: id, category, name, description, icon, grantConditions

- [ ] Create `src/Content/DTOs/SceneTemplateDTO.cs`
  - [ ] Properties: id, triggerConditions, introNarrative, situationGroup, resolutionNarrative, exclusive

### 2.2: Modify Existing DTOs

**Before Starting:**
- [ ] Search for `PlayerDTO` or equivalent
- [ ] Search for `LocationDTO` or equivalent
- [ ] Search for `NPCDTO` or equivalent
- [ ] Search for `InvestigationDTO` or equivalent
- [ ] Read each DTO completely to understand current structure

**Implementation:**

- [ ] Modify `PlayerDTO`:
  - [ ] Add `public int Resolve { get; set; }`
  - [ ] Add `public List<ScaleDTO> Scales { get; set; }`
  - [ ] Add `public List<string> ActiveStates { get; set; }` (StateType enum strings)
  - [ ] Add `public List<string> Achievements { get; set; }` (Achievement template IDs)
  - [ ] Add `public List<string> CompletedSituationIds { get; set; }`

- [ ] Modify `LocationDTO`:
  - [ ] Add `public List<string> InitialSituations { get; set; }`
  - [ ] REMOVE `public List<string> Goals { get; set; }` or `GoalIds` property
  - [ ] Search for any goal-related properties and DELETE them

- [ ] Modify `NPCDTO`:
  - [ ] Add `public int BondStrength { get; set; }`
  - [ ] Add `public List<string> InitialSituations { get; set; }`
  - [ ] Add `public List<string> Statuses { get; set; }` (NpcStatus enum strings)

- [ ] Modify `InvestigationDTO`:
  - [ ] For phase structure:
    - [ ] Change `public List<string> SpawnedGoals { get; set; }` → `public List<string> SpawnedSituations { get; set; }`
  - [ ] Search entire DTO for "Goal" and rename to "Situation"

### 2.3: Verify Legacy DTO Cleanup (from Prior Refactor)

**Before Starting:**
- [ ] Search for `GoalDTO` class definition
- [ ] Grep entire codebase for references to GoalDTO
- [ ] Verify all parsers use SituationDTO

**Verification:**

- [ ] VERIFY `GoalDTO` does not exist
  - [ ] Expected: Already deleted in prior Goal→Situation refactor
  - [ ] If found: Complete prior refactor cleanup first
  - [ ] Search for all GoalDTO references - should find NONE

---

## Phase 3: Domain Entity Layer

### 3.1: Create New Domain Entities

**Before Starting:**
- [ ] Search `src/GameState` or equivalent for existing entity files
- [ ] Verify entity naming conventions and folder structure
- [ ] Check if entities are one-per-file or grouped
- [ ] Ensure no Situation entity already exists under different name

**Implementation:**

- [ ] **EXTEND** `src/GameState/Situation.cs` (EXISTS - was Goal.cs, renamed in prior refactor)
  - [ ] VERIFY existing properties: Id, PlacementLocationId, PlacementNpcId, Status, SystemType, DeckId, SituationCards, Costs, ConsequenceType
  - [ ] ADD new properties:
    - [ ] TemplateId (string), ParentSituationId (string)
    - [ ] SpawnedDay (int), SpawnedTimeBlock (TimeBlock), SpawnedSegment (int)
    - [ ] CompletedDay? (int?), CompletedTimeBlock? (TimeBlock?), CompletedSegment? (int?)
    - [ ] InteractionType (enum: Instant, Challenge, Navigation) - replaces implicit SystemType check
    - [ ] NavigationPayload (object with DestinationId, AutoTriggerScene)
    - [ ] CompoundRequirement (object with OR paths)
    - [ ] ProjectedBondChanges (List<BondChange>), ProjectedScaleShifts (List<ScaleShift>), ProjectedStates (List<StateApplication>)
    - [ ] OnSuccessSpawns (List<SpawnRule>), OnFailureSpawns (List<SpawnRule>)
    - [ ] Tier (int, 0-4), Repeatable (bool)
    - [ ] GeneratedNarrative (string, cached), NarrativeHints (object)
  - [ ] EXTEND SituationCosts: Add Resolve (int) property
  - [ ] Initialize all new List properties inline: `= new List<T>();`

- [ ] Create `src/GameState/CompoundRequirement.cs`
  - [ ] Properties: OrPaths (List<RequirementPath>)
  - [ ] RequirementPath: StatThresholds (List<StatRequirement>), BondRequirements (List<BondRequirement>), ScaleThresholds (List<ScaleRequirement>), RequiredStates (List<StateType>), BlockedStates (List<StateType>), AchievementIds (List<string>), CoinsRequired (int)
  - [ ] Strong typing (NO dictionaries)

- [ ] Create `src/GameState/SpawnRule.cs`
  - [ ] Properties: TargetTemplateId (string), TargetPlacementId (string), RequirementOffsets (typed object), SpawnConditions (object)

- [ ] Create `src/GameState/PlayerScales.cs` (nested object, NOT list of entities)
  - [ ] Properties (6 int properties):
    - [ ] Morality (int, -10 to +10)
    - [ ] Lawfulness (int, -10 to +10)
    - [ ] Method (int, -10 to +10)
    - [ ] Caution (int, -10 to +10)
    - [ ] Transparency (int, -10 to +10)
    - [ ] Fame (int, -10 to +10)
  - [ ] Initialize all to 0 in constructor

- [ ] Create `src/GameState/ActiveState.cs` (player's active temporary conditions)
  - [ ] Properties:
    - [ ] Type (StateType enum)
    - [ ] Category (StateCategory enum)
    - [ ] AppliedDay (int), AppliedTimeBlock (TimeBlock), AppliedSegment (int)
    - [ ] DurationSegments (int?) - null means manual clear only
  - [ ] NO ClearConditions here - those are in State DEFINITIONS (metadata)

- [ ] Create `src/GameState/PlayerAchievement.cs` (player's earned achievements)
  - [ ] Properties:
    - [ ] AchievementId (string) - reference to achievement definition
    - [ ] EarnedDay (int), EarnedTimeBlock (TimeBlock), EarnedSegment (int)
    - [ ] RelatedNpcId (string?), RelatedLocationId (string?)

- [ ] Create supporting enums in `src/GameState/Enums/`:
  - [ ] `SituationStatus.cs`: Dormant, Available, Active, Completed
  - [ ] `InteractionType.cs`: Instant, Mental, Physical, Social, Navigation
  - [ ] `PlacementType.cs`: Location, NPC, Route
  - [ ] `ScaleType.cs`: Morality, Lawfulness, Method, Caution, Transparency, Fame
  - [ ] `StateType.cs`: All 20-25 state types
  - [ ] `StateCategory.cs`: Physical, Mental, Social
  - [ ] `AchievementCategory.cs`: Combat, Social, Investigation, Economic, Political
  - [ ] `NpcStatus.cs`: Grateful, Betrayed, Allied, Hostile, Trusting, Suspicious, Indebted

### 3.2: Modify Existing Domain Entities

**Before Starting:**
- [ ] Search for `Player` class definition
- [ ] Search for `Location` class definition
- [ ] Search for `NPC` class definition
- [ ] Search for `Investigation` class definition
- [ ] Read each entity completely to understand current properties

**Implementation:**

- [ ] Modify `src/GameState/Player.cs`:
  - [ ] Add `public int Resolve { get; set; } = 30;`
  - [ ] Add `public List<Scale> Scales { get; set; } = new List<Scale>();`
  - [ ] Add `public List<PlayerState> ActiveStates { get; set; } = new List<PlayerState>();`
  - [ ] Add `public List<Achievement> Achievements { get; set; } = new List<Achievement>();`
  - [ ] Add `public List<string> CompletedSituationIds { get; set; } = new List<string>();`
  - [ ] Keep all existing properties (Stats, Focus, Stamina, Health, Coins, Items, etc.)

- [ ] Modify `src/GameState/Location.cs`:
  - [ ] Add `public List<string> AvailableSituationIds { get; set; } = new List<string>();`
  - [ ] REMOVE `public List<string> GoalIds { get; set; }` or similar property
  - [ ] Search for any goal-tracking properties and DELETE them
  - [ ] Keep Investigation Cubes, Obstacle references (these remain)

- [ ] Modify `src/GameState/NPC.cs`:
  - [ ] Add `public int BondStrength { get; set; } = 0;`
  - [ ] Add `public List<NpcStatus> Statuses { get; set; } = new List<NpcStatus>();`
  - [ ] Add `public List<string> AvailableSituationIds { get; set; } = new List<string>();`
  - [ ] Keep Story Cubes property (tactical challenge benefits - unchanged)

- [ ] Modify `src/GameState/Investigation.cs`:
  - [ ] For phase properties:
    - [ ] Change `public List<string> SpawnedGoalIds` → `public List<string> SpawnedSituationIds`
  - [ ] Search entire class for "Goal" and rename to "Situation"

- [ ] Modify `src/GameState/GameWorld.cs`:
  - [ ] Add `public Dictionary<string, Situation> Situations { get; set; } = new Dictionary<string, Situation>();`
  - [ ] Add `public Dictionary<string, Achievement> Achievements { get; set; } = new Dictionary<string, Achievement>();`
  - [ ] Add scale/state definition dictionaries if needed for reference data
  - [ ] REMOVE `public Dictionary<string, Goal> Goals { get; set; }` or similar
  - [ ] Search for Goal dictionary and DELETE it

### 3.3: Verify Legacy Entity Cleanup (from Prior Refactor)

**Before Starting:**
- [ ] Search for `Goal.cs` in src/GameState/
- [ ] Grep entire codebase for "public class Goal" or ": Goal"
- [ ] Verify Situation.cs contains expected properties from Phase 0

**Verification:**

- [ ] VERIFY `Goal.cs` does not exist
  - [ ] Expected: Already renamed to Situation.cs in prior refactor
  - [ ] If found: Phase 0 verification should have caught this - STOP
  - [ ] Situation.cs should exist with PlacementLocationId, SituationCards, etc.

- [ ] VERIFY no "Goal" class references in code
  - [ ] Search for "new Goal()", "List<Goal>", "Goal goal"
  - [ ] Expected: All renamed to Situation equivalents
  - [ ] Only "Goal" references should be in comments: "was Goal, now Situation"

---

## Phase 4: Parser Layer

### 4.1: Create New Parsers

**Before Starting:**
- [ ] Search `src/Content` for existing parser files
- [ ] Identify parser naming conventions and structure
- [ ] Check if parsers are static classes or instance classes
- [ ] Verify folder structure for parsers

**Implementation:**

- [ ] Create `src/Content/Parsers/SituationParser.cs`
  - [ ] Static class with static Parse method
  - [ ] Input: SituationDTO, GameWorld (for template references)
  - [ ] Process:
    - [ ] Parse enum values (InteractionType, PlacementType)
    - [ ] Call RequirementCatalogue to translate categorical requirements
    - [ ] Call CostCatalogue to translate tier-based costs
    - [ ] Parse spawn rules (template references, requirement offsets)
    - [ ] Parse challenge/navigation definitions
    - [ ] Validate all ID references exist in GameWorld
  - [ ] Output: Situation entity
  - [ ] NO JsonElement passthrough (parse everything)

- [ ] Create `src/Content/Parsers/ScaleParser.cs`
  - [ ] Parse ScaleDTO → Scale definitions
  - [ ] Initialize Player.Scales with all scale types at 0

- [ ] Create `src/Content/Parsers/StateParser.cs`
  - [ ] Parse StateDTO → StateType enum catalog
  - [ ] Create lookup for blocked/enabled actions
  - [ ] Define clear condition mappings

- [ ] Create `src/Content/Parsers/AchievementParser.cs`
  - [ ] Parse AchievementDTO → Achievement template catalog
  - [ ] Validate grant conditions reference valid Situations

### 4.2: Create Catalogues

**Before Starting:**
- [ ] Search for existing Catalogue files (SocialCardEffectCatalog, etc.)
- [ ] Understand current catalogue pattern and structure
- [ ] Verify folder location for catalogues

**Implementation:**

- [ ] Create `src/Content/Catalogues/RequirementCatalogue.cs`
  - [ ] Static class with translation methods
  - [ ] `CalculateStatThreshold(categoricalLevel, playerLevel) → int`
  - [ ] `CalculateBondThreshold(relationshipDepth, npcContext) → int`
  - [ ] `BuildCompoundRequirement(requirementRules) → CompoundRequirement`
  - [ ] Use categorical enums (Capable, Commanding, Masterful)
  - [ ] Scale thresholds based on player progression
  - [ ] Return strongly-typed CompoundRequirement objects

- [ ] Create `src/Content/Catalogues/CostCatalogue.cs`
  - [ ] Static class with scaling methods
  - [ ] `ScaleResolveCost(tier, situationTier, playerLevel) → int`
  - [ ] `ScaleSystemCost(tier, challengeType, playerLevel) → int`
  - [ ] `ScaleEconomicCost(tier, playerWealth) → int`
  - [ ] Input: Categorical tier (Low, Medium, High, Extreme)
  - [ ] Output: Absolute integer costs
  - [ ] Scale with progression (higher level = higher costs)

- [ ] Create `src/Content/Catalogues/ConsequenceCatalogue.cs`
  - [ ] Static class with formatting methods
  - [ ] `GetBondChangeSignificance(delta) → enum`
  - [ ] `GetScaleShiftCategory(scaleType, delta) → description`
  - [ ] `FormatConsequenceDisplay(consequence) → string`
  - [ ] Used for UI display and AI hints

### 4.3: Modify Existing Parsers

**Before Starting:**
- [ ] Search for `InvestigationParser` class
- [ ] Search for `LocationParser` class
- [ ] Search for `NPCParser` class
- [ ] Search for `PlayerParser` or player initialization code
- [ ] Read each parser completely

**Implementation:**

- [ ] Modify `src/Content/Parsers/InvestigationParser.cs`:
  - [ ] Find phase parsing logic
  - [ ] Change `ParseGoals()` → `ParseSituations()`
  - [ ] Change property assignment: `phase.SpawnedGoalIds` → `phase.SpawnedSituationIds`
  - [ ] Update to reference Situation template IDs instead of Goal IDs
  - [ ] Search parser for "Goal" and rename to "Situation"

- [ ] Modify `src/Content/Parsers/LocationParser.cs`:
  - [ ] Add parsing for `InitialSituations` property
  - [ ] REMOVE parsing for static Goal lists
  - [ ] Initialize `AvailableSituationIds = new List<string>()` (empty at parse time)

- [ ] Modify `src/Content/Parsers/NPCParser.cs`:
  - [ ] Add parsing for `BondStrength` property (default 0)
  - [ ] Add parsing for `InitialSituations` property
  - [ ] Add parsing for `Statuses` property (parse enum strings)
  - [ ] Initialize `AvailableSituationIds = new List<string>()`

- [ ] Modify player initialization code:
  - [ ] Initialize `Resolve = 30`
  - [ ] Initialize `Scales = new PlayerScales()` (nested object, all properties default to 0)
  - [ ] Initialize `ActiveStates = new List<ActiveState>()`
  - [ ] Initialize `EarnedAchievements = new List<PlayerAchievement>()`
  - [ ] Initialize `CompletedSituationIds = new List<string>()`

### 4.4: Verify Legacy Parser Cleanup (from Prior Refactor)

**Before Starting:**
- [ ] Search for `GoalParser` class
- [ ] Grep codebase for GoalParser references
- [ ] Verify Phase 0 confirmed SituationParser exists

**Verification:**

- [ ] VERIFY `GoalParser.cs` does not exist
  - [ ] Expected: Already renamed to SituationParser in prior refactor
  - [ ] If found: Phase 0 should have caught this - STOP
  - [ ] SituationParser should exist with ConvertDTOToSituation method

---

## Phase 5: Facade Layer (Game Logic)

### 5.1: Create New Facades

**Before Starting:**
- [ ] Search `src/Services` or `src/Facades` for existing facade files
- [ ] Understand current facade pattern (constructor injection, dependencies, etc.)
- [ ] Verify folder structure and naming conventions

**Implementation:**

- [ ] Create `src/Services/SceneFacade.cs`
  - [ ] Dependency: GameWorld (constructor injection)
  - [ ] Method: `GenerateLocationScene(locationId, player) → Scene`
    - [ ] Query Location.AvailableSituationIds
    - [ ] Retrieve Situation entities from GameWorld.Situations
    - [ ] Evaluate each Situation's requirements against player state
    - [ ] Categorize as Available or Locked
    - [ ] Generate intro narrative (AI call - stub for now, implement in AI phase)
    - [ ] Check for Scene template trigger
    - [ ] Categorize Available Situations (Urgent/Progression/Relationship/etc.)
    - [ ] Return Scene object (ephemeral, not stored)
  - [ ] Method: `GenerateNPCScene(npcId, player) → Scene`
  - [ ] Method: `RegenerateScene(locationId, player) → Scene`
  - [ ] Scene object structure (return type, not stored entity)

- [ ] Create `src/Services/SituationFacade.cs`
  - [ ] Dependency: GameWorld, ConsequenceFacade, SpawnFacade, ChallengeFacade
  - [ ] Method: `SelectSituation(situationId, player) → SelectionResult`
    - [ ] Retrieve Situation from GameWorld
    - [ ] Re-validate requirements
    - [ ] Validate costs (player has enough Resolve, Coins, etc.)
    - [ ] Deduct costs from player
    - [ ] Update Situation.Status → Active
    - [ ] Route based on InteractionType
  - [ ] Method: `ResolveInstantSituation(situation, player) → Resolution`
    - [ ] Call ConsequenceFacade.ApplyConsequences
    - [ ] Call SpawnFacade.ExecuteSpawns
    - [ ] Mark Situation.Status → Completed
    - [ ] Generate resolution narrative (AI - stub)
    - [ ] Return Resolution summary
  - [ ] Method: `InitiateChallengeSituation(situation, player) → ChallengeContext`
    - [ ] Create ChallengeContext from Situation.Challenge
    - [ ] Pass to ChallengeFacade
  - [ ] Method: `CompleteChallengeSituation(situation, challengeResult) → Resolution`
    - [ ] Apply GoalCard tactical rewards
    - [ ] Apply Situation strategic consequences
    - [ ] Call SpawnFacade.ExecuteSpawns based on outcome
    - [ ] Mark Situation.Status → Completed
    - [ ] Return Resolution summary
  - [ ] Method: `ResolveNavigationSituation(situation, player) → NavigationResult`

- [ ] Create `src/Services/SpawnFacade.cs`
  - [ ] Dependency: GameWorld
  - [ ] Method: `ExecuteSpawns(parentSituation, outcome, player) → List<Situation>`
    - [ ] Get spawn rules from parent (OnSuccessSpawns or OnFailureSpawns)
    - [ ] For each spawn rule:
      - [ ] Retrieve target template from GameWorld
      - [ ] Instantiate new Situation from template
      - [ ] Calculate requirements (apply offsets using current player state)
      - [ ] Set Status → Dormant
      - [ ] Set ParentSituationId → parent.Id
      - [ ] Add to GameWorld.Situations
      - [ ] Check if immediately Available (requirements met)
      - [ ] Add to target Location/NPC if Available
    - [ ] Apply spawn caps (max 20 Available total, max 5 per location, etc.)
    - [ ] Check archetype cooldowns
    - [ ] Replace lowest-priority if at cap
    - [ ] Return spawned Situation list
  - [ ] Method: `EvaluateDormantSituations(player) → List<Situation>`
    - [ ] Query all Dormant Situations from GameWorld
    - [ ] For each, evaluate requirements against current player state
    - [ ] If any OR path satisfied, update Status → Available
    - [ ] Add to appropriate Location/NPC availability list
    - [ ] Return newly Available Situations

- [ ] Create `src/Services/ConsequenceFacade.cs`
  - [ ] Dependency: GameWorld, SpawnFacade
  - [ ] Method: `ApplyConsequences(consequences, player) → ConsequenceSummary`
    - [ ] Update player resources (Resolve, Coins, Focus, Stamina, Health)
    - [ ] Modify NPC bonds (update player Bond list, NPC BondStrength)
    - [ ] Shift player scales (add deltas, clamp -10 to +10)
    - [ ] Apply or clear states (add/remove from player ActiveStates)
    - [ ] Grant achievements (add to player Achievements)
    - [ ] Call SpawnFacade.EvaluateDormantSituations (newly met requirements)
    - [ ] Return ConsequenceSummary for UI display
  - [ ] Method: `FormatConsequenceDisplay(summary) → FormattedConsequenceData`
    - [ ] Create player-facing text for each change type
    - [ ] "Bond with Martha increased: 5 → 7"
    - [ ] "Morality +2 (now 8 - Altruistic)"

### 5.2: Modify Existing Facades

**Before Starting:**
- [ ] Search for `GameFacade` class
- [ ] Search for `ChallengeFacade` class
- [ ] Read each facade completely to understand current responsibilities

**Implementation:**

- [ ] Modify `src/Services/GameFacade.cs`:
  - [ ] Add dependency: SceneFacade, SituationFacade
  - [ ] Add method: `EnterLocation(locationId) → Scene`
    - [ ] Update Player.CurrentLocationId
    - [ ] Call SceneFacade.GenerateLocationScene
    - [ ] Return Scene for UI display
  - [ ] Add method: `InteractWithNPC(npcId) → Scene`
    - [ ] Call SceneFacade.GenerateNPCScene
    - [ ] Return Scene for UI display
  - [ ] Add method: `SelectSituation(situationId) → SelectionResult`
    - [ ] Call SituationFacade.SelectSituation
    - [ ] Route based on result
  - [ ] REMOVE any Goal-related methods
  - [ ] Search facade for "Goal" and delete/refactor to Situation

- [ ] Modify `src/Services/ChallengeFacade.cs`:
  - [ ] Minimal changes (existing challenge systems work unchanged)
  - [ ] Method: `LaunchChallenge(challengeContext) → Challenge`
    - [ ] Accept Situation.Challenge payload instead of Goal
    - [ ] Otherwise identical implementation
  - [ ] Method: `ResolveChallenge(challenge) → ChallengeResult`
    - [ ] Return GoalCard rewards + outcome
    - [ ] Return to SituationFacade instead of updating GameWorld directly

### 5.3: Delete Obsolete Facades

**Before Starting:**
- [ ] Search for any Goal-specific facades
- [ ] Verify they're no longer needed

**Implementation:**

- [ ] DELETE any `GoalFacade.cs` or `GoalManagementFacade.cs`
  - [ ] Only if separate facade existed for Goal logic
  - [ ] Replaced by SituationFacade

---

## Phase 6: UI Component Layer

### 6.1: Create New UI Components

**Before Starting:**
- [ ] Search `src/UI` or `src/Components` for existing component files
- [ ] Understand Blazor component structure and conventions
- [ ] Check folder organization (by feature? by type?)
- [ ] Verify naming conventions (.razor files)

**Implementation:**

- [ ] Create `src/UI/Screens/LocationSceneScreen.razor`
  - [ ] Parameter: Scene CurrentScene
  - [ ] Layout sections:
    - [ ] Location header (name, time, resources)
    - [ ] Scene narrative section (display Scene.GeneratedNarrative)
    - [ ] Situations section with CategoryTabs
    - [ ] Exit location button
  - [ ] Event handlers:
    - [ ] SelectSituation (calls parent GameScreen.SelectSituation)
    - [ ] ExitLocation (navigates back to map)
    - [ ] StayAtLocation (regenerates Scene)

- [ ] Create `src/UI/Components/SituationCard.razor`
  - [ ] Parameter: Situation CurrentSituation, bool IsLocked
  - [ ] Display structure:
    - [ ] Title bar (name, interaction type icon, status badge)
    - [ ] Requirements section (if locked - RequirementPathDisplay component)
    - [ ] Costs section (Resolve, System cost, Coins, Time)
    - [ ] Projected consequences section (bond changes, scale shifts, etc.)
    - [ ] Description text
    - [ ] Select button (if available) or locked indicator
  - [ ] Visual states: Available (green), Locked (gray), Urgent (red), Selected (expanded)
  - [ ] Click to expand/collapse functionality

- [ ] Create `src/UI/Components/ConsequenceModal.razor`
  - [ ] Parameter: Resolution CurrentResolution, bool Visible
  - [ ] Display structure:
    - [ ] Modal header "Resolution"
    - [ ] Narrative section (generated resolution text)
    - [ ] Changes section (resources, relationships, reputation, conditions, milestones)
    - [ ] Spawned Situations section (new opportunities, where they are)
    - [ ] Continue button with Stay/Leave options
  - [ ] Event handlers:
    - [ ] Stay (calls parent RegenerateScene)
    - [ ] Leave (calls parent NavigateToMap)

- [ ] Create `src/UI/Components/RequirementPathDisplay.razor`
  - [ ] Parameter: CompoundRequirement Requirements, Player CurrentPlayer
  - [ ] Display structure:
    - [ ] For each OR path:
      - [ ] Progress bar (percentage met)
      - [ ] Requirement list (checkmarks for met, X for unmet)
      - [ ] Missing requirements summary
      - [ ] "Show How" button (links to relevant Situations)
    - [ ] Highlight closest path
  - [ ] Calculate progress percentages for each path

- [ ] Create `src/UI/Components/CategoryTabs.razor`
  - [ ] Parameter: List<Situation> AllSituations
  - [ ] Tab structure:
    - [ ] Urgent (red badge with count)
    - [ ] Progression (gold badge with count)
    - [ ] Relationship (blue badge with count)
    - [ ] Opportunity (green badge with count)
    - [ ] Exploration (purple badge with count)
  - [ ] Each tab contains filtered SituationCard components
  - [ ] Collapsible functionality (default: Urgent + Progression expanded)

- [ ] Create `src/UI/Screens/NPCSceneScreen.razor`
  - [ ] Similar to LocationSceneScreen
  - [ ] Additional elements:
    - [ ] NPC portrait
    - [ ] Bond meter (visual representation)
    - [ ] NPC statuses display (Grateful, Allied, etc.)
  - [ ] Greeting narrative references relationship

### 6.2: Modify Existing UI Components

**Before Starting:**
- [ ] Search for `WorldMapScreen.razor`
- [ ] Search for `JournalScreen.razor`
- [ ] Search for `GameScreen.razor`
- [ ] Read each component completely to understand current structure

**Implementation:**

- [ ] Modify `src/UI/Screens/WorldMapScreen.razor`:
  - [ ] Add Situation count displays to locations
    - [ ] "Old Mill (3 available, 2 locked, 1 urgent)"
  - [ ] Add urgent indicators (red pulsing if location has urgent Situations)
  - [ ] Add tooltip showing Situation counts on hover
  - [ ] Change location click handler:
    - [ ] Call GameFacade.EnterLocation
    - [ ] Navigate to LocationSceneScreen (not direct location screen)
  - [ ] REMOVE any Goal-related displays

- [ ] Modify `src/UI/Screens/JournalScreen.razor`:
  - [ ] Add new tabs:
    - [ ] Active Situations (organized by location)
    - [ ] Dormant Situations (locked, shows requirements and progress)
    - [ ] Completed Situations (history log, shows cascade chains)
    - [ ] Achievements (earned milestones, organized by category)
    - [ ] Scales (current values with spectrum visualization)
    - [ ] Active States (temporary conditions with clear conditions)
  - [ ] REMOVE Goal-related tabs
  - [ ] Update Investigation tab to reference Situations instead of Goals

- [ ] Modify `src/UI/Screens/GameScreen.razor`:
  - [ ] Add state properties:
    - [ ] Scene? CurrentScene
    - [ ] Resolution? CurrentResolution
    - [ ] bool ShowConsequenceModal
  - [ ] Add navigation methods:
    - [ ] EnterLocation(locationId)
    - [ ] InteractWithNPC(npcId)
    - [ ] SelectSituation(situationId)
    - [ ] ShowConsequenceModal(resolution)
    - [ ] RegenerateScene()
  - [ ] Add routes:
    - [ ] LocationScene route
    - [ ] NPCScene route
  - [ ] Update cascading parameters passed to children
  - [ ] REMOVE Goal-related state and methods

### 6.3: Delete Obsolete UI Components

**Before Starting:**
- [ ] Search for Goal-specific UI components
- [ ] Search for static Location screen (if Scene-based replaces it)
- [ ] Grep for references to ensure safe to delete

**Implementation:**

- [ ] DELETE `src/UI/Components/GoalCard.razor` (if exists)
  - [ ] Replaced by SituationCard
  - [ ] Verify no references remain

- [ ] DELETE static `LocationScreen.razor` (if Scene-based replaces it)
  - [ ] Replaced by LocationSceneScreen
  - [ ] Only if completely replaced by Scene pattern

- [ ] DELETE any `GoalListDisplay.razor` or similar
  - [ ] Replaced by CategoryTabs + SituationCard

---

## Phase 7: State Management & Persistence

### 7.1: Modify Save/Load System

**Before Starting:**
- [ ] Search for save/load logic (SaveService, GameStateService, etc.)
- [ ] Read save file structure completely
- [ ] Understand current serialization approach (JSON, binary, etc.)

**Implementation:**

- [ ] Modify save data structure:
  - [ ] Player section:
    - [ ] Add resolve, scales, activeStates, achievements, completedSituationIds
  - [ ] Locations section:
    - [ ] Add availableSituationIds
    - [ ] Remove static goalIds if present
  - [ ] NPCs section:
    - [ ] Add bondStrength, statuses, availableSituationIds
  - [ ] New Situations section:
    - [ ] Save all Situation instances (status, requirements, narrative cache, timestamps, parent references)
  - [ ] Remove Goals section

- [ ] Modify save method:
  - [ ] Serialize new player properties
  - [ ] Serialize Situation collection from GameWorld
  - [ ] Serialize Location/NPC availability lists
  - [ ] REMOVE Goal serialization

- [ ] Modify load method:
  - [ ] Deserialize new player properties
  - [ ] Deserialize Situations collection
  - [ ] Rebuild GameWorld.Situations dictionary
  - [ ] Deserialize Location/NPC availability lists
  - [ ] Call SpawnFacade.EvaluateDormantSituations to update statuses
  - [ ] REMOVE Goal deserialization

- [ ] Add migration logic for old saves:
  - [ ] Detect missing new fields
  - [ ] Initialize with defaults (Resolve: 30, Scales: all 0, States: empty, Achievements: empty)
  - [ ] Convert Goal references to Situation references where possible
  - [ ] Log migration warnings

### 7.2: Delete Legacy Save Code

**Before Starting:**
- [ ] Identify Goal-specific save/load code
- [ ] Verify it's safe to delete after migration

**Implementation:**

- [ ] DELETE Goal save/load methods
  - [ ] Remove from save service
  - [ ] Remove Goal-specific serialization logic

---

## Phase 8: AI Integration (Narrative Generation)

### 8.1: Create AI Context System

**Before Starting:**
- [ ] Search for existing AI integration (if any)
- [ ] Determine AI service architecture (external API? local model? service class?)
- [ ] Understand current prompt structure if AI already used

**Implementation:**

- [ ] Create `src/Services/AI/AIContextBuilder.cs`
  - [ ] Method: `BuildSceneContext(location, player) → SceneContext`
    - [ ] Query player achievements (relevant to location)
    - [ ] Query NPC bonds at location
    - [ ] Query player scales with categorical labels
    - [ ] Query active states with descriptions
    - [ ] Query recent completed Situations (last 5-10)
    - [ ] Query location familiarity (Investigation Cubes)
    - [ ] Query current obstacles present
    - [ ] Return structured context object
  - [ ] Method: `BuildConsequenceContext(situation, outcome, consequences, spawned) → ConsequenceContext`
  - [ ] Method: `BuildRequirementContext(situation, requirement, player) → RequirementContext`

- [ ] Create `src/Services/AI/AIPromptBuilder.cs`
  - [ ] Method: `BuildSceneNarrativePrompt(context) → string`
    - [ ] System role definition
    - [ ] Context data formatting
    - [ ] Task specification
    - [ ] Requirements and constraints
    - [ ] Tone and setting guidelines
    - [ ] Avoid list (no anachronisms, no numbers, no meta-references)
  - [ ] Method: `BuildConsequenceNarrativePrompt(context) → string`
  - [ ] Method: `BuildRequirementExplanationPrompt(context) → string`

- [ ] Create `src/Services/AI/AIService.cs`
  - [ ] Dependency: HTTP client or AI SDK
  - [ ] Method: `GenerateSceneNarrative(prompt) → string`
    - [ ] Call AI API with prompt
    - [ ] Return generated text
    - [ ] Handle errors (return fallback text if fails)
  - [ ] Method: `GenerateConsequenceNarrative(prompt) → string`
  - [ ] Method: `GenerateRequirementExplanation(prompt) → string`

### 8.2: Integrate AI Calls into Facades

**Before Starting:**
- [ ] Verify AIService exists and is injectable
- [ ] Understand async patterns used in project

**Implementation:**

- [ ] Modify `SceneFacade.GenerateLocationScene`:
  - [ ] Add dependency: AIService, AIContextBuilder, AIPromptBuilder
  - [ ] After evaluating Situations, before returning Scene:
    - [ ] Build context using AIContextBuilder
    - [ ] Build prompt using AIPromptBuilder
    - [ ] Call AIService.GenerateSceneNarrative
    - [ ] Store in Scene.GeneratedNarrative
    - [ ] If AI fails, use fallback template text
  - [ ] Make method async if needed

- [ ] Modify `SituationFacade.ResolveInstantSituation`:
  - [ ] After applying consequences, before returning Resolution:
    - [ ] Build consequence context
    - [ ] Build consequence prompt
    - [ ] Call AIService.GenerateConsequenceNarrative
    - [ ] Store in Resolution.Narrative
    - [ ] If AI fails, use fallback template text

- [ ] Modify `SituationFacade.CompleteChallengeSituation`:
  - [ ] Same AI integration as ResolveInstantSituation

### 8.3: Create Narrative Caching System

**Before Starting:**
- [ ] Determine if caching needed (for performance)
- [ ] Understand session state management

**Implementation:**

- [ ] Create `src/Services/NarrativeCacheService.cs`
  - [ ] Dictionary: SceneId → Generated narrative
  - [ ] Dictionary: SituationId → Generated narrative
  - [ ] Method: `CacheSceneNarrative(sceneId, narrative)`
  - [ ] Method: `GetCachedSceneNarrative(sceneId) → string?`
  - [ ] Method: `CacheSituationNarrative(situationId, narrative)`
  - [ ] Method: `GetCachedSituationNarrative(situationId) → string?`
  - [ ] Clear cache on new game / load game

- [ ] Integrate caching into SceneFacade:
  - [ ] Check cache before generating
  - [ ] Store in cache after generating
  - [ ] Cache key: locationId + visitNumber + player state hash

### 8.4: Create Fallback Text System

**Before Starting:**
- [ ] Define fallback text templates
- [ ] Ensure fallbacks cover all generation types

**Implementation:**

- [ ] Create `src/Services/FallbackNarrativeService.cs`
  - [ ] Method: `GetFallbackSceneNarrative(location) → string`
    - [ ] Template-based text with variable substitution
    - [ ] "You arrive at {location.name}."
  - [ ] Method: `GetFallbackConsequenceNarrative(situation) → string`
    - [ ] "Your action has consequences."
  - [ ] Method: `GetFallbackRequirementExplanation(requirement) → string`
    - [ ] "This action requires specific capabilities."

- [ ] Integrate into AI service error handling:
  - [ ] If AI call fails 3 times, use fallback
  - [ ] If AI call times out, use fallback
  - [ ] Log all fallback usage for debugging

---

## Phase 9: Content Population

### 9.0: Create Tier 0 (A Thread) Safety Net

**Before Starting:**
- [ ] Read A/B/C Thread Architecture section in SCENE_SITUATION_ARCHITECTURE.md
- [ ] Identify hub location in locations.json (default: Brass Bell Inn - Common Room)
- [ ] Understand safety net guarantee (3 Situations, zero requirements, infinite repeat)

**Implementation:**

- [ ] Create Tier 0 Situation templates in situations.json:
  - [ ] `hub_physical_work` template:
    - [ ] ID: "hub_physical_work"
    - [ ] Name: "Load Wagons" (or contextually appropriate for hub type)
    - [ ] Requirements: {} (empty - ZERO requirements)
    - [ ] Costs: {stamina: 5} (renewable resource ONLY, NO Resolve)
    - [ ] Rewards: {coins: 8, statXP: {authority: 1}}
    - [ ] InteractionType: "Instant" (no challenge)
    - [ ] Tier: 0
    - [ ] Repeatable: true
    - [ ] DeleteOnSuccess: false
    - [ ] Spawns: [] (empty - no cascades)
    - [ ] NarrativeHints: {tone: "generic", style: "formulaic_work"}
    - [ ] PlacementType: "Location"
    - [ ] TargetPlacementId: "common_room" (hub location ID)

  - [ ] `hub_mental_work` template:
    - [ ] ID: "hub_mental_work"
    - [ ] Name: "Count Inventory" (or contextually appropriate)
    - [ ] Requirements: {} (empty - ZERO requirements)
    - [ ] Costs: {focus: 3} (renewable resource ONLY, NO Resolve)
    - [ ] Rewards: {coins: 7, statXP: {insight: 1}}
    - [ ] InteractionType: "Instant"
    - [ ] Tier: 0
    - [ ] Repeatable: true
    - [ ] DeleteOnSuccess: false
    - [ ] Spawns: [] (empty)
    - [ ] NarrativeHints: {tone: "generic", style: "formulaic_work"}
    - [ ] PlacementType: "Location"
    - [ ] TargetPlacementId: "common_room"

  - [ ] `hub_social_work` template:
    - [ ] ID: "hub_social_work"
    - [ ] Name: "Serve Customers" (or contextually appropriate)
    - [ ] Requirements: {} (empty - ZERO requirements)
    - [ ] Costs: {time: 2} (renewable resource ONLY, NO Resolve)
    - [ ] Rewards: {coins: 6, statXP: {rapport: 1}}
    - [ ] InteractionType: "Instant"
    - [ ] Tier: 0
    - [ ] Repeatable: true
    - [ ] DeleteOnSuccess: false
    - [ ] Spawns: [] (empty)
    - [ ] NarrativeHints: {tone: "generic", style: "formulaic_work"}
    - [ ] PlacementType: "Location"
    - [ ] TargetPlacementId: "common_room"

- [ ] Modify hub location in locations.json:
  - [ ] Add `isHub: true` property to Brass Bell Inn - Common Room
  - [ ] Add `tier0Situations: ["hub_physical_work", "hub_mental_work", "hub_social_work"]`
  - [ ] Ensure always accessible (no blocking properties)

- [ ] Create AI prompt templates for Tier 0 narrative generation:
  - [ ] Formulaic template: "You spend [time] [activity]. [NPC] [minimal reaction]. The work is [adjective] but pays."
  - [ ] Physical example: "You spend hours moving heavy sacks. Thomas nods but doesn't speak. Exhausting but pays."
  - [ ] Mental example: "You carefully count inventory. Tedious but necessary work that earns coins."
  - [ ] Social example: "You help customers at the counter. Simple interaction, modest pay."

**Validation Checklist:**
- [ ] Exactly 3 Tier 0 templates created
- [ ] All have Requirements: {} (empty object)
- [ ] NONE use Resolve in costs (only Stamina/Focus/Time)
- [ ] All have Repeatable: true
- [ ] All have DeleteOnSuccess: false
- [ ] All have Spawns: [] (empty)
- [ ] All have InteractionType: "Instant"
- [ ] All assigned to hub location
- [ ] Hub location has isHub: true

### 9.1: Create Tier 1-4 Situation Templates (B/C Thread)

**Before Starting:**
- [ ] Review situations.json structure
- [ ] Understand all interaction types
- [ ] Plan template variety (cover all categories)
- [ ] Ensure Tier 0 (A Thread) is complete first

**Implementation:**

- [ ] Create 50+ Situation templates in situations.json:
  - [ ] Tier 1 templates (10-15):
    - [ ] Always-available universal actions (Work, Rest, Observe)
    - [ ] Low requirement thresholds
    - [ ] Small costs (0-3 Resolve)
    - [ ] Simple spawn chains
  - [ ] Tier 2 templates (20-25):
    - [ ] Standard progression content
    - [ ] Moderate requirements (compound OR with 2-3 paths)
    - [ ] Medium costs (5-8 Resolve)
    - [ ] 2-3 level spawn chains
  - [ ] Tier 3 templates (10-15):
    - [ ] High-stakes content
    - [ ] Complex requirements (3-4 OR paths)
    - [ ] High costs (10-15 Resolve)
    - [ ] Deep spawn chains (4-5 levels)
  - [ ] Tier 4 templates (5-10):
    - [ ] Climactic moments
    - [ ] Very complex requirements
    - [ ] Extreme costs (18-25 Resolve)
    - [ ] Resolution/ending content
  - [ ] Cover all interaction types:
    - [ ] Instant (20 templates)
    - [ ] Mental Challenge (10 templates)
    - [ ] Physical Challenge (10 templates)
    - [ ] Social Challenge (10 templates)
    - [ ] Navigation (5 templates)
  - [ ] Cover all categories:
    - [ ] Urgent (5 templates)
    - [ ] Progression (15 templates)
    - [ ] Relationship (10 templates)
    - [ ] Opportunity (10 templates)
    - [ ] Exploration (10 templates)

- [ ] Define spawn chain examples:
  - [ ] Create 5-10 complete cascade chains
  - [ ] Each chain: Parent → Child1 → Child2 → Child3 → Child4
  - [ ] Use requirement offsets: "current + X"
  - [ ] Use conditional spawning: "if Scale.Morality >= 5"
  - [ ] Demonstrate branching (success vs failure spawns)

- [ ] Create compound requirement examples:
  - [ ] Every Tier 2+ template has 2-3 OR paths
  - [ ] Examples:
    - [ ] Path1: Bond + Scale requirement
    - [ ] Path2: Stat requirement
    - [ ] Path3: Economic requirement
  - [ ] Demonstrate all requirement types (stats, bonds, scales, states, achievements, coins)

### 9.2: Assign Initial Situations to Locations/NPCs

**Before Starting:**
- [ ] Review all locations in locations.json
- [ ] Review all NPCs in npcs.json

**Implementation:**

- [ ] Modify locations.json:
  - [ ] For each location, add `initialSituations` list
  - [ ] Assign 2-3 Tier 1 Situations per location
  - [ ] Ensure variety (not all work/rest, include exploration)

- [ ] Modify npcs.json:
  - [ ] For each NPC, add `initialSituations` list
  - [ ] Assign 1-2 Tier 1 Social Situations per NPC
  - [ ] First meeting Situations (low Bond requirements)

### 9.3: Update Investigation Content

**Before Starting:**
- [ ] Review investigations.json completely
- [ ] Understand current Investigation structure

**Implementation:**

- [ ] Modify investigations.json:
  - [ ] For each Investigation:
    - [ ] For each phase:
      - [ ] Replace `spawnedGoals` with `spawnedSituations`
      - [ ] Reference Situation template IDs
      - [ ] Ensure Situations have appropriate requirements (phase-gated)
  - [ ] Verify Investigation → Obstacle → Situation flow works

---

## Phase 10: Integration & Wiring

### 10.1: Update Dependency Injection

**Before Starting:**
- [ ] Search for `Startup.cs` or `Program.cs` (DI configuration)
- [ ] Understand current service registration pattern

**Implementation:**

- [ ] Register new facades:
  - [ ] `services.AddSingleton<SceneFacade>();`
  - [ ] `services.AddSingleton<SituationFacade>();`
  - [ ] `services.AddSingleton<SpawnFacade>();`
  - [ ] `services.AddSingleton<ConsequenceFacade>();`
  - [ ] `services.AddSingleton<AIService>();`
  - [ ] `services.AddSingleton<AIContextBuilder>();`
  - [ ] `services.AddSingleton<AIPromptBuilder>();`
  - [ ] `services.AddSingleton<NarrativeCacheService>();`
  - [ ] `services.AddSingleton<FallbackNarrativeService>();`

- [ ] Remove obsolete service registrations:
  - [ ] Remove `GoalFacade` if it existed
  - [ ] Remove any Goal-specific services

### 10.2: Update PackageLoader

**Before Starting:**
- [ ] Search for `PackageLoader.cs`
- [ ] Read loading sequence completely
- [ ] Understand loading order dependencies

**Implementation:**

- [ ] Modify `src/Content/PackageLoader.cs`:
  - [ ] Add loading for situations.json (call SituationParser)
  - [ ] Add loading for scales.json (call ScaleParser)
  - [ ] Add loading for states.json (call StateParser)
  - [ ] Add loading for achievements.json (call AchievementParser)
  - [ ] REMOVE loading for goals.json (no longer exists)
  - [ ] Update loading order:
    - [ ] Load scales/states/achievements early (referenced by Situations)
    - [ ] Load Situations after scales/states/achievements
    - [ ] Load Situations before Investigations (Investigations reference Situations)
  - [ ] Populate GameWorld.Situations dictionary
  - [ ] REMOVE GameWorld.Goals population

### 10.3: Update GameWorldInitializer

**Before Starting:**
- [ ] Search for `GameWorldInitializer.cs`
- [ ] Understand initialization sequence

**Implementation:**

- [ ] Modify `src/Content/GameWorldInitializer.cs`:
  - [ ] Initialize Player with new properties:
    - [ ] Resolve = 30
    - [ ] Scales = all 6 types at 0
    - [ ] ActiveStates = empty
    - [ ] Achievements = empty
    - [ ] CompletedSituationIds = empty
  - [ ] Initialize Locations with AvailableSituationIds = empty
  - [ ] Initialize NPCs with BondStrength = 0, Statuses = empty, AvailableSituationIds = empty
  - [ ] After loading content, spawn initial Situations:
    - [ ] For each Location.InitialSituations, instantiate Situations
    - [ ] For each NPC.InitialSituations, instantiate Situations
    - [ ] Add to GameWorld.Situations
    - [ ] Add to Location/NPC AvailableSituationIds
  - [ ] REMOVE Goal initialization

### 10.4: Update Navigation Flow

**Before Starting:**
- [ ] Understand current navigation between screens
- [ ] Identify all entry points to location/NPC interaction

**Implementation:**

- [ ] Modify GameScreen navigation logic:
  - [ ] When entering location:
    - [ ] Call GameFacade.EnterLocation
    - [ ] Receive Scene
    - [ ] Navigate to LocationSceneScreen
    - [ ] Pass Scene as parameter
  - [ ] When interacting with NPC:
    - [ ] Call GameFacade.InteractWithNPC
    - [ ] Receive Scene
    - [ ] Navigate to NPCSceneScreen
    - [ ] Pass Scene as parameter
  - [ ] REMOVE direct navigation to static location screens

---

## Phase 11: Validation & Cleanup

### 11.1: Remove All Goal References

**Before Starting:**
- [ ] Perform global search: "Goal" (case-sensitive)
- [ ] Perform global search: "goal" (case-insensitive)
- [ ] Review every result manually

**Implementation:**

- [ ] Search entire codebase for "Goal":
  - [ ] Rename class references: `Goal` → `Situation`
  - [ ] Rename variable names: `goal` → `situation`
  - [ ] Rename collection names: `Goals` → `Situations`
  - [ ] Rename property names: `GoalId` → `SituationId`
  - [ ] Exception: Keep `GoalCard` (different entity, tactical layer)
  - [ ] Verify each replacement manually (don't blindly replace)

- [ ] Delete files:
  - [ ] Delete `Goal.cs` (if separate Situation created)
  - [ ] Delete `GoalDTO.cs`
  - [ ] Delete `GoalParser.cs`
  - [ ] Delete `goals.json`
  - [ ] Delete any `GoalFacade.cs`
  - [ ] Delete any UI components: `GoalCard.razor`, `GoalList.razor`

- [ ] Clean up comments:
  - [ ] Search for comments referencing "Goal"
  - [ ] Update or delete obsolete comments
  - [ ] Remove all TODO comments about Goals

### 11.2: Verify Strong Typing

**Before Starting:**
- [ ] Search for `Dictionary<string, object>`
- [ ] Search for `Dictionary<string, >`
- [ ] Search for `HashSet<`
- [ ] Search for `var `

**Implementation:**

- [ ] Search for forbidden patterns:
  - [ ] `Dictionary<string, object>` → Replace with strongly-typed classes
  - [ ] `Dictionary<string, X>` → Replace with `List<X>` or typed class
  - [ ] `HashSet<T>` → Replace with `List<T>`
  - [ ] `var` → Replace with explicit type names
  - [ ] Any `SharedData`, `Context`, `Metadata` dictionaries → DELETE

- [ ] Verify all relationships are typed:
  - [ ] Situation.Requirements is CompoundRequirement (not Dictionary)
  - [ ] Situation.SpawnRules is List<SpawnRule> (not Dictionary)
  - [ ] Player.Scales is List<Scale> (not Dictionary<string, int>)
  - [ ] All entity references are strongly typed

### 11.3: Delete Legacy Code Blocks

**Before Starting:**
- [ ] Search for `// TODO`
- [ ] Search for `// LEGACY`
- [ ] Search for `// OLD`
- [ ] Search for commented-out code blocks

**Implementation:**

- [ ] Delete all TODO comments:
  - [ ] No TODOs allowed in production code
  - [ ] Either implement or delete

- [ ] Delete all commented-out code:
  - [ ] No legacy code blocks
  - [ ] If needed, exists in version control history

- [ ] Delete all compatibility layers:
  - [ ] No "old and new" coexistence
  - [ ] No dual code paths for migration

### 11.4: Verify Architecture Compliance

**Before Starting:**
- [ ] Re-read game-design-principles.md
- [ ] Review architecture constraints

**Implementation:**

- [ ] Verify Principle 1 (Single Source of Truth):
  - [ ] GameWorld owns all Situations (check)
  - [ ] Locations/NPCs reference Situations (placement, not ownership) (check)
  - [ ] No duplicate ownership (check)

- [ ] Verify Principle 2 (Strong Typing):
  - [ ] No dictionaries of generic types (check)
  - [ ] All collections are List<T> with typed T (check)
  - [ ] No var, no object, no HashSet (check)

- [ ] Verify Principle 3 (Ownership vs Placement):
  - [ ] Situations placed at Locations (metadata) (check)
  - [ ] GameWorld owns Situations (lifecycle) (check)
  - [ ] Distinction clear (check)

- [ ] Verify Principle 4 (No Boolean Gates):
  - [ ] All requirements are accumulative numerical (check)
  - [ ] Achievements part of compound OR (not sole gates) (check)
  - [ ] States temporary (not permanent flags) (check)
  - [ ] Resource costs for content access (check)

- [ ] Verify Principle 5 (Typed Rewards):
  - [ ] Situation consequences are typed objects (check)
  - [ ] Applied at completion (not continuous evaluation) (check)
  - [ ] Explicit connections (check)

- [ ] Verify Principle 6 (Resource Scarcity):
  - [ ] Resolve creates universal scarcity (check)
  - [ ] Focus/Stamina remain system-specific (check)
  - [ ] Impossible choices created (check)

- [ ] Verify Principle 7 (One Purpose Per Entity):
  - [ ] Situations: strategic choices (check)
  - [ ] GoalCards: tactical victory conditions (check)
  - [ ] Scenes: UI wrappers (check)
  - [ ] No multi-purpose entities (check)

- [ ] Verify Principle 10 (Perfect Information):
  - [ ] All requirements visible (check)
  - [ ] Locked Situations show why locked (check)
  - [ ] Costs displayed before selection (check)
  - [ ] Strategic vs tactical layer separated (check)

---

## Phase 12: Final Passes

### 12.1: Comprehensive Reference Verification

**Before Starting:**
- [ ] Prepare for full codebase scan
- [ ] Allocate time for thorough verification

**Implementation:**

- [ ] Verify all template ID references:
  - [ ] Grep for all Situation template IDs in JSON
  - [ ] Verify each is defined in situations.json
  - [ ] No broken references

- [ ] Verify all placement ID references:
  - [ ] Grep for all Location IDs referenced by Situations
  - [ ] Verify each Location exists in locations.json
  - [ ] Grep for all NPC IDs referenced by Situations
  - [ ] Verify each NPC exists in npcs.json

- [ ] Verify all parent-child references:
  - [ ] Trace spawn chains for circular references
  - [ ] Verify maximum depth doesn't exceed limit
  - [ ] No orphaned Situations (no valid parent)

- [ ] Verify all enum values:
  - [ ] All InteractionType values defined in enum
  - [ ] All PlacementType values defined in enum
  - [ ] All ScaleType values defined in enum
  - [ ] All StateType values defined in enum
  - [ ] All AchievementCategory values defined in enum
  - [ ] All NpcStatus values defined in enum
  - [ ] No undefined enum values in JSON

### 12.2: Delete Empty Classes

**Before Starting:**
- [ ] Search for classes with no logic
- [ ] Identify classes that became empty after refactoring

**Implementation:**

- [ ] Delete empty classes:
  - [ ] If Goal class became empty wrapper after rename, delete it
  - [ ] If any service/facade became empty after refactoring, delete it
  - [ ] If any DTO became empty, delete it
  - [ ] Remove all empty files from project

- [ ] Update project references:
  - [ ] Remove deleted files from .csproj
  - [ ] Remove deleted files from any import lists

### 12.3: Clean Imports

**Before Starting:**
- [ ] Understand current namespace structure
- [ ] Identify unused imports

**Implementation:**

- [ ] Remove unused using statements:
  - [ ] Every file that imported Goal namespace
  - [ ] Every file that imported obsolete DTOs
  - [ ] Every file that imported deleted services
  - [ ] Run IDE cleanup (remove unused usings)

- [ ] Update namespace references:
  - [ ] Situation namespace imports where needed
  - [ ] New facade namespace imports where needed

### 12.4: Documentation Update

**Before Starting:**
- [ ] Search for README files
- [ ] Search for inline documentation
- [ ] Identify all documentation that references Goals

**Implementation:**

- [ ] Update documentation files:
  - [ ] Search README.md for "Goal" references
  - [ ] Update to "Situation"
  - [ ] Update architecture diagrams
  - [ ] Update data flow descriptions

- [ ] Update inline documentation:
  - [ ] Search for /// XML comments referencing Goals
  - [ ] Update to reference Situations
  - [ ] Add documentation for new facades
  - [ ] Add documentation for new entities

---

## Completion Checklist

### Architecture Verification

- [ ] GameWorld owns all Situations (single source of truth)
- [ ] No dictionaries of generic types anywhere in codebase
- [ ] All collections are List<T> with strongly-typed T
- [ ] No boolean gates (all requirements are accumulative numerical)
- [ ] Compound OR requirements implemented (multiple unlock paths)
- [ ] Situations spawn from parents (cascade mechanics working)
- [ ] Scenes generate per visit (ephemeral UI constructs)
- [ ] Perfect information display (locked Situations show requirements)

### Code Quality Verification

- [ ] No Goal references remain (except GoalCard which is separate)
- [ ] No legacy code blocks or TODO comments
- [ ] No compatibility layers
- [ ] No empty classes or orphaned files
- [ ] All imports cleaned (no unused usings)
- [ ] Strong typing enforced everywhere
- [ ] Entity relationships clear (ownership vs placement vs reference)

### Functionality Verification

- [ ] Player has Resolve, Scales, States, Achievements properties
- [ ] Locations have AvailableSituationIds lists
- [ ] NPCs have BondStrength and AvailableSituationIds
- [ ] Situations parse from JSON correctly
- [ ] Situations spawn with calculated requirements
- [ ] Dormant → Available evaluation works
- [ ] Spawn cascade chains execute
- [ ] Consequence application updates player state
- [ ] AI narrative generation integrated (or stubbed for future)
- [ ] Save/load includes new properties
- [ ] UI displays Scenes with categorized Situations
- [ ] Consequence Modal shows spawns and changes

### Content Verification

- [ ] 50+ Situation templates created
- [ ] All interaction types represented
- [ ] All tiers (1-4) represented
- [ ] Spawn chain examples defined
- [ ] Compound requirement examples defined
- [ ] Initial Situations assigned to Locations/NPCs
- [ ] Investigations updated to reference Situations

---

## Notes

**This implementation plan achieves 100% Scene-Situation architecture as specified in SCENE_SITUATION_ARCHITECTURE.md**

**Breaking changes are acceptable and expected during implementation**

**Playability is not maintained during intermediate states**

**Speed of implementation is prioritized over safety**

**All legacy Goal system code is completely removed with no compatibility layers**

**The result is a complete architectural transformation from static Goals to dynamic Situations with Scene-based presentation, cascading spawns, compound OR requirements, and AI narrative generation**
