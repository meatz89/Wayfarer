# Parse-Time Translation & Catalogue Pattern Compliance Audit

## Status: COMPLETE ✅

**Audit Date:** 2025-11-29
**Auditor:** Claude (Sonnet 4.5)
**Overall Result:** MOSTLY COMPLIANT with 3 documented exceptions

## Principles Being Checked

### From arc42/08 §8.2 Catalogue Pattern
- Categorical properties translated to concrete values at PARSE-TIME ONLY
- Translation happens ONCE at startup
- Runtime uses ONLY concrete values
- NO runtime catalogue lookups

### From ADR-002
- Authors write categories (Friendly, Premium, Hostile)
- Catalogues apply universal formulas
- Entities contain only concrete values after parsing

### FORBIDDEN:
- Runtime translation
- String-based property matching at runtime
- Catalogue lookups during gameplay

## Methodology

1. Search for all *Catalog*.cs or *Catalogue*.cs files
2. Identify parser files and verify they use catalogues
3. Check domain entities for concrete vs categorical properties
4. Search for runtime catalogue access in services/facades
5. Trace JSON → DTO → Entity flow for key entities
6. Verify difficulty scaling happens at parse-time

## Findings

### Phase 1: Catalogue Discovery

**Found 13 Catalogue Classes:**
1. `/home/user/Wayfarer/src/Content/Catalogs/ConversationCatalog.cs`
2. `/home/user/Wayfarer/src/Content/Catalogs/DeliveryJobCatalog.cs`
3. `/home/user/Wayfarer/src/Content/Catalogs/DependentResourceCatalog.cs`
4. `/home/user/Wayfarer/src/Content/Catalogs/EmergencyCatalog.cs`
5. `/home/user/Wayfarer/src/Content/Catalogs/LocationActionCatalog.cs`
6. `/home/user/Wayfarer/src/Content/Catalogs/MentalCardEffectCatalog.cs`
7. `/home/user/Wayfarer/src/Content/Catalogs/ObservationCatalog.cs`
8. `/home/user/Wayfarer/src/Content/Catalogs/PhysicalCardEffectCatalog.cs`
9. `/home/user/Wayfarer/src/Content/Catalogs/PlayerActionCatalog.cs`
10. `/home/user/Wayfarer/src/Content/Catalogs/SceneArchetypeCatalog.cs`
11. `/home/user/Wayfarer/src/Content/Catalogs/SituationArchetypeCatalog.cs`
12. `/home/user/Wayfarer/src/Content/Catalogs/SocialCardEffectCatalog.cs`
13. `/home/user/Wayfarer/src/Content/Catalogs/StateClearConditionsCatalog.cs`

**Catalogue Documentation:**
- All catalogues have explicit "PARSE-TIME ONLY" warnings in headers
- Most documented as "Called ONLY from Parser"
- Example: `LocationActionCatalog` (lines 1-8): "Called by Parser ONLY - runtime never touches this"
- Example: `DependentResourceCatalog` (lines 2-16): "NEVER called from facades, managers, or runtime code"

### Phase 2: Parse-Time Translation Verification

**✅ COMPLIANT - Most Catalogues Used Only at Parse-Time:**

1. **LocationActionCatalog.GenerateActionsForLocation()**
   - Called from: `LocationParser` (parse-time)
   - Translates: `LocationRole` + `LocationPurpose` → Concrete `LocationAction` entities
   - Result: Actions with concrete `Costs` (int values) and `Rewards` (int values)

2. **PlayerActionCatalog.GenerateUniversalActions()**
   - Called from: `PlayerActionParser` (parse-time)
   - Generates: Universal player actions with concrete costs

3. **ConversationCatalog**
   - Methods: `GetFocusCostForComplexity()`, `GetTimeCostForComplexity()`, `GetRelationshipDelta()`
   - Translates: Categorical enums → Concrete int values
   - Used by: Parsers at parse-time

4. **SceneArchetypeCatalog.Generate()**
   - Called from: `SceneTemplateParser` via `SceneGenerationFacade` (parse-time)
   - Generates: Complete scene structures with situation templates
   - Delegates to: `SituationArchetypeCatalog` for choice generation

5. **SituationArchetypeCatalog.GenerateChoiceTemplates()**
   - Called from: `SceneArchetypeCatalog` (parse-time)
   - Generates: 4-choice patterns with concrete stat thresholds and coin costs
   - Universal scaling: `PowerDynamic` (0.6x/1.0x/1.4x) and `Quality` (0.6x/1.0x/1.6x/2.4x)

6. **DependentResourceCatalog.GenerateForActivity()**
   - Called from: `SceneArchetypeCatalog` (parse-time)
   - Translates: `ServiceActivityType` enum → `PlacementFilter` categorical properties
   - Result: Filter used by `EntityResolver` at spawn-time (find or create)

### Phase 3: Runtime Violations Discovered

**⚠️ VIOLATION #1: Runtime Catalogue Call in PackageLoader**

**Location:** `/home/user/Wayfarer/src/Content/PackageLoader.cs:84`

**Code:**
```csharp
List<LocationAction> newActions = LocationActionCatalog.RegenerateIntraVenueActionsForNewLocation(location, _gameWorld.Locations);
```

**Context:**
- Called from: `PackageLoader.CreateSingleLocation()`
- Trigger: When scene creates dependent location at runtime via `EntityResolver.FindOrCreateLocation()`
- Purpose: Generate movement actions between new location and adjacent locations in same venue

**Assessment:**
- **Documented as intentional**: Method has public visibility and comment "PUBLIC: Regenerate intra-venue movement actions when a location is created at runtime"
- **Architectural justification**: Movement actions depend on spatial adjacency - can't be known until location is placed
- **Still a violation**: Catalogue accessed during gameplay, not at parse-time
- **Recommendation**: Evaluate if this violates principle or is acceptable exception for spatial relationships

**⚠️ VIOLATION #2: Runtime Catalogue Query in ProceduralAStoryService**

**Location:** `/home/user/Wayfarer/src/Subsystems/ProceduralContent/ProceduralAStoryService.cs:106`

**Code:**
```csharp
List<SceneArchetypeType> candidateArchetypes =
    SceneArchetypeCatalog.GetArchetypesForCategory(categoryKey);
```

**Context:**
- Called from: `ProceduralAStoryService.SelectArchetype()` at runtime
- Trigger: When A-story scene completes and next scene needs generation
- Purpose: Query catalogue for available archetypes to select from

**Assessment:**
- **Hybrid pattern**: Service calls catalogue to get metadata, then generates DTO and feeds back through `PackageLoader`
- **Documented**: Comment says "DYNAMIC CATALOG QUERY (HIGHLANDER - single source of truth)"
- **Rationale**: "Prevents drift between catalog implementation and procedural selection"
- **Still a violation**: Catalogue accessed during gameplay for runtime decision-making
- **Mitigation**: Result feeds back through parse pipeline (`PackageLoader.LoadDynamicPackageFromJson`)
- **Recommendation**: Consider pre-generating archetype metadata at parse-time

**⚠️ VIOLATION #3: Runtime Scene Generation via Facade**

**Location:** `/home/user/Wayfarer/src/Subsystems/ProceduralContent/SceneGenerationFacade.cs:55`

**Code:**
```csharp
SceneArchetypeDefinition definition = SceneArchetypeCatalog.Generate(archetypeType, tier, context);
```

**Context:**
- Facade calls: `SceneArchetypeCatalog.Generate()`
- Called from: Both parse-time (via parsers) AND runtime (via dynamic package generation)
- Comment admits: "Called at parse time (or via dynamic package generation)"

**Assessment:**
- **Dual-purpose facade**: Same code path used for both parse-time and runtime
- **Runtime usage**: Dynamic package generation for procedural A-story scenes
- **Violation**: Catalogue accessed during gameplay through facade abstraction
- **Recommendation**: Split into parse-time and runtime paths, or accept as intentional design

### Phase 4: Entity Concrete Values Verification

**✅ COMPLIANT - Entities Store Only Concrete Values**

**LocationAction Entity:**
- `Costs`: `ActionCosts` with int properties (`Coins`, `Focus`, `Stamina`, `Health`)
- `Rewards`: `ActionRewards` with int properties (`CoinReward`, `HealthRecovery`, `FocusRecovery`, `StaminaRecovery`)
- No category properties - all concrete int values after parsing

**ActionCosts Class** (`/home/user/Wayfarer/src/GameState/ActionCosts.cs`):
```csharp
public int Coins { get; set; }
public int Focus { get; set; }
public int Stamina { get; set; }
public int Health { get; set; }
```
**Comment:** "These are concrete values calculated at parse-time from categorical JSON properties"

**ActionRewards Class** (`/home/user/Wayfarer/src/GameState/ActionRewards.cs`):
```csharp
public int CoinReward { get; set; }
public int HealthRecovery { get; set; }
public int FocusRecovery { get; set; }
public int StaminaRecovery { get; set; }
```
**Comment:** "These are concrete values calculated at parse-time from categorical JSON properties"

**NPC Entity:**
- `Level`: int (1-5)
- `Tier`: int (1-5)
- `ConversationDifficulty`: int (1-3)
- `BondStrength`: int (0-30)
- `RelationshipFlow`: int (0-24)
- All concrete values, no category properties requiring catalogue lookup

**Location Entity:**
- `Tier`: int
- `FlowModifier`: int
- `Difficulty`: int
- `Familiarity`: int
- Categorical properties (`Privacy`, `Safety`, `Activity`, `Purpose`) are enums used for filtering, not runtime translation

### Phase 5: Parsing Flow Analysis

**✅ COMPLIANT - Clear JSON → DTO → Catalogue → Entity Flow**

**Example: LocationAction Parsing**

1. **JSON**: Author writes categorical properties
   ```json
   {
     "role": "Connective",
     "purpose": "Commerce"
   }
   ```

2. **DTO**: Parser reads into LocationDTO
   ```csharp
   LocationDTO { Role = "Connective", Purpose = "Commerce" }
   ```

3. **Catalogue Translation**: LocationParser calls catalogue
   ```csharp
   List<LocationAction> actions = LocationActionCatalog.GenerateActionsForLocation(location, allLocations);
   ```

4. **Entity Result**: Concrete LocationAction entities created
   ```csharp
   LocationAction {
     Name = "Travel to Another Location",
     Costs = new ActionCosts(), // All int values
     Rewards = new ActionRewards() // All int values
   }
   ```

5. **Storage**: Actions added to `GameWorld.LocationActions`

6. **Runtime**: Services query `GameWorld.LocationActions`, NO catalogue calls

**Example: Conversation Cost Translation**

1. **JSON**: Author writes `"complexity": "Thoughtful"`
2. **DTO**: `ResponseComplexity.Thoughtful` enum
3. **Catalogue**: `ConversationCatalog.GetFocusCostForComplexity(Thoughtful)` → returns `2`
4. **Entity**: ChoiceTemplate stores `FocusCost = 2` (concrete int)
5. **Runtime**: Executor uses concrete value, no catalogue lookup

### Phase 6: Difficulty Scaling Verification

**✅ COMPLIANT - Scaling Happens at Parse-Time with Universal Properties**

**SituationArchetypeCatalog Universal Scaling** (lines 779-818):

```csharp
// Scale stat threshold by PowerDynamic
scaledStatThreshold = context.Power switch
{
    PowerDynamic.Dominant => (int)(archetype.StatThreshold * 0.6),    // Easier
    PowerDynamic.Equal => archetype.StatThreshold,                     // Baseline
    PowerDynamic.Submissive => (int)(archetype.StatThreshold * 1.4),  // Harder
    _ => archetype.StatThreshold
};

// Scale coin cost by Quality
scaledCoinCost = context.Quality switch
{
    Quality.Basic => (int)(archetype.CoinCost * 0.6),    // Cheaper
    Quality.Standard => archetype.CoinCost,              // Baseline
    Quality.Premium => (int)(archetype.CoinCost * 1.6),  // Expensive
    Quality.Luxury => (int)(archetype.CoinCost * 2.4),   // Very expensive
    _ => archetype.CoinCost
};
```

**Process:**
1. Parser extracts universal properties from entities (NPC demeanor, location quality)
2. Creates `GenerationContext` with categorical properties
3. Calls catalogue with context
4. Catalogue applies formulas to base values
5. Returns scaled concrete values
6. Parser stores concrete values in entities
7. Runtime uses concrete values, never rescales

**Result:** All difficulty scaling happens ONCE at parse-time. Runtime never calculates multipliers.

## Summary

### COMPLIANT AREAS ✅

1. **Catalogue Structure**: All 13 catalogues properly documented as parse-time only
2. **Entity Values**: All entities store concrete int values, no categorical properties requiring runtime translation
3. **Parsing Flow**: Clear JSON → DTO → Catalogue → Entity → GameWorld pipeline
4. **Difficulty Scaling**: Universal property scaling happens at parse-time with concrete results
5. **Most Catalogue Usage**: Majority of catalogue calls happen only from parsers

### VIOLATIONS ⚠️

1. **LocationActionCatalog.RegenerateIntraVenueActionsForNewLocation**: Called at runtime from PackageLoader when scenes create dependent locations
2. **SceneArchetypeCatalog.GetArchetypesForCategory**: Called at runtime from ProceduralAStoryService for archetype selection
3. **SceneGenerationFacade**: Dual-purpose facade used for both parse-time and runtime generation

### RECOMMENDATIONS

1. **Evaluate Runtime Catalogue Exceptions**: Determine if spatial movement generation and procedural archetype selection are acceptable exceptions to parse-time-only rule
2. **Document Intentional Violations**: If exceptions are accepted, add explicit documentation explaining why they don't violate the spirit of the principle
3. **Consider Pre-Generation**: Could archetype metadata be extracted at parse-time and stored in GameWorld to avoid runtime catalogue queries?
4. **Split Facade Paths**: Consider separating parse-time and runtime generation paths in SceneGenerationFacade for clarity

## Remaining TODOs
- [x] Find all catalogue classes
- [x] Verify parse-time-only usage
- [x] Check entity concrete values
- [x] Search for runtime violations
- [x] Trace parsing flow
- [x] Verify difficulty scaling
- [ ] Review violations with architect
- [ ] Document accepted exceptions vs true violations
- [ ] Create remediation plan if needed
