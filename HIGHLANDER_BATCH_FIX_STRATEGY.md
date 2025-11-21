# HIGHLANDER BATCH-FIX STRATEGY (468 Compilation Errors)

## EXECUTIVE SUMMARY

**Total errors**: 468 (down from 476 originally)
**Root cause**: HIGHLANDER refactoring deleted string IDs, replaced with object references
**Scope**: Parser layer complete, domain layer and UI layer need fixes

**Strategic approach**: Fix in 7 phases by error pattern, ordered to minimize cascading breaks

---

## PHASE 1: DELETED INVENTORY METHODS (54 errors)

### Error Pattern
```
Inventory" enthält keine Definition für "GetItemIds"
Inventory" enthält keine Definition für "AddItem"
Inventory" enthält keine Definition für "HasItem"
Inventory" enthält keine Definition für "RemoveItem"
Inventory" enthält keine Definition für "CanAddItem"
Inventory" enthält keine Definition für "GetItemCount"
```

### Root Cause
HIGHLANDER refactoring changed Inventory to store `List<Item>` instead of `Dictionary<string, int>`.

**OLD API (DELETED)**:
- `GetItemIds()` → returned List<string>
- `AddItem(string itemId)` → accepted ID
- `HasItem(string itemId)` → checked by ID
- `RemoveItem(string itemId)` → removed by ID
- `CanAddItem(string itemId, int quantity)` → checked capacity by ID

**NEW API (CORRECT)**:
- `GetAllItems()` → returns List<Item>
- `Add(Item item)` → accepts object
- `Contains(Item item)` → checks by object
- `Remove(Item item)` → removes by object
- `CanAddItem(Item item)` → checks capacity by object

### Affected Files (12)
1. `SpawnConditionsEvaluator.cs` (line 71) - `GetItemIds()`
2. `ArbitrageCalculator.cs` (line 246) - `GetItemIds()`
3. `MarketSubsystemManager.cs` (lines 523, 639) - `GetItemIds()`
4. `ResourceFacade.cs` (lines 244, 274) - `GetItemIds()`
5. `InventoryContent.razor.cs` (lines 29, 36) - `GetItemIds()`, `GetItemCount()`
6. `Player.cs` (line 496) - `AddItem()`
7. `DependentResourceOrchestrationService.cs` (line 82) - `AddItem()`
8. `ObligationActivity.cs` (line 302) - `AddItem()`
9. `RewardApplicationService.cs` (line 119) - `AddItem()`
10. `SituationCompletionHandler.cs` (line 191) - `AddItem()`
11. `SceneInstanceFacade.cs` (line 174) - `AddItem()`
12. `ExchangeFacade.cs` (lines 410, 415) - `HasItem()`, `RemoveItem()`
13. `ExchangeHandler.cs` (lines 163, 168) - `HasItem()`, `RemoveItem()`
14. `MarketSubsystemManager.cs` (lines 300, 391, 446, 572) - `CanAddItem()`

### Fix Strategy

**Pattern 1: GetItemIds() replacement**
```csharp
// BEFORE
List<string> itemIds = inventory.GetItemIds();
foreach (string itemId in itemIds) { ... }

// AFTER
List<Item> items = inventory.GetAllItems();
foreach (Item item in items) { ... }
```

**Pattern 2: AddItem() replacement**
```csharp
// BEFORE
inventory.AddItem(itemId);

// AFTER
inventory.Add(item); // item is object reference
```

**Pattern 3: HasItem() replacement**
```csharp
// BEFORE
if (inventory.HasItem(itemId))

// AFTER
if (inventory.Contains(item)) // item is object reference
```

**Pattern 4: RemoveItem() replacement**
```csharp
// BEFORE
inventory.RemoveItem(itemId);

// AFTER
inventory.Remove(item); // item is object reference
```

**Pattern 5: CanAddItem() signature change**
```csharp
// BEFORE
if (inventory.CanAddItem(itemId, quantity))

// AFTER
if (inventory.CanAddItem(item)) // Single parameter, checks weight
```

**Pattern 6: GetItemCount() replacement**
```csharp
// BEFORE
int count = inventory.GetItemCount(itemId);

// AFTER
int count = inventory.Count(item); // item is object reference
```

### Complexity
**MEDIUM** - Requires parameter conversion from string → Item object reference in each call site

---

## PHASE 2: DELETED PLAYER PROPERTIES (18 errors)

### Error Pattern
```
Player" enthält keine Definition für "ActiveObligationIds"
Player" enthält keine Definition für "CompletedSituationIds"
Player" enthält keine Definition für "InjuryCardIds"
Player" enthält keine Definition für "CollectedObservations"
```

### Root Cause
HIGHLANDER refactoring deleted string ID collections, replaced with object references.

**DELETED**:
- `Player.ActiveObligationIds` (List<string>)
- `Player.CompletedSituationIds` (List<string>)
- `Player.InjuryCardIds` (List<string>)
- `Player.CollectedObservations` (collection)

**CORRECT (EXISTING)**:
- `Player.ActiveObligations` (List<Obligation>) - object references
- No CompletedSituationIds - completed situations removed from GameWorld.Scenes
- No InjuryCardIds - injury cards added to deck directly
- No CollectedObservations - replaced with different system

### Affected Files (10)
1. `GameWorld.cs` (lines 489, 491, 529, 531, 569, 603, 605, 617) - `ActiveObligationIds`
2. `NumericRequirement.cs` (line 58) - `CompletedSituationIds`
3. `PhysicalDeckBuilder.cs` (lines 49, 459) - `InjuryCardIds`
4. `DiscoveryJournal.razor.cs` (lines 56, 61) - `CollectedObservations`

### Fix Strategy

**Pattern 1: ActiveObligationIds → ActiveObligations (object query)**
```csharp
// BEFORE
player.ActiveObligationIds.Add(obligationId);
player.ActiveObligationIds.Remove(obligationId);
bool hasObligation = player.ActiveObligationIds.Contains(obligationId);

// AFTER
player.ActiveObligations.Add(obligation); // Direct object manipulation
player.ActiveObligations.Remove(obligation);
bool hasObligation = player.ActiveObligations.Contains(obligation);
```

**Pattern 2: CompletedSituationIds → Query GameWorld**
```csharp
// BEFORE
int count = player.CompletedSituationIds.Count(id => condition);

// AFTER
// Completed situations removed from GameWorld.Scenes
// Query completion via other tracking (achievement, memory flags)
```

**Pattern 3: InjuryCardIds → Direct deck modification**
```csharp
// BEFORE
player.InjuryCardIds.Add(cardId);
List<string> injuries = player.InjuryCardIds;

// AFTER
// Injuries added directly to challenge deck
// No tracking collection needed
```

**Pattern 4: CollectedObservations → System removed**
```csharp
// BEFORE
player.CollectedObservations.Add(observation);

// AFTER
// System replaced - remove UI/logic references
```

### Complexity
**HIGH** - Requires understanding domain logic for each usage (some properties semantically removed)

---

## PHASE 3: PARAMETER TYPE MISMATCHES (76 errors)

### Error Pattern
```
Argument "1": Konvertierung von "string" in "NPC" nicht möglich
Argument "1": Konvertierung von "string" in "Location" nicht möglich
Argument "1": Konvertierung von "string" in "Item" nicht möglich
Argument "1": Konvertierung von "string" in "ConversationTree" nicht möglich
```

### Root Cause
Methods refactored to accept object references, but call sites still passing string IDs.

### Affected Files (20+)
- `NPCRepository.cs`, `RouteDiscoveryRepository.cs`, `PackageLoader.cs`, `TokenMechanicsManager.cs`
- `GameFacade.cs`, `SpawnConditionsEvaluator.cs`, `SituationCompletionHandler.cs`
- `LocationContent.razor.cs`, `GameScreen.razor.cs`, `ConversationTreeContent.razor.cs`
- `EmergencyContent.razor.cs`, `ExchangeContent.razor.cs`, `TravelFacade.cs`
- `DiscoveryJournal.razor.cs`, `PriceManager.cs`, `TravelPathContent.razor.cs`
- `StrangerList.razor.cs`, `TravelContent.razor.cs`, `SituationCard.razor`

### Fix Strategy

**Pattern 1: Find object from GameWorld, pass object**
```csharp
// BEFORE
someService.DoSomething(npcId); // string parameter

// AFTER
NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.Name == npcName);
someService.DoSomething(npc); // object parameter
```

**Pattern 2: Change method to accept object (if caller has object)**
```csharp
// Caller already has object but extracts ID:
var npc = GetNPC();
var id = npc.Id; // Extracting ID is WRONG
SomeMethod(id);

// CORRECT:
var npc = GetNPC();
SomeMethod(npc); // Pass object directly
```

### Complexity
**HIGH** - Requires analyzing each call site to determine if lookup or direct pass

---

## PHASE 4: DELETED ENTITY PROPERTIES - GENERAL (36 errors)

### Error Pattern
```
"Item" enthält keine Definition für "Id"
"NPC" enthält keine Definition für "ID"
"Location" enthält keine Definition für "Id"
"Scene" enthält keine Definition für "Id"
"Situation" enthält keine Definition für "Id"
"Region" enthält keine Definition für "Id"
"ExchangeCard" enthält keine Definition für "Id"
"DeliveryJob" enthält keine Definition für "Id"
"MeetingObligation" enthält keine Definition für "Id"
"ObservationAction" enthält keine Definition für "Id"
```

### Root Cause
HIGHLANDER deleted all `.Id` properties from domain entities.

### Affected Files (15+)
- `ItemRepository.cs`, `HexRouteGenerator.cs`, `LocationTags.cs`, `SceneInstanceFacade.cs`
- `ProceduralAStoryService.cs`, `SceneFacade.cs`, `SocialChallengeDeckBuilder.cs`
- `JobBoardView.razor.cs`, `MeetingManager.cs`, `ExchangeHandler.cs`

### Fix Strategy

**Pattern 1: Id used for logging/debugging**
```csharp
// BEFORE
Log.Information($"Processing item {item.Id}");

// AFTER
Log.Information($"Processing item {item.Name}"); // Use Name instead
```

**Pattern 2: Id used for comparison**
```csharp
// BEFORE
if (item.Id == targetId)

// AFTER
if (item == targetItem) // Direct object comparison
```

**Pattern 3: Id used for dictionary key**
```csharp
// BEFORE
Dictionary<string, Item> lookup = new();
lookup[item.Id] = item;

// AFTER
List<Item> items = new(); // Use List<T> instead (DOMAIN COLLECTION PRINCIPLE)
```

### Complexity
**MEDIUM** - Most usages are logging or comparison, straightforward to fix

---

## PHASE 5: DELETED ENTITY PROPERTIES - SPECIFIC (48 errors)

### Error Pattern
```
"Situation" enthält keine Definition für "LastChoiceId"
"SituationCard" enthält keine Definition für "Id"
"DialogueResponse" enthält keine Definition für "NextNodeId"
"RouteOption" enthält keine Definition für "EncounterDeckIds"
"Location" enthält keine Definition für "VenueName"
"ConversationTree" enthält keine Definition für "StartingNodeId"
"SituationCardRewards" enthält keine Definition für "EquipmentId"
"RouteSegment" enthält keine Definition für "MandatorySceneId"
"MentalSession" enthält keine Definition für "ObligationId"
```

### Root Cause
Domain entities refactored to remove redundant or ID-based properties.

### Affected Files (20+)
- `Scene.cs`, `SceneContent.razor.cs`, `ConversationContent.razor.cs`
- `MentalContent.razor.cs`, `PhysicalContent.razor.cs`, `RouteOption.cs`
- `PackageLoader.cs`, `ExchangeContent.razor.cs`, `SituationCompletionHandler.cs`
- `ConversationTreeFacade.cs`, `SituationCard.razor`, `TravelPathContent.razor`
- `MentalFacade.cs`, `DiscoveryJournal.razor`

### Fix Strategy

**Situation.LastChoiceId → LastChoice (object)**
```csharp
// BEFORE
string choiceId = situation.LastChoiceId;

// AFTER
SituationChoice lastChoice = situation.LastChoice; // Object reference
```

**DialogueResponse.NextNodeId → NextNode (object)**
```csharp
// BEFORE
string nextId = response.NextNodeId;

// AFTER
DialogueNode nextNode = response.NextNode; // Object reference
```

**RouteOption.EncounterDeckIds → Removed (generate procedurally)**
```csharp
// BEFORE
List<string> deckIds = route.EncounterDeckIds;

// AFTER
// Procedural generation - no pre-defined decks
```

**Location.VenueName → Location object reference**
```csharp
// BEFORE
string venueName = location.VenueName;

// AFTER
// Location IS the venue - use location.Name directly
```

**ConversationTree.StartingNodeId → RootNode (object)**
```csharp
// BEFORE
string startId = tree.StartingNodeId;

// AFTER
DialogueNode root = tree.RootNode; // Object reference
```

### Complexity
**HIGH** - Each property has different replacement pattern based on domain semantics

---

## PHASE 6: TYPE CONVERSION ERRORS (20 errors)

### Error Pattern
```
Der !=-Operator kann nicht auf Operanden vom Typ "Item" und "string" angewendet werden
Der Typ "Item" kann nicht in "string" konvertiert werden
Der Typ "string" kann nicht implizit in "Item" konvertiert werden
Der Typ "District" kann nicht implizit in "string" konvertiert werden
```

### Root Cause
Code comparing/assigning objects to strings (legacy ID-based logic).

### Affected Files (10)
- `RouteOption.cs` (lines 158, 298, 354)
- `TransportCompatibilityValidator.cs` (lines 106, 128)
- `DifficultyCalculationService.cs` (line 108)
- `TokenMechanicsManager.cs` (line 340)
- `TokenEffectProcessor.cs` (lines 100, 132, 179)
- `ObligationActivity.cs` (line 300)
- `SocialChallengeDeckBuilder.cs` (lines 169, 171)
- `LocationParser.cs` (lines 179, 182)
- `VenueParser.cs` (line 30)
- `PackageLoader.cs` (line 818)
- `SceneInstantiator.cs` (line 1131)

### Fix Strategy

**Item != string comparison**
```csharp
// BEFORE
if (requiredItem != itemId) // Item vs string comparison

// AFTER
if (requiredItem != item) // Item vs Item comparison
```

**Item cast to string**
```csharp
// BEFORE
(string)item // Cast to string

// AFTER
item.Name // Use property, not cast
```

**String assignment to Item**
```csharp
// BEFORE
Item item = itemId; // String to Item

// AFTER
Item item = _itemRepository.GetItemByName(itemId); // Lookup object
```

### Complexity
**MEDIUM** - Type errors are clear, fix patterns straightforward

---

## PHASE 7: DELETED DTO/STRUCT PROPERTIES (18 errors)

### Error Pattern
```
"WorkAction" enthält keine Definition für "Id"
"BondChange" enthält keine Definition für "NpcId"
"RouteDiscovery" enthält keine Definition für "RouteId"
"LocationDTO" enthält keine Definition für "VenueId"
"Region" enthält keine Definition für "DistrictIds"
"District" enthält keine Definition für "RegionId"
"RouteSegmentUnlock" enthält keine Definition für "PathDTO"
"NPCData" enthält keine Definition für "NpcId"
"CardContext" enthält keine Definition für "RequestId"
"ObligationIntroResult" enthält keine Definition für "ObligationId"
"ActiveObligation" enthält keine Definition für "ObligationId"
"ObligationProgressResult" enthält keine Definition für "ObligationId"
"ObligationCompleteResult" enthält keine Definition für "ObligationId"
"ObligationDiscoveryResult" enthält keine Definition für "ObligationId"
"RouteOptionViewModel" enthält keine Definition für "RouteId"
"NpcWithSituationsViewModel" enthält keine Definition für "Id"
"InventoryItemViewModel" enthält keine Definition für "ItemId"
```

### Root Cause
DTOs/ViewModels/Result objects still have ID properties that should use object references.

### Affected Files (10)
- `LocationParser.cs` (lines 170, 175, 176)
- `SceneArchetypeCatalog.cs` (line 353)
- `RouteDiscoveryRepository.cs` (line 19)
- `SceneInstantiator.cs` (line 1043)
- `PackageLoader.cs` (lines 609, 632, 633)
- `ObligationActivity.cs` (lines 111, 137, 171, 214, 265, 302, 344, 381)
- `LocationFacade.cs` (lines 368, 769)
- `ResourceFacade.cs` (line 254)
- `SituationCompletionHandler.cs` (line 276)
- `JsonNarrativeRepository.cs` (line 63)
- `MentalDeckBuilder.cs` (line 69)
- `PhysicalDeckBuilder.cs` (line 85)

### Fix Strategy

**Pattern 1: DTO property deleted - use object reference instead**
```csharp
// BEFORE
workAction.Id = "action_001";
workAction.VenueId = "venue_tavern";

// AFTER
workAction.Venue = venueObject; // Object reference, no IDs
```

**Pattern 2: ViewModel property deleted - pass object**
```csharp
// BEFORE
new RouteOptionViewModel { RouteId = route.Id }

// AFTER
new RouteOptionViewModel { Route = route } // Object reference
```

**Pattern 3: Result object property deleted - return object**
```csharp
// BEFORE
return new ObligationIntroResult { ObligationId = obligation.Id };

// AFTER
return new ObligationIntroResult { Obligation = obligation }; // Object reference
```

### Complexity
**MEDIUM** - Requires checking each DTO/ViewModel structure and updating consumers

---

## PHASE 8: MISCELLANEOUS (12 errors)

### Remaining Errors
1. `ExchangeContext.cs` (line 87) - Dictionary<ConnectionType, int> vs List<TokenCount>
2. `ExchangeCard.cs` (lines 196, 197) - RequiredVenueId not in scope
3. `RouteSegment.cs` (line 223) - RouteSegment.Paths deleted property
4. `Player.cs` (line 510) - GetUsedWeight() signature changed
5. `ServiceConfiguration.cs` (line 104) - NavigationManager missing using
6. `SceneFacade.cs` (line 147) - Variable name collision
7. `LocationFacade.cs` (line 978) - Variable name collision
8. `ExchangeHandler.cs` (line 235) - TokenCount vs KeyValuePair conversion
9. `ExchangeContent.razor.cs` (lines 431, 490) - TokenCount vs KeyValuePair conversion
10. `LocationContent.razor` (line 174) - EventCallback method group conversion
11. `ConversationContent.razor.cs` (line 351) - SocialChallengeContext.ConversationType deleted
12. Various enum value deletions (PersonalityType.NEUTRAL, Professions.Laborer, etc.)

### Fix Strategy
Each error requires individual analysis - these are one-off refactoring issues.

---

## EXECUTION ORDER (MINIMIZE CASCADING FAILURES)

### Why This Order?

1. **Phase 1 (Inventory)** - Self-contained system, fixes don't break other systems
2. **Phase 2 (Player properties)** - Core state changes, needed before service-layer fixes
3. **Phase 4 (Entity .Id deletions)** - General pattern, fixes enable Phase 3 lookups
4. **Phase 6 (Type conversions)** - Mechanical fixes, clear error messages
5. **Phase 7 (DTO/ViewModel)** - Contract layer between domain and UI
6. **Phase 3 (Parameter types)** - Requires Phase 1-6 fixes to provide objects for parameters
7. **Phase 5 (Specific properties)** - Complex semantic changes, benefits from other fixes
8. **Phase 8 (Miscellaneous)** - One-off fixes after systematic patterns resolved

### Parallel Execution
Phases 1, 2, 4, 6 can be worked **in parallel** (independent error clusters).

---

## RISK ASSESSMENT

### HIGH RISK (Manual Review Required)
- Phase 2: Player.CompletedSituationIds removal (domain logic change)
- Phase 5: Specific property semantics (each property different)
- Phase 3: Parameter lookups (may reveal missing GameWorld access)

### MEDIUM RISK (Pattern-Based)
- Phase 1: Inventory API changes (clear old→new mapping)
- Phase 4: Entity .Id deletions (mostly logging/debugging)
- Phase 7: DTO/ViewModel updates (mechanical refactoring)

### LOW RISK (Mechanical)
- Phase 6: Type conversions (compiler errors guide fixes)
- Phase 8: Miscellaneous (isolated issues)

---

## SUCCESS CRITERIA

**Phase complete when**: `dotnet build` shows 0 errors for that phase's file set
**Final success**: `cd src && dotnet build` succeeds with 0 errors

---

## ESTIMATED EFFORT

| Phase | Errors | Files | Complexity | Effort (hours) |
|-------|--------|-------|------------|----------------|
| 1     | 54     | 14    | MEDIUM     | 2-3            |
| 2     | 18     | 10    | HIGH       | 3-4            |
| 3     | 76     | 20+   | HIGH       | 4-6            |
| 4     | 36     | 15    | MEDIUM     | 2-3            |
| 5     | 48     | 20+   | HIGH       | 4-5            |
| 6     | 20     | 10    | MEDIUM     | 1-2            |
| 7     | 18     | 10    | MEDIUM     | 2-3            |
| 8     | 12     | 12    | HIGH       | 2-3            |
| **TOTAL** | **282** | **111+** | **MIXED** | **20-29 hours** |

**Note**: Some errors double-counted (single line has multiple issues), actual unique errors ~282.

---

## GORDON RAMSAY PRINCIPLE ENFORCEMENT

This refactoring is CORRECT. The errors exist because the codebase is transitioning from:
- ❌ String IDs everywhere (redundant, brittle, NO type safety)
- ✓ Object references only (HIGHLANDER, type-safe, compiler-verified)

**We do NOT add compatibility layers.**
**We do NOT preserve deleted properties "temporarily".**
**We FIX FORWARD - delete first, fix after.**

This batch-fix strategy completes the HIGHLANDER refactoring systematically.

**NO SHORTCUTS. NO HALF-MEASURES. FIX IT RIGHT.**
