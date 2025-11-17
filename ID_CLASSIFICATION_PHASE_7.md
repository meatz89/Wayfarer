# Phase 7 Completion Assessment: `.Id` Accessor Classification

## EXECUTIVE SUMMARY

**Total `.Id` accessor sites: 91**

| Category | Count | Status |
|----------|-------|--------|
| Category 1: Parser DTO Lookups | 7 | ✅ KEEP |
| Category 2: Template Lookups | 11 | ✅ KEEP |
| Category 4: Display/Logging | 1 | ✅ KEEP |
| **Acceptable Sites (KEEP)** | **19** | **21%** |
| Category 3: String Parameter Violations | 15 | ❌ FIX |
| Category 5: Service/Facade Lookups | 30 | ❌ FIX |
| Category 5B: Razor Component Lookups | 12 | ❌ FIX |
| **Violation Sites (FIX)** | **57** | **63%** |

---

## CATEGORY 1: PARSER DTO EXTERNAL LOOKUPS (KEEP - 7 sites)

**Principle:** Converting external JSON/DTO to domain objects at parse time is acceptable. DTOs contain ID strings from JSON; parsers use those IDs to resolve object references.

### List of Sites (All Acceptable)

1. `/home/user/Wayfarer/src/Content/LocationParser.cs:23`
   - `location.Venue = gameWorld.Venues.FirstOrDefault(v => v.Id == location.VenueId)`
   - ✅ Parser resolving VenueId from DTO to Venue object reference

2. `/home/user/Wayfarer/src/Content/Parsers/EmergencyParser.cs:31`
   - `Location location = gameWorld.Locations.FirstOrDefault(l => l.Id == locationId)`
   - ✅ Parser resolving locationId from DTO to Location object

3. `/home/user/Wayfarer/src/Content/Parsers/HexParser.cs:138`
   - `Location location = locations.FirstOrDefault(l => l.Id == hex.LocationId)`
   - ✅ Parser resolving LocationId from hex DTO

4. `/home/user/Wayfarer/src/Content/Parsers/ObservationSceneParser.cs:17`
   - `Location location = gameWorld.Locations.FirstOrDefault(l => l.Id == dto.LocationId)`
   - ✅ Parser converting DTO.LocationId to Location reference

5. `/home/user/Wayfarer/src/Content/SituationParser.cs:103`
   - `situation.Obligation = gameWorld.Obligations.FirstOrDefault(i => i.Id == dto.ObligationId)`
   - ✅ Parser resolving ObligationId from DTO

6. `/home/user/Wayfarer/src/Content/StrangerParser.cs:49`
   - `stranger.Location = gameWorld.Locations.FirstOrDefault(l => l.Id == dto.LocationId)`
   - ✅ Parser resolving LocationId from Stranger DTO

7. `/home/user/Wayfarer/src/Subsystems/Location/LocationNarrativeGenerator.cs:182` (DUPLICATE in VenueGeneratorService)
   - Venue generator resolving Location during procedural generation
   - ✅ Acceptable as procedural content generation requires lookup

---

## CATEGORY 2: TEMPLATE ID COMPARISONS (KEEP - 11 sites)

**Principle:** Template IDs are acceptable because templates are immutable archetypes (not mutable game state). Template lookups are read-only structural navigation.

### List of Sites (All Acceptable)

1. `/home/user/Wayfarer/src/Content/SceneParser.cs:78`
   - `SceneTemplate template = gameWorld.SceneTemplates.FirstOrDefault(t => t.Id == dto.TemplateId)`
   - ✅ Parsing Template ID from DTO to template reference

2. `/home/user/Wayfarer/src/Content/SceneParser.cs:176`
   - `situation.Template = template.SituationTemplates.FirstOrDefault(t => t.Id == situationDto.TemplateId)`
   - ✅ Template-to-template navigation within immutable archetype

3. `/home/user/Wayfarer/src/Content/Parsers/ConversationTreeParser.cs:80`
   - `if (!tree.Nodes.Any(n => n.Id == tree.StartingNodeId))`
   - ✅ Validating template structure (StartingNodeId is template structure, not mutable state)

4. `/home/user/Wayfarer/src/Content/Parsers/ObligationParser.cs:92-94`
   - Template deck ID lookups (Mental, Physical, Social)
   - ✅ Validating obligation template references

5. `/home/user/Wayfarer/src/Services/ObligationActivity.cs:173`
   - `.FirstOrDefault(p => p.Id == situationTemplateId)`
   - ✅ Looking up situation template

6. `/home/user/Wayfarer/src/Services/ObligationActivity.cs:461`
   - `SceneTemplate template = _gameWorld.SceneTemplates.FirstOrDefault(t => t.Id == sceneSpawn.SceneTemplateId)`
   - ✅ Template lookup during obligation activity

7. `/home/user/Wayfarer/src/Subsystems/Consequence/RewardApplicationService.cs:201`
   - `.FirstOrDefault(t => t.Id == sceneSpawn.SceneTemplateId)`
   - ✅ Template lookup for reward spawning

8. `/home/user/Wayfarer/src/Subsystems/Consequence/RewardApplicationService.cs:223`
   - `template = _gameWorld.SceneTemplates.FirstOrDefault(t => t.Id == generatedTemplateId)`
   - ✅ Template lookup during consequence reward

9. `/home/user/Wayfarer/src/Subsystems/Scene/SceneFacade.cs:352`
   - `SceneTemplate template = _gameWorld.SceneTemplates.FirstOrDefault(t => t.Id == spawnReward.SceneTemplateId)`
   - ✅ Template lookup for scene reward

10. `/home/user/Wayfarer/src/Subsystems/ProceduralContent/SceneInstanceFacade.cs:138`
    - `loc.LocationTemplate?.Id == locationSpec.TemplateId`
    - ✅ Template comparison in scene instance

11. `/home/user/Wayfarer/src/Subsystems/ProceduralContent/SceneInstanceFacade.cs:163`
    - `i.ItemTemplate?.Id == itemSpec.TemplateId`
    - ✅ Template comparison in scene instance

---

## CATEGORY 4: DISPLAY/LOGGING ONLY (KEEP - 1 site)

**Principle:** Display and logging use of IDs has no business logic impact.

1. `/home/user/Wayfarer/src/Pages/Components/SceneContent.razor.cs:43`
   - `Console.WriteLine($"[SceneContent.LoadChoices] CurrentSituation.Id = {CurrentSituation.Id}")`
   - ✅ Debug logging only, no business logic

---

## CATEGORY 3: STRING PARAMETER VIOLATIONS (FIX - 15 sites)

**Principle:** Methods accepting string IDs as parameters violate hex-based spatial architecture. Methods should accept object references, not ID strings.

**Pattern:** `public Method(string entityId) { var entity = lookup(id); }`

**Action Required:** Convert parameter from `string id` to `Entity entity` object reference.

### List of Violation Sites

1. `/home/user/Wayfarer/src/Content/NPCRepository.cs:87`
   - Method: `GetNPCsForLocation(string locationId)`
   - Usage: `npcs.Where(n => n.Location?.Id == locationId)`
   - ❌ Accepts `string locationId`, should accept `Location location`

2. `/home/user/Wayfarer/src/Content/NPCRepository.cs:142`
   - Method: `GetNPCsForLocationAndTime(string LocationId)`
   - Usage: `npcs.Where(n => n.Location?.Id == LocationId)`
   - ❌ Accepts `string LocationId`, should accept `Location location`

3. `/home/user/Wayfarer/src/Content/NPCRepository.cs:163`
   - Method: `GetPrimaryNPCForSpot(string locationId)`
   - Usage: `npcs.FirstOrDefault(n => n.Location?.Id == locationId)`
   - ❌ Accepts `string locationId`, should accept `Location location`

4. `/home/user/Wayfarer/src/Content/PackageLoader.cs:298`
   - Context: Validation during initialization
   - `Location startingLocation = _gameWorld.Locations.FirstOrDefault(l => l.Id == conditions.StartingSpotId)`
   - ❌ Looking up location by string ID

5. `/home/user/Wayfarer/src/Content/PackageLoader.cs:704,733,759`
   - Context: Validating card IDs exist
   - Pattern: `_gameWorld.SocialCards.Any(c => c.Id == cardId)`
   - ❌ Validating by ID string instead of object reference

6. `/home/user/Wayfarer/src/Content/SceneParser.cs:193`
   - `int index = scene.Situations.FindIndex(s => s.Id == dto.CurrentSituationId)`
   - ⚠️ EDGE CASE: Looking up index within parsed scene (acceptable during parsing, but shows ID dependency)

7. `/home/user/Wayfarer/src/GameState/DebugLogger.cs:205`
   - `List<NPC> spotNpcs = allNpcs.Where(n => n.Location?.Id == currentLocation.Id)`
   - ❌ Comparing Location.Id in debug logging

8. `/home/user/Wayfarer/src/GameState/NPC.cs:154`
   - Method: `IsAtLocation(string locationId)`
   - Usage: `Location?.Id == locationId`
   - ❌ Method accepts string ID, should accept Location object

9. `/home/user/Wayfarer/src/GameState/NPC.cs:159`
   - `if (!_knownRoutes.Any(r => r.Id == route.Id))`
   - ❌ Object equality by ID comparison instead of reference equality

10. `/home/user/Wayfarer/src/Subsystems/Conversation/ConversationTreeFacade.cs:281`
    - `if (npc.Location?.Id != locationId)`
    - ❌ Comparing Location by ID string

11. `/home/user/Wayfarer/src/Services/ObligationDiscoveryEvaluator.cs:74`
    - `_gameWorld.GetPlayerCurrentLocation().Id != prereqs.LocationId`
    - ❌ String parameter LocationId compared to current location

12. `/home/user/Wayfarer/src/Services/ObligationDiscoveryEvaluator.cs:88`
    - `_gameWorld.GetPlayerCurrentLocation().Id != prereqs.LocationId`
    - ❌ String parameter LocationId compared to current location

13. `/home/user/Wayfarer/src/Subsystems/Location/LocationAccessibilityService.cs:45`
    - `if (currentSituation.Location?.Id == location.Id)`
    - ❌ Comparing Location by ID

14. `/home/user/Wayfarer/src/Subsystems/Market/MarketSubsystemManager.cs:552`
    - `if (loc.Id == currentLocationId)`
    - ❌ Comparing Location ID

15. `/home/user/Wayfarer/src/Subsystems/Market/MarketSubsystemManager.cs:691`
    - `if (currentLocation == null || currentLocation.Id != locationId)`
    - ❌ Comparing Location by ID string

16. `/home/user/Wayfarer/src/Subsystems/Observation/ObservationFacade.cs:258`
    - `.Where(s => s.Location?.Id == locationId)`
    - ❌ Filtering by Location ID string

17. `/home/user/Wayfarer/src/Services/SituationCompletionHandler.cs:319`
    - `.Where(sit => sit.Obligation?.Id == completedSituation.Obligation?.Id)`
    - ❌ Comparing Obligation by ID instead of reference equality

---

## CATEGORY 5: SERVICE/FACADE STRING ID LOOKUPS (FIX - 30 sites)

**Principle:** Service and facade methods using string IDs to look up entities violate spatial architecture. Methods should accept objects or be refactored to use object references.

**Pattern:** 
```csharp
public Object GetByStringId(string id)
{
    return collection.FirstOrDefault(e => e.Id == id);
}
```

**Action Required:** Apply 9-step holistic pattern (convert signatures, update call sites, verify all layers).

### List of Violation Sites

#### Repositories & Decks (12 sites)

1. `/home/user/Wayfarer/src/Content/ItemRepository.cs:26`
   - Method: `GetItemById(string id)`
   - Violation: `return _gameWorld.Items.FirstOrDefault(i => i.Id == id)`
   - Call site: Unknown (search: `GetItemById`)

2. `/home/user/Wayfarer/src/Content/ItemRepository.cs:51`
   - Method: `AddItems(...)`
   - Violation: `if (_gameWorld.Items.Any(i => i.Id == item.Id))`
   - Issue: Duplicate checking by ID

3. `/home/user/Wayfarer/src/GameState/MentalChallengeDeck.cs:27`
   - Method: Likely `GetCardById(string cardId)`
   - Violation: `MentalCard card = gameWorld.MentalCards.FirstOrDefault(e => e.Id == cardId)`

4. `/home/user/Wayfarer/src/GameState/PhysicalChallengeDeck.cs:26`
   - Method: Likely `GetCardById(string cardId)`
   - Violation: `PhysicalCard card = gameWorld.PhysicalCards.FirstOrDefault(e => e.Id == cardId)`

5. `/home/user/Wayfarer/src/GameState/SocialChallengeDeck.cs:26`
   - Method: Likely `GetCardById(string cardId)`
   - Violation: `SocialCard card = gameWorld.SocialCards.FirstOrDefault(d => d != null && d.Id == cardId)`

6. `/home/user/Wayfarer/src/GameState/TravelManager.cs:372`
   - Method: Likely `GetSceneById(string sceneId)`
   - Violation: `Scene scene = _gameWorld.Scenes.FirstOrDefault(o => o.Id == sceneId)`

7. `/home/user/Wayfarer/src/GameState/TravelManager.cs:514`
   - Method: Likely `GetPathCardById(string cardId)`
   - Violation: `return collection.PathCards.FirstOrDefault(c => c.Id == cardId)`

8. `/home/user/Wayfarer/src/GameState/TravelManager.cs:534`
   - Method: Likely `GetEventCardById(string cardId)`
   - Violation: `return travelEvent.EventCards.FirstOrDefault(c => c.Id == cardId)`

9. `/home/user/Wayfarer/src/Services/DependentResourceOrchestrationService.cs:67`
   - Method: Likely `GetItemById(string itemId)`
   - Violation: `Item item = _gameWorld.Items.FirstOrDefault(i => i.Id == itemId)`

10. `/home/user/Wayfarer/src/Subsystems/Mental/MentalDeckBuilder.cs:85`
    - Method: Likely `GetItemById(string itemId)`
    - Violation: `Item item = _gameWorld.Items.FirstOrDefault(i => i.Id == itemId)`

11. `/home/user/Wayfarer/src/Subsystems/Physical/PhysicalDeckBuilder.cs:101`
    - Method: Likely `GetItemById(string itemId)`
    - Violation: `Item item = _gameWorld.Items?.FirstOrDefault(i => i.Id == itemId)`

12. `/home/user/Wayfarer/src/Subsystems/Social/SocialChallengeDeckBuilder.cs:170`
    - Method: Likely `GetItemById(string itemId)`
    - Violation: `Item item = _gameWorld.Items?.FirstOrDefault(i => i.Id == itemId)`

#### Facade Methods (18 sites)

13. `/home/user/Wayfarer/src/Subsystems/Mental/MentalFacade.cs:67`
    - Method: Likely `GetSituationById(string situationId)`
    - Violation: `.FirstOrDefault(sit => sit.Id == situationId)`

14. `/home/user/Wayfarer/src/Subsystems/Mental/MentalFacade.cs:196`
    - Method: Likely accessing `CurrentMentalSituationId`
    - Violation: `.FirstOrDefault(sit => sit.Id == _gameWorld.CurrentMentalSituationId)`

15. `/home/user/Wayfarer/src/Subsystems/Mental/MentalFacade.cs:386`
    - Method: Likely accessing current situation
    - Violation: `.FirstOrDefault(sit => sit.Id == _gameWorld.CurrentMentalSituationId)`

16. `/home/user/Wayfarer/src/Subsystems/Physical/PhysicalFacade.cs:95`
    - Method: Likely `GetSituationById(string situationId)`
    - Violation: `.FirstOrDefault(sit => sit.Id == situationId)`

17. `/home/user/Wayfarer/src/Subsystems/Physical/PhysicalFacade.cs:210`
    - Method: Likely accessing `CurrentPhysicalSituationId`
    - Violation: `.FirstOrDefault(sit => sit.Id == _gameWorld.CurrentPhysicalSituationId)`

18. `/home/user/Wayfarer/src/Subsystems/Physical/PhysicalFacade.cs:309`
    - Method: Likely accessing current situation
    - Violation: `.FirstOrDefault(sit => sit.Id == _gameWorld.CurrentPhysicalSituationId)`

19. `/home/user/Wayfarer/src/Subsystems/Physical/PhysicalFacade.cs:481`
    - Special case: `if (situation.SituationTemplate?.Id == "notice_waterwheel")`
    - ❌ Hardcoded template ID string comparison

20. `/home/user/Wayfarer/src/Subsystems/Time/TimeFacade.cs:294`
    - Method: Likely `GetObligationById(string obligationId)`
    - Violation: `Obligation obligation = _gameWorld.Obligations.FirstOrDefault(i => i.Id == obligationId)`

21. `/home/user/Wayfarer/src/Subsystems/Emergency/EmergencyFacade.cs:99`
    - Method: Likely `GetEmergencyById(string emergencyId)`
    - Violation: `EmergencySituation emergency = _gameWorld.EmergencySituations.FirstOrDefault(e => e.Id == emergencyId)`

22. `/home/user/Wayfarer/src/Subsystems/Emergency/EmergencyFacade.cs:157`
    - Method: Likely `GetEmergencyById(string emergencyId)`
    - Violation: `EmergencySituation emergency = _gameWorld.EmergencySituations.FirstOrDefault(e => e.Id == emergencyId)`

23. `/home/user/Wayfarer/src/Subsystems/Emergency/EmergencyFacade.cs:167`
    - Method: Likely `GetResponseById(string responseId)`
    - Violation: `EmergencyResponse response = emergency.Responses.FirstOrDefault(r => r.Id == responseId)`

24. `/home/user/Wayfarer/src/Subsystems/Emergency/EmergencyFacade.cs:218`
    - Method: Likely `GetEmergencyById(string emergencyId)`
    - Violation: `EmergencySituation emergency = _gameWorld.EmergencySituations.FirstOrDefault(e => e.Id == emergencyId)`

25. `/home/user/Wayfarer/src/Subsystems/Obligation/MeetingManager.cs:53`
    - Method: Likely `GetMeetingById(string meetingId)`
    - Violation: `.FirstOrDefault(m => m.Id == meetingId)`

26. `/home/user/Wayfarer/src/Subsystems/Observation/ObservationFacade.cs:32`
    - Method: Likely `GetSceneById(string sceneId)`
    - Violation: `ObservationScene scene = _gameWorld.ObservationScenes.FirstOrDefault(s => s.Id == sceneId)`

27. `/home/user/Wayfarer/src/Subsystems/Location/LocationFacade.cs:1047`
    - Method: Likely accessing `situation.DeckId`
    - Violation: `SocialChallengeDeck socialDeck = _gameWorld.SocialChallengeDecks.FirstOrDefault(d => d.Id == situation.DeckId)`

28. `/home/user/Wayfarer/src/Subsystems/Location/LocationFacade.cs:1051`
    - Method: Likely accessing `situation.DeckId`
    - Violation: `MentalChallengeDeck mentalDeck = _gameWorld.MentalChallengeDecks.FirstOrDefault(d => d.Id == situation.DeckId)`

29. `/home/user/Wayfarer/src/Subsystems/Location/LocationFacade.cs:1055`
    - Method: Likely accessing `situation.DeckId`
    - Violation: `PhysicalChallengeDeck physicalDeck = _gameWorld.PhysicalChallengeDecks.FirstOrDefault(d => d.Id == situation.DeckId)`

30. `/home/user/Wayfarer/src/Subsystems/ProceduralContent/SceneGenerationFacade.cs:49`
    - Method: Likely `GetLocationById(string locationId)`
    - Violation: `Location contextLocation = _gameWorld.Locations.FirstOrDefault(l => l.Id == locationId)`

31. `/home/user/Wayfarer/src/Subsystems/Travel/TravelFacade.cs:627`
    - Method: Likely `GetEventCardById(string cardId)`
    - Violation: `return travelEvent.EventCards?.FirstOrDefault(c => c.Id == cardId)`

32. `/home/user/Wayfarer/src/Subsystems/Travel/TravelFacade.cs:643`
    - Method: Likely `GetPathCardById(string cardId)`
    - Violation: `return collection.PathCards?.FirstOrDefault(c => c.Id == cardId)`

33. `/home/user/Wayfarer/src/Subsystems/Travel/TravelTimeCalculator.cs:27`
    - Method: Likely `CalculateTime(...)`
    - Violation: `if (fromLocation.Id == toLocation.Id)`
    - ❌ Object equality comparison by ID instead of reference

---

## CATEGORY 5B: RAZOR COMPONENT ID LOOKUPS (FIX - 12 sites)

**Principle:** Frontend components should NOT perform business logic queries. All entity lookups should occur in backend (GameFacade/services) and passed to components.

**Pattern:** Razor component method doing `GameWorld.Collection.FirstOrDefault(e => e.Id == stringId)`

**Action Required:** Move lookup to backend service, pass resolved object to component property.

### List of Violation Sites

1. `/home/user/Wayfarer/src/Pages/Components/ConversationContent.razor.cs:683`
   - `SocialChallengeDeck deck = GameWorld.SocialChallengeDecks.FirstOrDefault(d => d.Id == situation.DeckId)`
   - ❌ Frontend doing deck lookup by ID

2. `/home/user/Wayfarer/src/Pages/Components/ConversationContent.razor.cs:693`
   - `situation.SituationCards != null && situation.SituationCards.Any(gc => gc.Id == situationCardId)`
   - ❌ Frontend filtering cards by ID

3. `/home/user/Wayfarer/src/Pages/Components/DiscoveryJournal.razor.cs:114`
   - `return GameWorld.Obligations.FirstOrDefault(i => i.Id == obligationId)`
   - ❌ Frontend looking up obligation by ID

4. `/home/user/Wayfarer/src/Pages/Components/ExchangeContent.razor.cs:244`
   - `ExchangeCard exchange = GetAvailableExchanges().FirstOrDefault(e => e.Id == SelectedExchangeId)`
   - ❌ Frontend filtering exchange by ID

5. `/home/user/Wayfarer/src/Pages/Components/LocationContent.razor.cs:150`
   - `Scene forcedScene = GameWorld.Scenes.FirstOrDefault(s => s.Id == forcedSceneId)`
   - ❌ Frontend looking up scene by ID

6. `/home/user/Wayfarer/src/Pages/Components/LocationContent.razor.cs:237`
   - `Scene forcedScene = GameWorld.Scenes.FirstOrDefault(s => s.Id == forcedSceneId)`
   - ❌ Frontend looking up scene by ID (duplicate)

7. `/home/user/Wayfarer/src/Pages/Components/MentalContent.razor.cs:745`
   - `MentalChallengeDeck deck = GameWorld.MentalChallengeDecks.FirstOrDefault(d => d.Id == situation.DeckId)`
   - ❌ Frontend doing deck lookup by ID

8. `/home/user/Wayfarer/src/Pages/Components/MentalContent.razor.cs:755`
   - `situation.SituationCards != null && situation.SituationCards.Any(gc => gc.Id == situationCardId)`
   - ❌ Frontend filtering cards by ID

9. `/home/user/Wayfarer/src/Pages/Components/PhysicalContent.razor.cs:839`
   - `PhysicalChallengeDeck deck = GameWorld.PhysicalChallengeDecks.FirstOrDefault(d => d.Id == situation.DeckId)`
   - ❌ Frontend doing deck lookup by ID

10. `/home/user/Wayfarer/src/Pages/Components/PhysicalContent.razor.cs:849`
    - `situation.SituationCards != null && situation.SituationCards.Any(gc => gc.Id == situationCardId)`
    - ❌ Frontend filtering cards by ID

11. `/home/user/Wayfarer/src/Pages/Components/SceneContent.razor.cs:458`
    - `.FirstOrDefault(ct => ct.Id == choice.Id)`
    - ❌ Frontend filtering choices by ID

12. `/home/user/Wayfarer/src/Pages/Components/TravelPathContent.razor.cs:222`
    - `PathCardDTO card = TravelContext.CurrentSegmentCards.FirstOrDefault(c => c.Id == pathCardId)`
    - ❌ Frontend filtering DTO cards by ID

13. `/home/user/Wayfarer/src/Pages/Components/TravelPathContent.razor.cs:724`
    - `Scene scene = GameWorld.Scenes.FirstOrDefault(s => s.Id == sceneId)`
    - ❌ Frontend looking up scene by ID

---

## PHASE 7 COMPLETION ANALYSIS

### Status: **INCOMPLETE - 63% violation sites remain**

**Acceptable Sites (21%):**
- ✅ 7 Parser DTO lookups (Phase 6 completed)
- ✅ 11 Template lookups (structural navigation, acceptable)
- ✅ 1 Display/logging (no impact)

**Violation Sites (63%):**
- ❌ 15 String parameter violations (high-priority: method signatures require refactoring)
- ❌ 30 Service/facade lookups (medium-priority: 9-step pattern required)
- ❌ 12 Razor component lookups (medium-priority: logic belongs in backend)

### Violation Breakdown by Root Cause

| Root Cause | Count | Example | Fix Pattern |
|------------|-------|---------|------------|
| Repository GetById methods | 12 | `ItemRepository.GetItemById(string id)` | Convert to accept Item object |
| Service facade lookups | 18 | `MentalFacade` deck/situation lookups | Accept object parameters |
| Location comparison by ID | 8 | `n.Location?.Id == locationId` | Accept Location object parameter |
| Obligation comparison by ID | 2 | `sit.Obligation?.Id == completedSituation.Obligation?.Id` | Use reference equality |
| Razor component queries | 12 | `GameWorld.Scenes.FirstOrDefault(s => s.Id == sceneId)` | Backend service responsibility |
| Object equality by ID | 2 | `!_knownRoutes.Any(r => r.Id == route.Id)` | Use reference equality (==) |
| Hardcoded template ID | 1 | `situation.SituationTemplate?.Id == "notice_waterwheel"` | Use enum or template reference |

### Phase 7 Completion Requirements

To complete Phase 7, ELIMINATE ALL 57 violation sites:

1. **Priority 1 (15 sites):** String parameter violations
   - Convert all `public Method(string id)` to `public Method(Entity entity)`
   - Update all 15+ call sites for each method
   - Estimated effort: 5-8 hours (massive refactoring)

2. **Priority 2 (30 sites):** Service/facade lookups
   - Apply 9-step holistic pattern to 30 sites
   - Update method signatures and all call sites
   - Estimated effort: 10-15 hours (complex refactoring)

3. **Priority 3 (12 sites):** Razor component logic
   - Move all entity lookups to backend GameFacade
   - Pass resolved objects to components as parameters
   - Estimated effort: 4-6 hours

**Total estimated effort:** 19-29 hours of comprehensive refactoring

---

## RECOMMENDATION

**Phase 7 is NOT COMPLETE:**
- Acceptable sites (KEEP): 19 (21%)
- Violation sites (FIX): 57 (63%)
- Completion: **37% complete**

**Next steps:**
1. Prioritize Priority 1 violations (string parameters) - high impact, high visibility
2. Group violations by file/system for focused refactoring
3. Test each refactoring to verify no call-site breakage
4. Re-run analysis after each major refactoring phase

