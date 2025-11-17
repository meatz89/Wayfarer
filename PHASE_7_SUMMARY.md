# Phase 7 Status Summary: ID Accessor Classification

## Quick Findings

| Metric | Value |
|--------|-------|
| **Total .Id accessor sites** | 91 |
| **Acceptable sites (KEEP)** | 19 (21%) |
| **Violation sites (FIX)** | 57 (63%) |
| **Phase 7 completion** | 37% |

## Violation Breakdown

### By Category (57 violations requiring fixes)

1. **Category 3: String Parameter Violations (15 sites)** - HIGH PRIORITY
   - Methods accepting `string id` parameters
   - Example: `GetNPCsForLocation(string locationId)`
   - Fix: Change to `GetNPCsForLocation(Location location)`
   - Impact: High - affects 5+ method signatures

2. **Category 5: Service/Facade Lookups (30 sites)** - MEDIUM PRIORITY
   - Methods doing `collection.FirstOrDefault(e => e.Id == stringId)`
   - Spread across: ItemRepository, MentalFacade, PhysicalFacade, EmergencyFacade, etc.
   - Fix: Apply 9-step holistic refactoring pattern
   - Impact: Very high - affects 15+ different methods

3. **Category 5B: Razor Component Lookups (12 sites)** - MEDIUM PRIORITY
   - Frontend doing business logic queries
   - Example: `GameWorld.Scenes.FirstOrDefault(s => s.Id == sceneId)`
   - Fix: Move lookups to backend GameFacade, pass objects to components
   - Impact: Medium - affects 8 Razor component files

## Acceptable Sites (19 - Keep as-is)

### Category 1: Parser DTO Lookups (7 sites) ✅
- Converting JSON/DTO IDs to domain objects at parse time
- Example: `gameWorld.Venues.FirstOrDefault(v => v.Id == location.VenueId)`
- Status: Acceptable (end of parsing pipeline)

### Category 2: Template Lookups (11 sites) ✅
- Looking up immutable template archetypes
- Example: `gameWorld.SceneTemplates.FirstOrDefault(t => t.Id == dto.TemplateId)`
- Status: Acceptable (templates are read-only)

### Category 4: Display/Logging (1 site) ✅
- Debug console output only
- Status: Acceptable (no business logic impact)

## Violation Sites by Severity

### Critical (String Parameter Signatures) - 15 sites
```
NPCRepository.cs:87         GetNPCsForLocation(string locationId)
NPCRepository.cs:142        GetNPCsForLocationAndTime(string LocationId)
NPCRepository.cs:163        GetPrimaryNPCForSpot(string locationId)
NPC.cs:154                  IsAtLocation(string locationId)
ConversationTreeFacade:281  npc.Location?.Id != locationId
ObligationDiscoveryEval:74  _gameWorld.GetPlayerCurrentLocation().Id != prereqs.LocationId
ObligationDiscoveryEval:88  _gameWorld.GetPlayerCurrentLocation().Id != prereqs.LocationId
LocationAccessibilityService:45  currentSituation.Location?.Id == location.Id
MarketSubsystemManager:552  loc.Id == currentLocationId
MarketSubsystemManager:691  currentLocation.Id != locationId
ObservationFacade:258       .Where(s => s.Location?.Id == locationId)
SituationCompletionHandler:319  sit.Obligation?.Id == completedSituation.Obligation?.Id
PackageLoader.cs:298        Looking up starting location
PackageLoader.cs:704,733,759 Validating card IDs
NPC.cs:159                  Object equality check (r.Id == route.Id)
```

### High (Repository/Service Lookups) - 30 sites
```
ItemRepository.cs:26,51            GetItemById() / AddItems()
MentalChallengeDeck.cs:27          GetCardById()
PhysicalChallengeDeck.cs:26        GetCardById()
SocialChallengeDeck.cs:26          GetCardById()
TravelManager.cs:372,514,534       Scene/Card lookups
DependentResourceOrchestration:67  GetItemById()
MentalDeckBuilder:85               GetItemById()
PhysicalDeckBuilder:101            GetItemById()
SocialChallengeDeckBuilder:170     GetItemById()
MentalFacade.cs:67,196,386         Situation lookups
PhysicalFacade.cs:95,210,309,481   Situation/Template lookups
TimeFacade.cs:294                  GetObligationById()
EmergencyFacade.cs:99,157,167,218  Emergency lookups
MeetingManager.cs:53               GetMeetingById()
ObservationFacade.cs:32            GetSceneById()
LocationFacade.cs:1047-1055        Deck lookups
SceneGenerationFacade.cs:49        GetLocationById()
TravelFacade.cs:627,643            Card lookups
TravelTimeCalculator.cs:27         Location equality check
```

### Medium (Razor Component Logic) - 12 sites
```
ConversationContent.razor:683,693        Deck/card lookups
DiscoveryJournal.razor:114               GetObligationById()
ExchangeContent.razor:244                Exchange lookup
LocationContent.razor:150,237            Scene lookups
MentalContent.razor:745,755              Deck/card lookups
PhysicalContent.razor:839,849            Deck/card lookups
SceneContent.razor:458                   Choice lookup
TravelPathContent.razor:222,724          Card/scene lookups
```

## Root Causes

| Cause | Count | Why It's Wrong |
|-------|-------|----------------|
| Method signature accepts string ID | 15 | Should accept object, not string ID |
| Service method doing lookup by ID | 30 | Violates spatial architecture pattern |
| Razor component doing business logic | 12 | Frontend shouldn't query backend |
| Location/Obligation compared by ID | 8 | Use object reference equality instead |
| Object duplicate checking by ID | 2 | Use reference equality (`==`) not ID match |
| Hardcoded template ID string | 1 | Use enum or template reference |

## Estimated Effort to Complete Phase 7

| Priority | Category | Sites | Effort | Impact |
|----------|----------|-------|--------|--------|
| 1 | String Parameters | 15 | 5-8h | CRITICAL - many call sites |
| 2 | Service Lookups | 30 | 10-15h | HIGH - complex refactoring |
| 3 | Razor Components | 12 | 4-6h | MEDIUM - mostly search/replace |
| **TOTAL** | | **57** | **19-29h** | **63% of violations** |

## Next Steps

1. **Immediate (Priority 1):**
   - Fix 15 string parameter violations
   - Start with NPCRepository (most visible)
   - Update all call sites systematically

2. **Short-term (Priority 2):**
   - Refactor 30 service/facade lookups
   - Group by system (Mental, Physical, Social, etc.)
   - Apply 9-step holistic pattern

3. **Medium-term (Priority 3):**
   - Move 12 Razor component queries to backend
   - Clean up frontend logic
   - Add component properties for passed objects

4. **Verification:**
   - Re-run ID classification after each phase
   - Ensure no regressions introduced
   - Test all affected code paths

## Files to Consult

- Full analysis: `/home/user/Wayfarer/ID_CLASSIFICATION_PHASE_7.md`
- CLAUDE.md: Hex-based spatial architecture principles
- `08_crosscutting_concepts.md`: Pattern definitions
- `09_architecture_decisions.md`: Why patterns matter

---

**Phase 7 Status:** INCOMPLETE (37% done, 63% violations remain)

Next major refactoring phase required to eliminate 57 remaining violations.
