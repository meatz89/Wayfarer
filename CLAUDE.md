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

**WRONG** (AI tries to specify absolute values - AI doesn't know game state):
```json
// ❌ WRONG - AI specifies absolute values without game context
{
  "exhaustAfterUses": 2,     // Too weak for late game? Too strong for early?
  "repairCost": 10            // AI doesn't know player's coin economy
}

// ✅ CORRECT - AI specifies relative category
{
  "durability": "Fragile"     // Parser scales based on game state
}
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

**Why:** Players make strategic decisions based on perfect information, then execute tactically with skill-based play.

**Test:** Can the player make an informed decision about WHETHER to attempt a goal before entering the challenge? If not, strategic layer is leaking tactical complexity.

---

## Meta-Principle: Design Constraint as Quality Filter

**When you find yourself reaching for:**
- Dictionaries of generic types
- Boolean flags checked continuously
- Multi-purpose entities
- Ambiguous ownership
- Hidden costs

**STOP. The design is wrong.**

These aren't implementation problems to solve with clever code. They're signals that the game design itself is flawed. Strong typing and explicit relationships aren't constraints that limit you - they're filters that catch bad design before it propagates.

**Good design feels elegant. Bad design requires workarounds.**

**Rules**:
- GameWorld = single source of truth, zero dependencies
- ALL game state lives in GameWorld
- GameWorld depends on NOTHING (all dependencies flow INWARD)
- No SharedData dictionaries or parallel storage
- No parsers stored in GameWorld (initialization-only tools)
- GameWorld created through static GameWorldInitializer
- No GetRequiredService or DI service locator for GameWorld
- GameScreen authoritative - owns ALL screen state
- Children get parent via CascadingValue
- Children call parent methods DIRECTLY
- No complex event chains or sideways data passing
- Screen components render INSIDE GameScreen container
- Screens provide content only, NOT structure (no headers, containers)
- GameUIBase is ONLY navigation handler
- Delete first, fix compilation after
- No compatibility layers or legacy fallbacks
- No gradual migration - all at once or not at all
- Complete refactoring only (no half-measures)
- No TODO comments in code
- No legacy code left behind
- HIGHLANDER: One concept, one implementation (delete duplicates)
- No duplicate markdown files (update existing)
- Delete unnecessary abstractions
- Finish what you start - no excuses about "massive scope"
- Strong typing only: No var, Dictionary, HashSet, object, func, lambda
- No extension methods EVER
- No Helper or Utility classes
- Domain Services and Entities only
- One method, one purpose (no ByX, WithY, ForZ patterns)
- No exception handling unless specified
- Prefer int over float
- No logging until requested
- No inline styles - use CSS files
- Code over comments
- Domain Services and Entities only
- No Helper/Utility classes
- Strong typing only: No var, Dictionary, HashSet, object, func, lambda
- No extension methods EVER
- One method, one purpose
- Perfect information: Players can calculate all outcomes
- Deterministic systems: No hidden randomness
- All calculations visible to players
- Never assume - verify actual data
- No duplicate state tracking
- Non-idempotent initialization causes double-render bugs in Blazor prerendering
- Unclear persistence rules cause save/load bugs
- NEVER invent game mechanics
- Cards have ONE fixed cost from JSON
- No conditions, no alternatives, no "OR mechanics", no conditional costs
- If EXACT mechanic isn't documented, don't implement it
- One mechanic, one purpose
- Verisimilitude: Fiction supports mechanics
- Mechanics that make no narrative sense break immersion
- Every resource needs BOTH generation and consumption
- Resources with only generation lead to infinite accumulation
- Resources with only consumption lead to death spirals
- No soft-locks: Always forward progress possible
- Orphaned states create soft-locks
- Unclear transitions create confusion
- Incomplete loops break progression
- 9/10 certainty required before implementing fixes
- Deterministic systems
- Holistic impact analysis required for all changes
- Test ALL connected systems, not just the one you changed
- Features don't exist in isolation
- Changing one system ripples through connected systems
- Testing only changed system guarantees production bugs in systems you didn't test
- No soft-locks: Always forward progress possible
- Check for ripple effects and side effects
- Validate edge cases and boundary conditions
- Ensure player can never get stuck
- 9/10 certainty threshold required for accepting fixes
- Never assume - verify actual data
- NEVER view features in isolation
- Package cohesion: References must be in same package
- Lazy loading with skeleton system
- No hardcoded content in code
- No string/ID matching (use mechanical properties)
- All content from JSON files
- Parsers must parse (no JsonElement passthrough)
- **JSON field names MUST match C# property names EXACTLY**
- **NEVER use JsonPropertyName to map mismatched names**
- If JSON and C# don't match → FIX THE JSON, not the code
- Dependency analysis REQUIRED before changing files
- Use search tools to find ALL references
- Check dependencies
- Test impact radius
- Verify all related files still work
- Analyze BEFORE modifying, not after breaking things
- Build command: `cd src && dotnet build`

**Holistic Refactoring Process**:
1. STOP - Don't fix compilation error tactically
2. UNDERSTAND ARCHITECTURE - What is correct architectural principle?
3. TRACE THE TRIANGLE - Parser → JSON → Entity (all three must align)
4. VERIFY, DON'T ASSUME - Check actual code for what exists
5. FIX HOLISTICALLY - Change parser, entity, AND JSON structure together
6. BUILD AND VERIFY - Architectural fix resolves error class

**Parser-JSON-Entity Triangle**:
- Parser sets property → JSON must have field → Entity must have property
- Parser doesn't set property → Either JSON missing it OR entity shouldn't have it
- All three must align → Change all three together, never in isolation

**Anti-Patterns**:
- ❌ Add property to make compilation work
- ❌ Change method signature to accept different type
- ❌ Cast or convert to make types match
- ❌ Add compatibility layer for both old and new

**Correct Pattern**:
- ✅ Which entity should own this property architecturally?
- ✅ Trace parser → JSON → entity alignment
- ✅ Move property to correct entity in all three: JSON, parser, domain
- ✅ Delete legacy code that violates architecture

**Scorched Earth Process**:
1. DELETE file/class/method completely
2. Let compilation break
3. Fix ALL compilation errors
4. Grep for old name - zero results required
5. Commit complete refactoring

**Why This Matters**:
- Dictionary disease causes runtime type errors, impossible debugging, lost IntelliSense
- Extension methods hide domain logic where you can't find it
- Helper classes are grab bags that violate single responsibility
- Compatibility layers hide problems and create confusion
- Two sources of truth guarantee bugs
- Half-refactored code is worse than no refactoring
- TODOs in production are admissions of incomplete work

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

**Results**:
- ✅ DONE: Task complete, moves to Done
- ❌ READY: Task incomplete, returns to Ready with specific feedback

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

### The Solution: Trust Entity Contracts

**THREE INITIALIZATION PATTERNS:**

**Pattern 1: Inline Initialization (Preferred)**
```csharp
public class Goal
{
    // Collections ALWAYS initialize inline
    public List<string> GoalIds { get; set; } = new List<string>();
    public List<DifficultyModifier> Modifiers { get; set; } = new List<DifficultyModifier>();

    // Complex objects initialize to avoid null checks
    public GoalCosts Costs { get; set; } = new GoalCosts();

    // Value types and strings can have meaningful defaults
    public bool IsAvailable { get; set; } = true;
    public string Description { get; set; } = string.Empty;
}
```

**Pattern 2: Constructor Initialization**
```csharp
public class SocialTurnResult
{
    public List<CardInstance> DrawnCards { get; set; }
    public List<CardInstance> RemovedCards { get; set; }

    public SocialTurnResult()
    {
        DrawnCards = new List<CardInstance>();
        RemovedCards = new List<CardInstance>();
    }
}
```

**Pattern 3: Validation Null (Rare)**
```csharp
public class GameRules
{
    // Intentionally null until loaded from JSON - CRASHES if accessed before load
    public List<ListenDrawCountEntry> ListenDrawCounts { get; set; }

    public int GetListenDrawCount(ConnectionState state)
    {
        if (ListenDrawCounts == null || ListenDrawCounts.Count == 0)
        {
            throw new InvalidOperationException(
                "ListenDrawCounts not loaded from package content. " +
                "Ensure 05_gameplay.json contains listenDrawCounts configuration.");
        }
        // ...
    }
}
```

### Parser Rules

**CORRECT** (Trust Entity):
```csharp
return new SocialCard
{
    Id = dto.Id,
    Title = dto.Title,  // ✅ Entity crashes if null (validation added separately)
    PersonalityTypes = dto.PersonalityTypes != null
        ? new List<string>(dto.PersonalityTypes)
        : new List<string>()  // ✅ DTO might not initialize, entity does
};
```

**CORRECT** (Validate Required Fields):
```csharp
if (string.IsNullOrEmpty(dto.Title))
    throw new InvalidOperationException($"Card '{dto.Id}' missing required field 'title'");

return new SocialCard
{
    Title = dto.Title  // ✅ Validated above, no ?? needed
};
```

### Game Logic Rules

**CORRECT** (Trust Entity):
```csharp
// Entity guarantees GoalIds is never null
List<Goal> goals = obstacle.GoalIds
    .Select(id => _gameWorld.Goals[id])
    .ToList();  // ✅ No ?? operator needed
```

### The ?? Operator: When It's Allowed

**ONLY TWO VALID USES:**

**1. ArgumentNullException Guards:**
```csharp
public SomeClass(GameWorld gameWorld)
{
    _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));  // ✅ OK
}
```

**2. DTO → Entity Conversion (DTO May Not Initialize):**
```csharp
// DTO from JSON might not initialize collections
public Goal ParseGoal(GoalDTO dto)
{
    return new Goal
    {
        // ✅ OK: DTO might be null, entity guarantees initialization
        GoalCards = dto.GoalCards != null
            ? dto.GoalCards.Select(ParseGoalCard).ToList()
            : new List<GoalCard>()
    };
}
```

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
One mechanic, one purpose | Verisimilitude (fiction supports mechanics) | Perfect information (players can calculate) | Deterministic systems | No soft-locks (always forward progress)

**UI**
Dumb display only (no game logic in UI) | All choices are cards, not buttons | Backend determines availability | Unified screen architecture | Separate CSS files | Clean specificity (no !important hacks) | Resources always visible

**CONTENT**
Package cohesion (references in same package) | Lazy loading with skeletons | No hardcoded content | No string/ID matching | All content from JSON | Parsers must parse (no JsonElement passthrough) | JSON field names MUST match C# property names (NO JsonPropertyName workarounds)

**ASYNC**
Always async/await | Never .Wait(), .Result, .GetAwaiter().GetResult() | No Task.Run or parallel operations | If method calls async, it must be async | No synchronous wrappers | Propagate async to UI

---

## BUILD COMMAND

`cd src && dotnet build`