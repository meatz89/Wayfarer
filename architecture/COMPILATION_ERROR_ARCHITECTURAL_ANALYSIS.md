# Compilation Error Architectural Analysis

## Context

127 compilation errors remain after HIGHLANDER refactoring (deletion of .Id properties from domain entities). These errors are NOT simple syntax problems - they reveal **architectural boundary violations** that must be fixed following proper entity resolution patterns.

## Core Architectural Principles Applied

### 1. Entity Resolution Boundaries (Three-Boundary Architecture)

```
JSON (categorical) → PARSE → Templates (categorical) → SPAWN → Scenes (objects) → QUERY → Actions (objects)
```

**BOUNDARY 1: PARSE-TIME** - JSON → Templates
- Input: JSON with string properties
- Output: Templates with categorical filters
- Resolution: Template IDs resolved (immutable archetypes)
- Storage: GameWorld.SceneTemplates, ItemTemplates, etc.

**BOUNDARY 2: SPAWN-TIME** ⭐ **PRIMARY RESOLUTION POINT**
- Input: Templates with categorical filters
- Output: Scenes/Situations with resolved entity objects
- Resolution: EntityResolver.FindOrCreate converts categorical → objects
- Storage: Scene.PlacementLocation, Situation.RequiredNPC (object references)

**BOUNDARY 3: QUERY-TIME** - Scene → Actions
- Input: Scenes with resolved objects
- Output: Actions with inherited object references
- Resolution: NONE - copies Scene's resolved objects
- Storage: Ephemeral actions deleted after use

### 2. HIGHLANDER Principle

**One Resolution, One Storage, One Access Path:**
- Resolve entity object references ONCE at spawn-time
- Store resolved objects in Scene/Situation properties
- All downstream code uses those stored objects directly
- NEVER re-resolve, NEVER extract strings, NEVER lookup by ID

**Violations:**
- ❌ Storing string ID, looking up later
- ❌ Extracting .Name/.Id from object for comparison
- ❌ Methods accepting string parameters when object available upstream
- ❌ Result classes containing ID properties (upstream should pass objects)

### 3. String Usage - Three Permitted Contexts ONLY

**Context 1: Template IDs** (immutable archetypes)
- SceneTemplate.Id, SituationTemplate.Id, ItemTemplate.Id
- Hierarchical references between templates
- Never changes after parse-time

**Context 2: Display Output** (user-facing only)
- Console.WriteLine($"Item: {item.Name}")
- System messages, logging, UI labels
- Extract .Name/.Description at point of display

**Context 3: Categorical Properties** (type descriptors)
- locationType: "CommonRoom", quality: "Standard"
- Describe CATEGORIES, not specific entities
- Used in filters for EntityResolver.FindOrCreate

**FORBIDDEN:** Using .Name or .Id for identity, comparison, storage, or method parameters

### 4. CONTRACT BOUNDARIES FIRST (Refactoring Order)

When fixing cross-boundary violations:

1. **INPUT BOUNDARY** - JSON/DTOs contain what?
2. **PARSER TRANSFORMATION** - How resolve categorical → objects?
3. **PUBLIC API** - What signatures do facades expose?
4. **OUTPUT BOUNDARY** - What do ViewModels/Results contain?
5. **INTERNAL IMPLEMENTATION** - Falls into place after boundaries defined

**NEVER fix internal code before fixing boundaries.** Boundaries constrain implementation.

## Error Categories by Architectural Violation Type

### Category A: PARSER BOUNDARY (String→Object Resolution) - 10 errors

**Violation:** Parser creates templates/entities containing string properties that should be resolved to object references at parse-time or spawn-time.

**Examples:**
1. **Achievement strings** (SpawnRuleParser, SceneTemplateParser)
   - Current: Template contains `List<string> requiredAchievementNames`
   - Correct: Template contains `List<Achievement> requiredAchievements` (resolved at parse-time via AchievementRepository)

2. **ExchangeCard.RequiredVenueId** (2 errors)
   - Current: Card stores `string RequiredVenueId`
   - Correct: Card stores `Venue RequiredVenue` (resolved at instantiation)

3. **LocationDTO.VenueId** (SceneInstantiator line 1041)
   - Current: DTO has VenueId property, parser tries to access
   - Correct: DTO should have Venue resolved during parsing, or EntityResolver handles at spawn

**Architectural Fix:**
- If resolution needs GameWorld context → Spawn-time via EntityResolver
- If resolution is catalogue lookup → Parse-time via repository
- Change DTOs/Templates to store object references, not strings
- Parser/Instantiator calls resolver to convert strings → objects

### Category B: SPAWN-TIME BOUNDARY (Critical Resolution Errors) - 2 errors

**Violation:** SceneInstantiator performing incorrect resolution or missing required template properties.

**Examples:**
1. **District coalesce error** (SceneInstantiator line 1129)
   - Current: Code attempts `District ?? string` (type mismatch)
   - Indicates: Confusion between resolved District object vs string fallback
   - Correct: Either store District object OR handle null explicitly

2. **Situation.SituationTemplate missing** (ObligationActivity line 171)
   - Current: Situation entity missing Template property
   - Correct: Every instantiated Situation should reference its SituationTemplate (immutable archetype)
   - Architectural: Scene/Situation separation - Situation stores Template reference + resolved entities

**Architectural Fix:**
- Situations MUST have Template property (reference to immutable archetype)
- SceneInstantiator MUST set Situation.Template during instantiation
- No type mixing (District object vs string) - resolve fully or fail explicitly

### Category C: OUTPUT BOUNDARY (Result Class ID Properties) - 8 errors

**Violation:** Result classes contain .Id properties that were deleted. Upstream code extracts IDs instead of passing object references through call chain.

**Examples:**
1. **Challenge Context DeckIds** (MentalChallengeContext, PhysicalChallengeContext)
   - Current: Context stores `string DeckId`
   - Correct: Context stores `Deck Deck` object reference
   - Call chain: Facade creates context with Deck object → UI uses Deck.Cards

2. **Obligation Result IDs** (ObligationProgressResult, ObligationCompleteResult, ActiveObligation)
   - Current: Results store `string ObligationId`
   - Correct: Results store `Obligation Obligation` object reference
   - Call chain: Service receives Obligation object → Returns result with object → UI displays via object.Name

3. **TravelResult.RouteId** (GameFacade line 314)
   - Current: Result stores `string RouteId`
   - Correct: Result stores `RouteOption Route` object reference
   - Call chain: TravelFacade receives Route object → Returns result with object

**Architectural Fix:**
- DELETE all .Id properties from result classes
- ADD corresponding object reference properties
- Trace call chain UPSTREAM to find where string extraction happens
- Change extracting code to pass objects instead
- Result classes become simple DTOs containing objects for UI binding

### Category D: CALL CHAIN VIOLATIONS (String Parameters) - ~60 errors

**Violation:** Methods accept string parameters (npcId, locationId) when callers have object references available. String extraction violates HIGHLANDER.

**Examples:**
1. **GameFacade.Method(string locationId)** called with Location object
   - Current: `facade.Method(location.Name)` - extracts string
   - Correct: Change signature to `Method(Location location)`, caller passes object directly

2. **ConversationTree lookup by string** (multiple UI components)
   - Current: UI stores `string conversationTreeId`, calls `facade.GetTree(id)`
   - Correct: UI stores `ConversationTree tree` object, passes directly

3. **EmergencySituation/NPC/Location string parameters**
   - Current: Methods accept `string emergencySituationId`
   - Correct: Methods accept `EmergencySituation situation` object

**Architectural Fix:**
- For EACH error: Find method with string parameter
- Trace ALL callers to find origin of string
- If caller has object: Change caller to pass object, change method signature
- If caller has string from upstream: Recurse upstream until object source found
- Fix entire call chain from object source → final method
- Verify: No `.Name` or `.Id` extraction except display

### Category E: REMAINING ITEM CONVERSIONS - 3 errors

**Violation:** Code attempts to convert Item objects to strings (likely for method calls expecting string parameters).

**Examples:**
- ObligationActivity, GameFacade passing Item where string expected
- Indicates: Downstream method signature needs fixing (accept Item, not string)

**Architectural Fix:**
- Identify target method signature
- Change to accept Item object
- Remove conversion at call site
- Follow pattern from ExchangeCostStructure fixes (already completed)

## Systematic Fix Strategy (Contract Boundaries First)

### PHASE 1: Parser Boundary (Input→Template)
**Time: 2-4 hours**

1. Achievement resolution (2 files)
   - Add Achievement entity to GameWorld
   - Create AchievementRepository.GetByName()
   - SpawnRuleParser/SceneTemplateParser resolve strings → Achievement objects
   - Templates store List<Achievement>, not List<string>

2. ExchangeCard.RequiredVenue (1 file)
   - Change property from `string RequiredVenueId` to `Venue RequiredVenue`
   - Parser resolves venue name → Venue object during card creation
   - Remove all RequiredVenueId references

3. LocationDTO.VenueId (1 file)
   - Determine if DTO should resolve at parse-time or spawn-time
   - If parse: Parser resolves VenueId → Venue, stores object
   - If spawn: EntityResolver handles, pass filter

**Verification:** `grep -rn "string.*Id" src/Content/ | grep -v Template` → minimal results

### PHASE 2: Spawn-Time Boundary (Template→Scene)
**Time: 1-2 hours**

1. Situation.Template property (1 file)
   - Add `public SituationTemplate Template { get; set; }` to Situation entity
   - SceneInstantiator sets `situation.Template = situationTemplate` during instantiation
   - All spawned Situations reference their archetype

2. District coalesce fix (1 file)
   - Read SceneInstantiator line 1129 context
   - Determine correct null handling (fail vs create)
   - Fix type mismatch (District object vs string)

**Verification:** Situations have Template property, instantiator sets it

### PHASE 3: Output Boundary (Service→ViewModel)
**Time: 4-6 hours**

1. Challenge Context DeckIds (2 files)
   - Change `string DeckId` → `Deck Deck` in context classes
   - Trace upstream: Find where DeckId extracted
   - Change to pass Deck object instead
   - UI binds to `context.Deck.Name` for display

2. Obligation Result IDs (3 files + call sites)
   - Delete `string ObligationId` from result classes
   - Add `Obligation Obligation` property
   - Trace all creation sites: Pass Obligation object
   - UI binds to `result.Obligation.Name` for display

3. TravelResult.RouteId (1 file)
   - Delete `string RouteId` property
   - Add `RouteOption Route` property
   - TravelFacade sets `result.Route = route`

**Verification:** `grep -rn "\.Id\s*=" src/Services/ | grep -v Template` → zero results

### PHASE 4: Call Chain Refactoring (End-to-End)
**Time: 6-10 hours**

For each string parameter error:
1. Identify target method signature: `Method(string xId)`
2. Find all call sites: `grep -rn "Method\(" src/`
3. Trace each caller:
   - If caller has object: Change to pass object directly
   - If caller has string: Recurse upstream to object source
4. Change method signature: `Method(X x)`
5. Update method body: Use object directly, no lookup
6. Verify: Entire chain passes objects

**Pattern:**
```csharp
// BEFORE (VIOLATION)
UI: facade.DoThing(npc.Name);           // Extracts string
Facade: DoThing(string npcId) {
  NPC npc = GetById(npcId);             // Lookup
  service.Process(npc);
}

// AFTER (CORRECT)
UI: facade.DoThing(npc);                // Passes object
Facade: DoThing(NPC npc) {
  service.Process(npc);                 // Direct use
}
```

**Verification:** `grep -rn "\.Name\s*)" src/ | grep -v "Console\|WriteLine"` → zero results

### PHASE 5: Item Conversion Cleanup
**Time: 1 hour**

1. Find remaining Item→string conversions
2. Change target method signatures to accept Item
3. Remove conversions at call sites
4. Follow ExchangeCostStructure pattern

**Verification:** All Item-related errors resolved

## Expected Outcomes

**After PHASE 1-2:** ~20 errors resolved (parser + spawn boundary)
**After PHASE 3:** ~30 errors resolved (result class IDs)
**After PHASE 4:** ~70 errors resolved (call chain violations)
**After PHASE 5:** ~7 errors resolved (Item conversions)

**Total:** 127 errors → 0 errors

**Architectural Correctness:**
- ✅ Entity resolution happens ONCE at spawn-time
- ✅ Templates remain categorical (no object references)
- ✅ Scenes store resolved objects (no IDs)
- ✅ All downstream code uses Scene's objects (no lookups)
- ✅ Call chains pass typed objects end-to-end (no string extraction)

## Verification Commands

After all fixes, run:

```bash
# No string comparisons (except display/logging)
grep -rn "\.Name\s*==" src/ --include="*.cs" | grep -v "Console\|Log\|WriteLine"

# No ID comparisons (except templates)
grep -rn "\.Id\s*==" src/ --include="*.cs" | grep -v "Template"

# No string ID properties (except templates)
grep -rn "public string.*Id\s*{" src/GameState/ --include="*.cs" | grep -v "Template"

# No string extractions in call chains (except display)
grep -rn "=\s*.*\.Name\s*;" src/ --include="*.cs" | grep -v "Console\|Display\|Log"

# Build succeeds
cd src && dotnet build
```

Expected result: **ZERO violations, ZERO errors**

## Key Insight

These 127 errors are not 127 independent problems. They are manifestations of **3-5 core architectural violations**:

1. **Parser not resolving entities** (should create objects, creates strings)
2. **Result classes storing IDs** (should pass objects through call chain)
3. **Methods accepting strings** (should accept objects from upstream)
4. **String extraction in call chains** (should pass objects end-to-end)
5. **Missing template properties** (Situation needs Template reference)

Fix these 5 root causes following CONTRACT BOUNDARIES FIRST, and all 127 errors resolve systematically.

**The errors are symptoms. The boundaries are the disease. Fix boundaries holistically.**
