# Comprehensive Hardcoded Entity ID Violations Report

**Date:** 2025-11-15
**Total Violations Found:** 79
**Violations Fixed:** 11 (8 documentation + 3 JSON)
**Violations Remaining:** 68

---

## Executive Summary

After comprehensive search across all code, JSON, tests, and documentation, found **79 total violations** of the architectural mandate prohibiting hardcoded entity IDs.

**Violations fall into 4 categories:**

1. **FIXED** (11 violations) - Documentation cleaned + 1 JSON file migrated
2. **ARCHITECTURAL REDESIGN REQUIRED** (26 violations) - Cannot be fixed without major system changes
3. **QUESTIONABLE** (12 violations) - Need architectural clarification on whether these are violations
4. **FIXABLE** (48 violations) - Code and test violations that can be directly fixed

---

## CATEGORY 1: FIXED ✅ (11 violations)

### Documentation Violations (8 fixed)

**Commit:** `458a450` - "Eliminate all hardcoded entity ID violations from documentation"

1. **SCENE_ACTIVATION_BUG_ANALYSIS.md** - DELETED (429 lines proposing hardcoded ID support)
2. **design/06_narrative_design.md:62** - Replaced "Specific entity references" with categorical pattern
3. **test_notes_secure_lodging.md:245-282** - Restructured to reject hardcoded ID compatibility layers
4. **TUTORIAL_SPEC.md:528** - Removed hardcoded LocationId example
5. **08_crosscutting_concepts.md:527** - Fixed tutorial spawn description
6. **PLAYABILITY_FLOW.md:226-243** - Labeled legacy examples as violations
7. **design/07_content_generation.md:1286-1287** - Replaced with categorical filters
8. **design/08_balance_philosophy.md:1254** - Fixed NPC entity field name

### JSON Data Violations (3 fixed)

**Commit:** `8df27b2` - "Migrate 21_tutorial_scenes.json to categorical filters"

**File:** `src/Content/Core/21_tutorial_scenes.json`

9. Line 21: `"locationId": "common_room"` → `"locationProperties": ["Commercial", "Restful"]`
10. Line 25: `"npcId": "elena"` → `"professions": ["Innkeeper"]`
11. Line 37: `"locationId": "fountain_plaza"` → `"locationProperties": ["Public", "Outdoor"]`

---

## CATEGORY 2: ARCHITECTURAL REDESIGN REQUIRED 🚨 (26 violations)

These violations cannot be fixed with simple edits. They require fundamental architectural changes to the affected systems.

### 2.1 Conversation Trees (4 violations) - MAJOR REDESIGN

**File:** `src/Content/Core/15_conversation_trees.json`
**Lines:** 13, 96, 211, 296

**Current Pattern (VIOLATION):**
```json
{
  "id": "elena_welcome",
  "npcId": "elena_innkeeper",  ← Hardcoded NPC binding
  "nodes": [ /* dialogue tree */ ]
}
```

**Problem:**
- Conversation trees are CHARACTER-SPECIFIC authored dialogue
- Current architecture: ConversationTree → NPC (hardcoded ID lookup)
- Violates DDR-001: "Cannot reference specific entity IDs in preauthored content"

**Required Fix:**
- Reverse binding: NPC → ConversationTree (NPC entity owns list of tree IDs)
- Update NPCDTO to include `conversationTreeIds: string[]`
- Update NPC entity to include `ConversationTrees: List<ConversationTree>`
- Rewrite ConversationTreeParser to resolve trees TO NPCs instead of FROM trees
- Update all ConversationTreeFacade queries to find trees via NPC object

**Impact:** MAJOR - Touches parser, DTO, entity model, facade, UI queries

**Alternative:** Accept that authored character dialogue is inherently character-specific and exempt from categorical requirement (requires architecture decision)

---

### 2.2 Exchange Cards (7 violations) - MAJOR REDESIGN

**File:** `src/Content/Core/11_exchange_cards.json`
**Lines:** 14, 22, 30, 38, 46, 54, 62 (all reference "merchant_general")

**Current Pattern (VIOLATION):**
```json
{
  "id": "buy_rope",
  "npcId": "merchant_general",  ← Hardcoded merchant binding
  "cost": { "coins": 25 },
  "reward": { "itemId": "rope" }
}
```

**Problem:**
- Exchange cards are NPC-specific service offerings
- All 7 exchange cards hardcode "merchant_general" as provider
- Violates categorical matching requirement

**Required Fix (Option A - Categorical):**
- Add categorical NPC matching to exchange system
- Filter: `professions: ["Merchant"], services: ["Equipment"]`
- EntityResolver finds matching NPCs dynamically
- Exchange cards spawn at ANY matching merchant

**Required Fix (Option B - Reverse Binding):**
- NPC entities own exchange card lists
- NPCDTO includes `exchangeCardIds: string[]`
- ExchangeParser resolves cards TO NPCs

**Impact:** MAJOR - Either option requires parser rewrite, entity model changes, query pattern updates

---

### 2.3 Observation Scenes (4 violations) - MODERATE REDESIGN

**File:** `src/Content/Core/16_observation_scenes.json`
**Lines:** 13, 72, 131, 186

**Current Pattern (VIOLATION):**
```json
{
  "id": "common_room_investigation",
  "locationId": "common_room",  ← Hardcoded location binding
  "observations": [ /* discovery content */ ]
}
```

**Problem:**
- Observation scenes are LOCATION-SPECIFIC authored investigation content
- Current architecture hardcodes location IDs

**Required Fix:**
- Categorical location matching: `locationProperties: ["Commercial", "Social"]`
- OR reverse binding: Location → ObservationScenes
- Update ObservationParser and entity resolution

**Impact:** MODERATE - Parser rewrite, but simpler than conversation trees

---

### 2.4 Dynamic Exchange Generation (2 violations) - CODE ISSUE

**File:** `src/Content/ExchangeParser.cs`
**Lines:** 138, 164

**Current Pattern (VIOLATION):**
```csharp
exchanges.Add(new ExchangeCard
{
    Id = $"{npc.ID}_food_purchase",  ← Encoding NPC ID into card ID
    Name = "Buy Hunger",
    // ...
});
```

**Problem:**
- Procedurally generated exchange cards encode NPC ID into card ID
- Violates ID encoding prohibition (CLAUDE.md line 283)

**Required Fix:**
- Use GUID-only IDs: `Id = Guid.NewGuid().ToString()`
- Store NPC reference as object property, not encoded in ID
- Add `NPC Npc { get; set; }` property to ExchangeCard

**Impact:** LOW - Simple refactor, but must update all ID-based lookups

---

## CATEGORY 3: QUESTIONABLE ❓ (12 violations)

These may or may not be violations depending on architectural interpretation.

### 3.1 NPC Initial Placement (4 violations) - NEEDS CLARIFICATION

**File:** `src/Content/Core/03_npcs.json`
**Lines:** 18, 37, 39, 58

**Current Pattern:**
```json
{
  "id": "elena",
  "name": "Elena",
  "profession": "Innkeeper",
  "venueId": "brass_bell_inn",     ← Hardcoded venue
  "locationId": "common_room",     ← Hardcoded initial location
  "workLocationId": "common_room", ← Hardcoded work location
  "homeLocationId": "common_room"  ← Hardcoded home location
}
```

**Question:** Is initial NPC placement in authored world a violation?

**Arguments FOR violation:**
- DDR-001 says "Cannot reference specific entity IDs in preauthored content"
- NPCs should spawn categorically at matching venues/locations

**Arguments AGAINST violation:**
- Authored NPCs (Elena, Thomas) are PART of the authored starting world
- Initial world state requires positioning authored entities
- This is STRUCTURAL WORLD DATA, not dynamic content spawning
- Equivalent to defining "Elena is the innkeeper AT the Brass Bell Inn"

**Architectural Decision Needed:** Does "no hardcoded entity IDs" apply to initial world state definition, or only to content that spawns during gameplay?

---

### 3.2 Hex Grid Mappings (4 violations) - STRUCTURAL DATA?

**File:** `src/Content/Core/02_hex_grid.json`
**Lines:** 23, 31, 39, 307

**Current Pattern:**
```json
{
  "hexCoordinate": { "q": 0, "r": 0 },
  "locationId": "square_center"  ← Maps hex to location
}
```

**Question:** Is spatial topology data a violation?

**Arguments FOR violation:**
- Uses hardcoded location IDs

**Arguments AGAINST violation:**
- This is SPATIAL TOPOLOGY - defines world structure
- Hex grid IS the authored world map
- Equivalent to saying "square_center is located at coordinates 0,0"
- Not dynamic content spawning, but world definition

**Architectural Decision Needed:** Does spatial topology data get exemption from categorical requirement?

---

### 3.3 Delivery Job Format (1 violation) - DOCUMENTATION

**File:** `src/GameState/DeliveryJob.cs`
**Line:** 15

**Current Pattern:**
```csharp
/// <summary>
/// Unique identifier for this job.
/// Format: "delivery_{originId}_to_{destinationId}"
/// </summary>
public string Id { get; set; } = "";
```

**Question:** Is documenting an ID format a violation?

**Issue:** Documentation explicitly states hardcoded format that encodes location IDs

**Related Code:** `src/Content/Catalogs/DeliveryJobCatalog.cs:85` creates IDs in this format

**Decision Needed:** Is ID format documentation acceptable if IDs are never parsed for data extraction?

---

### 3.4 Test Data (1 violation) - TEST EXEMPTION?

**File:** `src/Content/Test/01_test_lodging.json`
**Line:** 51

**Current Pattern:**
```json
{
  "id": "test_npc",
  "LocationId": "test_inn_common"  ← Hardcoded test location
}
```

**Question:** Do test files get exemptions for synthetic test data?

**Arguments FOR exemption:**
- Test data is not production content
- Tests need controlled, predictable scenarios
- Hardcoded test IDs enable deterministic testing

**Arguments AGAINST exemption:**
- Tests should follow same architectural patterns as production
- Tests using hardcoded IDs don't validate categorical matching
- Bad patterns in tests leak into production

---

## CATEGORY 4: FIXABLE 🔧 (48 violations)

These violations can be fixed with direct code/test changes without architectural redesign.

### 4.1 CRITICAL Code Violations (4 violations)

#### 4.1.1 Route ID Parsing - CLAUDE.md Explicit Violation

**File:** `src/Content/PackageLoader.cs`
**Line:** 1504

**VIOLATION:**
```csharp
string[] idParts = forwardRoute.Id.Split("_to_");
string reverseId = idParts.Length == 2
    ? $"{idParts[1]}_to_{idParts[0]}"
    : $"{destVenueId}_to_{originVenueId}";
```

**Why CRITICAL:**
- CLAUDE.md line 284 explicitly forbids: "Parsing IDs to extract data: `.Split()`, `.Substring()`"
- This is the EXACT antipattern the architecture prohibits

**Fix:**
- Store origin/destination as properties on Route
- Construct reverse route from properties: `new Route { Origin = forward.Destination, Destination = forward.Origin }`
- Use GUID-only IDs

---

#### 4.1.2 Archetype ID-Based Routing - CLAUDE.md Explicit Violation

**File:** `src/Content/Catalogs/SituationArchetypeCatalog.cs`
**Lines:** 759, 763, 767

**VIOLATION:**
```csharp
if (archetype.Id == "service_negotiation")
{
    return GenerateServiceNegotiationChoices(archetype, situationTemplateId, context);
}
else if (archetype.Id == "service_execution_rest")
{
    return GenerateServiceExecutionRestChoices(situationTemplateId, context);
}
```

**Why CRITICAL:**
- CLAUDE.md line 285 explicitly forbids: "String matching on IDs for routing"
- Should use enum-based routing, not ID strings

**Fix:**
- Add `ArchetypeType` enum property to `SituationArchetype`
- Switch on `archetype.Type` instead of `archetype.Id`

---

#### 4.1.3 Route Key Format Mismatch - SILENT BUG

**File:** `src/Subsystems/Travel/TravelTimeCalculator.cs`
**Lines:** 65-66

**VIOLATION:**
```csharp
string routeKey = $"{fromLocationId}_to_{toLocationId}";
List<RouteImprovement> improvements = _gameWorld.RouteImprovements
    .Where(ri => ri.RouteId == routeKey).ToList();
```

**Why CRITICAL:**
- Constructs key with LOCATION IDs
- RouteImprovement.RouteId uses VENUE IDs (from DeliveryJobCatalog.cs:88)
- WHERE clause NEVER matches → route improvements NEVER applied
- **SILENT DATA LOSS BUG**

**Fix:**
- Use route.Id directly for lookup
- OR ensure consistent ID format

---

#### 4.1.4 Skeleton ID Parsing - CLAUDE.md Explicit Violation

**File:** `src/Content/SkeletonGenerator.cs`
**Line:** 208

**VIOLATION:**
```csharp
string venueId = source.Contains("location_")
    ? source.Substring(source.IndexOf("location_") + 9).Split('_')[0]
    : "unknown_location";
```

**Why CRITICAL:**
- Uses `.Substring()`, `.IndexOf()`, `.Split()` - all explicitly forbidden
- Fragile string parsing assumption

**Fix:**
- Pass `venueId` as explicit parameter instead of parsing from source

---

### 4.2 HIGH Code Violations (5 violations - ID Encoding with Substring)

All use forbidden `.Substring()` method to truncate GUIDs in ID encoding:

1. **VenueGeneratorService.cs:38** - `Guid.NewGuid().ToString().Substring(0, 8)`
2. **HexRouteGenerator.cs:383** - `Guid.NewGuid().ToString("N").Substring(0, 8)`
3. **SceneInstantiator.cs:108** - `Guid.NewGuid().ToString("N").Substring(0, 8)`
4. **SceneInstantiator.cs:180** - `Guid.NewGuid().ToString("N").Substring(0, 8)`
5. **SceneInstantiator.cs:304** - `Guid.NewGuid().ToString("N").Substring(0, 8)`

**Fix:** Use full GUID, no truncation: `Guid.NewGuid().ToString()`

---

### 4.3 MEDIUM Code Violations (7 violations - ID Encoding Patterns)

1. **EntityResolver.cs:248** - `$"generated_location_{Guid.NewGuid()}"`
2. **EntityResolver.cs:304** - `$"generated_route_{Guid.NewGuid()}"`
3. **SocialCardParser.cs:18** - `$"request_{conversationTypeId}"`
4. **HexRouteGenerator.cs:170** - `$"route_{origin.Id}_{destination.Id}"`
5. **HexRouteGenerator.cs:345** - `$"scene_{template.Id}_{route.Id}_seg{segment.SegmentNumber}"`
6. **DeliveryJobCatalog.cs:85** - `$"delivery_{origin.Id}_to_{destination.Id}"`
7. **LocationActionManager.cs:71** - `$"complete_delivery_{activeJob.Id}"`

**Impact:** Medium priority - these encode data but aren't currently parsed

**Fix:** Use GUID-only IDs, store data as properties

---

### 4.4 Test Violations (33 violations across 5 files)

All tests query by hardcoded scene/situation IDs instead of semantic properties.

**Pattern (WRONG):**
```csharp
SceneTemplate a1 = gameWorld.SceneTemplates.First(st => st.Id == "a1_secure_lodging");
SituationTemplate sit = a2.SituationTemplates.FirstOrDefault(s => s.Id.Contains("negotiate"));
```

**Pattern (CORRECT):**
```csharp
SceneTemplate a1 = gameWorld.SceneTemplates.First(st =>
    st.Category == StoryCategory.MainStory && st.MainStorySequence == 1);
SituationTemplate sit = a2.SituationTemplates.FirstOrDefault(s =>
    s.ChoiceTemplates.Any(c => RequiresNegotiation(c)));
```

**Files with violations:**
1. **AStoryPlayerExperienceTest.cs** - 4 violations
2. **TutorialInnLodgingIntegrationTest.cs** - 3 violations
3. **RapportBuildPlaythroughTest.cs** - 9 violations
4. **FallbackPathTests.cs** - 5 violations
5. **ParametricSpawningTests.cs** - 9 violations

**Fix:** Replace all ID-based queries with semantic property queries

---

## Summary by Priority

| Priority | Category | Count | Action Required |
|----------|----------|-------|-----------------|
| ✅ DONE | Documentation + Tutorial JSON | 11 | Already fixed and committed |
| 🔥 CRITICAL | Code violations (runtime bugs) | 4 | Fix immediately - breaks gameplay |
| 🚨 BLOCKER | Architectural redesign required | 26 | Requires design decisions + major refactoring |
| ❓ UNCLEAR | Needs architectural clarification | 12 | Requires design decision on exemptions |
| 🔧 HIGH | Code ID encoding with Substring | 5 | Fix when possible - violates CLAUDE.md |
| 🔧 MEDIUM | Code ID encoding patterns | 7 | Lower priority - not currently harmful |
| 🧪 TESTS | Test hardcoded ID queries | 33 | Fix to validate semantic queries work |

**Total:** 79 violations (11 fixed, 68 remaining)

---

## Recommended Action Plan

### Phase 1: CRITICAL CODE FIXES (Immediate)
1. Fix PackageLoader.cs route ID parsing
2. Fix SituationArchetypeCatalog.cs ID-based routing
3. Fix TravelTimeCalculator.cs route key mismatch bug
4. Fix SkeletonGenerator.cs ID substring parsing

**Impact:** Fixes runtime bugs, eliminates explicit CLAUDE.md violations

### Phase 2: ARCHITECTURAL DECISIONS (Design Review)
1. Decide: Are conversation trees exempt from categorical requirement?
2. Decide: Are exchange cards exempt or require redesign?
3. Decide: Are observation scenes exempt or require redesign?
4. Decide: Is NPC initial placement in authored world exempt?
5. Decide: Is hex grid spatial topology exempt?

**Impact:** Determines scope of remaining work

### Phase 3: BASED ON DECISIONS
- If exemptions granted: Document exemptions, close those violations as "architectural exceptions"
- If no exemptions: Implement required architectural redesigns

### Phase 4: CLEANUP (Lower Priority)
1. Fix HIGH code violations (Substring usage)
2. Fix MEDIUM code violations (ID encoding)
3. Fix test violations (semantic queries)

---

## Files Changed Summary

**Already Committed:**
- 8 documentation files (deleted/modified)
- 1 JSON data file (21_tutorial_scenes.json)

**Require Changes:**
- 11 C# code files (4 critical, 7 medium)
- 5 test files (33 query fixes)
- 3 JSON data files (conversation trees, exchange cards, observation scenes) - IF architectural decisions support fixing
- 2 JSON data files (NPCs, hex grid) - IF determined to be violations

**Total Files:** 30 files need changes (9 done, 21 pending decisions/fixes)
