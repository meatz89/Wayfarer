# ⚠️ ABSOLUTE FIRST RULE - NEVER ASSUME ⚠️

## MANDATORY CODEBASE INVESTIGATION BEFORE ANY WORK

**BEFORE YOU:**
- Answer ANY question
- Make ANY suggestion
- Write ANY code
- Propose ANY plan
- Modify ANY file

**YOU MUST:**

### 1. SEARCH THE CODEBASE EXHAUSTIVELY
- Use Glob to find ALL related files
- Use Grep to find ALL references to classes/methods/properties you're touching
- Read COMPLETE files (not partial with limit/offset)
- Search for related concepts with multiple search terms
- **ASSUME NOTHING EXISTS OR DOESN'T EXIST UNTIL YOU'VE VERIFIED**

### 2. UNDERSTAND THE COMPLETE ARCHITECTURE
- How does this fit in GameWorld?
- What screens/components exist?
- What CSS files are affected?
- What domain entities are involved?
- What services manage this?
- How does data flow from JSON → Parser → Domain → UI?

### 3. VERIFY ALL ASSUMPTIONS
- "Does X exist?" → SEARCH FOR IT
- "Is Y implemented?" → GREP FOR IT
- "Should I create Z?" → CHECK IF Z ALREADY EXISTS FIRST
- Never say "we need to create" without searching first
- Never say "this doesn't exist" without verifying

### 4. MAP ALL DEPENDENCIES
- What files reference this?
- What will break if I change this?
- What CSS classes are used by which components?
- What other systems connect to this?

### 5. HOLISTIC DELETION/REFACTORING - THE PARSER-JSON-ENTITY TRIANGLE

**⚠️ CRITICAL: When removing OR changing ANY property/field/feature, you MUST delete/update it EVERYWHERE in the data flow triangle ⚠️**

**THE TRIANGLE:**
```
JSON (source data)
  ↓
DTO (deserialization class)
  ↓
Parser (conversion logic)
  ↓
Entity (domain model)
  ↓
GameWorld (game state - single source of truth)
  ↓
Usage (services, UI, etc.)
```

**THE RULE: ALL FIVE LAYERS MUST MATCH**

When you DELETE a property:
1. ✅ Remove from JSON source files
2. ✅ Remove from DTO class
3. ✅ Remove from Parser code (assignment, validation, logic)
4. ✅ Remove from Entity class
5. ✅ Remove from all usage (services, UI, wherever it's referenced)

When you CHANGE a property (rename, type change, restructure):
1. ✅ Change in JSON source files
2. ✅ Change in DTO class
3. ✅ Change in Parser code
4. ✅ Change in Entity class
5. ✅ Change in all usage

**CORRECT APPROACH (HOLISTIC):**
```
✅ "Let me remove travelHubSpotId from the ENTIRE data flow"
   1. Search: grep -r "travelHubSpotId\|TravelHubSpotId" (find EVERYTHING)
   2. Delete from JSON source
   3. Delete from VenueDTO
   4. Delete from VenueParser assignment
   5. Delete from Venue entity
   6. Search for usage references
   7. Delete/refactor all usage
   8. Build and verify
```

**WHY THIS MATTERS:**

- Incomplete deletion leaves dead code that confuses future developers
- Parser might assign to non-existent properties (silent failure)
- DTO fields that aren't parsed waste memory and cause confusion
- Entity properties that aren't used violate single responsibility
- Half-deleted features create phantom dependencies

**EXAMPLES:**

**Example 1 - Deleting a property:**
User: "Remove travelHubSpotId - it's duplicate state (crossroads property is the source of truth)"

✅ CORRECT:
- grep -r "travelHubSpotId\|TravelHubSpotId" (find all references)
- Remove from 01_foundation.json
- Remove from VenueDTO.cs (public string TravelHubSpotId)
- Remove from VenueParser.cs (venue.TravelHubSpotId = dto.TravelHubSpotId)
- Remove from Venue.cs (public string TravelHubSpotId property)
- Search for usage (venue.TravelHubSpotId references)
- Delete usage or refactor to use crossroads property instead
- Build and verify

**Example 2 - Renaming a property:**
User: "Rename 'profession' to 'occupation' in NPC data"

✅ CORRECT:
- Update JSON: "profession" → "occupation"
- Update NPCDTO: public string Profession → public string Occupation
- Update NPCParser: dto.Profession → dto.Occupation
- Update NPC entity: if needed (or keep internal name as Profession if it's domain language)
- Search and update all usage
- Build and verify

### 6. UNDERSTAND PLAYER EXPERIENCE AND MENTAL STATE

**CRITICAL: Before proposing ANY UI changes, you MUST think like the PLAYER:**

**PLAYER MENTAL STATE:**
- What is the player DOING in this moment?
- What are they THINKING about?
- What is their INTENTION when they access this screen?
- What feels NATURAL to do next?

**VERISIMILITUDE IN UI:**
- Does this action make NARRATIVE sense?
- Would a REAL PERSON do this action at this moment?
- Does it break IMMERSION or preserve it?

**VISUAL NOVEL APPROACH:**
- Player makes decisions through CHOICES presented on screen
- Choices should be CONTEXTUAL to current situation
- "Check my belongings" is a DECISION OPTION, not a separate "screen" or "button"
- Equipment/inventory access should appear as NATURAL CHOICE mixed with other actions

**HOW ACTIONS ARE COMMUNICATED:**
- Visual novel = Player sees their OPTIONS as cards/choices
- Navigation happens through DECISIONS, not menus/buttons
- Everything is presented IN-WORLD, not as UI chrome

**EXAMPLES OF CORRECT THINKING:**

✅ Player at location → Sees options: "Talk to NPC", "Look around", "Check belongings", "Leave"
✅ "Check belongings" appears WHERE IT MAKES SENSE (not everywhere, only where contextually appropriate)
✅ Journal accessed via button because it's META (out-of-world reference material)
✅ Challenges are IMMERSIVE experiences (full screen, no chrome)

**THE CORRECT QUESTION:**

"What would a REAL PERSON in this situation naturally want to do, and how would they express that intention?"

NOT: "Where should I put this button?"

### ⚠️ CRITICAL PRINCIPLE: NEVER DELETE REQUIRED FEATURES ⚠️

**IF A FEATURE IS NEEDED BUT NOT YET IMPLEMENTED:**

❌ **WRONG RESPONSE:** "This action type doesn't exist, so I'll remove it from the JSON"
❌ **WRONG RESPONSE:** "This isn't supported yet, so I'll delete it"
❌ **WRONG RESPONSE:** "The code doesn't handle this, so I'll comment it out"

✅ **CORRECT RESPONSE:** "This action type doesn't exist. I will IMPLEMENT it by following the complete vertical slice pattern."

**THE RULE:**
- **NEVER remove necessary content just because it's not implemented yet**
- **ALWAYS implement the missing code to support required features**
- **DELETE ONLY when feature is genuinely unnecessary or wrong design**

**EXAMPLE:**

User needs "SleepOutside" player action for tutorial.

❌ **WRONG:** Remove "SleepOutside" from JSON because PlayerActionType enum doesn't have it
✅ **CORRECT:**
1. Add `SleepOutside` to PlayerActionType enum
2. Add handler in GameFacade.ExecutePlayerAction()
3. Add implementation in ResourceFacade
4. Keep "SleepOutside" in JSON

**WHY THIS MATTERS:**
- Deleting required features breaks the design
- Moving backward instead of forward wastes time
- Implementation exists for a REASON - fulfill that reason, don't delete it

**WHEN TO DELETE:**
- Feature is genuinely wrong design
- Feature duplicates existing functionality (HIGHLANDER violation)
- Feature is dead code with no references

**WHEN TO IMPLEMENT:**
- Feature is needed for current task
- Feature completes a logical pattern
- Feature supports user requirements

### CORRECT PROCESS:

1. **User asks question**
2. **STOP - Do NOT answer immediately**
3. **Search codebase thoroughly:**
   - Glob for related files
   - Grep for related classes/methods
   - Read complete files
   - Verify assumptions
4. **ONLY THEN answer based on ACTUAL codebase state**

### WHY THIS IS CRITICAL:

- Making assumptions wastes hours implementing things that already exist
- Suggesting changes without checking breaks existing code
- Proposing "new" features that already exist shows incompetence
- Not verifying leads to duplicate implementations (HIGHLANDER violation)

**IF YOU VIOLATE THIS RULE, YOU DESERVE GORDON RAMSAY'S WRATH:**

"YOU DONKEY! YOU SUGGESTED CREATING A JOURNAL WHEN IT ALREADY EXISTS! DID YOU EVEN FUCKING LOOK?!"

---

# ⚠️ HIGHLANDER PRINCIPLE: ONE CONCEPT, ONE REPRESENTATION ⚠️

## THE FUNDAMENTAL PRINCIPLE

**"There can be only one."** For any given concept at runtime, use EXACTLY ONE representation consistently throughout the codebase. Never store the same information in multiple forms that can desynchronize.

---

## CORE RULE: CONSISTENT ACCESS PATTERNS

**Single Representation (Correct):**
- Runtime navigation uses the SAME access pattern in ALL files
- Every code location accesses the property identically
- No mixing of object references and ID lookups for the same concept
- Consistent patterns across the entire codebase

**Mixed Representations (Wrong):**
- Some files use object reference, others use ID lookup
- Requires TWO properties that must stay synchronized
- Setting one without updating the other creates desync bugs
- Inconsistent patterns confuse developers

---

## THREE ENTITY PROPERTY PATTERNS

### Pattern A: Persistence IDs with Runtime Navigation (BOTH ID and Object)

**When to Use:** Property originates from JSON, needs frequent runtime navigation

**Characteristics:**
- ID property populated from JSON during parsing
- Object reference populated by parser during parsing (lookup in GameWorld)
- Runtime code uses ONLY object reference, never ID lookup
- ID immutable after parsing (never changes)
- No desync risk (ID is source, object is derived once)

**Examples:** Situation placement (location/NPC from JSON, frequent runtime access)

### Pattern B: Runtime-Only Navigation (Object ONLY, NO ID)

**When to Use:** Property is runtime state, not from JSON, changes during gameplay

**Characteristics:**
- NO ID property exists
- Object reference property only
- Runtime code uses object reference everywhere consistently
- Changes frequently during gameplay
- Desync risk if you add ID property alongside object

**Examples:** Player current location (runtime state, moves between locations)

### Pattern C: Lookup on Demand (ID ONLY, NO Object)

**When to Use:** Property from JSON, but lookups are infrequent

**Characteristics:**
- ID property from JSON
- NO cached object reference
- Lookup in GameWorld when needed (rare operations)
- No memory wasted on cached reference

**Examples:** Obligation patron NPC (referenced infrequently, lookup acceptable)

---

## WHY PERSISTENCE IDS ARE DIFFERENT FROM RUNTIME STATE

**Persistence Properties (Pattern A - BOTH allowed):**
- JSON contains ID reference that must be deserialized
- Parser resolves ID to object during parsing (one-time lookup)
- ID property immutable after parsing
- Object reference derived from ID (never changes independently)
- No desync risk (ID is source of truth, object is cached lookup result)
- Runtime uses object for performance, ID remains for reference integrity

**Runtime State (Pattern B - Object ONLY):**
- No JSON source (state created/modified during gameplay)
- Changes frequently during gameplay
- DESYNC RISK if both ID and object stored (easy to update one without the other)
- Object reference sufficient for all runtime needs
- No serialization needed during active gameplay

---

## DECISION TREE

**Question 1: Is this property deserialized from JSON?**

- **YES → Needs ID property** (for JSON parsing)
  - **Question 2: Is it frequently accessed at runtime for navigation?**
    - **YES → Pattern A (BOTH)** - ID for parsing, Object for runtime (parser resolves)
    - **NO → Pattern C (ID only)** - Lookup on demand when needed

- **NO → Runtime state only**
  - **Pattern B (Object ONLY)** - NO ID property, object reference everywhere

---

## ENFORCEMENT RULES

### Pattern A Rules (Persistence + Navigation)
- Entity has BOTH ID property and object reference property
- ID populated from JSON during parsing
- Object populated by parser during parsing (GameWorld lookup)
- Runtime code uses ONLY object reference (never ID lookup)
- ID immutable (never reassigned after parsing)

### Pattern B Rules (Runtime Navigation)
- Entity has ONLY object reference property (NO ID)
- Runtime code uses object EVERYWHERE consistently
- Never add ID property alongside object (desync risk)
- Object can change during gameplay

### Pattern C Rules (Lookup on Demand)
- Entity has ONLY ID property (NO cached object)
- GameWorld lookup when needed (infrequent operation)
- Never add cached object reference (wastes memory)

### FORBIDDEN Anti-Patterns
- **Redundant Storage:** Both object and ID for runtime state (desync risk)
- **Inconsistent Access:** Some files use object, others use ID lookup (pick one!)
- **Derived Properties:** ID computed from object as property (just use object.Id when needed)

---

## REFACTORING CHECKLIST

When you discover mixed representations:

1. **Identify primary representation:**
   - From JSON + frequent access → Pattern A (both)
   - Runtime-only state → Pattern B (object only)
   - From JSON + infrequent access → Pattern C (ID only)

2. **Search entire codebase:**
   - Find ALL usages of both forms
   - Determine dominant pattern

3. **Convert to single representation:**
   - Select pattern based on decision tree
   - Update ALL code consistently
   - Delete redundant properties

4. **Verify no desync:**
   - Build and test
   - Ensure no code path can create inconsistency

---

## SUMMARY

**HIGHLANDER Principle:** ONE concept, ONE representation, used CONSISTENTLY everywhere.

- **Pattern A (Persistence + Navigation):** ID from JSON, object resolved by parser, runtime uses object
- **Pattern B (Runtime Only):** Object reference only, NO ID property, consistent access everywhere
- **Pattern C (Lookup on Demand):** ID only, GameWorld lookup when needed, no cached object
- **Never store** the same information in multiple forms that can desynchronize
- **Be consistent** across ALL files in the codebase

**"There can be only one."**

---

# ⚠️ CATALOGUE PATTERN: NO STRING MATCHING, NO DICTIONARIES, EVER ⚠️

## THE FUNDAMENTAL PRINCIPLE

**JSON describes game entities categorically. Parsers translate categorical descriptions to concrete values via Catalogues. Runtime code uses only concrete, strongly-typed properties. No strings, no dictionaries, no lookups.**

---

## THREE PHASES OF DATA FLOW

### PHASE 1: AUTHORING (JSON - Categorical/Descriptive)
- Content creators describe entities in **human-readable categorical terms**
- Properties are **descriptive** ("Full", "Partial", "Fragile", "Simple")
- Some properties are **absolute concrete values** (coinCost: 10, time: 1)
- **NO runtime semantics embedded in JSON** - JSON describes WHAT, not HOW

### PHASE 2: PARSING (Translation - One Time Only)
- Parser reads JSON via DTO
- Parser calls **Catalogue** to translate categorical → concrete
- Catalogue returns **strongly-typed concrete values** (int, bool, object)
- Parser stores **concrete values** directly on entity properties
- Entity persisted to GameWorld
- **Translation happens ONCE at game initialization, NEVER during gameplay**

### PHASE 3: RUNTIME (Concrete Values Only)
- GameFacade/Facades fetch entities from GameWorld
- Use **concrete properties directly** (action.HealthRecovery, action.CoinCost)
- **NO catalogue lookups** - values already calculated
- **NO string matching** - no "if (id == 'something')" ever
- **NO dictionary lookups** - no Cost["coins"] ever
- **PURE strongly-typed property access** - compiler-enforced correctness

---

## WHAT THIS ELIMINATES

**FORBIDDEN FOREVER:**
1. ❌ String matching: `if (action.Id == "secure_room")`
2. ❌ Dictionary lookups: `Cost["coins"]`, `Cost.ContainsKey("coins")`
3. ❌ Dictionary properties: `Dictionary<string, int> Cost`
4. ❌ Enum routing at runtime: `switch (recoveryType)` in GameFacade
5. ❌ Catalogue calls at runtime: Catalogues live in Parsers folder, never imported by Facades
6. ❌ ID-based behavior branching: Entity IDs are for reference only, never control logic

**Why these are forbidden:**
- String matching = runtime typo bugs, no IntelliSense, couples code to JSON IDs
- Dictionaries = no type safety, hidden properties, runtime string keys
- Runtime catalogues = wasted CPU, violates parse-time translation principle
- ID-based logic = magic behavior tied to JSON string values, brittle

---

## WHY THIS MATTERS

**Benefits of Parse-Time Translation:**

1. **Fail Fast**: Bad JSON crashes at game init with clear error, not during gameplay
2. **Zero Runtime Overhead**: All calculations done once, not on every action execution
3. **Type Safety**: Compiler catches property access errors, no runtime string bugs
4. **IntelliSense Works**: Developers see real properties (action.HealthRecovery: int)
5. **AI Content Generation**: AI describes effects categorically, parser scales to current game state
6. **No Magic Strings**: Eliminates entire class of typo bugs ("coins" vs "coin")
7. **Testable**: Can unit test catalogues independently from runtime logic
8. **Maintainable**: Change catalogue = affects all entities, change entity property = compiler finds usages

---

## CATALOGUE PATTERN IN PRACTICE

**Catalogue Purpose:**
- Translate categorical/descriptive JSON properties → concrete mechanical values
- Apply game state scaling (player level, difficulty, max stats)
- Enforce domain rules (e.g., "Full" recovery always means max resources)
- **Called only at parse time, never at runtime**

**Catalogue Characteristics:**
- Static class with static methods
- Lives in `src/Content/Catalogs/`
- Throws InvalidDataException on unknown categorical values (fail fast)
- Returns concrete types (int, bool, strongly-typed objects)
- Deterministic (same inputs → same outputs)
- Well-documented with categorical meanings

**When to Create Catalogue:**
- JSON has categorical/descriptive properties ("Simple", "Full", "Fragile")
- Properties need translation to concrete game values
- Translation might scale based on game state
- Same categorical value reused across multiple entities

**When NOT to Create Catalogue:**
- JSON already has concrete absolute values (coinCost: 10) - just copy to entity
- Property is pure data with no translation needed
- Value is unique to one entity (no reuse)

### JSON Authoring Principles (Categorical-First Design)

**CRITICAL RULES FOR JSON CONTENT:**

1. **NO ICONS IN JSON**
   - Icons are UI concerns, not data
   - JSON stores categorical data, UI layer determines visual representation
   - ❌ WRONG: `"icon": "sword-icon.png"`
   - ✅ CORRECT: No icon property at all (UI derives icon from categorical properties like type/category)

2. **PREFER CATEGORICAL OVER NUMERICAL**
   - Always prefer categorical properties ("Capable", "Commanding", "Fragile") over numerical values in JSON
   - Content authors describe INTENT and RELATIVE MAGNITUDE, not exact mechanics
   - Catalogues translate intent → mechanics with game state scaling
   - ❌ WRONG: `"bondRequirement": 15` (hardcoded number)
   - ✅ CORRECT: `"bondRequirement": "Trusted"` (categorical level translated by catalogue)
   - ❌ WRONG: `"resolveCost": 8` (hardcoded number)
   - ✅ CORRECT: `"resolveCost": "Medium"` (tier-based, scales with progression)

3. **VALIDATE ID REFERENCES AT PARSE TIME**
   - ALL ID references in JSON must be validated against GameWorld entities during parsing
   - Parser must throw InvalidOperationException if referenced entity doesn't exist
   - Fail fast at game initialization, not during gameplay
   - ❌ WRONG: Silently ignore missing references or create placeholder
   - ✅ CORRECT: `if (!gameWorld.NPCs.ContainsKey(dto.NpcId)) throw new InvalidOperationException($"Situation '{dto.Id}' references unknown NPC '{dto.NpcId}'")`

**When Numerical Values ARE Appropriate:**
- Player state progression (XP: 150, Coins: 47) - runtime accumulation
- Time tracking (CurrentDay: 5, Segment: 3) - runtime clock
- Resource pools (Health: 12, MaxHealth: 18) - runtime state
- Scale positions (Morality: +7) - runtime spectrum tracking

**When Categorical Values ARE Required:**
- Content authoring (equipment durability, card effectiveness, cost tiers)
- Difficulty descriptors (stat requirements, challenge levels)
- Relative comparisons (bond depth, relationship stages)
- AI-generated content (needs relative properties, not absolute values)

**Validation Enforcement:**

All parsers MUST:
1. Validate required fields present (throw if missing)
2. Validate enum/categorical values (throw if unknown)
3. Validate ID references exist in GameWorld (throw if not found)
4. Validate nested objects properly structured (throw if malformed)

---

## EXISTING CATALOGUE EXAMPLES

**EquipmentDurabilityCatalog:**
- JSON: `"durability": "Fragile"`
- Catalogue translates: "Fragile" → (uses: 2, repairCost: 10)
- Entity stores: `equipment.ExhaustsAfterUses = 2`, `equipment.RepairCost = 10`
- Runtime uses: `if (equipment.UsesRemaining == 0) { ... }`

**SocialCardEffectCatalog:**
- JSON: `"stat": "Rapport", "depth": 2, "index": 0`
- Catalogue translates: (Rapport, depth 2) → CardEffectFormula with concrete effect values
- Entity stores: `card.EffectFormula = formula`
- Runtime uses: `understanding += card.EffectFormula.BaseValue`

**PhysicalCardEffectCatalog, MentalCardEffectCatalog:**
- Same pattern: Categorical properties → Concrete effect formulas at parse time

---

## ENFORCEMENT RULES

**Code Review - Parser:**
- ✅ All categorical JSON properties translated via catalogue?
- ✅ All concrete values stored on entity properties (int, bool, object)?
- ✅ NO Dictionary<string, X> on entities?
- ✅ Catalogue throws on unknown categorical values?
- ✅ Parser stores results on entity, not in temporary variables?

**Code Review - Runtime (GameFacade, Facades, Services):**
- ✅ NO catalogue imports (catalogues are parse-time only)?
- ✅ NO string comparisons (`action.Id == "something"`)?
- ✅ NO dictionary lookups (`dict["key"]`, `dict.ContainsKey()`)?
- ✅ NO enum switching for behavior (`switch (recoveryType)` in runtime)?
- ✅ ONLY concrete property access (`action.HealthRecovery`, `action.CoinCost`)?

**Code Review - Entities:**
- ✅ Properties are concrete types (int, bool, strongly-typed classes)?
- ✅ NO Dictionary<string, X> properties?
- ✅ NO enum properties that control behavior (enums for display/grouping only)?
- ✅ Property names describe mechanics, not categories ("HealthRecovery" not "RecoveryType")?

**If you see Dictionary or string matching in runtime code, STOP. The architecture is violated.**

---

## THE COMPLETE PRINCIPLE

**JSON is descriptive. Parsers translate descriptions to mechanics. Runtime executes mechanics.**

Content creators describe WHAT entities do in human terms.
Parsers translate WHAT to HOW using catalogues.
Runtime executes HOW using strongly-typed properties.

No strings in runtime. No dictionaries in entities. No catalogue lookups after initialization.
Parse once. Execute forever with pure data access.

---

# FUNDAMENTAL GAME SYSTEM ARCHITECTURE

## THE CORE PROGRESSION FLOW (NEVER DEVIATE FROM THIS)

**THIS IS THE SINGLE MOST IMPORTANT DIAGRAM IN THE ENTIRE CODEBASE:**

```
Obligation (multi-phase mystery structure - domain entity)
  ↓ spawns
Obstacles (challenges in the world - domain entity)
  ↓ contain
Goals (approaches to overcome obstacles - domain entity)
  ↓ appear at
Locations/NPCs/Routes (placement context - NOT ownership)
  ↓ when player engages, Goals become
Challenges (Social/Mental/Physical - gameplay subsystems)
  ↓ player plays
GoalCards (tactical victory conditions - domain entity)
  ↓ achieve
Goal Completion (mechanical outcomes)
  ↓ contributes to
Obstacle Progress (tracked progress)
  ↓ leads to
Obstacle Defeated
  ↓ advances
Obligation Phase Completion
  ↓ unlocks
Next Obligation Phase / Completion
```

## CRITICAL DISTINCTIONS

### Obligation ≠ Card
- **Obligation** = Multi-phase mystery/quest structure (e.g., "Investigate the Missing Grain")
- **GoalCard** = Tactical card played during challenges (e.g., "Sharp Remark" card)
- **NEVER** create "ObligationCard" entities - this makes no architectural sense

### Goal ≠ GoalCard
- **Goal** = Approach to overcome an obstacle (e.g., "Persuade the guard")
- **GoalCard** = Victory condition within that Goal's challenge (e.g., "Reach 15 Understanding")
- Goals CONTAIN GoalCards as win conditions

### Obstacle ≠ Challenge
- **Obstacle** = Persistent barrier in the world (e.g., "Suspicious Guard", "Locked Gate")
- **Challenge** = Active gameplay session when player engages a Goal (Social/Mental/Physical subsystem)
- Obstacles exist permanently; Challenges are temporary tactical gameplay

### Location ≠ Owner
- **Locations** are PLACEMENT context where Goals appear
- **Obstacles** OWN Goals (lifecycle control)
- **Investigations** OWN Obstacles (lifecycle control)
- Locations just provide spatial/narrative context

## ENTITY OWNERSHIP HIERARCHY

```
GameWorld (single source of truth)
 │
 ├─ Obligations (List<Obligation>)
 │   └─ Each Obligation spawns Obstacles (by ID reference)
 │
 ├─ Obstacles (List<Obstacle>)
 │   └─ Each Obstacle contains Goals (by ID reference)
 │
 ├─ Goals (List<Goal>)
 │   ├─ Has locationId (appears at location)
 │   ├─ Has npcId (optional - social context)
 │   └─ Contains GoalCards (List<GoalCard> inline)
 │
 ├─ Locations (List<Location>)
 │   └─ Does NOT own Goals - just placement context
 │
 └─ NPCs (List<NPC>)
     └─ Does NOT own Goals - just social context
```

# General Game Design Principles for Wayfarer

## Principle 1: Single Source of Truth with Explicit Ownership

**Every entity type has exactly one owner.**

- GameWorld owns all runtime entities via flat lists
- Parent entities reference children by ID, never inline at runtime
- Child entities reference parents by ID for lifecycle queries
- NO entity owned by multiple collections simultaneously

**Authoring vs Runtime:**
- **Authoring (JSON):** Nested objects for content creator clarity
- **Runtime (GameWorld):** Flat lists for single source of truth
- Parsers flatten nesting into lists, establish ID references

**Test:** Can you answer "who controls this entity's lifecycle?" with one word? If not, ownership is ambiguous.

## Principle 2: Strong Typing as Design Enforcement

**If you can't express it with strongly typed objects and lists, the design is wrong.**

- `List<Obstacle>` NOT `Dictionary<string, object>`
- `List<string> ObstacleIds` NOT `Dictionary<string, List<string>>`
- No `var`, no `object`, no `Dictionary<K,V>` AT ALL - FORBIDDEN
- No `HashSet<T>` - FORBIDDEN
- ONLY `List<T>` where T is entity or enum
- No `SharedData`, `Context`, `Metadata` dictionaries

**Why:** Dictionaries hide relationships. They enable lazy design where "anything can be anything." HashSets hide structure and ordering. Strong typing with Lists forces clarity about what connects to what and why.

**Test:** Can you draw the object graph with boxes and arrows where every arrow has a clear semantic meaning? If not, add structure.

## Principle 3: Categorical Properties → Dynamic Scaling Through Catalogues (AI Content Generation Pattern)

**CRITICAL: JSON contains RELATIVE/CATEGORICAL properties. Catalogues translate to ABSOLUTE values scaled dynamically by game state.**

**THE REASON: AI-Generated Runtime Content**

AI-generated content CANNOT specify absolute mechanical values because AI doesn't know:
- Current player progression level
- Existing game balance
- Global difficulty curve
- What other entities exist

AI CAN specify RELATIVE categorical properties:
- "This rope is Fragile" (relative to other ropes)
- "This card is a Remark" (relative to other conversational moves)
- "This NPC is Cunning" (relative to other personalities)

**The Pattern:**
```
AI generates JSON (Categorical/Relative)
    → Parser reads game state
    → Catalogue translates with DYNAMIC SCALING
    → Domain Entity (Absolute/Scaled values)
```

**Examples:**

**CORRECT** (AI-friendly categorical properties):
```csharp
// AI generates: "durability": "Fragile" (doesn't know absolute values)
// Parser reads game state: player level 3, difficulty Normal
// Catalogue scales dynamically:
DurabilityType durability = ParseEnum(dto.Durability);
int playerLevel = gameWorld.Player.Level;
DifficultyMode difficulty = gameWorld.CurrentDifficulty;
(int uses, int repairCost) = EquipmentDurabilityCatalog.GetDurabilityValues(
    durability, playerLevel, difficulty);

// Result at Level 1: Fragile → 2 uses, 10 coins
// Result at Level 5: Fragile → 4 uses, 25 coins (scaled up)
// Fragile ALWAYS weaker than Sturdy (relative consistency)
```

```csharp
// AI generates conversational move without knowing exact values
CardEffectFormula effect = SocialCardEffectCatalog.GetEffectFromCategoricalProperties(
    move, stat, depth, cardId, playerLevel);

// Catalogue scales based on progression:
// Early game: Remark/Rapport/Depth2 → +4 Understanding
// Late game: Remark/Rapport/Depth2 → +6 Understanding (scaled)
```

**Why This Matters:**

1. **AI Content Generation**: AI can create entities without knowing global game state
2. **Dynamic Scaling**: Same categorical value scales with player progression
3. **Relative Consistency**: "Fragile" always weaker than "Sturdy" regardless of scaling
4. **Future-Proof**: Works for procedural generation, runtime content, user-generated content
5. **Balance Maintenance**: Change ONE catalogue formula, ALL content scales consistently

**Catalogue Requirements:**
- **Static class** with pure scaling functions
- **Context-aware**: Accept game state parameters (player level, difficulty, etc.)
- **Deterministic scaling**: Same inputs → same outputs (reproducible)
- **Relative preservation**: Categorical relationships maintained across all scales
- **Enum-based**: Use strongly-typed enums for categorical properties

**Scaling Factors Catalogues Can Use:**
- Player level (progression)
- Current difficulty mode (Easy/Normal/Hard)
- Time in game (early/mid/late game)
- Existing entity counts (balance relative to what exists)
- Global economy state (coin inflation/deflation)

**Existing Catalogues:**
- `SocialCardEffectCatalog`: ConversationalMove/Stat/Depth → Scaled card effects
- `MentalCardEffectCatalog`: ObservationDepth/Stat → Scaled investigation effects
- `PhysicalCardEffectCatalog`: ActionType/Stat/Depth → Scaled obstacle effects
- `EquipmentDurabilityCatalog`: DurabilityType → Scaled uses/repair costs

**Test:** If you're adding a numeric property to a DTO, ask:
1. "Could AI generate this entity at runtime without knowing global game state?"
2. "Should this value scale with player progression?"
3. "Is this a RELATIVE property (compared to similar entities)?"

If YES to any → Create categorical enum + scaling catalogue. If NO → Consider if it's truly a design-time constant.

## Principle 4: Ownership vs Placement vs Reference

**Three distinct relationship types. Don't conflate them.**

**OWNERSHIP (lifecycle control):**
- Parent creates child
- Parent destroys child
- Child cannot exist without parent
- Example: Investigation owns Obstacles, Obstacle owns Goals

**PLACEMENT (presentation context):**
- Entity appears at a location for UI/narrative purposes
- Entity's lifecycle independent of placement
- Placement is metadata on the entity
- Example: Goal has `locationId` field, appears at that location

**REFERENCE (lookup relationship):**
- Entity A needs to find Entity B
- Entity A stores Entity B's ID
- No lifecycle dependency
- Example: Location has `obstacleIds`, queries GameWorld.Obstacles

**Common Error:** Making Location own Obstacles because goals appear there. Location is PLACEMENT context, not OWNER.

**Test:** If Entity A is destroyed, should Entity B be destroyed? 
- Yes = Ownership
- No = Placement or Reference

## Principle 5: Inter-Systemic Rules Over Boolean Gates

**Strategic depth emerges from shared resource competition, not linear unlocks.**

**Boolean Gates (Cookie Clicker):**
- "If completed A, unlock B"
- No resource cost
- No opportunity cost
- No competing priorities
- Linear progression tree
- NEVER USE THIS PATTERN

**Inter-Systemic Rules (Strategic Depth):**
- Systems compete for shared scarce resources
- Actions have opportunity cost (resource spent here unavailable elsewhere)
- Decisions close options
- Multiple valid paths with genuine trade-offs
- ALWAYS USE THIS PATTERN

**Examples:**
- ❌ "Complete conversation to discover investigation" (boolean gate)
- ✅ "Reach 10 Momentum to play GoalCard that discovers investigation" (resource cost)
- ❌ "Have knowledge token to unlock location" (boolean gate)
- ✅ "Spend 15 Focus to complete Mental challenge that grants knowledge" (resource cost)

**Test:** Does the player make a strategic trade-off (accepting one cost to avoid another)? If no, it's a boolean gate.

## Principle 6: Typed Rewards as System Boundaries

**Systems connect through explicitly typed rewards applied at completion, not through continuous evaluation or state queries.**

**Correct Pattern:**
```
System A completes action
→ Applies typed reward
→ Reward has specific effect on System B
→ Effect applied immediately
```

**Wrong Pattern:**
```
System A sets boolean flag
→ System B continuously evaluates conditions
→ Detects flag change
→ Triggers effect
```

**Why:** Typed rewards are explicit connections. Boolean gates are implicit dependencies. Explicit connections maintain system boundaries.

**Examples:**
- ✅ GoalCard.Rewards.DiscoverInvestigation (typed reward)
- ✅ GoalCard.Rewards.PropertyReduction (typed reward)
- ❌ Investigation.Prerequisites.CompletedGoalId (boolean gate)
- ❌ Continuous evaluation of HasKnowledge() (state query)

**Test:** Is the connection a one-time application of a typed effect, or a continuous check of boolean state? First is correct, second is wrong.

## Principle 7: Resource Scarcity Creates Impossible Choices

**For strategic depth to exist, all systems must compete for the same scarce resources.**

**Shared Resources (Universal Costs):**
- Time (segments) - every action costs time, limited total
- Focus - Mental system consumes, limited pool
- Stamina - Physical system consumes, limited pool
- Health - Physical system risks, hard to recover

**System-Specific Resources (Tactical Only):**
- Momentum/Progress/Breakthrough - builder resources within challenges
- Initiative/Cadence/Aggression - tactical flow mechanics
- These do NOT create strategic depth (confined to one challenge)

**The Impossible Choice:**
"I can afford to do A OR B, but not both. Both paths are valid. Both have genuine costs. Which cost will I accept?"

**Test:** Can the player pursue all interesting options without trade-offs? If yes, no strategic depth exists.

## Principle 8: One Purpose Per Entity

**Every entity type serves exactly one clear purpose.**

- Goals: Define tactical challenges available at locations/NPCs
- GoalCards: Define victory conditions within challenges
- Obstacles: Provide strategic information about challenge difficulty
- Investigations: Structure multi-phase mystery progression
- Locations: Host goals and provide spatial context
- NPCs: Provide social challenge context and relationship tracking

**Anti-Pattern: Multi-Purpose Entities**
- "Goal sometimes defines a challenge, sometimes defines a conversation topic, sometimes defines a shop transaction"
- NO. Three purposes = three entity types.

**Test:** Can you describe the entity's purpose in one sentence without "and" or "or"? If not, split it.

## Principle 9: Verisimilitude in Entity Relationships

**Entity relationships should match the conceptual model, not implementation convenience.**

**Correct (matches concept):**
- Investigations spawn Obstacles (discovering a mystery reveals challenges)
- Obstacles contain Goals (a challenge has multiple approaches)
- Goals appear at Locations (approaches happen at places)

**Wrong (backwards):**
- Obstacles spawn Investigations (a barrier creating a mystery makes no sense)
- Locations contain Investigations (places don't own mysteries)
- NPCs own Routes (people don't own paths)

**Test:** Can you explain the relationship in natural language without feeling confused? If the explanation feels backwards, it is backwards.

## Principle 10: Elegance Through Minimal Interconnection

**Systems should connect at explicit boundaries, not pervasively.**

**Correct Interconnection:**
- System A produces typed output
- System B consumes typed input
- ONE connection point (the typed reward)
- Clean boundary

**Wrong Interconnection:**
- System A sets flags in SharedData
- System B queries multiple flags
- System C also modifies those flags
- System D evaluates combinations
- Tangled web of implicit dependencies

**Test:** Can you draw the system interconnections with one arrow per connection? If you need a web of arrows, the design has too many dependencies.

## Principle 11: Perfect Information with Hidden Complexity

**All strategic information visible to player. All tactical complexity hidden in execution.**

**Strategic Layer (Always Visible):**
- What goals are available
- What each goal costs (resources)
- What each goal rewards
- What requirements must be met
- What the current world state is

**Tactical Layer (Hidden Until Engaged):**
- Specific cards in deck
- Exact card draw order
- Precise challenge flow
- Tactical decision complexity

**Why:** The single player of this single-player rpg makes strategic decisions based on perfect information, then execute tactically with skill-based play.

**Test:** Can the player make an informed decision about WHETHER to attempt a goal before entering the challenge? If not, strategic layer is leaking tactical complexity.

---

## Principle 12: Execution Context Entity Design (Holistic Architecture)

**FUNDAMENTAL RULE: Design entities around execution contexts, not implementation convenience.**

**The Anti-Pattern (Tactical Thinking):**
- "I need to check if this list contains a string" → Creates List<string> property
- Single mega-property conflates multiple unrelated execution contexts
- Runtime code interprets categorical strings (which facade checks which value?)
- No type safety, hidden semantics, implementation-first

**The Correct Pattern (Holistic Thinking):**
- "What execution contexts will check this entity?" → Design properties for contexts
- Categorical property DECOMPOSES into multiple semantic properties
- Each property documents WHERE it's used (ClearsOnRest → ResourceFacade, ClearsOnItemUse → ItemFacade)
- Full type safety, explicit semantics, architecture-first

**Three-Layer Data Flow (Universal Pattern):**
1. **JSON Layer**: Categorical strings (human-readable, content-author-friendly)
2. **Parse Layer**: Catalogue translates categorical → multiple strongly-typed properties
3. **Runtime Layer**: Facades check ONLY concrete types (bool, enum, int, List<ConcreteType>)

**FORBIDDEN AT RUNTIME:**
- ❌ String matching: `if (entity.SomeList.Contains("Value"))`
- ❌ Dictionary lookups: `entity.Properties["key"]`
- ❌ Catalogue calls: Catalogues are parse-time ONLY
- ❌ String interpretation: Runtime decides meaning of strings

**REQUIRED PATTERN:**
- ✅ Parse-time translation: Categorical → strongly-typed (happens ONCE at initialization)
- ✅ Context-aware properties: Named for WHERE they're checked (not WHAT they check)
- ✅ Semantic decomposition: One categorical input → Many contextual outputs
- ✅ Zero runtime interpretation: All meaning decided at parse time

**Application Process:**
When encountering categorical property (List<string>, string enum) during entity design:
1. Identify ALL execution contexts where this concept matters
2. Create one strongly-typed property PER context
3. Write catalogue that translates categorical → all contextual properties (parse-time only)
4. Runtime code checks ONLY contextual properties, NEVER original strings

**Why This Matters:**
- **Fails fast**: Invalid categorical values throw at parse time, not mid-gameplay
- **Zero interpretation**: Runtime has zero logic deciding what strings mean
- **Context-aware**: Properties named for purpose (ClearsOnRest, not generic "conditions")
- **Type-enforced**: Compiler catches misuse, IntelliSense guides correct usage
- **No hidden semantics**: Property name documents execution context
- **Performance**: No string comparisons in hot paths

**Test:** Can you name the exact facade method that checks each property? If no, property is poorly designed.

## Meta-Principle: Design Constraint as Quality Filter

**When you find yourself reaching for:**
- Dictionaries of generic types
- Boolean flags checked continuously
- Multi-purpose entities
- Ambiguous ownership
- Hidden costs
- **Runtime string matching or List<string>.Contains() checks**

**STOP. The design is wrong.**

These aren't implementation problems to solve with clever code. They're signals that the game design itself is flawed. Strong typing and explicit relationships aren't constraints that limit you - they're filters that catch bad design before it propagates.

**Good design feels elegant. Bad design requires workarounds.**

**Parser-JSON-Entity Triangle**:
- Parser sets property → JSON must have field → Entity must have property
- Parser doesn't set property → Either JSON missing it OR entity shouldn't have it
- All three must align → Change all three together, never in isolation

**Anti-Patterns**:
- ❌ Add property to make compilation work
- ❌ Change method signature to accept different type
- ❌ Cast or convert to make types match
- ❌ Add compatibility layer for both old and new
- ❌ Check List<string> at runtime instead of translating at parse time

**Correct Pattern**:
- ✅ Which entity should own this property architecturally?
- ✅ Trace parser → JSON → entity alignment
- ✅ Move property to correct entity in all three: JSON, parser, domain
- ✅ Delete legacy code that violates architecture
- ✅ Categorical properties translate to multiple strongly-typed properties at parse time

**Why This Matters**:
- Dictionary disease causes runtime type errors, impossible debugging, lost IntelliSense
- Extension methods hide domain logic where you can't find it
- Helper classes are grab bags that violate single responsibility
- Compatibility layers hide problems and create confusion
- Two sources of truth guarantee bugs
- Half-refactored code is worse than no refactoring
- TODOs in production are admissions of incomplete work
- Runtime string matching violates parse-time translation principle

**9/10 Certainty Test**:
- Traced EXACT data flow?
- Found EXACT broken component?
- Tested EXACT hypothesis?
- Can point to EXACT line?
- Verified with ACTUAL data?

If NO to any: You're below 9/10. Keep investigating.

**Why This Matters**:
- Fixes below 9/10 certainty waste hours on wrong problems
- "Probably" and "might be" lead to shotgun debugging
- Get proof before fixing

**Pass Criteria**:
- Acceptance criteria met
- ALL connected systems tested
- No compilation errors
- Architecture principles upheld
- No soft-locks or edge cases broken
- 9/10 certainty on correctness

**Why This Matters**:
- Testing ONE system instead of ALL connected systems causes production bugs
- Fixes below 9/10 certainty waste hours on wrong problems

**Rules**:

**Why This Matters**:
- Packages load atomically
- Split references break loading, create broken skeletons, make game unusable
- Hardcoded content violates data-driven design and requires code recompilation
- **JsonPropertyName is a code smell that hides mismatches and creates confusion**
- **Name mismatches indicate incomplete refactoring or poor design**
- **Fix the source data, not the deserialization layer**

## PRIME DIRECTIVES 

**READ ARCHITECTURE.MD FIRST**
Before ANY changes to Wayfarer codebase, read and understand complete Architecture.md. Contains critical system architecture, data flow patterns, dependency relationships. Violating these principles breaks the system.

**NEVER INVENT GAME MECHANICS**
Cards have ONE fixed cost from JSON. No conditions, no alternatives, no "OR mechanics", no conditional costs. If the EXACT mechanic isn't documented, don't implement it.

**DEPENDENCY ANALYSIS REQUIRED**
Before changing files (especially CSS, layouts, global components): Use search tools to find ALL references. Check dependencies. Test impact radius. Verify all related files still work. Analyze BEFORE modifying, not after breaking things.

**READ COMPLETE FILES**
Never use limit/offset unless file is genuinely too large. Always read complete files start to finish. Partial reads cause missing information and wrong assumptions.

**NEVER ASSUME - ASK FIRST**
Ask about actual values, actual assignments, actual data flow. Stop going in circles - investigate actual data. Look at full picture. Think first - understand WHY current approach fails.

**GORDON RAMSAY META-PRINCIPLE**
YOU ARE THE GORDON RAMSAY OF SOFTWARE ENGINEERING. Aggressive enforcement, zero tolerance for sloppiness, direct confrontation, expects perfection. "This code is FUCKING RAW!" Be a PARTNER, not a SYCOPHANT.

---

## ENTITY INITIALIZATION STANDARD ("LET IT CRASH" PHILOSOPHY)

### The Problem: Defensive Programming Hides Bugs

**ANTI-PATTERN** (Defensive Programming):
```csharp
// Parser hiding missing data with defaults
public SocialCard ParseCard(SocialCardDTO dto)
{
    return new SocialCard
    {
        Title = dto.Title ?? "",  // ❌ HIDES missing required field!
        PersonalityTypes = dto.PersonalityTypes ?? new List<string>()  // ❌ REDUNDANT!
    };
}

// Game logic not trusting entity initialization
List<Goal> goals = obstacle.GoalIds?.Select(id => _gameWorld.Goals[id]).ToList()
                   ?? new List<Goal>();  // ❌ GoalIds NEVER null!
```

**Why This Is Wrong:**
- Hides bugs instead of failing fast
- Creates dual source of truth (entity initialization + ?? fallback)
- Wastes developer time debugging silent failures
- Violates "Let It Crash" principle from Erlang/Elixir


**EVERYTHING ELSE IS FORBIDDEN.**

### Why This Matters

**Benefits:**
- Bugs fail fast with descriptive errors (easier debugging)
- Single source of truth (entity initialization contract)
- Less code (no redundant ?? operators everywhere)
- Forces fixing root cause (missing JSON data) instead of hiding it

## CONSTRAINT SUMMARY

**ARCHITECTURE**
GameWorld single source of truth | GameWorld has zero dependencies | No SharedData dictionaries | GameUIBase is only navigation handler | TimeBlockAttentionManager for attention | GameScreen authoritative SPA pattern

**CODE STANDARDS**
Strong typing (no var, Dictionary, HashSet, object, func, lambda) | No extension methods | No Helper/Utility classes | Domain Services and Entities only | One method, one purpose | No exception handling | int over float | No logging until requested | No inline styles | Code over comments

**ENTITY INITIALIZATION (LET IT CRASH PHILOSOPHY)**
Collections ALWAYS initialize inline (`public List<T> X { get; set; } = new List<T>();`) | NEVER null unless validation pattern | Parsers assign directly, NO ?? operators | Game logic trusts entity initialization | ArgumentNullException for null constructor parameters ONLY | Let missing data crash with descriptive errors | ?? operator FORBIDDEN except ArgumentNullException guards | Trust entity contracts, don't defend against them

**REFACTORING**
Delete first, fix after | No compatibility layers | No gradual migration | Complete refactoring only | No TODO comments | No legacy code | HIGHLANDER (one concept, one implementation) | No duplicate markdown files | Delete unnecessary abstractions | Finish what you start

**PROCESS**
Read Architecture.md first | Never invent mechanics | 9/10 certainty threshold before fixes | Holistic impact analysis | Dependency analysis before changes | Complete file reads | Never assume - verify

**DESIGN**
One mechanic, one purpose | Verisimilitude (fiction supports mechanics) | Perfect information (the single player of this single-player rpg can calculate) | Deterministic systems | No soft-locks (always forward progress)

**UI**
Dumb display only (no game logic in UI) | All choices are cards, not buttons | Backend determines availability | Unified screen architecture | Separate CSS files | Clean specificity (no !important hacks) | Resources always visible

**CONTENT**
Package cohesion (references in same package) | Lazy loading with skeletons | No hardcoded content | No string/ID matching | All content from JSON | Parsers must parse (no JsonElement passthrough) | JSON field names MUST match C# property names (NO JsonPropertyName workarounds)

**ASYNC**
Always async/await | Never .Wait(), .Result, .GetAwaiter().GetResult() | No Task.Run or parallel operations | If method calls async, it must be async | No synchronous wrappers | Propagate async to UI

---

## BUILD COMMAND

`cd src && dotnet build`