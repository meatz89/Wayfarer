# GAME ARCHITECTURE FINDINGS

This document captures critical architectural discoveries and patterns that must be maintained for system stability and design consistency.

## LETTER QUEUE SYSTEM ARCHITECTURE

### **Core Letter Queue Components**

**Letter Queue Manager**:
- **Single Responsibility**: Manages 8-slot priority queue with position enforcement
- **Repository Pattern**: Uses LetterRepository for all letter data access
- **Queue Operations**: Add, remove, reorder, manipulate queue positions
- **Deadline Management**: Daily countdown and expiration handling

**Letter Entity Structure**:
```csharp
public class Letter {
    public string Id { get; set; }
    public string SenderId { get; set; }
    public string RecipientId { get; set; }
    public ConnectionType TokenType { get; set; }
    public int Deadline { get; set; }
    public int Payment { get; set; }
    public int QueuePosition { get; set; }
    public LetterSize Size { get; set; }
    public bool IsFromPatron { get; set; }
}
```

**Connection Token System**:
- **Token Types**: Trust, Trade, Noble, Common, Shadow (enum)
- **Token Storage**: Player state with token counts by type
- **Token Operations**: Earn, spend, calculate gravity effects
- **Repository Access**: All token operations through ConnectionTokenRepository

**Standing Obligations System**:
- **Permanent Modifiers**: Change queue behavior forever
- **Benefit + Constraint**: Each obligation helps and restricts
- **Queue Integration**: Modify letter entry position, delivery requirements
- **Player State**: Track active obligations and their effects

### **Letter Queue Repository Patterns**

**LetterRepository**:
```csharp
public class LetterRepository {
    private readonly GameWorld _gameWorld;
    
    public List<Letter> GetPlayerQueue() => _gameWorld.WorldState.Player.LetterQueue;
    public void AddLetterToQueue(Letter letter) { /* Add to queue with position logic */ }
    public void RemoveLetterFromQueue(string letterId) { /* Remove and shift positions */ }
    public Letter GetLetterById(string id) { /* Standard repository lookup */ }
    public void UpdateLetterPosition(string letterId, int newPosition) { /* Queue manipulation */ }
}
```

**ConnectionTokenRepository**:
```csharp
public class ConnectionTokenRepository {
    private readonly GameWorld _gameWorld;
    
    // Player's total tokens by type (for queue manipulation)
    public Dictionary<ConnectionType, int> GetPlayerTokens() => _gameWorld.WorldState.Player.ConnectionTokens;
    
    // Tokens with specific NPC (for relationship screen)
    public Dictionary<string, Dictionary<ConnectionType, int>> GetTokensByNPC() => _gameWorld.WorldState.Player.NPCTokens;
    
    // Get tokens player has with specific NPC
    public Dictionary<ConnectionType, int> GetTokensWithNPC(string npcId) {
        var npcTokens = _gameWorld.WorldState.Player.NPCTokens;
        return npcTokens.ContainsKey(npcId) ? npcTokens[npcId] : new Dictionary<ConnectionType, int>();
    }
    
    // Add tokens earned from specific NPC
    public void AddTokens(ConnectionType type, int count, string npcId) {
        // Add to player's total tokens
        var playerTokens = _gameWorld.WorldState.Player.ConnectionTokens;
        playerTokens[type] = playerTokens.GetValueOrDefault(type) + count;
        
        // Track tokens by NPC for relationship screen
        var npcTokens = _gameWorld.WorldState.Player.NPCTokens;
        if (!npcTokens.ContainsKey(npcId)) npcTokens[npcId] = new Dictionary<ConnectionType, int>();
        npcTokens[npcId][type] = npcTokens[npcId].GetValueOrDefault(type) + count;
    }
    
    // Spend tokens from player's total (not NPC-specific)
    public bool SpendTokens(ConnectionType type, int count) {
        var playerTokens = _gameWorld.WorldState.Player.ConnectionTokens;
        if (playerTokens.GetValueOrDefault(type) >= count) {
            playerTokens[type] -= count;
            return true;
        }
        return false;
    }
    
    // Calculate queue entry position based on NPC tokens
    public int GetConnectionGravity(string npcId) {
        var npcTokens = GetTokensWithNPC(npcId);
        var totalTokens = npcTokens.Values.Sum();
        
        return totalTokens switch {
            >= 5 => 6,  // Strong connection - high priority
            >= 3 => 7,  // Moderate connection - slight boost
            _ => 8      // Default - bottom of queue
        };
    }
}
```

**StandingObligationRepository**:
```csharp
public class StandingObligationRepository {
    private readonly GameWorld _gameWorld;
    
    public List<StandingObligation> GetPlayerObligations() => _gameWorld.WorldState.Player.StandingObligations;
    public void AddObligation(StandingObligation obligation) { /* Permanent character change */ }
    public bool HasObligation(string obligationId) { /* Check active obligations */ }
}
```

### **Character Relationship Architecture**

**NPC Relationship Memory**:
- **Skip Tracking**: Last 3 skipped letters per NPC
- **Delivery History**: Consistent delivery improves relationship
- **Relationship Cooling**: Mathematical relationship degradation
- **Location Binding**: NPCs exist at specific locations only

**Character Relationship Repository**:
```csharp
public class CharacterRelationshipRepository {
    private readonly GameWorld _gameWorld;
    
    public List<NPC> GetAllKnownNPCs() { /* All NPCs player has interacted with */ }
    public NPC GetNPCAtLocation(string locationId) { /* Location-based lookup */ }
    public RelationshipStatus GetRelationshipStatus(string npcId) { /* Current standing */ }
    public void TrackLetterSkip(string npcId, string letterId) { /* Memory system */ }
    public void TrackLetterDelivery(string npcId, string letterId) { /* Positive interaction */ }
}
```

## CORE ARCHITECTURAL PATTERNS

### **CRITICAL: CONTENT/LOGIC SEPARATION PRINCIPLE**

**NEVER hardcode content IDs (location names, item names, NPC names, etc.) into business logic.**

**FUNDAMENTAL RULE**: Content IDs must NEVER control program flow. Content is data, not logic.

**VIOLATIONS - FORBIDDEN:**
```csharp
❌ switch (locationId) {
    case "town_square": pricing.BuyPrice = item.BuyPrice + 1; break;
    case "dusty_flagon": pricing.BuyPrice = item.BuyPrice - 1; break;
}

❌ if (itemId == "herbs") { /* special logic */ }
❌ if (npcId == "elena_trader") { /* special behavior */ }
❌ if (locationId.Contains("tavern")) { /* logic based on name */ }
```

**CORRECT PATTERNS - REQUIRED:**
```csharp
✅ switch (location.LocationType) {
    case LocationType.Town: pricing.BuyPrice = item.BuyPrice + 1; break;
    case LocationType.Tavern: pricing.BuyPrice = item.BuyPrice - 1; break;
}

✅ if (item.ItemCategory == ItemCategory.Herbs) { /* logic */ }
✅ if (npc.Profession == Professions.Trader) { /* logic */ }
✅ if (location.LocationType == LocationType.Tavern) { /* logic */ }
```

**ENFORCEMENT REQUIREMENTS:**
1. **Business logic operates on entity properties/categories, NEVER on specific IDs**
2. **Content IDs are for data reference only, not logic control**
3. **All hardcoded content IDs in business logic are critical architectural violations**
4. **Tests using production content IDs pollute business logic - use test categories/properties**

**RATIONALE:**
- Content should be configurable data, not part of code logic
- Hardcoded content IDs make code unmaintainable and brittle
- Content creators should be able to change IDs without breaking logic
- Business logic should work with any content that has the right properties

**LETTER QUEUE SPECIFIC ENFORCEMENT:**
- **NEVER** hardcode NPC IDs in queue logic: `if (senderId == "elena_trader")` ❌
- **ALWAYS** use NPC properties: `if (npc.TokenType == ConnectionType.Trust)` ✅
- **NEVER** hardcode letter IDs in business logic: `if (letterId == "patron_mission_1")` ❌
- **ALWAYS** use letter properties: `if (letter.IsFromPatron)` ✅
- **NEVER** hardcode obligation IDs: `if (obligationId == "nobles_courtesy")` ❌
- **ALWAYS** use obligation properties: `if (obligation.AffectsNobleLetters)` ✅

### CORE ARCHITECTURAL PATTERNS

#### **UI Access Patterns** 
*See CLAUDE.md for UI access patterns overview.*

#### **Stateless Repositories** 
Repositories MUST be completely stateless and only delegate to GameWorld - NO data storage or caching allowed.
- ✅ Correct: `private readonly GameWorld _gameWorld` (ONLY allowed private field)
- ✅ Correct: Every method accesses `_gameWorld.WorldState.Property` directly
- ❌ Wrong: `private List<Entity> _cachedEntities` or any state storage
- ❌ Wrong: `private WorldState` caching or direct WorldState access
- ❌ Wrong: Any private fields other than GameWorld dependency

**ENFORCEMENT**: Repository classes may ONLY have `private readonly GameWorld _gameWorld` field. All data comes from GameWorld on every method call.

#### **Repository-Mediated Access Only (CRITICAL ARCHITECTURAL PRINCIPLE)**
**ALL game state access MUST go through entity repositories - NEVER through direct GameWorld property access.**

**MANDATORY ENFORCEMENT RULES:**
1. **ONLY repositories may access GameWorld.WorldState properties**
2. **Business logic MUST use repositories, never GameWorld properties**  
3. **Tests MUST use repositories, never GameWorld properties**
4. **UI components MUST use repositories, never GameWorld properties**
5. **GameWorld properties exist ONLY for repository implementation**
6. **Test setup MUST use repositories for data creation, never direct WorldState access**

**VIOLATION EXAMPLES - FORBIDDEN:**
```csharp
❌ gameWorld.WorldState.AddCharacter(npc);           // Direct WorldState access
❌ gameWorld.WorldState.Items.Add(item);             // Direct collection access
❌ var locations = gameWorld.WorldState.locations;   // Direct property access
❌ gameWorld.WorldState.Contracts.AddRange(contracts); // Direct collection manipulation
```

**CORRECT PATTERNS - REQUIRED:**
```csharp
✅ npcRepository.AddNPC(npc);                        // Repository-mediated
✅ itemRepository.AddItem(item);                     // Repository-mediated
✅ var locations = locationRepository.GetAllLocations(); // Repository query
✅ contractRepository.AddContracts(contracts);      // Repository-mediated
```

**TEST ARCHITECTURE - SPECIAL CASE:**
Tests differ from production ONLY in GameWorld construction. The GameWorldManager → Repository → GameWorld flow remains identical:

```csharp
// ✅ CORRECT TEST PATTERN:
// 1. Construct GameWorld differently (TestGameWorldInitializer vs GameWorldInitializer)
GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);

// 2. Use IDENTICAL repository access patterns as production
NPCRepository npcRepository = new NPCRepository(gameWorld);
npcRepository.AddNPC(testNPC);  // Repository-mediated, just like production

// 3. GameWorldManager behavior is IDENTICAL to production
GameWorldManager manager = new GameWorldManager(gameWorld, ...repositories...);
```

**THIS APPLIES TO ALL CODE - NO EXCEPTIONS:**
- Production business logic
- Test setup and teardown
- UI component data access
- Manager classes
- Service classes

#### **Test Configuration Pattern (CRITICAL)**
**ALL tests MUST use ConfigureTestServices with test-specific content - NEVER production content.**

**MANDATORY TEST PATTERNS:**
```csharp
// ✅ CORRECT: Use test-specific configuration
services.ConfigureTestServices("Content");

// ❌ WRONG: Using production configuration in tests
services.ConfigureServices();

// ❌ WRONG: Using GameWorldInitializer directly without service configuration
GameWorldInitializer initializer = new GameWorldInitializer("Content");
```

**TEST CONTENT REQUIREMENTS:**
1. **Test-specific JSON files** in `/Wayfarer.Tests/Content/Templates/`
2. **Functionally meaningful names** (test_start_location, test_merchant_npc, not location1, npc1)
3. **Isolated test data** - never depend on production content
4. **Minimal viable data** - only include entities needed for specific tests

#### **Repository-Based ID Resolution Pattern**
Repositories are responsible for ID-to-object lookup, not initialization or business logic.
- **GameWorldInitializer**: Only loads raw JSON data, no relationship building
- **Repositories**: Provide `GetEntityById(string id)` methods for all lookups
- **Business Logic**: Uses repositories to resolve IDs when needed for rules/mechanics
- **NEVER** do direct LINQ lookups in business logic - always use repository methods
- Validation of missing/invalid IDs handled at repository level


#### **Testing Requirements**

**CRITICAL TESTING ISOLATION PRINCIPLE:**
- **NEVER** use production JSON content in tests
- **ALWAYS** create test-specific JSON data for each test class
- **EACH test class should have its own isolated test data** - never depend on shared production content
- **Tests must validate system logic, not production data integrity**

### **Per-NPC Token Tracking Clarifications**

**CRITICAL IMPLEMENTATION DETAILS:**

**Token Ownership Model**:
- **Player owns all tokens** - tokens are player resources, not NPC resources
- **Tokens track relationship history** - earned from specific NPCs but spent from player pool
- **NPC relationship display** - show tokens earned from each NPC for strategic planning
- **Queue manipulation** - spend from player's total token pool, not NPC-specific pools

**Data Structure Requirements**:
```csharp
public class Player {
    // Total tokens available for queue manipulation
    public Dictionary<ConnectionType, int> ConnectionTokens { get; set; } = new();
    
    // Track tokens earned from each NPC (for relationship screen)
    public Dictionary<string, Dictionary<ConnectionType, int>> NPCTokens { get; set; } = new();
    
    // Track relationship history with each NPC
    public Dictionary<string, NPCRelationship> NPCRelationships { get; set; } = new();
}

public class NPCRelationship {
    public string NPCId { get; set; }
    public RelationshipStatus Status { get; set; }  // Warm, Neutral, Cold, Frozen
    public List<string> SkippedLetters { get; set; } = new();  // Last 3 skipped letters
    public List<string> DeliveredLetters { get; set; } = new();  // Delivery history
    public DateTime LastInteraction { get; set; }
}
```

**Connection Gravity Calculation**:
- **Based on NPC-specific tokens** - tokens earned from specific NPC determine gravity
- **Not affected by spending** - spending tokens for queue manipulation doesn't reduce NPC gravity
- **Relationship depth indicator** - more tokens with NPC = stronger relationship = higher queue priority

**Character Relationship Screen Display**:
- **Per-NPC token counts** - show tokens earned from each specific NPC
- **Relationship progression** - visual indicators of relationship strength
- **Location information** - where to find each NPC for face-to-face interactions
- **Interaction availability** - only available when at NPC's location

**Queue Entry Position Logic**:
```csharp
public int GetLetterQueuePosition(Letter letter) {
    // Special case: Patron letters always jump to slots 1-3
    if (letter.IsFromPatron) {
        return Random.Range(1, 3);
    }
    
    // Calculate based on connection gravity with sender
    var gravity = connectionTokenRepository.GetConnectionGravity(letter.SenderId);
    
    // Check for standing obligations that modify entry position
    var obligations = standingObligationRepository.GetPlayerObligations();
    foreach (var obligation in obligations) {
        gravity = obligation.ModifyQueueEntry(letter.TokenType, gravity);
    }
    
    return gravity;
}
```

### **Letter Queue System Validation**

**Queue Order Enforcement Tests**:
```csharp
// ✅ CORRECT: Test queue logic, not specific letter content
[Test]
public void DeliverLetter_WithoutTokens_RequiresPosition1() {
    var letter = CreateTestLetter(position: 3);
    var result = letterQueueManager.DeliverLetter(letter.Id, spendTokens: false);
    Assert.That(result.Success, Is.False);
    Assert.That(result.Message, Contains.Substring("Must deliver from position 1"));
}

// ❌ WRONG: Testing with production content
[Test]
public void DeliverLetter_ElenaLetter_RequiresPosition1() {
    var result = letterQueueManager.DeliverLetter("elena_love_letter", spendTokens: false);
    // This test depends on production content, violates isolation
}
```

**Connection Token System Tests**:
```csharp
// ✅ CORRECT: Test token mechanics with categories
[Test]
public void SpendTokens_PurgeAction_Requires3AnyTokens() {
    playerRepository.SetTokens(ConnectionType.Trust, 2);
    playerRepository.SetTokens(ConnectionType.Trade, 1);
    var result = letterQueueManager.PurgeLetter("test_letter");
    Assert.That(result.Success, Is.True);
    Assert.That(playerRepository.GetTotalTokens(), Is.EqualTo(0));
}
```

**Standing Obligation System Tests**:
```csharp
// ✅ CORRECT: Test obligation mechanics with properties
[Test]
public void AddObligation_NoblesCourtesy_ModifiesNobleLetterBehavior() {
    var obligation = CreateTestObligation(type: ObligationType.NobleCourtesy);
    standingObligationRepository.AddObligation(obligation);
    
    var nobleLetter = CreateTestLetter(tokenType: ConnectionType.Noble);
    letterQueueManager.AddLetterToQueue(nobleLetter);
    
    Assert.That(nobleLetter.QueuePosition, Is.EqualTo(5)); // Noble letters enter at 5
}
```

**Character Relationship Tests**:
```csharp
// ✅ CORRECT: Test relationship mechanics with NPC properties
[Test]
public void SkipLetter_ThreeTimes_NPCStopsSendingLetters() {
    var npc = CreateTestNPC(tokenType: ConnectionType.Trust);
    var letters = CreateTestLetters(count: 3, senderId: npc.Id);
    
    foreach (var letter in letters) {
        letterQueueManager.SkipLetter(letter.Id);
    }
    
    var relationshipStatus = characterRelationshipRepository.GetRelationshipStatus(npc.Id);
    Assert.That(relationshipStatus, Is.EqualTo(RelationshipStatus.Frozen));
}
```

### **Letter Queue UI Architecture**

**REQUIRED UI SCREENS:**

**1. Letter Queue Screen** (Primary gameplay screen):
- **8-Slot Visual Queue**: Positions 1-8 with letter details
- **Deadline Countdown**: Days remaining with visual urgency indicators
- **Queue Manipulation Actions**: Purge, Priority, Extend, Skip with token costs
- **Standing Obligations Panel**: Current active obligations affecting queue
- **Daily Queue Actions**: Morning swap, letter acceptance/rejection
- **Token Balance Display**: Current connection tokens by type

**2. Character Relationship Screen** (NPC management):
- **NPC Overview**: All known NPCs with relationship standings
- **Location Information**: Where each NPC can be found
- **Per-NPC Token Display**: Connection tokens player has with each specific NPC
- **Relationship History**: Letters delivered/skipped per NPC
- **Interaction Options**: Available only when at NPC's location

**3. Standing Obligations Screen** (Character development):
- **Active Obligations**: All current permanent modifiers
- **Obligation Effects**: How each obligation changes queue behavior
- **Acquisition History**: When and how each obligation was gained
- **Conflict Warnings**: Obligations that conflict with each other
- **Breaking Consequences**: What happens if obligations are violated

**Connection Token Architecture**:
- **Player-Owned**: Tokens belong to player, not NPCs
- **NPC-Specific**: Each token relates to specific NPC relationship
- **Type-Based**: Trust/Trade/Noble/Common/Shadow categories
- **Spendable**: Tokens consumed for queue manipulation
- **Gravity Effect**: Token count affects where NPC's letters enter queue

**Repository-Based UI Data Access**:
```csharp
// ✅ CORRECT: Letter Queue Screen
public class LetterQueueComponent {
    private readonly LetterRepository _letterRepository;
    private readonly ConnectionTokenRepository _tokenRepository;
    private readonly StandingObligationRepository _obligationRepository;
    
    public void LoadQueueData() {
        var letters = _letterRepository.GetPlayerQueue();
        var tokens = _tokenRepository.GetPlayerTokens();
        var obligations = _obligationRepository.GetPlayerObligations();
        // Render 8-slot queue with obligations panel
    }
}

// ✅ CORRECT: Character Relationship Screen
public class CharacterRelationshipComponent {
    private readonly CharacterRelationshipRepository _relationshipRepository;
    private readonly ConnectionTokenRepository _tokenRepository;
    private readonly LocationRepository _locationRepository;
    
    public void LoadRelationshipData() {
        var npcs = _relationshipRepository.GetAllKnownNPCs();
        var npcTokens = _tokenRepository.GetTokensByNPC(); // Per-NPC token display
        var locations = _locationRepository.GetAllLocations();
        // Render NPC list with individual token counts
    }
}

// ✅ CORRECT: Standing Obligations Screen
public class StandingObligationsComponent {
    private readonly StandingObligationRepository _obligationRepository;
    private readonly LetterRepository _letterRepository;
    
    public void LoadObligationData() {
        var obligations = _obligationRepository.GetPlayerObligations();
        var affectedLetters = _letterRepository.GetLettersAffectedByObligations();
        // Render obligations with queue effects
    }
}
```

**UI Screen Integration Requirements**:
- **Letter Queue Screen**: Primary screen with queue + obligations panel
- **Character Relationship Screen**: Navigate from main menu, shows per-NPC tokens
- **Standing Obligations Screen**: Can be separate or integrated into queue screen
- **Cross-Screen Navigation**: Easy switching between queue and relationship management
- **Real-Time Updates**: All screens update when game state changes

## Architectural Transformation Documentation

**For comprehensive architectural transformation guidance**:
- **`LETTER-QUEUE-TRANSFORMATION-ANALYSIS.md`** - Complete analysis including:
  - Architecture ramifications (10x state complexity increase)
  - Repository pattern evolution for complex queries
  - System dependency transformations
  - Step-by-step implementation with code examples
  
- **`LETTER-QUEUE-UI-SPECIFICATION.md`** - Detailed UI architecture:
  - Complete screen layouts and components
  - Repository-based UI data access patterns
  - Cross-screen navigation requirements
  
- **`LETTER-QUEUE-INTEGRATION-PLAN.md`** - System transformation approach:
  - How each existing system transforms
  - Integration phases and dependencies
  - Migration strategies

**MANDATORY TEST DATA ISOLATION:**
1. **Create TestGameWorldInitializer** that uses test-specific content directories
2. **Each test file creates its own minimal JSON data** for the specific scenarios being tested
3. **Test data should be minimal and focused** - only include entities needed for the specific test
4. **Production content changes should NEVER break tests**
5. **Tests validate that contract logic works, not that specific game contracts work**

**Examples of Proper Test Isolation:**
```csharp
// ❌ WRONG: Using production contracts.json
Contract herbContract = contracts.GetContract("village_herb_delivery"); // Depends on production data

// ✅ CORRECT: Test-specific contract data
var testContracts = new[] {
    new Contract {
        Id = "test_travel_contract",
        RequiredDestinations = ["test_destination"],
        RequiredTransactions = []
    }
};
TestGameWorldInitializer.CreateWithContracts(testContracts);
```

**Test Data Organization:**
- `Tests/TestData/` directory for all test-specific JSON files
- Each test class has its own data subdirectory
- Minimal, focused data sets that test specific system behaviors
- No coupling to production content that could change

- **NEVER** manually create game objects in tests
- **ALWAYS** use `TestGameWorldInitializer` for test setup
- Tests must verify JSON content loads correctly into GameWorld
- Integration tests must validate complete initialization pipeline

#### **Critical Pattern: ID-based Serialization vs Object References**

**FUNDAMENTAL ARCHITECTURE**: The game uses a dual-reference system for entity relationships:

**JSON Serialization Layer (String IDs)**
```json
{
  "routes": [
    {
      "id": "road_to_market",
      "origin": "dusty_flagon",           // String ID reference
      "destination": "town_square",       // String ID reference
      "requiredItems": ["climbing_gear"]  // String ID references
    }
  ],
  "contracts": [
    {
      "id": "deliver_tools",
      "destinationLocation": "town_square", // String ID reference
      "requiredItems": ["tools"]            // String ID references
    }
  ]
}
```

**Runtime Code Layer (Object References)**
```csharp
public class RouteOption
{
    public string Origin { get; set; }        // String ID for serialization
    public string Destination { get; set; }   // String ID for serialization
    
    // Runtime: Code resolves IDs to actual objects when needed
    public Location GetOriginLocation(GameWorld gameWorld) =>
        gameWorld.WorldState.locations.First(loc => loc.Id == Origin);
    
    public Location GetDestinationLocation(GameWorld gameWorld) =>
        gameWorld.WorldState.locations.First(loc => loc.Id == Destination);
}
```

### **NO FUNC/ACTION/PREDICATE DELEGATES - CONCRETE TYPES ONLY**

**FUNDAMENTAL PRINCIPLE**: Main application code must never use `Func<>`, `Action<>`, `Predicate<>` or similar delegate types. Use concrete interfaces and classes for maintainability, testability, and clarity.

**Architectural Rationale**:
- **Maintainability**: Named interfaces are self-documenting and searchable
- **Testability**: Concrete interfaces can be easily mocked and stubbed
- **Clarity**: Intention is explicit rather than hidden in lambda expressions
- **Refactoring**: IDE tools work better with concrete types than delegates
- **Debugging**: Stack traces show concrete type names instead of generated delegate code

**Implementation Pattern**:
```csharp
// ❌ FORBIDDEN: Using delegates in main code
public List<RouteOption> FilterRoutes(Func<RouteOption, bool> predicate)
{
    return routes.Where(predicate).ToList();
}

// ✅ CORRECT: Use concrete interface
public interface IRouteValidator
{
    bool IsValid(RouteOption route);
}

public List<RouteOption> FilterRoutes(IRouteValidator validator)
{
    return routes.Where(route => validator.IsValid(route)).ToList();
}
```

**Allowed Exceptions**:
- **Test Files**: Builder patterns and test setup may use delegates for convenience
- **LINQ Methods**: Built-in LINQ operations like `.Where()`, `.Select()` are acceptable
- **Event Handlers**: UI event handlers may use delegates when required by framework

**Enforcement**:
- Code reviews must catch delegate usage in main application code
- Refactor existing delegate usage to concrete interfaces during maintenance
- Create specific, named interfaces for each functional requirement

### **SYNCHRONOUS EXECUTION MODEL - NO CONCURRENCY**

**FUNDAMENTAL ARCHITECTURE**: The game uses a purely synchronous execution model with no background operations, timers, or event-driven patterns.

**Critical Discovery**: Tests were failing due to assumptions about timing and concurrency that don't exist in our architecture. The game executes linearly - when a method is called, it completes fully before returning.

**Architectural Principles**:
1. **No Background Tasks** - Everything executes in the calling thread
2. **No Timers or Scheduling** - Game time advances only through explicit method calls
3. **No Event Bus** - Direct method invocation only, no decoupled messaging
4. **No Concurrent State Access** - Single-threaded model eliminates race conditions
5. **Async/Await for I/O Only** - And always immediately awaited

**Testing Implications**:
```csharp
// ✅ CORRECT: Tests can assume immediate, complete execution
var result = manager.ExecuteAction();
Assert.Equal(expected, result); // No timing issues possible

// ❌ WRONG: Never needed in our architecture
await Task.Delay(100); // NO - nothing runs in background
await WaitForEventCompletion(); // NO - no events to wait for
```

**Debugging Benefits**:
- **Linear execution flow** - Stack traces show complete call chain
- **Predictable state changes** - No concurrent modifications
- **Reproducible behavior** - Same inputs always produce same outputs
- **Simple test setup** - No need for synchronization or mocking of time

**Exceptions to Synchronous Model**:
1. **Blazor UI Polling**:
   - Blazor Server components poll GameWorld state because Blazor doesn't support push notifications
   - This is purely a UI rendering concern - domain logic remains synchronous
   
2. **AI Service Integration** (Future):
   - Long-running AI prompts will be awaited asynchronously to avoid blocking the UI
   - This is an infrastructure boundary concern, not core game logic
   - Game mechanics continue to execute synchronously before and after AI calls
   - **Current Status**: Not implemented - focusing on non-AI game mechanics first

### **TIME SYSTEM ARCHITECTURE - SINGLE TIME SOURCE**

**FUNDAMENTAL DESIGN**: The game uses a single, linear time progression system where all time tracking derives from one authoritative time value.

**Critical Architecture Principles**:

1. **Single Time Authority**: `TimeManager.CurrentTimeHours` is the ONLY authoritative time value
   - All other time representations derive from this value
   - `TimeBlocks` enum (Morning/Afternoon/Evening/Night) calculated from hours, not stored separately
   - No separate tracking of "time blocks consumed" - this is calculated from time progression

2. **Time Blocks Are Internal Mechanics Only**:
   - "Time blocks" represent action point consumption, not UI display concepts  
   - Players see actual time progression: "Morning 6:00" → "Afternoon 14:00"
   - UI should NEVER show "time blocks remaining (2/5)" - this violates player mental model

3. **Time Progression Pattern**:
   ```csharp
   // ✅ CORRECT: Actions advance actual time
   timeManager.ConsumeTimeBlock(1); // Advances CurrentTimeHours by calculated amount
   // Result: "Morning 6:00" becomes "Morning 9:00" or "Afternoon 12:00"
   
   // ❌ WRONG: Separate time block tracking
   usedTimeBlocks++; // This disconnects from actual time progression
   ```

4. **Five Time Blocks System**:
   The game divides each day into exactly 5 time blocks that correspond to natural time periods:
   
   ```csharp
   // ✅ CORRECT: 5 Time Blocks mapped to 24-hour day
   public TimeBlocks GetCurrentTimeBlock() {
       return CurrentTimeHours switch {
           >= 6 and < 9 => TimeBlocks.Dawn,      // 6:00-8:59 (3 hours)
           >= 9 and < 12 => TimeBlocks.Morning,   // 9:00-11:59 (3 hours) 
           >= 12 and < 16 => TimeBlocks.Afternoon, // 12:00-15:59 (4 hours)
           >= 16 and < 20 => TimeBlocks.Evening,   // 16:00-19:59 (4 hours)
           >= 20 or < 6 => TimeBlocks.Night,      // 20:00-5:59 (10 hours)
           _ => TimeBlocks.Night
       };
   }
   ```
   
   **Critical Design Notes**:
   - Exactly 5 time blocks per day (MaxDailyTimeBlocks = 5)
   - Each action typically consumes 1 time block = ~3.6 hours of game time
   - Night is longest period (10 hours) for rest and recovery
   - Dawn/Morning are shorter active periods (3 hours each)
   - Afternoon/Evening are medium active periods (4 hours each)

**Time System Violations to Prevent**:
- ❌ Displaying "time blocks remaining" in UI
- ❌ Separate tracking of time blocks vs actual time
- ❌ TimeBlocks enum stored as separate state
- ❌ Actions that consume time blocks without advancing clock

### **TRAVEL SYSTEM ARCHITECTURE - ROUTES ARE TRANSPORT METHODS**

**FUNDAMENTAL DESIGN**: Each route defines exactly one transport method. Routes ARE the transport selection, not a separate layer.

**Critical Architecture Principles**:

1. **Routes Define Transport Methods**:
   - "Walking Path" = walking transport method
   - "Standard Cart" = cart transport method  
   - "Express Coach" = premium carriage transport method
   - Each route has exactly one `method` field in routes.json

2. **No Separate Transport Selection**:
   - Player chooses route: "Walking Path" or "Standard Cart" or "Express Coach"
   - This IS the transport selection - no additional layer needed
   - UI shows route names with their inherent transport characteristics

3. **Route Selection Pattern**:
   ```csharp
   // ✅ CORRECT: Routes contain all transport information
   var routes = GetAvailableRoutes(fromLocation, toLocation);
   // Routes: [{"name": "Walking Path", "method": "Walking"}, {"name": "Standard Cart", "method": "Carriage"}]
   
   // ❌ WRONG: Separate transport selection on top of routes
   var transports = GetAvailableTransportOptions(route); // This is redundant
   ```

4. **Travel UI Pattern**:
   ```csharp
   // ✅ CORRECT: Direct route selection
   "Choose route to Town Square:"
   - "Walking Path (0 coins, 2 stamina)" 
   - "Standard Cart (4 coins, 1 stamina)"
   - "Express Coach (8 coins, 0 stamina)"
   
   // ❌ WRONG: Double selection
   "Choose route: Walking Path" → "Choose transport: Walking/Horseback/Cart"
   ```

**Travel System Violations to Prevent**:
- ❌ `TravelMethods` enum separate from route definitions
- ❌ Transport selection UI on top of route selection
- ❌ Multiple transport options per route
- ❌ "Transport compatibility" logic separate from route access logic

### **Repository Pattern Single Source of Truth**

**CRITICAL PRINCIPLE**: All game state access MUST go through entity repositories, never through direct GameWorld property access.

```csharp
// ✅ CORRECT: Repository Pattern
public class ContractSystem 
{
    private readonly ContractRepository _contractRepository;
    
    public List<Contract> GetAvailableContracts() 
    {
        return _contractRepository.GetAllContracts()
            .Where(c => c.IsAvailable())
            .ToList();
    }
}

// ✅ CORRECT: Stateless repository - ONLY GameWorld dependency
public class ContractRepository 
{
    private readonly GameWorld _gameWorld; // ONLY allowed private field
    
    public List<Contract> GetAllContracts() 
    {
        return _gameWorld.WorldState.Contracts ?? new List<Contract>();
    }
}

// ❌ WRONG: Direct GameWorld property access
return _gameWorld.Contracts.Where(c => c.IsAvailable()).ToList();
```

**ENFORCEMENT RULES**:
1. **ONLY repositories may access GameWorld.WorldState properties**
2. **Business logic MUST use repositories, never GameWorld properties**  
3. **Tests MUST use repositories, never GameWorld properties**
4. **Repositories are completely stateless - NO caching or state storage**

### **UI → GameWorldManager Gateway Pattern**
All UI components must route actions through GameWorldManager instead of injecting managers directly.
- ✅ Correct: UI → GameWorldManager → Specific Manager
- ❌ Wrong: UI → Direct Manager Injection

### **GameWorld Single Source of Truth**
GameWorld.WorldState is the authoritative source for all game state.
- All game state changes must go through WorldState
- GameWorld contains no business logic, only state management

## DUAL INITIALIZER PATTERN FOR TESTING

**CRITICAL FINDING**: Tests require different initialization strategy than production to eliminate async complexity.

**Architecture Pattern**:
```
Production: JSON Files → GameWorldSerializer → GameWorldInitializer (async) → GameWorld → Repositories
Testing:   TestScenarioBuilder → TestGameWorldInitializer (sync) → GameWorld → Repositories
```

**Key Implementation**:
- **TestGameWorldInitializer**: Synchronous, direct object creation for deterministic test state
- **TestScenarioBuilder**: Declarative fluent API for readable test scenario definition
- **Same GameWorld Type**: Both patterns produce identical GameWorld objects for consistent behavior

**Benefits**:
- Zero async complexity in tests
- Deterministic state (same input = same output)
- Production-identical game flow execution
- No mocks required through real object initialization

### **Separate JSON Files for Tests**

**CRITICAL ARCHITECTURAL DECISION**: Tests need isolated JSON data files, not shared production files.

```
Production: src/Content/Templates/*.json
Testing:    Wayfarer.Tests/Content/Templates/*.json (copied/customized)
```

**MANDATORY PROCESS**: When changing JSON data structures in production content, test JSON files MUST be updated to match.

**Files That Must Stay in Sync**:
- `contracts.json` - Contract structure, properties, requirements
- `items.json` - Item categories, properties, pricing
- `locations.json` - Location structure and properties  
- `routes.json` - Route definitions and terrain categories
- `actions.json` - Action definitions and requirements

## CRITICAL SYSTEM DEPENDENCIES

### **Time Window System Architecture**

**CRITICAL FINDING**: Location spot availability depends on proper time window initialization.

**Root Cause**: `WorldState.CurrentTimeWindow` defaults to `TimeBlocks.Dawn` (enum value 0), but many location spots don't include "Dawn" in their time windows.

**Solution**: Always initialize `CurrentTimeWindow = TimeBlocks.Morning` in WorldState.

```csharp
// CORRECT: WorldState.cs
public TimeBlocks CurrentTimeWindow { get; set; } = TimeBlocks.Morning;

// WRONG: Allowing default enum value (Dawn)
public TimeBlocks CurrentTimeWindow { get; set; }
```

**Impact**: Without this fix, `gameWorldManager.CanMoveToSpot()` returns false for all spots, breaking UI navigation and player movement.

### **JSON Content Parsing Validation**

**CRITICAL FINDING**: Enum parsing in JSON deserializers silently fails when enum values don't match, resulting in empty collections.

**Solution**: Ensure JSON content uses exact enum value names.

```csharp
// CORRECT: TerrainCategory enum values
Requires_Climbing, Wilderness_Terrain, Exposed_Weather, Dark_Passage

// CORRECT: routes.json
"terrainCategories": ["Exposed_Weather", "Wilderness_Terrain"]

// WRONG: Invalid enum names
"terrainCategories": ["Urban_Terrain", "Mountain_Path"]
```

## ACTION SYSTEM ARCHITECTURE

### **Action Creation and Execution Flow**

The action system uses a multi-stage transformation pipeline that converts static JSON definitions into executable runtime actions when players visit location spots.

**Key Transformation**: `ActionDefinition` → `LocationAction`

**Flow**:
```
1. Player visits location spot via MoveToLocationSpot()
2. actionRepository.GetActionsForSpot() loads templates
3. ActionFactory.CreateActionFromTemplate() creates runtime actions
4. ActionProcessor.CanExecute() validates requirements
5. ActionStateTracker.SetLocationSpotActions() makes available to UI
```

### **IRequirement/IMechanicalEffect Extension Pattern**

**CRITICAL ARCHITECTURAL DISCOVERY**: The game already has perfect extensible interfaces for categorical systems.

```csharp
public interface IRequirement
{
    bool IsMet(GameWorld gameWorld);
    string GetDescription();
}

public interface IMechanicalEffect  
{
    void Apply(EncounterState state);
    string GetDescriptionForPlayer();
}
```

**CORRECT APPROACH**: Implement categorical logic as `IRequirement` and `IMechanicalEffect` implementations, not parallel systems.

**Integration Points**:
- `ActionProcessor.CanExecute()` validates all `IRequirement` implementations
- `MessageSystem.AddOutcome()` processes `IMechanicalEffect` implementations  
- `ActionPreview.razor` displays requirement descriptions to players
- `ActionFactory.CreateRequirements()` builds requirement lists from templates

## CONTRACT-ACTION INTEGRATION

### **CRITICAL DESIGN PHILOSOPHY: Basic Actions Complete Contracts**

**FUNDAMENTAL PRINCIPLE**: Contracts are completed through the same basic actions players use for normal gameplay, NOT through special contract-specific actions.

**CORE DESIGN RULE**: Contracts create **context and objectives** for basic actions, they do NOT create new action types.

### **Contracts Only Check Completion Actions, Not Process**

**FUNDAMENTAL RULE**: Contracts should ONLY check for the specific action that completes them, NOT how the player got to that point.

**Example: "Deliver Trade Goods to Millbrook" Contract**
- ✅ **ONLY CHECKS**: Sell/deliver [Silk Bolts] at [Millbrook]
- ❌ **DOES NOT CHECK**: How player acquired Silk Bolts (buy, find, craft, steal)
- ❌ **DOES NOT CHECK**: How player reached Millbrook (travel, already there)

**Why This Matters**:
1. **Player Agency**: Players can complete contracts using ANY strategy they devise
2. **Emergent Gameplay**: Creative solutions are rewarded, not blocked
3. **No Railroad**: Players aren't forced into specific sequences
4. **True Sandbox**: Every contract has multiple valid completion paths

## CATEGORICAL SYSTEMS ARCHITECTURE

### **Equipment Categories Enhancement**

**Item Categorical Dimensions**:
- `EquipmentCategory`: Climbing_Equipment, Navigation_Tools, Weather_Protection, Social_Signaling, etc.
- `Size`: Tiny → Small → Medium → Large → Massive (affects transport and inventory)

### **Stamina Categorical System**

**PhysicalDemand Integration**: Hard categorical gates instead of sliding scale penalties.

```csharp
// CORRECT: Hard categorical thresholds
public bool CanPerformStaminaAction(PhysicalDemand demand) =>
    demand switch {
        PhysicalDemand.None => true,
        PhysicalDemand.Light => Stamina >= 2,
        PhysicalDemand.Moderate => Stamina >= 4,
        PhysicalDemand.Heavy => Stamina >= 6,
        PhysicalDemand.Extreme => Stamina >= 8,
        _ => false
    };

// WRONG: Sliding scale penalties
// efficiency = Stamina / MaxStamina; // FORBIDDEN PATTERN
```

### **Transport Compatibility System - COMPLETE ✅**

**IMPLEMENTED**: Categorical transport restrictions based on logical physical constraints.

**Transport Restriction Categories**:
- **Cart Transport**: Blocked on TerrainCategory.Requires_Climbing, TerrainCategory.Wilderness_Terrain
- **Boat Transport**: Only works on TerrainCategory.Requires_Water_Transport  
- **Heavy Equipment**: SizeCategory.Large/Massive blocks TravelMethods.Boat/Horseback
- **Water Routes**: All non-boat transport blocked on water terrain

**Architecture Pattern**:
```csharp
// CORRECT: Transport compatibility checking
public TransportCompatibilityResult CheckFullCompatibility(TravelMethods transport, RouteOption route, Player player)
{
    // Check terrain compatibility first
    TransportCompatibilityResult terrainResult = CheckTerrainCompatibility(transport, route.TerrainCategories);
    if (!terrainResult.IsCompatible) return terrainResult;
    
    // Check equipment compatibility  
    return CheckEquipmentCompatibility(transport, player);
}
```

**UI Integration**: TravelSelection.razor shows transport options with compatibility feedback.

### **Categorical Inventory Constraints System - COMPLETE ✅**

**Size-Aware Inventory Architecture**:
```csharp
// Size categories determining slot requirements
public enum SizeCategory { Tiny, Small, Medium, Large, Massive }

// Item slot calculation based on size
public int GetRequiredSlots() => Size switch {
    SizeCategory.Tiny => 1,    SizeCategory.Small => 1,   SizeCategory.Medium => 1,
    SizeCategory.Large => 2,   SizeCategory.Massive => 3, _ => 1
};

// Transport bonuses to inventory capacity
public int GetMaxSlots(TravelMethods? transport) => transport switch {
    TravelMethods.Cart => 7,      // Base 5 + 2 slots
    TravelMethods.Carriage => 6,  // Base 5 + 1 slot
    _ => 5                        // Base capacity
};
```

**Integration Architecture**:
- **TravelManager**: Provides transport-aware inventory status checking
- **MarketManager**: Uses size-aware inventory methods for purchase validation
- **UI Components**: Display slot usage, transport bonuses, and item constraints
- **TransportCompatibilityValidator**: Integrates inventory overflow checking

**Strategic Gameplay Impact**:
- Cart transport adds slots but blocks terrain access
- Large/Massive items require transport planning
- Equipment vs carrying capacity optimization
- Visual feedback for slot usage and transport bonuses

### **Contract Categorical System**

**Contract Enhancement Structure**:
- `RequiredEquipmentCategories`: List<EquipmentCategory>
- `RequiredToolCategories`: List<ToolCategory>
- `RequiredSocialStanding`: SocialRequirement
- `PhysicalRequirement`: PhysicalDemand
- `RequiredInformation`: List<InformationRequirementData>
- `Category`: ContractCategory (affects NPC relationships)
- `Priority`: ContractPriority (affects payment and reputation)
- `RiskLevel`: ContractRisk (affects failure consequences)

## VALIDATION CHECKLIST

Before implementing any system changes:

1. ✅ **Time Window Compatibility**: Ensure CurrentTimeWindow is initialized to a value that exists in location spot time windows
2. ✅ **Repository Pattern Compliance**: All state access goes through GameWorld.WorldState
3. ✅ **Enum Value Validation**: JSON content uses exact enum value names from C# enums
4. ✅ **Single Source of Truth**: No static property usage for game state
5. ✅ **Test Pattern Compliance**: Tests follow the same architectural patterns as production code

## FAILURE PATTERNS TO AVOID

1. **❌ Time Window Defaults**: Never rely on enum default values for time-sensitive systems
2. **❌ Static State Management**: Never use static properties for game state that should be instance-based
3. **❌ Silent Enum Failures**: Always validate that JSON enum values match C# enum definitions
4. **❌ Dual State Systems**: Never maintain the same data in both static and instance properties
5. **❌ Test Architecture Violations**: Never allow tests to use different patterns than production code
6. **❌ Direct GameWorld Access**: Never access GameWorld properties directly - always use repositories
7. **❌ Parallel Validation Systems**: Never build new validation systems when IRequirement/IMechanicalEffect interfaces exist

These patterns ensure system stability and prevent the cascade failures discovered during debugging sessions.

## TEST ARCHITECTURE PATTERNS

### **Test Isolation Principle**

**MANDATORY REQUIREMENT**: Tests must NEVER use production JSON content. Each test class should have its own isolated test data.

**Architecture Pattern**:
```
Production: src/Content/Templates/*.json
Testing:    Wayfarer.Tests/Content/Templates/*.json
```

**Test Configuration Pattern**:
```csharp
// ✅ CORRECT: Use test-specific configuration
services.ConfigureTestServices("Content");

// ❌ WRONG: Using production configuration in tests
services.ConfigureServices();
```

**Test JSON Naming Convention**:
- Use functionally meaningful names that describe test purpose
- Examples: `test_start_location`, `test_travel_destination`, `test_restricted_location`
- Avoid numbered names like `test_location_1` that don't convey purpose

**Implementation**:
```csharp
// ✅ CORRECT: Test-specific data loading
GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);

// ❌ WRONG: Using production content
var gameWorld = new GameWorldInitializer("Content").LoadGame();
```

**MSBuild Configuration**:
```xml
<ItemGroup>
  <Content Include="Content\Templates\*.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

**JSON Field Name Requirements**:
- Location JSON: Use `"locationSpots"` not `"locationSpotIds"`
- Location JSON: Use `"connectedTo"` not `"connectedLocationIds"`  
- LocationSpot JSON: Use `"CurrentTimeBlocks"` with capital 'C'
- LocationSpot JSON: `"type"` field must use valid `LocationSpotTypes` enum values (FEATURE, CHARACTER)

**Stub Service Pattern for Tests**:
When tests need services not relevant to the test scenario, provide null stubs:
```csharp
// In TestServiceConfiguration
services.AddSingleton<EncounterFactory>(sp => null); // OK for non-AI tests
services.AddSingleton<ChoiceProjectionService>(sp => null); // OK for non-AI tests
```

**Benefits**:
- Tests validate system logic, not production data integrity
- Fast, reliable test execution
- Systematic debugging capability
- No brittleness from production content changes

### **Repository Pattern Compliance in Tests**

**CRITICAL PRINCIPLE**: Tests must follow the same access patterns as business logic.

```csharp
// ✅ CORRECT: Repository-mediated access
LocationRepository locationRepo = new LocationRepository(gameWorld);
Location workshop = locationRepo.GetLocation("workshop");

// ❌ WRONG: Direct WorldState access
Location workshop = gameWorld.WorldState.locations.First(l => l.Id == "workshop");
```

**Enforcement**: Tests should use repositories for all data access, never direct GameWorld.WorldState access.

### **Test Data File Path Resolution**

**CORRECT PATTERN**: Use MSBuild content copying instead of relative path navigation.

```csharp
// ✅ CORRECT: Simple relative paths after MSBuild copying
string testFilePath = Path.Combine("Content", "Templates", "locations.json");

// ❌ WRONG: Complex relative path navigation
string testFilePath = Path.Combine("..", "..", "..", "..", "Wayfarer.Tests", "Content", "Templates", "locations.json");
```

**Architecture Benefits**:
- Cross-platform compatible
- Maintainable and standard .NET practice
- Files automatically available in test output directory
- Clean, simple path resolution

### **Systematic Test Debugging Pattern**

**PROVEN APPROACH**: Fix tests incrementally using controlled test data.

1. **Identify** production JSON dependencies in failing tests
2. **Create** minimal test-specific JSON data in `Wayfarer.Tests/Content/Templates/`
3. **Configure** MSBuild to copy files to output directory
4. **Update** TestGameWorldInitializer to load test files
5. **Fix** test entity IDs to match test data
6. **Debug** systematically one assertion at a time

**Result**: Enables progression through test failures line by line with complete control over test data.

## CONTRACTSTEP SYSTEM ARCHITECTURE

### **Unified Contract Completion Architecture**

**FUNDAMENTAL DESIGN**: Replace fragmented contract requirement arrays with a unified, extensible ContractStep system.

**Before (FRAGMENTED)**:
```csharp
// Multiple separate arrays for different requirement types
public List<string> RequiredDestinations { get; set; }
public List<ContractTransaction> RequiredTransactions { get; set; }
public List<string> RequiredNPCConversations { get; set; }
public List<string> RequiredLocationActions { get; set; }
```

**After (UNIFIED)**:
```csharp
// Single unified system for all contract requirements
public List<ContractStep> CompletionSteps { get; set; }
```

### **Polymorphic Step Type System**

**Architecture Pattern**: Abstract base class with concrete implementations for each step type.

```csharp
public abstract class ContractStep
{
    public string Id { get; set; }
    public string Description { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsRequired { get; set; } = true;
    public int OrderHint { get; set; } = 0;
    
    public abstract bool CheckCompletion(Player player, string currentLocationId, object actionContext = null);
    public abstract ContractStepRequirement GetRequirement();
}
```

**Concrete Step Implementations**:
- **TravelStep**: Requires traveling to specific location
- **TransactionStep**: Requires buying/selling items with price constraints
- **ConversationStep**: Requires talking to specific NPCs
- **LocationActionStep**: Requires performing actions at locations
- **EquipmentStep**: Requires obtaining equipment categories

### **JSON Polymorphic Deserialization**

**Type Discriminator Pattern**: JSON uses "type" field to determine concrete step class.

```json
{
  "completionSteps": [
    {
      "type": "TravelStep",
      "id": "travel_to_town",
      "description": "Travel to town square",
      "isRequired": true,
      "orderHint": 1,
      "requiredLocationId": "town_square"
    },
    {
      "type": "TransactionStep",
      "id": "buy_herbs",
      "description": "Purchase herbs",
      "isRequired": true,
      "orderHint": 2,
      "itemId": "herbs",
      "locationId": "town_square",
      "transactionType": "Buy",
      "quantity": 1,
      "maxPrice": 10
    }
  ]
}
```

**Parser Implementation**:
```csharp
private static ContractStep ParseContractStep(JsonElement stepElement)
{
    string stepType = GetStringProperty(stepElement, "type", "");
    
    ContractStep step = stepType switch
    {
        "TravelStep" => new TravelStep { RequiredLocationId = GetStringProperty(stepElement, "requiredLocationId", "") },
        "TransactionStep" => new TransactionStep { ItemId = GetStringProperty(stepElement, "itemId", "") },
        "ConversationStep" => new ConversationStep { RequiredNPCId = GetStringProperty(stepElement, "requiredNPCId", "") },
        "LocationActionStep" => new LocationActionStep { RequiredActionId = GetStringProperty(stepElement, "requiredActionId", "") },
        "EquipmentStep" => new EquipmentStep { RequiredEquipmentCategories = GetEquipmentCategoryArray(stepElement, "requiredEquipmentCategories") },
        _ => null
    };
}
```

### **Progress Calculation Enhancement**

**Unified Progress Tracking**: Progress calculation based on required vs optional steps.

```csharp
public float CalculateProgress()
{
    if (CompletionSteps.Any())
    {
        var requiredSteps = CompletionSteps.Where(step => step.IsRequired).ToList();
        if (!requiredSteps.Any()) return 1.0f;
        
        int completedRequired = requiredSteps.Count(step => step.IsCompleted);
        return (float)completedRequired / requiredSteps.Count;
    }
    
    // Fallback to legacy system for backward compatibility
    // [legacy calculation code...]
}
```

### **Action Context Integration**

**Typed Context Objects**: Provide structured data for step completion validation.

```csharp
// Transaction context for marketplace actions
public class TransactionContext
{
    public string ItemId { get; set; }
    public string LocationId { get; set; }
    public TransactionType TransactionType { get; set; }
    public int Quantity { get; set; }
    public int Price { get; set; }
}

// Usage in progression service
var transactionContext = new TransactionContext
{
    ItemId = itemId,
    LocationId = locationId,
    TransactionType = transactionType,
    Quantity = quantity,
    Price = price
};

progressMade = contract.CheckStepCompletion(player, locationId, transactionContext);
```

### **Dual-System Compatibility**

**Backward Compatibility Strategy**: Support both new ContractStep system and legacy arrays.

```csharp
// NEW: Check ContractStep system first
if (contract.CompletionSteps.Any())
{
    progressMade = contract.CheckStepCompletion(player, destinationLocationId);
}
else
{
    // LEGACY: Fall back to old system for backward compatibility
    if (contract.RequiredDestinations.Contains(destinationLocationId))
    {
        if (!contract.CompletedDestinations.Contains(destinationLocationId))
        {
            contract.CompletedDestinations.Add(destinationLocationId);
            progressMade = true;
        }
    }
}
```

**Legacy Property Deprecation**:
```csharp
[Obsolete("Use CompletionSteps with TravelStep instead")]
public List<string> RequiredDestinations { get; set; } = new List<string>();

[Obsolete("Use CompletionSteps with TransactionStep instead")]
public List<ContractTransaction> RequiredTransactions { get; set; } = new List<ContractTransaction>();
```

### **UI Integration Architecture**

**Step-Based Contract Display**: Enhanced UI showing individual step progress.

```razor
@if (contract.CompletionSteps.Any())
{
    <!-- NEW: ContractStep system display -->
    <div class="requirement-section">
        <h4 class="requirement-title">Contract Steps:</h4>
        @foreach (var step in contract.CompletionSteps.OrderBy(s => s.OrderHint))
        {
            <div class="requirement-item step-item @(step.IsCompleted ? "completed" : "pending") @(step.IsRequired ? "required" : "optional")">
                <div class="step-header">
                    <span class="requirement-icon">@(step.IsCompleted ? "✅" : step.IsRequired ? "🔲" : "🔳")</span>
                    <span class="step-description">@step.Description</span>
                    @if (!step.IsRequired)
                    {
                        <span class="optional-badge">Optional</span>
                    }
                </div>
                <!-- Step-specific details based on type -->
            </div>
        }
    </div>
}
```

### **ContractStep System Benefits**

1. **Unified Architecture**: Single system replaces 4+ separate requirement arrays
2. **Extensible Design**: Easy to add new step types via polymorphic pattern
3. **Rich Progression**: Support for optional steps, order hints, detailed requirements
4. **Type Safety**: Strongly typed action contexts and step validation
5. **Enhanced UI**: Step-by-step progress visualization with meaningful feedback
6. **Backward Compatibility**: Existing contracts continue working unchanged
7. **JSON Flexibility**: Content creators can mix different step types in any order

### **ContractStep Validation Checklist**

Before implementing contract step changes:

1. ✅ **Type Discriminator**: Ensure JSON "type" field matches C# class names exactly
2. ✅ **Required vs Optional**: Properly handle progress calculation for mixed step types
3. ✅ **Order Hints**: Support flexible step ordering without breaking logic
4. ✅ **Action Contexts**: Provide appropriate context objects for step validation
5. ✅ **Legacy Support**: Maintain dual-system compatibility during transition
6. ✅ **UI Integration**: Display step progress with clear visual indicators
7. ✅ **Repository Pattern**: Use repositories for all contract data access

### **ContractStep Failure Patterns to Avoid**

1. **❌ Direct Step Modification**: Never modify step completion status outside Contract methods
2. **❌ Missing Type Discriminators**: Always include "type" field in JSON step definitions
3. **❌ Hardcoded Step Validation**: Use polymorphic CheckCompletion() instead of switch statements
4. **❌ Legacy System Bypass**: Always check CompletionSteps.Any() before falling back to legacy
5. **❌ Context Mismatches**: Ensure action contexts match the step types that need them
6. **❌ Progress Inconsistency**: Keep step completion status synchronized with progress calculation

### **JSON Field Name Requirements (CRITICAL)**

**PARSER ARCHITECTURAL REQUIREMENT: ALL parsers MUST be case-insensitive to prevent fragile JSON-code coupling.**

**CRITICAL PRINCIPLE**: JSON field names should be case-insensitive to avoid parsing failures due to capitalization differences.

**JSON Field Names (Case-Insensitive):**

**NPCs.json Fields:**
- `locationId` or `locationid` or `LocationId` (case-insensitive)
- `spotId` or `spotid` or `SpotId` (case-insensitive)
- `services` or `Services` (case-insensitive)
- `availabilitySchedule` or `AvailabilitySchedule` (case-insensitive)
- `contractCategories` or `ContractCategories` (case-insensitive)

**Locations.json Fields:**
- `locationSpots` or `LocationSpots` (case-insensitive)
- `connectedTo` or `ConnectedTo` (case-insensitive)

**Location_spots.json Fields:**
- `currentTimeBlocks` or `CurrentTimeBlocks` (case-insensitive)
- `spotType` or `SpotType` (case-insensitive)

**PARSER IMPLEMENTATION REQUIREMENT:**
```csharp
// ✅ REQUIRED: Case-insensitive property lookup
public static string GetStringProperty(JsonElement root, string propertyName, string defaultValue)
{
    // Must implement case-insensitive property lookup
    foreach (var property in root.EnumerateObject())
    {
        if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
        {
            return property.Value.GetString() ?? defaultValue;
        }
    }
    return defaultValue;
}

// ❌ FORBIDDEN: Case-sensitive property lookup
root.GetProperty("locationId"); // Breaks if JSON has "LocationId"
```

**ENFORCEMENT REQUIREMENTS:**
1. **All parsers must use case-insensitive property lookup**
2. **JSON creators can use any reasonable capitalization**
3. **Parsers must not fail due to capitalization differences**
4. **Property names should be meaningful, not tied to exact case**

### **TimeManager Synchronization Pattern (CRITICAL)**

**ALWAYS use TimeManager.SetNewTime() to change time - NEVER set WorldState.CurrentTimeBlock directly.**

**WHY**: TimeManager calculates TimeBlocks from CurrentTimeHours. Setting WorldState.CurrentTimeBlock does NOT update the hours.

```csharp
// ❌ WRONG: Setting time block directly
gameWorld.WorldState.CurrentTimeBlock = TimeBlocks.Morning;

// ✅ CORRECT: Setting time through TimeManager
gameWorld.TimeManager.SetNewTime(9); // 9:00 AM = Morning block

// Time block mapping:
// Dawn: 6:00-8:59 (hours 6-8)
// Morning: 9:00-11:59 (hours 9-11)
// Afternoon: 12:00-15:59 (hours 12-15)
// Evening: 16:00-19:59 (hours 16-19)
// Night: 20:00-5:59 (hours 20-23, 0-5)
```

**ENFORCEMENT**: TimeManager.GetCurrentTimeBlock() is the ONLY source of truth for current time block.

## UI ARCHITECTURE PRINCIPLES

### **CONTEXTUAL INFORMATION ARCHITECTURE**

**FUNDAMENTAL RULE**: UI must show relevant information, not comprehensive information.

**CONTEXTUAL DISPLAY REQUIREMENTS:**
1. **Current Context Focus** - Show only information relevant to player's immediate situation
2. **Progressive Disclosure** - Essential info first, detailed info on demand
3. **Decision Support** - Present information that helps immediate decisions
4. **Spatial Efficiency** - Use screen space effectively, avoid verbose displays

**FORBIDDEN UI PATTERNS (Game Design Violations):**
- ❌ **"Strategic Market Analysis" sections** - Violates NO AUTOMATED CONVENIENCES principle
- ❌ **"Equipment Investment Opportunities"** - Tells players what to buy, removing discovery
- ❌ **"Trade Opportunity Indicators"** - Automated system solving optimization puzzles
- ❌ **"Profitable Items" lists** - Removes the challenge of finding profit opportunities
- ❌ **"Best Route" recommendations** - Eliminates route planning gameplay
- ❌ **Verbose NPC schedules** - Information overload that doesn't help decisions

**REQUIRED UI PATTERNS:**
- ✅ **Basic availability indicators** - Simple 🟢/🔴 status without detailed explanations
- ✅ **Item categories for filtering** - Help players find what they're looking for
- ✅ **Current status information** - What's happening right now
- ✅ **Essential action information** - What the player can do immediately
- ✅ **Click-to-expand details** - Full information available when specifically requested

### **INFORMATION DENSITY GUIDELINES**

**NPC Display Standards:**
- **Main Location Screen**: 3-4 lines per NPC maximum
- **Detailed View**: Full information only when specifically requested
- **Visual Hierarchy**: Use icons and colors instead of text where possible

**Market Display Standards:**
- **No Strategic Sections**: Remove all "strategic analysis" components
- **Simple Item Lists**: Basic item info (name, price, category)
- **Trader Status**: Simple availability without verbose explanations

**Time Planning Standards:**
- **Contextual Display**: Show current + next 1-2 time blocks by default
- **Compact Format**: Horizontal timeline instead of large cards
- **On-Demand Expansion**: Full day view available when needed

### **PROGRESSIVE DISCLOSURE IMPLEMENTATION**

**Click-to-Expand Pattern:**
```html
<!-- Simple display by default -->
<div class="npc-card-simple" @onclick="() => ExpandNPC(npc.Id)">
    <span class="npc-name">@npc.Name</span>
    <span class="npc-status">@GetAvailabilityIndicator(npc)</span>
</div>

<!-- Detailed view when expanded -->
@if (expandedNPCId == npc.Id)
{
    <div class="npc-details">
        <!-- Full NPC information -->
    </div>
}
```

**Context-Aware Information:**
- **Market Context**: Show only available traders and their items
- **Travel Context**: Show only accessible routes and their requirements
- **Contract Context**: Show only NPCs who can provide contracts
- **Time Context**: Show only current time block information unless specifically requested

### **UI Component Architecture Patterns**

**Component Structure Pattern:**
```
ComponentName.razor       // UI markup only
ComponentName.razor.cs    // Logic in base class (ComponentNameBase)
```

**Benefits:**
- Clean separation of markup and logic
- Testable business logic in base classes
- Type-safe parameter handling
- Consistent component organization

**Dependency Injection Pattern:**
```csharp
public class ComponentNameBase : ComponentBase
{
    [Inject] public GameWorld GameWorld { get; set; }
    [Inject] public GameWorldManager GameManager { get; set; }
    [Inject] public ItemRepository ItemRepository { get; set; }
    // Only inject what the component needs
}
```

**State Management Pattern:**
```csharp
// ❌ WRONG: Caching in components
private List<Item> _cachedItems;

// ✅ RIGHT: Always query fresh data
public List<Item> GetFilteredItems()
{
    return ItemRepository.GetItemsByCategory(selectedCategory);
}
```

### **UI Design Violations to Avoid**

**Strategic Analysis Violations:**
```csharp
// ❌ FORBIDDEN: Methods that solve puzzles for players
public RouteStrategicAnalysis AnalyzeRouteAccessibility()
public List<EquipmentInvestmentOpportunity> CalculateInvestmentROI()
public List<TradingOpportunity> FindProfitableRoutes()

// ✅ ALLOWED: Simple capability checks
public bool HasClimbingEquipment()
public bool CanAccessRoute(RouteOption route)
public List<Item> GetItemsInCategory(ItemCategory category)
```

**Information Overload Violations:**
```razor
<!-- ❌ WRONG: Showing everything at once -->
<div class="npc-info">
    <div>Name: @npc.Name</div>
    <div>Profession: @npc.Profession</div>
    <div>Schedule: @GetFullScheduleDescription(npc)</div>
    <div>Services: @string.Join(", ", npc.Services)</div>
    <div>Next Available: @GetNextAvailableTime(npc)</div>
    <div>Contract Types: @string.Join(", ", npc.ContractCategories)</div>
    <!-- 10+ more lines of details -->
</div>

<!-- ✅ RIGHT: Progressive disclosure -->
<div class="npc-compact" @onclick="() => ToggleExpanded(npc.Id)">
    <span class="npc-status">@GetStatusIcon(npc)</span>
    <span class="npc-name">@npc.Name</span>
    <span class="expand-indicator">▶</span>
</div>
```

### **CSS Architecture Patterns**

**Semantic Class Naming:**
```css
/* ✅ RIGHT: Purpose-based names */
.travel-context { }
.resource-warning { }
.weight-penalty { }

/* ❌ WRONG: Appearance-based names */
.red-text { }
.big-font { }
.float-left { }
```

**Component-Scoped Organization:**
```css
/* Travel Planning Component */
.travel-status { }
.travel-resources { }
.travel-context { }
.route-option { }
.route-blocked { }

/* Market Trading Component */
.trading-context { }
.trading-resources { }
.market-status { }
.trader-info { }
```

### **Navigation Architecture**

**Enum-Based Screen Management:**
```csharp
public enum CurrentViews
{
    LocationScreen,
    MapScreen,
    MarketScreen,
    TravelScreen,
    ContractScreen,
    RestScreen,
    PlayerStatusScreen,  // New screens follow same pattern
    EncounterScreen,
    NarrativeScreen
}

// Navigation method pattern
public void SwitchToPlayerStatusScreen()
{
    CurrentScreen = CurrentViews.PlayerStatusScreen;
    StateHasChanged();
}
```

**Screen Integration Pattern:**
```razor
@switch (CurrentScreen)
{
    case CurrentViews.PlayerStatusScreen:
        @if (IsGameDataReady())
        {
            <PlayerStatusView OnClose="() => CurrentScreen = CurrentViews.LocationScreen" />
        }
        else
        {
            <div class="loading-message">Loading...</div>
        }
        break;
}
```

### **UI Performance Patterns**

**StateHasChanged Usage:**
```csharp
// ✅ RIGHT: Only after state-changing operations
private void BuyItem(Item item)
{
    GameManager.ExecuteTradeAction(item.Id, "buy", Location.Id);
    StateHasChanged(); // UI needs to reflect purchase
}

// ❌ WRONG: In query methods
public List<Item> GetAvailableItems()
{
    var items = ItemRepository.GetItemsAtLocation(Location.Id);
    StateHasChanged(); // Don't do this!
    return items;
}
```

**Reactive Updates:**
- Let Blazor's change detection handle rendering
- Use @bind for two-way data binding
- Minimize manual StateHasChanged() calls
- Trust the framework's optimization