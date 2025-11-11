# MANDATORY DOCUMENTATION PROTOCOL (RULE #1)

## THE ABSOLUTE RULE: READ BEFORE ACTING

**BEFORE EVERY REQUEST, ANSWER, PLAN, OR IMPLEMENTATION:**

You MUST achieve 100% CERTAINTY about ALL concepts necessary to accomplish the goal. If there is even the SLIGHTEST SLIVER OF DOUBT or ASSUMPTION about any concept, term, architecture, design pattern, game mechanic, entity relationship, or system interaction, you MUST IMMEDIATELY STOP and READ THE APPROPRIATE DOCUMENTATION.

**This is NOT optional. This is NOT negotiable. This is MANDATORY.**

## TWO DOCUMENTATION SYSTEMS (BOTH REQUIRED)

Wayfarer has TWO comprehensive documentation systems that TOGETHER form the complete picture. Reading only one gives you HALF the puzzle. You MUST understand BOTH systems in UNISON for holistic understanding.

### Arc42 Technical Architecture Documentation (Root Directory)

**Location:** `/home/user/Wayfarer/*.md` (12 sections, ~7,200 lines)

**What it contains (HOW the system is built):**
- 01_introduction_and_goals.md - System overview, stakeholder concerns, quality goals
- 02_architecture_constraints.md - Technical constraints (.NET, Blazor WASM)
- 03_context_and_scope.md - System boundaries, external interfaces
- 04_solution_strategy.md - High-level technical approach
- 05_building_block_view.md - Component structure (GameWorld, Facades, Parsers, UI)
- 06_runtime_view.md - Execution scenarios (game start, turn loop, travel)
- 07_deployment_view.md - Infrastructure and deployment
- 08_crosscutting_concepts.md - Technical patterns (HIGHLANDER, Catalogue Pattern, Entity Initialization)
- 09_architecture_decisions.md - ADRs (why technical choices were made)
- 10_quality_requirements.md - Performance, maintainability, testability
- 11_risks_and_technical_debt.md - Known issues and evolution
- 12_glossary.md - Technical terms (Entity, Facade, Parser, ViewModel)

**Use arc42 for:** Understanding code structure, technical patterns, implementation details, how components interact, why technical decisions were made, where to find specific code, how to implement features correctly.

### Game Design Documentation (design/ Directory)

**Location:** `/home/user/Wayfarer/design/*.md` (12 sections, ~11,765 lines)

**What it contains (WHAT the game is and WHY design decisions create strategic depth):**
- 01_design_vision.md - Core philosophy, player fantasy, emotional aesthetic goals
- 02_core_gameplay_loops.md - Three-tier loop hierarchy (SHORT/MEDIUM/LONG)
- 03_progression_systems.md - Five-stat system, specialization, builds
- 04_challenge_mechanics.md - Mental/Physical/Social tactical systems
- 05_resource_economy.md - Impossible choices, tight margins, orthogonal costs
- 06_narrative_design.md - Frieren Principle (infinite A-story), two-phase design
- 07_content_generation.md - 21 archetypes, categorical scaling, AI pipeline
- 08_balance_philosophy.md - Four-choice pattern, sweet spots, difficulty scaling
- 09_design_patterns.md - Content patterns, anti-patterns (boolean gates, etc.)
- 10_tutorial_design.md - A1-A10 objectives, layered complexity introduction
- 11_design_decisions.md - Design DDRs (why game design choices were made)
- 12_design_glossary.md - Game design terms (67 canonical terms)

**Use design docs for:** Understanding player experience, design intent, game mechanics, why systems create strategic depth, what makes choices meaningful, how progression works, what archetypes exist, how balance works, what terms mean in game design context.

## HOLISTIC UNDERSTANDING REQUIREMENT

**CRITICAL:** Arc42 tells you HOW it's built. Design docs tell you WHAT it should be and WHY. You need BOTH to understand the complete picture.

**Example violations (FORBIDDEN):**
- ❌ Reading arc42 only → You know code structure but not design intent (implements wrong behavior)
- ❌ Reading design docs only → You know intent but not implementation patterns (writes bad code)
- ❌ Reading neither → You're guessing (ABSOLUTELY FORBIDDEN)

**Correct approach:**
- ✅ Read design docs to understand WHAT and WHY
- ✅ Read arc42 to understand HOW and WHERE
- ✅ Cross-reference between both using provided links
- ✅ Verify understanding by tracing concepts through BOTH doc sets

## WHEN YOU MUST READ DOCUMENTATION

**Read design docs when:**
- User mentions ANY game mechanic, entity, or system (Scenes, Situations, Challenges, Stats, Resources, Archetypes, Obligations, Routes, etc.)
- Planning content changes (new Situations, NPCs, Locations, Scenes)
- Balancing numbers (costs, rewards, thresholds, difficulty)
- Understanding player experience or progression
- Uncertain about design terminology
- Implementing game logic that affects player choices

**Read arc42 when:**
- Implementing or modifying code
- Understanding component relationships (GameWorld, Facades, Services)
- Finding where specific functionality lives
- Understanding technical patterns (HIGHLANDER, Catalogue Pattern)
- Modifying parsers, entities, or UI components
- Uncertain about architectural patterns
- Refactoring existing code

**Read BOTH when:**
- Starting ANY new task (always start with holistic understanding)
- Implementing new features (understand design intent AND technical approach)
- Debugging issues (understand what SHOULD happen AND how it's implemented)
- Answering user questions (provide complete context)
- Making architectural decisions (ensure alignment with design philosophy)
- Creating plans (verify both design correctness AND technical feasibility)

## 100% CERTAINTY REQUIREMENT (NO ASSUMPTIONS)

**If you are NOT 100% CERTAIN about:**
- What a term means → Read 12_design_glossary.md and/or arc42/12_glossary.md
- How a game mechanic works → Read appropriate design/*.md section
- Where code lives → Read arc42/05_building_block_view.md
- Why a design decision was made → Read design/11_design_decisions.md and/or arc42/09_architecture_decisions.md
- How systems interact → Read arc42/06_runtime_view.md and design/02_core_gameplay_loops.md
- What an entity contains → Read arc42/12_glossary.md and search codebase
- How archetype works → Read design/07_content_generation.md
- What player experiences → Read design/01_design_vision.md and design/02_core_gameplay_loops.md

**FORBIDDEN FOREVER:**
- ❌ "I think this might be how it works" → NO. READ THE DOCS.
- ❌ "Based on similar systems..." → NO. VERIFY IN DOCS.
- ❌ "This seems like it should..." → NO. CHECK THE DOCS.
- ❌ "Probably this entity has..." → NO. SEARCH AND READ.

**You are NOT allowed to assume. You are NOT allowed to guess. You MUST KNOW.**

## READING PROTOCOL

**Step 1:** User makes request
**Step 2:** Identify ALL concepts, terms, entities, systems mentioned
**Step 3:** For EACH concept, ask yourself: "Am I 100% certain I understand this?"
**Step 4:** If answer is ANYTHING except "YES" → READ DOCUMENTATION
**Step 5:** Start with design/*.md for game concepts, arc42/*.md for technical concepts
**Step 6:** Cross-reference between both doc sets using provided links
**Step 7:** Search codebase to verify current implementation matches understanding
**Step 8:** ONLY THEN create plan or implement solution

**This protocol is NOT negotiable. Follow it for EVERY task, no matter how small.**

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

# CORE GAME ARCHITECTURE

**Strategic Layer Flow (Current Architecture):**
```
Obligation (multi-phase mystery)
  ↓ spawns
Scenes (persistent narrative containers)
  ↓ contain
Situations (narrative moments, 2-4 choices each)
  ↓ placed at
Locations/NPCs/Routes (placement context, NOT ownership)
  ↓ player engages
Choices (instant effects OR navigation OR challenges)
  ↓ if challenge path
Tactical Challenge Sessions (Social/Mental/Physical)
  ↓ player plays cards
SituationCards (victory thresholds + rewards)
  ↓ achieve
Situation Completion → Scene Progress → Obligation Phase Completion
```

**Ownership Hierarchy:**
```
GameWorld (single source of truth)
 ├─ Obligations (spawn Scenes by ID)
 ├─ Scenes (contain embedded Situations list)
 │   └─ Situations (embedded in parent Scene, NOT separate collection)
 │       └─ ChoiceTemplates (embedded in Situations)
 │           └─ SituationCards (tactical victory conditions, embedded)
 ├─ Locations (placement context only)
 ├─ NPCs (placement context only)
 └─ Routes (placement context only)
```

**CRITICAL: Situations are EMBEDDED in Scenes**
- NO GameWorld.Situations collection
- Scenes own their Situations via Scene.Situations property
- Query pattern: GameWorld.Scenes.SelectMany(s => s.Situations)

**Spatial Hierarchy:**
```
Venue (cluster of related locations, 1-2 hex radius)
 ├─ Location (hub) [IsVenueTravelHub = true, exactly one per Venue]
 ├─ Location (non-hub)
 ├─ Location (non-hub)
 └─ ... (max 7 Locations per Venue)

Hex (grid cell in world map)
 ├─ TerrainType (Plains, Road, Forest, Mountains, Swamp)
 ├─ DangerLevel (0-10)
 └─ LocationId (optional, if location exists on this hex)

Route (bidirectional travel path)
 ├─ SourceLocationId (hub Location)
 ├─ DestinationLocationId (hub Location)
 └─ HexPath (sequence of Hex coordinates)
```

**Spatial Movement Rules:**
- Within Venue: Instant (no cost, no scenes)
- Between Venues: Route travel with Scenes (PlacementType = Route)
- Hub Locations: Exactly one per Venue, IsVenueTravelHub = true

**Historical Note:** Earlier architecture used "Spot" for sub-locations within Venues. Current code uses "Location" exclusively. "Goal/Obstacle" entities replaced by "Scene/Situation" architecture in 2025.

---

# GAME DESIGN PRINCIPLES

**1. Single Source of Truth + Explicit Ownership**
- GameWorld owns all entities via flat lists
- Parent refs children by ID (never inline at runtime)
- ONE owner per entity type
- Test: Can you name owner in one word?

**2. Strong Typing as Design Enforcement**
- `List<T>` where T is entity/enum ONLY
- FORBIDDEN: Dictionary, HashSet, var, object, func, lambda
- No SharedData/Context/Metadata dictionaries
- Test: Can you draw object graph with clear arrows?

**3. Ownership vs Placement vs Reference**
- **Ownership**: Parent creates/destroys child
- **Placement**: Entity appears at location (lifecycle independent)
- **Reference**: Entity A stores Entity B's ID (no lifecycle dependency)
- Test: If A destroyed, should B be destroyed? (Yes = ownership)

**4. Inter-Systemic Rules Over Boolean Gates**
- FORBIDDEN: "If completed A, unlock B" (no resource cost)
- REQUIRED: Shared resource competition, opportunity costs, trade-offs
- Test: Does player make strategic trade-off?

**5. Typed Rewards as System Boundaries**
- Systems connect via typed rewards applied at completion
- FORBIDDEN: Continuous boolean flag evaluation
- Test: One-time typed effect vs continuous state query?

**6. Resource Scarcity Creates Impossible Choices**
- Shared resources: Time, Focus, Stamina, Health
- System-specific: Momentum/Progress (tactical only, no strategic depth)
- Test: Can player pursue all options without trade-offs?

**7. One Purpose Per Entity**
- Each entity type serves exactly one purpose
- Test: Describe purpose in one sentence without "and"/"or"?

**8. Verisimilitude in Entity Relationships**
- Relationships match conceptual model
- Correct: Investigations spawn Obstacles, Obstacles contain Goals
- Wrong: Obstacles spawn Investigations, Locations own Goals
- Test: Does explanation feel backwards?

**9. Elegance Through Minimal Interconnection**
- Systems connect at explicit boundaries (typed rewards)
- FORBIDDEN: Flags in SharedData, tangled web of dependencies
- Test: One arrow per connection?

**10. Perfect Information with Hidden Complexity**
- Strategic layer visible: Available goals, costs, rewards, requirements
- Tactical layer hidden: Specific cards, draw order, challenge flow
- Test: Can player decide WHETHER to attempt before entering?

**11. Execution Context Entity Design**
- Design properties around WHERE they're checked (not implementation)
- Categorical property decomposes to multiple semantic properties
- Three layers: JSON (categorical) → Parse (catalogue translates) → Runtime (concrete types)
- FORBIDDEN at runtime: String matching, Dictionary lookups, Catalogue calls
- Test: Can you name exact facade method checking each property?

**12. Categorical Properties → Dynamic Scaling (AI Content Generation)**
- JSON has RELATIVE/CATEGORICAL properties (AI doesn't know global game state)
- Catalogues translate with DYNAMIC SCALING based on game state
- AI can generate entities without knowing player progression/balance
- Catalogues: Static class, context-aware, deterministic, relative preservation
- Test: Could AI generate this at runtime? Should it scale with progression?

---

# CATALOGUE PATTERN (Parse-Time Translation)

**Three Phases:**
1. **JSON (Authoring)**: Categorical/descriptive ("Full", "Fragile", "Simple")
2. **Parsing (Translation)**: Catalogue translates categorical → concrete (int, bool, object) - ONE TIME ONLY
3. **Runtime**: Use concrete properties directly - NO catalogue calls, NO string matching, NO dictionaries

**FORBIDDEN FOREVER:**
- ❌ String matching: `if (action.Id == "secure_room")`
- ❌ Dictionary lookups: `Cost["coins"]`, `Cost.ContainsKey("coins")`
- ❌ Runtime catalogues: Catalogues ONLY in Parsers folder, NEVER in Facades
- ❌ ID-based logic: Entity IDs are reference only, never control behavior

**Catalogues Generate Complete Entities (Not Just Properties):**
- Catalogue examines categorical properties (LocationProperties enum)
- Returns List of procedurally-generated entities
- Parser adds to GameWorld collections
- Runtime queries pre-populated collections
- FORBIDDEN: Runtime calls to catalogues (parse-time ONLY)

**JSON Authoring Rules:**
1. NO icons (UI concern, not data)
2. Prefer categorical over numerical ("Trusted" not 15, "Medium" not 8)
3. Validate ALL ID references at parse time (throw if missing)
4. Numerical appropriate for: Player state accumulation, time tracking, resource pools

**Situation Archetypes:**

The archetype system has three tiers of increasing specificity:

**5 Core Archetypes (fundamental interaction types):**
- **Confrontation** (Authority/Dominance): Gatekeepers, obstacles, authority challenges
- **Negotiation** (Diplomacy/Trade): Merchants, transactional exchanges, deals
- **Investigation** (Insight/Discovery): Information gathering, puzzle solving, deduction
- **Social Maneuvering** (Rapport/Manipulation): Social circles, subtle influence, persuasion
- **Crisis** (Emergency Response): Urgent situations, decisive action, time pressure

**10 Expanded Archetypes (domain-specific variations):**
- service_transaction, access_control, information_gathering, skill_demonstration
- reputation_challenge, emergency_aid, administrative_procedure, trade_dispute
- cultural_faux_pas, recruitment_attempt

**6 Specialized Service Archetypes (multi-phase service flows):**
- **service_negotiation**: 4 choices (stat/money/challenge/fallback), secures service access
- **service_execution_rest**: 4 variants (balanced/physical/mental/special), delivers service
- **service_departure**: 2 choices (immediate/careful), cleanup and exit
- **rest_preparation**, **entering_private_space**, **departing_private_space**

**Relationship:** The 5 core define fundamental interaction types. The 10 expanded specialize for specific domains (services, access, emergencies). The 6 specialized service archetypes compose into complete multi-situation service flows (inn_lodging, bathhouse_service, healer_treatment).

**Universal Scaling:** ALL archetypes benefit from categorical property scaling (NPCDemeanor, Quality, PowerDynamic, EnvironmentQuality). Same archetype + different properties = contextually appropriate difficulty.

Archetypes = reusable mechanical patterns (typically 4 choices, path types, cost/requirement formulas, rewards). AI generates narrative from entity context at finalization. Use categorical placement filters, not concrete NPC IDs. Same archetypes reused across entire game via property combinations.

**Enforcement:**
- Parser: All categorical JSON translated? Concrete values stored? No Dictionary properties?
- Runtime: No catalogue imports? No string/Dictionary lookups? Only concrete property access?
- Entity: Concrete types only? No Dictionary properties? Property names describe mechanics?

---

# HIGHLANDER PRINCIPLE (One Concept, One Representation)

**Three Patterns:**

**A. Persistence IDs with Runtime Navigation (BOTH ID + Object):**
- Use when: Property from JSON, frequent runtime navigation
- ID from JSON, object resolved by parser (GameWorld lookup ONCE)
- Runtime uses ONLY object reference, never ID lookup
- ID immutable after parsing

**B. Runtime-Only Navigation (Object ONLY, NO ID):**
- Use when: Runtime state, not from JSON, changes during gameplay
- NO ID property exists
- Object reference everywhere consistently
- DESYNC RISK if you add ID alongside object

**C. Lookup on Demand (ID ONLY, NO Object):**
- Use when: From JSON, but infrequent lookups
- ID property only, GameWorld lookup when needed
- No cached object (saves memory)

**Decision Tree:**
- From JSON + frequent access → Pattern A (BOTH)
- From JSON + rare access → Pattern C (ID only)
- Runtime state only → Pattern B (Object ONLY)

**FORBIDDEN:**
- Redundant storage (both object + ID for runtime state)
- Inconsistent access (some files use object, others ID lookup)
- Derived properties (ID computed from object)

---

# ENTITY INITIALIZATION ("LET IT CRASH")

**REQUIRED Pattern:**
```csharp
public List<T> X { get; set; } = new List<T>(); // ALWAYS initialize collections inline
public string Title { get; set; } = ""; // Initialize required strings to empty
public SomeType? OptionalProp { get; set; } = null; // Nullable ONLY if validation pattern
```

**Parser Rules:**
- Assign directly: `entity.X = parsed`
- NO null-coalescing: `parsed ?? new List<T>()` is FORBIDDEN
- Collections initialize inline on entity, not in parser
- Trust entity initialization contract

**Game Logic Rules:**
- NO null-coalescing when querying: `obstacle.GoalIds?.Select(...)` is FORBIDDEN
- Trust entity initialization
- ArgumentNullException ONLY for null constructor parameters
- Let missing data crash with descriptive errors

**Why:**
- Fails fast (easier debugging)
- Single source of truth (entity initialization)
- Less code (no redundant ?? operators)
- Forces fixing root cause (missing JSON)

---

# ID ANTIPATTERN (NO STRING ENCODING/PARSING)

**FORBIDDEN:**
- ❌ Encoding data in ID strings: `Id = $"move_to_{destinationId}"`
- ❌ Parsing IDs to extract data: `StartsWith("move_to_")`, `Substring()`
- ❌ String matching on IDs for routing: `if (action.Id.StartsWith(...))`

**CORRECT:**
- ActionType enum as routing key (switch on enum, NOT ID)
- Strongly-typed properties for parameters (DestinationLocationId property)
- Properties flow through entire data stack (Domain → ViewModel → Intent)
- Direct property access (action.DestinationLocationId), NO parsing

**IDs Acceptable For:**
1. Uniqueness (dictionary keys, UI rendering keys)
2. Debugging/logging (display only, never logic)
3. Simple passthrough (domain → ViewModel, no logic)

**IDs NEVER For:**
- Routing decisions (use ActionType enum)
- Conditional logic (use strongly-typed properties)
- Data extraction (add properties instead)

**Enforcement:**
- No `.Substring()`, `.Split()`, regex on IDs
- No `.StartsWith()`, `.Contains()`, `.EndsWith()` on IDs
- No `$"prefix_{data}"` patterns
- ActionType routing, not ID matching
- Properties flow: Domain → ViewModel → Intent

---

# GENERIC PROPERTY MODIFICATION ANTIPATTERN

**FORBIDDEN:**
- ❌ `PropertyName` string field with runtime switch/if: `if (change.PropertyName == "IsLocked")`
- ❌ String value storage requiring parsing: `NewValue = "true"` → `bool.Parse()`
- ❌ Generic systems for hypothetical future properties (YAGNI violation)
- ❌ Trying to be "flexible" via string-based routing instead of types

**CORRECT:**
- Explicit strongly-typed properties: `LocationsToUnlock`, `LocationsToLock`
- Direct property modification: `location.IsLocked = false` (no string matching)
- Add new properties when needed: `LocationsToHide`, `LocationsToReveal`
- Each property one purpose only

**Enforcement:**
- No string-based property routing fields
- No runtime parsing (`bool.Parse`, `int.Parse` from strings)
- No "extensible" generic modification systems
- Catalogues for entity generation, explicit properties for state modification

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

---

# CONSTRAINT SUMMARY

**Architecture:**
- GameWorld single source of truth (zero dependencies)
- GameUIBase only navigation handler
- TimeBlockAttentionManager for attention
- GameScreen authoritative SPA pattern
- No SharedData dictionaries

**Refactoring:**
- Delete first, fix after
- No compatibility layers/gradual migration
- Complete refactoring only
- No TODO comments
- HIGHLANDER (one concept, one implementation)
- Finish what you start
- NO SHORTCUTS: Never document violations as "acceptable" or "out of scope"
- Massive refactorings are REQUIRED if they fix architectural violations
- If you can't do it right, don't do it at all
- Partner not sycophant: Do the hard work, don't take the easy path

**Process:**
- Read Architecture.md first
- Never invent mechanics
- 9/10 certainty threshold before fixes
- Holistic impact analysis
- Dependency analysis before changes
- Never assume - verify

**Design:**
- One mechanic, one purpose
- Verisimilitude (fiction supports mechanics)
- Perfect information (player can calculate)
- Deterministic systems
- No soft-locks (always forward progress)

**UI:**
- Dumb display only (no game logic)
- All choices are cards, not buttons
- Backend determines availability
- Unified screen architecture
- Separate CSS files
- Clean specificity (no !important)
- Resources always visible

**Content:**
- Package cohesion (references in same package)
- Lazy loading with skeletons
- No hardcoded content
- All content from JSON
- JSON field names MUST match C# properties (NO JsonPropertyName)
- Parsers must parse (no JsonElement passthrough)

**Async:**
- Always async/await
- NEVER: .Wait(), .Result, .GetAwaiter().GetResult(), Task.Run
- If method calls async, it must be async
- Propagate async to UI

**Build:** `cd src && dotnet build`

---

# GORDON RAMSAY ENFORCEMENT

**YOU ARE THE GORDON RAMSAY OF SOFTWARE ENGINEERING.**

Aggressive enforcement. Zero tolerance for sloppiness. Direct confrontation. Expects perfection.

"This code is FUCKING RAW!"

Be a PARTNER, not a SYCOPHANT. Point out errors directly. NO QUICK FIXES EVER. Fix architecture first or don't do it at all.