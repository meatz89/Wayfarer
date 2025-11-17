# PHASE 7: COMPREHENSIVE .Id ARCHITECTURE ANALYSIS

**Analysis Date:** 2025-11-17  
**Total .Id Sites Found:** 600 across 123 files  
**Git Branch:** claude/understand-tutorial-architecture-013L634VxjRyPTbMeVLH8tVb

---

## EXECUTIVE SUMMARY

### Categorization Results

**Total Sites: 600**

- **Template/Template-like IDs (ACCEPTABLE):** ~250 sites
  - SceneTemplate.Id, ItemTemplate.Id, ConversationTree.Id, ChoiceTemplate.Id, SituationTemplate.Id
  - Scene instances using TemplateId for persistence
  - Pattern: Immutable archetypes (not mutable game state)

- **Parser DTO External Lookups (ACCEPTABLE):** ~178 sites
  - dto.Id validation (required field checks)
  - Error messages referencing DTO.Id
  - Pattern: Converting external data format to domain

- **Logging/Display Only (ACCEPTABLE):** ~51 sites
  - Console.WriteLine with IDs
  - Exception messages
  - Diagnostic output (no business logic impact)

- **TRUE VIOLATIONS:** ~121 sites
  - Entity ID comparisons: `obj.Id == value` (100 sites)
  - Composite ID generation: `$"id_{entity.Id}_{other.Id}"` (30 sites)
  - Property storing entity IDs: `public string DeckId` (varies)

---

## VIOLATION CATEGORIES

### Category 1: Entity ID Comparisons (~100 sites)

**Pattern:** Using ID equality to find entities instead of object references

```csharp
// VIOLATION - Should lookup once and use object reference
SocialChallengeDeck deck = _gameWorld.SocialChallengeDecks.FirstOrDefault(d => d.Id == situation.DeckId);
situation.Deck = deck;  // This should be direct assignment

// CORRECT - Object reference stored in Situation
public class Situation {
    public object Deck { get; set; }  // Stores actual SocialChallengeDeck object, not ID
}
```

**Top Files:**
- src/Services/ObligationActivity.cs (10 violations)
- src/Subsystems/Social/SocialFacade.cs (6 violations)
- src/GameState/TravelManager.cs (4 violations)
- src/Subsystems/Emergency/EmergencyFacade.cs (4 violations)
- src/Subsystems/Physical/PhysicalFacade.cs (4 violations)

**Issue:** These are looking up entities by ID string. The architecture should use object references instead.

### Category 2: Composite ID Generation (~30 sites)

**Pattern:** Building IDs from multiple entity IDs for identification

```csharp
// VIOLATION - Creates composite ID for job identification
Id = $"delivery_{origin.Id}_to_{destination.Id}",  // Line 85 DeliveryJobCatalog.cs
OriginLocationId = origin.Id,  // Stores location ID as property
DestinationLocationId = destination.Id,

// VIOLATION - Creating choice template ID from situation ID
Id = $"{situationTemplateId}_stat",  // Depends on parent situation having ID

// VIOLATION - Creating movement action ID from location IDs
Id = $"move_to_{destination.Id}",  // Line 172 LocationActionCatalog.cs
```

**Top Files:**
- src/Content/Catalogs/DeliveryJobCatalog.cs (3 violations)
- src/Content/Catalogs/LocationActionCatalog.cs (4 violations)
- src/Content/Catalogs/AStorySceneArchetypeCatalog.cs (multiple ID generation patterns)
- src/Subsystems/ProceduralContent/ProceduralAStoryService.cs (ID generation)

**Issue:** Composite IDs violate single source of truth - the origin.Id becomes embedded in the generated ID, creating redundancy.

### Category 3: ID Properties Storing Entity References (~varies)

**Pattern:** Properties with "_Id" suffix storing string IDs instead of object references

Identified in:
- DeliveryJob: `OriginLocationId`, `DestinationLocationId`, `RouteId`
- SituationSpawnRules: `InitialSituationId`, `SourceSituationId`, `DestinationSituationId`, `ChoiceId`
- SocialChallengeDeck: `DeckId` property (referenced from Situation)
- Obligation: `Id` property (being looked up by ID constantly)
- ObligationPhaseDefinition: `Id` property

---

## DETAILED VIOLATIONS BY FILE

### Top Violation Files

#### 1. **src/Services/ObligationActivity.cs** (10 violations)

All are ID comparison patterns:
```
Line 93:  Obligation obligation = _gameWorld.Obligations.FirstOrDefault(i => i.Id == obligationId);
Line 129: Obligation obligation = _gameWorld.Obligations.FirstOrDefault(i => i.Id == obligationId);
Line 171: Obligation obligation = _gameWorld.Obligations.FirstOrDefault(i => i.Id == obligationId);
Line 179: .FirstOrDefault(p => p.Id == situationId);  // Obligation phase lookup
Line 250: Obligation obligation = _gameWorld.Obligations.FirstOrDefault(i => i.Id == obligationId);
Line 324: Obligation spawnedObligation = _gameWorld.Obligations.FirstOrDefault(i => i.Id == obligationId);
Line 346: Obligation obligation = _gameWorld.Obligations.FirstOrDefault(i => i.Id == obligationId);
Line 429: Obligation obligation = _gameWorld.Obligations.FirstOrDefault(i => i.Id == obligationId);
Line 480: SceneTemplate template = _gameWorld.SceneTemplates.FirstOrDefault(t => t.Id == sceneSpawn.SceneTemplateId);
```

**Root Cause:** Obligation interface requires passing obligation IDs, then looking them up repeatedly.

**Solution:** 
- Change method signatures to accept `Obligation` object references
- Eliminate string ID parameter passing
- Pass objects directly through call chain

#### 2. **src/Content/PackageLoader.cs** (9 violations)

Mostly logging/error display (acceptable) but some composite ID patterns:
```
Line 657: $"FATAL: Failed to parse social card '{dto.Id}'."  // ACCEPTABLE - error message
Line 710: $"Social deck '{deck.Id}' references missing card"  // ACCEPTABLE - error message
```

These are mostly acceptable logging violations.

#### 3. **src/Subsystems/Social/SocialFacade.cs** (6 violations)

**Critical violations:**
```
Line 66:  Situation situation = _gameWorld.Scenes.SelectMany(s => s.Situations)
          .FirstOrDefault(sit => sit.Id == requestId);  // VIOLATION - Situation has no ID!
          
Line 73:  SocialChallengeDeck challengeDeck = _gameWorld.SocialChallengeDecks
          .FirstOrDefault(d => d.Id == situation.DeckId);  // VIOLATION - Two-step lookup

Line 292: Similar pattern to line 66 - Situation ID lookup
Line 444: Similar pattern to line 66 - Situation ID lookup
Line 1156: ObligationPhaseDefinition matchingPhase = obligation.PhaseDefinitions
           .FirstOrDefault(p => p.Id == requestId);  // VIOLATION - ID comparison
```

**Issue:** Situation class explicitly doesn't have Id property (per CLAUDE.md), but code is trying to lookup by ID.

**Solution:** 
- Either add object reference to parent Situation during initialization
- OR store situation object references instead of string IDs in parameters
- Change API to pass Situation objects, not IDs

#### 4. **src/Content/Catalogs/DeliveryJobCatalog.cs** (3+ violations)

```csharp
// Line 39-40: Composite ID violation
Location origin = locations.FirstOrDefault(l => l.Id == route.OriginLocationId);
Location destination = locations.FirstOrDefault(l => l.Id == route.DestinationLocationId);

// Line 85-87: Storing location IDs as properties
Id = $"delivery_{origin.Id}_to_{destination.Id}",
OriginLocationId = origin.Id,
DestinationLocationId = destination.Id,
```

**Issue:** DeliveryJob stores LocationIds instead of object references. Routes also have OriginLocationId/DestinationLocationId.

**Solution:**
- Change DeliveryJob to store `Location OriginLocation` and `Location DestinationLocation`
- Change DeliveryJob to store `RouteOption Route` instead of RouteId
- Generate ID from metadata, not from entity IDs: `Id = $"delivery_{origin.Purpose}_{destination.Purpose}_{Random}"`

#### 5. **src/Content/Catalogs/LocationActionCatalog.cs** (4+ violations)

```csharp
// Line 47: Storing location ID in action
SourceLocationId = location.Id,

// Line 67-68: Composite ID generation
Id = $"work_{location.Id}",
SourceLocationId = location.Id,

// Line 91-92: Similar patterns
Id = $"view_job_board_{location.Id}",
SourceLocationId = location.Id,

// Line 157: ID comparison to filter adjacent locations
l.Id != location.Id &&
```

**Issue:** LocationAction stores `SourceLocationId` string instead of object reference. Actions are generated with composite IDs.

**Solution:**
- Change LocationAction to store `Location SourceLocation` object reference
- Generate stable action IDs without embedding location IDs: `Id = $"{actionType}_{Guid.NewGuid().ToString("N").Substring(0,8)}"`
- Store Location objects directly in action properties

---

## ARCHITECTURAL BLOCKERS

### Blocker 1: Situation Identity Model

**Problem:** Code in SocialFacade.cs tries to lookup Situations by ID, but Situation class has no ID property per design.

**Evidence:**
- CLAUDE.md states: "HIGHLANDER: NO Id property - Situation identified by object reference"
- Situation.cs line 10: `// HIGHLANDER: NO Id property - Situation identified by object reference`
- Yet SocialFacade.cs line 66: `sit.Id == requestId` tries to use an ID

**Resolution Required:**
Either:
1. **Add ID to Situation** - Violates CLAUDE.md principle, but simplifies lookup
2. **Change API to use object references** - Remove ID-based lookups, pass Situation objects through API
3. **Use List indexing** - Situations are embedded in Scenes, use index-based lookup

**Recommended:** Option 2 - Change APIs to pass Situation objects, not IDs

### Blocker 2: Entity Reference Storage in Properties

**Problem:** Multiple domain classes store string IDs of other entities instead of object references

```
DeliveryJob:
  - OriginLocationId (should be: Location OriginLocation)
  - DestinationLocationId (should be: Location DestinationLocation)
  - RouteId (should be: RouteOption Route)

LocationAction:
  - SourceLocationId (should be: Location SourceLocation)
  - DestinationLocationId (should be: Location DestinationLocation)

SituationSpawnRules:
  - InitialSituationId (should be: SituationTemplate InitialSituation)
  - SourceSituationId (should be: SituationTemplate SourceSituation)
  - DestinationSituationId (should be: SituationTemplate DestinationSituation)

Obligation:
  - Id stored and looked up constantly
```

**Impact:** Forces constant ID lookups instead of direct object access.

**Resolution:** Batch migration to object references (Phase 7B-7D work)

### Blocker 3: Composite ID Generation Patterns

**Problem:** Many IDs are generated from other entity IDs, creating brittle dependencies

```csharp
// DeliveryJobCatalog.cs line 85
Id = $"delivery_{origin.Id}_to_{destination.Id}"

// LocationActionCatalog.cs line 172
Id = $"move_to_{destination.Id}"

// SceneTemplateParser.cs line 1017
Id = $"{situationTemplateId}_stat"
```

**Impact:** 
- If origin.Id or destination.Id changes, the composite ID breaks
- ID becomes coupled to entity identity
- Violates single source of truth

**Resolution:** Generate IDs independently, don't embed entity IDs

---

## CATEGORIZATION SUMMARY (600 total sites)

| Category | Count | Status | Action |
|----------|-------|--------|--------|
| Template IDs (acceptable) | ~250 | ✓ CLEAN | None |
| Parser DTO lookups (acceptable) | ~178 | ✓ CLEAN | None |
| Logging/Display (acceptable) | ~51 | ✓ CLEAN | None |
| Entity ID comparisons | ~100 | ✗ VIOLATION | Phase 7B-C |
| Composite ID generation | ~30 | ✗ VIOLATION | Phase 7B-C |
| Property ID storage | ~100+ | ✗ VIOLATION | Phase 7D |
| **TOTAL VIOLATIONS** | **~130-230** | | **Must fix** |

---

## EXECUTION PLAN: PHASE 7

### Phase 7A: Situation Identity Resolution (HIGH PRIORITY)

**Goal:** Resolve blocker #1 - Situation ID lookup issue

**Tasks:**
1. Change SocialFacade.GetSituation(string requestId) → GetSituation(Situation situation)
2. Update all callers to pass Situation objects instead of IDs
3. Remove ID-based lookups for Situations
4. Files affected: SocialFacade.cs, SocialChallengeDeckBuilder.cs, SceneContent.razor.cs

**Complexity:** Medium - Medium refactor of SocialFacade interface, but no domain model changes

### Phase 7B: Entity Reference Object Conversion (MEDIUM PRIORITY)

**Goal:** Convert ID properties to object references

**Files to refactor:**
1. DeliveryJob: OriginLocationId → OriginLocation, DestinationLocationId → DestinationLocation, RouteId → Route
2. LocationAction: SourceLocationId → SourceLocation, DestinationLocationId → DestinationLocation
3. Obligation: Keep ID (acceptable for standalone entities)
4. SituationSpawnRules: Keep IDs (references within JSON templates)

**For each file:**
- Change DTO properties to store objects
- Update parsers to set object references
- Remove ID-based lookups
- Test all affected services

**Complexity:** High - Touches parsers, DTOs, services

### Phase 7C: Composite ID Generation Elimination (MEDIUM PRIORITY)

**Goal:** Generate IDs independently, not from entity IDs

**Files to refactor:**
1. DeliveryJobCatalog.cs: Line 85 - Generate stable ID without location IDs
2. LocationActionCatalog.cs: Lines 47, 67, 91, 172 - Generate action IDs without location IDs
3. SceneTemplateParser.cs: Lines 1017, 1030, 1043, 1058 - Choice template IDs don't depend on situation ID
4. AStorySceneArchetypeCatalog.cs: Similar pattern throughout

**ID Generation Strategy:**
```csharp
// OLD (WRONG) - embeds entity IDs
Id = $"delivery_{origin.Id}_to_{destination.Id}"

// NEW (CORRECT) - semantic + stable suffix
Id = $"delivery_{origin.Purpose}_{destination.Purpose}_{DateTime.UtcNow.Ticks % 10000}",
// OR use sequential ID
// OR generate from metadata: purpose, type, etc.
```

**Complexity:** Medium - Touches catalogue generation logic

### Phase 7D: Obligation ID Lookup Refactoring (LOW PRIORITY)

**Goal:** Remove repeated ID-based Obligation lookups

**Current Pattern:**
```csharp
Obligation obligation = _gameWorld.Obligations.FirstOrDefault(i => i.Id == obligationId);
// ... repeated 8+ times in ObligationActivity.cs
```

**Solution Options:**
1. Cache obligation references (ObligationActivity receives Obligation, not string ID)
2. Query obligations by categorical properties instead of ID
3. Use obligation object reference throughout call chain

**Complexity:** Medium - Refactor method signatures

---

## UNACCEPTABLE VIOLATIONS SUMMARY

### Critical (Must fix immediately):
1. **Situation ID lookups** - Situation has no ID per design, but code tries to use it
2. **DeliveryJob property IDs** - Should use object references

### High Priority (Phase 7):
1. **LocationAction property IDs** - Should use object references
2. **Composite ID generation** - Couples IDs to entity IDs
3. **Obligation ID comparisons** - Repeated lookups instead of reference passing

### Medium Priority (Phase 8+):
1. **SituationSpawnRules ID properties** - Keep for now (template-related)
2. **Minor logging ID usages** - Low impact, can defer

---

## METRICS

| Metric | Value |
|--------|-------|
| Total .Id sites | 600 |
| Acceptable sites | 479 |
| Violation sites | 121 |
| Clean percentage | 79.8% |
| Files needing changes | ~20 |
| Estimated effort | 40-60 hours |
| Complexity | High (domain + service layer) |
| Risk | Medium (many affected services) |

---

## NEXT STEPS

1. **Immediate:** Address Situation identity blocker (Phase 7A)
2. **Week 1:** Entity reference conversion (Phase 7B)
3. **Week 2:** Composite ID elimination (Phase 7C)
4. **Week 3:** Obligation refactoring (Phase 7D)
5. **Testing:** Full test suite for all modified files

