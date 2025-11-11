# Architectural Patterns

**Purpose:** Authoritative source for all architectural patterns used throughout Wayfarer codebase.

**Last Updated:** 2025-01

**Related Documentation:**
- **DESIGN_PHILOSOPHY.md:** Design principles (why we design this way)
- **ARCHITECTURE.md:** System architecture (how systems connect)
- **CODING_STANDARDS.md:** Coding conventions (how to write code)

---

## Pattern 1: HIGHLANDER (One Concept, One Representation)

**Core Principle:** "There can be only ONE." One concept gets one representation. No redundant storage, no duplicate paths.

### Three Sub-Patterns

#### A. Persistence IDs with Runtime Navigation (BOTH ID + Object)

**Use When:** Property from JSON, frequent runtime navigation

**Pattern:**
- ID comes from JSON
- Object resolved by parser (GameWorld lookup ONCE)
- Runtime uses ONLY object reference, never ID lookup
- ID immutable after parsing

**Example:**
```csharp
public class Situation {
    public string TemplateId { get; set; }        // From JSON (persistence)
    public SituationTemplate Template { get; set; } // Resolved once at parse, cached
}

// Parser resolves ONCE
situation.Template = gameWorld.SituationTemplates[situation.TemplateId];

// Runtime uses cached reference (NEVER looks up by ID again)
var choices = situation.Template.ChoiceTemplates;
```

#### B. Runtime-Only Navigation (Object ONLY, NO ID)

**Use When:** Runtime state, not from JSON, changes during gameplay

**Pattern:**
- NO ID property exists
- Object reference everywhere consistently
- NEVER add ID alongside object

**Example:**
```csharp
public class Scene {
    public Situation CurrentSituation { get; set; }  // Runtime state, no ID
}

// Runtime navigation
scene.CurrentSituation = scene.Situations[0];

// ❌ FORBIDDEN - Don't add CurrentSituationId alongside CurrentSituation
```

**Why:** DESYNC RISK if you add ID alongside object. Object IS the single source of truth.

#### C. Lookup on Demand (ID ONLY, NO Object)

**Use When:** From JSON, but infrequent lookups

**Pattern:**
- ID property only
- GameWorld lookup when needed
- No cached object (saves memory)

**Example:**
```csharp
public class SceneSpawnReward {
    public string SceneTemplateId { get; set; }  // Just ID, no cached object
}

// Lookup on demand (rare operation)
var template = gameWorld.SceneTemplates[reward.SceneTemplateId];
```

### Decision Tree

- **From JSON + frequent access** → Pattern A (BOTH: ID for persistence, Object for runtime)
- **From JSON + rare access** → Pattern C (ID only, lookup on demand)
- **Runtime state only** → Pattern B (Object ONLY, no ID)

### FORBIDDEN Patterns

- ❌ Redundant storage (both object + ID for runtime-only state)
- ❌ Inconsistent access (some files use object, others ID lookup for same property)
- ❌ Derived properties (ID computed from object)

### Enforcement

**Question:** Is this property from JSON or runtime-only?
- **From JSON:** Use Pattern A or C
- **Runtime-only:** Use Pattern B (object only)

**Question:** Is this accessed frequently or rarely?
- **Frequent:** Pattern A (cache object reference)
- **Rare:** Pattern C (lookup on demand)

---

## Pattern 2: Catalogue Pattern (Parse-Time Translation)

**Core Principle:** JSON has categorical properties, catalogues translate to concrete types at parse-time, runtime uses concrete types directly.

### Three Phases

#### Phase 1: JSON (Authoring)

**Categorical/descriptive properties:**
- "Friendly" not 15
- "Standard" not 8
- "High" not 3
- Enums: `NPCDemeanor.Friendly`, `Quality.Premium`

**Why categorical:**
- AI can generate without knowing global game state
- Easier for human authors
- Self-documenting
- Enables dynamic scaling based on game progression

#### Phase 2: Parsing (Translation)

**Catalogue translates categorical → concrete:**
- `NPCDemeanor.Friendly` → StatThreshold = 3 (scaled by 0.6x)
- `Quality.Premium` → Cost = 12 (scaled by 1.6x)
- Returns complete entities with concrete values

**Happens ONCE at parse time:**
- Catalogues called from Parsers folder only
- Translation deterministic
- Results stored in GameWorld collections

#### Phase 3: Runtime

**Use concrete properties directly:**
- NO catalogue calls
- NO string matching
- NO dictionary lookups
- Direct property access

**Example:**
```csharp
// ❌ FORBIDDEN at runtime
if (npc.Demeanor == NPCDemeanor.Friendly) {
    int threshold = DemeanorCatalogue.GetThreshold(npc.Demeanor);  // NO!
}

// ✅ CORRECT - Pre-calculated at parse time
if (choice.StatThreshold <= player.Rapport) {  // Use concrete value directly
    // choice.StatThreshold was set at parse time by catalogue
}
```

### Catalogue Generation Pattern

**Catalogues generate complete entities, not just properties:**

```csharp
// Parse-time
public static List<ChoiceTemplate> GenerateChoices(
    SituationArchetype archetype,
    GenerationContext context
) {
    // Scale by categorical properties
    int scaledThreshold = archetype.StatThreshold;
    if (context.NpcDemeanor == NPCDemeanor.Friendly) {
        scaledThreshold = (int)(scaledThreshold * 0.6);
    }

    // Return complete entities with concrete values
    return new List<ChoiceTemplate> {
        new ChoiceTemplate {
            StatThreshold = scaledThreshold,  // Concrete int
            CoinCost = scaledCoinCost,        // Concrete int
            // ... all properties concrete
        }
    };
}
```

### JSON Authoring Rules

1. **NO icons** - UI concern, not data
2. **Prefer categorical over numerical** - "Trusted" not 15, "Medium" not 8
3. **Validate ALL ID references** at parse time (throw if missing)
4. **Numerical appropriate for:** Player state accumulation, time tracking, resource pools

### FORBIDDEN FOREVER

- ❌ String matching: `if (action.Id == "secure_room")`
- ❌ Dictionary lookups: `Cost["coins"]`, `Cost.ContainsKey("coins")`
- ❌ Runtime catalogues: Catalogues ONLY in Parsers folder, NEVER in Facades
- ❌ ID-based logic: Entity IDs are reference only, never control behavior

### Implementation Locations

**Catalogues:**
- `/Content/Catalogues/SituationArchetypeCatalog.cs`
- `/Content/Catalogues/SceneArchetypeCatalog.cs`

**Usage:**
- Called from Parsers ONLY
- Results stored in GameWorld.SceneTemplates, GameWorld.SituationTemplates
- Runtime queries GameWorld collections (pre-populated)

---

## Pattern 3: Three-Tier Timing Model

**Core Principle:** Content exists in three timing tiers to enable lazy instantiation and reduce memory.

### Why Three Tiers

**Problem:** Creating all actions at parse time bloats GameWorld with thousands of inaccessible actions.

**Solution:** Defer action creation until player actually reaches the context.

### Tier 1: Templates (Parse Time)

**When:** Game startup during JSON parsing

**What:** Immutable archetypes defining reusable patterns

**Structure:**
- SceneTemplate contains embedded List of SituationTemplates
- SituationTemplate contains embedded List of ChoiceTemplates
- ChoiceTemplate defines action structure

**Characteristics:**
- Created ONCE from JSON at parse time
- Stored in GameWorld.SceneTemplates collection
- NEVER modified during gameplay
- Serves as design language for content authors

**Example:**
```csharp
public class SceneTemplate {
    public string Id { get; set; }
    public List<SituationTemplate> SituationTemplates { get; set; } = new List<SituationTemplate>();
}

public class SituationTemplate {
    public string Id { get; set; }
    public List<ChoiceTemplate> ChoiceTemplates { get; set; } = new List<ChoiceTemplate>();
}
```

### Tier 2: Scenes/Situations (Spawn Time)

**When:** Scene spawns from Obligation or SceneSpawnReward trigger

**What:** Runtime instances with lifecycle and mutable state

**Characteristics:**
- Scene instance created with embedded Situations
- Situation.Template reference stored (ChoiceTemplates NOT instantiated yet)
- InstantiationState = Deferred
- NO actions created in GameWorld.LocationActions/NPCActions yet

**Why Deferred:**
Situations may require specific context (location + NPC combo) that player hasn't reached yet. Creating actions prematurely bloats GameWorld with thousands of inaccessible actions.

**Example:**
```csharp
public class Scene {
    public string Id { get; set; }
    public SceneTemplate Template { get; set; }  // Reference to Tier 1
    public List<Situation> Situations { get; set; } = new List<Situation>();
    public SceneState State { get; set; }  // Provisional, Active, Completed, Expired
}

public class Situation {
    public string Id { get; set; }
    public SituationTemplate Template { get; set; }  // Reference to Tier 1
    public InstantiationState InstantiationState { get; set; }  // Deferred or Instantiated
}
```

### Tier 3: Actions (Query Time)

**When:** Player enters context where situation becomes accessible

**What:** Concrete actions queryable by UI, placed in GameWorld collections

**Trigger:** SceneFacade queries active scene, checks if CurrentSituation.InstantiationState == Deferred

**Process:**
1. Check if situation's context is met (player at required location/NPC)
2. If met, instantiate actions from Situation.Template.ChoiceTemplates
3. Create concrete Action entities (LocationAction, NPCAction, PathCard)
4. Add to GameWorld.LocationActions / NPCActions / PathCards collections
5. Set Situation.InstantiationState = Instantiated

**Cleanup:** When scene completes/expires, actions removed from GameWorld collections

**Example:**
```csharp
// Query-time instantiation
if (situation.InstantiationState == InstantiationState.Deferred) {
    foreach (var choiceTemplate in situation.Template.ChoiceTemplates) {
        var action = new LocationAction {
            Id = $"{situation.Id}_{choiceTemplate.Id}",
            LocationId = scene.PlacementId,
            ActionType = choiceTemplate.ActionType,
            RequirementFormula = choiceTemplate.RequirementFormula,
            // ... map all properties from template
        };
        gameWorld.LocationActions.Add(action);
    }
    situation.InstantiationState = InstantiationState.Instantiated;
}
```

### Benefits

1. **Memory efficiency:** Actions created only when needed
2. **Performance:** No thousands of unused actions in GameWorld
3. **Clear separation:** Templates (immutable design) vs Instances (mutable runtime)
4. **Lazy loading:** Content loads progressively as player explores

### Complete Flow Example

```
PARSE TIME:
JSON → SceneTemplateParser → SceneTemplate (with SituationTemplates, ChoiceTemplates)
                           → Stored in GameWorld.SceneTemplates

SPAWN TIME:
Obligation triggers → SceneInstantiator → Scene (with Situations)
                                        → Situations have InstantiationState = Deferred
                                        → Stored in GameWorld.Scenes

QUERY TIME:
Player at Location → SceneFacade.GetActiveScene() → Checks CurrentSituation
                                                   → If Deferred + Context Met
                                                   → Instantiate Actions
                                                   → Add to GameWorld.LocationActions
                                                   → Set InstantiationState = Instantiated
```

---

## Pattern 4: Let It Crash

**Core Principle:** Fail fast with descriptive errors. No graceful degradation, no defaults, no hiding problems.

### Philosophy

**Traditional approach:**
```csharp
// ❌ FORBIDDEN
if (data == null) {
    data = new DefaultData();  // Hide problem with default
}
```

**Let It Crash approach:**
```csharp
// ✅ CORRECT
if (data == null) {
    throw new InvalidOperationException("Data missing: expected XYZ from JSON");
}
```

### Application Areas

#### 1. Entity Initialization

**Initialize collections inline, trust that contract:**

```csharp
public List<Situation> Situations { get; set; } = new List<Situation>();

// Parser trusts initialization, assigns directly
scene.Situations = parsedSituations;  // If null, let it crash

// Runtime trusts initialization, queries directly
var count = scene.Situations.Count;  // If null somehow, let it crash
```

#### 2. Parse-Time Validation

**Validate ALL references, throw if missing:**

```csharp
var template = gameWorld.SceneTemplates.FirstOrDefault(t => t.Id == templateId);
if (template == null) {
    throw new InvalidDataException($"SceneTemplate not found: {templateId}. Check JSON packages.");
}
```

#### 3. Missing Data

**No defaults, force root cause fix:**

```csharp
// ❌ FORBIDDEN
public int GetStatThreshold(Choice choice) {
    return choice.StatThreshold ?? 3;  // Default fallback
}

// ✅ CORRECT
public int GetStatThreshold(Choice choice) {
    if (choice.StatThreshold == 0) {
        throw new InvalidOperationException($"StatThreshold not set for choice {choice.Id}");
    }
    return choice.StatThreshold;
}
```

### Benefits

1. **Fails fast** - Bugs discovered immediately during development
2. **Clear stack traces** - Exact location of problem
3. **Forces fixes** - Can't paper over missing data with defaults
4. **No silent corruption** - Bad data doesn't propagate through system

### When NOT to Let It Crash

**User input validation** - Graceful error messages appropriate
**External API failures** - Retry logic appropriate
**Expected conditions** - Player not meeting requirements (show UI feedback, don't crash)

**Example of appropriate handling:**
```csharp
// ✅ CORRECT - Expected player state
if (player.Coins < cost) {
    return new ValidationResult {
        IsValid = false,
        Message = "Not enough coins"
    };  // Don't crash, this is expected gameplay
}
```

---

## Pattern 5: Sentinel Values Over Null

**Core Principle:** Never use null for domain logic. Create explicit sentinel objects with internal flags.

### Problem with Null

```csharp
// ❌ PROBLEMATIC
public SpawnConditions Conditions { get; set; } = null;  // null = always eligible?

// Later...
if (conditions != null && conditions.IsEligible(player)) { /* ... */ }
```

**Issues:**
- Ambiguous meaning (null = always eligible? never eligible? not set?)
- Null checks everywhere
- Easy to forget null check

### Sentinel Pattern

```csharp
// ✅ CORRECT
public class SpawnConditions {
    private bool _isAlwaysEligible;

    public static SpawnConditions AlwaysEligible => new SpawnConditions { _isAlwaysEligible = true };

    public bool IsEligible(Player player) {
        if (_isAlwaysEligible) return true;
        // ... actual condition checks
    }
}

// Usage
public SpawnConditions Conditions { get; set; } = SpawnConditions.AlwaysEligible;

// Later...
if (conditions.IsEligible(player)) { /* ... */ }  // No null check needed
```

### Benefits

1. **Explicit intent** - SpawnConditions.AlwaysEligible is clear
2. **No null checks** - Safe to call methods
3. **Type safety** - Compiler enforces correct usage
4. **Self-documenting** - Code reads naturally

### Pattern Structure

```csharp
public class SentinelClass {
    private bool _isSentinel;

    // Named sentinel factory
    public static SentinelClass DefaultValue => new SentinelClass { _isSentinel = true };

    // Methods check sentinel flag
    public bool SomeCheck() {
        if (_isSentinel) return true;  // Sentinel behavior
        // Normal behavior
    }
}
```

### When to Use

- **Domain defaults** - "Always eligible", "No requirements", "Unlimited"
- **Optional complex objects** - Better than null for objects with methods
- **State machines** - Explicit "None" or "Initial" states

### When NOT to Use

- **Simple optionals** - `int?`, `string?` are fine for simple types
- **Collections** - Empty `List<T>` is the sentinel (never null)
- **Strings** - Empty string `""` is the sentinel (never null)

---

## Pattern 6: Requirement Inversion

**Core Principle:** Entities spawn into world immediately, requirements filter visibility/selectability, not spawning.

**Full Documentation:** See REQUIREMENT_INVERSION_PRINCIPLE.md for comprehensive explanation.

### Quick Summary

**Traditional (Boolean Gate):**
```csharp
// ❌ WRONG
if (player.CompletedQuest("phase1")) {
    UnlockQuest("phase2");  // Phase 2 doesn't exist until unlock
}
```

**Requirement Inversion:**
```csharp
// ✅ CORRECT
// Phase 2 exists from game start
Scene phase2 = gameWorld.Scenes.First(s => s.Id == "phase2");

// Spawn conditions filter visibility
if (phase2.SpawnConditions.IsEligible(player)) {
    // Player can see/select phase 2
}
```

**Benefits:**
- Perfect information (player sees what's locked and requirements)
- No hidden content waiting for flags
- State-based visibility, not boolean gates
- Resource competition, not checklist completion

---

## Related Patterns

For coding-level patterns, see **CODING_STANDARDS.md**:
- ID Antipattern
- Entity Initialization
- Type System rules

For design-level principles, see **DESIGN_PHILOSOPHY.md**:
- Single Source of Truth
- Strong Typing
- One Purpose Per Entity
- Perfect Information
- Resource Scarcity

For architectural integration, see **ARCHITECTURE.md**:
- Two-Layer Architecture (Strategic/Tactical)
- GameWorld State Management
- Facade Responsibilities
