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
1. Remove from JSON source files
2. Remove from DTO class
3. Remove from Parser code (assignment, validation, logic)
4. Remove from Entity class
5. Remove from all usage (services, UI, wherever it's referenced)

When you CHANGE a property (rename, type change, restructure):
1. Change in JSON source files
2. Change in DTO class
3. Change in Parser code
4. Change in Entity class
5. Change in all usage

**CORRECT APPROACH (HOLISTIC):**
```
"Let me remove travelHubSpotId from the ENTIRE data flow"
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

### 6. SEMANTIC HONESTY - METHOD NAMES MUST MATCH REALITY

**⚠️ CRITICAL: Method names, parameter names, return types, properties, and comments must all align with what the code ACTUALLY does ⚠️**

**THE FUNDAMENTAL RULE:**

Code must be semantically honest. If a method says "Location" it must work on Locations. If a property stores a Venue ID, it must be named "VenueId". If a method returns a Venue, it must say "Venue" in the name.

**VIOLATIONS TO AVOID:**

❌ Method names referencing "Location" that take venueId parameter
❌ Property names referencing "LocationId" that store venue identifiers
❌ Methods named "GetLocationById" that return Venue entities
❌ Comments mentioning "location" when parameters/logic work on venues

**CORRECT PATTERNS:**

✅ Method names must match parameter entity type (RecordVenueVisit takes venueId)
✅ Property names must match stored data type (VenueVisitCount has VenueId property)
✅ Method names must match return type (GetVenueById returns Venue)
✅ All naming layers aligned: method name, parameters, return type, comments

**WHY THIS MATTERS:**

- **Developers trust names** - If a method says "GetLocationById", they expect it to return a Location entity
- **Semantic confusion wastes hours** - Reading code where names lie requires constant mental translation
- **Refactoring becomes impossible** - Can't search for "all methods working on venues" when they're named "Location"
- **Type system can't help** - Compiler allows `Venue GetLocationById()` because both are valid types
- **Future changes break silently** - If you implement actual location-level tracking, existing "Location" methods working on venues will conflict

**THE LITMUS TEST:**

Can you answer these questions by reading ONLY the method signature (no implementation)?
1. What entity type does this method work on?
2. What entity type does this method return?
3. What do the parameters represent?

If NO to any question → Method name lies → Rename it.

**REFACTORING CHECKLIST:**

When you discover semantic dishonesty:

1. **Identify the truth:** Does this method ACTUALLY work on Venues or Locations?
2. **Rename consistently:** Method name, parameter names, property names must match truth
3. **Search exhaustively:** Find ALL call sites (grep for method name)
4. **Update holistically:** Update method signature + ALL call sites + documentation
5. **Build and verify:** Ensure no references broken

**ARCHITECTURAL ALIGNMENT:**

This principle enforces architectural clarity:
- If Venues are organizational wrappers (not gameplay entities), methods working on venues should say "Venue"
- If Locations are spatial entities (actual gameplay happens here), methods working on locations should say "Location"
- Never mix terminology - pick one consistent with the architecture

### 7. SINGLE GRANULARITY PER CONCEPT

**⚠️ CRITICAL: Each concept should be tracked at EXACTLY ONE level of granularity throughout the codebase ⚠️**

**THE FUNDAMENTAL RULE:**

If you track familiarity at the Location level, ALL familiarity tracking must be at Location level. Don't mix venue-level and location-level tracking of the same concept.

**VIOLATION PATTERN:**

❌ Tracking related concepts (familiarity and visit counts) at different granularities (Location vs Venue)
❌ Player has LocationFamiliarity (location-level) but VenueVisits (venue-level)
❌ Creates confusion about relationship between concepts
❌ Data doesn't align: Location familiarity doesn't match Venue visit counts

**CORRECT PATTERN:**

✅ Track all related concepts at same granularity level
✅ If player familiarity tracked at Location level, visits also tracked at Location level
✅ If architectural review determines Venue-level appropriate, ALL tracking moves to Venue level
✅ Consistent granularity across all Player state properties

**WHY THIS MATTERS:**

- **Architectural consistency** - Mixed granularities indicate confused architecture
- **Prevents orphaned data** - If player gains familiarity with Location A (in Venue 1), but visit count tracked for Venue 1, the data doesn't align
- **Simplifies queries** - All related concepts queryable at same level
- **Future-proofs design** - Adding new tracking (discovery, mastery, etc.) has clear granularity to follow

**DECISION FRAMEWORK:**

When choosing granularity, ask:

1. **Where does the player ACTUALLY stand?** (Player is at a Location, not "at a Venue")
2. **What does the gameplay care about?** (Challenges happen at specific spots, not abstract areas)
3. **What level provides meaningful differentiation?** (Two locations in same venue can have different properties)

**WAYFARER ARCHITECTURE:**

- **Venues** = Organizational grouping (clustered hex cells, no spatial position, NO gameplay semantics)
- **Locations** = Spatial entities (actual hex cells where player stands, gameplay happens here)

Therefore:
- ✅ Player state tracked at **Location level** (familiarity, visits, discovery)
- ✅ Routes connect **Locations** (not Venues)
- ✅ Challenges happen at **Locations** (not Venues)
- ❌ NO player state tracked at Venue level (Venues are just organizational)

**CONSISTENCY CHECK:**

After refactoring, audit these questions:
1. Is familiarity tracked per-X?
2. Are visits tracked per-X?
3. Is discovery tracked per-X?
4. Are challenges placed at X?
5. Do routes connect X entities?

X should be THE SAME ANSWER for all questions (e.g., all "Location").

**EXCEPTION:** Venue-level aggregation for UI display is acceptable (e.g., "Show all locations in this venue"), but the SOURCE DATA must be at single granularity.

### 8. UNDERSTAND PLAYER EXPERIENCE AND MENTAL STATE

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

**CORRECT THINKING:**

Player at location → Sees options: "Talk to NPC", "Look around", "Check belongings", "Leave"
"Check belongings" appears WHERE IT MAKES SENSE (not everywhere, only where contextually appropriate)
Journal accessed via button because it's META (out-of-world reference material)
Challenges are IMMERSIVE experiences (full screen, no chrome)

**THE CORRECT QUESTION:**

"What would a REAL PERSON in this situation naturally want to do, and how would they express that intention?"

NOT: "Where should I put this button?"

### 9. SENTINEL VALUES OVER NULL FOR DOMAIN LOGIC

**⚠️ CRITICAL: Never use null to represent domain logic states. Use explicit sentinel values instead. ⚠️**

**THE FUNDAMENTAL RULE:**

Using `null` to mean a specific domain state (like "always eligible", "no restrictions", "default behavior") conflates **uninitialized state** with **intentional domain logic**. This violates DDD principles where domain concepts must be explicit.

**VIOLATIONS TO AVOID:**

❌ **null SpawnConditions = always spawn** (null is not a domain concept)
❌ **null PlacementFilter = spawn anywhere** (implicit logic through absence)
❌ **null Requirements = no requirements** (confuses missing data with intentional design)
❌ **Checking `if (entity.Property == null)` to trigger behavior** (null is implementation detail, not domain concept)

**CORRECT PATTERN - Sentinel Values:**

✅ **Static readonly sentinel value:** `SpawnConditions.AlwaysEligible`
✅ **Internal flag for evaluator:** `internal bool IsAlwaysEligible { get; init; }`
✅ **Parser returns sentinel, not null:** `return SpawnConditions.AlwaysEligible;`
✅ **Evaluator checks flag, not null:** `if (conditions.IsAlwaysEligible) return true;`
✅ **Explicit domain concept:** "AlwaysEligible" is a named state, discoverable via IntelliSense

**EXAMPLE IMPLEMENTATION:**

```csharp
public record SpawnConditions
{
    /// <summary>
    /// Sentinel value indicating scene is always eligible (no temporal filtering)
    /// Use this instead of null to explicitly represent unconditional spawning
    /// DDD pattern: Explicit domain concept, not implicit null check
    /// </summary>
    public static readonly SpawnConditions AlwaysEligible = new SpawnConditions
    {
        IsAlwaysEligible = true,
        PlayerState = new PlayerStateConditions(),
        WorldState = new WorldStateConditions(),
        EntityState = new EntityStateConditions()
    };

    /// <summary>
    /// Flag indicating this is the AlwaysEligible sentinel value
    /// Internal use only - evaluator checks this flag first
    /// </summary>
    internal bool IsAlwaysEligible { get; init; } = false;
}

// Parser returns sentinel instead of null:
public static SpawnConditions ParseSpawnConditions(SpawnConditionsDTO dto)
{
    if (dto == null)
        return SpawnConditions.AlwaysEligible; // ✅ Explicit sentinel
}

// Evaluator checks flag, not null:
public bool EvaluateAll(SpawnConditions conditions, Player player)
{
    if (conditions == null)
        throw new ArgumentNullException(nameof(conditions),
            "SpawnConditions cannot be null. Use SpawnConditions.AlwaysEligible for unconditional spawning.");

    if (conditions.IsAlwaysEligible)
        return true; // ✅ Explicit check for domain concept

    // ... evaluate conditions
}
```

**WHY THIS MATTERS:**

1. **Semantic Clarity** - "AlwaysEligible" is self-documenting, `null` requires comments/docs
2. **IntelliSense Discovery** - Developers find `SpawnConditions.AlwaysEligible` via autocomplete
3. **Fail Fast** - Evaluator throws on actual null (uninitialized), doesn't silently treat as valid
4. **Type Safety** - Sentinel value enforces initialization, prevents accidental null propagation
5. **Refactoring Safety** - Can add properties to AlwaysEligible without null checks scattered everywhere
6. **DDD Compliance** - Domain concept (unconditional spawning) has explicit representation in code

**WHEN TO USE SENTINEL VALUES:**

- Any time null would mean "default behavior" or "no restrictions"
- When absence of data represents a specific domain state
- When evaluators/validators need to check for special cases
- When domain logic routes based on "missing" values

**FORBIDDEN PATTERNS:**

❌ `if (entity.Property == null) { /* special behavior */ }`
❌ Parser returning null to mean "use default"
❌ Null-coalescing operators to provide domain logic defaults: `?? AlwaysEligible`
❌ Documentation saying "null means X" - make X explicit in code

**CORRECT PATTERN:**
- Create static readonly sentinel: `Entity.DefaultValue`, `Entity.NoRestrictions`, `Entity.AlwaysEligible`
- Add internal flag: `internal bool IsDefault { get; init; }`
- Parser/Factory returns sentinel, never null
- Evaluator checks flag first, throws on actual null
- Sentinel value is discoverable, self-documenting, type-safe

### ⚠️ CRITICAL PRINCIPLE: NEVER DELETE REQUIRED FEATURES ⚠️

**IF A FEATURE IS NEEDED BUT NOT YET IMPLEMENTED:**

❌ **WRONG RESPONSE:** "This action type doesn't exist, so I'll remove it from the JSON"
❌ **WRONG RESPONSE:** "This isn't supported yet, so I'll delete it"
❌ **WRONG RESPONSE:** "The code doesn't handle this, so I'll comment it out"

**CORRECT RESPONSE:** "This action type doesn't exist. I will IMPLEMENT it by following the complete vertical slice pattern."

**THE RULE:**
- **NEVER remove necessary content just because it's not implemented yet**
- **ALWAYS implement the missing code to support required features**
- **DELETE ONLY when feature is genuinely unnecessary or wrong design**

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

### ⚠️ PRIME DIRECTIVE: PLAYABILITY OVER IMPLEMENTATION ⚠️

**THE FUNDAMENTAL RULE: A game that compiles but is unplayable is WORSE than a game that crashes.**

**PLAYABILITY VALIDATION (MANDATORY FOR ALL WORK):**

Before marking ANY task complete, you MUST answer these questions with 9/10 certainty:

1. **Can the player REACH this content from game start?**
   - Trace the EXACT player action path from initial spawn location
   - Verify EVERY link in the chain (routes, scenes, NPCs, locations)
   - No missing connections, no broken references, no inaccessible islands

2. **Are ALL required actions VISIBLE and EXECUTABLE?**
   - Player can SEE the action in UI (button, card, scene choice)
   - Player can PERFORM the action (not blocked by missing resources/states)
   - Action produces EXPECTED result (changes game state correctly)

3. **Does the player have FORWARD PROGRESS from every state?**
   - No soft-locks (states where player cannot proceed)
   - No dead-ends (locations with no exit routes)
   - No orphaned content (systems with no entry point)

**THE THREE VALIDATION LEVELS:**

**Level 1: TECHNICAL (Compilation)**
- Code compiles without errors
- No null reference exceptions
- Types match between layers
- ❌ **NOT SUFFICIENT** - Technical correctness ≠ playability

**Level 2: ARCHITECTURAL (Data Flow)**
- JSON → DTO → Parser → Entity → GameWorld → UI
- All references resolve correctly
- No silent failures from missing data
- ❌ **STILL NOT SUFFICIENT** - Architecture correctness ≠ player accessibility

**Level 3: PLAYABILITY (Player Experience)** ✅ **REQUIRED**
- Player can start game
- Player can see available actions
- Player can execute actions
- Player can reach intended content
- Player has forward progress from all states

**FAIL FAST PRINCIPLE (ENFORCEMENT):**

**❌ FORBIDDEN - Silent defaults that hide missing JSON:**

- Using null-coalescing operator (??) to provide empty collections when JSON missing critical data
- Null-checking required content before accessing (if npc.Scenes != null pattern)
- Providing fallback default entities when required entity not found
- Result: Game compiles and runs, but player encounters soft-locks or inaccessible content

**✅ REQUIRED - Throw exceptions for missing critical content:**

- Check for null or empty collections of critical content at parse time
- Throw InvalidOperationException with descriptive message identifying missing content
- Force content creator to fix JSON source data, not hide the problem
- Result: Game crashes immediately at initialization with clear error pointing to exact problem

**PLAYABILITY AUDIT PROCESS:**

For EVERY piece of content (scene, NPC, location, route, challenge):

1. **Trace Player Path:**
   ```
   Game Start (square_center)
     ↓ Can player travel?
   Route exists? (square_to_inn)
     ↓ Can player see route?
   Route visible in UI? (Travel screen)
     ↓ Can player use route?
   Route requirements met? (stamina, knowledge)
     ↓ Does route work?
   Destination exists? (common_room)
     ↓ Can player act at destination?
   Actions/Scenes available? (Elena NPC, tutorial_evening_arrival)
   ```

2. **Validate Each Link:**
   - Link exists in JSON? ✅
   - Link parsed correctly? ✅
   - Link accessible in UI? ✅
   - Link executable by player? ✅
   - Link produces expected result? ✅

3. **Test in Browser:**
   - Start game
   - Follow traced path
   - Verify EVERY action works
   - Reach intended content
   - Confirm forward progress possible

**COMMON PLAYABILITY VIOLATIONS:**

❌ **"Tutorial implemented, scenes work"** - Did you TEST player can reach it?
❌ **"Route parsing works"** - Did you TEST player can see and use the route?
❌ **"NPC spawns correctly"** - Did you TEST player can interact with NPC?
❌ **"Challenge mechanics implemented"** - Did you TEST player can enter challenge?
❌ **"All compilation errors fixed"** - Did you TEST the PLAYER EXPERIENCE?

**THE TEST:**

Can you trace a COMPLETE PATH of player actions from game start to your implemented content? If ANY link in that chain is broken, missing, or inaccessible, the content is NOT PLAYABLE.

The number of actions is IRRELEVANT. What matters:
- Path EXISTS (all links present)
- Path is FUNCTIONAL (all links work)
- Path is DISCOVERABLE (player can find it through normal play)
- No BROKEN LINKS (no null routes, missing scenes, inaccessible NPCs)

**WHY THIS MATTERS:**

- Silent failures waste HOURS debugging "why isn't this working?"
- Inaccessible content is WORTHLESS (might as well not exist)
- Player experience is THE ONLY THING THAT MATTERS
- Technical correctness means NOTHING if player can't play

**GORDON RAMSAY ENFORCEMENT:**

"YOU DONKEY! The game COMPILES but the player can't DO ANYTHING! You validated the ARCHITECTURE but forgot to check if a HUMAN CAN ACTUALLY PLAY THE FUCKING GAME!"

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

### Pattern B: Runtime-Only Navigation (Object ONLY, NO ID)

**When to Use:** Property is runtime state, not from JSON, changes during gameplay

**Characteristics:**
- NO ID property exists
- Object reference property only
- Runtime code uses object reference everywhere consistently
- Changes frequently during gameplay
- Desync risk if you add ID property alongside object

### Pattern C: Lookup on Demand (ID ONLY, NO Object)

**When to Use:** Property from JSON, but lookups are infrequent

**Characteristics:**
- ID property from JSON
- NO cached object reference
- Lookup in GameWorld when needed (rare operations)
- No memory wasted on cached reference

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

### ⚠️ PRIME PRINCIPLE: CATALOGUES ARE PARSE-TIME ONLY ⚠️

**ABSOLUTE RULE - NO EXCEPTIONS:**

**Catalogues are ONLY called from PARSER. NEVER from game logic. NEVER from facades. NEVER at runtime.**

**THE ENTITY IS COMPLETE AFTER PARSING.**

Once the parser finishes, the entity has ALL properties populated. Runtime code queries GameWorld.
Runtime code NEVER generates entities. Runtime code NEVER calls catalogues.

**THIS IS NOT A GUIDELINE. THIS IS AN ARCHITECTURAL CONSTRAINT.**

Violating this principle breaks the entire data flow architecture.

**WHERE CATALOGUES CAN BE CALLED:**
- ✅ Parser classes (LocationParser, NPCParser, etc.) in `src/Content/Parsers/`
- ✅ PackageLoader in `src/Content/PackageLoader.cs`
- ✅ NOWHERE ELSE

**WHERE CATALOGUES ARE FORBIDDEN:**
- ❌ GameFacade
- ❌ Any Facade (LocationFacade, SocialFacade, MentalFacade, PhysicalFacade, etc.)
- ❌ Any Manager (LocationActionManager, etc.)
- ❌ Any Service
- ❌ Any UI Component (.razor, .razor.cs)
- ❌ Any runtime code that executes after game initialization

**HOW TO VERIFY:**
If you see `using Wayfarer.Content.Catalogues;` in ANY file except Parser or PackageLoader → ARCHITECTURAL VIOLATION.

### ⚠️ CRITICAL: CATALOGUES GENERATE ENTITIES, NOT JUST PROPERTIES ⚠️

**THE PATTERN:**

Catalogues do NOT just translate properties. They **PROCEDURALLY GENERATE COMPLETE ENTITIES** from categorical properties.

**CATALOGUE PROCEDURAL GENERATION PATTERN:**

**Catalogue Structure:**
- Static class with static methods that generate complete entity instances
- Examines entity's categorical properties (LocationProperties enum)
- Returns List of procedurally-generated entities based on those properties
- Called ONLY by parser at initialization, NEVER at runtime

**Parser Integration:**
- Parser creates Location entity from JSON
- Parser calls catalogue to generate actions based on Location's properties
- Parser adds generated actions to GameWorld.LocationActions collection
- Entities complete - no further generation needed

**Runtime Behavior:**
- Facades query GameWorld.LocationActions (pre-populated collection)
- Filter actions by location/time/availability criteria
- NO catalogue calls - all entities already exist
- Pure data access, no procedural generation at runtime

**WHY THIS MATTERS:**

1. **Entities complete after parsing** - No runtime generation
2. **Catalogues are parse-time ONLY** - Never imported by facades
3. **String/ID never stored** - Catalogue converts categorical → strongly-typed enum
4. **No JSON for actions** - Actions generated procedurally from properties

**FORBIDDEN PATTERNS:**

❌ **Runtime catalogue calls:**
- Facade method calls LocationActionCatalog.GenerateActionsForLocation during gameplay
- Violates parse-time-only constraint
- Wastes CPU regenerating entities that should already exist
- Indicates entities not properly populated during initialization

❌ **Actions defined explicitly in JSON:**
- JSON contains locationActions array with action definitions
- Actions should be generated procedurally from categorical properties, not authored explicitly
- Violates procedural generation pattern
- Creates maintenance burden (must update JSON for every action variation)

✅ **CORRECT PATTERN:**
- JSON contains only categorical properties (locationProperties: ["Crossroads", "Public"])
- Parser reads properties, calls catalogue to generate actions procedurally
- Catalogue generates Travel action from Crossroads property
- Parser adds generated actions to GameWorld.LocationActions
- Runtime queries pre-populated collection, never calls catalogue

### JSON Authoring Principles (Categorical-First Design)

**CRITICAL RULES FOR JSON CONTENT:**

1. **NO ICONS IN JSON**
   - Icons are UI concerns, not data
   - JSON stores categorical data, UI layer determines visual representation
   - ❌ WRONG: `"icon": "sword-icon.png"`
   - CORRECT: No icon property at all (UI derives icon from categorical properties like type/category)

2. **PREFER CATEGORICAL OVER NUMERICAL**
   - Always prefer categorical properties ("Capable", "Commanding", "Fragile") over numerical values in JSON
   - Content authors describe INTENT and RELATIVE MAGNITUDE, not exact mechanics
   - Catalogues translate intent → mechanics with game state scaling
   - ❌ WRONG: `"bondRequirement": 15` (hardcoded number)
   - CORRECT: `"bondRequirement": "Trusted"` (categorical level translated by catalogue)
   - ❌ WRONG: `"resolveCost": 8` (hardcoded number)
   - CORRECT: `"resolveCost": "Medium"` (tier-based, scales with progression)

3. **VALIDATE ID REFERENCES AT PARSE TIME**
   - ALL ID references in JSON must be validated against GameWorld entities during parsing
   - Parser must throw InvalidOperationException if referenced entity doesn't exist
   - Fail fast at game initialization, not during gameplay
   - ❌ WRONG: Silently ignore missing references or create placeholder
   - CORRECT: `if (!gameWorld.NPCs.ContainsKey(dto.NpcId)) throw new InvalidOperationException($"Situation '{dto.Id}' references unknown NPC '{dto.NpcId}'")`

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

### SITUATION ARCHETYPE SYSTEM

**What Archetypes Are:**

Archetypes are reusable mechanical patterns that define interaction structure. They specify:
- Number of choices (always 2-4, following Sir Brante pattern)
- Action types (Challenge vs Instant Resolution)
- Cost formulas (percentage of player resources, scaled by tier)
- Requirement formulas (stat thresholds, scaled by tier)
- Reward templates (bond changes, scene spawns, scale shifts)

Archetypes contain ZERO narrative content. They are pure mechanical patterns. AI generates all narrative text (situation descriptions, choice action text) from entity context at finalization time.

**The 5 Core Archetypes:**

1. **Confrontation** (Authority/Dominance)
   - **Use Case:** Facing gatekeepers, guards, obstacles requiring assertion
   - **Mechanical Pattern:** Authority stat check OR 15 coins OR Physical challenge OR fallback
   - **Trade-Offs:** Stat path builds authority relationship, coin path avoids relationship, challenge path risks resources, fallback expensive but guaranteed
   - **Narrative Flexibility:** Checkpoint passages, guard confrontations, territorial disputes, dominance challenges

2. **Negotiation** (Diplomacy/Trade)
   - **Use Case:** Merchants, pragmatic NPCs, transactional exchanges requiring persuasion
   - **Mechanical Pattern:** Diplomacy/Rapport stat check OR 15 coins OR Mental challenge OR fallback
   - **Trade-Offs:** Stat path builds social bonds, coin path is instant but no relationship, challenge path tests mental acuity, fallback expensive
   - **Narrative Flexibility:** Price disputes, access requests, favor negotiations, pragmatic deals, warehouse access, lodging arrangements

3. **Investigation** (Insight/Discovery)
   - **Use Case:** Information gathering, puzzle solving, discovering hidden truths
   - **Mechanical Pattern:** Insight/Cunning stat check OR 10 coins (bribe) OR Mental challenge OR fallback
   - **Trade-Offs:** Stat path demonstrates detective skill, coin path shortcuts investigation, challenge path deep analysis, fallback slower but certain
   - **Narrative Flexibility:** Crime investigation, secret discovery, uncovering schemes, research tasks, intelligence gathering

4. **Social Maneuvering** (Rapport/Manipulation)
   - **Use Case:** Social circles, reputations, subtle influence, gaining favor without direct confrontation
   - **Mechanical Pattern:** Rapport/Cunning stat check OR 10 coins (gift) OR Social challenge OR fallback
   - **Trade-Offs:** Stat path builds social standing, coin path buys goodwill, challenge path tests social intelligence, fallback slower
   - **Narrative Flexibility:** Court politics, social climbing, reputation building, favor currying, soft influence

5. **Crisis** (Emergency Response)
   - **Use Case:** Urgent situations requiring immediate decisive action under pressure
   - **Mechanical Pattern:** Authority/Insight stat check OR 25 coins (emergency expenditure) OR Physical challenge OR fallback
   - **Trade-Offs:** Stat path shows leadership under pressure, coin path resource-intensive solution, challenge path physical response, fallback accepts consequences
   - **Narrative Flexibility:** Emergencies, disasters, urgent obstacles, time-critical decisions, life-threatening situations

**How to Use Archetypes in JSON:**

In SituationTemplate, instead of hand-authoring `choiceTemplates` array, specify `archetypeId`:

```json
{
  "id": "initial_offer",
  "type": "Normal",
  "archetypeId": "negotiation",
  "narrativeTemplate": null,
  "priority": 100,
  "narrativeHints": {
    "tone": "business-like",
    "theme": "economic_negotiation",
    "context": "merchant_interaction",
    "style": "pragmatic"
  },
  "choiceTemplates": [],
  "autoProgressRewards": null
}
```

**Key Points:**
- `archetypeId` triggers parse-time choice generation (4 choices created by SituationArchetypeCatalog)
- `narrativeTemplate: null` because AI generates narrative from entity context
- `choiceTemplates: []` empty because archetype generates them
- `narrativeHints` provide AI generation guidance (tone, theme, context, style)

**When to Use Archetypes vs Hand-Authored:**

**Use Archetypes When:**
- Interaction follows a recognizable RPG pattern (negotiation, confrontation, investigation)
- Content needs to scale procedurally (many NPCs, many locations)
- Mechanical balance is critical (tier-scaled costs, progression-appropriate requirements)
- AI narrative generation will provide contextual variety

**Hand-Author When:**
- Tutorial content requiring exact costs for teaching (15 Focus, not formula-based)
- Unique story beats with non-standard mechanical structure
- Choices have unusual action types or special consequences
- Precise control needed over exact mechanical values

**Archetype Selection Guide:**

Ask: "What is the player trying to accomplish?"
- **Assert dominance/authority** → Confrontation
- **Make a deal/persuade pragmatically** → Negotiation
- **Discover information/solve puzzle** → Investigation
- **Build social influence subtly** → Social Maneuvering
- **Respond to urgent crisis** → Crisis

Ask: "What stats should gate success?"
- Authority/Dominance → Confrontation
- Diplomacy/Rapport (transaction) → Negotiation
- Insight/Cunning (discovery) → Investigation
- Rapport/Cunning (social) → Social Maneuvering
- Authority/Insight (emergency) → Crisis

**Categorical Placement with Archetypes:**

Archetypes work best with categorical placement filters (not concrete NPC IDs):

```json
{
  "placementFilter": {
    "placementType": "NPC",
    "personalityTypes": ["Mercantile", "Pragmatic"],
    "locationProperties": ["Commercial", "Public"],
    "minBond": 1,
    "maxBond": 10
  }
}
```

One archetype template spawns at EVERY matching NPC/Location, creating contextual variety through entity properties feeding AI generation.

**Mechanical Guarantees:**

All archetypes provide:
- **Perfect information:** Player sees costs/requirements before committing
- **Multiple valid paths:** No "correct" choice, genuine trade-offs
- **Tier scaling:** Costs/requirements scale with player progression
- **Formula-driven balance:** Consistent resource pressure across game
- **Always forward progress:** Fallback path ensures no soft-locks

---

## ENFORCEMENT RULES

**Code Review - Parser:**
- All categorical JSON properties translated via catalogue?
- All concrete values stored on entity properties (int, bool, object)?
- NO Dictionary<string, X> on entities?
- Catalogue throws on unknown categorical values?
- Parser stores results on entity, not in temporary variables?

**Code Review - Runtime (GameFacade, Facades, Services):**
- NO catalogue imports (catalogues are parse-time only)?
- NO string comparisons (`action.Id == "something"`)?
- NO dictionary lookups (`dict["key"]`, `dict.ContainsKey()`)?
- NO enum switching for behavior (`switch (recoveryType)` in runtime)?
- ONLY concrete property access (`action.HealthRecovery`, `action.CoinCost`)?

**Code Review - Entities:**
- Properties are concrete types (int, bool, strongly-typed classes)?
- NO Dictionary<string, X> properties?
- NO enum properties that control behavior (enums for display/grouping only)?
- Property names describe mechanics, not categories ("HealthRecovery" not "RecoveryType")?

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

**PLACEMENT (presentation context):**
- Entity appears at a location for UI/narrative purposes
- Entity's lifecycle independent of placement
- Placement is metadata on the entity

**REFERENCE (lookup relationship):**
- Entity A needs to find Entity B
- Entity A stores Entity B's ID
- No lifecycle dependency

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
- Parse-time translation: Categorical → strongly-typed (happens ONCE at initialization)
- Context-aware properties: Named for WHERE they're checked (not WHAT they check)
- Semantic decomposition: One categorical input → Many contextual outputs
- Zero runtime interpretation: All meaning decided at parse time

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
- Which entity should own this property architecturally?
- Trace parser → JSON → entity alignment
- Move property to correct entity in all three: JSON, parser, domain
- Delete legacy code that violates architecture
- Categorical properties translate to multiple strongly-typed properties at parse time

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

**NO QUICK FIXES, EVER**
Never implement "pragmatic" workarounds (string parsing, flag checks, conditional logic). Never defer architectural corrections to "later refactoring". Never ship half-implemented solutions. Always do FULL VERTICAL SLICE implementation from the start. If the architecture is wrong, FIX THE ARCHITECTURE FIRST. No shortcuts, no technical debt, no "we'll clean this up later". Quick fixes compound into unmaintainable messes. Do it right the first time or don't do it at all.

---

## ENTITY INITIALIZATION STANDARD ("LET IT CRASH" PHILOSOPHY)

### The Problem: Defensive Programming Hides Bugs

**ANTI-PATTERN** (Defensive Programming):

**Parser hiding missing data:**
- Using null-coalescing on required fields (Title ?? "")
- Provides empty string when JSON missing required property
- Hides missing data errors at parse time
- Results in entities with invalid/empty state that fail later during gameplay

**Parser hiding missing collections:**
- Using null-coalescing on collection properties (PersonalityTypes ?? new List<string>())
- Collections should initialize inline on entity class
- Parser should assign directly without ?? operator
- Redundant defensive code when entity already initializes collection

**Game logic not trusting initialization:**
- Using null-coalescing when querying entity properties (obstacle.GoalIds?.Select(...) ?? new List<Goal>())
- Defensive coding against properties that are NEVER null
- Creates dual source of truth (entity initialization + runtime fallback)
- Wastes CPU and hides architectural violations

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

---

## ⚠️ THE ID ANTIPATTERN: NO STRING ENCODING/PARSING ⚠️

### The Fundamental Rule

**IDs are for UNIQUENESS and DEBUGGING ONLY. NEVER encode data in ID strings. NEVER parse IDs to extract information.**

---

### FORBIDDEN PATTERNS (❌ NEVER DO THIS)

#### **Antipattern 1: Encoding Data in ID Strings**
- Creating IDs using string interpolation with embedded data (Id = "move_to_{destinationId}")
- Data hidden in string format, not accessible via properties
- Violates separation of identity vs data

#### **Antipattern 2: Parsing IDs to Extract Data**
- Runtime code checks ID prefixes (StartsWith("move_to_"))
- Runtime code uses Substring() or string parsing to extract embedded data
- Intent creation depends on parsing ID strings rather than accessing properties

#### **Antipattern 3: String Matching on IDs for Logic Routing**
- Using if/else chains checking ID string patterns for routing (if action.Id.StartsWith("move_to_"))
- Logic branches based on ID format rather than strongly-typed enum
- Runtime behavior coupled to string format conventions

**Why These Are Wrong:**
- **No type safety** - Compiler can't catch typos or format changes
- **Fragile** - Changing ID format breaks everything downstream
- **Hidden data** - Properties aren't discoverable in IntelliSense
- **Magic strings** - Couples implementation to string format
- **Violates strong typing principle** - Bypasses the type system

---

### CORRECT PATTERNS (✅ ALWAYS DO THIS)

#### **Pattern 1: ActionType as Routing Key**

ActionType enum is PRIMARY routing key. Never use ID for routing.

**Implementation:**
- Switch expression on LocationActionType enum
- Each case returns specific intent type
- IntraVenueMove case calls CreateIntraVenueMoveIntent()
- Rest case creates RestAtLocationIntent
- Travel case creates OpenTravelScreenIntent
- No string matching, pure enum routing

#### **Pattern 2: Strongly-Typed Properties for Parameterized Data**

When action needs parameters, add strongly-typed properties to ALL layers.

**Domain Entity:**
- Id property for uniqueness/debugging only
- ActionType property as routing key (enum)
- DestinationLocationId property as strongly-typed parameter (string)

**ViewModel:**
- Id property for uniqueness/debugging only
- ActionType property as routing key (lowercase enum string)
- DestinationLocationId property copied from domain entity

**Intent Creation:**
- Direct property access (action.DestinationLocationId)
- Validation throws if property missing
- No string parsing, no ID parsing
- Pure data access

#### **Pattern 3: Properties Flow Through Entire Data Stack**

**Data Flow:**
- Parse-time (Catalogue): Domain Entity gets DestinationLocationId = "fountain_plaza"
- Query-time (LocationActionManager): ViewModel copies DestinationLocationId = "fountain_plaza"
- Execution (LocationContent.razor.cs): Intent constructor receives "fountain_plaza" via direct property access
- No transformations, no parsing, straight property copying through layers

---

### WHEN IDs ARE ACCEPTABLE

IDs should ONLY be used for:

1. **Uniqueness** (dictionary keys, UI rendering keys):
- Blazor/Razor foreach loops using key attribute for rendering optimization
- ID provides unique identifier for DOM diffing
- No logic, no parsing - purely for framework internal use

2. **Debugging/Logging** (display only, never logic):
- Console.WriteLine or logging statements showing which entity being processed
- ID displayed for diagnostic purposes only
- Never branches logic based on ID value
- Read-only display for developers

3. **Simple Passthrough** (domain → ViewModel, no logic):
- ViewModel copies ID directly from domain entity
- Passthrough for debugging/rendering purposes
- No transformation, no parsing
- ActionType property used for actual routing logic, not ID

**IDs should NEVER be used for:**
- ❌ Routing decisions (use ActionType enum)
- ❌ Conditional logic (use strongly-typed properties)
- ❌ Data extraction (add properties instead)
- ❌ String matching/parsing (violates type safety)

---

### ENFORCEMENT CHECKLIST

Before committing code with IDs, verify:

- [ ] **No string parsing** - No `.Substring()`, `.Split()`, regex on IDs
- [ ] **No string matching** - No `.StartsWith()`, `.Contains()`, `.EndsWith()` on IDs
- [ ] **No encoding** - IDs don't contain `$"prefix_{data}"` patterns
- [ ] **ActionType routing** - Logic routes on ActionType, not ID
- [ ] **Strongly-typed properties** - Parameterized actions have explicit properties
- [ ] **Properties flow correctly** - Domain → ViewModel → Intent has same property

---

---

### WHY THIS MATTERS

**Type Safety:**
- Compiler catches property typos, not runtime string bugs
- IntelliSense shows available parameters
- Refactoring tools find all usages

**Maintainability:**
- Properties document what actions need
- No hidden coupling to ID format
- Easy to add new parameters (just add property)

**Correctness:**
- No string parsing edge cases (empty, malformed, wrong format)
- No magic string constants scattered across codebase
- Clear data flow from domain to execution

**"If you need to parse an ID to get data, you designed it wrong. Add a property."**

---

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
Package cohesion (references in same package) | Lazy loading with skeletons | No hardcoded content | No string/ID matching (see ID ANTIPATTERN section) | All content from JSON | Parsers must parse (no JsonElement passthrough) | JSON field names MUST match C# property names (NO JsonPropertyName workarounds)

**ASYNC**
Always async/await | Never .Wait(), .Result, .GetAwaiter().GetResult() | No Task.Run or parallel operations | If method calls async, it must be async | No synchronous wrappers | Propagate async to UI

---

## BUILD COMMAND

`cd src && dotnet build`