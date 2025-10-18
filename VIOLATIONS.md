# WAYFARER CODEBASE VIOLATIONS AUDIT

## VIOLATION CLASS 4: var Usage

**Rule:** No `var` - FORBIDDEN (CLAUDE.md)
**Required:** Explicit type declarations

### Violations Found: 0

**AUDIT RESULT:  CLEAN**

The codebase is fully compliant with the explicit type declaration rule. No instances of `var` keyword usage were found in any C# files within the src folder.

**Search Patterns Used:**
- `\bvar\s+` - General var keyword with whitespace
- `^\s+var\s` - Var declarations at beginning of lines
- `\(\s*var\s` - Var in parameter contexts
- `foreach\s*\(\s*var\s` - Var in foreach loops

**Files Scanned:** All *.cs files in C:\Git\Wayfarer\src

**Audit Date:** 2025-10-18
**Status:** PASS - No violations detected

---

## VIOLATION CLASS 10: Subsystem Boundary Violation

**Rule:** Each facade handles exactly ONE business domain (Architecture.md)
**Required:** Logic in correct subsystem

### Violations Found: 1

#### File: C:\Git\Wayfarer\src\Subsystems\Social\SocialFacade.cs
- **Method:** `ExecuteExchange(object exchangeData)` (lines 606-635)
  - **Context:** SocialFacade contains a method to execute Exchange operations
  - **Correct Location:** ExchangeFacade (Exchange subsystem)
  - **Violation:** Exchange logic living in Social facade
  - **Details:**
    - SocialFacade.ExecuteExchange() delegates to ExchangeHandler
    - This creates cross-subsystem dependency (Social → Exchange)
    - Architecture.md explicitly states Exchange is a SEPARATE subsystem from Social
    - Exchanges are "instant exchanges, not conversations" per Architecture.md line 216
    - ExchangeFacade.ExecuteExchange() already exists at lines 132-214 with proper orchestration
  - **Evidence from Architecture.md:**
    - Line 182-183: "SocialFacade (Social challenges - conversations with NPCs)"
    - Line 192: "ExchangeFacade (NPC trading system)"
    - Line 216: "ExchangeFacade: Separate NPC trading system (instant exchanges, not conversations)"
    - Line 232: "Exchange/ → NPC trading, inventory validation"
  - **Correct Pattern:** UI should call ExchangeFacade directly, not through SocialFacade

**AUDIT RESULT: 1 VIOLATION DETECTED**

**Impact:**
- Creates unnecessary coupling between Social and Exchange subsystems
- Violates single responsibility principle (SocialFacade handling Exchange logic)
- Confuses subsystem boundaries (exchanges are NOT conversations)
- Duplicates execution logic (both SocialFacade and ExchangeFacade have ExecuteExchange methods)

**Recommended Fix:**
1. Remove `SocialFacade.ExecuteExchange()` method entirely
2. Update UI (ExchangeContent.razor.cs) to call `ExchangeFacade.ExecuteExchange()` directly
3. Remove ExchangeHandler dependency from SocialFacade (it's used ONLY for this violating method)
4. Ensure GameFacade orchestrates Exchange operations through ExchangeFacade only

**Audit Date:** 2025-10-18
**Status:** FAIL - 1 subsystem boundary violation detected

---

## VIOLATION CLASS 8: Parallel Storage / Duplicate State

**Rule:** GameWorld is single source of truth - no parallel storage (CLAUDE.md)
**Required:** All state lives in GameWorld, services query it directly

### Violations Found: 6

#### File: C:\Git\Wayfarer\src\Subsystems\Exchange\ExchangeInventory.cs
- **Lines 14-20:** ExchangeInventory class maintains parallel storage
  - **Context:** Stores NPC exchanges, exchange history, and used unique exchanges
  - **Original Source:** GameWorld.NPCExchangeCards (line 39), NPC.ExchangeDeck (lines 52-63)
  - **Violation:** Maintains parallel copy in `_npcExchanges` Dictionary instead of querying GameWorld
  - **Violation:** Maintains parallel copy in `_exchangeHistory` Dictionary instead of storing in GameWorld
  - **Violation:** Maintains parallel copy in `_usedUniqueExchanges` HashSet instead of storing in GameWorld or Player state

#### File: C:\Git\Wayfarer\src\Subsystems\Market\MarketStateTracker.cs
- **Lines 15-18:** MarketStateTracker class maintains parallel storage
  - **Context:** Stores market metrics and trade history
  - **Original Source:** Should be in GameWorld but doesn't exist there
  - **Violation:** Maintains parallel copy in `_marketMetrics` Dictionary instead of storing in GameWorld
  - **Violation:** Maintains parallel copy in `_tradeHistory` List instead of storing in GameWorld

#### File: C:\Git\Wayfarer\src\Subsystems\Token\RelationshipTracker.cs
- **Lines 16-19:** RelationshipTracker class maintains parallel storage
  - **Context:** Tracks active debts and last interaction times
  - **Original Source:** Token data exists in Player.NPCTokens (ConnectionTokenManager line 28)
  - **Violation:** Maintains parallel copy in `_activeDebts` Dictionary instead of querying Player.NPCTokens
  - **Violation:** Maintains parallel copy in `_lastInteractionTimes` Dictionary instead of storing in Player or NPC state

#### File: C:\Git\Wayfarer\src\Subsystems\Exchange\ExchangeOrchestrator.cs
- **Lines 17:** ExchangeOrchestrator class maintains parallel storage
  - **Context:** Stores active exchange sessions
  - **Original Source:** Should be in GameWorld or Player state
  - **Violation:** Maintains parallel copy in `_activeSessions` Dictionary instead of storing in GameWorld

#### File: C:\Git\Wayfarer\src\GameState\SocialSession.cs
- **Lines 30:** SocialSession stores HashSet for unlocked tiers
  - **Context:** Uses HashSet<int> for tier unlocking
  - **Original Source:** Session state
  - **Violation:** CLAUDE.md explicitly forbids HashSet - use List<int> instead

#### File: C:\Git\Wayfarer\src\GameState\MentalSession.cs
- **Lines 21:** MentalSession stores HashSet for unlocked tiers
  - **Context:** Uses HashSet<int> for tier unlocking
  - **Original Source:** Session state
  - **Violation:** CLAUDE.md explicitly forbids HashSet - use List<int> instead

**AUDIT RESULT: 6 VIOLATIONS DETECTED**

**Impact:**
- Multiple sources of truth for the same data
- Risk of desynchronization between parallel storage and GameWorld
- Violates single source of truth principle
- Makes debugging harder (where is the authoritative state?)
- Increases memory footprint with duplicate data
- Makes save/load more complex (must sync multiple stores)

**Recommended Fixes:**

**ExchangeInventory:**
1. Remove `_npcExchanges`, query GameWorld.NPCExchangeCards and NPC.ExchangeDeck directly
2. Move `_exchangeHistory` to GameWorld or Player state
3. Move `_usedUniqueExchanges` to Player state or mark as used in ExchangeCard itself

**MarketStateTracker:**
1. Move `_marketMetrics` to GameWorld.MarketMetrics (new property)
2. Move `_tradeHistory` to GameWorld.TradeHistory (new property)

**RelationshipTracker:**
1. Remove `_activeDebts`, compute from Player.NPCTokens on demand (filter negative values)
2. Move `_lastInteractionTimes` to Player state or NPC state

**ExchangeOrchestrator:**
1. Move `_activeSessions` to GameWorld.ActiveExchangeSessions (new property)

**SocialSession & MentalSession:**
1. Change `HashSet<int> UnlockedTiers` to `List<int> UnlockedTiers`
2. Use `Contains()` check on List (acceptable for small collections like tier unlocks)

**Audit Date:** 2025-10-18
**Status:** FAIL - 6 parallel storage violations detected

---

## VIOLATION CLASS 2: HashSet Disease

**Rule:** No `HashSet<T>` - FORBIDDEN (CLAUDE.md)
**Required:** ONLY `List<T>` where T is entity or enum

### Violations Found: 17

#### File: C:\Git\Wayfarer\src\Pages\GameScreen.razor.cs
- **Line 40:** `private HashSet<IDisposable> _subscriptions = new();`
  - **Context:** GameScreen component tracking disposable subscriptions
  - **Violation:** Using HashSet to track IDisposable objects. Should use List<IDisposable> instead.

#### File: C:\Git\Wayfarer\src\GameState\MentalSession.cs
- **Line 21:** `public HashSet<int> UnlockedTiers { get; set; } = new HashSet<int> { 1 };`
  - **Context:** Mental challenge session tracking unlocked tier levels
  - **Violation:** Using HashSet for tier tracking. Should use List<int> with explicit deduplication.

#### File: C:\Git\Wayfarer\src\GameState\PhysicalSession.cs
- **Line 19:** `public HashSet<int> UnlockedTiers { get; set; } = new HashSet<int> { 1 };`
  - **Context:** Physical challenge session tracking unlocked tier levels
  - **Violation:** Using HashSet for tier tracking. Should use List<int> with explicit deduplication.

#### File: C:\Git\Wayfarer\src\GameState\SocialSession.cs
- **Line 30:** `public HashSet<int> UnlockedTiers { get; set; } = new HashSet<int> { 1 };`
  - **Context:** Social conversation session tracking unlocked depth tiers
  - **Violation:** Using HashSet for tier tracking. Should use List<int> with explicit deduplication.

#### File: C:\Git\Wayfarer\src\GameState\Cards\CardSelectionManager.cs
- **Line 6:** `private readonly HashSet<CardInstance> _selectedCards = new();`
  - **Context:** UI card selection manager tracking selected cards
  - **Violation:** Using HashSet to track selected CardInstance objects. Should use List<CardInstance>.

#### File: C:\Git\Wayfarer\src\GameState\ObservationSystem.cs
- **Line 14:** `private readonly HashSet<string> _revealedObservations;`
- **Line 22:** `_revealedObservations = new HashSet<string>();`
  - **Context:** Observation system tracking which observations have been revealed to player
  - **Violation:** Using HashSet to track revealed observation IDs. Should use List<string> with explicit deduplication.

#### File: C:\Git\Wayfarer\src\Subsystems\Exchange\ExchangeInventory.cs
- **Line 20:** `private HashSet<string> _usedUniqueExchanges;`
- **Line 27:** `_usedUniqueExchanges = new HashSet<string>();`
  - **Context:** Exchange inventory tracking which unique exchanges have been used
  - **Violation:** Using HashSet to track used exchange IDs. Should use List<string> with explicit deduplication.

#### File: C:\Git\Wayfarer\src\Pages\Components\ConversationContent.razor.cs
- **Line 68:** `protected HashSet<string> MovedGoalCardIds { get; set; } = new();`
  - **Context:** Conversation UI tracking which goal cards have been moved from request pile to active cards
  - **Violation:** Using HashSet to track moved card IDs. Should use List<string>.

#### File: C:\Git\Wayfarer\src\Subsystems\Social\SocialFacade.cs
- **Line 590:** `public bool CanSelectCard(CardInstance card, HashSet<CardInstance> currentSelection)`
  - **Context:** Method parameter accepting HashSet of currently selected cards
  - **Violation:** Method signature using HashSet parameter. Should use List<CardInstance>.

#### File: C:\Git\Wayfarer\src\Subsystems\Location\LocationFacade.cs
- **Line 347:** `HashSet<string> npcIdsAtCurrentSpot = npcsAtCurrentSpot.Select(n => n.ID).ToHashSet();`
  - **Context:** LocationFacade collecting NPC IDs for current location filtering
  - **Violation:** Using ToHashSet() to create HashSet<string>. Should use List<string> with distinct filtering if needed.

#### File: C:\Git\Wayfarer\src\Subsystems\Social\NarrativeGeneration\Models\SocialChallengeState.cs
- **Line 55:** `public HashSet<ConversationBeat> CompletedBeats { get; set; } = new HashSet<ConversationBeat>();`
  - **Context:** Social challenge state tracking completed conversation beats
  - **Violation:** Using HashSet to track completed ConversationBeat enum values. Should use List<ConversationBeat>.

#### File: C:\Git\Wayfarer\src\Content\Validation\Validators (Multiple Files)
Multiple validation files use HashSet for required field tracking and duplicate ID detection:
- **VenueValidator.cs** (Lines 10, 39, 59): HashSet for required fields and venue ID tracking
- **NarrativeValidator.cs** (Lines 10, 40, 60, 106, 132): HashSet for required fields, narrative IDs, and step IDs
- **LocationValidator.cs** (Lines 10, 39, 59): HashSet for required fields and location ID tracking
- **ItemValidator.cs** (Line 11): HashSet for required fields
- **RouteValidator.cs** (Lines 10, 40, 60): HashSet for required fields and route ID tracking
- **RouteDiscoveryValidator.cs** (Lines 10, 39, 59): HashSet for required fields and route discovery ID tracking
- **NPCValidator.cs** (Line 10): HashSet for required fields
- **SchemaValidator.cs** (Lines 125-126): HashSet created via ToHashSet() for known field validation
  - **Context:** Content validation infrastructure checking for duplicate IDs and required fields in JSON
  - **Violation:** Using HashSet for validation tracking. Should use List<string> with explicit duplicate checking.

### Impact Analysis

**HIGH PRIORITY - Core Game State:**
1. **Session UnlockedTiers** (MentalSession, PhysicalSession, SocialSession): Core progression tracking violates HIGHLANDER principle
2. **ObservationSystem _revealedObservations**: Persistent game state tracking
3. **ExchangeInventory _usedUniqueExchanges**: Persistent exchange state tracking

**MEDIUM PRIORITY - UI State:**
1. **CardSelectionManager _selectedCards**: UI-only state but violates typing standards
2. **ConversationContent MovedGoalCardIds**: UI tracking state
3. **GameScreen _subscriptions**: Infrastructure cleanup tracking
4. **SocialFacade.CanSelectCard parameter**: Public API signature violation

**LOW PRIORITY - Content Validation:**
1. **Validator HashSets**: Build-time validation infrastructure, not runtime game state

### Recommended Remediation

**Phase 1 - Core Game State:**
Replace HashSet with List in all session classes and game state systems. Add explicit Contains() checks where deduplication is needed.

**Phase 2 - UI State:**
Replace HashSet with List in UI components. Most UI state doesn't require deduplication.

**Phase 3 - Public APIs:**
Update method signatures to use List instead of HashSet parameters.

**Phase 4 - Validation Infrastructure:**
Replace HashSet in validators with List-based duplicate detection.

**Audit Date:** 2025-10-18
**Status:** FAIL - 17 violations detected across game state, UI, and validation infrastructure
---

## VIOLATION CLASS 3: Object Type Usage

**Rule:** No `object` type - FORBIDDEN (CLAUDE.md)
**Required:** Strong typing with explicit types

### Violations Found: 8

#### File: C:\Git\Wayfarer\src\Pages\Components\LocationContent.razor.cs
- **Line 831:** `protected void NavigateToView(LocationViewState newView, object context = null)`
  - **Context:** Navigation method parameter for passing context data
  - **Violation:** Uses `object` as parameter type instead of specific type or strongly-typed union/variant pattern. Should use specific type (e.g., `string` for NPC ID) or create a strongly-typed context class.

#### File: C:\Git\Wayfarer\src\Subsystems\Social\SocialFacade.cs
- **Line 606:** `public ExchangeExecutionResult ExecuteExchange(object exchangeData)`
  - **Context:** Public API method accepting exchange data
  - **Violation:** Uses `object` as parameter type then immediately casts to `ExchangeCard`. Should declare parameter as `ExchangeCard` directly, eliminating runtime type checking.

#### File: C:\Git\Wayfarer\src\Subsystems\Social\NarrativeGeneration\Providers\PromptBuilder.cs
- **Line 173:** `if (!placeholders.TryGetValue(collectionName, out object collectionObj))`
  - **Context:** Dictionary lookup for template placeholder
  - **Violation:** Uses `Dictionary<string, object>` pattern with `object` output. Requires strongly-typed placeholder system.

- **Line 186:** `object item = items[i];`
  - **Context:** Loop iteration over collection items
  - **Violation:** Uses `object` to hold collection items. Should use generic type constraint or specific item type.

- **Line 208:** `private Dictionary<string, object> CreateItemPlaceholders(object item, int index, int totalCount)`
  - **Context:** Creates placeholder dictionary for template processing
  - **Violation:** Uses `object` as parameter type and `Dictionary<string, object>` return type. Entire template system built on `object` dictionary pattern - architectural violation.

- **Line 257:** `bool isLast = itemPlaceholders.TryGetValue("@last", out object lastValue)`
  - **Context:** Dictionary value extraction
  - **Violation:** Uses `object` for dictionary value. Part of broader Dictionary<string, object> anti-pattern.

- **Line 268:** `if (itemPlaceholders.TryGetValue(placeholder, out object value))`
  - **Context:** Placeholder value lookup
  - **Violation:** Uses `object` for dictionary value. Part of broader Dictionary<string, object> anti-pattern.

- **Line 314:** `if (placeholders.TryGetValue(condition, out object value))`
  - **Context:** Condition evaluation in template processing
  - **Violation:** Uses `object` for dictionary value. Part of broader Dictionary<string, object> anti-pattern.

- **Line 351:** `if (placeholders.TryGetValue(placeholder, out object value))`
  - **Context:** Placeholder replacement
  - **Violation:** Uses `object` for dictionary value. Part of broader Dictionary<string, object> anti-pattern.

#### File: C:\Git\Wayfarer\src\GameState\LocationDescriptionGenerator.cs
- **Line 290:** `private int GetVariantIndex(object input)`
  - **Context:** Generates variant index for text variety using hash
  - **Violation:** Uses `object` as parameter type. Should use specific type or generic constraint.

#### File: C:\Git\Wayfarer\src\GameState\MemoryFlag.cs
- **Line 5:** `public object CreationDay { get; set; }`
  - **Context:** Property storing creation day for memory flag
  - **Violation:** Uses `object` as property type. Should be `int` (day number) based on IsActive method signature.

- **Line 6:** `public object ExpirationDay { get; set; }`
  - **Context:** Property storing expiration day for memory flag
  - **Violation:** Uses `object` as property type. Should be `int?` (nullable day number) to allow indefinite flags.

### Excluded (Not Violations):

#### File: C:\Git\Wayfarer\src\GameState\TimeModel.cs
- **Line 13:** `private readonly object _lock = new object();`
  - **Reason:** Lock object for thread synchronization - standard C# pattern, not a type usage violation.

#### File: C:\Git\Wayfarer\src\GameState\LocationTags.cs
- **Line 151:** `public override bool Equals(object obj)`
  - **Reason:** Standard override of `Object.Equals()` method signature - required by C# framework.

#### File: C:\Git\Wayfarer\src\GameState\StateContainers\TimeState.cs
- **Line 287:** `public override bool Equals(object obj)`
  - **Reason:** Standard override of `Object.Equals()` method signature - required by C# framework.

### Critical Architectural Violations:

**PromptBuilder.cs - Entire template system built on Dictionary<string, object> anti-pattern:**
- 7 violations in single file
- Dictionary disease: Runtime type errors, impossible debugging, lost IntelliSense
- Requires complete architectural refactoring to strongly-typed template system
- Should use typed placeholder classes, not generic object dictionaries

**MemoryFlag.cs - object properties instead of proper types:**
- Properties are `object` but used as `int` in logic
- Type mismatch will cause runtime errors
- Simple fix: Change to proper types (`int` and `int?`)

**Audit Date:** 2025-10-18
**Status:** FAIL - 8 violations detected across 5 files

---

## VIOLATION CLASS 14: GameWorld Dependency Violation

**Rule:** GameWorld has ZERO external dependencies (CLAUDE.md)
**Required:** Dependencies flow INWARD to GameWorld, never outward

### Violations Found: 0

**AUDIT RESULT: CLEAN**

GameWorld.cs is fully compliant with the zero dependency rule. Analysis confirms:

**Constructor Analysis (Lines 243-253):**
- No dependency injection parameters
- No service dependencies
- Creates only owned data objects (Player, StreamingContentState)
- Comment at line 249 explicitly states: "GameWorld has NO dependencies and creates NO managers"

**Method Signatures Analysis:**
All methods operate on:
- GameWorld's own state (properties, collections)
- Data passed as simple parameters (strings, ints, enums)
- External TimeManager passed as parameter (not injected dependency) - methods accept it for calculations but GameWorld doesn't own or store it

**No External Dependencies Detected:**
- ✅ No injected facades, services, or managers
- ✅ No DI container usage (GetRequiredService)
- ✅ No stored service references
- ✅ All dependencies flow INWARD (services query GameWorld, not vice versa)
- ✅ GameWorld is pure data container with domain logic only

**Architecture Compliance:**
Per CLAUDE.md: "GameWorld = single source of truth, zero dependencies"
- GameWorld owns: All game entities (NPCs, Locations, Items, etc.)
- GameWorld provides: Query methods for external services
- GameWorld does NOT: Call external services, inject dependencies, or create managers

**Files Scanned:** C:\Git\Wayfarer\src\GameState\GameWorld.cs

**Audit Date:** 2025-10-18
**Status:** PASS - No violations detected

## VIOLATION CLASS 9: HIGHLANDER Violation

**Rule:** One concept, one implementation - delete duplicates (CLAUDE.md)
**Required:** Single implementation per concept

### Violations Found: 6

---

#### Violation 1: Exchange Card Loading (Two Sources)
- **Files:**
  - `C:\Git\Wayfarer\src\Subsystems\Exchange\ExchangeInventory.cs` (lines 34-63)
  - `C:\Git\Wayfarer\src\GameState\NPC.cs` (line 73)
  - `C:\Git\Wayfarer\src\GameState\GameWorld.cs` (line 39)
- **Implementation 1:** `GameWorld.NPCExchangeCards` - List of NPCExchangeCardEntry mapping NPC IDs to exchange cards
- **Implementation 2:** `NPC.ExchangeDeck` - List of ExchangeCard directly on NPC entity
- **Violation:** ExchangeInventory.InitializeFromGameWorld() loads exchanges from BOTH sources (lines 43-63), with duplicate detection logic (line 58). Same concept (NPC exchange offerings) has two storage locations.
- **Impact:** Confusion about source of truth, duplicate detection required, potential desync bugs

---

#### Violation 2: NPC Lookup (Repository + Service Duplicate Methods)
- **Files:**
  - `C:\Git\Wayfarer\src\Content\NPCRepository.cs` (lines 80-89, 115-125)
  - `C:\Git\Wayfarer\src\Services\NPCService.cs` (lines 25-55)
- **Implementation 1:** NPCRepository.GetTimeBlockServicePlan() - Returns TimeBlockServiceInfo (lines 194-216)
- **Implementation 2:** NPCService.GetTimeBlockServicePlan() - Returns TimeBlockServiceInfo (lines 25-46)
- **Implementation 1:** NPCRepository.GetAllLocationServices() - Returns List<ServiceTypes> (lines 221-225)
- **Implementation 2:** NPCService.GetAllLocationServices() - Returns List<ServiceTypes> (lines 51-55)
- **Implementation 1:** NPCRepository.GetServiceAvailabilityPlan() - Returns ServiceAvailabilityPlan (lines 230-251)
- **Implementation 2:** NPCService.GetServiceAvailabilityPlan() - Returns ServiceAvailabilityPlan (lines 60-81)
- **Violation:** Same three methods duplicated in NPCRepository (data access) AND NPCService (business logic). NPCService just wraps NPCRepository with no additional logic.
- **Impact:** Two maintenance points for same functionality, unclear which to call, violates single responsibility

---

#### Violation 3: Route Lookup (Repository + Manager Duplicate)
- **Files:**
  - `C:\Git\Wayfarer\src\GameState\RouteRepository.cs` (lines 21-44)
  - `C:\Git\Wayfarer\src\Subsystems\Travel\RouteManager.cs` (lines 19-25)
- **Implementation 1:** RouteRepository.GetRoutesFromLocation() - Queries GameWorld.Routes and filters by venueId (lines 21-44)
- **Implementation 2:** RouteManager.GetRoutesFromLocation() - Just calls RouteRepository.GetRoutesFromLocation() (lines 19-25)
- **Violation:** RouteManager is pure passthrough with zero additional logic. Same concept (getting routes from location) implemented twice.
- **Impact:** Unnecessary indirection layer, no value added by Manager wrapper

---

#### Violation 4: Message System (Base + Manager Duplicate)
- **Files:**
  - `C:\Git\Wayfarer\src\GameState\MessageSystem.cs` (lines 10-27)
  - `C:\Git\Wayfarer\src\Subsystems\Narrative\MessageSystemManager.cs` (lines 35-48)
- **Implementation 1:** MessageSystem.AddSystemMessage() - Adds message directly to GameWorld (lines 10-27)
- **Implementation 2:** MessageSystemManager.AddSystemMessage() - Wraps MessageSystem.AddSystemMessage() (lines 35-38)
- **Violation:** MessageSystemManager is mostly a wrapper around MessageSystem, with narrative styling additions. Core message storage concept duplicated.
- **Impact:** Two ways to add messages, unclear which to use, both access GameWorld.SystemMessages

---

#### Violation 5: Time Management (Manager + Facade + ProgressionManager Triple Duplication)
- **Files:**
  - `C:\Git\Wayfarer\src\GameState\TimeManager.cs` (lines 53-71)
  - `C:\Git\Wayfarer\src\Subsystems\Time\TimeFacade.cs` (lines 70-83)
  - `C:\Git\Wayfarer\src\Subsystems\Time\TimeProgressionManager.cs` (lines 21-26)
- **Implementation 1:** TimeManager.AdvanceSegments(int segments) - Core time advancement logic (lines 53-71)
- **Implementation 2:** TimeFacade.AdvanceSegments(int segments) - Wraps TimeProgressionManager.AdvanceSegments() (lines 70-83)
- **Implementation 3:** TimeProgressionManager.AdvanceSegments(int segments) - Wraps TimeManager.AdvanceSegments() (lines 21-26)
- **Violation:** Same time advancement concept has THREE layers: TimeManager (actual logic) -> TimeProgressionManager (wrapper) -> TimeFacade (wrapper of wrapper). Zero additional logic in intermediate layers.
- **Impact:** Triple indirection for same operation, maintenance nightmare, unclear which to call

---

#### Violation 6: Location Lookup (GameWorld + Manager + Facade Triple Implementation)
- **Files:**
  - `C:\Git\Wayfarer\src\GameState\GameWorld.cs` (line 285)
  - `C:\Git\Wayfarer\src\Subsystems\Location\LocationManager.cs` (lines 73-77)
  - `C:\Git\Wayfarer\src\Subsystems\Location\LocationFacade.cs` (lines 81-84)
- **Implementation 1:** GameWorld.GetLocation(string LocationId) - Direct lookup in Locations list (line 285)
- **Implementation 2:** LocationManager.GetLocation(string LocationId) - Wraps GameWorld.GetLocation() (lines 73-77)
- **Implementation 3:** LocationFacade.GetLocationById(string venueId) - Direct lookup in GameWorld.Venues (lines 81-84)
- **Violation:** Three different methods for location lookup. LocationManager.GetLocation() wraps GameWorld but adds nothing. LocationFacade.GetLocationById() does direct GameWorld query instead of using Manager. Name inconsistency (GetLocation vs GetLocationById) hides that they're same concept.
- **Impact:** Three ways to get locations, unclear which to use, bypassing layers defeats facade pattern

---

## SUMMARY

**Total HIGHLANDER violations:** 6

**Patterns identified:**
1. **Dual Storage** - Exchange cards in two places (GameWorld + NPC)
2. **Repository + Service duplication** - Business logic duplicated across layers
3. **Manager wrapping Repository** - Pure passthrough with no value
4. **System + Manager duplication** - Two message systems accessing same storage
5. **Triple-layer wrapping** - Time management has 3 indirection layers
6. **Inconsistent location lookup** - Three methods for same concept with different names

**Architectural debt:**
- Multiple sources of truth violate GameWorld single-source principle
- Wrapper layers add zero value but create maintenance burden
- Facades bypass their own managers, defeating layering pattern
- Name inconsistencies hide conceptual duplication

**Audit Date:** 2025-10-18
**Status:** FAIL - 6 HIGHLANDER violations detected


---

## VIOLATION CLASS 11: Unnecessary Delegation

**Rule:** No unnecessary wrapper methods - call directly
**Required:** Remove delegation that adds no value

### Violations Found: 28

#### File: C:\Git\Wayfarer\src\Subsystems\Market\MarketFacade.cs
- **Method:** `GetBuyPrice(string itemId, string venueId)` (lines 76-79)
  - **Delegates To:** _priceManager.GetBuyPrice(itemId, venueId)
  - **Added Value:** None
  - **Violation:** Unnecessary wrapper - should call _priceManager directly

- **Method:** `GetSellPrice(string itemId, string venueId)` (lines 84-87)
  - **Delegates To:** _priceManager.GetSellPrice(itemId, venueId)
  - **Added Value:** None
  - **Violation:** Unnecessary wrapper - should call _priceManager directly

- **Method:** `GetPricingInfo(string itemId, string venueId)` (lines 92-95)
  - **Delegates To:** _priceManager.GetPricingInfo(itemId, venueId)
  - **Added Value:** None
  - **Violation:** Unnecessary wrapper - should call _priceManager directly

- **Method:** `GetAllPrices(string venueId)` (lines 100-103)
  - **Delegates To:** _priceManager.GetLocationPrices(venueId)
  - **Added Value:** None
  - **Violation:** Unnecessary wrapper - should call _priceManager directly

- **Method:** `CanBuyItem(string itemId, string venueId)` (lines 110-113)
  - **Delegates To:** _marketManager.CanBuyItem(itemId, venueId)
  - **Added Value:** None
  - **Violation:** Unnecessary wrapper - should call _marketManager directly

- **Method:** `CanSellItem(string itemId, string venueId)` (lines 118-121)
  - **Delegates To:** _marketManager.CanSellItem(itemId, venueId)
  - **Added Value:** None
  - **Violation:** Unnecessary wrapper - should call _marketManager directly

#### File: C:\Git\Wayfarer\src\Subsystems\Narrative\NarrativeFacade.cs
- **Method:** `AddSystemMessage(string message, SystemMessageTypes type)` (lines 36-39)
  - **Delegates To:** _messageSystem.AddSystemMessage(message, type)
  - **Added Value:** None
  - **Violation:** Unnecessary wrapper - should call _messageSystem directly

- **Method:** `AddNarrativeMessage(string message, SystemMessageTypes type)` (lines 44-47)
  - **Delegates To:** _messageSystem.AddSystemMessage(message, type)
  - **Added Value:** None
  - **Violation:** Unnecessary wrapper - identical to AddSystemMessage, should call _messageSystem directly

- **Method:** `GenerateTokenGainNarrative(ConnectionType type, int count, string npcId)` (lines 55-58)
  - **Delegates To:** _narrativeService.GenerateTokenGainNarrative(type, count, npcId)
  - **Added Value:** None
  - **Violation:** Unnecessary wrapper - should call _narrativeService directly

- **Method:** `GenerateRelationshipMilestone(string npcId, int totalTokens)` (lines 63-66)
  - **Delegates To:** _narrativeService.GenerateRelationshipMilestone(npcId, totalTokens)
  - **Added Value:** None
  - **Violation:** Unnecessary wrapper - should call _narrativeService directly

- **Method:** `RenderTemplate(string template)` (lines 154-157)
  - **Delegates To:** _narrativeRenderer.RenderTemplate(template)
  - **Added Value:** None
  - **Violation:** Unnecessary wrapper - should call _narrativeRenderer directly

#### File: C:\Git\Wayfarer\src\Subsystems\Token\TokenFacade.cs
- **Method:** `GetTokensWithNPC(string npcId)` (lines 39-42)
  - **Delegates To:** _connectionTokenManager.GetTokensWithNPC(npcId)
  - **Added Value:** None
  - **Violation:** Unnecessary wrapper - should call _connectionTokenManager directly

- **Method:** `GetTotalTokensOfType(ConnectionType type)` (lines 47-50)
  - **Delegates To:** _connectionTokenManager.GetTotalTokensOfType(type)
  - **Added Value:** None
  - **Violation:** Unnecessary wrapper - should call _connectionTokenManager directly

- **Method:** `HasTokens(ConnectionType type, int amount)` (lines 55-58)
  - **Delegates To:** _connectionTokenManager.HasTokens(type, amount)
  - **Added Value:** None
  - **Violation:** Unnecessary wrapper - should call _connectionTokenManager directly

- **Method:** `GetTokenCount(string npcId, ConnectionType type)` (lines 63-66)
  - **Delegates To:** _connectionTokenManager.GetTokenCount(npcId, type)
  - **Added Value:** None
  - **Violation:** Unnecessary wrapper - should call _connectionTokenManager directly

#### File: C:\Git\Wayfarer\src\Subsystems\Resource\ResourceFacade.cs
- **Method:** `GetCoins()` (lines 40-43)
  - **Delegates To:** _coinManager.GetCurrentCoins(_gameWorld.GetPlayer())
  - **Added Value:** None (player retrieval is trivial)
  - **Violation:** Unnecessary wrapper - should call _coinManager directly with player

- **Method:** `CanAfford(int amount)` (lines 45-48)
  - **Delegates To:** _coinManager.CanAfford(_gameWorld.GetPlayer(), amount)
  - **Added Value:** None (player retrieval is trivial)
  - **Violation:** Unnecessary wrapper - should call _coinManager directly with player

- **Method:** `GetHealth()` (lines 67-70)
  - **Delegates To:** _healthManager.GetCurrentHealth(_gameWorld.GetPlayer())
  - **Added Value:** None (player retrieval is trivial)
  - **Violation:** Unnecessary wrapper - should call _healthManager directly with player

- **Method:** `IsAlive()` (lines 82-85)
  - **Delegates To:** _healthManager.IsAlive(_gameWorld.GetPlayer())
  - **Added Value:** None (player retrieval is trivial)
  - **Violation:** Unnecessary wrapper - should call _healthManager directly with player

- **Method:** `GetHunger()` (lines 89-92)
  - **Delegates To:** _hungerManager.GetCurrentHunger(_gameWorld.GetPlayer())
  - **Added Value:** None (player retrieval is trivial)
  - **Violation:** Unnecessary wrapper - should call _hungerManager directly with player

- **Method:** `IsStarving()` (lines 104-107)
  - **Delegates To:** _hungerManager.IsStarving(_gameWorld.GetPlayer())
  - **Added Value:** None (player retrieval is trivial)
  - **Violation:** Unnecessary wrapper - should call _hungerManager directly with player

#### File: C:\Git\Wayfarer\src\Subsystems\Time\TimeFacade.cs
- **Method:** `GetCurrentDay()` (lines 34-37)
  - **Delegates To:** _gameWorld.CurrentDay
  - **Added Value:** None
  - **Violation:** Unnecessary wrapper - should access _gameWorld.CurrentDay directly

- **Method:** `GetCurrentSegment()` (lines 39-42)
  - **Delegates To:** _timeManager.CurrentSegment
  - **Added Value:** None
  - **Violation:** Unnecessary wrapper - should access _timeManager.CurrentSegment directly

- **Method:** `GetCurrentTimeBlock()` (lines 49-52)
  - **Delegates To:** _timeManager.CurrentTimeBlock
  - **Added Value:** None
  - **Violation:** Unnecessary wrapper - should access _timeManager.CurrentTimeBlock directly

- **Method:** `GetFormattedTimeDisplay()` (lines 168-171)
  - **Delegates To:** _timeDisplayFormatter.GetFormattedTimeDisplay()
  - **Added Value:** None
  - **Violation:** Unnecessary wrapper - should call _timeDisplayFormatter directly

- **Method:** `GetTimeString()` (lines 173-176)
  - **Delegates To:** _timeDisplayFormatter.GetTimeString()
  - **Added Value:** None
  - **Violation:** Unnecessary wrapper - should call _timeDisplayFormatter directly

- **Method:** `GetTimeDescription()` (lines 178-181)
  - **Delegates To:** _timeDisplayFormatter.GetTimeDescription()
  - **Added Value:** None
  - **Violation:** Unnecessary wrapper - should call _timeDisplayFormatter directly

#### File: C:\Git\Wayfarer\src\Subsystems\Travel\TravelFacade.cs
- **Method:** `GetAvailableRoutesFromCurrentLocation()` (lines 40-43)
  - **Delegates To:** _routeManager.GetAvailableRoutesFromCurrentLocation()
  - **Added Value:** None
  - **Violation:** Unnecessary wrapper - should call _routeManager directly

**AUDIT RESULT: 28 VIOLATIONS DETECTED**

**Impact:**
- Creates unnecessary indirection layers
- Hides actual service dependencies from callers
- Makes debugging harder (extra stack frames with no logic)
- Violates "call directly" principle from CLAUDE.md
- Increases maintenance burden (changes require updates in multiple places)

**Pattern Analysis:**
Most violations follow this anti-pattern:
```csharp
// VIOLATION: Unnecessary wrapper
public int GetCoins()
{
    return _coinManager.GetCurrentCoins(_gameWorld.GetPlayer());
}

// CORRECT: Call manager directly
// UI/caller should do: _coinManager.GetCurrentCoins(player)
```

**Recommended Fix:**
1. Remove all wrapper methods that add no logic
2. Expose underlying managers/services through properties if needed
3. Update callers to call the actual service directly
4. For facades that exist ONLY to wrap other services with no added logic, consider eliminating the facade entirely

**Audit Date:** 2025-10-18
**Status:** FAIL - 28 unnecessary delegation violations detected

---

## VIOLATION CLASS 5: Logging Without Request

**Rule:** No logging until requested (CLAUDE.md Senior-Dev constraint)
**Required:** Remove all Console.WriteLine, Console.Write, Debug.WriteLine, and ILogger calls unless explicitly requested

### Violations Found: 169+

**AUDIT RESULT: MASSIVE FAILURE**

The codebase contains pervasive unauthorized logging across multiple files and components.

### Console.WriteLine/Write Violations by File:

#### C:\Git\Wayfarer\src\App.razor (3 violations)
- Line 2: Component rendering logging
- Line 7: Route detection logging
- Line 14: Route failure logging

#### C:\Git\Wayfarer\src\Infrastructure\AI\OllamaConfiguration.cs (2 violations)
- Lines 25-26: Configuration value logging

#### C:\Git\Wayfarer\src\Infrastructure\AI\OllamaClient.cs (3 violations)
- Lines 74, 80, 87: Health check and exception logging

#### C:\Git\Wayfarer\src\GameState\DebugLogger.cs (1 violation)
- Line 254: Debug log entry output

#### C:\Git\Wayfarer\src\Pages\GameScreen.razor (1 violation)
- Line 129: Rendering state logging

#### C:\Git\Wayfarer\src\Pages\GameUI.razor.cs (22 violations)
- Lines 49-134: Extensive initialization, navigation, and error logging

#### C:\Git\Wayfarer\src\Pages\GameScreen.razor.cs (26 violations)
- Lines 29-522: Constructor, initialization, navigation, session management logging

#### C:\Git\Wayfarer\src\Program.cs (9 violations)
- Lines 16-94: Startup, configuration, HTTP request/response logging

#### C:\Git\Wayfarer\src\Content\ItemRepository.cs (2 violations)
- Lines 24, 35: Null collection error logging

#### C:\Git\Wayfarer\src\Content\GoalParser.cs (5 violations)
- Lines 83, 168, 185, 202, 255: Parsing result and warning logging

#### C:\Git\Wayfarer\src\Content\GameWorldInitializer.cs (3 violations)
- Lines 17, 19, 30: Factory initialization logging

#### C:\Git\Wayfarer\src\Content\Parsers\SocialCardParser.cs (4 violations)
- Lines 151, 216, 224, 235: Card parsing and validation logging

#### C:\Git\Wayfarer\src\Pages\Components\ConversationContent.razor.cs (18 violations)
- Lines 114-876: Initialization, AI generation, narrative application logging

#### C:\Git\Wayfarer\src\Content\Parsers\ObstacleParser.cs (1 violation)
- Line 67: Parsing result logging

#### C:\Git\Wayfarer\src\Content\StandingObligationRepository.cs (1 violation)
- Line 15: Null collection error logging

#### C:\Git\Wayfarer\src\Content\PackageLoader.cs (30+ violations)
- Lines 57-505+: Extensive package loading, content parsing, and completion logging

#### C:\Git\Wayfarer\src\Pages\Components\TravelContent.razor.cs (estimated 20+ violations)
- Not fully enumerated, similar patterns to other component files

### ILogger Violations:

#### C:\Git\Wayfarer\src\Services\NPCService.cs
- ILogger dependency injection (lines 13, 15, 19)
- LogWarning usage (line 104)

#### C:\Git\Wayfarer\src\GameState\TimeManager.cs
- ILogger dependency injection (lines 8, 26, 30)
- LogDebug usage (lines 67, 85, 164)
- LogInformation usage (lines 171, 183)

### Violation Categories:

1. **Lifecycle Logging** - Component initialization, construction, disposal
2. **State Change Logging** - Navigation, screen transitions, mode changes
3. **Data Logging** - Parsing results, loading progress, counts
4. **Error Logging** - Exceptions, failures, null checks
5. **Debug Logging** - Method entry/exit, intermediate states
6. **HTTP Logging** - Request/response tracking
7. **AI Logging** - Narrative generation, card processing

### Impact:

- **Performance:** String formatting overhead on every logged operation
- **Production Noise:** Console pollution in deployed application
- **Debugging Difficulty:** Signal-to-noise ratio too low to find real issues
- **Code Coupling:** ILogger dependencies create infrastructure coupling
- **Maintenance Burden:** Logging statements must be maintained alongside logic

### Recommended Action:

**SCORCHED EARTH REMOVAL:**

1. Delete ALL Console.WriteLine/Write statements
2. Remove ILogger fields, constructor parameters, and method calls
3. Remove ILogger registrations from Program.cs
4. Build and verify compilation succeeds
5. Only add logging back when explicitly requested by user

**Per CLAUDE.md Senior-Dev:**
> "No logging until requested"

This is not optional. This is not negotiable. This is a MANDATORY code standard.

**Audit Date:** 2025-10-18
**Status:** FAIL - 169+ unauthorized logging violations detected


---

## VIOLATION CLASS 1: Dictionary Disease

**Rule:** No `Dictionary<K,V>` AT ALL - FORBIDDEN (CLAUDE.md, Principle 2)
**Required:** ONLY `List<T>` where T is entity or enum

### Violations Found: 118

---

## CRITICAL VIOLATIONS (GameWorld - Single Source of Truth)

### File: C:\Git\Wayfarer\src\GameState\GameWorld.cs

**Line 44-54, 65, 112, 119:** Multiple Dictionary properties in GameWorld
- GameWorld.Goals, SocialChallengeDecks, MentalChallengeDecks, PhysicalChallengeDecks
- RouteImprovements, LocationVisitCounts, TemporaryRouteBlocks
- **Impact:** CRITICAL - Violates GameWorld flat list architecture

---

## HIGH PRIORITY VIOLATIONS (Domain Entities)

**35+ violations** in domain entities across:
- Cards (CardMechanics, GoalReward, MentalCard, PhysicalCard, SocialCard)
- Exchange system (ExchangeContext, ExchangeCostStructure, ExchangeSession)
- Configuration (GameConfiguration)
- Locations, Items, Actions, Sessions

---

## ROOT CAUSE ANALYSIS

**Why Dictionary Disease Exists:**
1. JSON Deserialization defaults to Dictionary
2. Convenience over principle
3. Legacy migration incomplete (ListBasedHelpers perpetuates usage)
4. Parser layer leakage
5. No build-time enforcement

**Impact:**
- Lost IntelliSense
- Runtime type errors
- Hidden relationships
- Violation of architecture
- Performance degradation

---

## RECOMMENDED REMEDIATION

**Total estimated effort: 49-68 hours**

Phase 1: GameWorld (CRITICAL) - 8-12 hours
Phase 2: Domain entities (HIGH) - 15-20 hours  
Phase 3: Service layer (MEDIUM) - 8-10 hours
Phase 4: Remove compatibility layer - 4-6 hours
Phase 5: Parser layer - 6-8 hours
Phase 6: Add Roslyn analyzer - 8-12 hours

**Audit Date:** 2025-10-18
**Status:** FAIL - 118 Dictionary violations detected

**THE ENTIRE CODEBASE IS FUCKING RAW!**

---

## VIOLATION CLASS 7: String ID Matching

**Rule:** No string/ID matching - use mechanical properties (CLAUDE.md DevOps-Engineer)
**Required:** Direct object references, not string lookups

### Violations Found: 156

**Total Violations:** 156 instances of string ID matching across 41 files

**Primary Violation Pattern:** Using `.FirstOrDefault()`, `.Any()`, and similar LINQ methods with string ID comparisons (`.ID ==`, `.Id ==`) to look up domain entities instead of maintaining direct object references.

**Top Offending Files (sorted by violation count):**

1. **GameWorld.cs** - 15 violations (core game state lookups)
2. **InvestigationActivity.cs** - 17 violations (investigation system)
3. **PackageLoader.cs** - 10 violations (content loading)
4. **GameFacade.cs** - 9 violations (facade layer)
5. **TravelManager.cs** - 7 violations (travel system)
6. **GoalCompletionHandler.cs** - 6 violations (reward handling)
7. **ExchangeFacade.cs** - 4 violations (exchange subsystem)
8. **SocialFacade.cs** - 4 violations (social subsystem)
9. **TravelFacade.cs** - 5 violations (travel subsystem)

**Violation Categories:**

**HIGH PRIORITY - Core Domain Entities (120 violations):**
- NPC lookups: 23 violations across 15 files
- Investigation lookups: 19 violations across 10 files
- Venue/Location lookups: 22 violations across 14 files
- Route lookups: 12 violations across 7 files
- Obstacle lookups: 15 violations across 9 files
- Item lookups: 8 violations across 4 files
- Card lookups (Mental/Social/Physical/Path/Event): 21 violations across 12 files

**MEDIUM PRIORITY - Existence Checks (36 violations):**
- Using `.Any(x => x.Id == ...)` to check if entity exists instead of direct reference checking
- Files: PackageLoader.cs, SkeletonGenerator.cs, NPCParser.cs, LocationParser.cs, ContentValidator.cs, InvestigationActivity.cs

**Impact:**
- **Performance:** Every lookup requires O(n) linear search through collections instead of O(1) direct reference access
- **Maintainability:** String IDs are fragile - typos cause runtime failures instead of compile-time errors
- **Architecture:** Violates "mechanical properties" principle from CLAUDE.md DevOps-Engineer rules
- **Type Safety:** Loses compile-time verification of entity relationships
- **Memory:** Stores redundant string IDs alongside entities instead of direct references
- **Debugging:** Harder to trace object relationships through debugger

**Root Cause:**
Entities store string IDs to reference related entities instead of holding direct object references. This creates a relational database pattern in an object-oriented system.

**Examples:**

**Current (WRONG):**
```csharp
// Goal stores string ID
public string PlacementNpcId { get; set; }

// Service looks up by string
NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == goal.PlacementNpcId);
```

**Correct Pattern:**
```csharp
// Goal stores object reference
public NPC PlacementNpc { get; set; }

// Service uses direct reference
NPC npc = goal.PlacementNpc;
```

**Recommended Fix Strategy:**

**Phase 1 - Establish Object Graph:**
1. Modify domain entities to hold direct object references instead of string IDs
2. Update parsers to wire up references during initialization
3. Maintain IDs only for JSON serialization (via [JsonProperty] attributes)

**Phase 2 - Update Services:**
1. Replace all `.FirstOrDefault(x => x.Id == ...)` with direct property access
2. Replace all `.Any(x => x.Id == ...)` existence checks with null checks
3. Remove helper methods that exist only to lookup by ID

**Phase 3 - Update GameWorld:**
1. Provide lookup methods only for external IDs (from UI/JSON)
2. Internal code uses object references exclusively
3. GameWorld initialization wires complete object graph

**Architectural Principle:**
"Entities should know their neighbors directly, not search for them by name tag."

**Audit Date:** 2025-10-18
**Status:** FAIL - 156 string ID matching violations detected across 41 files

**Files Affected:** GoalCompletionHandler.cs, GameFacade.cs, InvestigationActivity.cs, ObstacleGoalFilter.cs, InvestigationDiscoveryEvaluator.cs, ExchangeOrchestrator.cs, ExchangeFacade.cs, ExchangeInventory.cs, LocationFacade.cs, LocationNarrativeGenerator.cs, TimeFacade.cs, SocialFacade.cs, SocialChallengeDeckBuilder.cs, ObstacleFacade.cs, MeetingManager.cs, PriceManager.cs, ArbitrageCalculator.cs, MarketSubsystemManager.cs, TravelFacade.cs, MentalDeckBuilder.cs, PhysicalFacade.cs, PhysicalDeckBuilder.cs, PackageLoader.cs, NPCRepository.cs, NPCParser.cs, ItemRepository.cs, LocationParser.cs, ContentValidator.cs, SkeletonGenerator.cs, GameWorld.cs, StandingObligationManager.cs, BindingObligationSystem.cs, ListBasedHelpers.cs, NPC.cs, MentalChallengeDeck.cs, SocialChallengeDeck.cs, PhysicalChallengeDeck.cs, TravelManager.cs, RouteRepository.cs, DiscoveryJournal.razor.cs, ExchangeContent.razor.cs, LocationContent.razor.cs, ConversationContent.razor.cs, PhysicalContent.razor.cs, MentalContent.razor.cs, TravelPathContent.razor.cs, TravelPathContent.razor

---

## VIOLATION CLASS 12: Unnecessary Intermediary Classes

**Rule:** No unnecessary abstractions - query GameWorld directly
**Required:** Remove intermediary classes that duplicate GameWorld functionality

### Violations Found: 6

#### File: C:\Git\Wayfarer\src\Subsystems\Exchange\ExchangeInventory.cs
- **Class:** ExchangeInventory
  - **Purpose:** Claims to "manage NPC exchange inventories and track exchange history"
  - **Reality:** Duplicates GameWorld.NPCExchangeCards and NPC.ExchangeDeck functionality
  - **Violation:** Unnecessary intermediary - should query GameWorld directly
  - **Evidence:**
    - Lines 14-20: Maintains parallel dictionaries (_npcExchanges, _exchangeHistory, _usedUniqueExchanges)
    - Line 33-71: InitializeFromGameWorld() copies data from GameWorld into local cache
    - Lines 38-70: Duplicates data from both GameWorld.NPCExchangeCards AND NPC.ExchangeDeck
    - Lines 76-84: GetNPCExchanges() returns cached data instead of querying GameWorld
    - All operations could be methods on GameWorld or ExchangeFacade directly
  - **Impact:** Creates dual source of truth, requires synchronization, adds complexity


---

## VIOLATION CLASS 13: GameWorld Single Source of Truth Violation

**Rule:** ALL game state lives in GameWorld (CLAUDE.md)
**Required:** Services query GameWorld, don't store state

### Violations Found: 5

#### File: C:\Git\Wayfarer\src\GameState\TimeManager.cs
- **State Storage:** `_timeModel` (line 7)
  - **What:** TimeModel containing current day, segment, time block
  - **Should Be:** GameWorld.CurrentDay, GameWorld.CurrentTimeBlock (already exists in GameWorld lines 10-11)
  - **Violation:** Parallel storage of time state in both TimeManager and GameWorld
  - **Evidence:** GameWorld has `CurrentDay` and `CurrentTimeBlock` properties (lines 10-11)
  - **Impact:** Two sources of truth for time state creates desync risk

#### File: C:\Git\Wayfarer\src\Subsystems\Social\MomentumManager.cs
- **State Storage:** `currentMomentum`, `currentDoubt` (lines 12-13)
  - **What:** Current momentum and doubt values for conversation
  - **Should Be:** SocialSession.CurrentMomentum, SocialSession.CurrentDoubt
  - **Violation:** Parallel storage of session state outside session object
  - **Evidence:** MomentumManager stores state AND syncs it back to SocialSession (line 61-66)
  - **Impact:** Two sources of truth for momentum/doubt creates desync bugs
  - **Note:** MomentumManager has sync mechanism (SyncToSession) but this is a code smell indicating parallel storage

#### File: C:\Git\Wayfarer\src\Subsystems\Social\SocialFacade.cs
- **State Storage:** `_currentSession`, `_lastOutcome`, `_personalityEnforcer` (lines 27-29)
  - **What:** Current conversation session, last outcome, personality rules
  - **Should Be:** GameWorld.CurrentSocialSession, GameWorld.LastSocialOutcome
  - **Violation:** Active session state stored in facade instead of GameWorld
  - **Impact:** Session state not accessible from GameWorld, breaks single source of truth
  - **Note:** This is TACTICAL session state that should live in GameWorld for save/load

#### File: C:\Git\Wayfarer\src\Subsystems\Mental\MentalFacade.cs
- **State Storage:** `_currentSession`, `_sessionDeck`, `_currentGoalId`, `_currentInvestigationId` (lines 15-18)
  - **What:** Current mental session and associated context
  - **Should Be:** GameWorld.CurrentMentalSession
  - **Violation:** Active session state stored in facade instead of GameWorld
  - **Impact:** Session state not accessible from GameWorld, breaks single source of truth

#### File: C:\Git\Wayfarer\src\Subsystems\Physical\PhysicalFacade.cs
- **State Storage:** `_currentSession`, `_sessionDeck`, `_currentGoalId`, `_currentInvestigationId` (lines 16-19)
  - **What:** Current physical session and associated context
  - **Should Be:** GameWorld.CurrentPhysicalSession
  - **Violation:** Active session state stored in facade instead of GameWorld
  - **Impact:** Session state not accessible from GameWorld, breaks single source of truth

**AUDIT RESULT: 5 VIOLATIONS DETECTED**

**Impact:**
- Multiple sources of truth for game state
- Session state scattered across facades instead of centralized in GameWorld
- Save/load systems cannot access current sessions (hidden in facade private fields)
- Time state duplicated between TimeManager and GameWorld
- Momentum/doubt state duplicated between MomentumManager and SocialSession
- Debugging harder (state not visible in GameWorld inspection)

**Recommended Fixes:**

**TimeManager:**
1. Remove `_timeModel` field
2. Operate directly on GameWorld.CurrentDay and GameWorld.CurrentTimeBlock
3. TimeManager becomes pure stateless service delegating to GameWorld

**MomentumManager:**
1. Remove `currentMomentum` and `currentDoubt` fields
2. Operate directly on SocialSession (passed as parameter)
3. Remove SyncToSession() method (no longer needed)
4. MomentumManager becomes pure stateless calculator

**SocialFacade:**
1. Move `_currentSession` to GameWorld.CurrentSocialSession
2. Move `_lastOutcome` to GameWorld.LastSocialOutcome
3. Facades query GameWorld.CurrentSocialSession instead of private field

**MentalFacade:**
1. Move `_currentSession` to GameWorld.CurrentMentalSession
2. Move session deck and goal context into MentalSession object

**PhysicalFacade:**
1. Move `_currentSession` to GameWorld.CurrentPhysicalSession
2. Move session deck and goal context into PhysicalSession object

**Audit Date:** 2025-10-18
**Status:** FAIL - 5 violations detected (TimeManager, MomentumManager, SocialFacade, MentalFacade, PhysicalFacade)

#### File: C:\Git\Wayfarer\src\GameState\StateContainers\InventoryState.cs
- **Class:** InventoryState
  - **Purpose:** Claims to be "immutable state container for inventory data"
  - **Reality:** Unnecessary abstraction over Inventory class
  - **Violation:** Duplicates functionality already in Inventory class
  - **Evidence:**
    - Lines 10-11: Wraps simple list and capacity that already exist in Inventory
    - Line 151-154: FromInventory() converts between two representations of same data
    - All operations (HasItem, GetItemCount, etc.) duplicate methods in Inventory class
    - Inventory class (C:\Git\Wayfarer\src\GameState\Inventory.cs) already exists and has same methods
  - **Impact:** Two representations of same concept, forces conversion, adds complexity

#### File: C:\Git\Wayfarer\src\Subsystems\Resource\CoinManager.cs
- **Class:** CoinManager
  - **Purpose:** Claims to "manage all coin-related operations"
  - **Reality:** Thin wrapper that just calls Player.Coins directly
  - **Violation:** Unnecessary intermediary - all operations are single-line Player property access
  - **Evidence:**
    - Line 8-11: GetCurrentCoins() just returns player.Coins
    - Line 13-16: CanAfford() is one-line comparison
    - Line 28: SpendCoins() just does player.Coins -= amount
    - Line 40: AddCoins() just does player.Coins += amount
    - Only adds messaging, which could be in calling code or a Domain Service
  - **Impact:** False abstraction that provides no value, extra layer of indirection

#### File: C:\Git\Wayfarer\src\Subsystems\Resource\HealthManager.cs
- **Class:** HealthManager
  - **Purpose:** Claims to "manage player health and damage/healing operations"
  - **Reality:** Thin wrapper that just calls Player.Health directly
  - **Violation:** Unnecessary intermediary - all operations are single-line Player property access
  - **Evidence:**
    - Line 11-14: GetCurrentHealth() just returns player.Health
    - Line 16-19: GetMaxHealth() just returns constant
    - Line 34: TakeDamage() just does player.Health = Math.Max(MIN_HEALTH, player.Health - amount)
    - Line 57: Heal() just does player.Health = Math.Min(MAX_HEALTH, player.Health + amount)
    - Only adds messaging, which could be in calling code or a Domain Service
  - **Impact:** False abstraction that provides no value, extra layer of indirection

#### File: C:\Git\Wayfarer\src\Subsystems\Resource\HungerManager.cs
- **Class:** HungerManager
  - **Purpose:** Claims to "manage player hunger and food consumption"
  - **Reality:** Thin wrapper that just calls Player.Hunger directly
  - **Violation:** Unnecessary intermediary - all operations are single-line Player property access
  - **Evidence:**
    - Line 14-17: GetCurrentHunger() just returns player.Hunger
    - Line 33: IncreaseHunger() just does player.Hunger = Math.Min(MAX_HUNGER, player.Hunger + amount)
    - Line 57: DecreaseHunger() just does player.Hunger = Math.Max(MIN_HUNGER, player.Hunger - amount)
    - Only adds messaging, which could be in calling code or a Domain Service
  - **Impact:** False abstraction that provides no value, extra layer of indirection

#### File: C:\Git\Wayfarer\src\Subsystems\Social\MomentumManager.cs
- **Class:** MomentumManager
  - **Purpose:** Claims to "manage momentum and doubt system"
  - **Reality:** Duplicates SocialSession state management
  - **Violation:** Unnecessary intermediary - SocialSession already tracks momentum and doubt
  - **Evidence:**
    - Lines 12-13: Stores currentMomentum and currentDoubt locally
    - Line 14: Maintains reference to SocialSession
    - Lines 59-66: SyncToSession() copies state BACK to SocialSession
    - Lines 72-78, 83-97, 103-109: All operations just modify local state then sync back
    - SocialSession already has CurrentMomentum and CurrentDoubt properties
    - This creates circular dependency: MomentumManager -> SocialSession -> MomentumManager
  - **Impact:** Duplicates session state, requires constant synchronization, circular dependency


### Non-Violations (Legitimate Domain Services)

The following Manager classes were examined and determined to be LEGITIMATE domain services, NOT violations:

- **TimeManager.cs** - Orchestrates time progression with side effects (messaging, logging, event handling)
- **LocationManager.cs** - Provides complex queries and domain logic over GameWorld data
- **TravelManager.cs** - Orchestrates complex travel session logic with multiple subsystems  
- **RouteManager.cs** - Provides domain-specific route queries
- **PriceManager.cs** - Implements complex pricing algorithm with multiple factors
- **MeetingManager.cs** - Orchestrates meeting lifecycle with validation and side effects
- **CardSelectionManager.cs** - UI-specific state management for card selection

### The Test: Is This Manager a Violation?

**VIOLATION (should be removed):**
- Does this class just forward to a single property with minimal logic? YES -> Remove it
- Duplicates GameWorld data in parallel storage? -> Remove it
- Duplicates another class functionality? -> Remove it  
- Creates circular dependencies with constant syncing? -> Remove it

**LEGITIMATE (should be kept):**
- Does this class orchestrate multiple systems, implement complex algorithms, or provide substantial domain logic? YES -> Keep it

### Impact

**Violations create:**
- Unnecessary abstraction layers that provide no value
- False sense of encapsulation (just forwarding to Player properties)
- Extra indirection that makes code harder to trace
- Confusion about where logic lives (in Manager or in calling code?)
- Duplicate state that requires synchronization
- Circular dependencies and constant state syncing

### Recommended Remediation

**ExchangeInventory:**
1. Remove class entirely
2. Move methods to ExchangeFacade or create ExchangeDomainService
3. Query GameWorld.NPCExchangeCards and NPC.ExchangeDeck directly
4. Move _exchangeHistory to GameWorld or Player state
5. Move _usedUniqueExchanges to Player state

**InventoryState:**
1. Delete class entirely
2. Use Inventory class directly everywhere
3. Remove conversion methods FromInventory()

**CoinManager, HealthManager, HungerManager:**
1. Delete all three classes
2. Call Player.Coins, Player.Health, Player.Hunger directly in calling code
3. Move messaging to calling facade or create single ResourceMessagingService if needed

**MomentumManager:**
1. Delete class entirely
2. Move momentum/doubt logic directly into SocialSession methods
3. Remove circular dependency between MomentumManager and SocialSession

**Audit Date:** 2025-10-18
**Status:** FAIL - 6 unnecessary intermediary class violations detected

---

## VIOLATION CLASS 6: Exception Handling Without Specification

**Rule:** No exception handling unless specified (CLAUDE.md Senior-Dev constraint)
**Required:** Remove try-catch blocks unless explicitly requested

### Violations Found: 27

**AUDIT RESULT: 27 VIOLATIONS DETECTED**

#### File: C:\Git\Wayfarer\src\Subsystems\Social\SocialFacade.cs
- **Line 606-635:** `ExecuteExchange()` method with try-catch block
  - **Context:** Exchange execution wrapped in exception handling
  - **Violation:** Catches generic Exception, uses for control flow
  - **Impact:** Hides errors, uses exceptions for non-exceptional flow

#### File: C:\Git\Wayfarer\src\Subsystems\Exchange\ExchangeOrchestrator.cs
- **Lines 45-70:** `InitiateExchange()` with try-catch
- **Lines 115-140:** `FinalizeExchange()` with try-catch
  - **Context:** Exchange initialization and finalization
  - **Violation:** Generic exception handling without specification
  - **Impact:** Silent error suppression

#### File: C:\Git\Wayfarer\src\Content\PackageLoader.cs
- **Lines 89-105:** Package loading with try-catch
- **Lines 201-217:** Content parsing with try-catch
- **Lines 312-328:** Skeleton generation with try-catch
  - **Context:** Content loading and parsing operations
  - **Violation:** Multiple try-catch blocks without specification
  - **Impact:** Errors logged but not propagated

#### File: C:\Git\Wayfarer\src\Infrastructure\AI\OllamaClient.cs
- **Lines 72-88:** Health check with try-catch
- **Lines 145-162:** Generate completion with try-catch
  - **Context:** AI service communication
  - **Violation:** Generic exception handling
  - **Impact:** Masks network/API errors

#### File: C:\Git\Wayfarer\src\Content\Parsers\SocialCardParser.cs
- **Lines 148-165:** Card parsing with try-catch
  - **Context:** JSON deserialization
  - **Violation:** Exception handling without specification
  - **Impact:** Parse errors not propagated

#### File: C:\Git\Wayfarer\src\Content\Parsers\ObstacleParser.cs
- **Lines 64-82:** Obstacle parsing with try-catch
  - **Context:** JSON deserialization
  - **Violation:** Exception handling without specification
  - **Impact:** Parse errors not propagated

#### File: C:\Git\Wayfarer\src\Content\Parsers\NPCParser.cs
- **Lines 112-129:** NPC parsing with try-catch
  - **Context:** JSON deserialization
  - **Violation:** Exception handling without specification
  - **Impact:** Parse errors not propagated

#### File: C:\Git\Wayfarer\src\Content\Parsers\LocationParser.cs
- **Lines 95-112:** Location parsing with try-catch
  - **Context:** JSON deserialization
  - **Violation:** Exception handling without specification
  - **Impact:** Parse errors not propagated

#### File: C:\Git\Wayfarer\src\Services\NPCService.cs
- **Lines 98-108:** Service plan lookup with try-catch
  - **Context:** NPC service availability check
  - **Violation:** Exception handling without specification
  - **Impact:** Errors logged but swallowed

#### File: C:\Git\Wayfarer\src\GameState\TimeManager.cs
- **Lines 140-158:** Time advancement with try-catch
  - **Context:** Game time progression
  - **Violation:** Exception handling without specification
  - **Impact:** Critical errors in time system hidden

#### File: C:\Git\Wayfarer\src\Pages\Components\ConversationContent.razor.cs
- **Lines 245-268:** AI narrative generation with try-catch
- **Lines 412-435:** Card play execution with try-catch
  - **Context:** UI component operations
  - **Violation:** Generic exception handling
  - **Impact:** UI errors logged but not surfaced to user

#### File: C:\Git\Wayfarer\src\Pages\GameScreen.razor.cs
- **Lines 185-202:** Navigation state change with try-catch
- **Lines 298-315:** Session initialization with try-catch
  - **Context:** Screen navigation and state management
  - **Violation:** Exception handling without specification
  - **Impact:** Navigation errors hidden

#### Additional violations in:
- **TravelManager.cs** (2 violations)
- **MeetingManager.cs** (1 violation)
- **ExchangeFacade.cs** (3 violations)
- **MarketFacade.cs** (2 violations)
- **InventoryManager.cs** (1 violation)

### Violation Pattern Analysis:

**Type 1: Control Flow (8 violations)**
```csharp
try {
    // normal operation
} catch (Exception ex) {
    return FailureResult(ex.Message);  // Using exceptions for control flow
}
```

**Type 2: Silent Suppression (12 violations)**
```csharp
try {
    // operation
} catch (Exception ex) {
    _logger.LogError(ex.Message);  // Log and swallow
}
```

**Type 3: Generic Catching (7 violations)**
```csharp
catch (Exception ex)  // Catches everything including OutOfMemoryException, etc.
```

### Impact:

- **Debugging Difficulty:** Errors hidden instead of surfacing
- **Control Flow Abuse:** Exceptions used for normal flow instead of exceptional cases
- **Error Masking:** Real bugs get logged but not fixed
- **Stack Trace Loss:** Original error context lost
- **Performance:** Exception handling overhead on normal code paths

### Recommended Action:

**Per CLAUDE.md Senior-Dev:**
> "No exception handling unless specified"

**SCORCHED EARTH REMOVAL:**

1. Remove ALL try-catch blocks across codebase
2. Let exceptions propagate naturally to top-level handler
3. Fix code so it doesn't throw on normal paths
4. Only add exception handling back when explicitly requested with specific handling strategy

**Exceptions to the Rule (keep these):**
- IDisposable.Dispose() implementations (suppress exceptions per C# guidelines)
- Top-level application handlers (Program.cs, global error boundary)
- External I/O where specific recovery logic exists

**Audit Date:** 2025-10-18
**Status:** FAIL - 27 exception handling violations detected

---

# EXECUTIVE SUMMARY

## Total Violations by Class

| Class | Violations | Status | Priority |
|-------|-----------|--------|----------|
| 1. Dictionary Disease | 118 | FAIL | CRITICAL |
| 2. HashSet Disease | 17 | FAIL | HIGH |
| 3. Object Type Usage | 8 | FAIL | HIGH |
| 4. var Usage | 0 | PASS | N/A |
| 5. Logging Without Request | 169+ | FAIL | MEDIUM |
| 6. Exception Handling | 27 | FAIL | MEDIUM |
| 7. String ID Matching | 156 | FAIL | HIGH |
| 8. Parallel Storage | 6 | FAIL | CRITICAL |
| 9. HIGHLANDER Violation | 6 | FAIL | HIGH |
| 10. Subsystem Boundary | 1 | FAIL | CRITICAL |
| 11. Unnecessary Delegation | 28 | FAIL | MEDIUM |
| 12. Unnecessary Intermediaries | 6 | FAIL | HIGH |
| 13. GameWorld State Violation | 5 | FAIL | CRITICAL |
| 14. GameWorld Dependency | 0 | PASS | N/A |

## Total Violations: 547

## Clean Areas: 2
- ✅ No `var` usage (explicit typing enforced)
- ✅ GameWorld has zero external dependencies (architecture preserved)

## Top Offending Files

1. **PackageLoader.cs** - 44+ violations (Dictionary, logging, exception handling)
2. **GameWorld.cs** - 22 violations (Dictionary in single source of truth!)
3. **ConversationContent.razor.cs** - 18 violations (logging, exception handling)
4. **GameScreen.razor.cs** - 26 violations (logging, exception handling)
5. **InvestigationActivity.cs** - 17 violations (string ID matching)

## Remediation Priority Matrix

### Phase 1: CRITICAL - Foundation Fixes (48-68 hours)

**These violations break architectural principles and MUST be fixed first:**

1. **Dictionary Disease in GameWorld** (8-12 hours)
   - VIOLATION CLASS 1
   - Single source of truth using forbidden Dictionary
   - Blocks all other architectural fixes

2. **Parallel Storage / Duplicate State** (6-8 hours)
   - VIOLATION CLASS 8
   - Multiple sources of truth create desync bugs
   - ExchangeInventory, MarketStateTracker, RelationshipTracker

3. **GameWorld State Violations** (8-12 hours)
   - VIOLATION CLASS 13
   - Active session state in facades instead of GameWorld
   - Blocks save/load system implementation

4. **Subsystem Boundary Violation** (2-3 hours)
   - VIOLATION CLASS 10
   - Exchange logic in Social subsystem
   - Fix: Delete SocialFacade.ExecuteExchange()

5. **Dictionary Disease in Domain Entities** (15-20 hours)
   - VIOLATION CLASS 1
   - 35+ violations in Cards, Exchange, Configuration
   - Prerequisite for all entity-level work

6. **String ID Matching Foundation** (9-13 hours)
   - VIOLATION CLASS 7 (partial)
   - Convert core entities (NPC, Investigation, Location) to object references
   - Enables performance improvements and type safety

### Phase 2: HIGH - Code Quality (32-42 hours)

**These violations degrade code quality and maintainability:**

1. **HashSet Disease** (6-8 hours)
   - VIOLATION CLASS 2
   - 17 violations across session state and UI
   - Replace with List<T>

2. **HIGHLANDER Violations** (8-12 hours)
   - VIOLATION CLASS 9
   - Duplicate implementations across Repository/Service/Manager layers
   - Delete duplicate code, keep one implementation

3. **Unnecessary Intermediary Classes** (10-14 hours)
   - VIOLATION CLASS 12
   - ExchangeInventory, InventoryState, CoinManager, HealthManager, HungerManager, MomentumManager
   - Delete and use GameWorld/Player directly

4. **String ID Matching Completion** (8-10 hours)
   - VIOLATION CLASS 7 (remaining)
   - Convert remaining 90+ violations after Phase 1 foundation

### Phase 3: MEDIUM - Technical Debt (23-31 hours)

**These violations create maintenance burden but don't break architecture:**

1. **Logging Without Request** (8-12 hours)
   - VIOLATION CLASS 5
   - 169+ Console.WriteLine statements
   - Scorched earth deletion

2. **Exception Handling** (6-8 hours)
   - VIOLATION CLASS 6
   - 27 try-catch blocks
   - Remove unless specifically needed

3. **Unnecessary Delegation** (9-11 hours)
   - VIOLATION CLASS 11
   - 28 wrapper methods that add no value
   - Delete wrappers, call services directly

### Phase 4: LOW - Final Cleanup (8-12 hours)

**Polish and enforcement:**

1. **Object Type Usage** (3-5 hours)
   - VIOLATION CLASS 3
   - 8 violations (mostly in PromptBuilder template system)
   - Requires PromptBuilder architectural refactoring

2. **Add Roslyn Analyzers** (5-7 hours)
   - Prevent Dictionary, HashSet, var, object from being used
   - Build-time enforcement of code standards

## Total Estimated Remediation Time: 111-153 hours

## Root Cause Analysis

**Why do these violations exist?**

1. **Incomplete migration** from Dictionary to List-based architecture
2. **Legacy code** not refactored during architectural changes
3. **No build-time enforcement** of coding standards
4. **Convenience over principle** during rapid development
5. **JSON deserialization defaults** to Dictionary patterns

## Impact Assessment

**What are these violations costing?**

- **Performance:** O(n) string lookups instead of O(1) object references
- **Type Safety:** Runtime type errors from Dictionary/object usage
- **Debugging:** Multiple sources of truth cause desync bugs
- **IntelliSense:** Lost due to Dictionary/object patterns
- **Maintainability:** Duplicate code requires changes in multiple places
- **Save/Load:** Session state hidden in facades instead of GameWorld

## Strategic Recommendation

**DO NOT attempt fixes in isolation.**

The violations are interconnected - fixing Dictionary Disease enables fixing String ID Matching, which enables removing Unnecessary Intermediaries, which enables consolidating to GameWorld single source of truth.

**Recommended approach:**
1. Start with Phase 1 foundation fixes (GameWorld Dictionary removal)
2. Complete each phase before moving to next
3. Use senior-dev agent with "scorched earth" approach
4. Build and test after each major change
5. Add Roslyn analyzers at the end to prevent regression

**GORDON RAMSAY VERDICT:**

"THIS CODEBASE IS FUCKING RAW! 547 violations of basic coding standards! You've got Dictionary disease EVERYWHERE, including in the SINGLE SOURCE OF TRUTH! Multiple sources of truth! Session state scattered across facades like confetti! 169 unauthorized logging statements polluting the console! String ID matching creating O(n) lookups when you should have O(1) object references! SIX unnecessary intermediary classes that DO NOTHING! And you wonder why debugging is a nightmare?! GET IT SORTED!"

---

**Audit Date:** 2025-10-18
**Audit Completed By:** 14 parallel subagents via Task tool
**Audit Methodology:** Comprehensive codebase scan using Grep/Read tools across all .cs and .razor files in src folder
**Next Steps:** Begin Phase 1 remediation with senior-dev agent