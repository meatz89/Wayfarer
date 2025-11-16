# ID ELIMINATION REFACTORING - EXECUTIVE SUMMARY

## OBJECTIVE

Complete elimination of ALL ID properties and ID-based lookups from the Wayfarer codebase. HIGHLANDER enforcement: **Object references ONLY**.

---

## SCOPE METRICS

| Metric | Count |
|--------|-------|
| **Total C# files in project** | 655 |
| **Files with Id properties** | 74 |
| **Files with `.Id ==` comparisons** | 70 (214 occurrences) |
| **GetLocation/GetNPC/GetRoute calls** | 20 files (57 calls) |
| **Facade methods with Id parameters** | 31 files (138 methods) |
| **UI handlers with Id parameters** | 5 files (11 handlers) |
| **GetById methods to delete** | 5 methods |
| **Estimated files to modify** | **100-120 files** |
| **Estimated individual changes** | **500-1000 changes** |

---

## STRATEGY

**Clean break refactoring with NO compatibility layers.**

1. Delete all IDs simultaneously (intentionally breaks compilation)
2. Fix compilation errors in strict dependency order
3. No hybrid `GetByIdOrName` methods
4. No "deprecated" Id properties
5. No gradual migration

**Philosophy:** It's better to be completely broken for 6 hours than half-broken for 6 weeks.

---

## PHASE BREAKDOWN

### PHASE 1: Domain Entity Cleanup (BREAKS COMPILATION)
**Time:** 2-3 hours | **Files:** 15-20 | **Changes:** ~50 Id deletions

- Delete ALL `public string Id { get; set; }` from domain entities
- Delete ALL `List<string> EntityIds` properties
- **Exception:** Template Ids stay (SceneTemplate, SituationTemplate, Achievement, State)
- **Expected:** 500+ compilation errors

**Key files:**
- Location.cs, RouteOption.cs, Venue.cs, Item.cs
- Scene.cs, Situation.cs, DeliveryJob.cs, ObservationScene.cs
- ConversationTree.cs, EmergencySituation.cs, Obligation.cs

### PHASE 2: ViewModel Refactoring (FIXES UI LAYER)
**Time:** 2-3 hours | **Files:** 10-15 | **Changes:** 30-50 ViewModels

- Replace ALL `EntityId` properties with `Entity` object properties
- Update facade ViewModel builders to pass objects
- Frontend accesses `Entity.Name` directly from object

**Pattern:**
```csharp
// BEFORE
public class SomeViewModel { public string EntityId { get; set; } }

// AFTER
public class SomeViewModel { public EntityType Entity { get; set; } }
```

### PHASE 3: UI Event Handler Refactoring (FIXES USER INTERACTIONS)
**Time:** 1-2 hours | **Files:** 10 | **Changes:** 11 handlers + Razor templates

- Update handler signatures: `Handle(string id)` → `Handle(Entity entity)`
- Update @onclick calls to pass objects from ViewModels
- Update facade calls inside handlers to pass objects

**Pattern:**
```csharp
// BEFORE
protected async Task HandleTalkToNPC(string npcId) { ... }

// AFTER
protected async Task HandleTalkToNPC(NPC npc) { ... }
```

### PHASE 4: Facade Method Refactoring (FIXES BUSINESS LOGIC)
**Time:** 3-4 hours | **Files:** 22 facades | **Changes:** 138 methods + 5 deletions

- Delete ALL GetById methods (5 methods)
- Update ALL facade methods to receive objects instead of Ids
- Remove internal `GetEntityById` lookups

**Pattern:**
```csharp
// DELETE
public NPC GetNPCById(string npcId)
public Location GetLocationById(string locationId)

// UPDATE
public bool CanTravelTo(string locationId) → CanTravelTo(Location destination)
```

### PHASE 5: Parser/Service Updates (FIXES DATA LOADING)
**Time:** 3-4 hours | **Files:** 35 (15 parsers + 20 services)

- Update parsers to assign object references (not Ids)
- Fix SkeletonGenerator Id comparisons
- Update PackageLoader entity linking
- Fix all service layer entity lookups

**Pattern:**
```csharp
// BEFORE
var entity = new Entity { Id = dto.Id, RelatedEntityId = dto.RelatedId };

// AFTER
var entity = new Entity {
    RelatedEntity = gameWorld.Entities.FirstOrDefault(e => e.Name == dto.RelatedName)
};
```

### PHASE 6: Compilation Fix Sweep (FINAL CLEANUP)
**Time:** 2-4 hours | **Files:** 70 | **Changes:** 214 Id comparisons

- Delete `GetLocation(id)` / `GetNPC(id)` methods from GameWorld
- Fix ALL `.Id ==` comparisons (214 occurrences)
- Fix LINQ queries using Id filters
- Fix debug logging using Ids

**Pattern:**
```csharp
// BEFORE
if (npc.Location.Id == currentLocation.Id)
var route = routes.FirstOrDefault(r => r.Id == routeId);

// AFTER
if (npc.Location == currentLocation)  // Object equality
var route = routes.FirstOrDefault(r => r == targetRoute);
```

### PHASE 7: Special Cases (EDGE CASES)
**Time:** 1-2 hours | **Files:** Variable

- PlacementId system (use Name as PlacementId)
- Dictionary keys (eliminate dictionaries per ANTIPATTERN)
- Active session IDs (template references stay)
- SkeletonSource tracking (description string, not ID)

### PHASE 8: Verification and Testing
**Time:** 1-2 hours

- Compilation verification (`dotnet build` → 0 errors)
- Pattern verification (grep searches)
- Manual testing (start game, talk to NPC, travel, scenes)

---

## EXECUTION ORDER (DEPENDENCY CHAIN)

```
PHASE 1 (Domain)
    ↓
PHASE 2 (ViewModels) ← reads domain entities
    ↓
PHASE 3 (UI) ← uses ViewModels
    ↓
PHASE 4 (Facades) ← called by UI, builds ViewModels
    ↓
PHASE 5 (Parsers/Services) ← creates domain entities, calls facades
    ↓
PHASE 6 (Cleanup) ← fixes all remaining compilation errors
    ↓
PHASE 7 (Edge Cases) ← handles special patterns
    ↓
PHASE 8 (Verify) ← ensures correctness
```

**CRITICAL:** Must execute phases in order. Cannot skip phases. Each phase depends on previous.

---

## TIMELINE ESTIMATE

| Phase | Time | Cumulative |
|-------|------|------------|
| Phase 1: Domain Cleanup | 2-3 hours | 2-3 hours |
| Phase 2: ViewModels | 2-3 hours | 4-6 hours |
| Phase 3: UI Handlers | 1-2 hours | 5-8 hours |
| Phase 4: Facades | 3-4 hours | 8-12 hours |
| Phase 5: Parsers/Services | 3-4 hours | 11-16 hours |
| Phase 6: Compilation Fix | 2-4 hours | 13-20 hours |
| Phase 7: Edge Cases | 1-2 hours | 14-22 hours |
| Phase 8: Testing | 1-2 hours | **15-24 hours** |

**RECOMMENDED:** Execute in 2-3 continuous sessions to maintain mental context.

**Session 1:** Phases 1-3 (5-8 hours) - Break compilation, fix UI layer
**Session 2:** Phases 4-6 (8-12 hours) - Fix business logic, restore compilation
**Session 3:** Phases 7-8 (2-4 hours) - Edge cases and verification

---

## SUCCESS CRITERIA

Refactoring is **COMPLETE** when ALL of the following are true:

1. ✅ `cd src && dotnet build` → **0 Errors**
2. ✅ Domain entities have ZERO `public string Id { get; set; }` (except templates)
3. ✅ ViewModels have ZERO `EntityId` properties (objects only)
4. ✅ UI handlers receive objects, NOT string Ids
5. ✅ Facades receive objects, NOT string Ids
6. ✅ GetById methods are ALL deleted (5 methods gone)
7. ✅ `.Id ==` comparisons ONLY in template/skeleton tracking
8. ✅ Game runs without null reference exceptions
9. ✅ Manual testing passes:
   - Start new game
   - Talk to NPC
   - Travel to location
   - Start scene
   - Complete situation

**HIGHLANDER ACHIEVED:** One way to reference entities - **Object references ONLY**.

---

## KEY FILES BY PHASE

### Phase 1: Domain Entities (15-20 files)
- `src/Content/Location.cs`
- `src/GameState/RouteOption.cs`
- `src/GameState/Venue.cs`
- `src/GameState/Item.cs`
- `src/GameState/Scene.cs`
- `src/GameState/Situation.cs`
- `src/GameState/DeliveryJob.cs`
- `src/GameState/ObservationScene.cs`
- `src/GameState/ConversationTree.cs`
- `src/GameState/EmergencySituation.cs`
- `src/GameState/Obligation.cs`
- + 5-10 more entity files

### Phase 2: ViewModels (1 file, 30-50 classes)
- `src/ViewModels/GameFacadeViewModels.cs`

### Phase 3: UI Components (10 files)
- `src/Pages/Components/LocationContent.razor.cs`
- `src/Pages/Components/LocationContent.razor`
- `src/Pages/Components/TravelContent.razor.cs`
- `src/Pages/Components/TravelContent.razor`
- `src/Pages/Components/ConversationContent.razor.cs`
- `src/Pages/Components/ExchangeContent.razor.cs`
- `src/Pages/Components/SceneContent.razor.cs`
- `src/Pages/Components/MentalContent.razor.cs`
- `src/Pages/Components/PhysicalContent.razor.cs`
- `src/Pages/GameScreen.razor.cs`

### Phase 4: Facades (22 files)
- `src/Subsystems/Travel/TravelFacade.cs`
- `src/Subsystems/Location/LocationFacade.cs`
- `src/Subsystems/Conversation/ConversationTreeFacade.cs`
- `src/Subsystems/Social/SocialFacade.cs`
- `src/Subsystems/Mental/MentalFacade.cs`
- `src/Subsystems/Physical/PhysicalFacade.cs`
- `src/Subsystems/Exchange/ExchangeFacade.cs`
- `src/Subsystems/Scene/SceneFacade.cs`
- `src/Subsystems/Situation/SituationFacade.cs`
- `src/Subsystems/Observation/ObservationFacade.cs`
- + 12 more facade files

### Phase 5: Parsers/Services (35 files)
**Parsers (15 files):**
- `src/Content/LocationParser.cs`
- `src/Content/NPCParser.cs`
- `src/Content/RouteParser.cs`
- `src/Content/SceneParser.cs`
- `src/Content/SituationParser.cs`
- `src/Content/PackageLoader.cs`
- `src/Content/SkeletonGenerator.cs`
- + 8 more parsers

**Services (20 files):**
- `src/Services/GameFacade.cs`
- `src/Services/DependentResourceOrchestrationService.cs`
- `src/Subsystems/Travel/TravelManager.cs`
- `src/Subsystems/Travel/RouteManager.cs`
- `src/Subsystems/Location/LocationManager.cs`
- + 15 more services

### Phase 6: Compilation Fix (70 files)
- `src/GameState/GameWorld.cs` (delete GetById methods)
- All 70 files with `.Id ==` comparisons

---

## VERIFICATION COMMANDS

**After each phase, run verification:**

```bash
# Compilation check
cd /home/user/Wayfarer/src && dotnet build

# Pattern checks
grep -r "public string Id { get; set; }" src/GameState/ src/Content/ --include="*.cs" | grep -v Template | grep -v DTO
grep -r "GetLocationById\|GetNPCById\|GetRouteById" src/ --include="*.cs"
grep -r "GetLocation\(" src/ --include="*.cs" | grep -v "GetPlayerCurrentLocation\|GetLocationByName"
grep -r "\.Id\s*==" src/ --include="*.cs" | grep -v Template | grep -v DTO
```

**Expected results at end:**
- Build: **0 Errors**
- Id properties: **0 results** (except templates)
- GetById methods: **0 results**
- GetLocation calls: **minimal** (only GetLocationByName for parsing)
- .Id comparisons: **minimal** (only templates, PlacementId, skeleton tracking)

---

## EXCEPTIONS (WHAT STAYS)

### Template IDs (Catalogue Entities)
These reference configuration data, NOT runtime entities. IDs stay:
- `SceneTemplate.Id`
- `SituationTemplate.Id`
- `Achievement.Id`
- `State.Id`
- `PlayerStatDefinition.Id`
- Any other template/catalogue class

### DTO Classes
All DTO classes keep Id properties (they represent JSON structure):
- `LocationDTO.Id`
- `NPCDTO.Id`
- `RouteDTO.Id`
- All other DTOs

**Parsers map DTO → Domain:** Use Name as key, discard Id.

### Skeleton Tracking
```csharp
public string SkeletonSource { get; set; }  // "letter_template_elena_refusal"
```
This is a DESCRIPTION string, not an entity ID. Stays.

### Active Session References
IF these reference templates (not instances), they stay as Ids:
```csharp
public string PendingForcedSceneId { get; set; }  // SceneTemplate.Id reference
```

IF they reference runtime instances, convert to objects:
```csharp
public Scene PendingForcedScene { get; set; }
```

---

## CRITICAL WARNINGS

### ⚠️ 1. NO PARTIAL COMPLETION
Cannot stop halfway. Must complete ALL 8 phases. Game will be broken for 15-24 hours.

### ⚠️ 2. NO SHORTCUTS
No "quick fixes" or "temporary workarounds". Follow the plan exactly.

### ⚠️ 3. NO COMPATIBILITY LAYERS
No `GetByIdOrName` hybrid methods. No "deprecated but still works" Id properties. Clean break only.

### ⚠️ 4. MASSIVE COMPILATION ERRORS EXPECTED
After Phase 1: **500+ errors**. This is NORMAL. Keep going.

### ⚠️ 5. COMMIT AT END ONLY
Do NOT commit partial work. Only commit when ALL phases complete and game runs.

### ⚠️ 6. MENTAL CONTEXT REQUIRED
This refactoring requires deep understanding of architecture. Read all of:
- `CLAUDE.md` (MANDATORY DOCUMENTATION PROTOCOL)
- `05_building_block_view.md` (Component structure)
- `08_crosscutting_concepts.md` (HIGHLANDER pattern)

### ⚠️ 7. BUILD AFTER EVERY PHASE
Run `dotnet build` after each phase. Track error count. Should decrease phase by phase.

---

## ROLLBACK PLAN

If refactoring breaks beyond repair:

```bash
# Commit broken state (for analysis)
git add -A
git commit -m "WIP: ID elimination refactoring - incomplete (reverting)"

# Revert to previous working commit
git reset --hard HEAD~1
```

**However:** Incremental approach is NOT recommended. HIGHLANDER requires clean break.

Better to:
1. Read detailed plan again
2. Identify exact error
3. Fix systematically
4. Continue forward

---

## GORDON RAMSAY STANDARD

**Before refactoring:**
"This codebase is using IDs EVERYWHERE like some kind of DATABASE! You're doing lookups by ID, passing IDs around, checking `.Id == id` like it's 1995! Object references exist for a REASON! HIGHLANDER means ONE WAY to reference entities, not this ID SOUP!"

**After refactoring:**
"NOW we're talking. Object references ONLY. No IDs. No lookups. You pass an NPC, you HAVE the NPC. Clean. Simple. Professional. DONE."

---

## NEXT STEPS

1. **Read complete plan:** `/home/user/Wayfarer/ID_ELIMINATION_EXECUTION_PLAN.md`
2. **Understand architecture:** Read arc42 docs (Building Block View, Glossary)
3. **Create feature branch:**
   ```bash
   git checkout -b id-elimination-clean-break
   ```
4. **Begin Phase 1:** Delete all domain entity IDs
5. **Execute phases sequentially:** 1 → 2 → 3 → 4 → 5 → 6 → 7 → 8
6. **Verify success:** All 9 success criteria met
7. **Commit once:**
   ```bash
   git add -A
   git commit -m "Complete ID elimination: HIGHLANDER object references only"
   ```
8. **Create PR:** Celebrate massive refactoring completion

---

## RESOURCES

- **Detailed execution plan:** `/home/user/Wayfarer/ID_ELIMINATION_EXECUTION_PLAN.md`
- **Architecture docs:** `/home/user/Wayfarer/05_building_block_view.md`
- **HIGHLANDER pattern:** `/home/user/Wayfarer/08_crosscutting_concepts.md`
- **Coding standards:** `/home/user/Wayfarer/CODING_STANDARDS.md`
- **Process philosophy:** `/home/user/Wayfarer/CLAUDE.md`

---

**ESTIMATE:** 15-24 focused hours across 2-3 sessions

**DIFFICULTY:** High (requires deep architectural understanding)

**IMPACT:** Massive (100-120 files, 500-1000 changes)

**BENEFIT:** HIGHLANDER compliance, architectural purity, elimination of ID soup

**READY?** Read the detailed plan, understand the architecture, and BEGIN.
