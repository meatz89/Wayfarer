# REQUIRED READING - START HERE

**Before ANY task, achieve 100% certainty. Read documentation first, never assume.**

## Essential Documents (Always Read)

| Document | Purpose | When |
|----------|---------|------|
| `arc42/12_glossary.md` | Technical terminology | Any task - understand the vocabulary |
| `gdd/08_glossary.md` | Game design terminology | Any task - understand game concepts |
| `gdd/01_vision.md` | Design pillars, Requirement Inversion, Tier Hierarchy | Any task - understand WHY things exist |

## Implementation Document

| Location | Content |
|----------|---------|
| `gdd/` subdirectory | Detailed mechanics, archetypes, balance methodology |
| `arc42/` subdirectory | Complete arc42 technical documentation |

---

# ARC42 DOCUMENTATION PHILOSOPHY

Arc42 is a **cabinet, not a form**. Use it as organized drawers for architecture knowledge, not as a template to fill completely.

## Core Principles

| Principle | Meaning |
|-----------|---------|
| **"Dare to leave gaps"** | Deliberately exclude irrelevant sections. Empty sections are fine. |
| **Stakeholder-driven** | Document what stakeholders need, nothing more. |
| **Concepts over implementation** | Describe patterns and decisions, not property names or formulas. |
| **5-15 elements** | Diagrams should contain 5-15 elements. More = wrong abstraction level. |

## What Belongs in arc42

- **WHY** decisions were made
- **WHAT** patterns and principles apply
- Architectural trade-offs and their rationale
- Crosscutting concerns that span multiple components

## What Does NOT Belong in arc42

| FORBIDDEN | WHY |
|-----------|-----|
| Code blocks | Implementation belongs in code |
| Specific property names | Code is authoritative for naming |
| Concrete numbers/formulas | These change; code is source of truth |
| Enum value lists | Implementation detail |
| Method signatures | Code documents itself |

**Reference:** [arc42 FAQ](https://leanpub.com/arc42-faq/read), [arc42 in Practice](https://leanpub.com/arc42inpractice/read)

---

# RULE #0: NO HALF MEASURES

**FORBIDDEN:**
- "Quick wins" or partial implementations
- Compatibility layers or temporary solutions
- Deferring work ("let's come back to this later")
- TODO comments in code
- Stopping at 80% because "good enough"

**MANDATORY:**
- Complete the task fully before moving on
- Understand the complete system holistically
- Take the hard but correct path
- Use agents to plan and verify before implementing
- If blocked, solve it - don't pivot to something easier

**Vertical Slices:** If task is large, break into complete slices. Each slice: JSON → DTO → Parser → Entity → Service → UI → Tests. Never do "all JSON changes" then move on.

---

# ABSOLUTE ENFORCEMENT: NO BYPASSING

**THE RULE:** You may NEVER decide on your own to ignore test failures or pre-commit hook violations. All failures must be resolved before committing.

**Non-Negotiable Requirements:**
- Every test must pass before commit
- Every pre-commit hook check must pass before commit
- "Pre-existing" violations are NOT an excuse - fix them holistically
- `--no-verify` is FORBIDDEN unless explicitly approved by the user for a specific case

**Why Pre-Existing Problems Are Your Problem:**
- If you touch a file with violations, YOU own fixing those violations
- "It was already broken" is not acceptable - you make it right
- Holistic fixing prevents violation debt from accumulating
- The codebase improves with every commit, never degrades

**What To Do When Blocked:**

| Blocker | Required Action |
|---------|-----------------|
| Test failure | Fix the test OR fix the code causing failure |
| Pre-commit hook violation | Fix ALL violations in touched files |
| False positive in hook | Ask user for explicit approval to bypass |
| Systemic violation | Fix holistically (all instances across codebase) |

**FORBIDDEN:**
- Deciding on your own that a failure is "acceptable"
- Using `--no-verify` without explicit user approval
- Claiming "it was already broken" as justification
- Partial fixes that leave some violations behind
- Moving forward with failing tests

**Correct Pattern:**
1. Run tests and hooks BEFORE declaring work complete
2. If blocked, investigate and fix the root cause
3. If false positive, explain to user and request explicit bypass approval
4. If systemic issue, fix ALL instances (use agents for thorough search)
5. Only commit when ALL checks pass

---

# DOCUMENTATION AND BOUNDARIES FIRST

**THE RULE:** Before implementing ANY feature or refactoring, update documentation and define boundaries FIRST.

**Order of Operations:**
1. **Document the principle** - Update CLAUDE.md, arc42, or gdd with the rule
2. **Add enforcement** - Pre-commit hook, CI check, or compliance test
3. **Then implement** - Code changes come LAST, after boundaries are clear

**Why:**
- Documentation forces clarity BEFORE coding
- Enforcement prevents future violations (no exceptions accumulate)
- Implementation becomes mechanical once rules are clear
- Prevents "exception creep" where temporary workarounds become permanent

**FORBIDDEN:**
- Implementing code before documenting the principle
- Adding "temporary exceptions" to documented rules
- Coding first and "documenting later" (later never comes)

**Correct Pattern:** When you find a violation:
1. Document the correct principle (no exceptions)
2. Add enforcement (pre-commit hook)
3. Fix ALL existing violations
4. Commit documentation + enforcement + fixes together

---

# CRITICAL: DUAL-TIER ACTION SYSTEM

**READ FIRST:** `DUAL_TIER_ACTION_ARCHITECTURE.md`

LocationAction is a **UNION TYPE** supporting TWO intentional patterns:

| Pattern | Discriminator | Source | Properties |
|---------|---------------|--------|------------|
| **Atmospheric** | `ChoiceTemplate == null` | LocationActionCatalog (parse-time) | Direct `Costs`, `Rewards` |
| **Scene-Based** | `ChoiceTemplate != null` | SceneFacade (query-time) | `ChoiceTemplate.CostTemplate/RewardTemplate` |

**BOTH patterns are CORRECT.** Atmospheric actions prevent soft-locks. Scene-based actions provide narrative.

**FORBIDDEN ASSUMPTIONS:**
- "ActionCosts/ActionRewards look redundant, delete them"
- "Scene-Situation architecture replaced the old system"
- "ValidateLegacyAction methods are legacy code to remove"

**Before deleting ANY action property:** Search LocationActionCatalog, check arc42/08 §8.8, verify pattern discrimination in executors.

---

# MANDATORY: READ DOCS FIRST

You MUST achieve **100% CERTAINTY** before acting. If ANY doubt exists about a concept, term, architecture, or mechanic - STOP and READ DOCUMENTATION.

**Source of Truth Hierarchy:**
1. **Code** - Ultimate ground truth (what actually runs)
2. **Documentation** - Authoritative explanation (what it means)
3. **CLAUDE.md** - Process philosophy (how to work)

**Resolution:** Code wins over docs. Docs win over CLAUDE.md.

**Uncertainty indicators (READ DOCS NOW):**
- "I think this might be..."
- "Based on similar systems..."
- "This seems like it should..."
- "Probably this entity has..."

**You are NOT allowed to assume. You MUST KNOW.**

---

# CRITICAL INVESTIGATION PROTOCOL

**BEFORE ANY WORK:**
1. Read documentation first (see Required Reading above)
2. Search codebase exhaustively (Glob/Grep for ALL references)
3. Read COMPLETE files (no partial reads)
4. Map dependencies (what breaks if I change this?)
5. Verify ALL assumptions via code search and tasks / agents

**HOLISTIC DELETION:** When changing ANY property, update ALL layers:
JSON → DTO → Parser → Entity → Service/UI

**SEMANTIC HONESTY:** Method names MUST match reality. `GetVenueById` returning `Location` is FORBIDDEN.

**PLAYABILITY OVER COMPILATION:** Before marking complete:
1. Can player REACH this from game start?
2. Are ALL actions VISIBLE and EXECUTABLE?
3. Forward progress from every state?

---

# NO ENTITY INSTANCE IDs

**THE RULE:** Domain entities have **NO ID PROPERTIES**. Use **DIRECT OBJECT REFERENCES ONLY**.

**Exception:** Template IDs are acceptable (SceneTemplate.Id, SituationTemplate.Id) because templates are immutable archetypes, not game state.

**Why:** IDs create redundancy (storing both LocationId and Location violates Single Source of Truth). IDs enable violations (composite ID generation, ID parsing, hash code abuse). IDs break procedural generation (hardcoded references vs categorical filters).

**Correct Patterns:**
- Object references for relationships: `NPC.Location` not `NPC.LocationId`
- Categorical properties for queries: `EntityResolver.FindOrCreate` with filters
- Enums for routing: Switch on `ActionType` enum, not ID strings

**FORBIDDEN:**
- ID properties on domain entities
- Storing both ID string and object reference
- ID-based lookups in parsers
- ID encoding/parsing for logic

**Details:** See `arc42/08_crosscutting_concepts.md` §8.3

---

# CATALOGUE PATTERN (PARSE-TIME TRANSLATION)

**THE RULE:** Catalogues translate categorical properties to concrete values at **PARSE-TIME ONLY**. No exceptions.

| Layer | Responsibility |
|-------|----------------|
| Content JSON | Categorical descriptions (friendly, hostile, premium) |
| Catalogue | Translation formulas (parse-time via Parser) |
| Entity | Concrete values only (integers, no categories) |

**Why:** Single formula change rebalances all content. Zero runtime overhead. AI generates balanced content without knowing game math. Enforceable via pre-commit hook.

**FORBIDDEN:**
- Runtime catalogue lookups in Services (use Parser pipeline instead)
- String-based property matching at runtime
- Direct `Catalogue.GetX()` calls outside Parsers
- `Catalogue.` patterns in `/Services/`, `/Subsystems/` directories

**Correct Pattern for Procedural Content:**
1. Build DTO with categorical properties
2. Serialize to JSON package
3. Load via PackageLoader → Parser → Catalogue (parse-time)
4. Result: Entity with concrete values

**Details:** See `arc42/08_crosscutting_concepts.md` §8.2

---

# ARCHETYPE REUSABILITY (NO TUTORIAL HARDCODING)

**THE RULE:** Every archetype must work in ANY context through categorical scaling. No tutorial-specific code paths.

**Tutorial = Context, Not Code:** Tutorial scenes use the SAME archetypes as procedural scenes. The tutorial experience emerges from categorical properties (Tier 0, Friendly NPC, Basic quality), not from special tutorial code branches.

| Context | Same Archetype Produces |
|---------|------------------------|
| Tutorial (Tier 0, Friendly, Basic) | Easy requirements, low costs, modest rewards |
| Mid-game (Tier 2, Neutral, Standard) | Medium requirements, balanced costs/rewards |
| Late-game (Tier 3, Hostile, Premium) | Hard requirements, high costs, high rewards |

**FORBIDDEN:**
- `if (AStorySequence == N)` checks in archetypes
- `if (context.IsTutorial)` branching
- Different choice structures for different contexts
- Hardcoded values that don't scale with categorical properties

**Required:**
- All scaling via categorical properties (NPCDemeanor, Quality, PowerDynamic, Tier)
- Same four-choice structure regardless of context
- Context-agnostic archetype implementations

**Why:** Archetypes must be infinitely reusable. InnLodging in tutorial AND InnLodging in late-game use the SAME code. Categorical properties create appropriate difficulty.

**Details:** See `arc42/08_crosscutting_concepts.md` §8.23, `gdd/05_content.md` §5.3

---

# CONTEXT INJECTION (SCENE GENERATION)

**THE RULE:** Scene generation receives context as input. Generation NEVER reads context from GameWorld.

**Problem solved:** Without context injection, authored tutorials can't control scene sequencing. A1 completing doesn't guarantee A2 will be Investigation—it depends on player's current state at generation time.

**Solution:** Context is always injected by the caller:

| Content Type | Context Source |
|--------------|----------------|
| **Authored (A1-A10)** | Author specifies exact context in spawn reward |
| **Procedural (A11+)** | System reads GameWorld state, passes as context |

Same generation code handles both. HIGHLANDER compliance achieved through context injection, not conditional branching.

**FORBIDDEN:**
- `_gameWorld.GetPlayer()` inside generation methods
- `if (isAuthored) ... else ...` branching
- Context discovery at generation time
- Different code paths for tutorial vs procedural

**Required:**
- Context passed as parameter to generation methods
- Generation is pure function: same context → same output
- Caller (authored content or procedural system) constructs context
- Context carried in spawn request DTO

**Details:** See `arc42/08_crosscutting_concepts.md` §8.28, `gdd/06_balance.md` §6.8

---

# FAIL-FAST PHILOSOPHY

**THE RULE:** Missing data should fail loudly, not silently default.

**Why:** Null coalescing (`??`) and TryGetValue hide content authoring errors. Invalid state becomes player-visible bugs instead of parse-time crashes. Silent defaults make debugging impossible.

**FORBIDDEN:**
- `??` null coalescing in domain logic (hides missing required data)
- `TryGetValue` patterns in domain code (use direct access, let it throw)
- `TryParse` patterns (data should be valid by design)

**Correct Pattern:** Let exceptions propagate. Parse-time crashes are features, not bugs.

**Details:** See `arc42/08_crosscutting_concepts.md` §8.5

---

# EXPLICIT PROPERTY PRINCIPLE

Use explicit strongly-typed properties for state modifications. Never route changes through string-based generic systems.

**Why:** String property names fail silently at runtime. Strongly-typed properties catch errors at compile time.

**Correct:** `LocationsToUnlock`, `LocationsToLock` as explicit properties.
**Forbidden:** Generic `ModifyProperty(string name, object value)` patterns.

**Details:** See `arc42/08_crosscutting_concepts.md` §8.19

---

# GLOBAL NAMESPACE PRINCIPLE

**Policy:** No namespace declarations for domain code. Global namespace exposes conflicts immediately.

| Code Type | Namespace |
|-----------|-----------|
| Domain (GameState, Services, Subsystems) | None (global) |
| Blazor components | `Wayfarer.Pages.Components` (framework requirement) |
| Tests | `Wayfarer.Tests` (convention) |

**Why:** Custom namespaces hide duplicate classes. Directory structure already organizes code. Global namespace forces compiler to detect conflicts.

---

# DOMAIN COLLECTION PRINCIPLE

**Rule:** Dictionary, KeyValuePair, and wrapper classes are FORBIDDEN. Use explicit properties on entities.

**Why:**
- Dictionary patterns hide semantic meaning (what IS the key? what IS the value?)
- KeyValuePair iteration obscures domain concepts behind generic `.Key`/`.Value` accessors
- Wrapper classes (Entry classes on entities) are Dictionary patterns in disguise - they still require iteration/filtering to access values

**CRITICAL: Wrapper Classes Are The Same Antipattern**

A `List<TimeBlockEntry>` that you search with `.FirstOrDefault(e => e.TimeBlock == TimeBlocks.Morning)` is functionally identical to `Dictionary<TimeBlocks, List<Actions>>`. BOTH require iteration to find values. BOTH hide semantic meaning.

| Pattern | Problem | Solution |
|---------|---------|----------|
| `List<ConnectionTypeTokenEntry>` | Must iterate to find Trust | Explicit `TrustTokens`, `DiplomacyTokens`, etc. |
| `List<TimeBlockProfessionsEntry>` | Must iterate to find Morning | Explicit `MorningProfessions`, `MiddayProfessions`, etc. |
| `List<StatThresholdEntry>` | Must iterate to find Insight | Explicit `InsightThreshold`, `RapportThreshold`, etc. |

**Correct Pattern for Fixed Enums:**
When the "key" is a FIXED ENUM (TimeBlocks, ConnectionType, PlayerStatType), use EXPLICIT PROPERTIES on the entity. Access by property name, not by iteration.

**Correct Pattern for Variable Collections:**
When the collection contains truly VARIABLE items (NPCs, Locations, Items), use `List<T>` with object references and LINQ queries. The key difference: variable collections don't have predetermined keys.

**The Principle at Each Layer:**

| Layer | Forbidden | Required |
|-------|-----------|----------|
| **JSON** | `{ "Insight": 5, "Rapport": 3 }` | `[ { "stat": "Insight", "value": 5 }, ... ]` |
| **DTO** | `Dictionary<string, int>` | `List<StatRequirementDTO>` |
| **Parser** | `foreach (KeyValuePair kvp ...)` | Direct mapping via switch on enum |
| **Entity** | `List<WrapperEntry>` for fixed enums | Explicit properties per enum value |

**FORBIDDEN:**
- Dictionary, HashSet at any layer
- KeyValuePair iteration
- Wrapper/Entry classes for fixed enums on entities
- Files like CollectionEntries.cs containing wrapper class definitions

**Exception:** Dictionary acceptable ONLY for:
- Blazor framework parameters (required by framework)
- Never for game state, content, or caching

**NO CACHING PRINCIPLE:** Caching is FORBIDDEN anywhere in the game. All data should be read fresh. Templates, configurations, and state must never be cached. This ensures determinism, simplicity, and debuggability.

**Details:** See `arc42/08_crosscutting_concepts.md` §8.29

---

# HOLISTIC DATA LAYER CHANGES

**THE RULE:** When modifying JSON, DTO, or Parser - you MUST modify ALL THREE together. They are a single conceptual unit.

**The Data Flow Contract:**
```
JSON → DTO → Parser → Entity
```

These layers MUST match. Changing one without the others creates:
- Parse-time crashes (JSON structure doesn't match DTO)
- Type mismatches (DTO property type doesn't match parser expectation)
- Silent data loss (parser ignores unrecognized JSON fields)

**MANDATORY Checklist for Any Data Change:**

| Step | Action | Verify |
|------|--------|--------|
| 1 | Change JSON structure | Valid JSON, correct property names |
| 2 | Change DTO to match | Same property names and types as JSON |
| 3 | Change Parser to match | Reads DTO correctly, produces correct entity |
| 4 | Run parser tests | All data flows through without errors |

**FORBIDDEN:**
- Changing JSON without updating DTO
- Changing DTO without updating JSON AND parser
- Changing parser without verifying JSON and DTO match
- "I'll fix the other layers later" - NO, fix NOW

**This is a Vertical Slice:** JSON + DTO + Parser = ONE unit of change. Never commit partial changes.

---

# NO PARTIAL CLASSES

**THE RULE:** The `partial class` keyword is FORBIDDEN. No exceptions except Blazor.

**Why:**
- Partial classes split files, not responsibilities
- They hide complexity instead of reducing it
- No clear boundaries between concerns
- Testing remains difficult (one giant class)

**FORBIDDEN:**
- `partial class` keyword in domain/service files
- Splitting one class across multiple files
- "Organizing" large files into partials

**Exception:** Blazor code-behind files (`.razor.cs`) require `partial class` because the `.razor` file generates the other partial. This is a framework requirement, not a design choice.

**Enforcement:** Pre-commit hook blocks `partial class` patterns (except `.razor.cs` files).

---

# COMPOSITION OVER INHERITANCE

**THE RULE:** Prefer has-a relationships (composition) over is-a relationships (inheritance).

**Why:**
- Inheritance creates tight coupling between base and derived
- Composition allows runtime flexibility (swap implementations)
- Smaller, focused classes are easier to test
- Avoids fragile base class problem

**Correct Pattern:**

| Problem | Wrong (Inheritance) | Right (Composition) |
|---------|---------------------|---------------------|
| File too large | Split into partial files | Extract to composed services |
| Shared behavior | Base class with virtual methods | Inject shared service |
| Multiple responsibilities | Deep inheritance hierarchy | Multiple injected services |
| Debug methods bloating class | Inherit from DebugBase | Inject DebugService |

**When file exceeds 1000 lines:**
1. Identify cohesive method groups (seams)
2. Create new service class for each seam
3. Inject new service via constructor
4. Delegate to composed service
5. Original class becomes thin orchestrator

**FORBIDDEN:**
- Deep inheritance hierarchies (prefer flat + composition)
- Base classes just to share code (use composition)
- `protected` methods for subclass access (inject dependency instead)

---

# FACADE ISOLATION (NO LATERAL DEPENDENCIES)

**THE RULE:** Facade/subsystem classes may NEVER reference or call other facades/subsystems directly. Only GameOrchestrator can orchestrate between facades.

**Architecture:**

| Class Type | Can Call |
|------------|----------|
| GameOrchestrator | Any facade, any service |
| Facade (e.g., TravelFacade, SocialFacade) | Own domain services, GameWorld, never other facades |
| Service | Domain entities, never facades |
| UI Component | GameOrchestrator only |

**Why:**
- Prevents circular dependencies between subsystems
- Single point of coordination (GameOrchestrator)
- Clear responsibility boundaries per facade
- Testable isolation (mock GameOrchestrator, test facade in isolation)

**FORBIDDEN:**
- `TravelFacade` calling `ResourceFacade` directly
- `SocialFacade` injecting `LocationFacade`
- Any `*Facade` depending on another `*Facade`

**Correct Pattern:** When facade A needs facade B's functionality:
1. GameOrchestrator coordinates the call
2. GameOrchestrator calls A, gets result
3. GameOrchestrator calls B with result
4. GameOrchestrator returns combined result to UI

**Enforcement:** Pre-commit hook blocks facade classes that reference other facades.

---

# ICON SYSTEM (NO EMOJIS)

**Policy:**
- **FORBIDDEN:** Emojis for game content, code comments, documentation
- **REQUIRED:** SVG icons from game-icons.net via `<Icon>` component

**Usage:**
```razor
<Icon Name="coins" CssClass="resource-coin" />
<Icon Name="brain" CssClass="stat-insight" />
```

**Parameters:** Name (required), Size (default "16px"), Color (default "currentColor"), CssClass (optional)

**CSS Classes:** Stats: `stat-insight`, `stat-rapport`, `stat-authority`, `stat-diplomacy`, `stat-cunning`. Resources: `resource-coin`, `resource-health`, `resource-stamina`.

**Adding icons:** Download white SVG from game-icons.net → Save to `src/wwwroot/game-icons/` → Update attribution in `THIRD-PARTY-LICENSES.md`

---

# USER CODE PREFERENCES

**Types (DDR-007 Enforced):**
- ONLY: `List<T>`, strongly-typed objects, `int` (see GDD DDR-007: Intentional Numeric Design)
- FORBIDDEN: `Dictionary`, `HashSet`, `var`, `object`, `Func`, `Action`, tuples, `float`, `double`
- FORBIDDEN: Decimal multipliers (`* 0.X`, `* 1.X`), percentage calculations (`* 100 /`, `/ 100`), basis points
- Transform percentages to flat adjustments or integer division (see `arc42/08_crosscutting_concepts.md` §8.24)
- Enforcement: DDR007ComplianceTests.cs (CI), pre-commit hook (`scripts/hooks/install.sh`)

**Lambdas:**
- FORBIDDEN: Custom backend event handlers, DI registration code
- ALLOWED: LINQ queries, Blazor event handlers, framework extension methods (IServiceCollection)

**Structure:**
- Domain Services and Entities only (no Helper/Utility classes)
- No extension methods
- One method, one purpose

**Code Quality:**
- No exception handling (unless requested)
- No logging (unless requested)
- No comments (code should be self-documenting)
- No backwards compatibility layers

---

# BACKEND/FRONTEND SEPARATION

**Principle:** Backend returns domain semantics (WHAT). Frontend decides presentation (HOW).

**Backend provides:** Domain enums, plain values, state validity
**Frontend decides:** CSS classes, icons, display text, formatting

**FORBIDDEN in Backend:**
- CssClass properties in ViewModels
- IconName properties in ViewModels
- Display string generation in services

**Details:** See `arc42/08_crosscutting_concepts.md` §8.6

---

# WORKING PRINCIPLES

**Refactoring:**
- Delete first, fix after (don't preserve broken patterns)
- No compatibility layers (clean breaks only)
- HIGHLANDER: One concept, one implementation
- Finish what you start (no TODO comments)

**Contract Boundaries First:** When refactoring across system boundaries:
1. INPUT: Fix JSON/external data first
2. PARSER: DTOs match JSON, output domain objects
3. API: Facades accept objects, not strings
4. OUTPUT: ViewModels contain objects
5. INTERNAL: Services adapt to boundaries

**Technical Discipline:**
- Async everywhere (never `.Wait()` or `.Result`)
- JSON field names MUST match C# properties
- Parsers must parse (no JsonElement passthrough)
- Dumb UI (no game logic in components)

**Testing:**
- ALL logic changes require unit tests BEFORE committing
- Build: `cd src && dotnet build`
- Test: `cd src && dotnet test`

---

# FILE SIZE LIMIT (1000 LINES MAX)

**THE RULE:** No source file may exceed 1000 lines. Files over this threshold are a code smell indicating the need for holistic refactoring.

**Why:** Large files indicate:
- Too many responsibilities in one place (violates Single Responsibility)
- Insufficient abstraction (concepts are tangled, not separated)
- Cognitive overload (developers cannot hold the file in memory)
- Refactoring debt accumulating silently

**When a violation is detected:**

| Step | Action |
|------|--------|
| 1 | **STOP** - Do not commit, do not proceed with tactical fixes |
| 2 | **PLAN HOLISTICALLY** - Use agents to analyze the entire file and its dependencies |
| 3 | **DEBATE ALTERNATIVES** - Consider multiple refactoring strategies before choosing |
| 4 | **IMPLEMENT COMPLETELY** - Refactor in vertical slices, never leave partial work |

**FORBIDDEN:**
- "Quick split" into arbitrary files (tactical)
- Adding more code to an already-large file
- Bypassing the check without a refactoring plan
- Partial refactoring ("I'll finish this later")

**Holistic Refactoring Requirements:**
1. Understand the file's complete responsibility graph
2. Identify natural seams (distinct concepts, cohesive groups)
3. Plan the target structure BEFORE any code changes
4. Execute as vertical slices (each slice compiles and works)
5. Verify no regressions after each slice

**Enforcement:** Pre-commit hook blocks commits with files exceeding 1000 lines.

**Exceptions:**
- `GameOrchestrator.cs` - Coordinates ALL facades per FACADE ISOLATION principle. Extracting orchestration logic would either duplicate responsibility (executors already exist) or violate FACADE ISOLATION (new classes calling facades directly).
- `GameWorld.cs` - Single source of truth for ALL game state. As the central state container, it inherently requires many properties (entity collections) and methods (state queries). Extracting would create artificial seams and split state ownership.

---

# PRE-COMMIT HOOK

Pre-commit hooks are auto-installed at session start. They enforce the principles documented in CLAUDE.md by blocking commits with violations.

**Bypass:** `git commit --no-verify` (NOT RECOMMENDED - violations will fail CI)

The hook is self-documenting - run it to see what it checks. Source: `scripts/hooks/pre-commit`

---

# CLAUDE CODE HOOKS

**Location:** `.claude/settings.json` and `.claude/hooks/`

| Hook | Trigger | Purpose |
|------|---------|---------|
| SessionStart | Session begins | Ensure documentation context before work |
| Stop | Before stopping | Prevent premature stops |
| PostToolUse | After Edit/Write on arc42/*.md or gdd/*.md | Validate document structure |

**Philosophy:** Hooks remind, documentation governs. Hooks point to CLAUDE.md - they don't duplicate it.

**Division of Labor:**
- **Pre-commit hooks + CI**: Enforce compliance (scan for violations)
- **Claude Code hooks**: Remind about documentation and intent
- **Your job**: Understand principles through READING, not scanning

**DO NOT** scan the codebase for violations - pre-commit handles that. Use agents to gather context from documentation, not grep searches.

---

# DOCUMENTATION STANDARDS

## arc42 Documents

arc42 is a structured cabinet for architecture documentation. "Dare to leave gaps" - only document what matters.

| Principle | Rule |
|-----------|------|
| **Structure** | Pattern/principle tables, not prose paragraphs |
| **Content** | WHAT and WHY, never HOW |
| **Format** | "**Consequences:**" and "**Forbidden:**" sections |
| **Brevity** | Remove irrelevant sections |

**Anti-patterns:** Code blocks, concrete numbers, file paths, enum value lists, redundant content.

**Reference:** [arc42 Documentation](https://docs.arc42.org/), [arc42 Template](https://arc42.org/overview)

## GDD Documents

Game design documents describe the GAME EXPERIENCE, not implementation.

| Principle | Rule |
|-----------|------|
| **Structure** | "Why" and "How it manifests" for pillars |
| **Content** | Design intent and player experience |
| **Traceability** | Every feature traces to a design pillar |
| **Separation** | Technical details belong in arc42 or code |

**Exception:** `BASELINE_ECONOMY.md` may contain concrete balance values.

**Anti-patterns:** Code blocks, implementation details, file references, JSON structures.
