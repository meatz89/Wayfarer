# PHASE 3: PUBLIC API - String Lookup Method Elimination Plan

## Scope Analysis

**Target Methods (NPCRepository):**
- `GetById(string id)` - 30 references
- `GetByName(string name)` - 1 reference

**Total violations: 31 references across 11 files**

## Files Affected

1. **NPCRepository.cs** (method definitions)
2. **NarrativeService.cs** (3 references)
3. **TokenMechanicsManager.cs** (5 references)
4. **NPCService.cs** (1 reference)
5. **LocationFacade.cs** (1 reference)
6. **NPCLocationTracker.cs** (5 references)
7. **EventNarrator.cs** (3 references)
8. **MeetingManager.cs** (1 reference)
9. **ConnectionTokenManager.cs** (4 references)
10. **RelationshipTracker.cs** (6 references)
11. **TokenUnlockManager.cs** (1 reference)

## Root Cause Analysis

**Why GetById exists:**
- Legacy architecture from before DDR-006 (Categorical Scaling)
- Services accept NPC ID strings from upstream (ViewModels, event handlers)
- Lookup NPC object from ID to perform operations

**Correct Pattern (CONTRACT BOUNDARIES FIRST):**
- Facades accept NPC objects (not strings)
- ViewModels contain NPC objects (not NPC IDs)
- Event handlers receive NPC objects (not NPC IDs)
- NO ID lookups needed (objects flow through entire stack)

## Refactoring Strategy

### Phase 3A: Eliminate GetById from Services
**Files**: NarrativeService, TokenMechanicsManager, NPCService
**Pattern**: Change method signatures to accept `NPC npc` instead of `string npcId`
**Impact**: 9 methods updated

### Phase 3B: Eliminate GetById from Subsystems
**Files**: LocationFacade, NPCLocationTracker, EventNarrator, MeetingManager, ConnectionTokenManager, RelationshipTracker, TokenUnlockManager
**Pattern**: Change method signatures to accept `NPC npc` instead of `string npcId`
**Impact**: 21 methods updated

### Phase 3C: Delete GetById Method
**File**: NPCRepository.cs
**Action**: Delete `GetById` and `GetByName` methods
**Verification**: Zero references remain

## Execution Order

1. **Phase 3A** - Services (isolated, high-level)
2. **Phase 3B** - Subsystems (may depend on Services)
3. **Phase 3C** - Delete methods (after all references eliminated)

## Example Refactoring

**BEFORE (WRONG):**
```csharp
// Service accepts string ID
public void GrantToken(string npcId, ConnectionType tokenType)
{
    NPC npc = _npcRepository.GetById(npcId); // ID lookup
    npc.Tokens.Add(tokenType);
}

// Caller passes ID
service.GrantToken("elena_innkeeper", ConnectionType.Trust);
```

**AFTER (CORRECT):**
```csharp
// Service accepts NPC object
public void GrantToken(NPC npc, ConnectionType tokenType)
{
    npc.Tokens.Add(tokenType); // Direct object access
}

// Caller passes object
NPC elena = _gameWorld.NPCs.FirstOrDefault(n => n.Name == "Elena");
service.GrantToken(elena, ConnectionType.Trust);
```

## Dependencies

**Phase 3 requires Phase 4-5 to complete fully:**
- Phase 4 (ViewModels): ViewModels must contain NPC objects (not IDs)
- Phase 5 (UI Event Handlers): Event handlers must receive NPC objects (not IDs)

**Order**: Phase 3 (Services) → Phase 4 (ViewModels) → Phase 5 (UI) → Phase 3C (Delete)

## Current Status

- Phase 1 (JSON Input): COMPLETE ✅
- Phase 2 (Parser Transform): COMPLETE ✅
- Phase 3A (Services): PENDING
- Phase 3B (Subsystems): PENDING
- Phase 3C (Delete GetById): PENDING

## Next Action

Execute Phase 3A: Update Services to accept NPC objects instead of string IDs
