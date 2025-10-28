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

**6. STRATEGIC/TACTICAL LAYER SEPARATION (Resolve Consumption Architecture)**

**CRITICAL PRINCIPLE:** Tactical layer (Mental/Physical/Social challenge facades) MUST NEVER know about strategic resources like Resolve, Scales, CompoundRequirements, or any strategic concepts.

**The Separation:**

```
STRATEGIC LAYER (Situation Selection)
  ↓ Evaluates CompoundRequirements (unlock paths)
  ↓ Consumes STRATEGIC costs (Resolve, Time, Coins)
  ↓ Routes to appropriate subsystem based on InteractionType
  ↓
TACTICAL LAYER (Challenge Execution)
  ↓ Receives challenge payload ONLY (deck, target, GoalCards)
  ↓ Consumes TACTICAL costs (Focus for Mental, Stamina for Physical)
  ↓ Executes card-based gameplay
  ↓ Returns success/failure result
  ↓
STRATEGIC LAYER (Consequence Application)
  ↓ Applies consequences (bonds, scales, states)
  ↓ Executes spawn rules (cascading chains)
  ↓ Updates world state
```

**Where Resolve is Consumed:**
- ✅ CORRECT: SituationFacade.SelectAndExecuteSituation() - when player PICKS the situation (strategic choice)
- ❌ WRONG: MentalFacade.StartSession() - tactical layer must not know about Resolve

**Why This Matters:**
- Resolve consumption is baked into the CHOICE ITSELF, not its consequence
- Tactical challenges are reusable subsystems that work regardless of strategic context
- Strategic costs are about COMMITTING to an action (choosing to engage)
- Tactical costs are about EXECUTING an action (playing cards during challenge)
- This separation enables:
  - Instant-resolution situations (no challenge, just strategic cost + consequences)
  - Navigation situations (no challenge, just strategic cost + movement)
  - Challenge situations (strategic cost, then tactical challenge, then consequences)

**Implementation Location:**
- Strategic cost consumption: `SituationFacade` (to be created)
- Tactical cost consumption: `MentalFacade`, `PhysicalFacade`, `SocialFacade` (existing, unchanged)

**7. SCENE AS FUNDAMENTAL UNIT (Content Population Architecture)**

**CRITICAL PRINCIPLE:** Scene is the ACTIVE DATA SOURCE that populates LocationContent UI. Locations/NPCs/Routes are PERSISTENT world entities, but their displayed content comes from the current Scene.

**8. NO EVENTS (Synchronous Orchestration Only)**

**CRITICAL PRINCIPLE:** Game logic operates SYNCHRONOUSLY. GameFacade is the SINGLE orchestrator of control flow. NO event-driven patterns allowed except Blazor frontend events.

**Why This Matters:**

Events create complexity nightmares:
- Hidden control flow (who subscribed?)
- Async callback chains (event A → event B → event C)
- Race conditions (which handler fires first?)
- Impossible debugging (stack traces span multiple handlers)
- Maintainability nightmare (must search for all `+=` subscribers)
- Violates single orchestrator pattern (GameFacade controls ALL flow)

**The CORRECT Pattern - Synchronous Orchestration:**
```csharp
// ✅ CORRECT - GameFacade controls everything synchronously:
public SituationSelectionResult SelectAndExecuteSituation(string situationId)
{
    // 1. Validate (synchronous)
    if (!IsValid) return Failed();

    // 2. Consume costs (synchronous)
    player.Resolve -= costs;

    // 3. Execute logic (synchronous)
    ApplyConsequences();
    ExecuteSpawns();

    // 4. Return result (synchronous)
    return Success();
}
```

**The WRONG Pattern - Event-Driven:**
```csharp
// ❌ WRONG - Events scatter control flow:
public event Action<Situation> OnSituationCompleted;  // BAD
public event Action<List<SpawnRule>> OnSuccessSpawns;  // BAD

// Hidden callback hell:
OnSituationCompleted?.Invoke(situation);  // Who handles this?
OnSuccessSpawns?.Invoke(rules);  // What order do handlers fire?
```

**Naming Convention Violation:**

Property names with "On" prefix suggest event handlers:
- ❌ `OnSuccessSpawns` - sounds like event handler (WRONG)
- ❌ `OnFailureSpawns` - sounds like event handler (WRONG)
- ✅ `SuccessSpawns` - declarative data (CORRECT)
- ✅ `FailureSpawns` - declarative data (CORRECT)

**The Exception - Blazor Frontend:**

Blazor UI components DO use events (framework requirement):
```csharp
// ✅ CORRECT - Blazor event (framework pattern):
<button @onclick="HandleClick">Select</button>

private async Task HandleClick()
{
    // Immediately call GameFacade synchronously
    var result = GameFacade.SelectSituation(id);
    // NO event chains in game logic
}
```

**Enforcement Table:**

| Layer | Events? | Why |
|-------|---------|-----|
| GameFacade | ❌ NO | Single orchestrator |
| Domain Facades | ❌ NO | Synchronous results |
| Entities | ❌ NO | Pure data |
| Services | ❌ NO | Synchronous logic |
| Blazor UI | ✅ YES | Framework requirement |

**Implementation Requirement:**
- ALL game logic must be synchronous method calls
- GameFacade orchestrates ALL control flow
- NO `event` declarations in domain layer
- NO `OnX` naming convention for data properties
- Blazor events call GameFacade synchronously, no cascades

**The Architecture:**

```
Location (PERSISTENT - world entity, can be visited)
  ↓ has current
Scene (ACTIVE - content wrapper, generates situations)
  ↓ populates
LocationContent (PERSISTENT - UI screen, displays content)
```

**What This Means:**

**Scene is NOT:**
- A replacement for LocationContent UI
- A wrapper that makes Locations/NPCs go away
- Something that replaces persistent world entities

**Scene IS:**
- The data source for what situations appear
- The filter that separates available vs locked content
- The generator of contextual narrative intro
- The container that provides perfect information (locked situations visible)

**Locations/NPCs/Routes EXIST persistently:**
- Locations can be visited (spatial navigation works)
- NPCs exist at locations (interaction targets exist)
- Routes connect locations (travel system works)
- LocationContent screen exists and displays content

**BUT: Content displayed is POPULATED BY Scene:**
- When player enters Location → SceneFacade.GenerateLocationScene(locationId)
- Scene queries GameWorld for situations at that location
- Scene evaluates CompoundRequirements (available vs locked)
- Scene generates contextual intro narrative
- LocationContent renders the Scene data

**Default/Generic Scene:**
- If no authored scene is active → generic scene generated
- Generic scene has minimal atmospheric background options
- Generic scene has no special narrative or consequences
- Generic scene provides baseline "you can look around, travel, talk to people" functionality

**Scene Types:**
- **Location Scene:** Content at a physical location (Mental/Physical situations + NPCs present)
- **NPC Scene:** Conversation/interaction with specific NPC (Social situations)
- **Route Scene:** Travel segment with path choices/encounters
- **Event Scene:** Special authored critical moments (rare, hand-crafted)

**Why This Matters:**
- Perfect information display: Scene separates available vs locked situations, shows requirements
- Narrative coherence: Scene generates intro reflecting player state, relationships, achievements
- Dynamic content: Same location visited twice = different scenes with different content
- Sir Brante integration: Scene matches Sir Brante's scene-based progression model

**Implementation Pattern:**
```csharp
// LocationContent.razor.cs
protected override async Task OnInitializedAsync()
{
    var locationId = GameWorld.GetPlayer().CurrentLocation?.Id;
    CurrentScene = GameFacade.GetSceneFacade().GenerateLocationScene(locationId);
    // UI renders CurrentScene.AvailableSituations and CurrentScene.LockedSituations
}
```

**What Changes:**
- ✅ SceneFacade created (new service)
- ✅ SceneFacade.GenerateLocationScene() generates Scene instances
- ✅ LocationContent populated by Scene data instead of LocationFacade view models
- ❌ LocationContent screen NOT deleted (still displays content)
- ❌ LocationFacade NOT deleted (still handles navigation)
- ❌ Locations/NPCs NOT deleted (still exist as world entities)

---

## FOUNDATIONAL ARCHITECTURE: Three-Level Template System

**Reference:** [TEMPLATE_ARCHITECTURE.md](./TEMPLATE_ARCHITECTURE.md)

### Critical Principle: PATTERN ≠ TEMPLATE ≠ INSTANCE

The Situation Spawn Template system uses a **three-level architecture** that must NEVER be conflated:

### LEVEL 1: PATTERN (Documentation Layer)
**Purpose:** Guide content authoring with reusable conceptual frameworks

**Format:** Pure markdown documentation (NO JSON, NO CODE)

**Contains:**
- Conceptual structures ("Linear Progression", "Hub-and-Spoke", "Branching Consequences")
- Use cases and examples
- Decision space analysis
- Narrative structure guidance

**File:** `situation-refactor/situation-spawn-patterns.md`

**Example Patterns:**
- Linear Progression: A → B → C (sequential story beats)
- Hub-and-Spoke: Central situation spawns multiple parallel options
- Discovery Chain: Finding clues reveals new locations
- Branching Consequences: Success/failure lead to different futures

---

### LEVEL 2: TEMPLATE (JSON Archetype Layer)
**Purpose:** Immutable archetype definitions shared by ALL instances

**Format:** JSON with categorical properties and formulas

**Lives In:** `GameWorld.SituationTemplates` (List, NOT Dictionary)

**MUST Contain:**
- ✅ Archetype enums (`"archetype": "Rescue"`)
- ✅ Requirement **formulas** (`"baseValue": "CurrentPlayerBond", "offset": 3`)
- ✅ Template-to-template references (`"childTemplateId": "investigation_followup_template"`)
- ✅ **Categorical** entity filters (`"npcArchetype": "Innocent"`, NOT `"npcId": "elena"`)
- ✅ Narrative hints for AI (`"tone": "Urgent"`, `"theme": "Heroic sacrifice"`)

**MUST NOT Contain:**
- ❌ Specific entity IDs (`"npcId": "elena"` - this is INSTANCE data)
- ❌ Fixed thresholds (`"threshold": 5` - should be formula with offset)
- ❌ Runtime state (`"spawnedDay"`, `"completedDay"` - instance properties)

**Example Template JSON:**
```json
{
  "id": "rescue_plea_template",
  "archetype": "Rescue",
  "tier": 1,
  "interactionType": "NpcSocial",
  "npcFilters": {
    "archetype": "Innocent",
    "locationProximity": "Same",
    "bondStrengthMin": 5
  },
  "requirementFormula": {
    "orPaths": [{
      "numericRequirements": [{
        "type": "BondStrength",
        "baseValue": "CurrentPlayerBond",
        "offset": 3,
        "label": "Need trust to confide"
      }]
    }]
  },
  "successSpawns": [{
    "childTemplateId": "investigation_followup_template",
    "placementStrategy": "SameNpc",
    "requirementOffsets": {
      "bondStrengthOffset": 2
    }
  }]
}
```

---

### LEVEL 3: INSTANCE (Runtime Entity Layer)
**Purpose:** Concrete runtime entities with specific placements and state

**Format:** C# domain entity

**Lives In:** `GameWorld.Situations` (List, NOT Dictionary)

**Architecture:** **COMPOSITION** (NOT Cloning)

```csharp
public class Situation
{
    // COMPOSITION: Reference shared immutable template (NOT cloned)
    public SituationTemplate Template { get; set; }

    // RUNTIME INSTANCE PROPERTIES ONLY
    public string Id { get; set; }
    public NPC PlacementNpc { get; set; }  // Concrete entity (code selects)
    public Location PlacementLocation { get; set; }  // Concrete entity (code selects)
    public int SpawnedDay { get; set; }
    public string GeneratedNarrative { get; set; }
    // ... other runtime state
}
```

**Access Pattern:**
```csharp
// ✅ CORRECT - Access template through composition
SituationArchetype archetype = instance.Template.Archetype;
int tier = instance.Template.Tier;

// ❌ WRONG - Cloning template properties into instance
Situation instance = new Situation {
    Archetype = template.Archetype,  // Duplicated!
    Tier = template.Tier  // Duplicated!
};
```

---

### Code Execution Flow: Template → Instance

**1. Select Template (LINQ Query on List):**
```csharp
SituationTemplate template = gameWorld.SituationTemplates
    .FirstOrDefault(t => t.Id == "rescue_plea_template");
```

**2. Apply Categorical Filters to Find Concrete Entities:**
```csharp
// Template says: npcArchetype: "Innocent", locationProximity: "Same"
NPC targetNpc = gameWorld.NPCs
    .Where(npc => npc.Archetype == NpcArchetype.Innocent)
    .Where(npc => npc.Location == currentLocation)
    .FirstOrDefault();
```

**3. Calculate Requirements from Formulas:**
```csharp
// Template formula: baseValue: "CurrentPlayerBond", offset: 3
// If player bond = 8, calculated requirement = 11
int currentBond = gameWorld.Player.GetBondWith(targetNpc);
int threshold = currentBond + template.RequirementFormula.Offset;
```

**4. Create Instance with Composition:**
```csharp
Situation instance = new Situation
{
    Template = template,  // Reference shared template (NOT cloned)
    Id = GenerateUniqueId(),
    PlacementNpc = targetNpc,  // Concrete entity selected by code
    PlacementLocation = targetNpc.Location,
    Status = SituationStatus.Available,
    SpawnedDay = currentDay
};
gameWorld.Situations.Add(instance);  // List.Add()
```

---

### Strong Typing Enforcement

**FORBIDDEN:**
```csharp
// ❌ Dictionary lookup
public Dictionary<string, SituationTemplate> SituationTemplates { get; set; }

// ❌ HashSet
public HashSet<SituationTemplate> SituationTemplates { get; set; }
```

**REQUIRED:**
```csharp
// ✅ List with LINQ queries
public List<SituationTemplate> SituationTemplates { get; set; } = new();

// ✅ Lookup via LINQ
SituationTemplate template = gameWorld.SituationTemplates
    .FirstOrDefault(t => t.Id == "rescue_plea_template");
```

---

### Composition Over Cloning (NEVER CLONE)

**WHY COMPOSITION:**
1. **Single source of truth** - Template changes propagate to all instances
2. **Memory efficient** - Template data stored once, not duplicated
3. **Type safety** - Compiler enforces correct access patterns
4. **Clear separation** - Runtime state vs design-time archetypes

**CORRECT:**
- Instance has `Template` property (reference to shared object)
- Access via `instance.Template.Archetype`
- Multiple instances share same template object

**WRONG:**
- Instance copies template properties (duplicates data)
- Template properties stored directly on instance
- Each instance has independent copy of archetype data

---

### Summary Table

| Level | Lives In | Contains | Lookup | Purpose |
|-------|----------|----------|--------|---------|
| PATTERN | Markdown | Conceptual structures | N/A | Guide authoring |
| TEMPLATE | `List<SituationTemplate>` | Formulas, categorical filters | LINQ `.FirstOrDefault()` | Reusable archetypes |
| INSTANCE | `List<Situation>` | Concrete entities, runtime state, template reference | LINQ `.FirstOrDefault()` | Playable content |

---

### Implementation Files

**Pattern Layer:**
- ✅ `situation-refactor/situation-spawn-patterns.md`

**Template Layer:**
- ✅ `src/Content/Core/21_situation_templates.json`
- ✅ `src/Content/DTOs/SituationTemplateDTO.cs`
- ✅ `src/Content/SituationTemplateParser.cs`
- ✅ `src/GameState/SituationTemplate.cs`
- ✅ `src/GameState/SituationTemplateNpcFilters.cs`
- ✅ `src/GameState/SituationTemplateLocationFilters.cs`
- ✅ `src/GameState/TemplateSpawnRule.cs`
- ✅ `src/GameState/Enums/SituationArchetype.cs`
- ✅ `src/GameState/Enums/NpcArchetype.cs`
- ✅ `src/GameState/Enums/VenueType.cs`

**Instance Layer:**
- ✅ `src/GameState/Situation.cs` (modified to add Template reference)
- ✅ `src/GameState/GameWorld.cs` (modified to add `List<SituationTemplate>`)

**Integration (Pending):**
- ⏳ PackageLoader - Load templates from JSON
- ⏳ SituationInstantiator - Spawn instances from templates
- ⏳ Template validation - Verify child template references exist

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

**DECISION: NO CATALOGUES NEEDED FOR SCENE-SITUATION PARSERS**

After analyzing the existing `SocialCardEffectCatalog.cs` (708 lines), we determined that Scene-Situation parsers do NOT need catalogues because:

**Catalogue Purpose (from existing pattern):**
- Translate categorical/descriptive JSON properties (e.g., "Moderate" exertion) → concrete numerical values at parse time
- Apply game state scaling (player level, difficulty, max stats)
- Example: `SocialCardEffectCatalog` translates (ConversationalMove, Stat, Depth) → scaled effect values

**Why Scene-Situation Doesn't Need This:**
1. **Properties Already Concrete in JSON**:
   - JSON already contains absolute values: `morality: 0`, `resolve: 30`, `bondStrength: 10`
   - No categorical strings requiring translation ("Moderate" → 5)
2. **No Scaling Required**:
   - Values don't scale with player level or game state
   - A bond threshold of 10 is always 10, regardless of progression
3. **AchievementParser Inline Translation Appropriate**:
   - Dictionary→strongly-typed conversion is 1:1 mapping, not scaling
   - Entity-specific logic (not reused across multiple parsers)
   - Belongs in parser, not separate catalogue

**Catalogues ARE Used When:**
- JSON has categorical properties ("Capable", "Fragile", "Moderate")
- Values need scaling based on game state (player level, difficulty)
- Same translation logic reused across multiple entity types
- AI-generated content needs relative properties

**Catalogues NOT Used When:**
- JSON already has concrete absolute values
- No scaling or game state dependency
- Translation is simple 1:1 mapping
- Logic is entity-specific and not reused

**Examples:**
- ✅ `SocialCardEffectCatalog`: Translates (Remark, Rapport, Depth 2) → scaled effect values based on player level
- ✅ `EquipmentDurabilityCatalog`: Translates "Fragile" → (uses: 2, repairCost: 10)
- ✅ `StateClearConditionsCatalog`: Translates ["Rest", "ConsumeFood"] → ClearsOnRest: true, ClearingItemTypes: [Food]
- ❌ StateParser enum parsing: StateType enum already concrete, no translation needed
- ❌ AchievementParser: Dictionary keys map 1:1 to properties, inline is clearer

**Implementation Decision:**
- StateParser.cs: Enum parsing direct, BUT clear conditions require StateClearConditionsCatalog
- AchievementParser.cs: Inline Dictionary→strongly-typed conversion in ParseGrantConditions()
- SituationParser.cs: TimeBlock parsing already inline
- No RequirementCatalogue, CostCatalogue, or ConsequenceCatalogue needed at this time

**Catalogue + Resolver Pattern (Universal Architecture - CRITICAL PRINCIPLE):**

**THE THREE-LAYER PATTERN:**

When implementing systems that translate categorical properties to runtime behavior:

**1. Catalogue Layer (Parse-Time Translation):**
- Static class with pure functions
- Input: Categorical strings/enums from JSON
- Output: Strongly-typed behavior object
- Called ONCE by parser during initialization
- Example: `StateClearConditionsCatalogue.GetClearingBehavior(List<string>)` → StateClearingBehavior

**2. Behavior Object (Stored on Entity):**
- Data class with semantic properties
- Stored directly on domain entity
- Contains NO logic, only data
- Properties named for execution contexts
- Example: State.ClearingBehavior with ClearsOnRest, ClearingItemTypes properties

**3. Resolver Layer (Runtime Projection):**
- Service class injected via DI
- Methods return projections (what SHOULD happen)
- Uses GameWorld to query current state
- Checks behavior objects to make decisions
- Does NOT modify state directly (projection only)
- Example: `StateClearingResolver.GetStatesToClearOnRest(bool isSafe)` → List<StateType>

**4. Facade Layer (Application):**
- Calls resolver to get projection
- Applies projected changes to GameWorld
- Triggers cascades/side effects
- Example: ResourceFacade calls resolver, removes states, triggers EvaluateDormantSituations

**WHY THIS PATTERN EXISTS:**

❌ **WRONG - Direct Property Checking in Facades:**
```
// Scattered logic across facades
if (state.ClearConditions.Contains("Rest")) { ... }
```
- Runtime string matching (forbidden)
- Logic duplicated across facades
- No projection capability
- Hard to test

✅ **CORRECT - Catalogue + Resolver:**
```
// Centralized in resolver
List<StateType> statesToClear = _stateClearingResolver.GetStatesToClearOnRest(isSafe);
```
- Parse-time translation (zero runtime overhead)
- Single source of truth (all logic in resolver)
- Projection-based (can preview effects)
- Independently testable

**EXISTING WAYFARER EXAMPLES:**

**SocialCardEffectCatalog + SocialEffectResolver:**
- Catalogue: Translates (ConversationalMove, Stat, Depth) → CardEffectFormula
- Behavior: card.EffectFormula (strongly-typed formula object)
- Resolver: ProcessSuccessEffect() projects card effects
- Facade: Applies projected changes to session

**StateClearConditionsCatalogue + StateClearingResolver (This Implementation):**
- Catalogue: Translates ["Rest", "UseFood"] → StateClearingBehavior(ClearsOnRest=true, ClearingItemTypes=[Food])
- Behavior: state.ClearingBehavior (strongly-typed behavior object)
- Resolver: GetStatesToClearOnRest() projects which states should clear
- Facade: Removes states, triggers cascade

**BENEFITS:**
1. **Performance**: Catalogue called once (parse-time), not per-action (runtime)
2. **Projection**: Can preview effects before applying ("this WOULD clear X")
3. **Testing**: Resolver testable without full game initialization
4. **Single Source of Truth**: All decision logic in one place (resolver)
5. **Separation of Concerns**: Catalogue=translation, Resolver=logic, Facade=application
6. **Type Safety**: Behavior object properties compiler-enforced

**WHEN TO USE THIS PATTERN:**
- Categorical properties need translation to runtime behavior
- Multiple execution contexts check the same concept
- Need to project effects before applying them
- Logic should be centralized for maintainability
- System needs independent testability

**WHEN NOT TO USE THIS PATTERN:**
- Simple 1:1 property mapping (no translation needed)
- Single execution context (no logic centralization needed)
- No projection needed (effects always applied immediately)

---

**Execution Context Entity Design (Holistic Architecture - CRITICAL PRINCIPLE):**

**THE UNIVERSAL RULE:**

Design entities around execution contexts, not implementation convenience. Categorical properties from JSON MUST decompose into multiple strongly-typed properties at parse time, each named for its execution context.

**❌ WRONG - Tactical Implementation-First Thinking:**
- "I need to check this list of strings" → Creates List<string> property
- Single mega-property conflates multiple execution contexts
- Runtime code interprets strings
- No type safety

**✅ CORRECT - Holistic Architecture-First Thinking:**
- "What execution contexts will check this?" → Creates property per context
- Categorical property decomposes into semantic properties
- Each property named for WHERE used (ClearsOnRest → ResourceFacade)
- Full type safety

**APPLICATION PATTERN:**

When encountering categorical property during entity design:
1. Identify ALL execution contexts where concept is checked
2. Create one strongly-typed property PER context
3. Write catalogue that translates categorical → contextual properties (parse-time only)
4. Runtime checks ONLY contextual properties, NEVER original strings

**Why This Matters:**
- Fails fast: Invalid categorical values throw at parse time
- Zero interpretation: Runtime has zero logic deciding string meanings
- Context-aware: Properties named for purpose (not generic)
- Type-enforced: Compiler catches misuse, IntelliSense works
- No hidden semantics: Property name documents execution context

**JSON Authoring & Parse-Time Validation Principles:**

Following Wayfarer's catalogue pattern and fail-fast philosophy, all parsers MUST enforce:

1. **NO ICONS IN JSON** - Icons are UI concerns; JSON stores categorical data, UI derives icons from type/category
2. **PREFER CATEGORICAL OVER NUMERICAL** - Content describes intent ("Trusted" bond level), catalogues translate to mechanics (15 bond strength)
   - Exception: Player state uses numerical (XP, Coins, Scales - runtime progression values)
3. **VALIDATE ID REFERENCES** - All entity ID references must be validated against GameWorld at parse time, throw if not found
4. **VALIDATE ENUM STRINGS** - When JSON contains string values referencing enums/actions (e.g., "clearConditions": ["Rest"]), validate against enum definitions/catalogues
5. **FAIL FAST** - Throw InvalidOperationException at parse time for all validation failures, never silently ignore or create placeholders

Scene-Situation currently uses numerical values for player state (design decision) but could adopt categorical tiers in future enhancements.

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
    - [ ] Save all Situation instances (status, requirements, narrative cache, segment-based time, parent references)
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

## Notes

**This implementation plan achieves 100% Scene-Situation architecture as specified in SCENE_SITUATION_ARCHITECTURE.md**

**Foundation from Prior Refactor:**
- Goal→Situation rename was completed in a PRIOR refactor
- Situation.cs exists (was Goal.cs)
- SituationCard exists (was GoalCard)
- 05_situations.json exists (was goals.json)
- This plan EXTENDS Situation with Sir Brante features, not creates it from scratch

**Breaking changes are acceptable and expected during implementation**

**Playability is not maintained during intermediate states**

**Speed of implementation is prioritized over safety**

**Legacy Goal system code was completely removed in prior refactor with no compatibility layers**

**Strong Typing Enforcement:**
- PlayerScales is nested object with 6 int properties (Morality, Lawfulness, Method, Caution, Transparency, Fame)
- ActiveState uses StateType enum and segment-based time tracking
- NO Dictionary<string, X> patterns anywhere
- NO HashSet<T> anywhere
- ALL time tracking uses Day/TimeBlock/Segment (NO DateTime or timestamps)

**The result is a complete architectural transformation:**
- Extends existing Situation entity with spawn tracking, completion tracking, and template system
- Adds Scene-based presentation layer (ephemeral UI constructs)
- Implements cascading spawns (parent→child Situation chains)
- Implements compound OR requirements (multiple unlock paths)
- Integrates AI narrative generation foundation
- Adds Sir Brante-inspired player progression (Scales, States, Achievements)
