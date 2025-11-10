# CRITICAL INVESTIGATION PROTOCOL

**BEFORE ANY WORK:**
1. Search codebase exhaustively (Glob/Grep for ALL references)
2. Read COMPLETE files (no partial reads unless truly massive)
3. Understand architecture (GameWorld, screens, CSS, domain entities, services, data flow)
4. Verify ALL assumptions (search before claiming exists/doesn't exist)
5. Map dependencies (what breaks if I change this?)

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

```
Obligation (multi-phase mystery)
  ↓ spawns
Obstacles (challenges in world)
  ↓ contain
Goals (approaches to overcome obstacles)
  ↓ appear at
Locations/NPCs/Routes (placement context, NOT ownership)
  ↓ when engaged become
Challenges (Social/Mental/Physical subsystems)
  ↓ player plays
GoalCards (tactical victory conditions)
  ↓ achieve
Goal Completion → Obstacle Progress → Obligation Phase Completion
```

**Ownership Hierarchy:**
```
GameWorld (single source of truth)
 ├─ Obligations (spawn Obstacles by ID)
 ├─ Obstacles (contain Goals by ID)
 ├─ Goals (has locationId, optional npcId, inline GoalCards)
 ├─ Locations (placement context only)
 └─ NPCs (social context only)
```

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