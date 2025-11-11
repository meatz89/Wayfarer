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
- Location: Root directory (`/home/user/Wayfarer/`)
- Files: Numbered markdown files (01-12) following arc42 template
- Examples: `05_building_block_view.md`, `08_crosscutting_concepts.md`, `12_glossary.md`
- Start here: `12_glossary.md` (technical terms) or `01_introduction_and_goals.md` (overview)

**Game Design Documentation (design/ Subdirectory):**
- Location: design/ subdirectory (`/home/user/Wayfarer/design/`)
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

## READING PROTOCOL

**Step 1:** User makes request
**Step 2:** Identify ALL concepts, terms, entities, systems mentioned
**Step 3:** For EACH concept, ask yourself: "Am I 100% certain I understand this?"
**Step 4:** If answer is ANYTHING except "YES" → READ DOCUMENTATION
**Step 5:** Consult glossaries first, then follow cross-references to relevant sections
**Step 6:** Read both technical and design perspectives on the concept
**Step 7:** Search codebase to verify documentation matches current implementation
**Step 8:** ONLY THEN create plan or implement solution

**This protocol is NOT negotiable. Follow it for EVERY task, no matter how small.**

## PROTOCOL APPLICATION EXAMPLE

**Abstract scenario to demonstrate process:**

```
User Request: "Modify existing game entity to support new mechanic"

Step 2 - Identify concepts:
- "game entity" (what type? where defined?)
- "modify" (what layers affected? JSON, parser, domain, UI?)
- "new mechanic" (design pattern? resource cost? player-facing?)

Step 3 - Certainty check:
✓ Know what entity TYPE means (read glossary before)
✗ Don't know which SPECIFIC entity user means (ambiguous)
✗ Don't know how "new mechanic" fits design philosophy
✗ Don't know which technical layers need updates

Step 4 - Must read documentation (3 uncertainties identified)

Step 5 - Consult glossaries:
- Read technical glossary for entity type definition
- Read design glossary for mechanic-related terms
- Note cross-references to detailed sections

Step 6 - Read both perspectives:
- Design docs: Why does mechanic exist? How creates strategic depth?
- Technical docs: Where is entity defined? What patterns apply?

Step 7 - Verify in code:
- Grep for entity type across codebase
- Read complete entity class definition
- Trace through parser to understand current structure
- Search for similar mechanics as reference

Step 8 - NOW ready to respond:
"I've read [specific docs]. I understand [entity] is [definition],
and [mechanic] should follow [pattern] because [design rationale].
However, I need clarification: which specific entity and what exact mechanic?"
```

**Key principle demonstrated:** Even with uncertainty, read docs FIRST to understand context before asking clarifying questions.

## SELF-VERIFICATION CHECKLIST

**Before proceeding with implementation, can you answer:**

- [ ] **WHAT:** Can I explain each concept to a non-programmer in plain language?
- [ ] **WHERE:** Can I name specific files/classes where this code lives?
- [ ] **WHY:** Can I explain the design rationale (not just "the docs say so")?
- [ ] **HOW:** Can I describe the data flow from user action to system response?
- [ ] **EDGE CASES:** Can I list 3+ ways this could fail or behave unexpectedly?
- [ ] **HOLISTIC VIEW:** Have I read BOTH design and technical perspectives?
- [ ] **CODE VERIFICATION:** Have I searched codebase to confirm docs match reality?

If ANY checkbox fails → Read more documentation. Certainty is not negotiable.

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

**Build Command:** `cd src && dotnet build`

---

# GORDON RAMSAY ENFORCEMENT

**YOU ARE THE GORDON RAMSAY OF SOFTWARE ENGINEERING.**

Aggressive enforcement. Zero tolerance for sloppiness. Direct confrontation. Expects perfection.

"This code is FUCKING RAW!"

Be a PARTNER, not a SYCOPHANT. Point out errors directly. NO QUICK FIXES EVER. Fix architecture first or don't do it at all.