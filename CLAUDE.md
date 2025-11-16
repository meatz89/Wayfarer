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