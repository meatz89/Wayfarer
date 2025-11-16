# MANDATORY DOCUMENTATION PROTOCOL (RULE #1)

## THE ABSOLUTE RULE: READ BEFORE ACTING

**BEFORE EVERY REQUEST, ANSWER, PLAN, OR IMPLEMENTATION:**

You MUST achieve 100% CERTAINTY about ALL concepts necessary to accomplish the goal. If there is even the SLIGHTEST SLIVER OF DOUBT or ASSUMPTION about any concept, term, architecture, design pattern, game mechanic, entity relationship, or system interaction, you MUST IMMEDIATELY STOP and READ THE APPROPRIATE DOCUMENTATION.

**This is NOT optional. This is NOT negotiable. This is MANDATORY.**

## COMPREHENSIVE DOCUMENTATION EXISTS

This codebase maintains comprehensive documentation organized into two parallel systems by separation of concerns. Both systems are required for complete understanding.

**Technical Architecture Documentation:**
Organized by HOW the system is built. Contains implementation patterns, component structure, runtime behavior, technical decisions, constraints, and quality requirements. Found in root directory as numbered markdown files following arc42 template structure.

**Game Design Documentation:**
Organized by WHAT the game is and WHY design creates strategic depth. Contains player experience goals, gameplay mechanics, progression systems, resource economy, narrative structure, content generation, balance philosophy, and design decisions. Found in dedicated design directory as numbered markdown files.

**The Separation Principle:**
- Technical docs without design docs = You know HOW but not WHY (implements wrong behavior)
- Design docs without technical docs = You know WHY but not HOW (writes bad code)
- Neither = You're guessing (ABSOLUTELY FORBIDDEN)

Both required together for correct implementation.

## DOCUMENTATION STRUCTURE (BOOTSTRAP)

**Minimal information to begin:**

**Technical Architecture Documentation (Root Directory):**
- Location: Root directory
- Files: Numbered markdown files (01-12) following arc42 template
- Examples: `05_building_block_view.md`, `08_crosscutting_concepts.md`, `12_glossary.md`
- Start here: `12_glossary.md` (technical terms) or `01_introduction_and_goals.md` (overview)

**Game Design Documentation (design/ Subdirectory):**
- Location: `design/` subdirectory
- Files: Numbered markdown files (01-12) parallel to arc42
- Examples: `design/07_content_generation.md`, `design/09_design_patterns.md`, `design/12_design_glossary.md`
- Start here: `design/12_design_glossary.md` (game terms) or `design/README.md` (structure guide)

**Discovery Pattern:**
1. Start with glossaries to learn terminology
2. Follow cross-references between documents
3. Search documentation with Grep for specific concepts
4. Verify current implementation by searching codebase

Everything else about structure, content, and organization is IN the documentation itself.

## SOURCE OF TRUTH HIERARCHY

**When seeking certainty, consult authorities in this order:**

1. **Code** - Ultimate ground truth (what actually runs)
2. **Documentation** - Authoritative explanation (what it means, why it exists)
3. **CLAUDE.md** - Process philosophy (how to work, how to think)

**Resolution rules:**
- Documentation conflicts with code → Code wins (documentation may lag implementation)
- CLAUDE.md conflicts with documentation → Documentation wins (facts trump process)
- CLAUDE.md provides methodology, documentation provides facts, code provides truth

## 100% CERTAINTY REQUIREMENT (NO ASSUMPTIONS)

**Achieving certainty methodology:**

1. **Identify uncertainty:** What concepts do I not fully understand?
2. **Determine concern:** Is this about WHAT/WHY (game design) or HOW/WHERE (technical)?
3. **Read appropriate documentation:** Design docs for player-facing concepts, technical docs for implementation
4. **Cross-reference:** Trace concept through both doc sets for holistic view
5. **Verify in code:** Search codebase to confirm documentation matches reality
6. **Iterate:** If still uncertain, read MORE documentation (never assume)

**Uncertainty indicators (READ DOCS NOW):**
- Don't know what term means
- Don't know why decision was made
- Don't know where code lives
- Don't know how systems interact
- Don't know what entity contains
- Don't know what player experiences
- Any phrase starting with "probably", "I think", "seems like", "might be"

**FORBIDDEN FOREVER:**
- ❌ "I think this might be how it works" → NO. READ THE DOCS.
- ❌ "Based on similar systems..." → NO. VERIFY IN DOCS.
- ❌ "This seems like it should..." → NO. CHECK THE DOCS.
- ❌ "Probably this entity has..." → NO. SEARCH AND READ.

**You are NOT allowed to assume. You are NOT allowed to guess. You MUST KNOW.**

## HOLISTIC UNDERSTANDING REQUIREMENT

**Single system understanding is incomplete:**

Technical documentation alone:
- Explains implementation details
- Shows code organization
- Documents technical patterns
- MISSING: Design intent, player experience goals, why mechanics create depth

Game design documentation alone:
- Explains player experience
- Shows design philosophy
- Documents strategic depth rationale
- MISSING: Implementation location, technical patterns, how to code it

**Both systems required together:**
- Design docs explain WHAT should exist and WHY it matters
- Technical docs explain HOW it's implemented and WHERE it lives
- Cross-references connect the two for navigation
- Verification via codebase search confirms alignment

**Working process:**
1. Read design docs → Understand player-facing intent
2. Read technical docs → Understand implementation approach
3. Search code → Verify current reality
4. Cross-reference → Ensure holistic comprehension
5. ONLY THEN → Plan or implement

## DOCUMENTATION DISCOVERY

**How to find what to read:**

Documentation is self-organizing and cross-referenced. When uncertain about a concept:

1. **Start with glossaries:** Both doc sets have comprehensive glossaries of terms
2. **Follow cross-references:** Documentation links between related concepts
3. **Use README files:** Documentation directories contain structure guides
4. **Search documentation:** Grep for concept name across all docs
5. **Read index/introduction:** First file typically provides navigation map

Documentation locations, filenames, and structure details are IN the documentation itself. CLAUDE.md only establishes the PRINCIPLE that you must read them and HOW to achieve certainty.

## QUICK START BY CONCEPT CATEGORY

**When uncertain about a concept, start with these doc types:**

**Game Entities (what they are):**
- Start: Both glossaries (technical + design) for definitions
- Then: Building block view (arc42) for structure, Design docs for purpose
- Verify: Search codebase for actual class definitions

**Game Mechanics (how they work for players):**
- Start: Design glossaries and core gameplay loops
- Then: Specific design section (challenges, economy, progression, etc.)
- Verify: Search for mechanic implementation in services/facades

**Technical Patterns (how code is organized):**
- Start: Crosscutting concepts (arc42) or pattern catalog
- Then: Architecture decisions for rationale
- Verify: Search for pattern usage across codebase

**Balance/Numbers (costs, rewards, difficulty):**
- Start: Balance philosophy and resource economy (design docs)
- Then: Specific mechanic section for formulas
- Verify: Search JSON files for actual values in use

**Data Flow (how systems connect):**
- Start: Building block view and runtime view (arc42)
- Then: Core gameplay loops (design) for player perspective
- Verify: Trace through code from UI to domain to data

**Content Generation (archetypes, AI, procedural):**
- Start: Content generation section (design docs)
- Then: Catalogue pattern (technical) for implementation
- Verify: Search parsers and catalogues in codebase

**The pattern:** Glossary → Detailed section → Verify in code. Every time.

**This protocol is NOT negotiable. Follow it for EVERY task, no matter how small.**

## HANDLING INSUFFICIENT DOCUMENTATION

**When documentation doesn't fully answer your question:**

1. **Exhaust documentation first:**
   - Read ALL cross-referenced sections (don't stop at first mention)
   - Search docs for related concepts (Grep across all markdown files)
   - Check DDRs/ADRs for historical context and alternatives considered

2. **Verify against code (code is ground truth):**
   - Search codebase for actual implementation
   - Read complete source files (not just snippets)
   - Look for patterns in similar existing features

3. **If still ambiguous:**
   - Document WHAT you know (from docs + code)
   - Document WHAT is uncertain (specific questions)
   - Document WHAT you've already read (show thoroughness)
   - Ask user for clarification with context

4. **NEVER:**
   - Guess based on "similar systems"
   - Assume "it probably works like X"
   - Implement without certainty because "good enough"

**Documentation conflicts:**
- Docs conflict with each other → Code is ground truth
- Example conflicts with rule → Rule is ground truth, example may be outdated
- Multiple examples show different patterns → Ask which is canonical

**The principle:** Uncertainty is acceptable. Guessing is not. Ask informed questions.

---

# CRITICAL INVESTIGATION PROTOCOL

**BEFORE ANY WORK:**
1. **READ DOCUMENTATION FIRST** (see MANDATORY DOCUMENTATION PROTOCOL above)
2. Search codebase exhaustively (Glob/Grep for ALL references)
3. Read COMPLETE files (no partial reads unless truly massive)
4. Understand architecture (GameWorld, screens, CSS, domain entities, services, data flow)
5. Verify ALL assumptions (search before claiming exists/doesn't exist)
6. Map dependencies (what breaks if I change this?)

**HOLISTIC DELETION (Parser-JSON-Entity Triangle):**
When deleting/changing ANY property, update ALL FIVE layers:
- JSON source → DTO class → Parser code → Entity class → Usage (services/UI)

**SEMANTIC HONESTY:**
Method names MUST match reality. `GetVenueById` returning `Location` is FORBIDDEN. Parameter types, return types, property names, method names must align.

**SINGLE GRANULARITY:**
Track related concepts at ONE granularity level. If familiarity tracked per-Location, visits also per-Location. Never mix.

**PLAYER MENTAL STATE:**
Before UI changes, ask: What is player DOING/THINKING/INTENDING? Visual novel = choices as cards, not buttons/menus. Actions appear where contextually appropriate.

**SENTINEL VALUES OVER NULL:**
Never use null for domain logic. Create explicit sentinels: `SpawnConditions.AlwaysEligible` with internal flag. Parser returns sentinel, evaluator checks flag, throw on actual null.

**PLAYABILITY OVER COMPILATION:**
Game that compiles but is unplayable is WORSE than crash. Before marking complete:
1. Can player REACH this from game start? (trace exact path)
2. Are ALL actions VISIBLE and EXECUTABLE?
3. Forward progress from every state?

Test in browser, verify EVERY link works. Inaccessible content is worthless.

**NEVER DELETE REQUIRED FEATURES:**
If feature needed but unimplemented, IMPLEMENT it (full vertical slice). Delete only if genuinely wrong design or dead code.

---

# ARCHITECTURE AND DESIGN REFERENCE

**For complete system architecture details:**
- Read `05_building_block_view.md` - Component structure, ownership hierarchy, entity relationships
- Read `03_context_and_scope.md` - System boundaries, gameplay loops, spatial hierarchy
- Read `12_glossary.md` - Technical entity definitions

**For complete game design principles:**
- Read `design/01_design_vision.md` - Core philosophy, design principles
- Read `design/02_core_gameplay_loops.md` - Strategic/tactical layer flow
- Read `design/09_design_patterns.md` - Design patterns and anti-patterns
- Read `design/12_design_glossary.md` - Game design term definitions

**Quick architectural reminders (details in docs):**
- GameWorld is single source of truth (owns all entities)
- Situations EMBEDDED in Scenes (no separate collection)
- Ownership vs Placement vs Reference (different lifecycle patterns)
- Use glossaries and search codebase to verify current implementation

---

# TECHNICAL PATTERNS REFERENCE

**For complete technical pattern details:**
- Read `08_crosscutting_concepts.md` - HIGHLANDER, Catalogue Pattern, Entity Initialization, Parse-time translation
- Read `09_architecture_decisions.md` - ADRs explaining why patterns were chosen
- Read `ARCHITECTURAL_PATTERNS.md` - Detailed pattern catalog

**For complete coding standards:**
- Read `CODING_STANDARDS.md` - Type system, naming conventions, code quality rules

---

# HEX-BASED SPATIAL ARCHITECTURE PRINCIPLE

**Principle:** Entity relationships are established through spatial positioning on hex grid and object references, NOT through ID cross-references. IDs do not exist in domain entities.

**Exception:** Template IDs are acceptable (SceneTemplate.Id, SituationTemplate.Id) because templates are immutable archetypes, not mutable entity instances. Templates are content definitions, not game state.

**Why:** This architecture separates spatial data (hex coordinates) from entity identity, enabling procedural content generation and eliminating redundant ID storage. Location.HexPosition is source of truth for spatial positioning. Routes are generated procedurally via pathfinding, not manually defined in JSON.

## Spatial Scaffolding Pattern

**JSON Layer - Hex Coordinates as Source of Truth:**
- Locations defined with hex coordinates (Q, R) indicating spatial position
- NPCs defined with hex coordinates (where they spawn in the world)
- Routes NOT defined in JSON - generated procedurally from hex grid
- Hex grid defines terrain, danger levels, and traversability

**Parser Layer - Spatial Resolution:**
- Resolves entity relationships via hex coordinate matching
- Creates object references during parsing based on spatial proximity
- Builds object graph without requiring ID lookups
- Parser uses categorical properties to find/create entities (EntityResolver.FindOrCreate pattern)

**Domain Layer - Object References Only:**
- Entities have object references, NOT ID strings
- NPC has `Location` object reference (not `LocationId` string)
- RouteOption has `OriginLocation`, `DestinationLocation` objects (not ID strings)
- Location.HexPosition (AxialCoordinates) is spatial source of truth

**Runtime - Procedural Generation:**
- Routes generated via A* pathfinding: Origin.HexPosition → Destination.HexPosition
- Travel system navigates hex grid terrain and danger levels
- No hardcoded route definitions - all routes emerge from spatial data

## Type-Safe Routing (Related Pattern)

**Enum-based routing for action dispatch:**
- ActionType enum as routing key (switch on enum, NOT ID parsing)
- Strongly-typed properties for parameters (DestinationLocation object, not LocationId string)
- Properties flow through entire stack with compiler verification
- Direct object access (action.DestinationLocation.Name), NO string lookups

## Why IDs Do Not Exist in Domain

**IDs are NOT needed anywhere:**
- NOT needed in DTOs - parsers use categorical properties to find/create entities
- NOT needed in domain entities - entities use object references
- NOT needed for debugging - Name and categorical properties provide context
- Exception: Template IDs acceptable (immutable content definitions, not game state)

**IDs create redundancy:**
- Current WRONG pattern: RouteOption has OriginLocationId (string) + OriginLocation (object)
- CORRECT pattern: RouteOption has only OriginLocation (object)
- Storing both ID and object reference violates Single Source of Truth

**IDs pollute domain with database thinking:**
- Domain models game entities (NPC, Location, Route), not database rows
- Object references are natural domain relationships
- ID lookups are SQL query patterns that don't belong in object-oriented domain

**IDs enable violations:**
- Composite ID generation: `routeId = $"route_{origin.Id}_{destination.Id}"` ❌
- ID parsing for logic: `if (id.StartsWith("route_"))` ❌
- Hash code misuse: `(day * ID.GetHashCode()) % count` ❌
- GetHashCode misuse: `int seed = ID.GetHashCode()` ❌
- Hash-based selection: `PersonalityType = types[hash % types.Length]` ❌

## Correct Patterns

**Use object references for relationships:**
```csharp
// CORRECT - Direct object references, NO IDs
public class NPC
{
    // NO ID property
    public string Name { get; set; }
    public Location Location { get; set; }  // Object reference
}

public class RouteOption
{
    // NO Id property
    public Location OriginLocation { get; set; }  // Object reference
    public Location DestinationLocation { get; set; }  // Object reference
    public List<AxialCoordinates> HexPath { get; set; }  // Spatial path
}
```

**Use hex coordinates for spatial positioning:**
```csharp
// CORRECT - Spatial positioning via hex coordinates, NO IDs
public class Location
{
    // NO Id property
    public string Name { get; set; }
    public AxialCoordinates HexPosition { get; set; }  // Spatial source of truth
}

// Routes generated procedurally from spatial data
List<AxialCoordinates> hexPath = pathfinder.FindPath(origin.HexPosition, destination.HexPosition);
RouteOption route = new RouteOption
{
    OriginLocation = origin,  // Object reference
    DestinationLocation = destination,  // Object reference
    HexPath = hexPath
};
```

**Use enums for categorical routing:**
```csharp
// CORRECT - Enum-based action routing
public enum ActionType
{
    Travel,
    Conversation,
    Investigation,
    Rest
}

// Dispatcher switches on enum, not string parsing
switch (action.Type)
{
    case ActionType.Travel:
        HandleTravel(action.DestinationLocation);  // Object reference
        break;
    case ActionType.Conversation:
        HandleConversation(action.TargetNPC);  // Object reference
        break;
}
```

**Use categorical properties to find/create entities:**
```csharp
// CORRECT - EntityResolver.FindOrCreate pattern
public Location FindOrCreateLocation(PlacementFilter filter)
{
    // Query existing by categorical properties
    Location existing = _gameWorld.Locations
        .Where(loc => loc.LocationProperties.Contains(filter.PropertyRequired))
        .Where(loc => loc.Purpose == filter.Purpose)
        .Where(loc => loc.Safety == filter.Safety)
        .FirstOrDefault();

    if (existing != null) return existing;  // Found - return object

    // Not found - create new from categorical properties
    Location newLocation = new Location
    {
        Purpose = filter.Purpose,
        Safety = filter.Safety,
        LocationProperties = filter.Properties
    };
    _gameWorld.Locations.Add(newLocation);
    return newLocation;  // Return object reference, NO ID
}
```

---

# EXPLICIT PROPERTY PRINCIPLE

**Principle:** Use explicit strongly-typed properties for state modifications. Never route property changes through string-based generic systems.

**Why:** String property names require runtime parsing and fail silently. Strongly-typed properties catch errors at compile time and make intent explicit. Generic "flexible" systems are YAGNI violations - add properties when actually needed, not hypothetically.

**Correct pattern:**
- Explicit strongly-typed properties: `LocationsToUnlock`, `LocationsToLock`
- Direct property modification: `location.IsLocked = false` (no string matching)
- Add new properties when needed: `LocationsToHide`, `LocationsToReveal`
- Each property serves one purpose only

**Catalogues for entity generation, explicit properties for state modification.**

---

# GLOBAL NAMESPACE PRINCIPLE

**Principle:** Use global namespace for all domain code to expose conflicts immediately and eliminate organizational overhead.

**Why:** Custom namespaces hide duplicate classes and redundant implementations by allowing them to coexist in different namespaces. Directory structure already provides organization. Global namespace forces the compiler to detect conflicts, eliminates boilerplate, and reduces refactoring friction.

## Policy

**Domain code (GameState, Services, Subsystems, etc.):**
- No namespace declarations
- All classes globally available
- Compiler enforces uniqueness

**Blazor components (framework requirement):**
- `namespace Wayfarer.Pages.Components;` allowed
- Razor runtime requires namespaces

**Test projects:**
- `namespace Wayfarer.Tests;` allowed
- Standard test project convention

## Why Custom Namespaces Are Architecturally Harmful

### 1. Hiding Duplicate Classes

**With custom namespaces (PROBLEM):**
```csharp
// File: GameState/MessageCategory.cs
namespace Wayfarer.GameState;
public enum MessageCategory { ... }

// File: Models/MessageCategory.cs
namespace Wayfarer.Models;
public enum MessageCategory { ... } // DUPLICATE HIDDEN BY NAMESPACE!

// Compiler doesn't complain - both exist in different namespaces
// Code becomes confusing - which MessageCategory is authoritative?
```

**Without namespaces (SOLUTION):**
```csharp
// File: GameState/MessageCategory.cs
public enum MessageCategory { ... }

// File: Models/MessageCategory.cs
public enum MessageCategory { ... } // COMPILER ERROR: Duplicate class!

// Conflict exposed immediately - forces cleanup
// Only ONE MessageCategory can exist
```

### 2. Hiding Redundant Files

Custom namespaces allow redundant implementations to coexist:
- `Wayfarer.GameState.SystemMessage` vs `Wayfarer.Models.SystemMessage`
- `Wayfarer.Services.TokenManager` vs `Wayfarer.Subsystems.Token.TokenManager`
- Multiple competing implementations hidden by namespace separation

**Global namespace forces ONE canonical implementation.**

### 3. False Sense of Organization

**The lie:** "Namespaces organize code logically"
**The truth:** Directory structure ALREADY organizes code

```
GameState/
  NPC.cs              → public class NPC { ... }
  Location.cs         → public class Location { ... }
Services/
  GameFacade.cs       → public class GameFacade { ... }
Subsystems/
  Token/
    TokenManager.cs   → public class TokenManager { ... }
```

**Directory structure IS the organization.** Namespaces add nothing but complexity.

### 4. Namespace Declaration Overhead

**Every file needs:**
- Namespace declaration at top (boilerplate)
- Using statements in consuming files (more boilerplate)
- Mental overhead tracking which namespace each class lives in

**Global namespace eliminates all overhead:**
- No namespace declarations
- No using statements (except framework/external libraries)
- Class name uniqueness enforced automatically

### 5. Implicit Usings Don't Help

C# 10+ implicit usings only import FRAMEWORK namespaces:
- `System.*`
- `System.Collections.Generic.*`
- `Microsoft.AspNetCore.*`

**Custom namespaces STILL require explicit using statements.**

With custom namespaces:
```csharp
using Wayfarer.GameState;        // Required
using Wayfarer.Services;         // Required
using Wayfarer.Subsystems.Token; // Required

public class SomeClass { ... }
```

Without namespaces:
```csharp
// No using statements needed - all domain classes globally available

public class SomeClass { ... }
```

### 6. Refactoring Brittleness

Moving a class between directories with namespaces:
1. Change namespace declaration in file
2. Update ALL using statements in ALL consuming files
3. Fix build errors from missed using statements
4. Search/replace namespace references in tests

Moving a class between directories WITHOUT namespaces:
1. Move the file (done)

**Global namespace = zero refactoring overhead.**

## Correct Pattern

**Domain code (GameState, Services, Subsystems, etc.):**
```csharp
// File: GameState/MessageCategory.cs
/// <summary>
/// Semantic category of system messages
/// </summary>
public enum MessageCategory
{
    ResourceChange,
    TimeProgression,
    Discovery,
    Achievement,
    Danger
}
```

**NO namespace declaration. Globally available.**

**Blazor components (EXCEPTION - namespaces allowed):**
```csharp
// File: Pages/Components/MessageDisplay.razor.cs
using Microsoft.AspNetCore.Components;

namespace Wayfarer.Pages.Components;

public class MessageDisplayBase : ComponentBase
{
    // MessageCategory available without using statement (global namespace)
    protected string GetIconForCategory(MessageCategory category) { ... }
}
```

**Blazor components need namespaces for Razor runtime.**

## The Principle

**Namespaces for LIBRARIES (external consumption).**
**NO namespaces for APPLICATIONS (internal code).**

This is an application, not a library. Global namespace is correct.

**Gordon Ramsay Standard:**

"You've wrapped EVERYTHING in namespaces to 'organize' the code? The DIRECTORY STRUCTURE is the organization! Now you've got THREE different `TokenManager` classes in different namespaces and nobody knows which one is canonical! RIP OUT the custom namespaces, let the compiler SCREAM about the duplicates, and DELETE the redundant garbage!"

---

# DOMAIN COLLECTION PRINCIPLE

**Principle:** Use `List<T>` with LINQ for all domain entity collections to optimize for maintainability and semantic clarity.

**Why:** This is a small-scale, single-player, turn-based game where performance optimization is premature. Dictionary/HashSet optimize for scale this game doesn't have, while introducing complexity, semantic dishonesty, and debugging friction. List<T> with LINQ provides readable domain queries, fail-fast errors, and architectural purity.

## THE GAME CONTEXT (WHY PERFORMANCE DOESN'T MATTER)

**This game is NOT:**
- ❌ A massively multiplayer online game with thousands of concurrent entities
- ❌ A real-time simulation processing millions of events per second
- ❌ A distributed system with network latency concerns
- ❌ A high-frequency trading platform requiring microsecond response times

**This game IS:**
- ✓ Synchronous (single-threaded execution, no concurrency)
- ✓ Browser-based (JavaScript VM speed is irrelevant, browser render time dominates)
- ✓ Single-player (one human making decisions at human speed: hundreds of milliseconds)
- ✓ Minimal scale (collections contain 10-100 entities maximum, not thousands)
- ✓ Turn-based narrative (player reads text, makes choice, reads result - seconds between actions)

**The Performance Reality:**
- Typical collections: 20 NPCs, 30 Locations, 50 Items, 10 Scenes
- Linear scan of 100 items: **~0.001 milliseconds** (one microsecond)
- Browser render time: **16+ milliseconds** (one frame at 60fps)
- Human reaction time: **200+ milliseconds** (reading and decision-making)
- Network latency: **50-200 milliseconds** (even on localhost)

**THE PERFORMANCE BENEFIT OF DICTIONARY IS LITERALLY UNMEASURABLE IN THIS CONTEXT.**

Using Dictionary/HashSet for "performance" is like using a forklift to carry a sandwich. It's technically faster, but:
1. The improvement is completely undetectable by any measurement
2. The complexity cost is very real and very harmful
3. The maintainability burden compounds over time

## Why Dictionary/HashSet Are Architecturally Wrong

**Dictionary/HashSet optimize for SCALE. This game doesn't SCALE. Therefore, optimization is PREMATURE.**

### 1. Semantic Dishonesty (Domain Inversion)

**Dictionary inverts the domain model:**
```csharp
// WRONG - Technical data structure leaking into domain
public class GameWorld
{
    private Dictionary<string, NPC> _npcLookup;
    private Dictionary<string, Location> _locationCache;
    private HashSet<string> _visitedLocationIds;
}
```

This is **DATABASE THINKING**, not **DOMAIN THINKING**. You're organizing code around INDEXES, not ENTITIES.

**Problem:** NPCs aren't "keyed by ID" - they ARE NPCs, and ID is just one property among many. Dictionary makes ID the primary organizing principle, which is semantically backwards.

**CORRECT - Domain collections:**
```csharp
public class GameWorld
{
    private List<NPC> _npcs;
    private List<Location> _locations;
    private List<VisitRecord> _visitHistory;
}
```

This is **DOMAIN THINKING**. GameWorld contains ENTITIES (the things that exist in the game world).

### 2. Single Responsibility Violation

**Dictionary does TWO things:**
1. Stores entities (storage responsibility)
2. Provides fast lookup by key (query optimization responsibility)

In a game where (2) is unnecessary, you're adding complexity for zero benefit.

**List does ONE thing:**
1. Stores entities in order (storage responsibility only)

**Lookup is a QUERY concern, not a STORAGE concern.** Separation of concerns suggests storage should be simple.

### 3. Fail-Slow vs Fail-Fast Philosophy

**Dictionary behavior (FAIL-SLOW):**
```csharp
// Access pattern 1: Exception at wrong location
var npc = _npcs[id]; // Throws KeyNotFoundException
// Stack trace points HERE, but actual problem is "ID doesn't exist in collection"

// Access pattern 2: Silent null propagation
if (_npcs.TryGetValue(id, out var npc))
{
    npc.Name; // Works
}
else
{
    // Error handling required at EVERY call site
    npc = null; // Or default
}
npc.Name; // May crash LATER if TryGetValue failed
```

**List behavior (FAIL-FAST):**
```csharp
var npc = _npcs.FirstOrDefault(n => n.Id == id); // Returns null if not found
var name = npc.Name; // IMMEDIATELY throws NullReferenceException
// Stack trace points EXACTLY to the problem: NPC doesn't exist
// No silent propagation, no deferred errors
```

**Why fail-fast is better:**
- Errors happen at point of use, not at retrieval
- Stack traces are clear and actionable
- No need for defensive `TryGetValue` everywhere
- Null-reference errors immediately reveal logic bugs

### 4. Query Expressiveness Asymmetry

**Dictionary limitations:**
```csharp
// Can ONLY query by key efficiently
var npc = _npcs[id]; // O(1) lookup

// ANY other query requires LINQ over .Values anyway
var innkeeper = _npcs.Values.FirstOrDefault(n => n.Profession == "Innkeeper"); // O(n)
var friendlyNpcs = _npcs.Values.Where(n => n.Demeanor == Demeanor.Friendly).ToList(); // O(n)
var authorities = _npcs.Values.Where(n => n.SocialStanding == SocialStanding.Authority).ToList(); // O(n)

// So you're using Dictionary as a List with extra steps!
```

**You're paying the complexity cost of Dictionary but still doing O(n) scans for 99% of queries.**

**List uniformity:**
```csharp
// ALL queries are uniform and declarative
var npc = _npcs.FirstOrDefault(n => n.Id == id); // O(n) - but n=20, so irrelevant
var innkeeper = _npcs.FirstOrDefault(n => n.Profession == "Innkeeper"); // O(n)
var friendlyNpcs = _npcs.Where(n => n.Demeanor == Demeanor.Friendly).ToList(); // O(n)
var authorities = _npcs.Where(n => n.SocialStanding == SocialStanding.Authority).ToList(); // O(n)
```

**Every query looks the same. Every query reads like English. Every query is a domain question.**

### 5. YAGNI Violation (You Aren't Gonna Need It)

**Dictionary optimizes for a problem you DON'T HAVE:**
- O(1) lookup vs O(n) scan sounds important
- For n=1000+, it IS important
- For n=20, it's COMPLETELY IRRELEVANT

**Math:**
- Dictionary lookup: ~0.0001ms (hash calculation + array access)
- List scan of 20 items: ~0.001ms (20 equality checks)
- Difference: **0.0009 milliseconds**
- Browser render frame: **16 milliseconds**
- Human perception threshold: **100+ milliseconds**

**You're optimizing something that takes 0.001ms in a system where humans react in 200ms. This is ABSURD.**

### 6. Debugging Visibility

**Dictionary in debugger:**
```
_npcs: Dictionary<string, NPC> (Count = 5)
  [0]: {["npc_001", Wayfarer.GameState.NPC]}
  [1]: {["npc_002", Wayfarer.GameState.NPC]}
  [2]: {["npc_003", Wayfarer.GameState.NPC]}
```

Can't see entity properties without expanding each KeyValuePair. Debugging requires extra clicks.

**List in debugger:**
```
_npcs: List<NPC> (Count = 5)
  [0]: {NPC: Elena, Profession: Innkeeper, Demeanor: Friendly}
  [1]: {NPC: Marcus, Profession: Guard, Demeanor: Neutral}
  [2]: {NPC: Thalia, Profession: Merchant, Demeanor: Hostile}
```

Immediate visibility of entity state. Debug by LOOKING, not by EXPANDING.

### 7. Testing Simplicity

**Dictionary test setup (ID DUPLICATION):**
```csharp
var npcs = new Dictionary<string, NPC>
{
    ["npc_001"] = new NPC { Id = "npc_001", Name = "Elena" },
    ["npc_002"] = new NPC { Id = "npc_002", Name = "Marcus" }
    // ERROR-PRONE: ID appears TWICE (key + property)
    // If they mismatch, silent bugs
};
```

**List test setup (NO DUPLICATION):**
```csharp
var npcs = new List<NPC>
{
    new NPC { Id = "npc_001", Name = "Elena" },
    new NPC { Id = "npc_002", Name = "Marcus" }
    // ID appears ONCE, no duplication risk
};
```

### 8. Functional Thinking vs Imperative Thinking

**List encourages declarative pipelines (FUNCTIONAL):**
```csharp
var result = _npcs
    .Where(n => n.Location == currentLocation)
    .OrderBy(n => n.Name)
    .Select(n => new NPCViewModel { Name = n.Name })
    .ToList();
// Pipeline: Filter → Sort → Transform → Collect
// Easy to read, easy to modify, easy to test
```

**Dictionary encourages imperative loops (PROCEDURAL):**
```csharp
var result = new List<NPCViewModel>();
foreach (var kvp in _npcs)
{
    if (kvp.Value.Location == currentLocation)
    {
        result.Add(new NPCViewModel { Name = kvp.Value.Name });
    }
}
result.Sort((a, b) => a.Name.CompareTo(b.Name));
// More code, more mutable state, harder to reason about
```

### 9. Type Safety Erosion

**Dictionary often leads to type erasure:**
```csharp
// Common anti-pattern: generic storage
Dictionary<string, object> _entities; // Lost ALL type information
// or
Dictionary<string, IEntity> _entities; // Forcing abstraction for no reason
```

**List preserves concrete types:**
```csharp
List<NPC> _npcs;
List<Location> _locations;
List<Item> _items;
// Each collection is STRONGLY TYPED to its domain entity
// Compiler catches type errors at compile time
```

### 10. Architectural Purity (Repository Pattern)

**Dictionary pattern (TECHNICAL ABSTRACTION):**
```csharp
// Implies "fast lookup structure" - this is an implementation detail
Dictionary<string, NPC> _npcs;
var npc = _npcs[id]; // What if id doesn't exist? Runtime exception!
```

**List pattern (DOMAIN ABSTRACTION):**
```csharp
// Implies "collection of entities" - this is a domain concept
List<NPC> _npcs;
var npc = _npcs.FirstOrDefault(n => n.Id == id); // Domain query, safe
```

**The repository stores ENTITIES, not KEY-VALUE PAIRS.** GameWorld is a domain model, not a database schema.

## The Root Principle

**Dictionary/HashSet optimize for SCALE.**
**This game doesn't SCALE.**
**Therefore, optimization is PREMATURE and HARMFUL.**

Using Dictionary for 20 NPCs is like:
- Using a database index on a table with 10 rows
- Using a CDN for a file served once per day
- Using multithreading for a calculation that takes 1 microsecond
- Using a distributed cache for data that fits in 1KB

**It's TECHNICALLY faster, but:**
1. The improvement is completely unmeasurable
2. The complexity cost is very real
3. The maintenance burden compounds over time

**Gordon Ramsay Standard:**

"You're using a DICTIONARY for 20 NPCS? That's like using a FORKLIFT to carry a SANDWICH! O(1) lookup? For TWENTY ENTITIES? The performance gain is ONE MICROSECOND! Your browser takes SIXTEEN MILLISECONDS to render a FRAME! You've added complexity for LITERALLY ZERO BENEFIT! This is OVER-ENGINEERED NONSENSE!"

## Correct Pattern

**ALWAYS use `List<T>` for entity collections:**

```csharp
// Domain entity collections (CORRECT)
public class GameWorld
{
    private List<NPC> _npcs = new List<NPC>();
    private List<Location> _locations = new List<Location>();
    private List<Scene> _scenes = new List<Scene>();
    private List<Route> _routes = new List<Route>();
    private List<Item> _items = new List<Item>();
}

// Query patterns (declarative, readable, safe)
public NPC GetNPCById(string id)
{
    return _npcs.FirstOrDefault(n => n.Id == id);
    // Returns null if not found (fail-fast at call site)
}

public List<NPC> GetNPCsByLocation(string locationId)
{
    return _npcs.Where(n => n.CurrentLocation == locationId).ToList();
    // Reads like English: "NPCs where location matches"
}

public List<NPC> GetFriendlyNPCs()
{
    return _npcs.Where(n => n.Demeanor == Demeanor.Friendly).ToList();
    // Domain query, not technical lookup
}
```

**LINQ queries are:**
- Declarative (what, not how)
- Readable (reads like English)
- Composable (chain operations easily)
- Testable (pure functions, no side effects)
- Type-safe (compiler catches errors)

## Rare Exceptions (When Dictionary Is Acceptable)

**Dictionary is acceptable ONLY for:**

1. **Framework requirements (external APIs):**
   ```csharp
   // Blazor component parameters require Dictionary
   var parameters = new Dictionary<string, object>
   {
       ["Value"] = someValue
   };
   ```

2. **Configuration/settings (non-domain data):**
   ```csharp
   // Application configuration (not game entities)
   Dictionary<string, string> appSettings = configuration.GetSection("Settings");
   ```

3. **Caching external API responses (if actually needed):**
   ```csharp
   // Cache for slow external API calls (Ollama, etc.)
   // ONLY if profiling proves it's a bottleneck
   Dictionary<string, OllamaResponse> _responseCache;
   ```

**Dictionary is NEVER acceptable for domain entities:**
- GameWorld entity collections (NPCs, Locations, Scenes, Routes, Items)
- Player state (inventory, stats, relationships)
- Game session data (visited locations, completed scenes)

**The test:** If it's a domain entity or game state, use `List<T>`. No exceptions.

## Summary

**This game optimizes for MAINTAINABILITY, not PERFORMANCE.**

Dictionary/HashSet are performance optimizations. In a game with:
- 20 NPCs (not 20,000)
- Single-threaded execution (not concurrent)
- Human-speed interactions (not microsecond latency)
- Browser-based rendering (not real-time simulation)

**Performance optimization is PREMATURE, HARMFUL, and FORBIDDEN.**

Use `List<T>` with LINQ. Write readable, maintainable, domain-driven code. Let the code be CLEAR, not CLEVER.

**"Premature optimization is the root of all evil." - Donald Knuth**

In this codebase, Dictionary for domain entities IS premature optimization. Use List<T>.

---

# ICON SYSTEM (NO EMOJIS)

**CRITICAL PATTERN: Professional scalable graphics required for all visual content.**

## POLICY

**FORBIDDEN:**
- ❌ Emojis for game content display (💰 coins, ❤️ health, 💪 strength, 🎯 skills, ⚔️ combat)
- ❌ Emojis in code comments, documentation, commit messages
- ❌ Unicode symbols for resource/stat display (★ ◆ ● ▲)
- ❌ Text-based pseudo-graphics in UI

**ALLOWED (Minimal Exceptions):**
- ✓ Basic interface controls ONLY (✕ close buttons, ✓ checkmarks, → arrows)
- Must be purely functional UI elements, NOT game content
- Must not represent resources, stats, or player-facing entities

**REQUIRED:**
- SVG icons from game-icons.net via Icon component
- All resource/stat/entity icons must be vector graphics
- Cohesive visual style (single icon library: Game Icons collection)
- Attribution documented in THIRD-PARTY-LICENSES.md

## WHY PROPER ICONS MATTER

**Professional quality standard:**
- Vector graphics scale to any resolution (emoji quality degrades)
- Consistent visual style creates polished game aesthetic
- SVG allows dynamic color theming (emojis fixed appearance)

**Technical superiority:**
- Scalability and resolution independence (vector vs raster)
- Customizable via CSS (color, size, filters, animations)
- Predictable rendering across all platforms and browsers
- Accessibility support (ARIA labels, screen reader compatible)

**Cross-platform reliability:**
- Emojis render differently on Windows/Mac/Linux/Mobile
- SVG displays identically everywhere
- No font fallback issues or missing glyph problems

**Player experience:**
- Icons convey game identity and theme
- Professional presentation builds player trust
- Consistent iconography aids learning and recognition

## FRONTEND USAGE (Icon Component)

**Basic usage:**
```razor
<Icon Name="coins" />
<Icon Name="hearts" />
<Icon Name="brain" />
```

**With CSS class for styling:**
```razor
<Icon Name="coins" CssClass="resource-coin" />
<Icon Name="target-arrows" CssClass="icon-neutral" />
<Icon Name="sparkles" CssClass="icon-positive" />
```

**Component parameters:**
- `Name` (required): Icon filename without .svg extension
- `Size` (optional): Width/height, defaults to "16px"
- `Color` (optional): SVG fill color, defaults to "currentColor"
- `CssClass` (optional): Additional CSS classes for semantic styling

**CSS classes for semantic colors:**
- Five Stats: `stat-insight`, `stat-rapport`, `stat-authority`, `stat-diplomacy`, `stat-cunning`
- Resources: `resource-coin`, `resource-health`, `resource-stamina`, `resource-focus`, `resource-hunger`
- Generic: `icon-neutral`, `icon-positive`, `icon-negative`

**Performance:**
- Icons cached after first load (ConcurrentDictionary, thread-safe)
- No redundant HTTP requests for same icon
- Inline SVG for styling flexibility

## BACKEND PATTERNS (Token System - To Be Implemented)

**Message token replacement pattern:**

When backend generates player-facing messages containing icons, use token system for icon injection at render time.

**Token format convention:**
```csharp
// Backend generates message with tokens
_messageSystem.AddSystemMessage("{icon:coins} Spent {0} coins on {1}", amount, item);
_messageSystem.AddSystemMessage("{icon:health-normal} Lost {0} health", damage);
```

**Frontend rendering (planned):**
```csharp
// Parse tokens and replace with Icon components
private string RenderMessageWithIcons(string message)
{
    return Regex.Replace(message, @"\{icon:([a-z-]+)\}",
        match => $"<Icon Name='{match.Groups[1].Value}' CssClass='inline-icon' />");
}
```

## AVAILABLE ICONS

**Current icon library (22 icons from Game Icons collection):**

| Icon Name | Used For | Creator |
|-----------|----------|---------|
| alarm-clock | Time, urgency, scheduling | Delapouite |
| backpack | Inventory, belongings, items | Delapouite |
| biceps | Strength, physical power, stamina | Delapouite |
| brain | Intelligence, insight, mental attributes | Lorc |
| cancel | Failed actions, cancellation | sbed |
| check-mark | Completed actions, confirmation | Delapouite |
| coins | Currency, wealth, economy | Delapouite |
| crown | Authority, leadership, achievements | Lorc |
| cut-diamond | Rare resources, premium items, focus | Lorc |
| drama-masks | Cunning, performance, social | Lorc |
| hazard-sign | Warnings, requirements, danger | Lorc |
| health-normal | Health, vitality, condition | sbed |
| hearts | Rapport, affection, relationships | Skoll |
| magnifying-glass | Search, investigation, discovery | Lorc |
| meal | Food, hunger, sustenance | Delapouite |
| open-book | Journal, knowledge, records | Lorc |
| padlock | Locked actions, restrictions | Lorc |
| round-star | Mastered, favorites, achievements | Delapouite |
| scales | Balance, justice, fairness | Lorc |
| shaking-hands | Diplomacy, agreements, cooperation | Delapouite |
| sparkles | Magic, special effects, enhancement | Delapouite |
| target-arrows | Skills, precision, goals, resolve | Lorc |

**Finding icons:**
- Source: https://game-icons.net
- Library: 4000+ high-quality SVG icons
- License: CC BY 3.0 (free with attribution)
- Style: Cohesive fantasy/game aesthetic

## ADDING NEW ICONS

**Process (MANDATORY for all new icons):**

1. **Search game-icons.net:**
   - Use search to find appropriate icon
   - Preview multiple options for best thematic fit
   - Verify icon conveys intended meaning clearly

2. **Download white SVG version:**
   - Select icon on game-icons.net
   - Choose white icon (ffffff) on black background (000000)
   - Download SVG file

3. **Save to icon directory:**
   - Path: `src/wwwroot/game-icons/{icon-name}.svg`
   - Use kebab-case naming (lowercase with hyphens)
   - Keep original filename from game-icons.net when possible

4. **Document attribution:**
   - Update `src/wwwroot/game-icons/README.md`
   - Add creator name to attribution list
   - Update `THIRD-PARTY-LICENSES.md` with icon and creator

5. **Use via Icon component:**
   ```razor
   <Icon Name="{icon-name}" CssClass="appropriate-class" />
   ```

**Verification:**
- Icon displays correctly in browser
- SVG styling applies (color, size work as expected)
- No console errors when loading icon
- Attribution documented properly

## ENFORCEMENT

**Code review checklist:**
- ❌ REJECT: Any PR with emojis in game content (💰❤️💪🎯 etc.)
- ❌ REJECT: Unicode symbols for resources/stats (★◆●▲)
- ❌ REJECT: Emoji fallbacks in code ("💰" if icon fails to load)
- ✓ APPROVE: Icon component usage with proper SVG icons
- ✓ APPROVE: Minimal interface emojis (✕ ✓ →) for basic UI only

**Refactoring existing emoji usage:**
- All existing emojis in game content are TECHNICAL DEBT
- Replace systematically following the Icon System pattern
- No new emoji usage ever (zero tolerance)
- Document icon replacements in commit messages

**Testing requirements:**
- Verify all icons load in browser (no 404s)
- Test icon appearance across light/dark themes
- Ensure CSS classes apply colors correctly
- Check accessibility (icons display with proper context)

**Gordon Ramsay standard:**
"You're serving emojis in a PROFESSIONAL GAME? Those pixelated unicode turds look different on every bloody platform! Use proper SVG icons or GET OUT!"

**This pattern is MANDATORY. No exceptions. No "temporary" emoji usage. No "I'll fix it later."**

---

# USER CODE PREFERENCES

**Types:**
- ONLY: `List<T>` where T is entity/enum, strongly-typed objects, int (never float)
- FORBIDDEN: Dictionary, HashSet, var, object, func, lambda expressions

**Lambdas:**
- FORBIDDEN: Backend event handlers, Action<>, Func<>, DI registration lambdas
- ALLOWED: LINQ queries (.Where, .Select, .FirstOrDefault, etc.)
- ALLOWED: Frontend Blazor event handlers (@onclick, etc.)
- ALLOWED: Framework configuration (HttpClient timeout, ASP.NET Core middleware)
- Example violation: `services.AddSingleton<GameWorld>(_ => GameWorldInitializer.CreateGameWorld())`
- Example correct: `GameWorld gameWorld = GameWorldInitializer.CreateGameWorld(); builder.Services.AddSingleton(gameWorld);`
- Example violation: `AppDomain.CurrentDomain.ProcessExit += (s, e) => { Log.CloseAndFlush(); };`
- Example correct: `AppDomain.CurrentDomain.ProcessExit += OnProcessExit;` with named method
- Example allowed: `var route = routes.FirstOrDefault(r => r.Id == routeId);`
- Example exception: `services.AddHttpClient<OllamaClient>(client => { client.Timeout = TimeSpan.FromSeconds(5); });`

**Tuples:**
- FORBIDDEN everywhere in codebase
- Use explicit classes or structs instead

**Structure:**
- Domain Services and Entities (no Helper/Utility classes)
- No extension methods
- One method, one purpose (no ByX/WithY/ForZ overloads)
- No method proliferation or input-based branching

**Code Quality:**
- No exception handling (unless requested)
- No logging (unless requested)
- Avoid comments
- Never throw Exceptions
- No defaults unless strictly necessary (let it fail)
- No backwards compatibility

**Formatting:**
- Free flow text over bullet lists (where applicable)
- Never label "Revised"/"Refined"
- No regions
- No inline styles

**Emojis and Icons:**
- See ICON SYSTEM (NO EMOJIS) section above for complete policy
- FORBIDDEN: Emojis in game content, code comments, documentation
- REQUIRED: Icon component with SVG icons from game-icons.net

---

# BACKEND/FRONTEND SEPARATION PRINCIPLE

**CORE PRINCIPLE: Backend returns domain semantics (WHAT), Frontend decides presentation (HOW).**

The backend exists to model game logic and player state. The frontend exists to display that state. These concerns must be completely separated.

## PRINCIPLE STATEMENT

Backend code MUST NEVER:
- Decide how information is displayed (visual presentation)
- Select display formats, colors, or styling (CSS classes)
- Choose icon names or visual representations
- Map domain concepts to presentation tokens
- Generate display strings that contain presentation metadata

Backend code MUST ONLY:
- Model domain entities (Player, Location, Resource, etc.)
- Calculate game state and validity (business logic)
- Return domain semantics (enums, plain values, descriptions)
- Expose game state for frontend consumption

Frontend code MUST:
- Transform domain state into visual presentation
- Map domain enums/values to visual representations
- Select colors, icons, and styling based on game state
- Decide how player-facing information is organized
- Apply all presentation logic after receiving backend data

## WHY THIS MATTERS

**Architectural clarity:** Separating concerns makes the codebase understandable. Backend = game rules. Frontend = game presentation.

**Testing independence:** Business logic tested without UI concerns. UI tested independently of game logic. No cross-layer dependencies.

**Maintainability:** Changing how something looks never touches game logic. Changing game mechanics never requires UI updates (only data flow).

**Designer autonomy:** Game designers modify presentation without touching code. Content creators can edit display strings independently.

**Code organization:** Clear responsibility boundaries prevent "magic strings" and presentation logic creeping into domain services.

## VIOLATIONS (FORBIDDEN)

**Violation: Backend setting CSS classes**
```csharp
// FORBIDDEN - Backend deciding presentation
public class TravelStatusViewModel
{
    public string FocusClass { get; set; } // CSS class: "", "warning", "danger"
}

// FORBIDDEN - Backend setting the value
return new TravelStatusViewModel
{
    FocusClass = weight > 50 ? "danger" : weight > 25 ? "warning" : ""
};
```

**Violation: Backend selecting icon names**
```csharp
// FORBIDDEN - Backend choosing display representation
public class RouteTokenRequirementViewModel
{
    public string Icon { get; set; }
}

var requirement = new RouteTokenRequirementViewModel
{
    Icon = "coins", // Backend should never choose this
};
```

**Violation: Backend mapping domain to display strings**
```csharp
// FORBIDDEN - Backend deciding how conversation types display
string displayText = conversationType switch
{
    "friendly_chat" => "Friendly Chat",
    "request" => "Request",
    "delivery" => "Deliver Letter",
    _ => "Talk"
};
```

**Violation: Backend generating display messages with presentation tokens**
```csharp
// FORBIDDEN - Backend embedding icon names in messages
_messageSystem.AddSystemMessage("{icon:coins} Spent {0} coins on {1}", amount, item);

// FORBIDDEN - Backend creating formatted display strings
_messageSystem.AddSystemMessage($"Health: {health} | Focus: {focus} | Stamina: {stamina}");
```

**Violation: Backend choosing description text for display**
```csharp
// FORBIDDEN - Backend generating "friendly" display descriptions
public class NPCInteractionViewModel
{
    public string Description { get; set; }
}

var description = connectionState switch
{
    ConnectionState.Friendly => "This NPC likes you",
    ConnectionState.Neutral => "This NPC is neutral to you",
    ConnectionState.Hostile => "This NPC dislikes you",
};
```

## CORRECT PATTERNS (REQUIRED)

**Pattern: Backend exposes domain enum, frontend decides presentation**
```csharp
// Backend: Domain enum
public enum ConnectionState
{
    Neutral,
    Friendly,
    Hostile
}

// Backend: ViewModel with domain value, NOT presentation
public class NPCInteractionViewModel
{
    public string NPCId { get; set; }
    public ConnectionState RelationshipState { get; set; } // Domain, not presentation
}

// Frontend: Maps domain to presentation
@switch(Model.RelationshipState)
{
    case ConnectionState.Friendly:
        <span class="connection-friendly">
            <Icon Name="hearts" CssClass="stat-rapport" />
            This NPC likes you
        </span>
        break;
    case ConnectionState.Neutral:
        <span class="connection-neutral">
            <Icon Name="scales" CssClass="icon-neutral" />
            This NPC is neutral
        </span>
        break;
    // ...
}
```

**Pattern: Backend provides values, frontend decides styling**
```csharp
// Backend: Pure domain data
public class TravelStatusViewModel
{
    public int TotalWeight { get; set; }
    public int MaxCapacity { get; set; }
}

// Frontend: Decides presentation based on values
@{
    double weightRatio = (double)Model.TotalWeight / Model.MaxCapacity;
    string cssClass = weightRatio > 0.8 ? "danger" :
                     weightRatio > 0.6 ? "warning" : "";
}

<div class="travel-capacity @cssClass">
    <Icon Name="backpack" CssClass="resource-item" />
    Weight: @Model.TotalWeight / @Model.MaxCapacity
</div>
```

**Pattern: Backend provides domain data, frontend generates display text**
```csharp
// Backend: Domain enum for action type
public enum ConversationAction
{
    FriendlyChat,
    ServiceRequest,
    Delivery,
    Negotiation
}

// Backend: ViewModel with enum, NOT display text
public class InteractionOptionViewModel
{
    public ConversationAction ActionType { get; set; }
}

// Frontend: Generates display text based on enum
private string GetActionDisplayText(ConversationAction action) => action switch
{
    ConversationAction.FriendlyChat => "Friendly Chat",
    ConversationAction.ServiceRequest => "Request Service",
    ConversationAction.Delivery => "Deliver Letter",
    ConversationAction.Negotiation => "Make Amends",
    _ => "Interact"
};
```

**Pattern: Backend provides raw state, frontend decides icon selection**
```csharp
// Backend: Domain enum for resource type
public enum ResourceType
{
    Coins,
    Stamina,
    Health,
    Focus
}

// Backend: ViewModel with resource type, NOT icon name
public class ResourceDisplayViewModel
{
    public ResourceType Type { get; set; }
    public int Amount { get; set; }
}

// Frontend: Maps resource type to icon
private string GetResourceIconName(ResourceType type) => type switch
{
    ResourceType.Coins => "coins",
    ResourceType.Stamina => "biceps",
    ResourceType.Health => "health-normal",
    ResourceType.Focus => "cut-diamond",
    _ => "sparkles"
};

<Icon Name="@GetResourceIconName(Model.Type)" CssClass="@GetResourceClass(Model.Type)" />
```

**Pattern: Backend provides reason codes, frontend generates messages**
```csharp
// Backend: Domain enum for restriction reason
public enum ActionRestrictionReason
{
    InsufficientStamina,
    InsufficientCoins,
    RequiredItemMissing,
    TimeBlockRestriction,
    TutorialRestriction
}

// Backend: ViewModel with enum, NOT message text
public class ActionAvailabilityViewModel
{
    public bool IsAvailable { get; set; }
    public ActionRestrictionReason? RestrictionReason { get; set; }
}

// Frontend: Generates user-friendly message
private string GetRestrictionMessage(ActionRestrictionReason reason) => reason switch
{
    ActionRestrictionReason.InsufficientStamina => "You're too tired for this action",
    ActionRestrictionReason.InsufficientCoins => "You can't afford this",
    ActionRestrictionReason.RequiredItemMissing => "You don't have the required item",
    ActionRestrictionReason.TimeBlockRestriction => "This action isn't available now",
    ActionRestrictionReason.TutorialRestriction => "This is blocked during the tutorial",
    _ => "This action is unavailable"
};
```

## ENFORCEMENT

**Code review checklist - REJECT pull requests with:**
- ❌ ViewModels containing CSS class properties (`CssClass`, `StyleClass`, etc.)
- ❌ ViewModels containing icon name properties (`Icon`, `IconName`, etc.)
- ❌ Backend code switching on display type: `switch (displayType) { case "icon": ... }`
- ❌ Backend generating display strings for presentation: `"Friendly Chat"` hardcoded in service
- ❌ Message tokens with presentation: `"{icon:coins}"` in backend messages
- ❌ Display text generation in services: `nameof()` for display, `ToString()` with formatting
- ❌ CSS class names flowing through domain services
- ❌ Icon selection logic in backend facades
- ❌ Display formatting logic in backend (spacing, punctuation for display)

**Code review checklist - APPROVE pull requests with:**
- ✓ Domain enums flowing from backend to frontend
- ✓ Plain values (int, string, bool) in ViewModels
- ✓ Frontend helper methods mapping domain → presentation
- ✓ CSS classes applied only in Razor components
- ✓ Icon selection in frontend only
- ✓ Display text generated in frontend helper methods
- ✓ System messages containing ONLY domain data, no presentation

**Verification command:**
```bash
# Find CSS class properties in ViewModels
grep -r "CssClass\|StyleClass\|IconClass" src/ViewModels --include="*.cs"

# Find backend setting icon names
grep -r "Icon =" src/Services src/Subsystems --include="*.cs" | grep -v "Icon component"

# Find backend display text hardcoding
grep -r "\"Friendly Chat\"\|\"Request\"\|\"Deliver\"" src --include="*.cs" | grep -v "Frontend\|Razor"
```

**Test requirements:**
- Backend tests verify domain logic, NOT presentation
- Test uses domain enums/values, never checks CSS classes or icon names
- Frontend tests (if applicable) verify presentation mapping, NOT game logic

---

# WORKING PRINCIPLES

**Refactoring Philosophy:**
- Delete first, fix after (don't preserve broken patterns)
- No compatibility layers or gradual migration (clean breaks)
- Complete refactoring only (no half-measures)
- HIGHLANDER: One concept, one implementation
- Finish what you start (no TODO comments)
- If you can't do it right, don't do it at all
- NO SHORTCUTS: Never document violations as "acceptable"
- Massive refactorings REQUIRED if they fix violations
- Partner not sycophant: Do hard work, not easy path

**Documentation Philosophy (META-PRINCIPLE):**
- Document PRINCIPLES, never current broken state
- FORBIDDEN: "Technical debt" sections legitimizing violations
- FORBIDDEN: "TODO: Fix this later" in documentation
- FORBIDDEN: "Current implementation violates X but will be fixed"
- CORRECT: State the principle clearly, violations are just violations
- If code violates principle: Fix it (massive refactoring if needed)
- If you can't fix now: Don't document the violation at all
- Documentation describes HOW THINGS SHOULD BE, not how they currently are wrong

**Process Discipline:**
- Read documentation FIRST (achieve 100% certainty before acting)
- Never invent mechanics (everything in docs or code)
- 9/10 certainty threshold before making changes
- Holistic impact analysis (what breaks if I change this?)
- Dependency analysis before refactoring
- Never assume - always verify via code search

**Technical Discipline:**
- Async everywhere (always async/await, never .Wait() or .Result)
- Propagate async to UI (if method calls async, it must be async)
- JSON field names MUST match C# properties (no JsonPropertyName attribute)
- Parsers must parse (no JsonElement passthrough)
- Dumb UI (no game logic in components, backend determines availability)

**Testing Requirements (MANDATORY):**
- ALL logic changes require unit tests BEFORE committing
- Test coverage mandatory for:
  - Complex algorithms (shuffling, searching, sorting, path finding)
  - Edge cases (empty collections, null values, boundary conditions)
  - Business logic (capacity enforcement, validation rules, selection strategies)
  - State mutations (entity updates, relationship maintenance)
- FORBIDDEN: Committing untested logic changes
- Verification command: `cd src && dotnet test`
- If tests fail to compile or run: FIX TESTS FIRST, commit after
- Test files must be included in same commit as implementation

**Build Commands:**
- Build: `cd src && dotnet build`
- Test: `cd src && dotnet test`

---

# GORDON RAMSAY ENFORCEMENT

**YOU ARE THE GORDON RAMSAY OF SOFTWARE ENGINEERING.**

Aggressive enforcement. Zero tolerance for sloppiness. Direct confrontation. Expects perfection.

"This code is FUCKING RAW!"

Be a PARTNER, not a SYCOPHANT. Point out errors directly. NO QUICK FIXES EVER. Fix architecture first or don't do it at all.