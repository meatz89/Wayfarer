# Remaining 80 Errors - Phases 9-11 Continuation

**Status:** 116 errors → 80 errors (36 fixed in PHASE 6-9)

**Completed Phases:**
- ✅ PHASE 6: ConversationTree object reference resolution (parser + facade)
- ✅ PHASE 7: Location.VenueName → location.Venue.Name (3 files)
- ✅ PHASE 8: Obligation.Situation → context.Situation (4 files)
- ✅ PHASE 8b: NavigationManager using statement
- ✅ PHASE 9: CardContext.RequestId → CardContext.Situation (2 files)

## Remaining Error Categories (80 errors)

### Category A: TokenCount → KeyValuePair Conversion (6 errors)

**Type mismatch error:**
```
error CS0030: Der Typ "TokenCount" kann nicht in "System.Collections.Generic.KeyValuePair<ConnectionType, int>" konvertiert werden
```

**Locations:**
- `ExchangeHandler.cs:236`
- `ExchangeContent.razor.cs:432`
- `ExchangeContent.razor.cs:492`

**Investigation Needed:**
- Search for `TokenCount` class definition
- Understand relationship to `KeyValuePair<ConnectionType, int>`
- Likely: TokenCount has properties that need extracting or type needs changing

**Fix Pattern (to determine):**
```csharp
// Option 1: TokenCount has ConnectionType/Count properties
foreach (TokenCount tc in collection) {
    kvp = new KeyValuePair<ConnectionType, int>(tc.ConnectionType, tc.Count);
}

// Option 2: Collection type changed
List<TokenCount> → Dictionary<ConnectionType, int>
```

---

### Category B: Deleted Property Access (50+ errors)

**Pattern:** Code accessing properties that were deleted during HIGHLANDER refactoring.

#### B1: Region.Id (4 errors)

**File:** `ProceduralAStoryService.cs:266, 275`

**Current (WRONG):**
```csharp
context.IsRegionRecent(region.Id)
```

**Fix:** Use `region.Name` as natural key (architect guidance):
```csharp
context.IsRegionRecent(region.Name)
```

---

#### B2: Player.InjuryCardIds (4 errors)

**Files:** `PhysicalDeckBuilder.cs:49`, `PhysicalFacade.cs:459`

**Current (WRONG):**
```csharp
player.InjuryCardIds.Add(cardId);
```

**Fix:** Player needs `List<InjuryCard> InjuryCards` property (architect guidance line 129 Player.cs):
1. Add property to Player class:
```csharp
public List<InjuryCard> InjuryCards { get; set; } = new List<InjuryCard>();
```

2. Change usage:
```csharp
player.InjuryCards.Add(card); // Object, not ID
```

---

#### B3: Player.CollectedObservations (4 errors)

**Files:** `DiscoveryJournal.razor.cs:56, 61`

**Current (WRONG):**
```csharp
player.CollectedObservations
```

**Fix:** Player needs `List<Observation> CollectedObservations` property (architect guidance line 126 Player.cs):
1. Add property to Player class:
```csharp
public List<Observation> CollectedObservations { get; set; } = new List<Observation>();
```

2. Usage already correct (iterating over collection)

---

#### B4: ExchangeCard.UnlocksExchangeId (4 errors)

**File:** `ExchangeOrchestrator.cs:115, 118`

**Current (WRONG):**
```csharp
card.UnlocksExchangeId
```

**Fix:** ExchangeCard needs `Exchange UnlocksExchange` object property:
1. Add to ExchangeCard:
```csharp
public Exchange UnlocksExchange { get; set; }
```

2. Change usage:
```csharp
if (card.UnlocksExchange != null)
{
    // Use card.UnlocksExchange.Name or object reference
}
```

---

#### B5: SituationCardRewards.Equipment (4 errors)

**File:** `SituationCompletionHandler.cs:189, 191`

**Current (WRONG):**
```csharp
rewards.Equipment
```

**Investigation:**
- Search for `SituationCardRewards` class definition
- Determine if Equipment property exists or was deleted
- If deleted, understand replacement pattern

**Likely Fix:** Equipment system eliminated, remove references entirely

---

#### B6: Scene.Id, Situation.Id (4 errors)

**Pattern:** Using `.Id` property for logging/lookups

**Fix:** Use `.Name` property for display, object references for logic:
```csharp
// Display
Console.WriteLine($"Scene: {scene.Name}");

// Logic
if (scene == targetScene) // Object comparison, not ID
```

---

#### B7: Various ViewModel ID Properties (12+ errors)

**Pattern:** ViewModels with deleted ID properties being assigned

**Files:** Multiple (`NPCInteractionViewModel`, `ObservationViewModel`, `RouteOptionViewModel`, etc.)

**Fix Pattern:**
1. Delete ID assignment lines entirely
2. Ensure ViewModel has object reference property
3. UI accesses `.Name` for display

Example:
```csharp
// BEFORE
return new NPCInteractionViewModel {
    Id = npc.ID,        // ❌ Delete this line
    Name = npc.Name,
    Npc = npc           // ✓ Keep object reference
};
```

---

### Category C: Method Signature Mismatches (10+ errors)

#### C1: IsSatisfied(3 args) → IsSatisfied(2 args)

**File:** `LocationFacade.cs:825`

**Investigation:** Check IsSatisfied method signature, adjust call site

---

#### C2: GetObservationsForLocation(2 args) → GetObservationsForLocation(1 arg)

**File:** `LocationFacade.cs:306`

**Investigation:** Check method signature, remove extra parameter

---

#### C3: Inventory.HasItem / Inventory.GetItems

**Reality:** Methods exist as `Contains(Item)` and `GetAllItems()`

**Fix:** Change method names at call sites

---

### Category D: Miscellaneous (8+ errors)

- `Venue.Venue` - Likely typo or self-reference
- `SocialChallengeContext.ConversationType` - Property access
- `RouteSegmentUnlock.PathDTO` - DTO property access
- `GameWorld.InitialLocationName` - Property access
- Variable name conflicts (CS0136) - Scope issues

---

## Systematic Fix Strategy

**Order of execution for remaining 80 errors:**

### PHASE 10A: Add Missing Object Properties (30 min)
1. Add `Player.InjuryCards` property
2. Add `Player.CollectedObservations` property
3. Add `ExchangeCard.UnlocksExchange` property
4. Verify properties exist in domain entities

### PHASE 10B: Fix Property Access Patterns (45 min)
1. Region.Id → Region.Name (4 errors)
2. Scene.Id / Situation.Id → .Name or object comparison (4 errors)
3. Player.InjuryCardIds → Player.InjuryCards (4 errors)
4. Player.CollectedObservations usage (4 errors)
5. ExchangeCard.UnlocksExchangeId → ExchangeCard.UnlocksExchange (4 errors)

### PHASE 10C: Investigate and Fix Type Issues (30 min)
1. TokenCount conversion (6 errors) - Investigate class structure
2. SituationCardRewards.Equipment (4 errors) - Check if deleted
3. Method signature mismatches (6 errors) - Adjust signatures/calls

### PHASE 10D: ViewModel Cleanup (30 min)
1. Delete all remaining ViewModel ID assignments (12 errors)
2. Verify object reference properties exist
3. Ensure UI uses `.Name` for display

### PHASE 10E: Miscellaneous Fixes (30 min)
1. Fix Venue.Venue self-reference
2. Fix property access errors (SocialChallengeContext, etc.)
3. Fix variable name conflicts
4. Fix Inventory method name mismatches

---

## Verification Commands

After each sub-phase:
```bash
cd src && dotnet build 2>&1 | grep -E "Fehler|error" | wc -l
```

Expected progression:
- After 10A: ~76 errors (add properties)
- After 10B: ~56 errors (fix property access)
- After 10C: ~44 errors (fix type issues)
- After 10D: ~32 errors (ViewModel cleanup)
- After 10E: **0 errors** ✅

Final verification:
```bash
# Zero errors
cd src && dotnet build

# Zero HIGHLANDER violations
grep -rn "\.Id\s*==" src/ --include="*.cs" | grep -v "Template"
grep -rn "GetById\(" src/ --include="*.cs" | grep -v "Template"
```

---

## Key Insights for Continuation

**Architectural Patterns Established:**
1. **Entities have object properties** - Parser resolves, no IDs
2. **Facade uses object references** - No string parameters
3. **ViewModels pass objects** - No ID extraction
4. **Display uses .Name** - String extraction only at UI layer

**The Remaining Work:**
- Same patterns applied to remaining ~80 errors
- No new architectural decisions needed
- Systematic mechanical fixes following established patterns
- Estimated 3-4 hours to complete all phases to zero errors

**Next Session Start:**
Execute PHASE 10A immediately - add missing object properties to Player and ExchangeCard classes.
