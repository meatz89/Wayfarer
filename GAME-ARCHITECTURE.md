# GAME ARCHITECTURE PRINCIPLES

This document defines the core architectural patterns and principles that must be maintained for system stability and design consistency.

## CORE ARCHITECTURAL PATTERNS

### GameWorld as Single Source of Truth (CRITICAL)
**GameWorld is the ONLY authoritative source for all game entities at runtime.**

```csharp
// ✅ CORRECT: All entities must exist in GameWorld
var location = gameWorld.WorldState.locations.FirstOrDefault(l => l.Id == locationId);

// ❌ WRONG: Creating entities outside of GameWorld
var location = new Location("id", "name"); // This entity doesn't exist in the game!
```

**Key Principles:**
1. **Factories create entities but don't store them** - Factories are stateless entity builders
2. **GameWorld owns all entities** - Every entity must be added to GameWorld to exist in the game
3. **Repositories read from GameWorld** - They provide convenient access but don't store separate copies
4. **Reference validation during loading** - Factories validate references during JSON loading, not at runtime
5. **No parallel entity storage** - Never maintain separate collections of entities outside GameWorld

**Factory Pattern Usage:**
```csharp
// ✅ CORRECT: Factory creates, GameWorld stores
var location = locationFactory.CreateLocation(id, name, ...);
gameWorld.WorldState.locations.Add(location);

// ❌ WRONG: Factory maintains its own entity collection
class LocationFactory {
    private List<Location> _createdLocations; // NO! Factories must be stateless
}
```

### Repository-Mediated Access (CRITICAL)
**ALL game state access MUST go through entity repositories.**

```csharp
// ❌ WRONG: Direct access
gameWorld.WorldState.Items.Add(item);

// ✅ CORRECT: Repository-mediated
itemRepository.AddItem(item);
```

### UI Access Patterns
- **Actions (State Changes)**: UI → GameWorldManager → Specific Manager
- **Queries (Reading State)**: UI → Repository → GameWorld.WorldState

### NO EVENTS OR DELEGATES (CRITICAL)
**Events and delegates are FORBIDDEN - they create hidden dependencies and make code flow impossible to trace.**

#### Why Are Events Forbidden?
1. **Hidden Dependencies** - Can't see who's listening by looking at the code
2. **Memory Leaks** - Forgotten event subscriptions prevent garbage collection
3. **Order Dependencies** - Event handler execution order is not guaranteed
4. **Debugging Nightmare** - Can't step through code flow easily
5. **Testing Complexity** - Must mock event subscriptions
6. **Null Reference Risks** - Events can be null if no subscribers

#### What to Use Instead

**Option 1: Direct Method Calls**
```csharp
// ❌ WRONG: Using events
class LetterQueueManager {
    public event Action<Letter> LetterDelivered;
    
    void DeliverLetter(Letter letter) {
        // delivery logic
        LetterDelivered?.Invoke(letter);
    }
}

// ✅ CORRECT: Direct dependency injection
class LetterQueueManager {
    private readonly ILetterDeliveryHandler _deliveryHandler;
    
    void DeliverLetter(Letter letter) {
        // delivery logic
        _deliveryHandler.HandleDelivery(letter);
    }
}
```

**Option 2: Return Values**
```csharp
// ❌ WRONG: Event to notify of completion
public event Action<ProcessResult> ProcessComplete;

// ✅ CORRECT: Return the result
public ProcessResult Process() {
    // do work
    return result;
}
```

**Option 3: Command Pattern**
```csharp
// ✅ CORRECT: Explicit command handling
interface ICommand {
    void Execute();
}

class DeliverLetterCommand : ICommand {
    public void Execute() { /* delivery logic */ }
}
```

### NO CIRCULAR DEPENDENCIES (CRITICAL)
**Circular dependencies are FORBIDDEN and must be eliminated immediately when found.**

#### What Are Circular Dependencies?
```csharp
// ❌ WRONG: Direct circular dependency
class LetterQueueManager {
    private LetterChainManager _chainManager;
    public LetterQueueManager(LetterChainManager chainManager) { ... }
}

class LetterChainManager {
    private LetterQueueManager _queueManager;
    public LetterChainManager(LetterQueueManager queueManager) { ... }
}

// ❌ WRONG: Transitive circular dependency
// A → B → C → A

// ❌ WRONG: Setter workaround (still a circular dependency!)
class LetterQueueManager {
    private LetterChainManager _chainManager;
    public void SetLetterChainManager(LetterChainManager manager) { 
        _chainManager = manager; // This is still circular!
    }
}
```

#### Why Are They Forbidden?
1. **Breaks Dependency Injection** - DI containers cannot resolve circular dependencies
2. **Violates Single Responsibility** - Classes with circular deps are doing too much
3. **Creates Tight Coupling** - Changes in one class affect the other
4. **Makes Testing Impossible** - Cannot create one without the other
5. **Indicates Poor Design** - The architecture needs redesign

#### How to Fix Circular Dependencies

**Option 1: Extract Common Interface**
```csharp
// ✅ CORRECT: Use interfaces
interface ILetterQueue {
    void AddLetter(Letter letter);
}

class LetterQueueManager : ILetterQueue {
    // No dependency on LetterChainManager
}

class LetterChainManager {
    private ILetterQueue _queue;
    public LetterChainManager(ILetterQueue queue) { ... }
}
```

**Option 2: Use Events/Observer Pattern**
```csharp
// ✅ CORRECT: Event-based communication
class LetterQueueManager {
    public event Action<Letter> LetterAdded;
    
    public void AddLetter(Letter letter) {
        // Add to queue
        LetterAdded?.Invoke(letter);
    }
}

class LetterChainManager {
    public LetterChainManager(LetterQueueManager queueManager) {
        queueManager.LetterAdded += OnLetterAdded;
    }
}
```

**Option 3: Extract Third Service**
```csharp
// ✅ CORRECT: Mediator pattern
class LetterCoordinator {
    private LetterQueueManager _queueManager;
    private LetterChainManager _chainManager;
    
    public LetterCoordinator(LetterQueueManager queue, LetterChainManager chain) {
        _queueManager = queue;
        _chainManager = chain;
    }
    
    public void ProcessLetter(Letter letter) {
        _queueManager.AddToQueue(letter);
        _chainManager.CheckChains(letter);
    }
}
```

**Option 4: Merge Classes (if truly interdependent)**
```csharp
// ✅ CORRECT: Single class if responsibilities are truly inseparable
class LetterManagementService {
    // Handles both queue and chain logic
}
```

### Stateless Repositories
Repositories MUST be stateless and only delegate to GameWorld.

## LETTER QUEUE SYSTEM ARCHITECTURE

### Core Components
- **LetterQueueManager**: 8-slot priority queue with position enforcement and chain letter generation
- **Letter Entity**: Id, SenderId, RecipientId, TokenType, Deadline, Payment, QueuePosition
- **ConnectionTokenManager**: 5 token types (Trust/Trade/Noble/Common/Shadow) with per-NPC tracking
- **StandingObligationManager**: Permanent queue behavior modifiers

### Unified Letter Queue Management
**LetterQueueManager handles the complete letter lifecycle including chain letter generation.**

#### Why Chain Letters are Part of Queue Management
1. **Inseparable Lifecycle** - Chain letters are generated as a direct result of delivery
2. **Immediate Queue Impact** - Generated letters must be added to the queue immediately
3. **Single Transaction** - Delivery, history recording, and chain generation happen atomically
4. **No External Coordination** - The queue manager has all context needed for chain generation
5. **Simplified Architecture** - Eliminates circular dependencies and complex coordination

#### Chain Letter Flow
```csharp
// Unified flow within LetterQueueManager
public void RecordLetterDelivery(Letter letter) {
    // 1. Record delivery in history
    player.NPCLetterHistory[senderId].RecordDelivery();
    
    // 2. Process chain letters immediately
    ProcessChainLetters(letter);
}

private void ProcessChainLetters(Letter deliveredLetter) {
    // Generate any chain letters
    var chainLetters = GenerateChainLetters(deliveredLetter);
    
    // Add them to the queue with narrative feedback
    foreach (var chainLetter in chainLetters) {
        AddLetterWithObligationEffects(chainLetter);
        _messageSystem.AddSystemMessage("📬 Follow-up letter generated!");
    }
}
```

This design follows the principle: if responsibilities are truly inseparable, they belong in the same class.

### Contextual Token Spending Principle
**All token spending must be contextually tied to specific NPCs**

Token spending is NEVER abstract - every token spent damages a specific NPC relationship:

#### Letter Queue Operations
- **Skip Letter**: Tokens from the letter SENDER's relationship (asking them for priority)
- **Extend Deadline**: Tokens from the letter SENDER's relationship (negotiating more time)
- **Priority Move**: Tokens from the letter SENDER's relationship (urgent handling)
- **Purge Letter**: Damages relationship with the SENDER whose letter is destroyed

#### Other Token Spending
- **Route Unlocking**: Tokens from the NPC who controls/knows that route
- **Special Services**: Tokens from the NPC providing the service
- **Access Permissions**: Tokens from the NPC granting access
- **Information Purchase**: Tokens from the NPC selling the information

This creates narrative coherence - you're not spending abstract "Trade tokens", you're specifically burning your relationship with Marcus the Merchant. Every token transaction has a face and a consequence.

### Repository Pattern Implementation

```csharp
// LetterRepository - All letter data access
public class LetterRepository {
    private readonly GameWorld _gameWorld;
    
    public List<Letter> GetPlayerQueue() => _gameWorld.WorldState.Player.LetterQueue;
    public void AddLetterToQueue(Letter letter) { /* Position logic */ }
    public void RemoveLetterFromQueue(string letterId) { /* Shift positions */ }
}

// ConnectionTokenRepository - Token management
public class ConnectionTokenRepository {
    public Dictionary<ConnectionType, int> GetPlayerTokens();
    public Dictionary<string, Dictionary<ConnectionType, int>> GetTokensByNPC();
    public void AddTokens(ConnectionType type, int count, string npcId);
    public bool SpendTokens(ConnectionType type, int count);
}
```

## NARRATIVE COMMUNICATION PRINCIPLE (CRITICAL)

### All Mechanics Must Communicate Through UI and Story
**FUNDAMENTAL RULE**: Every mechanical change in the game must be communicated to the player through visible UI and narrative context. Silent background mechanics violate player trust and agency.

#### The Principle
1. **NO SILENT MECHANICS**: If something changes in the game world, the player must see it happen
2. **NARRATIVE FRAMING**: Mechanical events need story context, not just number changes
3. **PLAYER INVOLVEMENT**: Players should trigger mechanics through deliberate actions, not have them happen automatically
4. **CLEAR COMMUNICATION**: Every cause and effect must be visible and understandable

#### Examples of Violations (❌ FORBIDDEN)
```csharp
// ❌ Silent token addition
player.NPCTokens[npcId][tokenType] += 1;

// ❌ Automatic progression without player awareness
if (player.DeliveredLetters > 10) { UnlockNewArea(); }

// ❌ Hidden state changes
npc.Mood = CalculateMood(player.Actions);

// ❌ Background reputation decay
player.Reputation -= TimeDecay();
```

#### Examples of Correct Implementation (✅ REQUIRED)
```csharp
// ✅ Token addition with narrative context
_messageSystem.AddSystemMessage($"Elena smiles warmly. 'Thank you for the delivery!'", SystemMessageTypes.Success);
_tokenManager.AddTokens(ConnectionType.Trust, 1, "elena_id");
_messageSystem.AddSystemMessage($"Your relationship with Elena has strengthened (+1 Trust)", SystemMessageTypes.Info);

// ✅ Player-triggered progression with clear feedback
if (playerAcceptsIntroduction)
{
    ShowIntroductionDialogue(introducer, newNPC);
    _networkManager.ProcessIntroduction(introducerId, newNPCId);
    _messageSystem.AddSystemMessage($"You can now find {newNPC.Name} at {location}", SystemMessageTypes.Info);
}

// ✅ Visible state changes with UI feedback
ShowNPCReaction("Elena looks disappointed as you skip her letter");
_relationshipManager.RecordSkip(npcId);
UpdateNPCMoodDisplay(npcId);
```

#### Implementation Requirements
1. **Every mechanical change needs**:
   - A MessageSystem notification
   - UI visual feedback
   - Narrative context explaining why it happened
   
2. **Player actions must**:
   - Show what will happen before confirmation
   - Display costs and consequences clearly
   - Provide narrative reasoning for the action

3. **System events must**:
   - Announce themselves when they occur
   - Explain their effects in story terms
   - Show their impact on the game state visually

## CONTENT/LOGIC SEPARATION (CRITICAL)

### Content ≠ Game Logic
**NEVER hardcode content IDs in business logic**

```csharp
// ❌ WRONG: Hardcoded content
if (itemId == "silver_sword") { damage = 10; }

// ✅ CORRECT: Use properties
if (item.Category == ItemCategory.Weapon) { damage = item.Damage; }
```

### Rules
1. **NO HARDCODED CONTENT IDS**: Never use specific names in switch statements
2. **USE PROPERTIES/CATEGORIES**: Business logic operates on entity properties
3. **CONTENT IS DATA**: Content should be configurable, not coded
4. **SEPARATE PROGRESSION FROM CONTENT**: Progression rules (unlocks, requirements) belong in dedicated progression files, not mixed with entity definitions

### Progression Separation Principle (CRITICAL)
**Progression data must be completely separated from content definitions**

**Core Rules:**
- ✅ **Dedicated Progression Files** - All unlock rules, requirements, and progression data in separate JSON files
- ✅ **Progression Managers** - Dedicated managers handle progression logic, not entity managers
- ✅ **Clean Entity Definitions** - NPCs, Locations, Items contain only their intrinsic properties
- ❌ **NO Progression in Entities** - Never add "unlocks", "requirements", or "prerequisites" to entity files
- ❌ **NO Mixed Concerns** - Content files describe what exists; progression files describe how to access it

**File Structure:**
```
Content/Templates/
├── npcs.json              # NPC definitions only (name, profession, location)
├── routes.json            # Route definitions only (origin, destination, time)
├── locations.json         # Location definitions only (name, description)
└── Progression/
    ├── route_discovery.json    # Which NPCs can teach which routes
    ├── network_unlocks.json    # How NPCs introduce other NPCs
    ├── access_requirements.json # What's needed to enter locations
    └── standing_obligations.json # Obligation effects and requirements
```

**Example Separation:**
```json
// ❌ WRONG: routes.json with progression
{
  "id": "mountain_pass",
  "requiredUsageCount": 5,  // NO! Progression logic in content
  "unlockedByNPC": "thomas"  // NO! Progression data in route
}

// ✅ CORRECT: routes.json (content only)
{
  "id": "mountain_pass",
  "origin": "millbrook",
  "destination": "thornhaven",
  "travelTimeHours": 8
}

// ✅ CORRECT: route_discovery.json (progression only)
{
  "routeId": "mountain_pass",
  "knownByNPCs": ["thomas_ranger", "elena_scout"],
  "tokenCostToUnlock": 2,
  "requiredEquipment": ["climbing_gear"]
}
```

This separation ensures content can be freely edited without breaking progression logic, and new progression systems can be added without modifying core content files.

## GAME INITIALIZATION PIPELINE

```
JSON Files → GameWorldSerializer → GameWorldInitializer → GameWorld → Repositories
```

### Key Content Files
- `letter_templates.json` - Letter generation templates
- `npcs.json` - NPCs with professions and relationships
- `actions.json` - Player actions and requirements
- `routes.json` - Travel routes with unlock conditions
- `locations.json` - Game locations with access requirements

## NAVIGATION ARCHITECTURE

### Game Flow
```
App Start → Content Validation → Character Selection → Game Start → Letter Queue (Primary Screen)
```

### Three-Tier Navigation Structure

#### Tier 1: Game Entry (GameUI.razor)
- **MissingReferences** - Only shown if content validation fails
- **CharacterCreation** - For new players or starting new game
- **MainGameplayView** - Core game container (hosts all gameplay screens)

#### Tier 2: Primary Navigation Hub
**Letter Queue Screen** serves as the central hub with three main contexts:

1. **Queue Management** (Primary Context)
   - Letter Queue Display (8-slot priority queue)
   - Queue Actions (swap, purge, priority, extend)
   - Active Obligations Summary

2. **Location Activities** (Secondary Context)
   - Current Location Display
   - Travel Planning
   - Market/Trading
   - Rest/Recovery
   - Letter Board (Dawn only)

3. **Character Management** (Tertiary Context)
   - Character Status
   - Relationships (with Connection Tokens)
   - Standing Obligations (Full View)

#### Tier 3: Contextual Sub-Screens
Each main context has related screens that are contextually accessible.

### Navigation Patterns

#### Primary Navigation Bar (Always Visible)
```
[Queue] [Location] [Character] [System]
```
- **Queue**: Always returns to Letter Queue (home screen)
- **Location**: Goes to current location with activity options
- **Character**: Opens character management hub
- **System**: Save/Load/Settings/Exit

#### Contextual Sub-Navigation
When in a main context, show relevant sub-options:
- **Queue Context**: No sub-nav (it's the home screen)
- **Location Context**: [Map] [Market] [Rest] [Board*]
- **Character Context**: [Status] [Relations] [Obligations]

### Screen Accessibility Matrix

| From Screen | Can Navigate To | Via |
|-------------|----------------|-----|
| Letter Queue | Location, Character, System | Primary Nav |
| Location | Travel, Market, Rest, Board | Context Actions |
| Character | Status, Relations, Obligations | Sub-Nav |
| Any Sub-Screen | Parent Context | Back Button |
| Any Screen | Letter Queue | Primary Nav |

### Navigation Service Architecture

```csharp
public interface INavigationService
{
    CurrentViews CurrentScreen { get; }
    Stack<CurrentViews> NavigationHistory { get; }
    
    void NavigateTo(CurrentViews screen);
    void NavigateBack();
    bool CanNavigateBack();
    
    // Context awareness
    NavigationContext GetCurrentContext();
    List<CurrentViews> GetContextualScreens();
}

public enum NavigationContext
{
    Queue,      // Letter Queue management
    Location,   // Location-based activities
    Character,  // Character management
    System      // System/meta functions
}
```

### Key Navigation Principles

1. **Letter Queue Centricity**: Queue is always one click away from any screen
2. **Context Preservation**: Navigation maintains context (e.g., selected NPC when moving between relationship screens)
3. **Minimal Depth**: No screen is more than 2 clicks from the queue
4. **Visual Hierarchy**: Primary nav > Context nav > Screen actions
5. **Click-based Navigation**: All navigation through buttons and UI elements

## TESTING ARCHITECTURE

### Test Isolation
- **Each test class uses its own JSON content**
- **NEVER use production JSON in tests**
- **Create test-specific data**

### Test Patterns
```csharp
// ✅ CORRECT: Isolated test data
[Test]
public void TestLetterDelivery() {
    var testGameWorld = TestGameWorldFactory.CreateWithTestData();
    var letterQueueManager = new LetterQueueManager(testGameWorld, ...);
    // Test logic
}
```

## MESSAGE SYSTEM USAGE (CRITICAL)

**ALWAYS use MessageSystem for user feedback**

```csharp
// ❌ WRONG: Console output
Console.WriteLine("Letter delivered!");

// ✅ CORRECT: MessageSystem
MessageSystem.AddSystemMessage("Letter delivered!", SystemMessageTypes.Success);
```

### Message Types
- `SystemMessageTypes.Success` - Positive outcomes
- `SystemMessageTypes.Warning` - Cautions
- `SystemMessageTypes.Danger` - Failures
- `SystemMessageTypes.Info` - Neutral information

## CSS ARCHITECTURE PRINCIPLES

### Separate CSS Files
- **All CSS in dedicated .css files** - Never inline in Razor components
- **Use CSS variables** - Always use existing variables (--text-primary, --bg-panel)
- **Check existing styles first** - Always review existing CSS before creating new files
- **Maintain visual hierarchy** - Follow established font sizes and spacing

```css
/* ✅ CORRECT: Use CSS variables */
.letter-queue {
    background: var(--bg-panel);
    color: var(--text-primary);
}

/* ❌ WRONG: Hardcoded colors */
.letter-queue {
    background: #2a2a2a;
    color: #ffffff;
}
```

## CATEGORICAL DESIGN PRINCIPLE

### All Entities Need Categories
**Every entity must have unique types/categories for system interactions**

- **NPCs**: `TokenType`, `Profession`, `Schedule`
- **Letters**: `TokenType`, `Size`, `IsFromPatron`
- **Locations**: `AccessLevel`, `TokenRequirement`
- **Equipment**: `Category`, `EnablesAccess`

### Category-Based Rules
```csharp
// Game rules emerge from categorical interactions
if (npc.Profession == Professions.Merchant) {
    tokenType = ConnectionType.Trade;
}
```

## LEGACY CODE ELIMINATION

### Immediate Removal Principle
1. **DELETE THE ENTIRE FILE** - Don't comment out, remove completely
2. **REMOVE REFERENCES** - Clean up all dependencies
3. **NO PARTIAL PRESERVATION** - Don't try to salvage parts
4. **DOCUMENT REMOVAL** - Add removal principles to architecture docs

### Architectural Bug Discovery
**If you discover architectural bugs, STOP and fix them immediately**

Common violations:
- Duplicate state storage
- Inconsistent data access patterns
- Hardcoded content IDs in business logic
- Direct GameWorld access bypassing repositories

## GAME DESIGN PRINCIPLES

### Games Create Optimization Puzzles
**Players solve puzzles, systems don't solve for players**

```csharp
// ❌ WRONG: Automated system
public List<Item> GetProfitableItems() { /* Solves puzzle for player */ }

// ✅ CORRECT: Player discovers
public List<Item> GetAvailableItems() { /* Player finds opportunities */ }
```

### Emergent Complexity
- **Simple systems** (queue position, deadlines, tokens) interact to create strategic depth
- **Player agency** through meaningful choices with consequences
- **Discovery gameplay** through exploration and experimentation

### Route Discovery Principle (CRITICAL)
**Routes are discovered through relationships and natural play, never through arbitrary counters or requirements.**

**Core Rules:**
- ✅ **NPC Knowledge** - NPCs share routes based on relationship tokens (3+ tokens = trust)
- ✅ **Contextual Discovery** - Routes revealed through letter deliveries and conversations
- ✅ **Equipment Enables** - Proper gear makes NPCs willing to share dangerous routes
- ✅ **Token Investment** - Spend tokens with specific NPCs who know the routes
- ✅ **Obligation Networks** - Standing obligations grant access to specialized routes
- ❌ **NO Usage Counters** - Never "use route X times to unlock Y"
- ❌ **NO Level Gates** - No arbitrary progression requirements
- ❌ **NO Achievement Hunting** - Discovery through relationships, not checklist completion

**Example**: A player with 3+ Common tokens with Thomas the Ranger can spend 2 tokens to learn about a hidden mountain pass. The tokens represent earning trust and local knowledge, not an abstract currency.

**Route Properties**:
- `knownByNPCs`: Which NPCs can teach this route
- `requiredEquipment`: Equipment that suggests this route exists
- `tokenCostToUnlock`: Relationship cost to learn the route
- `requiredStandingObligation`: Specialized network access

This creates emergent route discovery where players naturally learn paths through relationship building, creating strategic choices about which NPCs to befriend based on their geographic knowledge.

## TIME SYSTEM PRINCIPLE (CRITICAL)

### Time Flows in Hours, Not Blocks
**In Wayfarer, time passes in HOURS as the fundamental unit.**

**Core Rules:**
- ✅ **All travel times are in hours** - Routes take X hours to travel (e.g., 8 hours, 12 hours)
- ✅ **All durations are in hours** - Letter deadlines, travel times, etc.
- ✅ **TimeBlocks are ONLY for scheduling** - NPCs available during Morning/Noon/Afternoon/Evening/Night
- ❌ **NEVER use TimeBlockCost** - This is a legacy concept that has been removed
- ❌ **NEVER convert hours to blocks** - Time blocks are not units of time passage

**Implementation:**
```csharp
// ✅ CORRECT: Time in hours
public int TravelTimeHours { get; set; }  // Route takes 8 hours
public int DeadlineHours { get; set; }   // Letter expires in 72 hours

// ❌ WRONG: Time in blocks
public int TimeBlockCost { get; set; }   // NEVER USE THIS

// ✅ CORRECT: TimeBlocks for scheduling only
public TimeBlocks? DepartureTime { get; set; }  // Boat leaves at Morning
public bool IsAvailable(TimeBlocks currentTime) // Shop open during Afternoon
```

**Why This Matters:**
- Hours provide precise time tracking for the letter deadline system
- TimeBlocks create narrative moments (dawn letter selection, evening rest)
- Separation prevents confusion between duration (hours) and schedule (blocks)

## DEPENDENCY INJECTION PATTERNS (CRITICAL)

### Service Registration
**NEVER use `new` to instantiate services or call `BuildServiceProvider()` in configuration.**

#### ❌ FORBIDDEN Patterns
```csharp
// ❌ WRONG: Manual instantiation
var gameWorldInitializer = new GameWorldInitializer(...);

// ❌ WRONG: Intermediate service provider
var serviceProvider = services.BuildServiceProvider();
var factory = serviceProvider.GetRequiredService<Factory>();

// ❌ WRONG: Manual dependency resolution
services.AddSingleton<Service>(sp => {
    var dep1 = sp.GetRequiredService<Dep1>();
    var dep2 = sp.GetRequiredService<Dep2>();
    return new Service(dep1, dep2);
});
```

#### ✅ CORRECT Patterns
```csharp
// ✅ CORRECT: Let DI handle everything
services.AddSingleton<GameWorldInitializer>();
services.AddSingleton<LetterQueueManager>();

// ✅ CORRECT: Factory delegate for complex initialization
services.AddSingleton<GameWorld>(sp => {
    var initializer = sp.GetRequiredService<GameWorldInitializer>();
    return initializer.LoadGame();
});

// ✅ CORRECT: Direct registration when possible
services.AddSingleton<ActionProcessor>();
```

### Manager Dependencies
```csharp
// Managers declare dependencies in constructors
public LetterQueueManager(
    GameWorld gameWorld,
    LetterTemplateRepository letterTemplateRepository,
    NPCRepository npcRepository,
    MessageSystem messageSystem) { }
```

### Why This Matters
1. **Testability** - Proper DI allows easy mocking and testing
2. **Maintainability** - Dependencies are explicit and clear
3. **Lifecycle Management** - Container handles object lifetimes correctly
4. **Circular Dependency Detection** - Container detects issues at startup
5. **Single Responsibility** - Configuration only registers, doesn't create

## TRANSFORMATION ARCHITECTURE

### System Interconnections
**All systems serve the letter queue priority system**

- **Travel System** → Queue delivery order enforcement
- **Inventory System** → Letter carrying capacity constraints
- **NPC System** → Connection token earning and spending
- **Equipment System** → Access to delivery locations
- **Time System** → Deadline pressure creation

### Queue-Centric Design
Every feature must answer: "How does this serve the letter queue optimization puzzle?"

---

**This document contains the essential architectural patterns. Follow these principles to maintain system consistency and avoid architectural violations.**