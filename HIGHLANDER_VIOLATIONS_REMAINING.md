# HIGHLANDER Architecture Violations - Remaining Work

## Status: Phase 3A Complete, Phase 3B-C Pending

### Completed in This PR (Phase 3A)
- ✅ `var` keyword violations (2 occurrences) - ELIMINATED
  - ServiceConfiguration.cs:104 - replaced with explicit `NavigationManager` type
  - ProceduralAStoryService.cs:373 - replaced anonymous type with `ProceduralTemplatePackage` class
- ✅ NPCRepository.GetById violations (3 callers) - ELIMINATED
  - TokenMechanicsManager.cs:262 - changed to `npcEntry.Npc` direct object access
  - LocationFacade.cs:342 - changed to use `obs.RelevantNPCs` object collection directly
  - NPCService.cs:42 - deleted dead code method `IsNPCAvailable(string npcId)`
- ✅ NPCRepository.GetById/GetByName methods - DELETED

### Remaining Violations (Phase 3B-C) - CRITICAL

#### 1. GameWorld.GetLocation(string) - 20+ Callers
**Method Definition:**
- `GameWorld.cs:344` - `public Location GetLocation(string locationName)`

**Callers (must be refactored before method can be deleted):**
- `PackageLoader.cs` (7 occurrences)
  - Line 299: `Location startingLocation = _gameWorld.GetLocation(conditions.StartingSpotId);`
  - Line 861: `Location? existingSkeleton = _gameWorld.GetLocation(dto.Name);`
  - Line 978: `Location originSpot = _gameWorld.GetLocation(dto.OriginSpotId);`
  - Line 1009: `Location destSpot = _gameWorld.GetLocation(dto.DestinationSpotId);`
  - Line 1061: `Location location = _gameWorld.GetLocation(LocationId);`
  - Line 1279: `OriginLocation = _gameWorld.GetLocation(dto.OriginSpotId)`
  - Line 1280: `DestinationLocation = _gameWorld.GetLocation(dto.DestinationSpotId)`
  - Line 1606: `Location location = _gameWorld.GetLocation(LocationId);`
- `TravelContent.razor.cs` (3 occurrences)
  - Line 96: `Location location = GameFacade.GetLocation(destinationSpotId);`
  - Line 110: `Location location = GameFacade.GetLocation(destinationSpotId);`
  - Line 122: `Location location = GameFacade.GetLocation(destinationSpotId);`
- `LocationManager.cs` (3 occurrences)
  - Line 65-68: `public Location GetLocation(string LocationId)` wrapper method
  - Line 146: `return GetLocation(locationId) != null;`
- `ObligationParser.cs` (2 occurrences)
  - Line 134: `location = _gameWorld.GetLocation(dto.LocationId);`
  - Line 180: `location = _gameWorld.GetLocation(dto.LocationId);`
- `LocationFacade.cs` (1 occurrence)
  - Line 301: `Location location = _gameWorld.GetLocation(locationName);`
- `SpawnConditionsEvaluator.cs` (1 occurrence)
  - Line 165: `Location location = _gameWorld.GetLocation(placementId);`
- `SpawnedScenePlayabilityValidator.cs` (1 occurrence)
  - Line 194: `Location location = _gameWorld.GetLocation(situation.Location.Name);`

**Refactoring Strategy:**
For each caller, trace upstream to find where the string comes from:
- **PackageLoader.cs**: DTOs contain string LocationId properties → Refactor DTOs to use PlacementFilter or EntityResolver.FindOrCreate pattern
- **TravelContent.razor.cs**: UI receives string from route → Change UI to pass Location object through event handlers
- **LocationManager.cs**: Wrapper method → Delete after fixing callers
- **Parsers**: String from JSON → Use EntityResolver.FindOrCreate with categorical properties

**Estimated Scope:** 50+ files changed (upstream call chain modifications)

#### 2. String Comparison Violations (`.Name ==` pattern) - 15+ Files
**Files with violations:**
- TravelTimeCalculator.cs
- LocationFacade.cs
- VenueGeneratorService.cs
- GameFacade.cs
- Player.cs
- GameWorld.cs
- SceneInstantiator.cs
- PackageLoader.cs
- NPCRepository.cs
- SceneTemplateParser.cs
- SituationParser.cs
- SkeletonGenerator.cs
- NumericRequirement.cs
- DiscoveryJournal.razor.cs
- DependentResourceOrchestrationService.cs

**Pattern:**
```csharp
// WRONG - String comparison for identity
if (npc1.Name == npc2.Name) { ... }
if (location.Name == searchName) { ... }
items.FirstOrDefault(i => i.Name == itemName)

// CORRECT - Object equality
if (npc1 == npc2) { ... }
if (items.Contains(item)) { ... }
```

**Refactoring Strategy:**
- Change comparisons to use object equality
- Trace upstream to eliminate string extraction (`.Name`, `.Id`)
- Change method signatures to accept typed objects instead of strings
- Update callers to pass objects instead of extracting strings

**Estimated Scope:** 30+ files changed

#### 3. GameWorld.GetDistrictForLocation(string) - String Parameter
**Method Definition:**
- `GameWorld.cs:267-274` - Accepts `string districtName`, returns `District`
- **Violation**: Should accept `District` object and return from object reference

**Method Implementation:**
```csharp
public District GetDistrictForLocation(string districtName)
{
    District district = Districts.FirstOrDefault(d => d.Name == districtName);
    // VIOLATION: String lookup with .Name comparison
    // ...
}
```

**Callers:**
- `GameWorld.cs:281` - `District district = GetDistrictForLocation(venueName);`
- Possibly others (need to grep)

**Refactoring Strategy:**
- Change signature to `public District GetDistrictForLocation(Venue venue)`
- Use `venue.District` object reference instead of string lookup
- Fix callers to pass `Venue` object instead of string

#### 4. Other String Lookup Methods
**LocationManager.GetVenue(string venueId)**
- Wrapper around `_gameWorld.Venues.FirstOrDefault(v => v.Name == venueId)`
- Should be eliminated (callers should query GameWorld.Venues directly or receive object)

**GameWorld Helper Methods with String Parameters:**
- `GetJobByLocations(string originLocationName, string destinationLocationName)` - Line 573
- `GetStrangerByName(string strangerName)` - Line 430
- `GetAvailableStrangers(string locationName, TimeBlocks currentTimeBlock)` - Line 414
- `AddStrangerToLocation(string locationName, NPC stranger)` - Line 403
- `MarkStrangerAsTalkedTo(string strangerName)` - Line 463
- `GetFullLocationPath(string venueName)` - Line 276
- `ActivateObligation(string obligationName, TimeManager timeManager)` - Line 495
- `CompleteObligation(string obligationName, TimeManager timeManager)` - Line 520
- `ApplyDeadlineConsequences(string obligationName)` - Line 607

**All violate object reference architecture - accept strings instead of typed objects**

### Verification Commands

Run these to confirm violations eliminated:
```bash
# Verify no var keywords
grep -rn "\bvar\b" src/ --include="*.cs" | grep -v "Pages\|Components"

# Verify no GetById calls (except method definitions)
grep -rn "GetById\(" src/ --include="*.cs"

# Verify no GetByName calls (except method definitions)
grep -rn "GetByName\(" src/ --include="*.cs"

# Verify no GetLocation calls (except method definition)
grep -rn "GetLocation\(" src/ --include="*.cs"

# Find string comparison violations
grep -rn "\.Name\s*==" src/ --include="*.cs" | grep -v "Console\|Log\|Display"
```

### Next Steps (Phase 3B-C)

1. **Phase 3B: Refactor GetLocation Callers**
   - Start with PackageLoader (use EntityResolver pattern)
   - Fix parsers (ObligationParser, etc.)
   - Fix UI layer (TravelContent.razor.cs)
   - Fix managers (LocationManager wrapper)
   - Estimated: 50+ files

2. **Phase 3C: Delete String Lookup Methods**
   - Delete `GameWorld.GetLocation(string)`
   - Delete `LocationManager.GetLocation(string)`
   - Delete `LocationManager.GetVenue(string)`
   - Delete other string-based helper methods
   - Verify with grep (zero occurrences)

3. **Phase 3D: Eliminate .Name Comparisons**
   - Fix 15+ files with `.Name ==` violations
   - Change to object equality
   - Update method signatures upstream
   - Estimated: 30+ files

### Total Estimated Scope: ~100 files

**Recommendation:** Split into 3 separate PRs:
- PR #1 (this): Phase 3A (var + GetById elimination) - 5 files
- PR #2: Phase 3B-C (GetLocation elimination) - 50 files
- PR #3: Phase 3D (string comparison elimination) - 30 files
