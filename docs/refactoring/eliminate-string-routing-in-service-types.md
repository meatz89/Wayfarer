# Eliminating String Routing in ServiceType Architecture

## Executive Summary

**What:** Refactor ServiceType system from string-based routing to pure polymorphic behavior.

**Why:** Current implementation violates "no string matching" architecture principle by using strings as routing keys across three layers.

**Goal:** Achieve true polymorphism where ServiceType classes contain substantial scene generation logic, with zero string matching or routing anywhere in the system.

## The Problem: String Routing Antipattern

### Three Layers of String Violations

**Layer 1: ServiceType Returns String**
```csharp
// VIOLATION: Returns string used as routing key
public abstract string GetSituationArchetypeId();

// Implementation
public override string GetSituationArchetypeId() {
    return "service_transaction";  // Magic string
}
```

**Layer 2: SituationTemplate Stores String**
```csharp
// VIOLATION: String property stores routing key
public string ArchetypeId { get; init; }

// Usage
SituationTemplate template = new SituationTemplate {
    ArchetypeId = "service_transaction"  // Magic string stored
};
```

**Layer 3: Catalogue Matches String**
```csharp
// VIOLATION: String switch for routing
public static SituationArchetype GetArchetype(string archetypeId) {
    return archetypeId?.ToLowerInvariant() switch {
        "service_transaction" => CreateServiceTransaction(),
        "negotiation" => CreateNegotiation(),
        // ... more string matching
    };
}
```

### Why This Violates Architecture Principles

1. **String matching forbidden**: Strings should identify, never route behavior
2. **Magic strings**: "service_transaction", "negotiation" are routing keys, not data
3. **Routing logic scattered**: Behavior determined by string matching in multiple places
4. **Not polymorphic**: Enum was replaced with classes, but string routing remains
5. **Violates HIGHLANDER**: One concept (situation archetype) has multiple representations (string AND object)

### Current Flow (BROKEN)

```
JSON "serviceType": "lodging"
  ↓
ServiceTypeCatalogue.GetByIdOrThrow("lodging") → LodgingService instance
  ↓
LodgingService.GetSituationArchetypeId() → returns "service_transaction" STRING
  ↓
SituationTemplate created with ArchetypeId = "service_transaction" STRING
  ↓
Parser calls SituationArchetypeCatalog.GetArchetype("service_transaction")
  ↓
STRING SWITCH matches "service_transaction" → returns archetype OBJECT
  ↓
Generate ChoiceTemplates from archetype object
```

**Problem:** Archetype exists as BOTH string ("service_transaction") AND object (SituationArchetype), violating HIGHLANDER principle.

---

## Current Architecture (As-Is)

### Component Overview

**ServiceType (Abstract Base Class)**
- Represents service types: Lodging, Bathing, Healing, etc.
- Polymorphic classes: LodgingService, HealingService, TrainingService
- Methods:
  - `GetSituationArchetypeId()` - Returns string ❌ VIOLATION
  - `GenerateRewards(tier)` - Returns ChoiceReward object ✓
  - `GenerateMultiSituationArc(...)` - Generates 4-situation structure
  - `GenerateSingleSituation(...)` - Generates standalone situation

**PhysicalServiceType (Intermediate Class)**
- Lodging, Bathing, Healing, Storage services
- Overrides `GetSituationArchetypeId()` to return "service_transaction" ❌
- Implements `GenerateMultiSituationArc()` with 200+ lines of logic ✓

**SituationArchetypeService (Intermediate Class)**
- Negotiation, Investigation, Crisis services
- Overrides `GetSituationArchetypeId()` to return `Id` ❌
- Less complex, single-situation pattern

**SituationArchetype (Data Class)**
- NOT polymorphic, just data structure
- Properties: PrimaryStat, SecondaryStat, CoinCost, ChallengeType
- Defines mechanical 4-choice pattern
- Created by SituationArchetypeCatalog factory methods

**SituationArchetypeCatalog (Static Class)**
- Parse-time ONLY
- `GetArchetype(string)` - STRING SWITCH routing ❌
- 15 factory methods: CreateNegotiation(), CreateConfrontation(), etc.
- Returns SituationArchetype data objects

**SceneArchetypeCatalog (Static Class)**
- Parse-time ONLY
- Calls `serviceType.GetSituationArchetypeId()` to get string ❌
- Creates SituationTemplate with `ArchetypeId` string ❌
- Returns SceneArchetypeDefinition to parser

**SceneTemplateParser**
- `EnrichSituationTemplateFromArchetype()` method
- Reads `SituationTemplate.ArchetypeId` string ❌
- Calls `SituationArchetypeCatalog.GetArchetype(archetypeId)` ❌
- Generates ChoiceTemplates from archetype object
- Returns complete SituationTemplate

### Data Flow Diagram (Current)

```
┌─────────────────────────────────────────────────────────────┐
│ JSON: "serviceType": "lodging"                              │
└───────────────────────┬─────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────────┐
│ ServiceTypeCatalogue                                        │
│ Dictionary<string, ServiceType> lookup ❌ VIOLATION         │
│ Returns: LodgingService instance                            │
└───────────────────────┬─────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────────┐
│ LodgingService.GetSituationArchetypeId()                    │
│ Returns: "service_transaction" STRING ❌ VIOLATION          │
└───────────────────────┬─────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────────┐
│ SceneArchetypeCatalog.GenerateMultiSituationArc()           │
│ Creates SituationTemplate:                                  │
│   ArchetypeId = "service_transaction" ❌ STRING STORED      │
└───────────────────────┬─────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────────┐
│ Parser.EnrichSituationTemplateFromArchetype()               │
│ Reads: template.ArchetypeId string ❌                       │
└───────────────────────┬─────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────────┐
│ SituationArchetypeCatalog.GetArchetype(archetypeId)         │
│ STRING SWITCH on archetypeId ❌ VIOLATION                   │
│ Returns: SituationArchetype object                          │
└───────────────────────┬─────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────────┐
│ Parser generates ChoiceTemplates from archetype             │
│ Returns complete SituationTemplate                          │
└─────────────────────────────────────────────────────────────┘
```

---

## Target Architecture (To-Be)

### Core Principle

**Catalogues generate COMPLETE domain objects at parse time. Parser receives complete objects, no post-processing.**

### Component Changes

**ServiceType (Abstract Base Class)**
- DELETED: `GetSituationArchetypeId()` method ✓
- KEEPS: `GenerateMultiSituationArc(...)` - now generates COMPLETE SituationTemplates
- KEEPS: `GenerateSingleSituation(...)` - now generates COMPLETE SituationTemplates

**PhysicalServiceType**
- Calls SituationArchetypeCatalog directly to get archetype OBJECTS
- Generates ChoiceTemplates from archetype objects
- Returns SituationTemplates with ChoiceTemplates POPULATED

**SituationTemplate**
- DELETED: `ArchetypeId` property ✓
- KEEPS: `ChoiceTemplates` list - ALWAYS populated from catalogue

**SituationArchetypeCatalog**
- Factory methods return SituationArchetype OBJECTS
- NO string parameter methods
- Called directly by ServiceType classes

**SceneTemplateParser**
- DELETED: `EnrichSituationTemplateFromArchetype()` method
- DELETED: `GenerateChoiceTemplatesFromArchetype()` method
- Receives complete SituationTemplates from catalogue, just embeds them

### Data Flow Diagram (Target)

```
┌─────────────────────────────────────────────────────────────┐
│ JSON: "serviceType": "lodging"                              │
└───────────────────────┬─────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────────┐
│ ServiceTypeCatalogue                                        │
│ List<ServiceType>.FirstOrDefault() ✓ NO DICTIONARY         │
│ Returns: LodgingService instance                            │
└───────────────────────┬─────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────────┐
│ PhysicalServiceType.GenerateMultiSituationArc()             │
│ Calls: SituationArchetypeCatalog.GetServiceTransaction()   │
│ Returns: SituationArchetype OBJECT ✓ NO STRING             │
└───────────────────────┬─────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────────┐
│ GenerateChoicesFromArchetype(archetypeObject)               │
│ Uses: archetype.PrimaryStat, archetype.CoinCost, etc.      │
│ Returns: List<ChoiceTemplate> COMPLETE ✓                   │
└───────────────────────┬─────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────────┐
│ Create SituationTemplate:                                   │
│   ChoiceTemplates = completeChoices ✓ POPULATED            │
│   NO ArchetypeId property ✓ DELETED                        │
└───────────────────────┬─────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────────┐
│ Return SceneArchetypeDefinition to Parser                   │
│ Contains: COMPLETE SituationTemplates ✓                    │
└───────────────────────┬─────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────────┐
│ Parser embeds complete objects, NO PROCESSING ✓             │
└─────────────────────────────────────────────────────────────┘
```

---

## Key Architectural Decisions

### Decision 1: Delete `GetSituationArchetypeId()` Method

**Rationale:** This method returns a string used as a routing key, violating the no-string-matching principle.

**Impact:**
- ServiceType classes lose ability to return situation archetype as string
- Forces classes to work with archetype OBJECTS instead
- Eliminates one layer of string routing

**Alternative Considered:** Return SituationArchetype object instead of string
**Why Rejected:** Still requires storing archetype reference, adds unnecessary coupling

### Decision 2: Delete `SituationTemplate.ArchetypeId` Property

**Rationale:** Storing archetype as string violates HIGHLANDER (one concept, one representation). Archetype exists as both string AND object.

**Impact:**
- SituationTemplate no longer stores archetype identifier
- Forces catalogues to generate COMPLETE templates with ChoiceTemplates populated
- Eliminates string storage layer

**Alternative Considered:** Change `string ArchetypeId` to `SituationArchetype Archetype` object
**Why Rejected:** Archetype only needed at parse time, shouldn't persist on template

### Decision 3: Catalogues Generate Complete ChoiceTemplates

**Rationale:** Catalogue Pattern dictates "translation happens ONCE at parse time." Parser should receive complete domain objects.

**Impact:**
- PhysicalServiceType calls SituationArchetypeCatalog directly
- Generates ChoiceTemplates from archetype objects
- Returns SituationTemplates with ChoiceTemplates POPULATED
- Parser becomes thin - just embeds complete objects

**Alternative Considered:** Two-phase generation (catalogue creates template, parser adds choices)
**Why Rejected:** Violates Catalogue Pattern, requires parser to know about archetype structure

### Decision 4: Catalogue-to-Catalogue Imports Acceptable

**Rationale:** Both catalogues are parse-time only. Catalogues calling catalogues for translation is architecturally sound.

**Impact:**
- PhysicalServiceType (and SceneArchetypeCatalog if used) imports SituationArchetypeCatalog
- Direct method calls: `SituationArchetypeCatalog.GetServiceTransaction()` returns object
- Clean separation: both parse-time, neither runtime

**Alternative Considered:** Create abstraction layer between catalogues
**Why Rejected:** Unnecessary complexity, both are parse-time catalogues

### Decision 5: ServiceType Classes Contain Substantial Logic

**Rationale:** Polymorphism means behavior lives in classes, not in switch statements. Each ServiceType should know how to generate its own scene structure.

**Impact:**
- PhysicalServiceType: 200+ lines of scene generation logic
- Not "data holders" - true behavior classes
- Each subclass can override specific generation behavior

**Alternative Considered:** Keep logic in catalogues, use ServiceType as data
**Why Rejected:** That's what we had with enums - defeats purpose of polymorphism

---

## Step-by-Step Refactoring Plan

### Phase 1: Delete String Routing Infrastructure ✓ COMPLETED

**Actions:**
1. Delete `ServiceType.GetSituationArchetypeId()` abstract method ✓
2. Delete `PhysicalServiceType.GetSituationArchetypeId()` override ✓
3. Delete `SituationArchetypeService.GetSituationArchetypeId()` override ✓
4. Delete `TrainingService.GetSituationArchetypeId()` override ✓
5. Delete `SituationTemplate.ArchetypeId` property ✓

**Impact:** Code BROKEN - methods called that no longer exist

**Verification:**
- `grep -r "GetSituationArchetypeId" src/` returns zero results
- `grep -r "\.ArchetypeId" src/` returns zero results (property access)

### Phase 2: Update PhysicalServiceType.GenerateMultiSituationArc()

**Current State:**
```csharp
SituationTemplate serviceSituation = new SituationTemplate {
    ArchetypeId = GetSituationArchetypeId(),  // ❌ DELETED METHOD
    // ... other properties
};
```

**Target State:**
```csharp
// Get archetype OBJECT from catalogue
SituationArchetype serviceArchetype = SituationArchetypeCatalog.GetServiceTransaction();

// Generate complete ChoiceTemplates
List<ChoiceTemplate> serviceChoices = GenerateChoicesFromArchetype(
    serviceArchetype,
    $"{Id}_service",
    tier
);

// Create COMPLETE SituationTemplate
SituationTemplate serviceSituation = new SituationTemplate {
    Id = $"{Id}_service",
    ChoiceTemplates = serviceChoices,  // ✓ POPULATED
    // ... other properties
};
```

**Actions:**
1. Import SituationArchetypeCatalog
2. Add helper method: `GenerateChoicesFromArchetype(archetype, situationId, tier)`
3. Update negotiation situation generation
4. Update service situation generation
5. Remove all `ArchetypeId = ...` assignments

**Files Modified:**
- `src/GameState/ServiceTypes/PhysicalServiceType.cs`

### Phase 3: Update Situation Archetype Object Access

**Current Violation:**
```csharp
protected virtual string DetermineNegotiationArchetype(NPC npc) {
    if (npc.PersonalityType == PersonalityType.DEVOTED)
        return "social_maneuvering";  // ❌ STRING
    return "service_transaction";      // ❌ STRING
}
```

**Target:**
```csharp
protected virtual SituationArchetype DetermineNegotiationArchetype(NPC npc) {
    if (npc.PersonalityType == PersonalityType.DEVOTED)
        return SituationArchetypeCatalog.GetSocialManeuvering();  // ✓ OBJECT
    return SituationArchetypeCatalog.GetServiceTransaction();      // ✓ OBJECT
}
```

**Actions:**
1. Change return type from `string` to `SituationArchetype`
2. Replace string returns with catalogue method calls
3. Update all callers to receive archetype objects

### Phase 4: Update SituationArchetypeService.GenerateSingleSituation()

**Similar pattern to PhysicalServiceType:**
1. Get archetype OBJECT from catalogue using `Id`
2. Generate ChoiceTemplates from archetype object
3. Return complete SituationTemplate

**Files Modified:**
- `src/GameState/ServiceTypes/SituationArchetypeService.cs`

### Phase 5: Add Choice Generation Helper Method

**Location:** PhysicalServiceType (protected method, reusable)

**Purpose:** Generate 4 ChoiceTemplates from SituationArchetype structure

**Signature:**
```csharp
protected List<ChoiceTemplate> GenerateChoicesFromArchetype(
    SituationArchetype archetype,
    string situationId,
    int tier)
```

**Logic:** (Copy from existing Parser.GenerateChoiceTemplatesFromArchetype)
- CHOICE 1: Stat-gated (Primary OR Secondary stat requirement)
- CHOICE 2: Money (Coin cost, no requirements)
- CHOICE 3: Challenge (Resolve cost, starts tactical challenge)
- CHOICE 4: Fallback (Time cost, always available)

### Phase 6: Update SceneTemplateParser

**Delete These Methods:**
1. `EnrichSituationTemplateFromArchetype()` - no longer needed
2. `GenerateChoiceTemplatesFromArchetype()` - moved to ServiceType classes

**Why:** Parser receives COMPLETE SituationTemplates from catalogue. No enrichment needed.

**Files Modified:**
- `src/Content/Parsers/SceneTemplateParser.cs`

**Verification:**
- Parser.ParseSceneTemplate() directly embeds `archetypeDefinition.SituationTemplates`
- No post-processing of situations
- No archetype-related logic in parser

### Phase 7: Fix Dictionary Violation in ServiceTypeCatalogue

**Current Violation:**
```csharp
private static readonly Dictionary<string, ServiceType> _serviceTypes = ...;
```

**Target:**
```csharp
private static readonly List<ServiceType> _allServiceTypes = new List<ServiceType> {
    new LodgingService(),
    new BathingService(),
    // ... all 22 services
};

public static ServiceType GetByIdOrThrow(string id) {
    ServiceType found = _allServiceTypes.FirstOrDefault(
        s => s.Id.Equals(id, StringComparison.OrdinalIgnoreCase)
    );
    if (found == null)
        throw new InvalidDataException($"Unknown serviceType: '{id}'");
    return found;
}
```

**Files Modified:**
- `src/Content/Catalogs/ServiceTypeCatalogue.cs`

### Phase 8: Update SceneArchetypeCatalog (if still used)

**Current:** May still call `serviceType.GetSituationArchetypeId()`

**Target:** All logic moved to ServiceType classes

**Potential Action:** Delete SceneArchetypeCatalog entirely if all logic moved to ServiceType

**Files Modified:**
- `src/Content/Catalogs/SceneArchetypeCatalog.cs` (potentially delete)

### Phase 9: Update SituationArchetypeCatalog Public API

**Current:** `GetArchetype(string archetypeId)` with string switch

**Target:** Remove string parameter method, expose only factory methods

**Actions:**
1. Make `GetArchetype(string)` private or delete entirely
2. Ensure factory methods are public: `GetNegotiation()`, `GetConfrontation()`, etc.
3. ServiceType classes call factory methods directly

**Files Modified:**
- `src/Content/Catalogs/SituationArchetypeCatalog.cs`

### Phase 10: Build and Verify

**Build:**
```bash
cd src && dotnet build
```

**Verification Checklist:**
- [ ] Zero compilation errors
- [ ] `grep -r "GetSituationArchetypeId" src/` returns 0 results
- [ ] `grep -r "\.ArchetypeId" src/` returns 0 results (except comments)
- [ ] `grep -r "Dictionary<string, ServiceType>" src/` returns 0 results
- [ ] ServiceType classes call SituationArchetypeCatalog factory methods
- [ ] SituationTemplates have ChoiceTemplates populated from catalogue
- [ ] Parser has no archetype enrichment logic

---

## Code Flow Comparison

### BEFORE: String Routing (Antipattern)

```
┌────────────────────────────────────────────────────────┐
│ 1. JSON "serviceType": "lodging"                       │
└───────────────────┬────────────────────────────────────┘
                    ↓
┌────────────────────────────────────────────────────────┐
│ 2. ServiceTypeCatalogue.GetByIdOrThrow("lodging")     │
│    Dictionary<string, ServiceType> ❌ VIOLATION       │
│    Returns: LodgingService instance                    │
└───────────────────┬────────────────────────────────────┘
                    ↓
┌────────────────────────────────────────────────────────┐
│ 3. LodgingService inherits from PhysicalServiceType   │
│    GetSituationArchetypeId()                           │
│    Returns: "service_transaction" STRING ❌           │
└───────────────────┬────────────────────────────────────┘
                    ↓
┌────────────────────────────────────────────────────────┐
│ 4. Catalogue creates SituationTemplate                │
│    ArchetypeId = "service_transaction" ❌ STRING      │
│    ChoiceTemplates = [] (empty, populated later)      │
└───────────────────┬────────────────────────────────────┘
                    ↓
┌────────────────────────────────────────────────────────┐
│ 5. Parser.EnrichSituationTemplateFromArchetype()      │
│    Reads: template.ArchetypeId ❌ STRING              │
└───────────────────┬────────────────────────────────────┘
                    ↓
┌────────────────────────────────────────────────────────┐
│ 6. SituationArchetypeCatalog.GetArchetype(string)     │
│    switch (archetypeId.ToLowerInvariant()) ❌         │
│    case "service_transaction": return CreateService() │
└───────────────────┬────────────────────────────────────┘
                    ↓
┌────────────────────────────────────────────────────────┐
│ 7. Parser.GenerateChoiceTemplatesFromArchetype()      │
│    Uses archetype OBJECT to generate 4 choices        │
│    Returns: List<ChoiceTemplate>                      │
└───────────────────┬────────────────────────────────────┘
                    ↓
┌────────────────────────────────────────────────────────┐
│ 8. Parser enriches template                           │
│    template.ChoiceTemplates = generatedChoices        │
│    Returns COMPLETE SituationTemplate                 │
└────────────────────────────────────────────────────────┘

STRING VIOLATIONS: 3 layers (method return, property storage, catalogue routing)
```

### AFTER: Pure Polymorphism (Correct)

```
┌────────────────────────────────────────────────────────┐
│ 1. JSON "serviceType": "lodging"                       │
└───────────────────┬────────────────────────────────────┘
                    ↓
┌────────────────────────────────────────────────────────┐
│ 2. ServiceTypeCatalogue.GetByIdOrThrow("lodging")     │
│    List<ServiceType>.FirstOrDefault() ✓               │
│    Returns: LodgingService instance                    │
└───────────────────┬────────────────────────────────────┘
                    ↓
┌────────────────────────────────────────────────────────┐
│ 3. PhysicalServiceType.GenerateMultiSituationArc()    │
│    Calls: SituationArchetypeCatalog                   │
│              .GetServiceTransaction() ✓               │
│    Returns: SituationArchetype OBJECT ✓               │
└───────────────────┬────────────────────────────────────┘
                    ↓
┌────────────────────────────────────────────────────────┐
│ 4. GenerateChoicesFromArchetype(archetypeObject)      │
│    Uses: archetype.PrimaryStat, .CoinCost, etc. ✓    │
│    Returns: List<ChoiceTemplate> (4 choices) ✓       │
└───────────────────┬────────────────────────────────────┘
                    ↓
┌────────────────────────────────────────────────────────┐
│ 5. Create COMPLETE SituationTemplate                  │
│    ChoiceTemplates = generatedChoices ✓ POPULATED    │
│    NO ArchetypeId property ✓ DELETED                 │
└───────────────────┬────────────────────────────────────┘
                    ↓
┌────────────────────────────────────────────────────────┐
│ 6. Return SceneArchetypeDefinition to Parser          │
│    Contains: COMPLETE SituationTemplates ✓           │
└───────────────────┬────────────────────────────────────┘
                    ↓
┌────────────────────────────────────────────────────────┐
│ 7. Parser embeds complete objects                     │
│    NO enrichment, NO processing ✓                     │
│    sceneTemplate.Situations = archetypeDefinition     │
│                              .SituationTemplates      │
└────────────────────────────────────────────────────────┘

STRING VIOLATIONS: 0 (zero)
POLYMORPHIC BEHAVIOR: ServiceType classes generate complete structures
CATALOGUE PATTERN: Translation happens ONCE at parse time
```

---

## Benefits Analysis

### Benefit 1: Zero String Matching Eliminated

**Before:**
- 3 layers of string routing
- Strings as magic constants: "service_transaction", "negotiation", etc.
- If-statement and switch-based routing

**After:**
- Zero string matching anywhere
- Archetype accessed as OBJECT references
- Polymorphic method calls replace switches

**Measurable:** `grep -r "switch.*ToLowerInvariant" src/` returns 0 results

### Benefit 2: True Polymorphism Achieved

**Before:**
- ServiceType classes were data holders with 6-15 lines each
- Logic scattered in SceneArchetypeCatalog (200+ lines)
- Enum replaced with classes, but behavior still external

**After:**
- ServiceType classes contain substantial logic (200+ lines)
- PhysicalServiceType.GenerateMultiSituationArc() encapsulates complete scene generation
- Each class knows its own behavior, not routed externally

**Measurable:** Lines of code in ServiceType classes vs catalogues

### Benefit 3: Extensibility Without Modification

**Before:**
- Adding new service type requires:
  1. New class (small)
  2. Modify SceneArchetypeCatalog switch statements
  3. Modify SituationArchetypeCatalog if new archetype needed
  4. Multiple files touched

**After:**
- Adding new service type requires:
  1. New class (large, contains all behavior)
  2. Add to ServiceTypeCatalogue list
  3. Zero catalogue modifications

**Principle:** Open-Closed Principle (OCP) - open for extension, closed for modification

### Benefit 4: Parse-Time Catalogue Pattern Correct

**Before:**
- Partial translation: Catalogue creates partial templates
- Parser completes translation (enrichment step)
- Two-phase translation violates Catalogue Pattern

**After:**
- Complete translation: Catalogue creates complete templates
- Parser receives complete objects, zero processing
- True Catalogue Pattern: "Translation happens ONCE at parse time"

**Measurable:** Parser has zero archetype-related methods

### Benefit 5: HIGHLANDER Principle Enforced

**Before:**
- Situation archetype exists as BOTH:
  - String: "service_transaction" (ArchetypeId property)
  - Object: SituationArchetype instance
- Violates "one concept, one representation"

**After:**
- Situation archetype exists ONLY as object reference
- Used at parse time, doesn't persist on template
- One concept, one representation ✓

### Benefit 6: Single Responsibility Clear

**Before:**
- Catalogue: Partial scene generation
- Parser: Complete scene generation (enrichment)
- Responsibility split across two layers

**After:**
- ServiceType: Complete scene generation (all logic)
- Catalogue: Archetype factory (data objects)
- Parser: Object embedding (zero logic)
- Each layer has single clear responsibility

---

## Risks and Mitigations

### Risk 1: Code Breaks During Refactoring

**Description:** Deleting methods/properties breaks existing code. Large surface area of changes.

**Impact:** HIGH - Code won't compile mid-refactoring

**Mitigation:**
1. Follow step-by-step plan in order
2. Don't skip phases
3. Verify each phase before moving to next
4. Use compiler errors as checklist (fix all before proceeding)

**Recovery:** Git revert to last working state if needed

### Risk 2: Catalogue-to-Catalogue Coupling

**Description:** PhysicalServiceType imports SituationArchetypeCatalog. Catalogues calling catalogues could be seen as coupling.

**Impact:** MEDIUM - Architectural concern

**Mitigation:**
1. Both catalogues are parse-time ONLY (never runtime)
2. Parse-time catalogues calling parse-time catalogues is acceptable
3. Follows Catalogue Pattern: translation helpers can use other catalogues
4. Document this is correct architecture

**Recovery:** If proven problematic, create abstraction layer

### Risk 3: Large Methods in ServiceType Classes

**Description:** PhysicalServiceType.GenerateMultiSituationArc() will be 200+ lines. Could be seen as code smell.

**Impact:** LOW - Readability concern

**Mitigation:**
1. This is behavior encapsulation, not bloat
2. Logic belongs in the class that owns it
3. Method does ONE thing: generate complete scene arc
4. Can extract helper methods if needed (DRY principle)

**Justification:** 200 lines of behavior in the correct place beats 20 lines of routing logic

### Risk 4: Missing String References

**Description:** Might miss some string-based archetype references during refactoring

**Impact:** MEDIUM - Compilation errors or runtime bugs

**Mitigation:**
1. Use grep to find all references: `grep -r "ArchetypeId" src/`
2. Use grep to find string matches: `grep -r "service_transaction" src/`
3. Compiler will catch method calls to deleted methods
4. Verification checklist at end of each phase

**Recovery:** Compiler errors guide remaining fixes

---

## Files Impacted

### Files to MODIFY

**ServiceType Classes:**
- `src/GameState/ServiceTypes/ServiceType.cs` - Delete abstract method ✓
- `src/GameState/ServiceTypes/PhysicalServiceType.cs` - Delete override, add logic ✓
- `src/GameState/ServiceTypes/SituationArchetypeService.cs` - Delete override, add logic ✓
- `src/GameState/ServiceTypes/TrainingService.cs` - Delete override ✓

**Domain Entities:**
- `src/GameState/SituationTemplate.cs` - Delete ArchetypeId property ✓

**Catalogues:**
- `src/Content/Catalogs/ServiceTypeCatalogue.cs` - Replace Dictionary with List
- `src/Content/Catalogs/SituationArchetypeCatalog.cs` - Remove string parameter method

**Parser:**
- `src/Content/Parsers/SceneTemplateParser.cs` - Delete enrichment methods

### Files to POTENTIALLY DELETE

- `src/Content/Catalogs/SceneArchetypeCatalog.cs` - If all logic moved to ServiceType classes

### Files NOT IMPACTED (Read-only from JSON)

- All JSON files (service type values remain strings in JSON)
- DTOs (ServiceTypeDTO, SceneTemplateDTO remain unchanged)

---

## Testing Strategy

### Build Verification

```bash
cd src && dotnet build
```

**Success criteria:**
- Zero compilation errors
- Zero warnings related to deleted methods/properties

### String Routing Elimination Verification

```bash
# Should return 0 results (method deleted)
grep -r "GetSituationArchetypeId" src/

# Should return 0 results (property deleted, except comments)
grep -r "\.ArchetypeId" src/

# Should return 0 results (Dictionary violation fixed)
grep -r "Dictionary<string, ServiceType>" src/

# Should return 0 results (string switch eliminated)
grep -r "switch.*ToLowerInvariant.*archetype" src/
```

### Polymorphism Verification

**Check ServiceType classes contain substantial logic:**
```bash
wc -l src/GameState/ServiceTypes/PhysicalServiceType.cs
# Expected: 200+ lines

wc -l src/GameState/ServiceTypes/SituationArchetypeService.cs
# Expected: 50+ lines
```

**Check catalogues are thin:**
```bash
wc -l src/Content/Catalogs/ServiceTypeCatalogue.cs
# Expected: < 50 lines (just lookup logic)
```

### Catalogue Pattern Verification

**Parser should have no archetype logic:**
```bash
grep -r "SituationArchetypeCatalog" src/Content/Parsers/
# Expected: 0 results (parser doesn't call catalogue)

grep -r "EnrichSituation" src/Content/Parsers/
# Expected: 0 results (method deleted)
```

### Runtime Verification (Manual)

1. Run game
2. Trigger lodging service scene
3. Verify 4 choices appear correctly
4. Verify negotiation situation has correct archetypes
5. Verify no runtime errors related to missing ArchetypeId

---

## Conclusion

This refactoring eliminates the string routing antipattern by:

1. **Deleting string-based routing infrastructure** (GetSituationArchetypeId, ArchetypeId property)
2. **Moving substantial logic into ServiceType classes** (true polymorphism)
3. **Using object references instead of strings** (SituationArchetype objects)
4. **Catalogues generating complete domain objects** (Catalogue Pattern compliance)
5. **Parser receiving complete objects, zero processing** (single responsibility)

The result is **pure polymorphic architecture** with **zero string matching** anywhere in the system.

**Next Steps:** Execute refactoring plan phase by phase, verifying after each phase.
