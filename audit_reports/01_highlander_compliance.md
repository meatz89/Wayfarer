# HIGHLANDER & Single Source of Truth Compliance Audit

**Audit Date:** 2025-11-29
**Auditor:** Claude Code Agent
**Scope:** Complete codebase compliance with arc42 §8.1, §8.15, ADR-004

---

## Executive Summary

**Status:** AUDIT COMPLETE - 3 CRITICAL VIOLATIONS, 2 CONCERNS

This audit verifies compliance with:
- HIGHLANDER Principle: One canonical storage location per game state
- Single Source of Truth: GameWorld is sole state container
- Separated Responsibilities: Resolver/Creator/Orchestrator boundaries

**Overall Assessment:** The codebase demonstrates strong HIGHLANDER compliance in core domain entities (Player, NPC, Location, RouteOption) with NO ID properties. GameWorld is correctly implemented as the sole state container with zero external dependencies. However, several entities claiming to be "templates" violate the immutability requirement by storing mutable state alongside their Id properties.

---

## Violations Found

### CRITICAL VIOLATIONS

#### 1. EmergencySituation: Mutable State with Id Property
**File:** `/home/user/Wayfarer/src/GameState/EmergencySituation.cs`
**Lines:** 8, 26-28

**Issue:** EmergencySituation has an Id property (line 8) with comment claiming it's a "template (immutable archetype)", BUT it also has mutable state properties:
- `IsTriggered` (line 26)
- `IsResolved` (line 27)
- `TriggeredAtSegment` (line 28)

**Why This Violates HIGHLANDER:**
Templates are immutable archetypes that can have IDs. Game state entities are mutable instances that cannot have IDs. EmergencySituation is trying to be both. If it has state (IsTriggered, IsResolved), it's NOT a template - it's a game state entity that VIOLATES the no-ID rule.

**Evidence:**
```csharp
public class EmergencySituation
{
    // ADR-007: Id property RESTORED - Templates (immutable archetypes) ARE allowed to have IDs
    public string Id { get; set; }  // ← Claims to be template
    // ...
    // State
    public bool IsTriggered { get; set; }  // ← BUT HAS MUTABLE STATE
    public bool IsResolved { get; set; }
    public int? TriggeredAtSegment { get; set; }
}
```

**Impact:** SEVERE - Violates fundamental HIGHLANDER principle. Creates ambiguity about whether Id or object reference is source of truth.

---

#### 2. ObservationScene: Mutable State with Id Property
**File:** `/home/user/Wayfarer/src/GameState/ObservationScene.cs`
**Lines:** 10, 25, 27

**Issue:** ObservationScene has an Id property (line 10) claiming to be a "template", BUT has mutable state:
- `IsCompleted` (line 25)
- `ExaminedPoints` (line 27) - tracks which points player has examined

**Why This Violates HIGHLANDER:**
Same pattern as EmergencySituation. Cannot be both immutable template (with Id) and mutable game state (with completion tracking).

**Evidence:**
```csharp
public class ObservationScene
{
    // ADR-007: Id property RESTORED - Templates (immutable archetypes) ARE allowed to have IDs
    public string Id { get; set; }  // ← Claims to be template
    // ...
    // Completion state
    public bool IsCompleted { get; set; }  // ← BUT HAS MUTABLE STATE
    public List<ExaminationPoint> ExaminedPoints { get; set; } = new List<ExaminationPoint>();
}
```

**Impact:** SEVERE - Same violation as EmergencySituation.

---

#### 3. ObligationActivity Service: Stores Game State
**File:** `/home/user/Wayfarer/src/Services/ObligationActivity.cs`
**Lines:** 4, 14-18

**Issue:** ObligationActivity claims to be "STATE-LESS: All state lives in GameWorld.ObligationJournal" (line 4), BUT stores pending results as private fields:
- `_pendingDiscoveryResult` (line 14)
- `_pendingActivationResult` (line 15)
- `_pendingProgressResult` (line 16)
- `_pendingCompleteResult` (line 17)
- `_pendingIntroResult` (line 18)

**Why This Violates Single Source of Truth:**
Per arc42 §8.15, services must be stateless - they contain logic, not state. Game state belongs ONLY in GameWorld. These pending results are game state (they affect UI display and game flow) but are stored in the service instead of GameWorld.

**Evidence:**
```csharp
/// <summary>
/// STATE-LESS: All state lives in GameWorld.ObligationJournal  // ← CLAIMS stateless
/// </summary>
public class ObligationActivity
{
    private ObligationDiscoveryResult _pendingDiscoveryResult;  // ← BUT STORES STATE
    private ObligationActivationResult _pendingActivationResult;
    private ObligationProgressResult _pendingProgressResult;
    private ObligationCompleteResult _pendingCompleteResult;
    private ObligationIntroResult _pendingIntroResult;
```

**Impact:** MODERATE-SEVERE - Violates service statelessness principle. Creates parallel state outside GameWorld.

---

### MODERATE VIOLATIONS
(None found - violations are either critical or acceptable)

### MINOR VIOLATIONS
(None found)

---

## Concerns/Questions

### 1. RestOption.Id Property
**File:** `/home/user/Wayfarer/src/GameState/RestOption.cs`
**Line:** 13

**Issue:** RestOption has `public string Id { get; internal set; }` but appears to have state properties like `IsAvailable` (line 8).

**Question:** Is RestOption a template (immutable archetype) or a game state entity?
- If template → IsAvailable shouldn't exist (templates are immutable)
- If game state → Id property violates HIGHLANDER

**Recommendation:** Investigate RestOption usage to determine correct classification.

---

### 2. LoadingStateService Stores UI State
**File:** `/home/user/Wayfarer/src/Services/LoadingStateService.cs`
**Lines:** 6-8

**Issue:** LoadingStateService stores UI presentation state:
- `IsLoading` (line 6)
- `LoadingMessage` (line 7)
- `Progress` (line 8)

**Analysis:** This is NOT game state - it's pure UI/presentation state. The service statelessness principle may apply only to game state, not UI state. However, this creates a gray area.

**Question:** Do services need to be stateless for ALL state, or only GAME state?

**Recommendation:** Clarify in arc42 whether UI services can store presentation state.

---

## Verified Compliant

### Core Domain Entities: NO ID Properties ✓
**Files Checked:**
- `/home/user/Wayfarer/src/GameState/Player.cs` - Line 1-100: NO ID property, uses Name as natural key
- `/home/user/Wayfarer/src/GameState/NPC.cs` - Line 3: "// HIGHLANDER: Name is natural key, NO Id property"
- `/home/user/Wayfarer/src/Content/Location.cs` - Line 3: "// HIGHLANDER: Name is natural key, NO Id property"

**Verification:** Core domain entities properly use object references without ID properties. All three entities explicitly document HIGHLANDER compliance.

---

### GameWorld: Zero External Dependencies ✓
**File:** `/home/user/Wayfarer/src/GameState/GameWorld.cs`
**Line:** 338

**Evidence:**
```csharp
public GameWorld()
{
    if (GameInstanceId == Guid.Empty) GameInstanceId = Guid.NewGuid();
    Player = new Player();
    // GameWorld has NO dependencies and creates NO managers
    StreamingContentState = new StreamingContentState();
}
```

**Verification:** GameWorld constructor takes zero parameters and creates no service dependencies. All state lives in GameWorld collections.

---

### EntityResolver: Pure Find Service ✓
**File:** `/home/user/Wayfarer/src/Content/EntityResolver.cs`
**Lines:** 1-11, entire file

**Evidence:**
```csharp
/// <summary>
/// System 4: Entity Resolver - FIND ONLY
/// Pure query service - searches for entities matching categorical filters.
/// Returns null if not found (caller decides whether to create or throw).
/// NO creation logic - that belongs to PackageLoader (HIGHLANDER principle).
```

**Verification:** EntityResolver contains ONLY Find* methods. No Create, Update, or Delete operations. Returns null when entity not found, leaving orchestration to caller. Properly separated from creation logic.

---

### Template IDs: Correctly Used ✓
**Files Checked:**
- `/home/user/Wayfarer/src/GameState/SceneTemplate.cs` - Line 15: Immutable template with Id property
- `/home/user/Wayfarer/src/GameState/PhysicalChallengeDeck.cs` - Line 9: Immutable deck definition with Id
- `/home/user/Wayfarer/src/GameState/MentalChallengeDeck.cs` - Line 9: Immutable deck definition with Id

**Verification:** These are genuinely immutable templates (archetypes) with no mutable state properties. Id usage is compliant per CLAUDE.md exception for templates.

---

### No Legacy ID Properties Found ✓
**Search Performed:** `grep -r "CurrentLocationId|ActiveSceneId|PlayerLocationId"`
**Result:** No matches found

**Verification:** No legacy ID properties exist alongside object references. Clean migration to object-reference-only architecture.

---

### GameWorld as Sole State Container ✓
**File:** `/home/user/Wayfarer/src/GameState/GameWorld.cs`
**Lines:** 1-1072

**Verification:** All game state stored in GameWorld collections:
- NPCs (line 10)
- Locations (line 9)
- Venues (line 8)
- Scenes (line 184)
- Obligations (line 85)
- Routes (line 216)
- Player (private field, line 33, accessor line 344)

No state stored in external managers or services (except violation #3 above).

---

## Recommendations

### Fix Critical Violations

#### 1. Split EmergencySituation into Template + Instance
**Current:** Single class trying to be both template and state
**Recommended:**
```csharp
// Immutable template (can have Id)
public class EmergencySituationTemplate
{
    public string Id { get; init; }  // immutable archetype
    public string Name { get; init; }
    public string Description { get; init; }
    public List<EmergencyResponse> Responses { get; init; }
    // ... all immutable properties
}

// Mutable game state (NO Id)
public class ActiveEmergency
{
    public EmergencySituationTemplate Template { get; set; }  // object reference
    public bool IsResolved { get; set; }
    public int? TriggeredAtSegment { get; set; }
    // ... all mutable state
}
```

**Pattern:** Template-Instance separation (same as SceneTemplate → Scene)

---

#### 2. Split ObservationScene into Template + Instance
**Apply same pattern as EmergencySituation:**
- ObservationSceneTemplate (immutable, has Id)
- ActiveObservationScene (mutable, NO Id, references Template)

---

#### 3. Move Pending Results to GameWorld
**Current:** ObligationActivity stores _pending*Result fields
**Recommended:** Move to GameWorld as collections
```csharp
// In GameWorld.cs
public ObligationDiscoveryResult PendingDiscoveryResult { get; set; }
public ObligationActivationResult PendingActivationResult { get; set; }
// ... etc
```

**Pattern:** Service becomes stateless, all state lives in GameWorld

---

### Investigate RestOption
1. Determine if RestOption is template or game state
2. If template → remove IsAvailable (make immutable)
3. If game state → remove Id property, split into Template + Instance pattern

---

### Clarify UI Service State Policy
Document in arc42 §8.15 whether services can store UI/presentation state (like LoadingStateService) or if ALL services must be stateless.

---

## Audit Log

### Phase 1: Initial Structure
- Report created
- Beginning systematic audit

### Phase 2: Core Entity Audit (Complete)
- Searched for `*Id` properties in domain entities
- Verified Player, NPC, Location have NO ID properties
- Found EmergencySituation, ObservationScene with Id + state (violations)
- Found RestOption with Id (needs investigation)
- Verified template entities (SceneTemplate, challenge decks) correctly use Ids

### Phase 3: GameWorld & Service Audit (Complete)
- Verified GameWorld has zero external dependencies (line 338)
- Verified GameWorld is sole state container
- Found ObligationActivity stores state (violation)
- Found LoadingStateService stores UI state (concern)
- Verified other services are stateless (NPCService, DevModeService)

### Phase 4: Separation of Responsibilities (Complete)
- Verified EntityResolver is pure Find service (no Create)
- No EntityCreator class found (creation handled by PackageLoader)
- SceneInstantiator found as orchestrator (coordinates find-or-create)
- Comments mention "FindOrCreate pattern" but no such method exists (pattern, not implementation)

### Phase 5: Legacy ID Search (Complete)
- Searched for CurrentLocationId, ActiveSceneId, PlayerLocationId
- NO matches found - clean migration complete

### Phase 6: Final Review (Complete)
- Compiled all findings
- Categorized violations by severity
- Generated recommendations
- Audit complete
