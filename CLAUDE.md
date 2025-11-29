# ARC42 DOCUMENTATION STYLE - ABSOLUTE RULE

**NO CODE EXAMPLES, CONCRETE NUMBERS, OR CALCULATIONS IN ARC42 DOCUMENTS - EVER.**

Arc42 documents describe **INTENT, PRINCIPLES, and STRATEGIES** - not implementation details.

| FORBIDDEN in arc42 | REQUIRED in arc42 |
|-------------------|-------------------|
| Code blocks (C#, JSON, etc.) | Conceptual descriptions |
| Specific file paths or line numbers | Pattern names and relationships |
| Concrete numbers (1.1x, 50 coins) | Trade-off explanations |
| Enum value lists | Categorical thinking |
| Implementation switch statements | Decision rationale |

**Philosophy:** Arc42 answers WHY and WHAT, never HOW. Implementation details belong in code with self-documenting names. Documentation describes approaches, rules, principles, tactics, and strategies.

**"Dare to leave gaps"** - Don't cover everything. Travel light. Focus on decisions that matter.

---

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

## Reading Order by Task Type

**Implement a feature** → Glossaries → `arc42/08` → `arc42/05` → `gdd/01` → `gdd/03`

**Understand balance** → Glossaries → `gdd/01` → `gdd/06` → `gdd/BASELINE_ECONOMY.md`

**Understand an entity** → Glossaries → `arc42/05` → `arc42/08` → Search codebase

**Understand a decision** → Glossaries → `gdd/07` → `arc42/09` → `gdd/01`

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

**THE RULE:** Catalogues translate categorical properties to concrete values at **PARSE-TIME ONLY**.

| Layer | Responsibility |
|-------|----------------|
| Content JSON | Categorical descriptions (friendly, hostile, premium) |
| Catalogue | Translation formulas (parse-time) |
| Entity | Concrete values only (integers, no categories) |

**Why:** Single formula change rebalances all content. Zero runtime overhead. AI generates balanced content without knowing game math.

**FORBIDDEN:**
- Runtime catalogue lookups in Services
- String-based property matching at runtime
- Catalogue calls during player actions

**Exception:** Procedural generation (ProceduralAStoryService) may call catalogues during scene creation - this is "content creation time" not "player action time".

**Details:** See `arc42/08_crosscutting_concepts.md` §8.2

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

**Rule:** Use `List<T>` for all domain entity collections. Never Dictionary or HashSet.

**Why:** Game has ~20 NPCs, ~30 Locations. Dictionary O(1) vs List O(n) saves 0.0009ms. Browser render takes 16ms. Human reaction takes 200ms. **Performance optimization is unmeasurable and premature.**

**Correct:** `List<NPC>`, `List<Location>` with LINQ queries.
**Forbidden:** `Dictionary<string, NPC>`, `HashSet<Location>`.

**Exception:** Dictionary acceptable only for framework requirements (Blazor parameters) or external API caching.

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
- Transform percentages to flat adjustments or integer division (see `arc42/08_crosscutting_concepts.md` §8.22)
- Enforcement: DDR007ComplianceTests.cs (CI), pre-commit hook (`scripts/hooks/install.sh`)

**Lambdas:**
- FORBIDDEN: Backend event handlers, DI registration lambdas
- ALLOWED: LINQ queries, Blazor event handlers, framework configuration

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

# PRE-COMMIT HOOK (REQUIRED)

**Installation (run once per clone):**
```bash
./scripts/hooks/install.sh
```

**What it enforces:**
| Category | Checks |
|----------|--------|
| DDR-007 | Decimal multipliers, basis points, float/double types |
| TYPE | Dictionary, HashSet, `var` keyword |
| HIGHLANDER | Entity instance ID properties |
| FAIL-FAST | Null coalescing (??), TryGetValue/TryParse |
| SEPARATION | CssClass/IconName in backend services |
| QUALITY | TODO/FIXME comments, .Wait()/.Result, extension methods |
| NAMESPACE | Namespace declarations in domain code |
| DETERMINISM | Random usage outside Pile.cs |
| ARC42 | Code blocks in architecture docs |

**Bypass:** `git commit --no-verify` (NOT RECOMMENDED - violations will fail CI)

**Reference:** `arc42/08_crosscutting_concepts.md` for architectural rationale
