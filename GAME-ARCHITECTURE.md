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
6. **GameWorld has NO dependencies** - GameWorld is pure data and state, with NO references to services, managers, or external systems
7. **All dependencies flow INWARD** - Services depend on GameWorld, never the other way around

**Dependency Flow:**
```
Services/Managers → GameWorld (✅ CORRECT)
GameWorld → Services/Managers (❌ FORBIDDEN)
```

**GameWorld Creation:**
- GameWorld is created first during startup
- GameWorld does NOT inject or reference any services
- GameWorld does NOT create any managers or services
- Services that need GameWorld inject it as a dependency

**Testing:** See TESTING-STRATEGY.md for automated tests that validate GameWorld has no circular dependencies

**Example:**
```csharp
// ❌ WRONG: GameWorld depending on services
public class GameWorld {
    public ITimeManager TimeManager { get; set; }  // NO!
    public CommandDiscoveryService CommandService { get; set; }  // NO!
}

// ✅ CORRECT: GameWorld as pure data
public class GameWorld {
    public int CurrentDay { get; set; }
    public TimeBlocks CurrentTimeBlock { get; set; }
    // Only data and state, no service references
}

// ✅ CORRECT: Services depend on GameWorld
public class LetterQueueManager {
    private readonly GameWorld _gameWorld;
    private readonly ITimeManager _timeManager;
    // Services can depend on both GameWorld and other services
}
```

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

**Option 2: Extract Third Service**
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

### CLEAN ARCHITECTURE PRINCIPLE (CRITICAL)
**ALWAYS use interfaces and dependency injection for behavioral variations. NEVER use mode flags or conditional logic.**

```csharp
// ❌ WRONG: Mode flags and conditionals
public class ConversationManager
{
    private bool _isDeterministic;
    
    public async Task GenerateNarrative()
    {
        if (_isDeterministic)
            return GenerateDeterministicNarrative();
        else
            return await GenerateAINarrative();
    }
}

// ✅ CORRECT: Interface and dependency injection
public interface INarrativeProvider
{
    Task<string> GenerateIntroduction(ConversationContext context);
    Task<List<ConversationChoice>> GenerateChoices(ConversationContext context);
}

public class ConversationManager
{
    private readonly INarrativeProvider _narrativeProvider;
    
    public ConversationManager(INarrativeProvider narrativeProvider)
    {
        _narrativeProvider = narrativeProvider;
    }
}

// Register in ServiceRegistrations.cs
services.AddScoped<INarrativeProvider, DeterministicNarrativeProvider>(); // For POC
// services.AddScoped<INarrativeProvider, AINarrativeProvider>(); // For full game
```

**Key Principles:**
1. **Interfaces define contracts** - Behavior variations implement same interface
2. **Dependency injection selects implementation** - ServiceRegistrations determines which to use
3. **No conditional logic in classes** - The class doesn't know or care which implementation
4. **Easy testing** - Mock implementations for unit tests
5. **Clean separation** - AI vs deterministic logic completely separated

### NO CLASS INHERITANCE PRINCIPLE (CRITICAL)
**NEVER use class inheritance or extensions. Use composition and helper methods instead.**

```csharp
// ❌ WRONG: Extending existing classes
public class DeterministicConversationManager : ConversationManager
{
    // This violates our architecture principles
}

// ✅ CORRECT: Add helper methods to the existing class
public class ConversationManager
{
    private bool _isDeterministic;
    
    public void SetDeterministicMode(bool isDeterministic)
    {
        _isDeterministic = isDeterministic;
    }
    
    private ConversationChoice GenerateDeterministicChoice(ActionOption action)
    {
        // Helper method for non-AI choices
    }
}
```

**Key Principles:**
1. **Composition over inheritance** - Use member variables and helper methods
2. **Single class responsibility** - One class handles all modes of operation
3. **Mode flags over subclasses** - Use boolean flags to switch behavior
4. **Helper methods for variants** - Private methods handle mode-specific logic

**Why This Matters:**
- **Maintains single source of truth** - One class means one place to look
- **Avoids fragmentation** - Logic stays together instead of scattered across subclasses
- **Simplifies debugging** - All behavior in one file
- **Prevents divergence** - Subclasses can drift from parent behavior
- **Enables easy mode switching** - Can change behavior at runtime

## LETTER QUEUE SYSTEM ARCHITECTURE

### Core Components
- **LetterQueueManager**: 8-slot priority queue with leverage-based positioning and displacement
- **Letter Entity**: Id, SenderId, RecipientId, TokenType, Deadline, Payment, QueuePosition
- **ConnectionTokenManager**: 5 token types (Trust/Trade/Noble/Common/Shadow) with per-NPC tracking, supports debt
- **StandingObligationManager**: Permanent queue behavior modifiers
- **Leverage System**: Token debt creates queue position modifications (see below)

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

### Letter Template Design Principle
**Letter templates define letter types, not specific senders/recipients**

Letter generation follows a clear separation of concerns:

#### Regular Letters (Trust/Trade/Noble/Common/Shadow)
1. **Templates define**: TokenType, payment range, deadline range, size, properties
2. **System picks NPCs**: Based on matching letterTokenTypes array
3. **Example**: Trade letter template → System finds NPCs with letterTokenTypes containing "Trade"
4. **Method used**: `GenerateLetterFromTemplate(template, senderName, recipientName)`

#### Special Narrative Letters (Patron Letters)
1. **Templates define**: Only mechanical properties (tokenType: Noble, high payment, etc.)
2. **PatronLetterService generates**: Narrative names like "Your Patron", "Field Agent"
3. **Not real NPCs**: These are narrative text generated by the service
4. **No template fields needed**: Service has hardcoded arrays of appropriate names

This design maintains categorical purity - templates only define mechanical properties, narrative elements are generated by specialized services.

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

### Leverage-Based Queue Positioning
**Letter queue positions emerge from social status and token debt relationships**

#### Base Positions by Social Status
```csharp
public static class LeverageConstants
{
    public const int PATRON_BASE_POSITION = 1;
    public const int NOBLE_BASE_POSITION = 3;
    public const int TRADE_BASE_POSITION = 5;
    public const int SHADOW_BASE_POSITION = 5;
    public const int COMMON_BASE_POSITION = 7;
    public const int TRUST_BASE_POSITION = 7;
}
```

#### Leverage Calculation
```csharp
private int CalculateLeveragePosition(Letter letter)
{
    // Get base position from social status
    int basePosition = GetBasePositionForTokenType(letter.TokenType);
    
    // Get token balance with sender
    var senderId = GetNPCIdByName(letter.SenderName);
    var tokenBalance = _connectionTokenManager.GetTokensWithNPC(senderId)[letter.TokenType];
    
    // Apply token-based leverage
    if (tokenBalance < 0)
    {
        // Debt creates leverage - each negative token moves position up
        basePosition += tokenBalance; // Subtracts since negative
    }
    else if (tokenBalance >= 4)
    {
        // High positive relationship reduces leverage
        basePosition += 1;
    }
    
    return Math.Max(1, Math.Min(8, basePosition));
}
```

#### Queue Displacement
When a letter's leverage position conflicts with existing letters:
1. Letter enters at its calculated leverage position
2. Existing letters at and below that position shift down
3. Letters pushed past position 8 are automatically discarded
4. No token penalty for forced discards (not player's choice)

This creates emergent gameplay where debt inverts social hierarchies - a common merchant you owe money to can have noble-level priority in your queue.

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

### IMMEDIATE LEGACY CODE ELIMINATION
If you discover ANY legacy code, compilation errors, or deprecated patterns during development:
1. **CREATE HIGH-PRIORITY TODO ITEM** to fix the legacy code
2. **STOP current work** and fix the legacy code immediately
3. **NEVER ignore or postpone** legacy code fixes
4. **NEVER say "these are just dependency fixes"** - fix them now or create immediate todo items

### REFACTORING PRINCIPLES
**When refactoring systems (e.g., Encounter → Conversation):**

1. **NEVER use Compile Remove in .csproj** - This hides compilation errors and mistakes
2. **RENAME files and classes** - Don't delete and recreate, preserve git history
3. **Fix ALL references** - Update every usage throughout the codebase
4. **Complete the refactoring** - Don't leave half-renamed systems
5. **NO HACKS OR LEGACY CODE** - Remove fields/properties entirely if not needed, don't leave empty strings or commented code
6. **Update ALL related classes** - Including DTOs, inputs, parsers, and results
7. **Update Enums** - MessageType.PostEncounterEvolution → PostConversationEvolution
8. **Update Service Registration** - ConversationSystem → ConversationManager (use actual class names)

```csharp
// ❌ WRONG: Hiding errors with .csproj exclusions
<ItemGroup>
    <Compile Remove="Game\EncounterSystem\**" />
</ItemGroup>

// ✅ CORRECT: Rename and fix all references
mv EncounterSystem ConversationSystem
// Then update all class names and references
```

**Refactoring Process:**
1. Move/rename directories
2. Rename files to match new system name
3. Update class names inside files
4. Fix all references in other files
5. Update using statements
6. Fix method signatures
7. Update XML comments and documentation
8. Update related enums (MessageType, etc.)
9. Update DTOs and input/output classes
10. Update parsers and builders
11. Fix service registrations to use correct class names

### NEVER USE REFLECTION (CRITICAL)
Reflection makes code unmaintainable and breaks refactoring:
1. **IMMEDIATELY create highest priority TODO** to remove any reflection
2. **STOP all other work** - reflection usage is a critical violation
3. **Fix it properly** - make fields public, add proper accessors, or redesign the architecture
4. **NO EXCEPTIONS** - There is never a valid reason to use reflection in production code

### NEVER USE STRING-BASED CATEGORY MAPPING
Categories must be properly defined in JSON and parsed into enums/classes:
1. **FORBIDDEN**: Mapping categories based on string matching in item IDs (e.g., `itemId.Contains("hammer")`)
2. **REQUIRED**: Categories must be explicit properties in JSON files
3. **REQUIRED**: Parsers must map JSON category properties to proper enum values
4. **NO EXCEPTIONS** - String-based inference of categories violates the categorical design principle

### NEVER LEAVE DEPRECATED CODE
Remove deprecated fields, properties, and methods immediately:
1. **FORBIDDEN**: Leaving deprecated fields/properties/methods with [Obsolete] attributes
2. **FORBIDDEN**: Keeping old implementations "for backward compatibility"
3. **REQUIRED**: Delete deprecated code immediately when refactoring
4. **REQUIRED**: Update all references to use new implementations
5. **NO EXCEPTIONS** - Deprecated code creates confusion and maintenance debt

### NO FALLBACKS FOR OLD DATA
Fix data files instead of adding compatibility code:
1. **FORBIDDEN**: Adding fallback logic to handle old JSON/data formats
2. **FORBIDDEN**: Writing code like "fallback to old property if new one missing"
3. **REQUIRED**: Update all JSON/data files to use new format immediately
4. **REQUIRED**: Remove old properties from data files completely
5. **NO EXCEPTIONS** - Fallback code is technical debt that will never be cleaned up

### NAMESPACE POLICY
Special exception for Blazor components:
1. **NO NAMESPACES in regular C# files** - Makes code easier to work with, no using statements needed
2. **EXCEPTION: Blazor/Razor components MAY use namespaces** - Required for Blazor's component discovery
3. **Blazor namespace pattern**: Use `Wayfarer.Pages` for pages, `Wayfarer.Pages.Components` for components
4. **Update _Imports.razor** - Include necessary namespace imports for Blazor components only

## ROUTE DISCOVERY PRINCIPLE (CRITICAL)
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

## AI CONVERSATION SYSTEM ARCHITECTURE

### Overview
The conversation system enables AI-driven dynamic interactions with NPCs using a structured approach that integrates with the letter queue and token systems.

### Core Components

#### ConversationManager
- **Purpose**: Orchestrates AI-driven conversations between player and NPCs
- **Key Responsibilities**:
  - Manages conversation state and flow
  - Coordinates with AIGameMaster for narrative generation
  - Tracks conversation progress and choices
  - Handles conversation completion and outcomes

#### AIGameMaster
- **Purpose**: Interfaces with the AI provider to generate dynamic narrative content
- **Key Methods**:
  - `GenerateIntroduction`: Creates opening narrative for conversations
  - `RequestChoices`: Generates contextual player choices
  - `GenerateReaction`: Creates NPC responses to player choices
  - `GenerateConclusion`: Wraps up conversations meaningfully
  - `ProcessPostConversationEvolution`: Handles world changes after important conversations

#### Message Type System
- **Purpose**: Categorizes different types of AI-generated content
- **Key Types**:
  - `Introduction`: Opening narrative
  - `ChoicesGeneration`: Player choice generation
  - `PlayerChoice`: Player's selected action
  - `Reaction`: NPC response
  - `PostConversationEvolution`: World state changes
  - `Conclusion`: Conversation ending

#### ConversationHistoryManager
- **Purpose**: Maintains conversation context for coherent AI responses
- **Key Features**:
  - Filters out system messages from history
  - Optimizes context to stay within token limits
  - Preserves narrative continuity
  - Tracks conversation flow for AI context

### Integration with Game Systems

#### Letter Collection Integration
```csharp
// Conversations enable physical letter collection
if (npc.HasLettersToCollect && conversation.Outcome == Success)
{
    EnableLetterCollection(npc.OfferedLetters);
}
```

#### Token Building Integration
```csharp
// Successful conversations build connection tokens
if (conversation.BuildsTrust)
{
    connectionTokenManager.AddTokens(npc.TokenType, 1, npc.Id);
}
```

### PostConversationEvolution System
This subsystem handles world changes that occur as a result of important conversations:

1. **Trigger Conditions**:
   - Major story conversations
   - First meetings with key NPCs
   - Revealing important information
   - Completing narrative arcs

2. **Possible Changes**:
   - New NPCs become available
   - Locations are revealed
   - Routes become known
   - Letter opportunities arise
   - Story progression occurs

3. **Implementation Flow**:
   ```
   ConversationManager → AIGameMaster → PostConversationEvolutionParser → WorldState Updates
   ```

### Design Principles for AI Conversations

1. **Minimal Complexity in POC**:
   - Focus on letter collection mechanics
   - Simple token-building interactions
   - No complex branching narratives
   - No skill checks or combat

2. **Context Preservation**:
   - Keep conversation history concise
   - Filter mechanical messages from context
   - Preserve narrative flow
   - Optimize for AI token limits

3. **Clear Outcomes**:
   - Every conversation has a clear purpose
   - Results are immediately visible
   - No hidden state changes
   - Player understands consequences

### Refactoring Insights

When refactoring from Encounter to Conversation system:

1. **Complete Renaming Required**:
   - All classes, methods, and properties must be renamed
   - Enums need updating (MessageType.PostEncounterEvolution → PostConversationEvolution)
   - DTOs, inputs, outputs, and parsers all need renaming
   - Prompt files and CSS files need renaming

2. **Service Registration Patterns**:
   - Use actual class names, not assumed names
   - ConversationSystem doesn't exist - use ConversationManager
   - Logger types must match constructor expectations

3. **Clean Removal Approach**:
   - Remove old systems entirely, don't leave placeholders
   - Delete flag system when using token system
   - Remove action system when using location-based actions
   - No compatibility layers - clean break

## NO OPTIONAL PARAMETERS PRINCIPLE (CRITICAL)

### All Method Parameters Must Be Explicit and Required
**NEVER use optional parameters with default values. Every parameter must be explicitly provided by the caller.**

#### ❌ FORBIDDEN Patterns
```csharp
// ❌ WRONG: Optional parameters with defaults
public NPC CreateNPC(string id, string name, string spotId = null)
{
    // This hides important data requirements
}

// ❌ WRONG: Default values for any parameters
public void AddTokens(ConnectionType type, int amount = 1, string npcId = null)
{
    // Caller might forget to provide critical npcId
}

// ❌ WRONG: Method overloading to simulate optional parameters
public Letter CreateLetter(string id, string sender)
{
    return CreateLetter(id, sender, null); // Still using null defaults
}
```

#### ✅ CORRECT Patterns
```csharp
// ✅ CORRECT: All parameters required
public NPC CreateNPC(string id, string name, string spotId)
{
    if (string.IsNullOrEmpty(spotId))
        throw new ArgumentException("SpotId is required", nameof(spotId));
}

// ✅ CORRECT: Explicit parameters force intentional usage
public void AddTokens(ConnectionType type, int amount, string npcId)
{
    // Caller must explicitly provide all values
}

// ✅ CORRECT: Different methods for different use cases
public NPC CreateLocationNPC(string id, string name, string locationId, string spotId)
{
    // For NPCs tied to specific spots
}

public NPC CreateWanderingNPC(string id, string name, string locationId)
{
    // For NPCs without fixed spots - different method, not optional parameter
}
```

#### Why This Matters
1. **Prevents Bugs** - Optional parameters hide missing data that causes runtime errors
2. **Forces Intentional Design** - Callers must think about every value they provide
3. **Improves Debugging** - No hidden nulls or defaults to trace
4. **Better Refactoring** - Adding parameters breaks callers intentionally, ensuring updates
5. **Clear Data Flow** - Every piece of data is explicitly passed and visible

#### Implementation Guidelines
1. **Remove all `= null` and `= defaultValue` from parameters**
2. **Create separate methods for different scenarios** instead of optional parameters
3. **Validate all parameters** at method entry
4. **Use different factory methods** for different creation patterns
5. **Make nullable intent explicit** with proper null checks and exceptions

## NO METHOD OVERLOADING PRINCIPLE (CRITICAL)

### Every Method Must Have a Unique, Descriptive Name
**NEVER use method overloading. Each method must have a distinct name that clearly describes its purpose and parameters.**

#### ❌ FORBIDDEN Patterns
```csharp
// ❌ WRONG: Method overloading hides different behaviors
public Letter CreateLetter(string id, string sender)
{
    // Basic letter creation
}

public Letter CreateLetter(string id, string sender, string recipient)
{
    // Different behavior with recipient
}

public Letter CreateLetter(string id, string sender, string recipient, int deadline)
{
    // Yet another variant
}

// ❌ WRONG: Overloading with different parameter types
public void ProcessLetter(Letter letter) { }
public void ProcessLetter(string letterId) { }
public void ProcessLetter(int queuePosition) { }
```

#### ✅ CORRECT Patterns
```csharp
// ✅ CORRECT: Unique names describe the specific behavior
public Letter CreateBasicLetter(string id, string sender)
{
    // Clear what this does
}

public Letter CreateAddressedLetter(string id, string sender, string recipient)
{
    // Different name for different behavior
}

public Letter CreateTimedLetter(string id, string sender, string recipient, int deadline)
{
    // Explicit about the deadline parameter
}

// ✅ CORRECT: Clear method names for different input types
public void ProcessLetterObject(Letter letter) { }
public void ProcessLetterById(string letterId) { }
public void ProcessLetterAtPosition(int queuePosition) { }

// ✅ CORRECT: Factory methods with descriptive names
public NPC CreateLocationBoundNPC(string id, string name, string locationId, string spotId)
{
    // Clear that this NPC is bound to a location
}

public NPC CreateWanderingNPC(string id, string name, string locationId)
{
    // Clear that this NPC wanders without a fixed spot
}
```

#### Why This Matters
1. **Code Clarity** - Method name tells you exactly what it does
2. **Easier Debugging** - Stack traces show specific method names
3. **Better IntelliSense** - IDE shows all variants clearly
4. **Prevents Mistakes** - Can't accidentally call wrong overload
5. **Self-Documenting** - Method names explain the differences

#### Implementation Rules
1. **Never use the same method name with different parameters**
2. **Include parameter context in the method name**
3. **Use verb phrases that describe the specific action**
4. **Create distinct names even for similar operations**
5. **Prefer longer, descriptive names over short, ambiguous ones**

### Future Expansion (Full Game)

In the full game, conversations will expand to include:
- Relationship building with deep narrative branches
- Information gathering and route discovery
- World evolution through player choices
- Crisis management and negotiation
- The "Denna Problem" - conflicting carrier obligations

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

## COMPOUND ACTION PRINCIPLE (CRITICAL)

### Natural Action Overlap Creates Emergent Efficiency
**Actions should naturally accomplish multiple goals without special bonuses or explicit compound mechanics.**

**Core Principle**: When player actions naturally overlap (like carrying trade goods while delivering letters), the efficiency emerges from the systems working as designed, not from special "compound action" bonuses.

**Implementation Pattern**:
```csharp
// ✅ CORRECT: Natural overlap detection
private string GetTradeCompoundEffect(NPC merchant)
{
    // Check what player is already carrying
    var profitableItems = CheckInventoryForTradeGoods();
    if (profitableItems.Any())
    {
        // Show the natural benefit, don't add special bonuses
        return $"Access market + sell {profitableItems.Count} items for +{totalProfit} profit";
    }
}

// ❌ WRONG: Special compound bonuses
if (hasLetterDelivery && hasTradeGoods)
{
    bonusMultiplier = 1.5; // NO! No special bonuses for combining
}
```

**Examples of Natural Overlap**:
1. **Trade + Delivery**: Carrying trade goods to a letter destination naturally allows profitable trading
2. **Work + Relationship**: Working for an NPC naturally builds connection tokens
3. **Rest + Socializing**: Buying drinks at a tavern restores stamina AND builds relationships
4. **Gathering + Travel**: Collecting resources while traveling between locations

**Why This Matters**:
- Players discover efficiencies through play, not through tooltips
- No complex "combo" systems to explain or balance
- Emergent gameplay feels more satisfying than prescribed combinations
- Maintains simplicity while allowing depth

## LOCATION-BASED ACTION GENERATION

### Domain Tags Drive Environmental Actions
**Location spots use domain tags to generate contextual actions, creating distinct location personalities.**

**Domain Tag Categories**:
- **RESOURCES**: Natural gathering opportunities (berries, herbs, materials)
- **COMMERCE**: Trading and market activities
- **SOCIAL**: Information gathering and relationship building
- **LABOR**: Work opportunities when NPCs aren't available
- **CRAFTING**: Equipment maintenance and creation
- **TRANSPORT**: Alternative travel arrangements
- **RESTRICTED**: Limited access based on relationships or equipment

**Implementation Requirements**:
1. **Actions emerge from tags, not hardcoded spot IDs**
2. **Environmental actions supplement, don't replace NPC interactions**
3. **Contextual availability based on time, NPCs present, and player state**
4. **No optimization hints - players discover uses through exploration**

```csharp
// ✅ CORRECT: Tag-based action generation
if (spot.DomainTags.Contains("RESOURCES"))
{
    actions.Add(new GatherBerriesAction());
}

// ❌ WRONG: Hardcoded spot checks
if (spot.SpotID == "millbrook_forest")
{
    actions.Add(new GatherBerriesAction());
}
```

### Queue-Centric Design
Every feature must answer: "How does this serve the letter queue optimization puzzle?"

---

**This document contains the essential architectural patterns. Follow these principles to maintain system consistency and avoid architectural violations.**